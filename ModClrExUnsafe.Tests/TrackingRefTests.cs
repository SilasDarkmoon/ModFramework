using Mod.LowLevel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// GC-compaction semantics of RawTrackingRef (ByRefUtils.TrackingRef.dll):
    /// the tracking slot must be updated by a compacting GC, so GetRef() keeps
    /// serving the MOVED address. The check is done purely with byrefs:
    ///
    ///   r1 = GetRef()   - a real GC-tracked byref, moved by the GC along with
    ///                     the target; it is the "truth"
    ///   r2 = GetRef()   - freshly served from the slot
    ///
    /// Both track the same target, so Ref.RefEquals(ref r1, ref r2) must hold
    /// forever: both byrefs are updated by the GC in lockstep. The ONLY way
    /// they can differ is the slot serving a stale (pre-move) address to r2
    /// while the GC already moved r1 - that is exactly the bug being probed.
    /// No IntPtr snapshots are taken: converting a byref to an address and
    /// comparing those has its own race (the two snapshots can straddle a GC
    /// move and produce false positives).
    /// </summary>
    public class TrackingRefTests
    {
        private class Target { public int Field = 77; }

        [Fact]
        public void RawTrackingRef_GetRef_CompactGC_RefEqualsConsistency()
        {
            int stop = 0;
            var failures = new System.Collections.Concurrent.ConcurrentQueue<string>();

            var churn = Task.Run(() =>
            {
                while (Volatile.Read(ref stop) == 0)
                {
                    var arr = new byte[64 * 1024];
                    arr[0] = 1;
                }
            });
            var compact = Task.Run(() =>
            {
                while (Volatile.Read(ref stop) == 0)
                {
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    Thread.Sleep(5);
                }
            });

            try
            {
                for (int round = 0; round < 20; ++round)
                {
                    var target = new Target { Field = 77 };
                    using (var tr = RawTrackingRef<int>.Create())
                    {
                        tr.SetRef(ref target.Field);

                        // r1: the GC-tracked truth. Re-fetch it once before the
                        // polling loop so both sides come from GetRef.
                        ref int r1 = ref tr.GetRef();

                        for (int i = 0; i < 500_000; ++i)
                        {
                            ref int r2 = ref tr.GetRef();
                            if (!Ref.RefEquals(ref r1, ref r2))
                            {
                                // stale slot served to r2 while the GC moved r1
                                // (r1 is by-ref and always current; r2 came from
                                // the slot and lagged behind the move)
                                failures.Enqueue(
                                    $"round {round}, poll {i}: RefEquals(r1, r2) is false - " +
                                    $"stale slot (r1 reads {r1}, r2 reads {r2})");
                                break;
                            }
                        }
                    }
                }
            }
            finally
            {
                Volatile.Write(ref stop, 1);
                Task.WaitAll(churn, compact);
            }

            Assert.Empty(failures);
        }
    }
}
