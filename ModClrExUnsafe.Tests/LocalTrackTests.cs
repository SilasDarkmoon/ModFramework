using Mod.LowLevel;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Small-scale GC-compaction regressions distilled from the LocalTrackTest
    /// stress harness (four-platform: net10/net48/Mono/NativeAOT). The stress
    /// runs billions of iterations; these run seconds but keep the same probe
    /// discipline (unique cookie + write-through + identity check).
    ///
    /// Guarded invariants (2026-10-26, inner RawRef.GetRef carries
    /// [MethodImpl(NoInlining)] — the call boundary is what makes these hold):
    ///   1. The manager slot (int&-declared stack locals in MakeMoreSlot)
    ///      FOLLOWS a compacting GC: GetRef keeps serving the moved address.
    ///   2. The double-read check in RawTrackingRef/LocalRef.GetRef stays
    ///      alive (the NoInlining call is an alias barrier — the two slot
    ///      reads cannot be merged by VN/CSE).
    ///   3. rv is typed by the call SIGNATURE (T&), so the GC updates it
    ///      after the check passes — no stale byref escapes.
    /// </summary>
    public class LocalTrackTests
    {
        private class Target { public int Field; }

        // ============ 1. deterministic follow-check (verify mode, small) ============
        // Forces a compacting GC BETWEEN SetRef and GetRef, then verifies the
        // served ref still reads/writes the MOVED object. Direct judgement for
        // "the second-level field (slot content) follows the GC".
        [Fact]
        public void RawTrackingRef_GetRef_ForcedCompactGC_SlotFollowsMove()
        {
            var tr = RawTrackingRef<int>.Create();
            Assert.False(tr.Ref2Ref.Address == IntPtr.Zero, "Create() failed (no slot)");
            try
            {
                long moved = 0, readFail = 0, writeFail = 0;
                for (int i = 0; i < 50; i++)
                {
                    int cookie = i;
                    // churn: holes give the compactor a reason to move survivors
                    for (int j = 0; j < 32; j++) { var garbage = new byte[96]; garbage[0] = 1; }

                    var target = new Target { Field = cookie };
                    tr.SetRef(ref target.Field);

                    // move detection (pre/post snapshots around the forced GC)
                    RawRef pre = new RawRef();
                    pre.SetRef(ref target.Field);
                    IntPtr before = pre.Address;
                    GC.Collect(0, GCCollectionMode.Forced, true, true);
                    GC.Collect(1, GCCollectionMode.Forced, true, true);
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    RawRef post = new RawRef();
                    post.SetRef(ref target.Field);
                    if (before == post.Address) continue;   // not moved → trivially fine

                    moved++;
                    ref int r = ref tr.GetRef();
                    int read = r;
                    if (read != cookie) readFail++;
                    r = cookie + 1;
                    if (target.Field != cookie + 1) writeFail++;
                    r = cookie;
                }
                // On a compacting GC (moved > 0) the slot MUST have followed:
                // every read and write must have hit the moved object.
                Assert.True(readFail == 0 && writeFail == 0,
                    $"slot did not follow the move: read-fail={readFail} write-fail={writeFail} (moved={moved}/50)");
            }
            finally
            {
                tr.Dispose();
            }
        }

        // ============ 2. concurrent compacting GC (theap mode, small) ============
        // Workers + churn + compacting GC thread. Exercises the double-read
        // check and the signature-typed rv under real GC timing. Small enough
        // for CI (seconds); the full-scale version (64 workers x 5M) lives in
        // LocalTrackTest and caught every regression in this area.
        [Fact]
        public void RawTrackingRef_GetRef_ConcurrentCompactingGC_ReadWriteIdentity()
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
            var compact = new Thread(() =>
            {
                int tick = 0;
                while (Volatile.Read(ref stop) == 0)
                {
                    GC.Collect(0, GCCollectionMode.Forced, true, true);
                    GC.Collect(1, GCCollectionMode.Forced, true, true);
                    if ((tick++ & 7) == 0) GC.Collect(2, GCCollectionMode.Forced, true, true);
                    Thread.Sleep(1);
                }
            });
            compact.Start();

            try
            {
                const int workers = 4, iters = 200_000;
                var ws = new Task[workers];
                for (int w = 0; w < workers; w++)
                {
                    int wid = w;
                    ws[w] = Task.Run(() =>
                    {
                        var tr = RawTrackingRef<int>.Create();
                        if (tr.Ref2Ref.Address == IntPtr.Zero) return;   // no slot — skip
                        try
                        {
                            for (long i = 0; i < iters; i++)
                            {
                                int cookie = (int)(i & 0x3FFFFFFF);
                                var target = new Target { Field = cookie };
                                tr.SetRef(ref target.Field);
                                try
                                {
                                    ref int r = ref tr.GetRef();
                                    int read = r;
                                    if (read != cookie)
                                        failures.Enqueue($"w{wid} i{i}: READ read={read} cookie={cookie}");
                                    r = cookie + 1;
                                    if (target.Field != cookie + 1)
                                        failures.Enqueue($"w{wid} i{i}: WRITE target.Field={target.Field} expected={cookie + 1} (write missed the moved object)");
                                    if (!Ref.RefEquals(ref r, ref target.Field))
                                        failures.Enqueue($"w{wid} i{i}: IDENTITY served ref != truth ref (slot lagged behind the move)");
                                    r = cookie;
                                }
                                catch (NullReferenceException)
                                {
                                    failures.Enqueue($"w{wid} i{i}: NRE");
                                }
                            }
                        }
                        finally { tr.Dispose(); }
                    });
                }
                Task.WaitAll(ws);
            }
            finally
            {
                Volatile.Write(ref stop, 1);
                churn.Wait();
                compact.Join();
            }

            Assert.Empty(failures);
        }

        // ============ 3. LocalRef -> stack slot -> stack local (L-stack mode) ============
        // The stack never moves; the slot content stays valid forever.
        // Read + write-through must hold across forced GCs.
        [Fact]
        public void LocalRef_GetRef_StackTarget_ReadWriteThrough()
        {
            long readFail = 0, writeFail = 0;
            for (int i = 0; i < 50; i++)
            {
                int cookie = i;
                int stackVar = cookie;
                RawRef slot = default;
                RawRef tmp = new RawRef();
                tmp.SetRef(ref slot);                    // tmp._Ref = &slot (stack address)
                LocalRef lr = new LocalRef(tmp.Address); // _Ref2Ref points at the stack slot
                lr.SetRef(ref stackVar);                 // slot._Ref = &stackVar

                GC.Collect(0, GCCollectionMode.Forced, true, true);
                GC.Collect(2, GCCollectionMode.Forced, true, true);

                ref int r = ref lr.GetRef<int>();
                if (r != cookie) readFail++;
                r = cookie + 1;
                if (stackVar != cookie + 1) writeFail++;
                r = cookie;
            }
            Assert.True(readFail == 0 && writeFail == 0,
                $"LocalRef stack target broken: read-fail={readFail} write-fail={writeFail}");
        }
    }
}
