using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

// Deep tests complementing CStringSpanSmokeTests: boundary shapes, in-place vs
// new-instance paths, the CopyTo ref-trap regression, and large overlapping shifts
// that cross the runtime memmove block sizes.

namespace ModClrExUnsafe.Tests
{
public class CStringSpanDeepTests
{
    private static byte[] B(string s) => Encoding.ASCII.GetBytes(s);

    // =====================================================================
    // indexer boundaries
    // =====================================================================

    [Fact]
    public void Indexer_WriteZeroAtLengthPosition_NoChange()
    {
        var s = new CStringSpan("abc");
        Assert.Equal(3, s.Length);

        s[3] = 0;                                   // writing zero AT the nul position
        Assert.Equal(3, s.Length);                  // does not shrink (index < Length is false)
        Assert.Equal("abc", s.ToString());
    }

    [Fact]
    public void Indexer_WriteNonZeroAtLengthPosition_ExtendsByOne()
    {
        var s = CStr.NewSpan("abc", 8);          // capacity 8
        s[3] = (byte)'d';                           // writing at the nul position extends
        Assert.Equal(4, s.Length);
        Assert.Equal("abcd", s.ToString());
    }

    [Fact]
    public void Indexer_TruncateThenExtend_RestoresContent()
    {
        var s = new CStringSpan("abcde");
        s[2] = 0;                                   // truncate to "ab"
        Assert.Equal(2, s.Length);

        Assert.Equal((byte)'e', s[4]);              // the old bytes are still in the buffer (read does not extend)
        s[2] = (byte)'C';                           // extend again
        s[3] = (byte)'D';
        Assert.Equal("abCD", s.ToString());         // length only reaches the last written position
        s[4] = (byte)'E';
        Assert.Equal("abCDE", s.ToString());
    }

    [Fact]
    public void Indexer_OutOfRange_Throws()
    {
        // ref struct locals cannot be captured by lambdas - construct inside instead.
        // "abc" has Size 4 (content + nul slot): index 3 is the legal nul position.
        Assert.Throws<IndexOutOfRangeException>(() => new CStringSpan("abc")[-1]);
        Assert.Throws<IndexOutOfRangeException>(() => { var t = new CStringSpan("abc"); t[4] = 0; });   // == Size
        Assert.Throws<IndexOutOfRangeException>(() => new CStringSpan("abc").Ref[4]);
    }

    // =====================================================================
    // Append: over-capacity returns a NEW instance; exactly-full skips the terminator
    // =====================================================================

    [Fact]
    public void Append_OverCapacity_ReturnsNewInstance_OriginalUntouched()
    {
        var original = new byte[4];
        var s = new CStringSpan(original, 0);
        s = s.Append((byte)'a').Append((byte)'b').Append((byte)'c');    // len 3 in a 4-byte buffer

        s = s.Append((byte)'d');                    // len 4 == Size: exactly full, still in place
        Assert.Equal(4, s.Size);
        Assert.Equal("abcd", s.ToString());

        s = s.Append((byte)'e');                    // now beyond: new instance with its own buffer

        Assert.Equal("abcde", s.ToString());
        Assert.Equal(6, s.Size);                    // new byte[4 + 2]
        Assert.Equal(new byte[] { (byte)'a', (byte)'b', (byte)'c', (byte)'d' }, original);   // original untouched
    }

    [Fact]
    public void Append_Span_OverCapacity_Grows()
    {
        var s = CStr.NewSpan("ab", 3);           // capacity 3, len 2
        s = s.Append(B("cdef"));                    // needs 6 > 3: new instance

        Assert.Equal("abcdef", s.ToString());
        Assert.Equal(6, s.Length);
    }

    [Fact]
    public void Append_ExactlyFull_NoTerminatorSlot()
    {
        var s = CStr.NewSpan("ab", 3);           // capacity 3, len 2
        s = s.Append((byte)'c');                    // len 3 == capacity: no room for a terminator

        Assert.Equal(3, s.Length);
        Assert.Equal("abc", s.ToString());          // content is still fully readable via _Length
    }

    [Fact]
    public void Append_CStringSpan_Chains()
    {
        var a = new CStringSpan("foo");
        var b = CStr.NewSpan("bar", 8);
        b = b.Append(a);                            // b's content comes first, then a's

        Assert.Equal("barfoo", b.ToString());
        Assert.Equal(6, b.Length);
    }

    // =====================================================================
    // Insert: boundaries, repeat, large overlap, over-capacity
    // =====================================================================

    [Fact]
    public void Insert_AtStart_And_AtEnd()
    {
        var s = CStr.NewSpan("bc", 8);
        s = s.Insert(0, (byte)'a');
        Assert.Equal("abc", s.ToString());

        s = s.Insert(3, (byte)'d');                 // insert at Length appends
        Assert.Equal("abcd", s.ToString());
    }

    [Fact]
    public void Insert_RepeatMultiple()
    {
        var s = CStr.NewSpan("ad", 16);
        s = s.Insert(1, (byte)'x', 3);
        Assert.Equal("axxxd", s.ToString());
    }

    [Fact]
    public void Insert_RepeatZero_IsNoOp()
    {
        var s = new CStringSpan("abc");
        var r = s.Insert(1, (byte)'x', 0);
        Assert.Equal("abc", r.ToString());
        Assert.Equal(3, r.Length);
    }

    [Fact]
    public void Insert_InvalidIndex_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Insert(-1, (byte)'x'));
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Insert(4, (byte)'x'));   // Length + 1
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Insert(0, (byte)'x', -1));
    }

    [Fact]
    public void Insert_LargeOverlap_CrossesMemmoveBlocks()
    {
        // A >2KB in-place right shift exercises the runtime memmove chain (1024/512/...
        // blocks) - the same hazard class the MemCopyRight bugs lived in, here covered
        // by Span.CopyTo's runtime memmove.
        var content = new byte[3000];
        for (int i = 0; i < 2000; i++) content[i] = (byte)((i % 26) + 'a');
        var s = new CStringSpan(content, 2000);

        s = s.Insert(1, (byte)'Z', 64);             // shift 1999 bytes right by 64

        Assert.Equal(2064, s.Length);
        Assert.Equal((byte)'Z', s[1]);
        Assert.Equal((byte)'Z', s[64]);
        Assert.Equal(content[0], s[0]);             // head untouched
        for (int i = 0; i < 1999; i++)
        {
            Assert.Equal((byte)(((i + 1) % 26) + 'a'), s[65 + i]);   // body (original index 1..) shifted intact
        }
    }

    [Fact]
    public void Insert_OverCapacity_ReturnsNewInstance()
    {
        // CStringSpan("ac", 3): buffer = max(3, 2) + 1 = 4 bytes (capacity excludes the nul slot)
        var s = CStr.NewSpan("ac", 3);
        s = s.Insert(1, (byte)'b');                 // len 3 <= Size 4: in place
        Assert.Equal("abc", s.ToString());
        Assert.Equal(4, s.Size);

        s = s.Insert(0, (byte)'Z');                 // len 4 == Size 4: exactly full, still in place
        Assert.Equal("Zabc", s.ToString());
        Assert.Equal(4, s.Size);

        s = s.Insert(0, (byte)'Y');                 // len 5 > Size 4: new instance
        Assert.Equal("YZabc", s.ToString());
        Assert.Equal(6, s.Size);                    // new byte[5 + 1]
    }

    // =====================================================================
    // Remove: boundaries and argument checks
    // =====================================================================

    [Fact]
    public void Remove_Boundaries()
    {
        var s = CStr.NewSpan("abcdef", 16);
        s = s.Remove(0, 2);                         // remove the head
        Assert.Equal("cdef", s.ToString());

        var t = CStr.NewSpan("abcdef", 16);
        t = t.Remove(4, 2);                         // remove the tail
        Assert.Equal("abcd", t.ToString());
    }

    [Fact]
    public void Remove_ZeroLength_IsNoOp()
    {
        var s = new CStringSpan("abc");
        var r = s.Remove(1, 0);
        Assert.Equal("abc", r.ToString());
    }

    [Fact]
    public void Remove_InvalidArguments_Throw()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Remove(3, 1));   // startIndex == Length
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Remove(1, 3)); // length beyond remainder
        Assert.Throws<ArgumentOutOfRangeException>(() => CStr.NewSpan("abc", 8).Remove(1, -1));
    }

    [Fact]
    public void Remove_RewritesTerminator_LengthStaysConsistent()
    {
        var s = CStr.NewSpan("abcdef", 16);
        s = s.Remove(2, 2);
        Assert.Equal(4, s.Length);

        s.RecalculateLength();                      // the nul was rewritten: rescan agrees
        Assert.Equal(4, s.Length);
    }

    // =====================================================================
    // Replace: shrink / equal / growth paths
    // =====================================================================

    [Fact]
    public void Replace_FindEmpty_IsNoOp()
    {
        var s = CStr.NewSpan("abc", 8);
        var r = s.Replace(default, B("x"));         // empty find
        Assert.Equal("abc", r.ToString());
        Assert.Equal(3, r.Length);
    }

    [Fact]
    public void Replace_NoMatch_IsNoOp()
    {
        var s = CStr.NewSpan("abc", 8);
        var r = s.Replace(B("zz"), B("y"));
        Assert.Equal("abc", r.ToString());
        Assert.Equal(3, r.Length);                   // content and length untouched
    }

    [Fact]
    public void Replace_EqualLength_InPlace()
    {
        var s = CStr.NewSpan("hello world", 16);
        s = s.Replace(B("world"), B("WORLD"));
        Assert.Equal("hello WORLD", s.ToString());
        Assert.Equal(11, s.Length);
    }

    [Fact]
    public void Replace_Shrink_InPlace_RewritesTerminator()
    {
        var s = CStr.NewSpan("a  b", 16);        // two spaces shrink to one
        s = s.Replace(B("  "), B(" "));
        Assert.Equal("a b", s.ToString());
        Assert.Equal(3, s.Length);

        s.RecalculateLength();
        Assert.Equal(3, s.Length);                  // nul rewritten at the new end
    }

    [Fact]
    public void Replace_GrowthOverCapacity_NewInstance()
    {
        var s = CStr.NewSpan("aa", 3);          // capacity 3
        s = s.Replace(B("a"), B("bb"));            // needs 4 > 3: new buffer

        Assert.Equal("bbbb", s.ToString());
        Assert.True(s.Size >= 5);
    }

    [Fact]
    public void Replace_SelfReplacement_Idempotent()
    {
        var s = CStr.NewSpan("abcabc", 16);
        s = s.Replace(B("ab"), B("ab"));
        Assert.Equal("abcabc", s.ToString());
        Assert.Equal(6, s.Length);
    }

    [Fact]
    public void Replace_MatchAtVeryEnd()
    {
        var s = CStr.NewSpan("abcxx", 16);
        s = s.Replace(B("xx"), B("y"));
        Assert.Equal("abcy", s.ToString());
    }

    // =====================================================================
    // Split boundaries
    // =====================================================================

    [Fact]
    public void Split_AllSeparators_OnlyEmptyFields()
    {
        var s = new CStringSpan(",,,");
        var parts = new List<string>();
        foreach (var part in s.Split((byte)','))
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "", "", "", "" }, parts);
    }

    [Fact]
    public void Split_LeadingSeparator_EmptyFirstField()
    {
        var s = new CStringSpan(",a");
        var parts = new List<string>();
        foreach (var part in s.Split((byte)','))
        {
            parts.Add(part.ToString());
        }
        Assert.Equal(new[] { "", "a" }, parts);
    }

    // =====================================================================
    // CopyTo / CopyFrom: the ref-trap regression
    // =====================================================================

    [Fact]
    public void CopyTo_RefCStringSpan_UpdatesTargetLength()
    {
        var src = CStr.NewSpan("hello world", 16);
        var dst = new CStringSpan(new byte[16], 0);

        src.CopyTo(ref dst);                        // the ref overload copies AND updates _Length

        Assert.Equal("hello world", dst.ToString());
        Assert.Equal(11, dst.Length);
    }

    [Fact]
    public void CopyTo_SpanOverload_CopiesBytesButNotLength()
    {
        // KNOWN TRAP (documented on CStringRef, same shape here): dropping the `ref`
        // silently binds the Span<byte> overload - bytes arrive, _Length does not.
        // This test locks the difference so a future overload change cannot hide it.
        var src = CStr.NewSpan("hello world", 16);
        var dst = new CStringSpan(new byte[16], 0);

        Span<byte> dstSpan = dst;                    // implicit conversion
        src.CopyTo(dstSpan);                        // no ref: byte-copy overload

        Assert.Equal(0, dst.Length);                 // length NOT updated - the trap
        Assert.Equal((byte)'h', dst[0]);             // but the bytes are there
        dst.RecalculateLength();
        Assert.Equal(11, dst.Length);                // manual rescan recovers
    }

    [Fact]
    public void CopyFrom_SourceToTarget()
    {
        var src = CStr.NewSpan("data", 8);
        var dst = new CStringSpan(new byte[8], 0);

        dst.CopyFrom(src);

        Assert.Equal("data", dst.ToString());
        Assert.Equal(4, dst.Length);
    }

    // =====================================================================
    // misc: Clear, Freeze/Unfreeze, operator+
    // =====================================================================

    [Fact]
    public void Clear_ResetsLength()
    {
        var s = CStr.NewSpan("hello", 8);
        s = s.Clear();
        Assert.Equal(0, s.Length);
        Assert.Equal(string.Empty, s.ToString());
    }

    [Fact]
    public void Freeze_Unfreeze_RoundTrip()
    {
        var s = CStr.NewSpan("abc", 8);
        s.Freeze();
        Assert.True(s.Readonly);
        Assert.Throws<InvalidOperationException>(() =>
        {
            var t = CStr.NewSpan("abc", 8);        // ref struct: construct inside the lambda
            t.Freeze();
            t[0] = (byte)'x';
        });

        s.Unfreeze();
        s[0] = (byte)'X';
        Assert.Equal("Xbc", s.ToString());
    }

    [Fact]
    public void OperatorPlus_Concatenates()
    {
        var a = CStr.NewSpan("foo", 8);
        var b = CStr.NewSpan("bar", 8);
        var c = a + b;
        Assert.Equal("foobar", c.ToString());
    }
}
}
