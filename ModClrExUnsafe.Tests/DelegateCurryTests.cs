using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for DelegateCurry.cs. The Curry family is template-generated 1..16 times
    /// over three delegate kinds (Func / Action / UnifiedRefFunc); these tests sample
    /// the endpoints (1, 16) and the middle (2, 8) of each family, plus the PostInvoke
    /// hook that fires after every curried invocation with the fixed target as payload.
    /// </summary>
    public class DelegateCurryTests
    {
        // =====================================================================
        // Func family
        // =====================================================================

        [Fact]
        public void Curry_Func_1Arg_FixatesFirstArgument()
        {
            Func<int, int, int> add = (a, b) => a + b;

            var curried = add.Curry<int, int, int>(10);      // Func<int, int>

            Assert.Equal(15, curried(5));
            Assert.Equal(20, curried(10));
        }

        [Fact]
        public void Curry_Func_2Arg_LeavesTwoParameters()
        {
            Func<int, int, int, int> sum3 = (a, b, c) => a + b + c;

            var curried = sum3.Curry<int, int, int, int>(100);   // Func<int, int, int>

            Assert.Equal(106, curried(1, 5));
        }

        [Fact]
        public void Curry_Func_8Arg()
        {
            Func<int, int, int, int, int, int, int, int, int, int> sum9 =
                (a, b, c, d, e, f, g, h, i) => a + b + c + d + e + f + g + h + i;

            var curried = sum9.Curry<int, int, int, int, int, int, int, int, int, int>(1);   // 8 params left

            Assert.Equal(45, curried(2, 3, 4, 5, 6, 7, 8, 9));   // 1 + (2..9) = 45
        }

        [Fact]
        public void Curry_Func_16Arg()
        {
            // Func is capped at 16 inputs (17 type args): the 16-slot Curry takes a
            // 16-input Func and leaves 15 parameters.
            Func<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> sum16 =
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =>
                    a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;

            var curried = sum16.Curry<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>(1);

            Assert.Equal(136, curried(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));   // 1 + 135
        }

        // =====================================================================
        // Action family
        // =====================================================================

        [Fact]
        public void Curry_Action_1Arg_FixatesOnlyArgument()
        {
            int received = 0;
            Action<int> record = v => received = v;

            Action curried = record.Curry(42);

            curried();
            Assert.Equal(42, received);
        }

        [Fact]
        public void Curry_Action_2Arg()
        {
            int sum = 0;
            Action<int, int, int> add = (a, b, c) => sum = a + b + c;

            var curried = add.Curry<int, int, int>(10);      // Action<int, int>

            curried(2, 3);
            Assert.Equal(15, sum);
        }

        [Fact]
        public void Curry_Action_8Arg()
        {
            int sum = 0;
            Action<int, int, int, int, int, int, int, int, int> add9 =
                (a, b, c, d, e, f, g, h, i) => sum = a + b + c + d + e + f + g + h + i;

            var curried = add9.Curry<int, int, int, int, int, int, int, int, int>(1);

            curried(2, 3, 4, 5, 6, 7, 8, 9);
            Assert.Equal(45, sum);
        }

        [Fact]
        public void Curry_Action_16Arg()
        {
            long sum = 0;
            Action<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> add16 =
                (a, b, c, d, e, f, g, h, i, j, k, l, m, n, o, p) =>
                    sum = (long)a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;

            var curried = add16.Curry<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>(1);

            curried(2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            Assert.Equal(136L, sum);                       // 1 + (2..16)
        }

        // =====================================================================
        // UnifiedRefFunc family
        // =====================================================================

        // UnifiedRefFunc returns 'ref R'. Since ByRefUtils.dll now carries
        // [module: RefSafetyRules(11)] (injected by ByRefUtilsGenerator), the
        // Ref.IgnoreOut bridge is consumable from C# latest: `return ref Ref.IgnoreOut(out r)`
        // compiles and the returned ref aliases the out slot itself (true
        // write-through), replacing the former static-slot workaround.

        private static ref int TwiceImpl(out int r, in int p1) { r = p1 * 2; return ref Ref.IgnoreOut(out r); }

        [Fact]
        public void Curry_UnifiedRefFunc_WriteThrough()
        {
            // locks the true write-through semantics of the whole chain:
            // handler (Ref.IgnoreOut bridge) -> CurryWrapper.UnifiedInvoke (out
            // passthrough) -> caller's out slot. Writing the returned ref must
            // reach the caller's out variable.
            UnifiedRefFunc<int, int> twice = TwiceImpl;

            var curried = twice.Curry<int, int>(21);

            ref int rr = ref curried(out int result);
            Assert.Equal(42, result);
            rr = 99;
            Assert.Equal(99, result);            // write-through reaches the out slot
        }

        [Fact]
        public void Curry_UnifiedRefFunc_1Arg()
        {
            UnifiedRefFunc<int, int> twice = TwiceImpl;

            var curried = twice.Curry<int, int>(21);        // UnifiedRefFunc<int>

            curried(out int result);
            Assert.Equal(42, result);
        }

        private static ref int WeightedImpl(out int r, in int a, in int b) { r = a * 100 + b; return ref Ref.IgnoreOut(out r); }

        [Fact]
        public void Curry_UnifiedRefFunc_2Arg()
        {
            UnifiedRefFunc<int, int, int> weighted = WeightedImpl;

            var curried = weighted.Curry<int, int, int>(2);    // UnifiedRefFunc<int, int>

            curried(out int result, 7);
            Assert.Equal(207, result);
        }

        private static ref int Sum8Impl(out int r, in int a, in int b, in int c, in int d,
            in int e, in int f, in int g, in int h)
        {
            r = a + b + c + d + e + f + g + h;
            return ref Ref.IgnoreOut(out r);
        }

        [Fact]
        public void Curry_UnifiedRefFunc_8Arg()
        {
            UnifiedRefFunc<int, int, int, int, int, int, int, int, int> sum8 = Sum8Impl;

            var curried = sum8.Curry<int, int, int, int, int, int, int, int, int>(1);

            curried(out int result, 2, 3, 4, 5, 6, 7, 8);
            Assert.Equal(36, result);                      // 1 + (2..8)
        }

        private static ref int Sum16Impl(out int r, in int a, in int b, in int c, in int d,
            in int e, in int f, in int g, in int h, in int i, in int j, in int k, in int l,
            in int m, in int n, in int o, in int p)
        {
            r = a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;
            return ref Ref.IgnoreOut(out r);
        }

        [Fact]
        public void Curry_UnifiedRefFunc_16Arg()
        {
            UnifiedRefFunc<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> sum16 = Sum16Impl;

            var curried = sum16.Curry<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>(1);

            curried(out int result, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16);
            Assert.Equal(136, result);                     // 1 + (2..16)
        }

        // =====================================================================
        // PostInvoke hook
        // =====================================================================

        [Fact]
        public void PostInvoke_FiresAfterCurriedCall_WithFixedTarget()
        {
            Func<int, int, int> add = (a, b) => a + b;
            var curried = add.Curry<int, int, int>(7);

            object seenPayload = "<none>";
            curried.PostInvoke(payload => seenPayload = payload);

            curried(1);

            Assert.Equal(7, seenPayload);                  // the callback receives the fixed target
        }

        [Fact]
        public void PostInvoke_OnNonCurriedDelegate_Throws()
        {
            Func<int, int, int> plain = (a, b) => a + b;

            Assert.Throws<ArgumentException>(() => plain.PostInvoke(_ => { }));
        }

        // =====================================================================
        // ChangeDelegateType + implicit multi-step currying
        // (migrated from TestScript_DelegateCurry.cs Pass2/Pass3)
        // =====================================================================

        private struct CurryFields
        {
            public int F0, F1, F2, F3, F4, F5;
            public CurryFields(int f0, int f1, int f2, int f3, int f4, int f5)
            {
                F0 = f0; F1 = f1; F2 = f2; F3 = f3; F4 = f4; F5 = f5;
            }
        }
        private delegate CurryFields TestCurryDel(int p1, int p2, int p3, int p4, int p5, int p6);
        private delegate CurryFields TestFinalCurriedDel(int p6);

        [Fact]
        public void Curry_ChangeDelegateType_MidChain()
        {
            // custom 6-arg delegate -> Func (ChangeDelegateType) -> curry 5 steps
            // -> custom 1-arg delegate (ChangeDelegateType again)
            TestCurryDel odel = (f0, f1, f2, f3, f4, f5) => new CurryFields(f0, f1, f2, f3, f4, f5);
            var func = odel.ChangeDelegateType<Func<int, int, int, int, int, int, CurryFields>>()
                .Curry(30).Curry(40).Curry(50).Curry(60).Curry(70)
                .ChangeDelegateType<TestFinalCurriedDel>();

            var inst = func(80);
            Assert.Equal((30, 40, 50, 60, 70, 80), (inst.F0, inst.F1, inst.F2, inst.F3, inst.F4, inst.F5));
        }

        [Fact]
        public void Curry_ImplicitMultiStep()
        {
            // implicit currying: Curry on the custom delegate directly (no
            // ChangeDelegateType needed for the steps), typed back at the end
            TestCurryDel odel = (f0, f1, f2, f3, f4, f5) => new CurryFields(f0, f1, f2, f3, f4, f5);
            var func = odel.Curry(9).Curry(8).Curry(7).Curry(6).Curry(5)
                .ChangeDelegateType<TestFinalCurriedDel>();

            var inst = func(4);
            Assert.Equal((9, 8, 7, 6, 5, 4), (inst.F0, inst.F1, inst.F2, inst.F3, inst.F4, inst.F5));
        }
    }
}
