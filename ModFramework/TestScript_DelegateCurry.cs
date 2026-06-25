using Mod;
using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
#if UNITY_5_3_OR_NEWER
using SpanStub = Mod.LowLevel.SpanStubModern;
#elif NETFRAMEWORK
using SpanStub = Mod.LowLevel.SpanStubOld;
#else
using SpanStub = Mod.LowLevel.SpanStubModern;
#endif

public static class TestScript_DelegateCurry
{
    public static void Test()
    {
#if NET7_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif

        Console.WriteLine("---------- Test For Delegate Currying ----------");
        { // Pass1: explicit
            var func = DelegateCurry.Curry((int f0, int f1, int f2, int f3, int f4, int f5) => new TestFieldsStruct<int>(f0, f1, f2, f3, f4, f5),
                3).Curry(4).Curry(5).Curry(6).Curry(7);
            var inst10 = func(8);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        { // Pass2: ConvertedClone
            TestCurryDel odel = (int f0, int f1, int f2, int f3, int f4, int f5) => new TestFieldsStruct<int>(f0, f1, f2, f3, f4, f5);
            var func = odel.ConvertedClone<Func<int, int, int, int, int, int, TestFieldsStruct>>().Curry(30).Curry(40).Curry(50).Curry(60).Curry(70).ConvertedClone<TestFinalCurriedDel>();
            var inst10 = func(80);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        {
            // Pass3: implicit
            // notice, on NativeAOT we should preserve the explicit curry code some where.
            // and implicit currying is slow!
            TestCurryDel odel = (int f0, int f1, int f2, int f3, int f4, int f5) => new TestFieldsStruct<int>(f0, f1, f2, f3, f4, f5);
            var func = odel.Curry(9).Curry(8).Curry(7).Curry(6).Curry(5).ConvertedClone<TestFinalCurriedDel>();
            var inst10 = func(4);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
    }

    delegate long TestDelegate11(ref int r);
    delegate long TestDelegate21(object thiz, ref int r);
    delegate void ConstructorDelegate1(ref UnsafeEx.ConstructorToken obj, int p1, int p2, int p3, int p4, int p5, int p6);
    delegate TestFieldsStruct<int> TestCurryDel(int p1, int p2, int p3, int p4, int p5, int p6);
    delegate TestFieldsStruct<int> TestFinalCurriedDel(int p6);

    static long TestMethod1(IntPtr p)
    {
        return p.ToInt64() + 1;
    }
    public static ref int TestMethod2(ref int r)
    {
        ++r;
        return ref r;
    }
    public static void TestMethod3(ref int r)
    {
        Console.WriteLine($"in {nameof(TestMethod3)}");
        ++r;
    }

    public static ReadOnlySpan<char> TestSpan1(string str)
    {
        ReadOnlySpan<char> rv = str.AsSpan();
        return rv;
    }
    public static string TestSpan2(ReadOnlySpan<char> span)
    {
        return new string(span.ToArray());
    }

    public class TestBaseClass
    {
        public virtual long TestMethod(IntPtr p)
        {
            Console.WriteLine($"{nameof(TestBaseClass)}.{nameof(TestMethod)}");
            return p.ToInt64() + 1;
        }
    }
    public class TestChildClass : TestBaseClass
    {
        public override long TestMethod(IntPtr p)
        {
            Console.WriteLine($"{nameof(TestChildClass)}.{nameof(TestMethod)}");
            return p.ToInt64() - 1;
        }
    }

    class TestFieldsClass
    {
        public int Field0 = 0;
        public int Field1 = 1;
        public int Field2 = 2;
        public int Field3 = 3;
        public int Field4 = 4;
        public int Field5 = 5;

        public TestFieldsClass() { }
        public TestFieldsClass(int i0, int i1, int i2, int i3, int i4, int i5)
        {
            Field0 = i0;
            Field1 = i1;
            Field2 = i2;
            Field3 = i3;
            Field4 = i4;
            Field5 = i5;
        }
    }
    struct TestFieldsStruct
    {
        public int Field0;
        public int Field1;
        public int Field2;
        public int Field3;
        public int Field4;
        public int Field5;

        public TestFieldsStruct(int i0, int i1, int i2, int i3, int i4, int i5)
        {
            Field0 = i0;
            Field1 = i1;
            Field2 = i2;
            Field3 = i3;
            Field4 = i4;
            Field5 = i5;
        }

        public T TestGeneric<T>(T t)
        {
            return t;
        }

        public void TestValueTypeInstanceMethod(int v)
        {
            Field0 = v;
            Field1 = v + 1;
            Field2 = v + 2;
            Field3 = v + 3;
            Field4 = v + 4;
            Field5 = v + 5;
        }
    }
    class TestFieldsClass<T>
    {
        public T Field0;
        public T Field1;
        public T Field2;
        public T Field3;
        public T Field4;
        public T Field5;

        public TestFieldsClass() { }
        public TestFieldsClass(T i0, T i1, T i2, T i3, T i4, T i5)
        {
            Field0 = i0;
            Field1 = i1;
            Field2 = i2;
            Field3 = i3;
            Field4 = i4;
            Field5 = i5;
        }

        public long TestMethod(ref T r)
        {
            var rr = RawRef.Of(ref r);
            return (long)(rr.Address + 1);
        }
    }
    struct TestFieldsStruct<T>
    {
        public T Field0;
        public T Field1;
        public T Field2;
        public T Field3;
        public T Field4;
        public T Field5;

        public TestFieldsStruct(T i0, T i1, T i2, T i3, T i4, T i5)
        {
            Field0 = i0;
            Field1 = i1;
            Field2 = i2;
            Field3 = i3;
            Field4 = i4;
            Field5 = i5;
        }
        public T TestGeneric<U>(U u)
        {
            return RawRef.Of(ref u).GetRef<T>();
        }
    }
}
