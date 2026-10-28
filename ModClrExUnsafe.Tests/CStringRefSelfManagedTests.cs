using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// CStringRef in self-managed native mode (<c>_IsSelfManagedNative</c>): the view owns an
    /// unmanaged block from <c>Marshal.AllocHGlobal</c> and <c>Dispose()</c> frees it. All growth
    /// paths stay native: <c>Append</c> re-allocs in place (<c>ReAllocHGlobal</c>), Insert/Replace
    /// allocate a fresh block, copy, then free the old one, and <c>Clone()</c> allocates an
    /// independent native copy.
    ///
    /// <para>
    /// Ownership convention for this file (differs from the Tracking-mode files!): the IntPtr
    /// constructors build the view in Raw mode - <c>_Address</c> is a plain address VALUE, so
    /// every struct copy carries its own independent copy of it (no shared slot, no
    /// indirection). A growth path releases the old block (ReAllocHGlobal / explicit
    /// FreeHGlobal) and re-points only <c>this</c> at the new one; who else ends up on the
    /// new address depends on how the mutator was invoked, because a struct method re-points
    /// its receiver only when called directly on a variable:
    ///  - direct call (<c>var r = s.Append(x)</c>): s itself is re-pointed too, so s and r
    ///    are two independent handles on ONE block - dispose exactly one, never touch the
    ///    other again;
    ///  - by-value receiver (<c>var sum = a + b</c>: operator parameters are by value): the
    ///    mutation lands on the copy; the caller's a keeps the OLD address, which the growth
    ///    has already released - a dangling pointer, so only the return value is a live handle.
    /// Dispose in this mode is <c>FreeHGlobal</c> with no guard flag and no pool: disposing
    /// two handles on one block, or disposing a stale copy, corrupts the NT heap - silent,
    /// no fail-fast exception like the Tracking slot pool. In-capacity mutations move
    /// nothing: every copy stays valid, dispose one of them.
    /// </para>
    /// <para>
    /// ReAllocHGlobal does not zero the grown region, so the growth paths restore the
    /// terminating NUL themselves (<c>RebindToNewBuffer</c> writes it once for every
    /// growth path). The tests below rely on that: an in-capacity Append right after
    /// an overflowing one scans for exactly that NUL (<c>Span.Append</c> -> StrLen).
    /// </para>
    /// </summary>
    public class CStringRefSelfManagedTests
    {
        /// <summary>
        /// Allocates a zeroed native block of <paramref name="size"/> bytes holding the UTF-8
        /// text plus a terminating NUL. AllocHGlobal does not zero, so the block is cleared
        /// first - StrLen must stop on a real NUL.
        /// </summary>
        private static IntPtr AllocNative(string text, int size)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            Assert.True(bytes.Length < size, "text must fit the block with room for the NUL");
            var ptr = Marshal.AllocHGlobal(size);
            Marshal.Copy(new byte[size], 0, ptr, size);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return ptr;
        }

        /// <summary>Self-managed view with spare capacity: Size = size, Length = byte count, [Length] = 0.</summary>
        private static CStringRef NewSelfManaged(string text, int size)
        {
            return new CStringRef(AllocNative(text, size), size, Encoding.UTF8.GetByteCount(text), true);
        }

        /// <summary>Self-managed view at full capacity: Length == Size, no NUL slot behind the content.</summary>
        private static CStringRef NewFullSelfManaged(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            return new CStringRef(ptr, bytes.Length, bytes.Length, true);
        }

        private static byte At(IntPtr ptr, int offset) => Marshal.ReadByte(ptr, offset);

        // ---------- constructors ----------

        [Fact]
        public void Ctor_ExplicitLength_ReadsContent()
        {
            var ptr = AllocNative("hello", 32);
            var s = new CStringRef(ptr, 32, 5, true);
            Assert.Equal(5, s.Length);
            Assert.Equal(32, s.Size);
            Assert.False(s.Readonly);
            Assert.Equal("hello", s.ToString());
            Assert.Equal((byte)'o', s[4]);

            s.Dispose();       // frees the block
        }

        [Fact]
        public void Ctor_WithSize_ScansForNul()
        {
            var ptr = AllocNative("hello", 32);
            var s = new CStringRef(ptr, 32, true);
            Assert.Equal(5, s.Length);
            Assert.Equal(32, s.Size);
            Assert.Equal("hello", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Ctor_WithoutSize_SizeIsStrLenPlusNul()
        {
            var ptr = AllocNative("hello", 32);
            var s = new CStringRef(ptr, true);
            Assert.Equal(5, s.Length);
            Assert.Equal(6, s.Size);
            Assert.Equal("hello", s.ToString());

            s.Dispose();
        }

        [Fact]
        public void Ctor_NotSelfManaged_DisposeKeepsBlockAlive()
        {
            var ptr = AllocNative("hello", 32);
            var s = new CStringRef(ptr, 32, 5, false);
            s.Dispose();                          // isSelfManaged = false: nothing freed
            Assert.Equal((byte)'h', At(ptr, 0));  // the block is still readable
            Assert.Equal((byte)'o', At(ptr, 4));
            Marshal.FreeHGlobal(ptr);              // the caller still owns it
        }

        [Fact]
        public void Ctor_ZeroAddressZeroSize_IsEmptyView()
        {
            var s = new CStringRef(IntPtr.Zero, 0, 0, true);
            Assert.True(s.IsNull());
            Assert.True(s.IsEmpty());
            Assert.Equal(0, s.Length);
            Assert.Equal(0, s.Size);
            s.Dispose();                          // addr == Zero: nothing to free, must not throw

            var scanned = new CStringRef(IntPtr.Zero, 0, true);   // StrLen(Zero, 0) == 0
            Assert.True(scanned.IsNull());
            Assert.Equal(0, scanned.Length);
            Assert.Equal(0, scanned.Size);
            scanned.Dispose();
        }

        // ---------- Dispose ----------

        [Fact]
        public void Dispose_SelfManaged_DoesNotThrow()
        {
            var s = NewSelfManaged("dispose-me", 32);
            Assert.NotEqual(IntPtr.Zero, s.Address);
            s.Dispose();
        }

        // ---------- Append ----------

        [Fact]
        public void Append_Byte_WithinCapacity_WritesInPlace()
        {
            var s = NewSelfManaged("hi", 16);
            var before = s.Address;
            var r = s.Append((byte)'!');
            Assert.Equal("hi!", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(16, r.Size);
            Assert.Equal(before, r.Address);      // no growth: still the same block
            Assert.Equal((byte)'!', At(before, 2));

            r.Dispose();      // r and s share the address: dispose exactly one handle
        }

        [Fact]
        public void Append_Byte_Overflow_ReallocsNativeBlock()
        {
            var s = NewFullSelfManaged("ab");     // Length == Size == 2
            var r = s.Append((byte)'c');          // ReAllocHGlobal(2 -> 4)
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(4, r.Size);              // growth formula: _Length + 2

            r.Dispose();      // direct call re-pointed s too: one block, two handles -
                              // dispose exactly one and never touch the other again
        }

        [Fact]
        public void Append_CStringRef_WithinCapacity_WritesInPlace()
        {
            var s = NewSelfManaged("he", 16);
            var before = s.Address;
            var r = s.Append(new CStringRef("llo"));
            Assert.Equal("hello", r.ToString());
            Assert.Equal(5, r.Length);
            Assert.Equal(16, r.Size);
            Assert.Equal(before, r.Address);

            r.Dispose();
        }

        [Fact]
        public void Append_CStringRef_Overflow_ReallocsNativeBlock()
        {
            var s = NewFullSelfManaged("ab");         // Size == 2
            var r = s.Append(new CStringRef("cdef")); // ReAllocHGlobal(2 -> 7)
            Assert.Equal("abcdef", r.ToString());
            Assert.Equal(6, r.Length);
            Assert.Equal(7, r.Size);                  // growth formula: _Length + other._Length + 1

            r.Dispose();
        }

        [Fact]
        public void Append_CStringRef_RepeatedOverflow_ReallocsEachTime()
        {
            var s = NewFullSelfManaged("a");          // Size 1
            var r = s.Append(new CStringRef("bc"));   // ReAllocHGlobal(1 -> 4)
            Assert.Equal("abc", r.ToString());
            Assert.Equal(4, r.Size);
            var r2 = r.Append(new CStringRef("def")); // 3 + 3 > 4: ReAllocHGlobal(4 -> 7)
            Assert.Equal("abcdef", r2.ToString());
            Assert.Equal(6, r2.Length);
            Assert.Equal(7, r2.Size);

            r2.Dispose();     // the live handle; r holds the same block, s holds the block the
                              // second re-alloc released - neither may be touched or disposed
        }

        [Fact]
        public void Append_EmptyOther_AtFullCapacity_IsNoOp()
        {
            var s = NewFullSelfManaged("abc");    // Length == Size == 3
            var before = s.Address;
            var r = s.Append(CStringRef.Empty);   // early return: no re-allocation
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(3, r.Size);
            Assert.Equal(before, r.Address);

            r.Dispose();      // same block: dispose one of r/s
        }

        [Fact]
        public void Append_Overflow_ThenInCapacityInsert_ThenOverflowAppend()
        {
            // A mutation chain across the native growth paths. The in-capacity Insert is
            // deterministic despite the garbage byte ReAllocHGlobal leaves behind the new
            // content: Insert never scans for a NUL, it works purely by offset.
            var s = NewFullSelfManaged("ab");     // Size == 2
            var r = s.Append((byte)'c');          // realloc -> "abc", Size 4
            Assert.Equal("abc", r.ToString());
            var r2 = r.Insert(1, (byte)'X');      // newLength 4 == Size: in-capacity shift
            Assert.Equal("aXbc", r2.ToString());
            var r3 = r2.Append((byte)'d');        // 4 == Size: realloc again -> "aXbcd"
            Assert.Equal("aXbcd", r3.ToString());
            Assert.Equal(5, r3.Length);
            Assert.Equal(6, r3.Size);

            r3.Dispose();     // the live handle; r2 holds the same block, s/r hold the block
                              // the last re-alloc released - only r3 may be disposed
        }

        [Fact]
        public void OperatorPlus_Overflow_ResultOwnsTheReallocatedBlock()
        {
            var a = NewFullSelfManaged("ab");     // Size == 2
            var b = new CStringRef("cd");
            var sum = a + b;                      // a.Append(b): ReAllocHGlobal(2 -> 5)
            Assert.Equal("abcd", sum.ToString());
            Assert.Equal(4, sum.Length);
            Assert.Equal(5, sum.Size);

            sum.Dispose();    // operator+ received a BY VALUE: the caller's a was never
                              // re-pointed and still holds the released pre-realloc address -
                              // sum is the only live handle on the grown block
            b.Dispose();
        }

        // ---------- NUL after growth (ReAllocHGlobal/AllocHGlobal leave garbage) ----------

        [Fact]
        public void Append_Byte_Overflow_ThenInCapacityAppend_NulIsRestored()
        {
            var s = NewFullSelfManaged("ab");     // Size == 2
            var r = s.Append((byte)'c');          // ReAllocHGlobal(2 -> 4): [3] must be the NUL
            Assert.True(r.IsNullTerminated());    // not the garbage ReAllocHGlobal left there
            var r2 = r.Append((byte)'d');          // in-capacity: StrLen scans to the NUL at [3]
            Assert.Equal("abcd", r2.ToString());
            Assert.Equal(4, r2.Length);

            r2.Dispose();   // s and r hold the same block: dispose exactly one of the three
        }

        [Fact]
        public void Append_CStringRef_Overflow_ThenInCapacityAppend_NulIsRestored()
        {
            var s = NewFullSelfManaged("ab");          // Size == 2
            var r = s.Append(new CStringRef("cd"));    // ReAllocHGlobal(2 -> 5): "abcd", [4] = NUL
            Assert.True(r.IsNullTerminated());
            var r2 = r.Append((byte)'e');              // in-capacity: lands right behind the NUL
            Assert.Equal("abcde", r2.ToString());
            Assert.Equal(5, r2.Length);

            r2.Dispose();
        }

        [Fact]
        public void Insert_Byte_Overflow_ThenInCapacityAppend_NulIsRestored()
        {
            var s = NewFullSelfManaged("ab");     // Size == 2
            var r = s.Insert(1, (byte)'x');       // AllocHGlobal(4): "axb", [3] = NUL
            Assert.True(r.IsNullTerminated());
            var r2 = r.Append((byte)'c');         // in-capacity: lands at [3]
            Assert.Equal("axbc", r2.ToString());
            Assert.Equal(4, r2.Length);

            r2.Dispose();
        }

        [Fact]
        public void Replace_Overflow_ThenInCapacityAppend_NulIsRestored()
        {
            var s = NewFullSelfManaged("abcabc");     // Size == 6
            var r = s.Replace(new CStringRef("c"), new CStringRef("ZZ"));   // AllocHGlobal(9): [8] = NUL
            Assert.True(r.IsNullTerminated());
            var r2 = r.Append((byte)'!');             // in-capacity: lands at [8]
            Assert.Equal("abZZabZZ!", r2.ToString());
            Assert.Equal(9, r2.Length);

            r2.Dispose();
        }

        // ---------- Insert ----------

        [Fact]
        public void Insert_Byte_WithinCapacity_WritesInPlace()
        {
            var s = NewSelfManaged("ac", 16);
            var before = s.Address;
            var r = s.Insert(1, (byte)'b');
            Assert.Equal("abc", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(16, r.Size);
            Assert.Equal(before, r.Address);

            r.Dispose();
        }

        [Fact]
        public void Insert_Byte_Overflow_AllocsFreshNativeBlock()
        {
            var s = NewFullSelfManaged("ab");     // Size == 2
            var r = s.Insert(1, (byte)'x');       // newLength 3 > 2: AllocHGlobal(4) + copy + free old
            Assert.Equal("axb", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(4, r.Size);

            r.Dispose();      // direct call re-pointed s too: one block, two handles, dispose one
        }

        [Fact]
        public void Insert_CStringRef_Overflow_AllocsFreshNativeBlock()
        {
            var s = NewFullSelfManaged("ac");         // Size == 2
            var r = s.Insert(1, new CStringRef("bb")); // newLength 4 > 2
            Assert.Equal("abbc", r.ToString());
            Assert.Equal(4, r.Length);
            Assert.Equal(5, r.Size);

            r.Dispose();
        }

        [Fact]
        public void Insert_SubstringViewOfSelf_Overflow_ReadsOldBlockBeforeFree()
        {
            // The growth path must copy out of the OLD block (the view's source) before
            // freeing it - alloc-new, copy, free-old, in that order.
            var s = NewFullSelfManaged("abc");    // Size == 3
            var one = s.Substring(1, 1);          // "b": CStringSpan view into the native block
            var r = s.Insert(0, (CStringRef)one); // newLength 4 > 3
            Assert.Equal("babc", r.ToString());

            r.Dispose();
        }

        // ---------- Replace ----------

        [Fact]
        public void Replace_Byte_InPlace_KeepsBlock()
        {
            var s = NewSelfManaged("a.b.c", 16);
            var before = s.Address;
            var r = s.Replace((byte)'.', (byte)'-');
            Assert.Equal("a-b-c", r.ToString());
            Assert.Equal(before, r.Address);
            Assert.Equal((byte)'-', At(before, 1));

            r.Dispose();
        }

        [Fact]
        public void Replace_Growth_Overflow_AllocsFreshNativeBlock()
        {
            var s = NewFullSelfManaged("abcabc");     // Size == 6
            var r = s.Replace(new CStringRef("c"), new CStringRef("ZZ"));   // newTotal 8 > 6
            Assert.Equal("abZZabZZ", r.ToString());
            Assert.Equal(8, r.Length);
            Assert.Equal(9, r.Size);

            r.Dispose();
        }

        [Fact]
        public void Replace_Growth_WithinCapacity_WritesTerminatingNul()
        {
            // In-capacity growth writes the NUL behind the new content explicitly, so a
            // follow-up in-capacity Append (which scans for that NUL) stays correct.
            var s = NewSelfManaged("aXbXc", 16);
            var r = s.Replace(new CStringRef("X"), new CStringRef("YY"));   // newTotal 7 <= 16
            Assert.Equal("aYYbYYc", r.ToString());
            var r2 = r.Append((byte)'!');        // in-capacity: lands right behind the NUL
            Assert.Equal("aYYbYYc!", r2.ToString());
            Assert.Equal(8, r2.Length);

            r2.Dispose();    // the whole chain stayed in the same block
        }

        [Fact]
        public void Replace_Shrink_InPlace_CleansTail()
        {
            var s = NewSelfManaged("a1b1c1d1", 16);
            var before = s.Address;
            var r = s.Replace(new CStringRef("1"), CStringRef.Empty);
            Assert.Equal("abcd", r.ToString());
            Assert.Equal(4, r.Length);
            Assert.Equal(before, r.Address);
            Assert.Equal((byte)0, At(before, 4));    // Length setter zeroed the tail

            r.Dispose();
        }

        // ---------- Clone ----------

        [Fact]
        public void Clone_AllocatesIndependentNativeBlock()
        {
            var s = NewSelfManaged("hello", 16);
            var clone = s.Clone();
            Assert.Equal("hello", clone.ToString());
            Assert.NotEqual(s.Address, clone.Address);   // a fresh block, not a view
            clone[0] = (byte)'H';
            Assert.Equal("Hello", clone.ToString());
            Assert.Equal("hello", s.ToString());          // the source block is untouched
            clone.Dispose();                              // frees the clone's own block only
            Assert.Equal("hello", s.ToString());          // ... and the source survived that
            s.Dispose();
        }

        [Fact]
        public void Clone_ResultIsMutableAndGrowable()
        {
            var s = NewSelfManaged("hi", 16);
            var clone = s.Clone();                // AllocHGlobal(3): "hi\0", Size 3
            var r = clone.Append((byte)'!');      // in-capacity (the NUL came along with the copy)
            Assert.Equal("hi!", r.ToString());
            var r2 = r.Append((byte)'!');         // full now: ReAllocHGlobal(3 -> 5)
            Assert.Equal("hi!!", r2.ToString());
            Assert.Equal(4, r2.Length);
            Assert.Equal(5, r2.Size);

            r2.Dispose();    // the live handle; clone holds the block the re-alloc released
            s.Dispose();
        }

        // ---------- CopyTo / CopyFrom ----------

        [Fact]
        public void CopyTo_SelfManagedTarget_OverwritesContent()
        {
            var src = new CStringRef("hello");
            var dst = NewSelfManaged("xxxxxxx", 16);
            src.CopyTo(ref dst);
            Assert.Equal("hello", dst.ToString());
            Assert.Equal(5, dst.Length);
            Assert.Equal((byte)0, At(dst.Address, 5));   // the tail was cleaned

            src.Dispose();
            dst.Dispose();
        }

        [Fact]
        public void CopyFrom_SelfManagedSourceIntoManaged()
        {
            var src = NewSelfManaged("hello", 16);
            var dst = CStr.New("xxxxx", 3);
            dst.CopyFrom(src);
            Assert.Equal("hello", dst.ToString());

            src.Dispose();
            dst.Dispose();
        }

        // ---------- consistency with the managed growth path ----------

        [Fact]
        public void OverflowAppend_MatchesManagedGrowthFormula()
        {
            var native = NewFullSelfManaged("ab");    // Size 2, Length 2
            var managed = new CStringRef("ab");       // Size 2, Length 2
            var rn = native.Append((byte)'c');        // ReAllocHGlobal path
            var rm = managed.Append((byte)'c');       // new byte[] path
            Assert.Equal(rm.ToString(), rn.ToString());
            Assert.Equal(rm.Length, rn.Length);
            Assert.Equal(rm.Size, rn.Size);           // same growth formula: _Length + 2
            Assert.True(rn.Equals(rm));

            rn.Dispose();
            rm.Dispose();
        }

        [Fact]
        public void SameInCapacitySequence_MatchesManagedInstance()
        {
            var native = NewSelfManaged("a.b.c", 16);
            var managed = CStr.New("a.b.c", 11);      // same Size (16)
            native.Replace((byte)'.', (byte)'-');
            managed.Replace((byte)'.', (byte)'-');
            native.Insert(0, (byte)'>');
            managed.Insert(0, (byte)'>');
            native.Remove(1, 1);
            managed.Remove(1, 1);
            Assert.Equal(">-b-c", native.ToString());
            Assert.Equal(managed.ToString(), native.ToString());
            Assert.Equal(managed.Length, native.Length);
            Assert.Equal(CStr.Content(managed), CStr.Content(native));
            Assert.Equal(managed.GetHashCode(), native.GetHashCode());

            native.Dispose();
            managed.Dispose();
        }

        // ---------- readonly ----------

        [Fact]
        public void Readonly_Mutators_Throw_BeforeTouchingTheBlock()
        {
            var s = NewSelfManaged("abc", 16);
            var before = s.Address;
            s.Freeze();
            Assert.Throws<InvalidOperationException>(() => s.Append((byte)'x'));
            Assert.Throws<InvalidOperationException>(() => s.Insert(1, (byte)'x'));
            Assert.Throws<InvalidOperationException>(() => s.Replace(new CStringRef("a"), new CStringRef("z")));
            Assert.Throws<InvalidOperationException>(() => { s.Length = 1; });
            Assert.Equal("abc", s.ToString());
            Assert.Equal(before, s.Address);     // nothing re-allocated

            s.Dispose();
        }

        // ---------- reads & views over the owned block ----------

        [Fact]
        public void Indexer_RefIndexer_AndSearch_ReachTheOwnedBlock()
        {
            var s = NewSelfManaged("hello world", 16);
            s[0] = (byte)'H';
            Assert.Equal((byte)'H', At(s.Address, 0));       // plain indexer write reached the block
            s.Ref[6] = (byte)'W';
            Assert.Equal((byte)'W', At(s.Address, 6));       // ref indexer write reached the block
            Assert.Equal("Hello World", s.ToString());
            Assert.Equal(6, s.IndexOf((byte)'W'));
            Assert.Equal(9, s.LastIndexOf((byte)'l'));
            Assert.True(s.Contains(new CStringSpan("lo Wo").Slice()));
            Assert.True(s.StartsWith(new CStringSpan("Hello").Slice()));
            Assert.True(s.EndsWith(new CStringSpan("World").Slice()));
            var tail = s.Substring(6);                       // CStringSpan view into the owned block
            Assert.Equal("World", tail.ToString());
            s[6] = (byte)'w';
            Assert.Equal((byte)'w', tail[0]);                // the view follows the block

            s.Dispose();
        }

        [Fact]
        public void Trim_AndRemove_ShiftInPlaceAndCleanTail()
        {
            var s = NewSelfManaged("..hi..", 16);
            var before = s.Address;
            s.Trim(CStr.Bytes("."));
            Assert.Equal("hi", s.ToString());
            Assert.Equal((byte)'h', At(before, 0));          // shifted left inside the same block
            Assert.Equal((byte)0, At(before, 2));            // tail cleaned

            var s2 = NewSelfManaged("abcdef", 16);
            s2.Remove(2, 2);
            Assert.Equal("abef", s2.ToString());
            Assert.Equal((byte)0, At(s2.Address, 4));

            s.Dispose();
            s2.Dispose();
        }

        [Fact]
        public void View_SurvivesAllocationChurn()
        {
            // Raw mode + a native block: the GC has nothing to say about either. Natural
            // churn only (no forced collection - parallel test classes own managed buffers
            // that a forced GC must not relocate behind their backs).
            var s = NewSelfManaged("stable", 16);
            for (int i = 0; i < 64; ++i)
            {
                _ = new byte[1024];
            }
            Assert.Equal("stable", s.ToString());
            Assert.Equal((byte)'s', s[0]);
            Assert.Equal((byte)'e', s[5]);

            s.Dispose();
        }

        [Fact]
        public void Append_String_WithinCapacity_DoesNotAllocate()
        {
            var s = NewSelfManaged("he", 16);
            Assert.Equal("he", s.ToString());   // warm the string cache before measuring
            long before = GC.GetAllocatedBytesForCurrentThread();
            var r = s.Append("llo");            // in-capacity: encodes straight into the native block
            long after = GC.GetAllocatedBytesForCurrentThread();
            Assert.Equal(0, after - before);    // no byte[] round-trip, no stackalloc copy
            Assert.Equal("hello", r.ToString());

            r.Dispose();   // r and s share the address: dispose exactly one handle
        }

        // ---------- growth from an empty view ----------

        [Fact]
        public void ZeroAddress_EmptyView_AppendGrowsViaReAlloc()
        {
            var s = new CStringRef(IntPtr.Zero, 0, 0, true);
            var r = s.Append((byte)'a');          // ReAllocHGlobal(Zero, 2) == AllocHGlobal(2)
            Assert.Equal("a", r.ToString());
            Assert.Equal(1, r.Length);
            Assert.Equal(2, r.Size);
            Assert.False(r.IsNull());

            r.Dispose();     // s was re-pointed to the same block: dispose exactly one handle
        }

        // ---------- Resize / Expand (native realloc paths) ----------

        [Fact]
        public void Resize_Expand_ReallocsNativeBlock()
        {
            var s = NewSelfManaged("hello", 16);
            var r = s.Resize(32);                 // ReAllocHGlobal(16 -> 32)
            Assert.Equal("hello", r.ToString());
            Assert.Equal(32, r.Size);
            Assert.Equal(5, r.Length);
            Assert.True(r.IsNullTerminated());      // NUL restored over the realloc garbage

            r.Dispose();       // direct call re-pointed s too: one block, dispose exactly one
        }

        [Fact]
        public void Resize_Shrink_ReallocsDown_AndTruncates()
        {
            var s = NewSelfManaged("hello", 16);
            var r = s.Resize(3);                  // ReAllocHGlobal(16 -> 3): tail released
            Assert.Equal("hel", r.ToString());
            Assert.Equal(3, r.Length);
            Assert.Equal(3, r.Size);

            r.Dispose();
        }

        [Fact]
        public void Resize_EmptyStart_GrowsViaReAlloc()
        {
            var s = new CStringRef(true);          // empty self-managed view (Zero address)
            var r = s.Resize(8);                   // ReAllocHGlobal(Zero, 8) == AllocHGlobal(8)
            Assert.Equal(8, r.Size);
            Assert.Equal(0, r.Length);
            r[0] = (byte)'h';                      // indexer extends Length
            r[1] = (byte)'i';
            Assert.Equal("hi", r.ToString());

            r.Dispose();       // s was re-pointed to the same block: dispose exactly one handle
        }

        [Fact]
        public void Expand_Overflow_ReallocsNativeBlock()
        {
            var s = NewSelfManaged("hi", 8);
            var r = s.Expand(24);
            Assert.Equal("hi", r.ToString());
            Assert.Equal(24, r.Size);
            Assert.True(r.IsNullTerminated());

            r.Dispose();
        }
    }
}
