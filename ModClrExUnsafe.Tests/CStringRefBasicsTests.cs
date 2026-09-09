using System;
using System.Text;
using Xunit;

namespace Mod.Tests
{
    public class CStringRefBasicsTests
    {
        // ---------- construction & conversion ----------

        [Fact]
        public void Constructor_FromAsciiString_ByteLengthAndContent()
        {
            var s = new CStringRef("Hello");
            Assert.Equal(5, s.Length);
            Assert.Equal(5, s.Size);
            Assert.False(s.Readonly);
            Assert.Equal("Hello", s.ToString());
        }

        [Fact]
        public void Constructor_FromUtf8String_LengthIsByteCount()
        {
            var s = new CStringRef("你好A");
            Assert.Equal(Encoding.UTF8.GetByteCount("你好A"), s.Length);
            Assert.Equal("你好A", s.ToString());
        }

        [Fact]
        public void Constructor_FromBuffer_StopsAtFirstNul()
        {
            var buffer = new byte[] { (byte)'a', (byte)'b', 0, (byte)'d', (byte)'e' };
            var s = new CStringRef(new Span<byte>(buffer));
            Assert.Equal(2, s.Length);
            Assert.Equal(5, s.Size);
            Assert.False(s.Readonly);
            Assert.Equal("ab", s.ToString());
        }

        [Fact]
        public void Constructor_FromBufferWithExplicitLength_ClampsToSize()
        {
            var buffer = new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, (byte)'x' };
            var s = new CStringRef(new Span<byte>(buffer), 4);
            Assert.Equal(4, s.Length);
            Assert.Equal(5, s.Size);
            Assert.Equal("abc\0", s.ToString());
        }

        [Fact]
        public void Default_IsEmptyAndZeroSized()
        {
            CStringRef s = default;
            Assert.Equal(0, s.Length);
            Assert.Equal(0, s.Size);
            Assert.True(s.IsEmpty());
            Assert.Equal("", s.ToString());
            Assert.Equal(0, CStringRef.Empty.Length);
        }

        [Fact]
        public void StringConversions_RoundTrip()
        {
            var s = new CStringRef("abc");
            string str = s;                 // implicit CStringRef -> string
            Assert.Equal("abc", str);
            CStringRef back = (CStringRef)"abc";   // explicit string -> CStringRef
            Assert.Equal(s, back);
        }

        [Fact]
        public void SpanConversions_RoundTrip()
        {
            var s = new CStringRef("abc");
            Span<byte> span = s;
            Assert.Equal(new byte[] { 97, 98, 99 }, span.ToArray());
            CStringRef viaSpan = new CStringRef(span);
            Assert.Equal("abc", viaSpan.ToString());
        }

        // ---------- Length / Size / Readonly ----------

        [Fact]
        public void Size_IsCapacity_NotContent()
        {
            var s = CStr.New("abc", 5);
            Assert.Equal(3, s.Length);
            Assert.Equal(8, s.Size);
        }

        [Fact]
        public void Length_Setter_Shrink_ZeroesTailAndClearsCache()
        {
            var s = CStr.New("hello", 3);
            Assert.Equal("hello", s.ToString());   // populate cache
            s.Length = 2;
            Assert.Equal(2, s.Length);
            Assert.Equal("he", s.ToString());
            Assert.Equal(new byte[] { 104, 101 }, CStr.Content(s));
        }

        [Fact]
        public void Length_Setter_ExtendWithinCapacity_DoesNotInventContent()
        {
            var s = CStr.New("hi", 6);   // Size = 8, content "hi"
            s.Length = 5;
            Assert.Equal(5, s.Length);
            Assert.Equal(new byte[] { 104, 105, 0, 0, 0 }, CStr.Content(s));
        }

        [Fact]
        public void Length_Setter_SameValue_IsNoOp()
        {
            var s = new CStringRef("abc");
            s.Length = 3;
            Assert.Equal("abc", s.ToString());
        }

        [Fact]
        public void Length_Setter_OutOfRange_Throws()
        {
            var s = CStr.New("abc", 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Length = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Length = 6);   // > Size (5)
        }

        [Fact]
        public void Length_Setter_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.True(s.Readonly);
            Assert.Throws<InvalidOperationException>(() => s.Length = 2);
        }

        [Fact]
        public void Freeze_Unfreeze_TogglesReadonly()
        {
            var s = CStr.New("abc", 4);
            s.Freeze();
            Assert.True(s.Readonly);
            s.Unfreeze();
            Assert.False(s.Readonly);
            s[0] = (byte)'z';   // no longer throws
            Assert.Equal("zbc", s.ToString());
        }

        [Fact]
        public void RecalculateLength_RecomputesFromTerminatingNul()
        {
            var s = CStr.New("abc", 5);
            s.Length = 6;                   // expose trailing zeroes
            Assert.Equal(6, s.Length);
            s.RecalculateLength();
            Assert.Equal(3, s.Length);
            Assert.Equal("abc", s.ToString());
        }

        // ---------- indexer ----------

        [Fact]
        public void Indexer_Get_InRangeAndBounds()
        {
            var s = new CStringRef("abc");
            Assert.Equal((byte)'a', s[0]);
            Assert.Equal((byte)'c', s[2]);
            Assert.Throws<IndexOutOfRangeException>(() => s[-1]);
            Assert.Throws<IndexOutOfRangeException>(() => s[3]);
            Assert.Throws<IndexOutOfRangeException>(() => s[100]);
        }

        [Fact]
        public void Indexer_Set_NonZeroAtLength_Appends()
        {
            var s = CStr.New("ab", 5);
            Assert.Equal(2, s.Length);
            s[2] = (byte)'c';
            Assert.Equal(3, s.Length);
            s[3] = (byte)'d';
            Assert.Equal(4, s.Length);
            Assert.Equal("abcd", s.ToString());
        }

        [Fact]
        public void Indexer_Set_NulInsideContent_Truncates()
        {
            var s = new CStringRef("hello");
            Assert.Equal("hello", s.ToString());
            s[2] = 0;
            Assert.Equal(2, s.Length);
            Assert.Equal("he", s.ToString());
        }

        [Fact]
        public void Indexer_Set_NulAtLength_KeepsLength()
        {
            var s = CStr.New("ab", 2);
            s[2] = 0;
            Assert.Equal(2, s.Length);
            Assert.Equal("ab", s.ToString());
        }

        [Fact]
        public void Indexer_Set_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s[1] = (byte)'x');
        }

        [Fact]
        public void Indexer_Set_OverwriteNonZero_InvalidatesStringAndHashCache()
        {
            var s = CStr.New("hello", 0);
            Assert.Equal("hello", s.ToString());
            int hashBefore = s.GetHashCode();

            s[1] = (byte)'X';

            Assert.Equal("hXllo", s.ToString());
            Assert.NotEqual(hashBefore, s.GetHashCode());
        }

        // ---------- cache ----------

        [Fact]
        public void ToString_ReturnsCachedInstance()
        {
            var s = new CStringRef("abc");
            object a = s.ToString();
            object b = s.ToString();
            Assert.Same(a, b);
        }

        // ---------- copy / clone ----------

        [Fact]
        public void Clone_IsIndependentCopy()
        {
            var s = new CStringRef("hello");
            var clone = s.Clone();
            Assert.Equal("hello", clone.ToString());
            clone[0] = (byte)'H';
            Assert.Equal("Hello", clone.ToString());
            Assert.Equal("hello", s.ToString());
        }

        [Fact]
        public void CopyTo_OtherCStringRef()
        {
            var dst = CStr.New("xxxxx", 0);
            new CStringRef("hi").CopyTo(dst);
            Assert.Equal("hi", dst.ToString());
        }

        [Fact]
        public void CopyTo_Span_WritesContent()
        {
            var span = new byte[4];
            new CStringRef("hi").CopyTo(span);
            Assert.Equal(new byte[] { 104, 105, 0, 0 }, span);
        }

        // ---------- equality / ordering / hash ----------

        [Fact]
        public void Equals_ByContent_IgnoresBufferCapacity()
        {
            var a = new CStringRef("hello");
            var b = CStr.New("hello", 8);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a == b);
            Assert.False(a != b);
        }

        [Fact]
        public void Equals_DifferentContent_False()
        {
            var a = new CStringRef("hello");
            var b = new CStringRef("hellp");
            Assert.False(a.Equals(b));
            Assert.True(a != b);
        }

        [Fact]
        public void CompareTo_OrdersByBytesThenLength()
        {
            var a = new CStringRef("ab");
            var b = new CStringRef("abc");
            Assert.Equal(0, a.CompareTo(new CStringRef("ab")));
            Assert.True(a.CompareTo(b) < 0);        // shorter prefix sorts first
            Assert.True(b.CompareTo(a) > 0);
            Assert.True(a.CompareTo(new CStringRef("aa")) > 0);
            Assert.Equal(0, a.CompareTo("ab"));
        }

        [Fact]
        public void GetHashCode_EqualForEqualContent()
        {
            var a = new CStringRef("hello");
            var b = CStr.New("hello", 3);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
        }

        // ---------- slicing / substring ----------

        [Fact]
        public void Slice_ReturnsCurrentLogicalContent()
        {
            var s = CStr.New("hello", 3);
            Assert.Equal(CStr.Content(s), s.Slice().ToArray());
        }

        [Fact]
        public void Slice_OffsetAndLength()
        {
            var s = new CStringRef("hello world");
            Assert.Equal(CStr.Bytes("lo wo"), s.Slice(3, 5).ToArray());
        }

        [Fact]
        public void Substring_Middle_ReturnsExactContent()
        {
            var parent = new CStringRef("hello world");
            var sub = parent.Substring(6, 5);
            Assert.Equal("world", sub.ToString());
        }

        [Fact]
        public void Substring_Empty_ValidAtBothEnds()
        {
            var s = new CStringRef("abc");
            Assert.Equal("", s.Substring(0, 0).ToString());
            Assert.Equal("", s.Substring(3, 0).ToString());
        }

        [Fact]
        public void Substring_Tail_IsAViewOverSameBuffer()
        {
            var parent = CStr.New("hello world", 4);
            var tail = parent.Substring(6, parent.Length - 6);
            Assert.Equal("world", tail.ToString());

            parent[6] = (byte)'W';      // mutate the parent through indexer
            Assert.Equal((byte)'W', tail[0]);
        }

        [Fact]
        public void Substring_InvalidRange_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(3, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(1, 3));
        }

        // ---------- enumeration ----------

        [Fact]
        public void Enumerator_YieldsBytesInOrder()
        {
            var s = new CStringRef("abc");
            int sum = 0;
            int count = 0;
            foreach (byte b in s)
            {
                sum += b;
                ++count;
            }
            Assert.Equal(3, count);
            Assert.Equal(97 + 98 + 99, sum);
        }
    }
}
