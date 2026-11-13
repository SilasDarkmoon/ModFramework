using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for RefInvoker.cs: the RefInvoker extension family bridging
    /// Action/Func/RefFunc&lt;LocalRef&lt;P&gt;..&gt; delegates into the Generator-woven
    /// ActionRefInvoker/FuncRefInvoker/RefFuncRefInvoker classes (PointerDelegate).
    /// LocalRef&lt;P&gt; is bit-compatible with ByRefLocalSlot, so the delegate is
    /// reinterpreted via ChangeDelegateType and the woven invoker wraps the
    /// caller's argument slots (GC-tracked byref, no address snapshot) — the
    /// lambda reads/writes the caller's variables through LocalRef.Value/GetRef().
    /// Families are sampled at n = 1/2/3/8/16 of the 1..16 family, plus the
    /// EX=ValueTuple packing pattern (the designed EX usage: the non-ref
    /// parameters are packed into a ValueTuple passed as the single EX argument),
    /// an EX boundary-arity case, and a GC-compaction tracking case.
    /// RefFunc ref-returns go through the IFreeInvokableFuncN&lt;R&gt; interface —
    /// the public wrapper returns R by value.
    /// </summary>
    public class RefInvokerTests
    {
        // RefFunc ref-return path: the public wrapper returns R by value; the
        // ref-return passthrough lives on the IFreeInvokableFuncN<R> interface.
        static ref int RefInvokeRef(RefFuncRefInvoker1<int> inv, in int a)
            => ref ((IFreeInvokableFunc1<int>)inv).Invoke(out _, in a);
        static ref int RefInvokeRef(RefFuncRefInvoker2<int> inv, in int a, in int b)
            => ref ((IFreeInvokableFunc2<int>)inv).Invoke(out _, in a, in b);
        static ref int RefInvokeRef(RefFuncRefInvoker3<int> inv, in int a, in int b, in int c)
            => ref ((IFreeInvokableFunc3<int>)inv).Invoke(out _, in a, in b, in c);
        static ref int RefInvokeRef(RefFuncRefInvoker8<int> inv,
            in int a1, in int a2, in int a3, in int a4,
            in int a5, in int a6, in int a7, in int a8)
            => ref ((IFreeInvokableFunc8<int>)inv).Invoke(out _,
                in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8);
        static ref int RefInvokeRef(RefFuncRefInvoker16<int> inv,
            in int a1, in int a2, in int a3, in int a4, in int a5, in int a6, in int a7, in int a8,
            in int a9, in int a10, in int a11, in int a12, in int a13, in int a14, in int a15, in int a16)
            => ref ((IFreeInvokableFunc16<int>)inv).Invoke(out _,
                in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16);

        // =====================================================================
        // Family 1: Action — the lambda writes through every LocalRef; all
        // caller variables change.
        // =====================================================================

        [Fact]
        public void RefInvoke_Void_1Arg_WritesThrough()
        {
            int a1 = 1;
            Action<LocalRef<int>> act = (LocalRef<int> r1) => { r1.Value = 10; };
            act.RefInvoker().Invoke(in a1);
            Assert.Equal(10, a1);
        }

        [Fact]
        public void RefInvoke_Void_2Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2;
            Action<LocalRef<int>, LocalRef<int>> act = (LocalRef<int> r1, LocalRef<int> r2) =>
            {
                r1.Value = 10;
                r2.Value = 20;
            };
            act.RefInvoker().Invoke(in a1, in a2);
            Assert.Equal(10, a1);
            Assert.Equal(20, a2);
        }

        [Fact]
        public void RefInvoke_Void_3Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3;
            Action<LocalRef<int>, LocalRef<int>, LocalRef<int>> act =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3) =>
            {
                r1.Value = 10;
                r2.Value = 20;
                r3.Value = 30;
            };
            act.RefInvoker().Invoke(in a1, in a2, in a3);
            Assert.Equal(10, a1);
            Assert.Equal(20, a2);
            Assert.Equal(30, a3);
        }

        [Fact]
        public void RefInvoke_Void_8Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            Action<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>> act =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) =>
            {
                r1.Value = 10; r2.Value = 20; r3.Value = 30; r4.Value = 40;
                r5.Value = 50; r6.Value = 60; r7.Value = 70; r8.Value = 80;
            };
            act.RefInvoker().Invoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8);
            Assert.Equal(10, a1); Assert.Equal(20, a2); Assert.Equal(30, a3); Assert.Equal(40, a4);
            Assert.Equal(50, a5); Assert.Equal(60, a6); Assert.Equal(70, a7); Assert.Equal(80, a8);
        }

        [Fact]
        public void RefInvoke_Void_16Arg_WritesThrough()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            Action<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>> act =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) =>
            {
                r1.Value = 100; r2.Value = 200; r3.Value = 300; r4.Value = 400;
                r5.Value = 500; r6.Value = 600; r7.Value = 700; r8.Value = 800;
                r9.Value = 900; r10.Value = 1000; r11.Value = 1100; r12.Value = 1200;
                r13.Value = 1300; r14.Value = 1400; r15.Value = 1500; r16.Value = 1600;
            };
            act.RefInvoker().Invoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16);
            Assert.Equal(100, a1); Assert.Equal(200, a2); Assert.Equal(300, a3); Assert.Equal(400, a4);
            Assert.Equal(500, a5); Assert.Equal(600, a6); Assert.Equal(700, a7); Assert.Equal(800, a8);
            Assert.Equal(900, a9); Assert.Equal(1000, a10); Assert.Equal(1100, a11); Assert.Equal(1200, a12);
            Assert.Equal(1300, a13); Assert.Equal(1400, a14); Assert.Equal(1500, a15); Assert.Equal(1600, a16);
        }

        // =====================================================================
        // Family 2: Func — the lambda reads every LocalRef and returns a value.
        // =====================================================================

        [Fact]
        public void RefInvoke_R_1Arg_ReadsValue()
        {
            int a = 43;
            Func<LocalRef<int>, int> f = (LocalRef<int> r) => r.Value * 2;
            var result = f.RefInvoker().Invoke(in a);
            Assert.Equal(86, result);
        }

        [Fact]
        public void RefInvoke_R_2Arg_ReadsValues()
        {
            int a = 20, b = 22;
            Func<LocalRef<int>, LocalRef<int>, int> f = (LocalRef<int> ra, LocalRef<int> rb) => ra.Value + rb.Value;
            var sum = f.RefInvoker().Invoke(in a, in b);
            Assert.Equal(42, sum);
        }

        [Fact]
        public void RefInvoke_R_3Arg_ReadsValues()
        {
            int a = 10, b = 20, c = 12;
            Func<LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> ra, LocalRef<int> rb, LocalRef<int> rc) => ra.Value + rb.Value + rc.Value;
            var sum = f.RefInvoker().Invoke(in a, in b, in c);
            Assert.Equal(42, sum);
        }

        [Fact]
        public void RefInvoke_R_8Arg_ReadsValues()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            Func<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) =>
                r1.Value + r2.Value + r3.Value + r4.Value + r5.Value + r6.Value + r7.Value + r8.Value;
            var sum = f.RefInvoker().Invoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8);
            Assert.Equal(36, sum);
        }

        [Fact]
        public void RefInvoke_R_16Arg_ReadsValues()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            Func<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) =>
                r1.Value + r2.Value + r3.Value + r4.Value + r5.Value + r6.Value + r7.Value + r8.Value
                + r9.Value + r10.Value + r11.Value + r12.Value + r13.Value + r14.Value + r15.Value + r16.Value;
            var sum = f.RefInvoker().Invoke(in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16);
            Assert.Equal(136, sum);
        }

        [Fact]
        public void RefInvoke_R_3Arg_MixedParamTypes()
        {
            string name = "hello";
            int extra = 2;
            double factor = 2.5;
            Func<LocalRef<string>, LocalRef<int>, LocalRef<double>, string> f =
                (LocalRef<string> rn, LocalRef<int> re, LocalRef<double> rf) => rn.Value + ":" + (re.Value + rf.Value);
            var result = f.RefInvoker().Invoke(in name, in extra, in factor);
            Assert.Equal("hello:4.5", result);
        }

        // =====================================================================
        // Family 3: RefFunc — the lambda ref-returns one argument; the caller
        // writes through the returned ref (via the IFreeInvokableFuncN<R>
        // interface) and that variable (and only that one) changes.
        // =====================================================================

        [Fact]
        public void RefInvoke_RefR_1Arg_ReturnsRefToArg()
        {
            int a = 43;
            RefFunc<LocalRef<int>, int> f = (LocalRef<int> r) => ref r.GetRef();
            ref var ra = ref RefInvokeRef(f.RefInvoker(), in a);
            ++ra;
            Assert.Equal(44, a);
        }

        [Fact]
        public void RefInvoke_RefR_2Arg_ReturnsRefToSelectedArg()
        {
            int a = 40, b = 2;
            RefFunc<LocalRef<int>, LocalRef<int>, int> f = (LocalRef<int> ra, LocalRef<int> r2) => ref r2.GetRef();
            ref var rb = ref RefInvokeRef(f.RefInvoker(), in a, in b);
            rb += 100;
            Assert.Equal(40, a);
            Assert.Equal(102, b);
        }

        [Fact]
        public void RefInvoke_RefR_3Arg_ReturnsRefToSelectedArg()
        {
            int a = 1, b = 2, c = 3;
            RefFunc<LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3) => ref r3.GetRef();
            ref var rc = ref RefInvokeRef(f.RefInvoker(), in a, in b, in c);
            rc = 42;
            Assert.Equal(1, a);
            Assert.Equal(2, b);
            Assert.Equal(42, c);
        }

        [Fact]
        public void RefInvoke_RefR_8Arg_ReturnsRefToSelectedArg()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8;
            RefFunc<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4,
                 LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8) => ref r5.GetRef();
            ref var sel = ref RefInvokeRef(f.RefInvoker(), in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8);
            sel += 100;
            Assert.Equal(1, a1); Assert.Equal(2, a2); Assert.Equal(3, a3); Assert.Equal(4, a4);
            Assert.Equal(105, a5); Assert.Equal(6, a6); Assert.Equal(7, a7); Assert.Equal(8, a8);
        }

        [Fact]
        public void RefInvoke_RefR_16Arg_ReturnsRefToSelectedArg()
        {
            int a1 = 1, a2 = 2, a3 = 3, a4 = 4, a5 = 5, a6 = 6, a7 = 7, a8 = 8, a9 = 9, a10 = 10, a11 = 11, a12 = 12, a13 = 13, a14 = 14, a15 = 15, a16 = 16;
            RefFunc<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>, int> f =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5, LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8,
                 LocalRef<int> r9, LocalRef<int> r10, LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15, LocalRef<int> r16) => ref r12.GetRef();
            ref var sel = ref RefInvokeRef(f.RefInvoker(),
                in a1, in a2, in a3, in a4, in a5, in a6, in a7, in a8,
                in a9, in a10, in a11, in a12, in a13, in a14, in a15, in a16);
            sel += 1000;
            Assert.Equal(1, a1); Assert.Equal(2, a2); Assert.Equal(3, a3); Assert.Equal(4, a4);
            Assert.Equal(5, a5); Assert.Equal(6, a6); Assert.Equal(7, a7); Assert.Equal(8, a8);
            Assert.Equal(9, a9); Assert.Equal(10, a10); Assert.Equal(11, a11); Assert.Equal(1012, a12);
            Assert.Equal(13, a13); Assert.Equal(14, a14); Assert.Equal(15, a15); Assert.Equal(16, a16);
        }

        // =====================================================================
        // EX = ValueTuple — the designed EX usage: the non-ref parameters are
        // packed into a ValueTuple passed as the single EX argument (e.g. a
        // native int Foo(int a, float b, ref int result) becomes
        // Func<LocalRef<int>, (int a, float b), int>).
        // =====================================================================

        [Fact]
        public void RefInvoke_EX_ValueTuple_Func_PacksNonRefParams()
        {
            int result = 0;
            Func<LocalRef<int>, (int a, float b), int> f =
                (LocalRef<int> r, (int a, float b) ex) =>
                {
                    r.Value = ex.a + (int)(ex.b * 2);
                    return ex.a * 100;
                };
            int ret = f.RefInvoker().Invoke(in result, (3, 2.5f));
            Assert.Equal(8, result);
            Assert.Equal(300, ret);
        }

        [Fact]
        public void RefInvoke_EX_ValueTuple_Action_MultiSlot()
        {
            int r1 = 0, r2 = 0;
            Action<LocalRef<int>, LocalRef<int>, (int mul, string tag)> act =
                (LocalRef<int> a, LocalRef<int> b, (int mul, string tag) ex) =>
                {
                    a.Value = ex.mul;
                    b.Value = ex.tag.Length;
                };
            act.RefInvoker().Invoke(in r1, in r2, (7, "hey"));
            Assert.Equal(7, r1);
            Assert.Equal(3, r2);
        }

        [Fact]
        public void RefInvoke_EX_ValueTuple_RefFunc_ReturnsRefToArg()
        {
            int target = 5;
            RefFunc<LocalRef<int>, (int add, int mul), int> f =
                (LocalRef<int> r, (int add, int mul) ex) => ref r.GetRef();
            var inv = f.RefInvoker();
            ref var rr = ref ((IFreeInvokableFunc2<int>)inv).Invoke(out _, in target, (10, 3));
            rr *= 100;
            Assert.Equal(500, target);
        }

        [Fact]
        public void RefInvoke_EX_15Slots_BoundaryArity_WritesThrough()
        {
            var t = new int[15];
            Action<LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, LocalRef<int>,
                LocalRef<int>, LocalRef<int>, LocalRef<int>, (int mul, string tag)> act =
                (LocalRef<int> r1, LocalRef<int> r2, LocalRef<int> r3, LocalRef<int> r4, LocalRef<int> r5,
                 LocalRef<int> r6, LocalRef<int> r7, LocalRef<int> r8, LocalRef<int> r9, LocalRef<int> r10,
                 LocalRef<int> r11, LocalRef<int> r12, LocalRef<int> r13, LocalRef<int> r14, LocalRef<int> r15,
                 (int mul, string tag) ex) =>
            {
                r1.Value = ex.mul;
                r15.Value = ex.tag.Length;
            };
            act.RefInvoker().Invoke(in t[0], in t[1], in t[2], in t[3], in t[4], in t[5], in t[6], in t[7],
                in t[8], in t[9], in t[10], in t[11], in t[12], in t[13], in t[14], (7, "boundary"));
            Assert.Equal(7, t[0]);
            Assert.Equal(8, t[14]);
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

                Func<LocalRef<long>, long> f = (LocalRef<long> rv) =>
                {
                    // compacting GCs while the ByRefLocalSlot is live
                    GC.Collect(0, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.Collect(1, GCCollectionMode.Forced, blocking: true, compacting: true);
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    return rv.Value;
                };
                long got = f.RefInvoker().Invoke(in holder.Value);

                Assert.Equal(0x1234000000L + round, got);
            }
        }
    }
}
