using Mod.LowLevel;
using System;
using System.Reflection;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for UnifiedInvokers.cs: the Invoke extension family (UnifiedRefFunc ->
    /// value), AsUnifiedFunc (ref-func -> value-func), CreateDelegateUnified
    /// (MethodBase -> UnifiedRefFunc via PointerFunc.Invoke method group) and
    /// CreateFreeInvokable (MethodBase -> PointerFunc). UnifiedRefFunc handlers are
    /// written with the Ref.IgnoreOut bridge - ByRefUtils.dll now carries
    /// [module: RefSafetyRules(11)], so `return ref Ref.IgnoreOut(out r)` compiles
    /// from C# latest and the returned ref aliases the out slot (true write-through).
    /// </summary>
    public class UnifiedInvokersTests
    {
        // =====================================================================
        // Invoke extension family (samples 0/1/2/8/16 of the 1..16 family)
        // =====================================================================

        [Fact]
        public void Invoke_0Arg_ReturnsValue()
        {
            UnifiedRefFunc<int> f = (out int r) => { r = 42; return ref Ref.IgnoreOut(out r); };

            Assert.Equal(42, f.Invoke());
        }

        [Fact]
        public void Invoke_1Arg_ReturnsValue()
        {
            UnifiedRefFunc<int, int> twice = (out int r, in int p1) => { r = p1 * 2; return ref Ref.IgnoreOut(out r); };

            Assert.Equal(42, twice.Invoke(21));
            Assert.Equal(10, twice.Invoke(5));
        }

        [Fact]
        public void Invoke_2Arg_ReturnsValue()
        {
            UnifiedRefFunc<int, int, int> add = (out int r, in int a, in int b) => { r = a + b; return ref Ref.IgnoreOut(out r); };

            Assert.Equal(42, add.Invoke(20, 22));
        }

        [Fact]
        public void Invoke_8Arg_ReturnsValue()
        {
            UnifiedRefFunc<int, int, int, int, int, int, int, int, int> sum8 =
                (out int r, in int a, in int b, in int c, in int d, in int e, in int f, in int g, in int h) =>
                {
                    r = a + b + c + d + e + f + g + h;
                    return ref Ref.IgnoreOut(out r);
                };

            Assert.Equal(36, sum8.Invoke(1, 2, 3, 4, 5, 6, 7, 8));
        }

        [Fact]
        public void Invoke_16Arg_ReturnsValue()
        {
            UnifiedRefFunc<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int> sum16 =
                (out int r, in int a, in int b, in int c, in int d, in int e, in int f, in int g,
                 in int h, in int i, in int j, in int k, in int l, in int m, in int n, in int o, in int p) =>
                {
                    r = a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;
                    return ref Ref.IgnoreOut(out r);
                };

            Assert.Equal(136, sum16.Invoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
        }

        [Fact]
        public void Invoke_Handler_IsTrueWriteThrough()
        {
            // the handler's returned ref must alias the out slot itself
            UnifiedRefFunc<int, int> twice = (out int r, in int p1) => { r = p1 * 2; return ref Ref.IgnoreOut(out r); };

            ref int rr = ref twice(out int result, 20);
            Assert.Equal(40, result);
            rr = 99;
            Assert.Equal(99, result);            // writing the ref reaches the out slot
        }

        // =====================================================================
        // AsUnifiedFunc (ref-func -> value-func)
        // =====================================================================

        [Fact]
        public void AsUnifiedFunc_1Arg()
        {
            UnifiedRefFunc<int, int> twice = (out int r, in int p1) => { r = p1 * 2; return ref Ref.IgnoreOut(out r); };

            UnifiedFunc<int, int> f = twice.AsUnifiedFunc();

            Assert.Equal(42, f(21));
        }

        [Fact]
        public void AsUnifiedFunc_2Arg()
        {
            UnifiedRefFunc<int, int, int> add = (out int r, in int a, in int b) => { r = a + b; return ref Ref.IgnoreOut(out r); };

            UnifiedFunc<int, int, int> f = add.AsUnifiedFunc();

            Assert.Equal(42, f(20, 22));
        }

        // =====================================================================
        // CreateFreeInvokable + CreateDelegateUnified (MethodBase -> invoker)
        // =====================================================================

        private static int Answer() => 42;
        private static int AddOne(int x) => x + 1;
        private static int AddTwo(int a, int b) => a + b;

        private static MethodInfo M(string name) => typeof(UnifiedInvokersTests).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);

        [Fact]
        public void CreateFreeInvokable_1Arg_ValueInvoke()
        {
            var pf = M(nameof(AddOne)).CreateFreeInvokable<int, int>();

            Assert.Equal(42, pf.Invoke(41));      // non-generic value-returning Invoke
        }

        [Fact]
        public void CreateFreeInvokable_1Arg_GenericRefInvoke()
        {
            var pf = M(nameof(AddOne)).CreateFreeInvokable<int, int>();

            ref int rr = ref pf.Invoke(out int r, 41);   // generic out/ref Invoke
            Assert.Equal(42, r);
            Assert.Equal(42, rr);
        }

        [Fact]
        public void CreateDelegateUnified_0Arg()
        {
            UnifiedRefFunc<int> u = M(nameof(Answer)).CreateDelegateUnified<int>();

            u(out int result);
            Assert.Equal(42, result);
        }

        [Fact]
        public void CreateDelegateUnified_1Arg()
        {
            UnifiedRefFunc<int, int> u = M(nameof(AddOne)).CreateDelegateUnified<int, int, int>();

            Assert.Equal(42, u.Invoke(41));
        }

        [Fact]
        public void CreateDelegateUnified_2Arg()
        {
            UnifiedRefFunc<int, int, int> u = M(nameof(AddTwo)).CreateDelegateUnified<int, int, int, int, int>();

            Assert.Equal(42, u.Invoke(20, 22));
        }

        [Fact]
        public void CreateDelegateUnified_WriteThrough()
        {
            // the chain MethodBase -> PointerFunc -> Invoke method group -> UnifiedRefFunc
            // must preserve write-through: the returned ref aliases the out slot.
            UnifiedRefFunc<int, int> u = M(nameof(AddOne)).CreateDelegateUnified<int, int, int>();

            ref int rr = ref u(out int result, 41);
            Assert.Equal(42, result);
            rr = 99;
            Assert.Equal(99, result);
        }
    }
}
