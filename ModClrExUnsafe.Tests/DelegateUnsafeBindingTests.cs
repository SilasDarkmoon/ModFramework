using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// CreateDelegateUnsafe binding shapes not covered by DelegateExTests,
    /// migrated from TestScript.cs / TestScript_UnsafeInstanceDelegate.cs:
    /// "fake" instance bindings (a deliberately mismatching target object),
    /// virtual dispatch through the bound target, binding to System.Func,
    /// struct-instance bindings and value-type ChangeTarget.
    /// </summary>
    public class DelegateUnsafeBindingTests
    {
        private delegate long TestDelegatei1();
        private delegate long TestDelegatei2(ref int r);

        private static long TestMethod1(IntPtr p) => p.ToInt64() + 1;

        public class TestBaseClass
        {
            public virtual long TestMethod(IntPtr p) => p.ToInt64() + 1;
        }
        public class TestChildClass : TestBaseClass
        {
            public override long TestMethod(IntPtr p) => p.ToInt64() - 1;
        }

        private class TestFieldsClass<T>
        {
            public long TestMethod(ref T r)
            {
                var rr = RawRef.Of(ref r);
                return (long)(rr.Address + 1);
            }
        }
        private struct TestFieldsStruct
        {
            public int Field0;
            public TestFieldsStruct(int i0) { Field0 = i0; }
            public int GetField0() => Field0;
        }
        private struct TestFieldsStruct<T>
        {
            public T Field0;
            public TestFieldsStruct(T i0) { Field0 = i0; }
            public T GetField0() => Field0;
        }

        // =====================================================================
        // fake static binding: static method bound to a delegate with a
        // receiver slot - the target object is ignored for static methods
        // =====================================================================

        [Fact]
        public void FakeStaticBinding_TargetIgnored()
        {
            var mi = typeof(DelegateUnsafeBindingTests).GetMethod(nameof(TestMethod1),
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            object o = new object();
            TestDelegatei1 del = o.CreateDelegateUnsafe<TestDelegatei1>(mi);
            Assert.Equal(RawRef.Of(o).Address + 1, del());
        }

        // =====================================================================
        // fake instance binding: instance method bound to a MISMATCHING target
        // object - the receiver slot passes whatever object we choose
        // =====================================================================

        [Fact]
        public void FakeInstanceBinding_VirtualDispatchesOverride()
        {
            // bound via the base-class method info, but the target is a string:
            // the calli goes to the method body directly (no vtable) - locks the
            // "fake binding" semantics: receiver is raw, dispatch is static
            var mi = typeof(TestChildClass).GetMethod(nameof(TestChildClass.TestMethod));
            var del = "".CreateDelegateUnsafe<TestDelegatei2>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address - 1, del(ref a));   // Child body: p - 1
        }

        [Fact]
        public void FakeInstanceBinding_GenericClass()
        {
            var mi = typeof(TestFieldsClass<Guid>).GetMethod(nameof(TestFieldsClass<Guid>.TestMethod));
            var del = "".CreateDelegateUnsafe<TestDelegatei2>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, del(ref a));
        }

        [Fact]
        public void BindToSystemFunc_IntPtrArg()
        {
            var mi = typeof(TestFieldsClass<Guid>).GetMethod(nameof(TestFieldsClass<Guid>.TestMethod));
            var del = "".CreateDelegateUnsafe<Func<IntPtr, long>>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Assert.Equal(r.Address + 1, del(r.Address));
        }

        // =====================================================================
        // struct instance bindings (value-type receivers)
        // =====================================================================

        [Fact]
        public void StructInstanceMethod_Binding()
        {
            var mi = typeof(TestFieldsStruct).GetMethod(nameof(TestFieldsStruct.GetField0));
            var inst = new TestFieldsStruct(3);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            Assert.Equal(3, del());
        }

        [Fact]
        public void GenericStructInstanceMethod_Binding()
        {
            var mi = typeof(TestFieldsStruct<int>).GetMethod(nameof(TestFieldsStruct<int>.GetField0));
            var inst = new TestFieldsStruct<int>(4);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            Assert.Equal(4, del());
        }

        [Fact]
        public void StructInstanceMethod_ChangeTarget()
        {
            var mi = typeof(TestFieldsStruct<int>).GetMethod(nameof(TestFieldsStruct<int>.GetField0));
            var inst = new TestFieldsStruct<int>(4);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            var inst2 = new TestFieldsStruct<int>(40);
            del.ChangeTarget(inst2);
            Assert.Equal(40, del());
        }
    }
}
