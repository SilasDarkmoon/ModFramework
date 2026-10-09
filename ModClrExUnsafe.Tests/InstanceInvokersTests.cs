using Mod.LowLevel;
using System;
using System.Reflection;
using Xunit;
using SpanStub = Mod.LowLevel.SpanStubModern;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for the Invoker family (UnifiedInvokers.cs): CreateInvoker (static /
    /// open-instance / value-type-this) and CreateInstanceInvoker (bound target).
    /// Migrated from ModFramework's TestScript.cs "Test For Invokers" region, which
    /// used to be a manual Main-run - here every printed value became an assertion.
    ///
    /// The U/P convention: U are the target method's parameter types (ByRefParam
    /// marks "address of the caller's variable"), P are the UnifiedRefFunc `in`
    /// parameter types (the carrier for that address).
    /// </summary>
    public class InstanceInvokersTests
    {
        // =====================================================================
        // fixtures (from TestScript.cs)
        // =====================================================================

        private static long TestMethod1(IntPtr p) => p.ToInt64() + 1;

        private static ref int TestMethod2(ref int r)
        {
            ++r;
            return ref r;
        }

        private static void TestMethod3(ref int r)
        {
            ++r;
        }

        private static ReadOnlySpan<char> TestSpan1(string str)
        {
            ReadOnlySpan<char> rv = str.AsSpan();
            return rv;
        }

        private static string TestSpan2(ReadOnlySpan<char> span)
        {
            return new string(span.ToArray());
        }

        public class TestBaseClass
        {
            public virtual long TestMethod(IntPtr p) => p.ToInt64() + 1;
        }
        public class TestChildClass : TestBaseClass
        {
            public override long TestMethod(IntPtr p) => p.ToInt64() - 1;
        }

        private class TestFieldsClass
        {
            public int Field0, Field1, Field2, Field3, Field4, Field5;
            public TestFieldsClass() { }
            public TestFieldsClass(int i0, int i1, int i2, int i3, int i4, int i5)
            {
                Field0 = i0; Field1 = i1; Field2 = i2; Field3 = i3; Field4 = i4; Field5 = i5;
            }
        }
        private struct TestFieldsStruct
        {
            public int Field0, Field1, Field2, Field3, Field4, Field5;
            public TestFieldsStruct(int i0, int i1, int i2, int i3, int i4, int i5)
            {
                Field0 = i0; Field1 = i1; Field2 = i2; Field3 = i3; Field4 = i4; Field5 = i5;
            }

            public void TestValueTypeInstanceMethod(int v)
            {
                Field0 = v; Field1 = v + 1; Field2 = v + 2;
                Field3 = v + 3; Field4 = v + 4; Field5 = v + 5;
            }
        }
        private class TestFieldsClass<T>
        {
            public T Field0, Field1, Field2, Field3, Field4, Field5;
            public TestFieldsClass() { }
            public TestFieldsClass(T i0, T i1, T i2, T i3, T i4, T i5)
            {
                Field0 = i0; Field1 = i1; Field2 = i2; Field3 = i3; Field4 = i4; Field5 = i5;
            }

            public long TestMethod(ref T r)
            {
                var rr = RawRef.Of(ref r);
                return (long)(rr.Address + 1);
            }
        }

        // =====================================================================
        // static invoker
        // =====================================================================

        [Fact]
        public void StaticInvoker_IntPtrArg()
        {
            var invoker = typeof(InstanceInvokersTests).CreateInvoker<long, IntPtr, IntPtr>(nameof(TestMethod1));
            int a = 0;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, invoker.Invoke(r.Address));
        }

        // =====================================================================
        // instance invokers (target bound in)
        // =====================================================================

        [Fact]
        public void InstanceInvoker_BoundTarget_VirtualDispatch()
        {
            // the target is baked in; the override must be dispatched
            TestChildClass inst = new TestChildClass();
            var invoker = inst.CreateInstanceInvoker<long, object, ByRefParam, object, int>(nameof(inst.TestMethod));
            int a = 12;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address - 1, invoker.Invoke(in a));    // Child: p - 1
        }

        [Fact]
        public void GenericInstanceInvoker_BoundTarget()
        {
            TestFieldsClass<Guid> inst = new TestFieldsClass<Guid>();
            var invoker = inst.CreateInstanceInvoker<long, object, ByRefParam, object, int>(nameof(inst.TestMethod));
            int a = 13;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, invoker.Invoke(in a));    // TestMethod: addr + 1
        }

        // =====================================================================
        // open instance invokers (this passed as an argument)
        // =====================================================================

        [Fact]
        public void OpenInstanceInvoker_ReferenceThis()
        {
            var invoker = typeof(TestBaseClass).CreateInvoker<long, object, ByRefParam, object, int>(nameof(TestBaseClass.TestMethod));
            int a = 12;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, invoker.Invoke(new TestBaseClass(), in a));   // Base: p + 1
        }

        [Fact]
        public void OpenInstanceInvoker_ValueTypeThis_WritesThrough()
        {
            // U1=ByRefParam: the struct `this` is passed by address, so the method
            // writes the caller's struct (write-through, not a copy)
            var inst = new TestFieldsStruct(9, 8, 7, 6, 5, 4);
            var invoker = typeof(TestFieldsStruct).CreateInvoker<VoidReturn, ByRefParam, int, TestFieldsStruct, int>(nameof(TestFieldsStruct.TestValueTypeInstanceMethod));
            invoker.Invoke(in inst, 100);
            Assert.Equal(100, inst.Field0);
            Assert.Equal(101, inst.Field1);
            Assert.Equal(105, inst.Field5);
        }

        [Fact]
        public void GenericOpenInstanceInvoker()
        {
            var invoker = typeof(TestFieldsClass<Guid>).CreateInvoker<long, object, ByRefParam, object, int>(nameof(TestFieldsClass<Guid>.TestMethod));
            int a = 13;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, invoker.Invoke(new TestFieldsClass<Guid>(), in a));
        }

        // =====================================================================
        // return-shape markers
        // =====================================================================

        [Fact]
        public void RefReturnInvoker_ByRefParam_ToRef()
        {
            var invoker = typeof(InstanceInvokersTests).CreateInvoker<ByRefParam, ByRefParam, int>(nameof(TestMethod2));
            int a = 0;
            Assert.Equal(1, invoker(out _, in a).ToRef<int>());   // ++r, returned ref aliases r
            Assert.Equal(1, a);
        }

        [Fact]
        public void VoidReturnInvoker_VoidReturn()
        {
            var invoker = typeof(InstanceInvokersTests).CreateInvoker<VoidReturn, ByRefParam, int>(nameof(TestMethod3));
            int a = 0;
            invoker.Invoke(in a);
            Assert.Equal(1, a);
        }

        // =====================================================================
        // Span marshaling through SpanStub
        // =====================================================================

        [Fact]
        public void SpanReturnInvoker()
        {
            var invoker = typeof(InstanceInvokersTests).CreateInvoker<SpanStub, string, string>(nameof(TestSpan1));
            string str = "Hello, World!";
            var span = invoker.Invoke(str).ToReadOnlySpan<char>();
            Assert.Equal("Hello, World!", new string(span.ToArray()));
        }

        [Fact]
        public void SpanArgumentInvoker()
        {
            var invoker = typeof(InstanceInvokersTests).CreateInvoker<string, SpanStub, SpanStub>(nameof(TestSpan2));
            Span<char> span = stackalloc char[] { 'H', 'e', 'l', 'l', 'o' };
            Assert.Equal("Hello", invoker.Invoke(SpanStub.Of(span)));
        }
    }
}
