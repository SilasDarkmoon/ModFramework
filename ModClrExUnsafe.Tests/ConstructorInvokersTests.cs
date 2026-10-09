using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for the constructor paths, migrated from TestScript.cs:
    ///  - UnsafeEx.Constructor<T, TDelegate> (Alloc + Init with an explicit
    ///    ctor delegate whose first parameter is the ref ConstructorToken)
    ///  - CreateCustructorInvoker (the unified ctor invoker family; note the
    ///    spelling is preserved from the library)
    /// </summary>
    public class ConstructorInvokersTests
    {
        private delegate void ConstructorDelegate1(ref UnsafeEx.ConstructorToken obj, int p1, int p2, int p3, int p4, int p5, int p6);

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
        }
        private class TestFieldsClass<T>
        {
            public T Field0, Field1, Field2, Field3, Field4, Field5;
            public TestFieldsClass(T i0, T i1, T i2, T i3, T i4, T i5)
            {
                Field0 = i0; Field1 = i1; Field2 = i2; Field3 = i3; Field4 = i4; Field5 = i5;
            }
        }
        private struct TestFieldsStruct<T>
        {
            public T Field0, Field1, Field2, Field3, Field4, Field5;
            public TestFieldsStruct(T i0, T i1, T i2, T i3, T i4, T i5)
            {
                Field0 = i0; Field1 = i1; Field2 = i2; Field3 = i3; Field4 = i4; Field5 = i5;
            }
        }

        private static void AssertFields(int f0, int f1, int f2, int f3, int f4, int f5,
            int e0, int e1, int e2, int e3, int e4, int e5)
        {
            Assert.Equal((e0, e1, e2, e3, e4, e5), (f0, f1, f2, f3, f4, f5));
        }

        // =====================================================================
        // UnsafeEx.Constructor (explicit ctor delegate + Alloc/Init)
        // =====================================================================

        [Fact]
        public void Constructor_Struct_AllocInit()
        {
            var con = new UnsafeEx.Constructor<TestFieldsStruct, ConstructorDelegate1>();
            con.Init(ref con.Alloc(out var inst), 9, 8, 7, 6, 5, 4);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                9, 8, 7, 6, 5, 4);
        }

        [Fact]
        public void Constructor_Class_AllocInit()
        {
            var con = new UnsafeEx.Constructor<TestFieldsClass, ConstructorDelegate1>();
            con.Init(ref con.Alloc(out var inst), 2, 3, 4, 5, 6, 7);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                2, 3, 4, 5, 6, 7);
        }

        // =====================================================================
        // CreateCustructorInvoker (unified ctor invoker family)
        // =====================================================================

        [Fact]
        public void CtorInvoker_Struct()
        {
            var ctor = typeof(TestFieldsStruct).CreateCustructorInvoker<TestFieldsStruct, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst = ctor.Invoke(1, 1, 1, 1, 1, 1);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                1, 1, 1, 1, 1, 1);
        }

        [Fact]
        public void CtorInvoker_Class()
        {
            var ctor = typeof(TestFieldsClass).CreateCustructorInvoker<TestFieldsClass, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst = ctor.Invoke(2, 2, 2, 2, 2, 2);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                2, 2, 2, 2, 2, 2);
        }

        [Fact]
        public void CtorInvoker_GenericStruct()
        {
            var ctor = typeof(TestFieldsStruct<int>).CreateCustructorInvoker<TestFieldsStruct<int>, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst = ctor.Invoke(3, 3, 3, 3, 3, 3);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                3, 3, 3, 3, 3, 3);
        }

        [Fact]
        public void CtorInvoker_GenericClass()
        {
            var ctor = typeof(TestFieldsClass<int>).CreateCustructorInvoker<TestFieldsClass<int>, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst = ctor.Invoke(4, 4, 4, 4, 4, 4);
            AssertFields(inst.Field0, inst.Field1, inst.Field2, inst.Field3, inst.Field4, inst.Field5,
                4, 4, 4, 4, 4, 4);
        }
    }
}
