using Mod.LowLevel;
using System;
using System.Text;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Convention for this file: mutators (<c>Append</c>/<c>Insert</c>/<c>Replace</c>/<c>Trim</c>/
    /// <c>Remove</c>/<c>Clear</c>) always return the receiver — the result <c>r</c> and the
    /// receiver <c>s</c> share ONE slot, so exactly ONE of them is disposed (the final handle;
    /// growth re-binds the slot via <c>RebindToNewBuffer</c> instead of creating a new CStringRef).
    /// <c>Substring()</c> returns a <c>CStringSpan</c> — a slot-free view, nothing to dispose.
    /// Purely temporary arguments (<c>new CStringRef("x")</c>, <c>(CStringRef)spanView</c>) are
    /// intermediates and are NOT disposed (see CStringRefBasicsTests for the slot-lease rationale).
    /// </summary>
    public class CStringRefMutationTests
    {
        // ---------- Append ----------

        [Fact]
        public void Append_Byte_InPlace_WhenCapacityAllows()
        {
            var s = CStr.New("he", 6);
            var r = s.Append((byte)'y');
            Assert.Equal(3, r.Length);
            Assert.Equal("hey", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_Byte_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ab");      // Size == 2
            var r = s.Append((byte)'c');       // growth re-binds the slot to a larger buffer
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal("abc", s.ToString()); // r and s are the same view now
            Assert.True(r.Size > 3);

            r.Dispose();                       // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Append_CStringRef_ComposesInPlace()
        {
            var s = CStr.New("he", 8);
            var r = s.Append(new CStringRef("llo"));
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_Empty_IsNoOp()
        {
            var s = new CStringRef("x");
            var r = s.Append(CStringRef.Empty);
            Assert.Equal(1, r.Length);
            Assert.Equal("x", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Append((byte)'c'));

            s.Dispose();
        }

        [Fact]
        public void OperatorPlus_Concatenates()
        {
            var left = new CStringRef("foo");
            var right = new CStringRef("bar");
            var sum = left + right;          // operator+ forwards to left.Append(right): the mutation
                                             // lands on the operator's by-value copy, so sum is the live
                                             // handle (left keeps a stale Length - use the return value)
            Assert.Equal("foobar", sum.ToString());
            Assert.Equal("bar", right.ToString());

            right.Dispose();
            sum.Dispose();                   // sum and left share one slot: dispose exactly one handle
        }

        // ---------- Append(ReadOnlySpan<byte>) ----------

        [Fact]
        public void Append_ReadOnlySpan_ComposesInPlace()
        {
            var s = CStr.New("he", 8);
            var r = s.Append((ReadOnlySpan<byte>)CStr.Bytes("llo"));
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_ReadOnlySpan_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ab");      // Size == 2
            var r = s.Append((ReadOnlySpan<byte>)CStr.Bytes("cde"));   // growth re-binds the slot
            Assert.Equal("abcde", r.ToString());
            Assert.Equal(5, r.Length);
            Assert.True(r.Size > 5);
            Assert.Equal("abcde", s.ToString()); // r and s are the same view now

            r.Dispose();                       // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Append_ReadOnlySpan_Empty_IsNoOp()
        {
            var s = new CStringRef("x");
            var r = s.Append(default(ReadOnlySpan<byte>));
            Assert.Equal(1, r.Length);
            Assert.Equal("x", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_ReadOnlySpan_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Append((ReadOnlySpan<byte>)CStr.Bytes("c")));

            s.Dispose();
        }

        // ---------- Append(string) ----------

        [Fact]
        public void Append_String_DefaultUtf8_ComposesInPlace()
        {
            var s = CStr.New("he", 8);
            var r = s.Append("llo");
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Append_String_Utf8_EncodesMultibyte()
        {
            var s = new CStringRef("你");
            var r = s.Append("好");
            Assert.Equal("你好", r.ToString());
            Assert.Equal(Encoding.UTF8.GetByteCount("你好"), r.Length);

            r.Dispose();
        }

        [Fact]
        public void Append_String_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ab");      // Size == 2
            var r = s.Append("cde");           // growth re-binds the slot
            Assert.Equal("abcde", r.ToString());
            Assert.Equal(5, r.Length);
            Assert.True(r.Size > 5);

            r.Dispose();                       // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Append_String_WithEncoding_UsesIt()
        {
            var s = CStr.New("h", 8);
            var r = s.Append("i", Encoding.ASCII);
            Assert.Equal("hi", r.ToString());
            Assert.Equal(2, r.Length);

            r.Dispose();
        }

        [Fact]
        public void Append_String_NullEncoding_FallsBackToDefault()
        {
            var s = CStr.New("x", 4);
            var r = s.Append("y", null);
            Assert.Equal("xy", r.ToString());

            r.Dispose();
        }

        [Fact]
        public void Append_String_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Append("c"));

            s.Dispose();
        }

        [Fact]
        public void Append_String_WithinCapacity_DoesNotAllocate()
        {
            var s = CStr.New("he", 16);
            Assert.Equal("he", s.ToString());   // warm the string cache before measuring
            long before = GC.GetAllocatedBytesForCurrentThread();
            var r = s.Append("llo");            // in-capacity: encodes straight into the buffer
            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.Equal(0, after - before);    // no byte[] round-trip, no stackalloc copy
            Assert.Equal("hello", r.ToString());

            r.Dispose();
        }

        // ---------- Trim family ----------

        [Fact]
        public void Trim_DefaultWhitespace_BothEnds()
        {
            var s = CStr.New("  \thello \r\n", 4);
            var r = s.Trim();
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Trim_SpecificBytes()
        {
            var s = CStr.New("..hello..", 4);
            var r = s.Trim(CStr.Bytes("."));
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void TrimStart_SpecificBytes()
        {
            var s = CStr.New("..hello", 4);
            var r = s.TrimStart(CStr.Bytes("."));
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void TrimEnd_SpecificBytes()
        {
            var s = CStr.New("hello..", 4);
            var r = s.TrimEnd(CStr.Bytes("."));
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Trim_WhitespaceOnly_BecomesEmpty()
        {
            var s = CStr.New("   ", 2);
            var r = s.Trim();
            Assert.Equal(0, r.Length);
            Assert.Equal("", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Trim_NothingToTrim_ReturnsUnchanged()
        {
            var s = CStr.New("hello", 2);
            var r = s.Trim();
            Assert.Equal("hello", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Trim_Readonly_ThrowsWhenContentChanges()
        {
            var s = CStr.New("  hi", 4);
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Trim());

            s.Dispose();
        }

        // ---------- Clear / Remove ----------

        [Fact]
        public void Clear_ResetsLength_KeepsCapacity()
        {
            var s = CStr.New("hello", 4);
            int size = s.Size;
            var r = s.Clear();
            Assert.Equal(0, r.Length);
            Assert.Equal(size, r.Size);
            Assert.Equal("", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Clear_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Clear());

            s.Dispose();
        }

        [Fact]
        public void Remove_Middle_ShrinksString()
        {
            var s = new CStringRef("abcdef");
            var r = s.Remove(2, 3);
            Assert.Equal("abf", r.ToString());
            Assert.Equal(new byte[] { 97, 98, 102 }, CStr.Content(r));


            r.Dispose();
        }

        [Fact]
        public void Remove_FromStart_And_ToEnd()
        {
            var fromStart = new CStringRef("abcdef");
            var fromStartResult = fromStart.Remove(0, 3);
            Assert.Equal("def", fromStartResult.ToString());

            var toEnd = new CStringRef("abcdef");
            var toEndResult = toEnd.Remove(3, 3);
            Assert.Equal("abc", toEndResult.ToString());


            fromStartResult.Dispose();
            toEndResult.Dispose();
        }

        [Fact]
        public void Remove_WholeString_BecomesEmpty()
        {
            var s = new CStringRef("abcdef");
            var r = s.Remove(0, 6);
            Assert.Equal(0, r.Length);
            Assert.Equal("", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Remove_InvalidArguments_Throw()
        {
            var s = new CStringRef("abcdef");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Remove(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Remove(6, 1));      // startIndex == Length
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Remove(3, 4));      // overruns the end

            s.Dispose();
        }

        [Fact]
        public void Remove_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Remove(1, 1));

            s.Dispose();
        }

        // ---------- Insert(byte) ----------

        [Fact]
        public void Insert_Byte_InPlace_MiddleAndEnds()
        {
            var s = CStr.New("ac", 6);
            Assert.Equal("abc", s.Insert(1, (byte)'b').ToString());

            var atStart = CStr.New("ac", 6);
            Assert.Equal("aac", atStart.Insert(0, (byte)'a').ToString());

            var atEnd = CStr.New("ab", 6);
            Assert.Equal("abc", atEnd.Insert(2, (byte)'c').ToString());

            s.Dispose();
            atStart.Dispose();
            atEnd.Dispose();
        }

        [Fact]
        public void Insert_Byte_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ab");      // Size == 2, inserting needs 3
            var r = s.Insert(1, (byte)'x');    // growth re-binds the slot to a larger buffer
            Assert.Equal("axb", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal("axb", s.ToString()); // r and s are the same view now
            Assert.True(r.Size > 3);

            r.Dispose();                       // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_Byte_Repeated()
        {
            var s = CStr.New("ab", 8);
            var r = s.Insert(1, (byte)'-', 3);
            Assert.Equal("a---b", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Insert_Byte_RepeatZero_ReturnsUnchanged()
        {
            var s = new CStringRef("ab");
            Assert.Equal("ab", s.Insert(0, (byte)'x', 0).ToString());

            s.Dispose();
        }

        [Fact]
        public void Insert_Byte_InvalidIndex_Throws()
        {
            var s = new CStringRef("ab");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Insert(3, (byte)'x'));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Insert(-1, (byte)'x'));

            s.Dispose();
        }

        [Fact]
        public void Insert_Byte_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Insert(1, (byte)'x'));

            s.Dispose();
        }

        // ---------- Insert(CStringRef) ----------

        [Fact]
        public void Insert_CStringRef_InPlace()
        {
            var s = CStr.New("ac", 6);
            var r = s.Insert(1, new CStringRef("b"));
            Assert.Equal("abc", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Insert_CStringRef_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ac");     // Size == 2
            var r = s.Insert(1, new CStringRef("bb"));   // growth re-binds the slot
            Assert.Equal("abbc", r.ToString());
            Assert.Equal("abbc", s.ToString());          // r and s are the same view now

            r.Dispose();                      // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_CStringRef_FromSameBuffer_InPlace()
        {
            var s = CStr.New("abc", 8);
            var tail = s.Substring(1, 2);           // "bc" — CStringSpan view over s's buffer
            var r = s.Insert(0, (CStringRef)tail);
            Assert.Equal("bcabc", r.ToString());

            r.Dispose();       // r and s share one slot (mutators return this)
        }

        [Fact]
        public void Insert_CStringRef_FromSameBuffer_Overflow()
        {
            var s = new CStringRef("abc");          // Size == 3
            var one = s.Substring(1, 1);            // "b" — CStringSpan view over s's buffer
            var r = s.Insert(0, (CStringRef)one);   // growth re-binds the slot (reads the old buffer first)
            Assert.Equal("babc", r.ToString());
            Assert.Equal("babc", s.ToString());     // r and s are the same view now

            r.Dispose();                           // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_CStringRef_RepeatedFromSameBuffer()
        {
            var s = CStr.New("abc", 8);
            var last = s.Substring(2, 1);           // "c" — CStringSpan view over s's buffer
            var r = s.Insert(1, (CStringRef)last, 3);
            Assert.Equal("acccbc", r.ToString());

            r.Dispose();       // r and s share one slot (mutators return this)
        }

        [Fact]
        public void Insert_CStringRef_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Insert(1, new CStringRef("x")));

            s.Dispose();
        }

        // ---------- Insert(ReadOnlySpan<byte>) ----------

        [Fact]
        public void Insert_ReadOnlySpan_InPlace()
        {
            var s = CStr.New("ac", 6);
            var r = s.Insert(1, (ReadOnlySpan<byte>)CStr.Bytes("b"));
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);


            r.Dispose();
        }

        [Fact]
        public void Insert_ReadOnlySpan_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ac");     // Size == 2
            var r = s.Insert(1, (ReadOnlySpan<byte>)CStr.Bytes("bb"));   // growth re-binds the slot
            Assert.Equal("abbc", r.ToString());
            Assert.Equal("abbc", s.ToString());          // r and s are the same view now
            Assert.True(r.Size > 4);

            r.Dispose();                      // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_ReadOnlySpan_Repeated()
        {
            var s = CStr.New("ab", 8);
            var r = s.Insert(1, (ReadOnlySpan<byte>)CStr.Bytes("-"), 3);
            Assert.Equal("a---b", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Insert_ReadOnlySpan_FromSameBuffer_Overflow()
        {
            var s = new CStringRef("abc");          // Size == 3
            var one = s.ToSpan().Slice(1, 1);        // "b" — ReadOnlySpan view over s's buffer
            var r = s.Insert(0, one);                // growth re-binds the slot (snapshot protects the aliasing view)
            Assert.Equal("babc", r.ToString());
            Assert.Equal("babc", s.ToString());     // r and s are the same view now

            r.Dispose();                           // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_ReadOnlySpan_Empty_IsNoOp()
        {
            var s = new CStringRef("ab");
            var r = s.Insert(0, default(ReadOnlySpan<byte>));
            Assert.Equal("ab", r.ToString());
            Assert.Equal(2, r.Length);

            r.Dispose();
        }

        [Fact]
        public void Insert_ReadOnlySpan_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Insert(1, (ReadOnlySpan<byte>)CStr.Bytes("x")));

            s.Dispose();
        }

        // ---------- Insert(string) ----------

        [Fact]
        public void Insert_String_InPlace()
        {
            var s = CStr.New("ac", 6);
            var r = s.Insert(1, "b");
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);


            r.Dispose();
        }

        [Fact]
        public void Insert_String_Utf8_EncodesMultibyte()
        {
            var s = new CStringRef("你A");     // bytes: E4 BD A0 41
            var r = s.Insert(3, "好");         // insert after 你 (3 bytes in)
            Assert.Equal("你好A", r.ToString());
            Assert.Equal(Encoding.UTF8.GetByteCount("你好A"), r.Length);

            r.Dispose();
        }

        [Fact]
        public void Insert_String_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("ac");     // Size == 2
            var r = s.Insert(1, "bb");        // growth re-binds the slot
            Assert.Equal("abbc", r.ToString());
            Assert.Equal("abbc", s.ToString());   // r and s are the same view now
            Assert.True(r.Size > 4);

            r.Dispose();                      // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Insert_String_WithEncoding_UsesIt()
        {
            var s = CStr.New("ac", 6);
            var r = s.Insert(1, "b", Encoding.ASCII);
            Assert.Equal("abc", r.ToString());

            r.Dispose();
        }

        [Fact]
        public void Insert_String_NullEncoding_FallsBackToDefault()
        {
            var s = CStr.New("ac", 6);
            var r = s.Insert(1, "b", null);
            Assert.Equal("abc", r.ToString());

            r.Dispose();
        }

        [Fact]
        public void Insert_String_Empty_IsNoOp()
        {
            var s = new CStringRef("ab");
            var r = s.Insert(1, "");
            Assert.Equal("ab", r.ToString());
            Assert.Equal(2, r.Length);

            r.Dispose();
        }

        [Fact]
        public void Insert_String_Readonly_Throws()
        {
            var s = new CStringRef("ab");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Insert(1, "x"));

            s.Dispose();
        }

        [Fact]
        public void Insert_String_WithinCapacity_DoesNotAllocate()
        {
            var s = CStr.New("ac", 6);
            Assert.Equal("ac", s.ToString());   // warm the cache before measuring
            long before = GC.GetAllocatedBytesForCurrentThread();
            var r = s.Insert(1, "b");           // in-capacity: encodes straight into the buffer
            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.Equal(0, after - before);   // no byte[] round-trip, no stackalloc copy
            Assert.Equal("abc", r.ToString());

            r.Dispose();
        }

        // ---------- Replace(byte, byte) ----------

        [Fact]
        public void Replace_Byte_SameLength()
        {
            var s = CStr.New("a.b.c", 2);
            var r = s.Replace((byte)'.', (byte)'-');
            Assert.Equal("a-b-c", r.ToString());

            r.Dispose();       // r and s share one slot (byte replace is in-place)
        }

        [Fact]
        public void Replace_Byte_WithZero_DoesNotTruncate()
        {
            var s = new CStringRef("a.b.c");
            var r = s.Replace((byte)'.', 0);
            Assert.Equal(5, r.Length);
            Assert.Equal(new byte[] { 97, 0, 98, 0, 99 }, CStr.Content(r));

            r.Dispose();       // r and s share one slot (byte replace is in-place)
        }

        [Fact]
        public void Replace_Byte_NoMatch_ReturnsUnchanged()
        {
            var s = new CStringRef("abc");
            var r = s.Replace((byte)'z', (byte)'y');
            Assert.Equal("abc", r.ToString());

            r.Dispose();       // r and s share one slot (byte replace is in-place)
        }

        [Fact]
        public void Replace_Byte_Windowed()
        {
            var s = new CStringRef("abcabc");
            var r = s.Replace((byte)'b', (byte)'X', 1, 4);   // window covers indexes 1..4
            Assert.Equal("aXcaXc", r.ToString());

            r.Dispose();       // r and s share one slot (byte replace is in-place)
        }

        [Fact]
        public void Replace_Byte_InvalidWindow_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Replace((byte)'a', (byte)'b', -1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Replace((byte)'a', (byte)'b', 3, 1));

            s.Dispose();
        }

        [Fact]
        public void Replace_Byte_Readonly_ThrowsOnMatch()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Replace((byte)'a', (byte)'x'));

            s.Dispose();
        }

        // ---------- Replace(oldstr, newstr) ----------

        [Fact]
        public void Replace_EqualLength_ManyMatches()
        {
            var s = new CStringRef("ababababab");      // 5 matches -> exercises positions buffer
            var r = s.Replace(new CStringRef("ab"), new CStringRef("XY"));
            Assert.Equal("XYXYXYXYXY", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_Growth_InPlaceWithinCapacity()
        {
            var s = CStr.New("abcXdefXghi", 8);
            var r = s.Replace(new CStringRef("X"), new CStringRef("XY"));
            Assert.Equal("abcXYdefXYghi", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_Growth_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("abcabc");          // Size == 6
            var r = s.Replace(new CStringRef("c"), new CStringRef("ZZ"));   // growth re-binds the slot
            Assert.Equal("abZZabZZ", r.ToString());
            Assert.Equal("abZZabZZ", s.ToString());    // r and s are the same view now

            r.Dispose();                              // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Replace_Deletion_ManyMatches_InPlace()
        {
            var s = new CStringRef("a1b1c1d1");        // 4 deletions
            var r = s.Replace(new CStringRef("1"), CStringRef.Empty);
            Assert.Equal("abcd", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_OverlappingMatches_NonOverlappingReplacement()
        {
            var flat = CStr.New("aaaa", 0);
            var flatResult = flat.Replace(new CStringRef("aa"), new CStringRef("b"));
            Assert.Equal("bb", flatResult.ToString());

            var grow = CStr.New("aaaa", 4).Replace(new CStringRef("aa"), new CStringRef("bbb"));
            Assert.Equal("bbbbbb", grow.ToString());


            flatResult.Dispose();
            grow.Dispose();        // grow is the only handle on the temporary's slot
        }

        [Fact]
        public void Replace_WindowGrowth_PreservesOutOfWindowMatches()
        {
            // '-' at index 1 is outside the window and must stay untouched.
            var s = CStr.New("x-y-zz", 2);
            var r = s.Replace(new CStringRef("-"), new CStringRef("QQ"), 2, 4);
            Assert.Equal("x-yQQzz", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_WindowDeletion_PreservesTail()
        {
            var s = new CStringRef("abXcdXefXg");      // X at 2,5,8; window covers only 2 & 5
            var r = s.Replace(new CStringRef("X"), CStringRef.Empty, 0, 7);
            Assert.Equal("abcdefXg", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_NoMatch_ReturnsUnchanged_EvenReadonly()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            var r = s.Replace(new CStringRef("zz"), new CStringRef("y"));
            Assert.Equal("abc", r.ToString());          // no mutation needed, no throw


            r.Dispose();
        }

        [Fact]
        public void Replace_EmptyOldString_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentException>(() => s.Replace(CStringRef.Empty, new CStringRef("x")));

            s.Dispose();
        }

        [Fact]
        public void Replace_WindowZeroCount_NoChange()
        {
            var s = new CStringRef("abc");
            var r = s.Replace(new CStringRef("b"), new CStringRef("X"), 1, 0);
            Assert.Equal("abc", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_Readonly_ThrowsOnMatch()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Replace(new CStringRef("b"), new CStringRef("X")));

            s.Dispose();
        }

        [Fact]
        public void Replace_OldEqualToNew_NoNetChange()
        {
            var s = new CStringRef("abcabc");
            var r = s.Replace(new CStringRef("bc"), new CStringRef("bc"));
            Assert.Equal("abcabc", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_FromAliasingNewString_IsSafe()
        {
            var s = CStr.New("abXcd", 8);
            var part = s.Substring(0, 2);               // "ab" — CStringSpan view over the same buffer
            var r = s.Replace(new CStringRef("X"), (CStringRef)part);
            Assert.Equal("ababcd", r.ToString());

            r.Dispose();       // r and s share one slot (mutators return this)
        }

        // ---------- Replace(ReadOnlySpan<byte>) ----------

        [Fact]
        public void Replace_ReadOnlySpan_EqualLength_ManyMatches()
        {
            var s = new CStringRef("ababababab");      // 5 matches -> exercises positions buffer
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("ab"), (ReadOnlySpan<byte>)CStr.Bytes("XY"));
            Assert.Equal("XYXYXYXYXY", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_ReadOnlySpan_Growth_Overflow_GrowsInPlace()
        {
            var s = new CStringRef("abcabc");          // Size == 6
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("c"), (ReadOnlySpan<byte>)CStr.Bytes("ZZ"));   // growth re-binds the slot
            Assert.Equal("abZZabZZ", r.ToString());
            Assert.Equal("abZZabZZ", s.ToString());    // r and s are the same view now

            r.Dispose();                              // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Replace_ReadOnlySpan_Deletion_ManyMatches_InPlace()
        {
            var s = new CStringRef("a1b1c1d1");        // 4 deletions
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("1"), default(ReadOnlySpan<byte>));
            Assert.Equal("abcd", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_ReadOnlySpan_WindowGrowth_PreservesOutOfWindowMatches()
        {
            // '-' at index 1 is outside the window and must stay untouched.
            var s = CStr.New("x-y-zz", 2);
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("-"), (ReadOnlySpan<byte>)CStr.Bytes("QQ"), 2, 4);
            Assert.Equal("x-yQQzz", r.ToString());


            r.Dispose();
        }

        [Fact]
        public void Replace_ReadOnlySpan_EmptyOldString_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentException>(() => s.Replace(default(ReadOnlySpan<byte>), (ReadOnlySpan<byte>)CStr.Bytes("x")));

            s.Dispose();
        }

        [Fact]
        public void Replace_ReadOnlySpan_NoMatch_ReturnsUnchanged_EvenReadonly()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("zz"), (ReadOnlySpan<byte>)CStr.Bytes("y"));
            Assert.Equal("abc", r.ToString());          // no mutation needed, no throw


            r.Dispose();
        }

        [Fact]
        public void Replace_ReadOnlySpan_FromAliasingNewString_IsSafe()
        {
            var s = CStr.New("abXcd", 8);
            var part = s.ToSpan().Slice(0, 2);          // "ab" — ReadOnlySpan view over s's buffer
            var r = s.Replace((ReadOnlySpan<byte>)CStr.Bytes("X"), part);
            Assert.Equal("ababcd", r.ToString());

            r.Dispose();       // r and s share one slot (mutators return this)
        }

        [Fact]
        public void Mutators_ShareOneSlot_DisposeExactlyOneHandle()
        {
            var s = new CStringRef("  hello  ");
            var r = s.Trim();               // r and s share one slot (mutators return this)
            Assert.Equal("hello", r.ToString());
            Assert.Equal("hello", s.ToString());

            r.Dispose();                    // disposes the shared slot
        }

        [Fact]
        public void ChainedMutators_ShareOneSlot_DisposeOnce()
        {
            var space = new CStringRef(" ");
            var s = new CStringRef("a b c ");
            var r = s.Replace(space, CStringRef.Empty).TrimEnd();   // the whole chain shares s's slot
            Assert.Equal("abc", r.ToString());

            r.Dispose();                    // the chain's single slot
            space.Dispose();                // space kept its own slot
        }

        // ---------- Resize / Expand ----------

        [Fact]
        public void Resize_Expand_PreservesContent_AndMaintainsNul()
        {
            var s = CStr.New("hello", 2);       // Size 7
            var r = s.Resize(16);
            Assert.Equal("hello", r.ToString());   // content preserved
            Assert.Equal(16, r.Size);
            Assert.Equal(5, r.Length);
            Assert.True(r.IsNullTerminated());      // [5] = 0 maintained after growth
            Assert.Equal("hello", s.ToString());   // r and s are the same view now

            r.Dispose();       // r and s share one slot: dispose exactly one handle
        }

        [Fact]
        public void Resize_Expand_KeepsDataBeyondLength()
        {
            // realloc semantics: the WHOLE old block is preserved, not just the content.
            var s = CStr.New("hello", 2);       // Size 7, Length 5
            s.Ref[6] = (byte)'X';               // raw write beyond Length (no _Length bump)
            var r = s.Resize(16);
            Assert.Equal((byte)'X', r.Ref[6]);     // the byte behind the NUL survived

            r.Dispose();
        }

        [Fact]
        public void Resize_Shrink_TruncatesContent()
        {
            var s = CStr.New("hello", 4);       // Size 9, Length 5
            var r = s.Resize(3);
            Assert.Equal("hel", r.ToString());     // content clamped to the new size
            Assert.Equal(3, r.Length);
            Assert.Equal(3, r.Size);

            r.Dispose();
        }

        [Fact]
        public void Resize_Shrink_WithinLength_KeepsContent()
        {
            var s = CStr.New("hello", 8);       // Size 13, Length 5
            var r = s.Resize(6);
            Assert.Equal("hello", r.ToString());   // Length 5 <= 6: no truncation
            Assert.Equal(5, r.Length);
            Assert.Equal(6, r.Size);
            Assert.True(r.IsNullTerminated());

            r.Dispose();
        }

        [Fact]
        public void Resize_SameSize_NoOp_EvenReadonly()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            var r = s.Resize(3);                // same size: no-op, readonly not consulted
            Assert.Equal("abc", r.ToString());

            s.Dispose();
        }

        [Fact]
        public void Resize_Negative_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Resize(-1));

            s.Dispose();
        }

        [Fact]
        public void Resize_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Resize(8));

            s.Dispose();
        }

        [Fact]
        public void Expand_Grows_PreservesContent()
        {
            var s = CStr.New("hello", 2);
            var r = s.Expand(16);
            Assert.Equal("hello", r.ToString());
            Assert.Equal(16, r.Size);
            Assert.True(r.IsNullTerminated());

            r.Dispose();
        }

        [Fact]
        public void Expand_AtOrBelowSize_NoOp_EvenReadonly()
        {
            var s = new CStringRef("abc");      // Size 3
            s.Freeze();
            var r = s.Expand(3);                // equal: no-op
            Assert.Equal(3, r.Size);
            Assert.Equal("abc", r.ToString());
            var r2 = s.Expand(2);               // smaller: no-op (Expand never shrinks)
            Assert.Equal(3, r2.Size);
            Assert.Equal("abc", r2.ToString());

            s.Dispose();
        }

        [Fact]
        public void Expand_Readonly_Throws_WhenGrowing()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Expand(8));

            s.Dispose();
        }

        // ---------- CStringSpan-parameter overloads (zero-cost views) ----------

        [Fact]
        public void CStringSpan_Overloads_Mutators()
        {
            var s = CStr.New("he", 8);
            var r = s.Append(new CStringSpan("llo"));            // Append(CStringSpan)
            Assert.Equal("hello", r.ToString());

            var i = CStr.New("ac", 8);
            var ri = i.Insert(1, new CStringSpan("b"));          // Insert(CStringSpan)
            Assert.Equal("abc", ri.ToString());
            var ir = CStr.New("ab", 8);
            var rir = ir.Insert(1, new CStringSpan("-"), 3);     // Insert(CStringSpan, repeat)
            Assert.Equal("a---b", rir.ToString());

            var p = new CStringRef("abcabc");
            var rp = p.Replace(new CStringSpan("c"), new CStringSpan("ZZ"));   // Replace(CStringSpan, CStringSpan)
            Assert.Equal("abZZabZZ", rp.ToString());
            var pw = CStr.New("x-y-zz", 2);
            var rpw = pw.Replace(new CStringSpan("-"), new CStringSpan("QQ"), 2, 4);   // window version
            Assert.Equal("x-yQQzz", rpw.ToString());

            r.Dispose(); ri.Dispose(); rir.Dispose(); rp.Dispose(); rpw.Dispose();
        }

        [Fact]
        public void CStringSpan_Overload_CopyFrom()
        {
            var src = new CStringSpan("hello");                  // view over its own buffer
            var dst = CStr.New("xxxxx", 3);
            dst.CopyFrom(src);                                   // CopyFrom(CStringSpan)
            Assert.Equal("hello", dst.ToString());
            Assert.Equal(5, dst.Length);

            var ro = CStr.New("abc", 2);
            ro.Freeze();
            Assert.Throws<InvalidOperationException>(() => ro.CopyFrom(new CStringSpan("z")));
            ro.Dispose();
            dst.Dispose();
        }

        [Fact]
        public void CStringSpan_Overloads_SearchAndCompare()
        {
            var s = new CStringRef("hello world");
            Assert.Equal(6, s.IndexOf(new CStringSpan("world")));            // + startIndex variant
            Assert.Equal(6, s.IndexOf(new CStringSpan("world"), 3));
            Assert.True(s.Contains(new CStringSpan("lo wo")));
            Assert.True(s.StartsWith(new CStringSpan("hello")));
            Assert.True(s.EndsWith(new CStringSpan("world")));
            Assert.Equal(2, s.LastIndexOf(new CStringSpan("ll")));   // the only "ll" in "hello world"

            Assert.True(s.Equals(new CStringSpan("hello world")));           // Equals(CStringSpan)
            Assert.False(s.Equals(new CStringSpan("hello")));
            Assert.Equal(0, s.CompareTo(new CStringSpan("hello world")));    // CompareTo(CStringSpan)
            Assert.True(s.CompareTo(new CStringSpan("hello")) > 0);
            Assert.Equal(0, CStringRef.Compare(new CStringSpan("ab"), new CStringSpan("ab")));   // static Compare

            s.Dispose();
        }

        [Fact]
        public void Ownership_DefaultAndEmptyDisposeIsNoOp()
        {
            CStringRef.Empty.Dispose();     // default: never leased a slot, no throw
            var s = default(CStringRef);
            s.Dispose();                    // no-op
        }
    }
}
