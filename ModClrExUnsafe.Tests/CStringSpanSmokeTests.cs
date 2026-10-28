using Mod.LowLevel;
using System;
using System.Text;
using Xunit;

// CStringSpan 是 ref struct + Span<byte> 载体——GC 安全是构造保证的，
// 不需要 CStringRef 那套"局部变量一律 = default"的约定（本文件刻意不置空以证明这一点）。

namespace ModClrExUnsafe.Tests
{
public class CStringSpanSmokeTests
{
    [Fact]
    public void Construction_And_Basics()
    {
        var s = new CStringSpan("hello");
        Assert.Equal(5, s.Length);
        Assert.Equal(6, s.Size);                    // +1 null 终止
        Assert.Equal("hello", s.ToString());
        Assert.Equal((byte)'h', s[0]);
        Assert.Equal((byte)'o', s[4]);
    }

    [Fact]
    public void Construction_IntSize_EmptyThenAppendString()
    {
        var s = new CStringSpan(16);                // empty view over a zeroed buffer
        Assert.Equal(0, s.Length);
        Assert.Equal(16, s.Size);
        Assert.Equal("", s.ToString());

        s = s.Append("hello");                      // the (string, capacity) replacement pattern
        Assert.Equal("hello", s.ToString());
        Assert.Equal(5, s.Length);
        Assert.Equal(16, s.Size);
    }

    [Fact]
    public void Construction_IntSize_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CStringSpan(-1));
    }

    [Fact]
    public void Indexer_WriteSemantics()
    {
        var s = new CStringSpan("abcdef");
        s[2] = (byte)'X';                           // 写非 0 → 内容替换
        Assert.Equal("abXdef", s.ToString());
        s[3] = 0;                                   // 写 0 → 截断
        Assert.Equal("abX", s.ToString());
        Assert.Equal(3, s.Length);
    }

    [Fact]
    public void Indexer_WriteNonZero_Extends()
    {
        var s = new CStringSpan(new byte[16], 0);
        s[0] = (byte)'a';
        s[1] = (byte)'b';
        Assert.Equal(2, s.Length);
        Assert.Equal("ab", s.ToString());
    }

    [Fact]
    public void RefIndexer_LiveReference()
    {
        var s = new CStringSpan("hello");
        s.Ref[1] = (byte)'E';                       // write through the live ref
        Assert.Equal("hEllo", s.ToString());
        ref byte b = ref s.Ref[4];                  // hold the live ref
        b = (byte)'O';
        // Writes through a held live ref bypass cache invalidation (same as
        // CStringRef) — the contract is manual ClearCache after external writes.
        s.ClearCache();
        Assert.Equal("hEllO", s.ToString());
    }

    [Fact]
    public void Readonly_Throws()
    {
        var s = new CStringSpan(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("hi")));
        Assert.True(s.Readonly);

        bool threw1 = false;
        try { s[0] = (byte)'x'; }
        catch (InvalidOperationException) { threw1 = true; }
        Assert.True(threw1);

        bool threw2 = false;
        try { s = s.Append((byte)'x'); }
        catch (InvalidOperationException) { threw2 = true; }
        Assert.True(threw2);
    }

    [Fact]
    public void Append_InPlace_And_Growth()
    {
        var s = new CStringSpan(new byte[8], 0);
        s = s.Append((byte)'a').Append((byte)'b');
        Assert.Equal("ab", s.ToString());
        Assert.Equal(2, s.Length);

        for (int i = 0; i < 10; i++) s = s.Append((byte)'x');    // 超容 → 新实例
        Assert.Equal("abxxxxxxxxxx", s.ToString());
        Assert.Equal(12, s.Length);
    }

    [Fact]
    public void Append_String_Utf8_And_Encoding()
    {
        var s = new CStringSpan(new byte[16], 0);
        s = s.Append("he").Append("llo");
        Assert.Equal("hello", s.ToString());
        Assert.Equal(5, s.Length);

        var u = new CStringSpan("你");            // UTF8: 3 bytes
        u = u.Append("好");                       // +3 bytes
        Assert.Equal("你好", u.ToString());
        Assert.Equal(Encoding.UTF8.GetByteCount("你好"), u.Length);

        var a = new CStringSpan(new byte[8], 0);
        a = a.Append("hi", Encoding.ASCII);
        Assert.Equal("hi", a.ToString());

        var n = new CStringSpan(new byte[8], 0);
        n = n.Append("x", null);                  // null → Encoding.Default
        Assert.Equal("x", n.ToString());
    }

    [Fact]
    public void Append_String_Growth_ReturnsNewInstance()
    {
        var s = new CStringSpan("ab");            // Size 3
        s = s.Append("cde");                      // 2 + 3 > 3 → new instance
        Assert.Equal("abcde", s.ToString());
        Assert.Equal(5, s.Length);
        Assert.True(s.Size > 5);
    }

    [Fact]
    public void Append_String_WithinCapacity_DoesNotAllocate()
    {
        var s = new CStringSpan(new byte[16], 0);
        s = s.Append("he");
        Assert.Equal("he", s.ToString());       // warm the string cache before measuring
        long before = GC.GetAllocatedBytesForCurrentThread();
        s = s.Append("llo");                    // in-capacity: encodes straight into the buffer
        long after = GC.GetAllocatedBytesForCurrentThread();
        Assert.Equal(0, after - before);        // no byte[] round-trip, no stackalloc copy
        Assert.Equal("hello", s.ToString());
    }

    [Fact]
    public void Resize_Shrink_SlicesInPlace()
    {
        var s = CStr.NewSpan("hello", 8);      // Size 13, Length 5
        s = s.Resize(3);                          // shrink: in-place slice, returns this
        Assert.Equal("hel", s.ToString());
        Assert.Equal(3, s.Length);
        Assert.Equal(3, s.Size);

        var t = CStr.NewSpan("hello", 8);      // shrink within Length: content kept
        t = t.Resize(6);
        Assert.Equal("hello", t.ToString());
        Assert.Equal(5, t.Length);
        Assert.Equal(6, t.Size);
    }

    [Fact]
    public void Resize_Expand_ReturnsNewInstance_OriginalUnchanged()
    {
        var s = new CStringSpan("hello");         // Size 6
        var grown = s.Resize(16);                 // expand: fresh buffer, NEW instance
        Assert.Equal("hello", grown.ToString());
        Assert.Equal(16, grown.Size);
        Assert.Equal("hello", s.ToString());     // the original view is untouched
        Assert.Equal(6, s.Size);
    }

    [Fact]
    public void Expand_Grows_OrNoOp()
    {
        var s = CStr.NewSpan("hi", 4);         // Size 5 (max(4, 2) + 1)
        var same = s.Expand(5);                   // equal: no-op
        Assert.Equal(5, same.Size);
        Assert.Equal("hi", same.ToString());
        var smaller = s.Expand(2);                // smaller: no-op (Expand never shrinks)
        Assert.Equal(5, smaller.Size);

        var grown = s.Expand(20);
        Assert.Equal(20, grown.Size);
        Assert.Equal("hi", grown.ToString());
        Assert.Equal(5, s.Size);                  // original still untouched
    }

    [Fact]
    public void Resize_Expand_NegativeAndReadonly_Throw()
    {
        var s = CStr.NewSpan("abc", 4);
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 4).Resize(-1));
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 4).Expand(-1));

        var ro = new CStringSpan(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("abc")));
        bool threwResize = false;
        try { ro = ro.Resize(8); }                 // ref struct: no lambda capture, try/catch instead
        catch (InvalidOperationException) { threwResize = true; }
        Assert.True(threwResize);
        bool threwExpand = false;
        try { ro = ro.Expand(8); }
        catch (InvalidOperationException) { threwExpand = true; }
        Assert.True(threwExpand);
        var noop = ro.Resize(3);                   // same size: no-op even when readonly
        Assert.Equal("abc", noop.ToString());
    }

    [Fact]
    public void Insert_And_Remove()
    {
        var s = CStr.NewSpan("ac", 6);
        s = s.Insert(1, (byte)'b');
        Assert.Equal("abc", s.ToString());

        var r = new CStringSpan("abcdef");
        r = r.Remove(2, 2);
        Assert.Equal("abef", r.ToString());
    }

    [Fact]
    public void Insert_CStringSpan_InPlace_Overflow_And_Aliasing()
    {
        var s = CStr.NewSpan("ac", 8);
        s = s.Insert(1, new CStringSpan("b"));            // in-capacity
        Assert.Equal("abc", s.ToString());

        var o = new CStringSpan("ac");                    // Size 3
        o = o.Insert(1, new CStringSpan("bb"));          // newLength 4 > 3 → new instance
        Assert.Equal("abbc", o.ToString());
        Assert.True(o.Size > 4);

        var a = CStr.NewSpan("abc", 8);
        var tail = a.Substring(1, 2);                     // "bc" view over a's buffer
        a = a.Insert(0, tail);                            // snapshot protects the aliasing view
        Assert.Equal("bcabc", a.ToString());

        var rep = CStr.NewSpan("abc", 8);
        var last = rep.Substring(2, 1);                   // "c" view
        rep = rep.Insert(1, last, 3);
        Assert.Equal("acccbc", rep.ToString());
    }

    [Fact]
    public void Insert_CStringSpan_Readonly_Throws()
    {
        var s = new CStringSpan(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("ab")));
        bool threw = false;
        try { s = s.Insert(1, new CStringSpan("x")); }
        catch (InvalidOperationException) { threw = true; }
        Assert.True(threw);
    }

    [Fact]
    public void Insert_ReadOnlySpan_InPlace_Overflow_And_Repeat()
    {
        var s = CStr.NewSpan("ac", 8);
        s = s.Insert(1, (ReadOnlySpan<byte>)new byte[] { (byte)'b' });   // in-capacity
        Assert.Equal("abc", s.ToString());

        var o = new CStringSpan("ac");                    // Size 3
        o = o.Insert(1, (ReadOnlySpan<byte>)new byte[] { (byte)'b', (byte)'b' });   // 4 > 3 → new instance
        Assert.Equal("abbc", o.ToString());
        Assert.True(o.Size > 4);

        var rep = CStr.NewSpan("ab", 8);
        rep = rep.Insert(1, (ReadOnlySpan<byte>)new byte[] { (byte)'-' }, 3);
        Assert.Equal("a---b", rep.ToString());

        var empty = CStr.NewSpan("ab", 4);
        empty = empty.Insert(0, default(ReadOnlySpan<byte>));   // empty sub: no-op
        Assert.Equal("ab", empty.ToString());
    }

    [Fact]
    public void Insert_String_Utf8_Encoding_Empty()
    {
        var s = CStr.NewSpan("ac", 8);
        s = s.Insert(1, "b");                             // in-capacity
        Assert.Equal("abc", s.ToString());

        var u = new CStringSpan("你A");
        u = u.Insert(3, "好");                            // UTF8: insert after 3 bytes
        Assert.Equal("你好A", u.ToString());
        Assert.Equal(Encoding.UTF8.GetByteCount("你好A"), u.Length);

        var o = new CStringSpan("ac");                   // Size 3
        o = o.Insert(1, "bb");                           // 4 > 3 → new instance
        Assert.Equal("abbc", o.ToString());
        Assert.True(o.Size > 4);

        var enc = CStr.NewSpan("ac", 8);
        enc = enc.Insert(1, "b", Encoding.ASCII);        // explicit encoding
        Assert.Equal("abc", enc.ToString());

        var e = CStr.NewSpan("ab", 4);
        e = e.Insert(1, "");                              // empty: no-op
        Assert.Equal("ab", e.ToString());
    }

    [Fact]
    public void Trim_Family()
    {
        var s = new CStringSpan("  hello  ");
        s = s.Trim();
        Assert.Equal("hello", s.ToString());

        var t = new CStringSpan("..hello..");
        t = t.Trim(new byte[] { (byte)'.' });
        Assert.Equal("hello", t.ToString());
    }

    [Fact]
    public void Replace_NonOverlapping()
    {
        var s = CStr.NewSpan("aaaa", 0);
        s = s.Replace(new byte[] { (byte)'a', (byte)'a' }, new byte[] { (byte)'b' });
        Assert.Equal("bb", s.ToString());

        var g = CStr.NewSpan("aaaa", 4);
        g = g.Replace(new byte[] { (byte)'a', (byte)'a' }, new byte[] { (byte)'b', (byte)'b', (byte)'b' });
        Assert.Equal("bbbbbb", g.ToString());
    }

    [Fact]
    public void Replace_Byte_SameLength_And_ZeroDoesNotTruncate()
    {
        var s = CStr.NewSpan("a.b.c", 4);
        s = s.Replace((byte)'.', (byte)'-');
        Assert.Equal("a-b-c", s.ToString());

        var z = new CStringSpan("a.b.c");
        z = z.Replace((byte)'.', 0);
        Assert.Equal(5, z.Length);                       // NUL written into content, no truncation
        Assert.Equal(new byte[] { 97, 0, 98, 0, 99 }, z.Slice().ToArray());
    }

    [Fact]
    public void Replace_Byte_Windowed()
    {
        var s = CStr.NewSpan("abcabc", 2);            // window covers indexes 1..4
        s = s.Replace((byte)'b', (byte)'X', 1, 4);
        Assert.Equal("aXcaXc", s.ToString());
    }

    [Fact]
    public void Replace_Byte_NoMatch_Readonly_NoThrow()
    {
        var s = new CStringSpan(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("abc")));
        s = s.Replace((byte)'z', (byte)'y');            // no match: no mutation, readonly not consulted
        Assert.Equal("abc", s.ToString());
    }

    [Fact]
    public void Replace_Byte_Readonly_ThrowsOnMatch()
    {
        var s = new CStringSpan(new ReadOnlySpan<byte>(Encoding.UTF8.GetBytes("abc")));
        bool threw = false;
        try { s = s.Replace((byte)'a', (byte)'x'); }
        catch (InvalidOperationException) { threw = true; }
        Assert.True(threw);
    }

    [Fact]
    public void Replace_Window_Growth_PreservesOutOfWindowMatches()
    {
        // '-' at index 1 is outside the window and must stay untouched.
        var s = CStr.NewSpan("x-y-zz", 2);
        s = s.Replace((ReadOnlySpan<byte>)new byte[] { (byte)'-' },
                      (ReadOnlySpan<byte>)new byte[] { (byte)'Q', (byte)'Q' }, 2, 4);
        Assert.Equal("x-yQQzz", s.ToString());
    }

    [Fact]
    public void Replace_Window_Deletion_PreservesTail()
    {
        var s = new CStringSpan("abXcdXefXg");          // X at 2,5,8; window covers only 2 & 5
        s = s.Replace((ReadOnlySpan<byte>)new byte[] { (byte)'X' }, default(ReadOnlySpan<byte>), 0, 7);
        Assert.Equal("abcdefXg", s.ToString());
    }

    [Fact]
    public void Replace_Window_ZeroCount_NoChange()
    {
        var s = CStr.NewSpan("abc", 2);
        s = s.Replace((ReadOnlySpan<byte>)new byte[] { (byte)'b' }, (ReadOnlySpan<byte>)new byte[] { (byte)'X' }, 1, 0);
        Assert.Equal("abc", s.ToString());
    }

    [Fact]
    public void Replace_CStringSpan_Typed()
    {
        var s = CStr.NewSpan("ababababab", 2);
        s = s.Replace(new CStringSpan("ab"), new CStringSpan("XY"));
        Assert.Equal("XYXYXYXYXY", s.ToString());
    }

    [Fact]
    public void Replace_FromAliasingNewString_IsSafe()
    {
        var s = CStr.NewSpan("abXcd", 8);
        var part = s.Substring(0, 2);                    // "ab" view over the same buffer
        s = s.Replace(new CStringSpan("X"), part);      // snapshot protects the aliasing view
        Assert.Equal("ababcd", s.ToString());
    }

    [Fact]
    public void Substring_IsView()
    {
        var s = new CStringSpan("hello world");
        var sub = s.Substring(6);
        Assert.Equal("world", sub.ToString());
        var sub2 = s.Substring(0, 5);
        Assert.Equal("hello", sub2.ToString());
    }

    [Fact]
    public void Search_Family()
    {
        var s = new CStringSpan("hello world hello");
        Assert.Equal(0, s.IndexOf((byte)'h'));
        Assert.Equal(12, s.LastIndexOf((byte)'h'));
        Assert.Equal(6, s.IndexOf((byte)'w'));
        Assert.True(s.Contains((byte)'w'));
        Assert.True(s.StartsWith(new byte[] { (byte)'h', (byte)'e' }));
        Assert.True(s.EndsWith(new byte[] { (byte)'l', (byte)'o' }));
        Assert.Equal(2, s.IndexOf(new byte[] { (byte)'l', (byte)'l' }));
    }

    [Fact]
    public void Search_Family_CStringSpan_Typed()
    {
        var s = new CStringSpan("hello world hello");
        Assert.Equal(6, s.IndexOf(new CStringSpan("world")));
        Assert.Equal(12, s.IndexOf(new CStringSpan("hello"), 6));   // startIndex overload
        Assert.True(s.Contains(new CStringSpan("lo wo")));
        Assert.Equal(14, s.LastIndexOf(new CStringSpan("ll")));     // last "ll" in the trailing "hello"
        Assert.True(s.StartsWith(new CStringSpan("hello")));
        Assert.True(s.EndsWith(new CStringSpan("hello")));
        Assert.False(s.Contains(new CStringSpan("xyz")));
        Assert.False(s.StartsWith(new CStringSpan("world")));
        Assert.False(s.EndsWith(new CStringSpan("world")));
        Assert.Equal(-1, s.IndexOf(new CStringSpan("world"), 7));   // no match past index 7
        Assert.Equal(-1, s.IndexOf((byte)'z', 5));                  // byte overload: not-found stays -1 (was startIndex-1)
    }

    [Fact]
    public void Split_SingleSeparator()
    {
        var s = new CStringSpan("a,b,c");
        var parts = new System.Collections.Generic.List<string>();
        foreach (var part in s.Split((byte)','))
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "a", "b", "c" }, parts);
    }

    [Fact]
    public void Split_EmptyFieldsPreserved()
    {
        var s = new CStringSpan("a,,b,");
        var parts = new System.Collections.Generic.List<string>();
        foreach (var part in s.Split((byte)','))
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "a", "", "b", "" }, parts);
    }

    [Fact]
    public void Split_ByWhitespace()
    {
        var s = new CStringSpan("  one \t two\r\nthree  ");
        var parts = new System.Collections.Generic.List<string>();
        foreach (var part in s.Split())
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "one", "two", "three" }, parts);
    }

    [Fact]
    public void Split_MultipleSeparators()
    {
        var s = new CStringSpan("a,b;c");
        var parts = new System.Collections.Generic.List<string>();
        foreach (var part in s.Split(new byte[] { (byte)',', (byte)';' }))
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "a", "b", "c" }, parts);
    }

    [Fact]
    public void Enumerator_Foreach()
    {
        var s = new CStringSpan("abc");
        int sum = 0;
        foreach (byte b in s)
        {
            sum += b;
        }
        Assert.Equal((byte)'a' + (byte)'b' + (byte)'c', sum);
    }

    [Fact]
    public void CopyTo_CStringSpan()
    {
        var src = new CStringSpan("hello");
        var dst = new CStringSpan(new byte[16], 0);
        src.CopyTo(ref dst);                        // 注意：必须显式 ref！
        Assert.Equal("hello", dst.ToString());
    }

    [Fact]
    public void Clone_IsIndependent()
    {
        var s = new CStringSpan("hello");
        var c = s.Clone();
        c[0] = (byte)'X';
        Assert.Equal("Xello", c.ToString());
        Assert.Equal("hello", s.ToString());       // 原版不受影响
    }

    [Fact]
    public void Conversions()
    {
        var s = (CStringSpan)"hello";              // explicit from string
        string str = s;                            // implicit to string
        Assert.Equal("hello", str);
        Span<byte> span = s;                       // implicit to Span
        Assert.Equal(6, span.Length);
        CStringSpan fromSpan = new byte[] { 1, 2, 3 }.AsSpan();   // implicit from Span
        Assert.Equal(3, fromSpan.Size);
    }

    [Fact]
    public void CompareTo_And_Equality()
    {
        var a = new CStringSpan("abc");
        var b = new CStringSpan("abd");
        Assert.True(a != b);
        Assert.True(a == new CStringSpan("abc"));
        Assert.Equal(-1, a.CompareTo(b));
    }

    [Fact]
    public void GC_Safety_ByConstruction()
    {
        // 核心卖点：GC 压力下值依然正确——不需要任何置空约定
        var s = new CStringSpan("hello world this is a test");
        byte[][] keep = new byte[64][];
        for (int i = 0; i < 2_000_000; i++) keep[i & 63] = new byte[96];
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.Collect(2, GCCollectionMode.Forced, true, true);
        GC.KeepAlive(keep);
        Assert.Equal("hello world this is a test", s.ToString());
        Assert.Equal((byte)'h', s[0]);
    }

    [Fact]
    public void RecalculateLength()
    {
        var s = new CStringSpan(new byte[16], 0);
        s.ToSpan()[0] = (byte)'a';
        s.ToSpan()[1] = (byte)'b';
        s.ToSpan()[2] = 0;
        s.RecalculateLength();
        Assert.Equal(2, s.Length);
        Assert.Equal("ab", s.ToString());
    }

    [Fact]
    public void IsWhiteSpace()
    {
        Assert.True(new CStringSpan(new byte[] { (byte)' ', (byte)'\t' }).IsWhiteSpace());
        Assert.False(new CStringSpan(" a ").IsWhiteSpace());
    }
}
}
