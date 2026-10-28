using Mod;
using System;
using System.Text;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Convention for this file: every local that holds a constructed CStringRef
    /// (<c>new CStringRef("...")</c>, <c>new CStringRef(span)</c>, <c>CStr.New(...)</c> and
    /// <c>Clone()</c>) is disposed with <c>Dispose()</c> before the test returns.
    /// <c>Substring()</c> returns a <c>CStringSpan</c> — a slot-free view, nothing to dispose.
    ///
    /// Why: <c>CStringRef._Address</c> is a <c>RawTrackingRef&lt;byte&gt;</c> — a slot in the
    /// dedicated tracking thread's stack that the GC follows as an interior byref. The slot is
    /// leased by <c>RawTrackingRef.Create()</c> and only returned by <c>Dispose()</c>; a view that
    /// is never disposed keeps its slot (and therefore keeps the target buffer alive) for the
    /// rest of the process. Disposing is what makes the lease short-lived. Please keep the
    /// <c>Dispose()</c> calls.
    /// </summary>
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

            s.Dispose();
        }

        [Fact]
        public void Constructor_FromUtf8String_LengthIsByteCount()
        {
            var s = new CStringRef("你好A");
            Assert.Equal(Encoding.UTF8.GetByteCount("你好A"), s.Length);
            Assert.Equal("你好A", s.ToString());

            s.Dispose();
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

            s.Dispose();
        }

        [Fact]
        public void Constructor_FromBufferWithExplicitLength_ClampsToSize()
        {
            var buffer = new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, (byte)'x' };
            var s = new CStringRef(new Span<byte>(buffer), 4);
            Assert.Equal(4, s.Length);
            Assert.Equal(5, s.Size);
            Assert.Equal("abc\0", s.ToString());

            s.Dispose();
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
        public void Constructor_IntSize_Managed_IsEmptyViewWithCapacity()
        {
            var s = new CStringRef(16);
            Assert.Equal(0, s.Length);
            Assert.Equal(16, s.Size);
            Assert.True(s.IsEmpty());
            Assert.Equal("", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Constructor_IntSize_ThenAppendString_ReplacesCapacityCtor()
        {
            var s = new CStringRef(16).Append("hello");   // the (string, capacity) replacement pattern
            Assert.Equal("hello", s.ToString());
            Assert.Equal(5, s.Length);
            Assert.Equal(16, s.Size);
            Assert.True(s.IsNullTerminated());

            s.Dispose();
        }

        [Fact]
        public void Constructor_IntSize_SelfManaged_AllocatesNativeBlock()
        {
            var s = new CStringRef(16, true);
            Assert.Equal(16, s.Size);
            Assert.Equal(0, s.Length);
            Assert.False(s.IsNull());
            Assert.True(s.IsFixed);                  // Raw mode
            s[0] = (byte)'h';                        // indexer extends Length
            s[1] = (byte)'i';
            Assert.Equal("hi", s.ToString());

            s.Dispose();                             // frees the native block
        }

        [Fact]
        public void Constructor_IntSize_Zero_ManagedAndSelfManaged()
        {
            var m = new CStringRef(0);
            Assert.Equal(0, m.Size);
            Assert.True(m.IsEmpty());
            m.Dispose();

            var n = new CStringRef(0, true);         // Zero address: nothing to free
            Assert.Equal(0, n.Size);
            Assert.True(n.IsNull());
            n.Dispose();                             // addr == Zero: no-op, must not throw
        }

        [Fact]
        public void Constructor_IntSize_Negative_Throws()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new CStringRef(-1));
            Assert.Throws<ArgumentOutOfRangeException>(() => new CStringRef(-1, true));
        }

        [Fact]
        public void StringConversions_RoundTrip()
        {
            var s = new CStringRef("abc");
            string str = s;                 // implicit CStringRef -> string
            Assert.Equal("abc", str);
            CStringRef back = (CStringRef)"abc";   // explicit string -> CStringRef
            Assert.Equal(s, back);

            s.Dispose();
            back.Dispose();
        }

        [Fact]
        public void SpanConversions_RoundTrip()
        {
            var s = new CStringRef("abc");
            Span<byte> span = s;
            Assert.Equal(new byte[] { 97, 98, 99 }, span.ToArray());
            CStringRef viaSpan = new CStringRef(span);
            Assert.Equal("abc", viaSpan.ToString());

            s.Dispose();
            viaSpan.Dispose();
        }

        // ---------- Length / Size / Readonly ----------

        [Fact]
        public void Size_IsCapacity_NotContent()
        {
            var s = CStr.New("abc", 5);
            Assert.Equal(3, s.Length);
            Assert.Equal(8, s.Size);

            s.Dispose();
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

            s.Dispose();
        }

        [Fact]
        public void Length_Setter_ExtendWithinCapacity_DoesNotInventContent()
        {
            var s = CStr.New("hi", 6);   // Size = 8, content "hi"
            s.Length = 5;
            Assert.Equal(5, s.Length);
            Assert.Equal(new byte[] { 104, 105, 0, 0, 0 }, CStr.Content(s));

            s.Dispose();
        }

        [Fact]
        public void Length_Setter_SameValue_IsNoOp()
        {
            var s = new CStringRef("abc");
            s.Length = 3;
            Assert.Equal("abc", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Length_Setter_OutOfRange_Throws()
        {
            var s = CStr.New("abc", 2);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Length = -1);
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Length = 6);   // > Size (5)

            s.Dispose();
        }

        [Fact]
        public void Length_Setter_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.True(s.Readonly);
            Assert.Throws<InvalidOperationException>(() => s.Length = 2);

            s.Dispose();
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

            s.Dispose();
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

            s.Dispose();
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

            s.Dispose();
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

            s.Dispose();
        }

        [Fact]
        public void Indexer_Set_NulInsideContent_Truncates()
        {
            var s = new CStringRef("hello");
            Assert.Equal("hello", s.ToString());
            s[2] = 0;
            Assert.Equal(2, s.Length);
            Assert.Equal("he", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Indexer_Set_NulAtLength_KeepsLength()
        {
            var s = CStr.New("ab", 2);
            s[2] = 0;
            Assert.Equal(2, s.Length);
            Assert.Equal("ab", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Indexer_Set_Readonly_Throws()
        {
            var s = new CStringRef("abc");
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s[1] = (byte)'x');

            s.Dispose();
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

            s.Dispose();
        }

        // ---------- cache ----------

        [Fact]
        public void ToString_ReturnsCachedInstance()
        {
            var s = new CStringRef("abc");
            object a = s.ToString();
            object b = s.ToString();
            Assert.Same(a, b);

            s.Dispose();
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

            s.Dispose();
            clone.Dispose();
        }

        [Fact]
        public void CopyTo_OtherCStringRef()
        {
            var src = new CStringRef("hi");
            var dst = CStr.New("xxxxx", 0);
            src.CopyTo(ref dst);
            Assert.Equal("hi", dst.ToString());

            src.Dispose();
            dst.Dispose();
        }

        [Fact]
        public void CopyTo_Span_WritesContent()
        {
            var span = new byte[4];
            var src = new CStringRef("hi");
            src.CopyTo(span);
            Assert.Equal(new byte[] { 104, 105, 0, 0 }, span);

            src.Dispose();
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

            a.Dispose();
            b.Dispose();
        }

        [Fact]
        public void Equals_DifferentContent_False()
        {
            var a = new CStringRef("hello");
            var b = new CStringRef("hellp");
            Assert.False(a.Equals(b));
            Assert.True(a != b);

            a.Dispose();
            b.Dispose();
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

            a.Dispose();
            b.Dispose();
        }

        [Fact]
        public void CompareTo_ReadOnlySpan_MatchesCStringRefBehaviour()
        {
            var a = new CStringRef("ab");
            Assert.Equal(0, a.CompareTo((ReadOnlySpan<byte>)CStr.Bytes("ab")));
            Assert.True(a.CompareTo((ReadOnlySpan<byte>)CStr.Bytes("abc")) < 0);   // shorter prefix first
            Assert.True(a.CompareTo((ReadOnlySpan<byte>)CStr.Bytes("aa")) > 0);    // byte difference
            Assert.True(a.CompareTo((ReadOnlySpan<byte>)CStr.Bytes("a")) > 0);     // length tiebreak
            Assert.True(a.CompareTo(default(ReadOnlySpan<byte>)) > 0);             // empty span sorts first

            a.Dispose();
        }

        [Fact]
        public void Equals_ReadOnlySpan_ByContent()
        {
            var a = new CStringRef("hello");
            Assert.True(a.Equals((ReadOnlySpan<byte>)CStr.Bytes("hello")));
            Assert.False(a.Equals((ReadOnlySpan<byte>)CStr.Bytes("hell")));        // shorter
            Assert.False(a.Equals((ReadOnlySpan<byte>)CStr.Bytes("hello!")));      // longer
            Assert.False(a.Equals((ReadOnlySpan<byte>)CStr.Bytes("hellp")));       // last byte differs
            Assert.True(a.Equals(default(ReadOnlySpan<byte>)) == a.IsEmpty());     // empty vs empty

            var e = new CStringRef("");
            Assert.True(e.Equals(default(ReadOnlySpan<byte>)));                    // both empty
            Assert.False(e.Equals((ReadOnlySpan<byte>)CStr.Bytes("x")));

            a.Dispose();
            e.Dispose();
        }

        [Fact]
        public void GetHashCode_EqualForEqualContent()
        {
            var a = new CStringRef("hello");
            var b = CStr.New("hello", 3);
            Assert.Equal(a.GetHashCode(), b.GetHashCode());

            a.Dispose();
            b.Dispose();
        }

        // ---------- slicing / substring ----------

        [Fact]
        public void Slice_ReturnsCurrentLogicalContent()
        {
            var s = CStr.New("hello", 3);
            Assert.Equal(CStr.Content(s), s.Slice().ToArray());

            s.Dispose();
        }

        [Fact]
        public void Slice_OffsetAndLength()
        {
            var s = new CStringRef("hello world");
            Assert.Equal(CStr.Bytes("lo wo"), s.Slice(3, 5).ToArray());

            s.Dispose();
        }

        [Fact]
        public void Substring_Middle_ReturnsExactContent()
        {
            var parent = new CStringRef("hello world");
            var sub = parent.Substring(6, 5);      // CStringSpan view: no slot, no Dispose
            Assert.Equal("world", sub.ToString());

            parent.Dispose();
        }

        [Fact]
        public void Substring_Empty_ValidAtBothEnds()
        {
            var s = new CStringRef("abc");
            Assert.Equal("", s.Substring(0, 0).ToString());
            Assert.Equal("", s.Substring(3, 0).ToString());

            s.Dispose();
        }

        [Fact]
        public void Substring_Tail_IsAViewOverSameBuffer()
        {
            var parent = CStr.New("hello world", 4);
            var tail = parent.Substring(6, parent.Length - 6);   // CStringSpan view over the same buffer
            Assert.Equal("world", tail.ToString());

            parent[6] = (byte)'W';      // mutate the parent through indexer
            Assert.Equal((byte)'W', tail[0]);

            parent.Dispose();
        }

        [Fact]
        public void Substring_InvalidRange_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(-1, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(3, 1));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.Substring(1, 3));

            s.Dispose();
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

            s.Dispose();
        }

        // ---------- Track / Untrack (UnionRef mode switching) ----------

        [Fact]
        public void Track_Untrack_RoundTrip_PreservesContent()
        {
            var s = new CStringRef("hello");       // net10: Tracking mode by default
            s.Untrack();                            // Tracking → Raw (old slot returned)
            Assert.Equal("hello", s.ToString());    // content survives the switch
            Assert.Equal(5, s.Length);
            s.Track();                              // Raw → Tracking (fresh slot leased)
            Assert.Equal("hello", s.ToString());
            Assert.Equal(5, s.Length);

            s.Dispose();                            // returns the current Tracking slot
        }

        [Fact]
        public void Untrack_ThenMutate_StillWorks()
        {
            var s = CStr.New("he", 8);
            s.Untrack();                            // Raw mode: plain address, no slot
            s[2] = (byte)'y';                       // indexer write through the raw address
            Assert.Equal("hey", s.ToString());
            var r = s.Append((byte)'!');            // mutation chain in Raw mode
            Assert.Equal("hey!", r.ToString());

            r.Track();                              // back to Tracking before Dispose
            r.Dispose();
        }

        [Fact]
        public void RepeatedSwitching_KeepsPoolConsistent()
        {
            // Each Untrack returns the old Tracking slot; each Track leases a fresh
            // one. Many rounds must not throw (the IntBag bitmap would catch any
            // double return / double allocation) and the final Dispose must return
            // exactly the current slot.
            var s = new CStringRef("x");
            for (int i = 0; i < 100; i++)
            {
                s.Untrack();
                s.Track();
            }
            Assert.Equal("x", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Untrack_ThenDispose_IsSafeNoOp()
        {
            var s = new CStringRef("abc");
            s.Untrack();                // Raw mode holds no slot
            s.Dispose();                // nothing to return — must not throw
        }

        [Fact]
        public void TrackGC_SurvivesForcedCollection()
        {
            var s = new CStringRef("tracked");
            s.Track();                  // ensure Tracking mode (the net10 default anyway)
            byte[][] junk = new byte[64][];
            for (int i = 0; i < 2_000_000; i++) junk[i & 63] = new byte[96];
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.Collect(2, GCCollectionMode.Forced, true, true);
            GC.KeepAlive(junk);
            Assert.Equal("tracked", s.ToString());   // the byref followed the moved buffer
            Assert.Equal((byte)'t', s[0]);

            s.Dispose();
        }
    }
}
