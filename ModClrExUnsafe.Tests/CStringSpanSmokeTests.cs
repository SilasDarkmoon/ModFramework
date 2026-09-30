using Mod;
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
    public void Insert_And_Remove()
    {
        var s = new CStringSpan("ac", 6);
        s = s.Insert(1, (byte)'b');
        Assert.Equal("abc", s.ToString());

        var r = new CStringSpan("abcdef");
        r = r.Remove(2, 2);
        Assert.Equal("abef", r.ToString());
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
        var s = new CStringSpan("aaaa", 0);
        s = s.Replace(new byte[] { (byte)'a', (byte)'a' }, new byte[] { (byte)'b' });
        Assert.Equal("bb", s.ToString());

        var g = new CStringSpan("aaaa", 4);
        g = g.Replace(new byte[] { (byte)'a', (byte)'a' }, new byte[] { (byte)'b', (byte)'b', (byte)'b' });
        Assert.Equal("bbbbbb", g.ToString());
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
