using System;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// GC stress for <b>managed-backed</b> <see cref="CStringRef"/> (UnionRef Tracking mode).
    ///
    /// <para>
    /// Background: the old RefContainer design stored the buffer address in an <c>object</c> slot,
    /// so any GC while a managed-backed view was alive crashed the process (0x80131506). The
    /// UnionRef migration (Tracking mode on .NET Core) is supposed to fix that: the byref is
    /// tracked by the GC and follows the buffer across compacting collections.
    /// </para>
    /// <para>
    /// These tests deliberately avoid out-of-capacity Append/Insert/Replace (their snapshot path
    /// goes through cross-region MemCopy - a separate, known bug); only in-capacity mutations
    /// and reads are used here so a failure unambiguously points at GC handling.
    /// </para>
    /// <para>
    /// Convention (same as the other CStringRef test files): every constructed CStringRef local
    /// is disposed before the test returns - the slot lease is load-bearing.
    /// </para>
    /// </summary>
    public class CStringRefManagedGcStressTests
    {
        private static void ForceFullCompactGC()
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        /// <summary>Allocation churn that promotes fragmentation and makes compaction likely.</summary>
        private static byte[][] Churn(int rounds = 2_000_000, int size = 96)
        {
            var junk = new byte[64][];
            for (int i = 0; i < rounds; i++)
            {
                junk[i & 63] = new byte[size];
            }
            return junk;
        }

        // ---------- 1. default construction path (no explicit Track) ----------

        [Fact]
        public void DefaultCtor_SurvivesForcedCompactGC()
        {
            // On .NET Core the Span ctor picks Tracking mode automatically; no explicit
            // s.Track() here - the default path itself must be GC-safe.
            var s = CStr.New("hello-gc");
            var junk = Churn();
            ForceFullCompactGC();
            GC.KeepAlive(junk);

            Assert.Equal("hello-gc", s.ToString());
            Assert.Equal(8, s.Length);
            Assert.Equal((byte)'h', s[0]);
            Assert.Equal((byte)'c', s[7]);

            s.Dispose();
        }

        // ---------- 2. boxing (one of the three always-crash shapes of the old design) ----------

        [Fact]
        public void BoxedInstance_SurvivesForcedGC()
        {
            var s = CStr.New("boxed");
            object boxed = s;                       // struct copy onto the heap
            s = default;                            // the boxed copy is now the sole owner
            var junk = Churn();
            ForceFullCompactGC();
            GC.KeepAlive(junk);

            var unboxed = (CStringRef)boxed;
            Assert.Equal("boxed", unboxed.ToString());
            Assert.Equal(5, unboxed.Length);

            unboxed.Dispose();                      // single dispose: the boxed copy owns the slot
        }

        // ---------- 3. many instances alive at once ----------

        [Fact]
        public void ManyInstancesAliveTogether_AcrossGC()
        {
            const int Count = 16;
            var all = new CStringRef[Count];
            for (int i = 0; i < Count; i++)
            {
                all[i] = CStr.New("item-" + i);
            }

            var junk = Churn();
            ForceFullCompactGC();
            GC.KeepAlive(junk);

            for (int i = 0; i < Count; i++)
            {
                Assert.Equal("item-" + i, all[i].ToString());
                Assert.Equal(CStr.Bytes("item-" + i), CStr.Content(all[i]));
            }

            foreach (var s in all)
            {
                s.Dispose();
            }
        }

        // ---------- 4. the byref must follow a moved buffer ----------

        [Fact]
        public void TrackingByRef_FollowsMovedBuffer()
        {
            var s = CStr.New("follow-me");
            IntPtr before = s.ToSpan().GetAddress();

            var junk = Churn(4_000_000, 128);       // heavier churn -> compaction very likely
            ForceFullCompactGC();
            GC.KeepAlive(junk);

            IntPtr after = s.ToSpan().GetAddress();
            // The address may or may not change (compaction is best-effort); what must hold in
            // both cases is that the view still reads the right bytes - if the buffer moved,
            // the tracked byref has to have been rewritten to the new location.
            Assert.Equal("follow-me", s.ToString());
            Assert.Equal((byte)'f', s[0]);
            Assert.Equal((byte)'e', s[8]);
            if (before != after)
            {
                // moved: verify every byte through the (rewritten) byref
                Assert.Equal(CStr.Bytes("follow-me"), CStr.Content(s));
            }

            s.Dispose();
        }

        // ---------- 5. mutations interleaved with collections ----------

        [Fact]
        public void MutationsInterleavedWithGC_KeepConsistency()
        {
            var s = CStr.New("aaaaaaaa", 0);        // len 8, no spare capacity needed
            var expected = "aaaaaaaa".ToCharArray();

            for (int round = 0; round < 8; round++)
            {
                int idx = round % 8;
                char c = (char)('A' + round);
                s[idx] = (byte)c;                   // indexer write (in-capacity, no MemCopy)
                expected[idx] = c;

                ForceFullCompactGC();

                Assert.Equal(new string(expected), s.ToString());
            }

            s.Dispose();
        }

        // ---------- 6. in-capacity Append interleaved with collections ----------

        [Fact]
        public void InCapacityAppend_InterleavedWithGC()
        {
            var s = CStr.New("ab", 8);              // capacity 10, len 2

            s.Append((byte)'c');                    // in-capacity: ToSpan().Append, no MemCopy
            ForceFullCompactGC();
            Assert.Equal("abc", s.ToString());

            s.Append((byte)'d');
            var junk = Churn();
            ForceFullCompactGC();
            GC.KeepAlive(junk);
            Assert.Equal("abcd", s.ToString());
            Assert.Equal(4, s.Length);

            s.Dispose();
        }

        // ---------- 7. alloc / discard / survive cycles (slot pool consistency) ----------

        [Fact]
        public void AllocDiscardSurvive_Cycles_KeepPoolAndContentConsistent()
        {
            const int Survivors = 10;
            const int Rounds = 200;
            var alive = new CStringRef[Survivors];
            for (int i = 0; i < Survivors; i++)
            {
                alive[i] = CStr.New("alive-" + i);
            }

            for (int round = 0; round < Rounds; round++)
            {
                // short-lived view: lease + release every round; a broken slot pool (double
                // return / double allocation) would throw here.
                using (var tmp = CStr.New("tmp-" + round))
                {
                    Assert.Equal("tmp-" + round, tmp.ToString());
                }

                if (round % 50 == 49)
                {
                    ForceFullCompactGC();
                    for (int i = 0; i < Survivors; i++)
                    {
                        Assert.Equal("alive-" + i, alive[i].ToString());
                    }
                }
            }

            foreach (var s in alive)
            {
                s.Dispose();
            }
        }

        // ---------- 8. held as an object field across natural collections ----------

        private sealed class Holder
        {
            public CStringRef S;
        }

        [Fact]
        public void HolderField_SurvivesNaturalGC()
        {
            var holder = new Holder { S = CStr.New("held-field") };

            // Natural collections only - no GC.Collect call (same shape as the unmanaged
            // counterpart in CStringRefNativeAddressTests).
            for (int i = 0; i < 4000; i++)
            {
                var junk = new byte[64 * 1024];
                junk[0] = (byte)i;
                GC.KeepAlive(junk);
            }

            Assert.Equal("held-field", holder.S.ToString());
            Assert.Equal((byte)'h', holder.S[0]);
            Assert.Equal(10, holder.S.Length);

            holder.S.Dispose();
        }
    }
}
