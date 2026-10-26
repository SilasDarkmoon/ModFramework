using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for RefInvoker.cs: the RefInvoke family over FreeFunc+ByRefLocalSlot —
    /// the invoker wraps each `in P` argument slot into a GC-tracked ByRefLocalSlot
    /// (bit-compatible with ByRefUtils' LocalRef&lt;P&gt;), so the lambda reads/writes
    /// the caller's variables through LocalRef.Value / GetRef(). Three families are
    /// sampled at n = 1/2/3/8/16 of the 1..16 family (mirroring UnifiedInvokersTests'
    /// sampling style), plus a mixed-P-types case and a GC-compaction tracking case.
    /// </summary>
    public class RefInvokerTests
    {
        // =====================================================================
        // Family 1: void RefInvoke(in p1..in pn, Action&lt;LocalRef&lt;P1&gt;..&gt;)
        // — the lambda writes through every LocalRef; all caller variables change.
        // =====================================================================

        [Fact]
        public void RefInvoke_Void_1Arg_WritesThrough()
        {
            int a1 = 1;
            UnsafeEx.RefInvoke(in a1, (LocalRef<int> r1) => { r1.Value = 10; });
            Assert.Equal(10, a1);
        }

        [Fact]
        public void RefInvoke_Void_2Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2;
            UnsafeEx.RefInvoke(in a1, in a2, (LocalRef<int> r1, LocalRef<int> r2) =>
            {
                r1.Value = 10;
                r2.Value = 20;
            });
            Assert.Equal(10, a1);
            Assert.Equal(20, a2);
        }

        [Fact]
        public void RefInvoke_Void_3Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3;
            UnsafeEx.RefInvoke(in a1, in a2, in a3, (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3) =>
            {
                r1.Value = 10;
                r2.Value = 20;
                r3.Value = 30;
            });
            Assert.Equal(10, a1);
            Assert.Equal(20, a2);
            Assert.Equal(30, a3);
        }

        [Fact]
        public void RefInvoke_Void_8Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) =>
            {
                r1.Value = 10; r2.Value = 20; r3.Value = 30; r4.Value = 40;
                r5.Value = 50; r6.Value = 60; r7.Value = 70; r8.Value = 80;
            });
            Assert.Equal(10, a1); Assert.Equal(20, a2); Assert.Equal(30, a3); Assert.Equal(40, a4);
            Assert.Equal(50, a5); Assert.Equal(60, a6); Assert.Equal(70, a7); Assert.Equal(80, a8);
        }

        [Fact]
        public void RefInvoke_Void_16Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8, in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) =>
            {
                r1.Value = 100; r2.Value = 200; r3.Value = 300; r4.Value = 400;
                r5.Value = 500; r6.Value = 600; r7.Value = 700; r8.Value = 800;
                r9.Value = 900; r10.Value = 1000; r11.Value = 1100; r12.Value = 1200;
                r13.Value = 1300; r14.Value = 1400; r15.Value = 1500; r16.Value = 1600;
            });
            Assert.Equal(100, a1); Assert.Equal(200, a2); Assert.Equal(300, a3); Assert.Equal(400, a4);
            Assert.Equal(500, a5); Assert.Equal(600, a6); Assert.Equal(700, a7); Assert.Equal(800, a8);
            Assert.Equal(900, a9); Assert.Equal(1000, a10); Assert.Equal(1100, a11); Assert.Equal(1200, a12);
            Assert.Equal(1300, a13); Assert.Equal(1400, a14); Assert.Equal(1500, a15); Assert.Equal(1600, a16);
        }

        // =====================================================================
        // Family 2: R RefInvoke(in p1..in pn, Func&lt;LocalRef&lt;P1&gt;.., R&gt;)
        // — the lambda reads every LocalRef and returns a value.
        // =====================================================================

        [Fact]
        public void RefInvoke_R_1Arg_ReadsValue()
        {
            int a = 43;
            var result = UnsafeEx.RefInvoke(in a, (LocalRef<int> r) => r.Value * 2);
            Assert.Equal(86, result);
        }

        [Fact]
        public void RefInvoke_R_2Arg_ReadsValues()
        {
            int a = 20, b = 22;
            var sum = UnsafeEx.RefInvoke(in a, in b, (LocalRef<int> ra, LocalRef<int> rb) => ra.Value + rb.Value);
            Assert.Equal(42, sum);
        }

        [Fact]
        public void RefInvoke_R_3Arg_ReadsValues()
        {
            int a = 10, b = 20, c = 12;
            var sum = UnsafeEx.RefInvoke(in a, in b, in c, (LocalRef<int> ra, LocalRef<int> rb, LocalRef<int> rc) => ra.Value + rb.Value + rc.Value);
            Assert.Equal(42, sum);
        }

        [Fact]
        public void RefInvoke_R_8Arg_ReadsValues()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            var sum = UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) =>
                r1.Value + r2.Value + r3.Value + r4.Value + r5.Value + r6.Value + r7.Value + r8.Value);
            Assert.Equal(36, sum);
        }

        [Fact]
        public void RefInvoke_R_16Arg_ReadsValues()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            var sum = UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8, in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) =>
                r1.Value + r2.Value + r3.Value + r4.Value + r5.Value + r6.Value + r7.Value + r8.Value
                + r9.Value + r10.Value + r11.Value + r12.Value + r13.Value + r14.Value + r15.Value + r16.Value);
            Assert.Equal(136, sum);
        }

        [Fact]
        public void RefInvoke_R_3Arg_MixedParamTypes()
        {
            string name = "hello";
            int extra = 2;
            double factor = 2.5;
            var result = UnsafeEx.RefInvoke(in name, in extra, in factor,
                (LocalRef<string> rn, LocalRef<int> re, LocalRef<double> rf) => rn.Value + ":" + (re.Value + rf.Value));
            Assert.Equal("hello:4.5", result);
        }

        // =====================================================================
        // Family 3: ref R RefInvoke(in p1..in pn, RefFunc&lt;LocalRef&lt;P1&gt;.., R&gt;)
        // — the lambda ref-returns one argument; the caller writes through the
        // returned ref and that variable (and only that one) changes.
        // =====================================================================

        [Fact]
        public void RefInvoke_RefR_1Arg_ReturnsRefToArg()
        {
            int a = 43;
            ref var ra = ref UnsafeEx.RefInvoke(in a, (LocalRef<int> r) => ref r.GetRef());
            ++ra;
            Assert.Equal(44, a);
        }

        [Fact]
        public void RefInvoke_RefR_2Arg_ReturnsRefToSelectedArg()
        {
            int a = 40, b = 2;
            ref var rb = ref UnsafeEx.RefInvoke(in a, in b, (LocalRef<int> ra, LocalRef<int> r2) => ref r2.GetRef());
            rb += 100;
            Assert.Equal(40, a);
            Assert.Equal(102, b);
        }

        [Fact]
        public void RefInvoke_RefR_3Arg_ReturnsRefToSelectedArg()
        {
            int a = 1, b = 2, c = 3;
            ref var rc = ref UnsafeEx.RefInvoke(in a, in b, in c, (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3) => ref r3.GetRef());
            rc = 42;
            Assert.Equal(1, a);
            Assert.Equal(2, b);
            Assert.Equal(42, c);
        }

        [Fact]
        public void RefInvoke_RefR_8Arg_ReturnsRefToSelectedArg()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            ref var sel = ref UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) => ref r5.GetRef());
            sel += 100;
            Assert.Equal(1, a1); Assert.Equal(2, a2); Assert.Equal(3, a3); Assert.Equal(4, a4);
            Assert.Equal(105, a5); Assert.Equal(6, a6); Assert.Equal(7, a7); Assert.Equal(8, a8);
        }

        [Fact]
        public void RefInvoke_RefR_16Arg_ReturnsRefToSelectedArg()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            ref var sel = ref UnsafeEx.RefInvoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8, in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16,
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) => ref r12.GetRef());
            sel += 1000;
            Assert.Equal(1, a1); Assert.Equal(2, a2); Assert.Equal(3, a3); Assert.Equal(4, a4);
            Assert.Equal(5, a5); Assert.Equal(6, a6); Assert.Equal(7, a7); Assert.Equal(8, a8);
            Assert.Equal(9, a9); Assert.Equal(10, a10); Assert.Equal(11, a11); Assert.Equal(1012, a12);
            Assert.Equal(13, a13); Assert.Equal(14, a14); Assert.Equal(15, a15); Assert.Equal(16, a16);
        }

        // =====================================================================
        // GC compaction: the argument is a heap field; the lambda forces compacting
        // GCs before reading — the ByRefLocalSlot wraps a GC-tracked byref slot, so
        // the value read through LocalRef must survive the relocation (the heavy
        // stress lives in PointerDelegateTestSuite; this checks the RefInvoke pipe).
        // =====================================================================

        sealed class CookieHolder { public long Value; }

        [Fact]
        public void RefInvoke_R_HeapField_SurvivesCompactingGc()
        {
            for (int round = 0; round < 64; round++)
            {
                var holder = new CookieHolder { Value = 0x1234000000L + round };
                for (int g = 0; g < 32; g++) { var junk = new byte[(round * 7 + g) % 128 + 1]; junk[0] = 1; }

                long got = UnsafeEx.RefInvoke(in holder.Value, (LocalRef<long> rv) =>
                {
                    // compacting GCs while the ByRefLocalSlot is live
                    GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.Collect(1, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    return rv.Value;
                });

                Assert.Equal(0x1234000000L + round, got);
            }
        }
    }
}
