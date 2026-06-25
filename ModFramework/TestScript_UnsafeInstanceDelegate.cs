using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public static class TestScript_UnsafeInstanceDelegate
{
    public static void Test()
    {
#if NET7_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif

        #region Test For Delegate
        Console.WriteLine("---------- Test For Instance Delegate ----------");
        // Pass1
        { // fake static binding
            var mi = typeof(TestScript).GetMethods(nameof(TestMethod1)).FindOverload(new[] { typeof(IntPtr) });
            object o = new object();
            TestDelegatei1 del = o.CreateDelegateUnsafe<TestDelegatei1>(mi);
            var r = RawRef.Of(o);
            Console.WriteLine($"{r} -> {del():X}");
        }
        // Pass2
        { // fake instance binding
            //var ti = new TestDelegateChildClass(); we want to call to a mismatching target.
            var mi = typeof(TestBaseClass).GetMethods(nameof(TestBaseClass.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = "".CreateDelegateUnsafe<TestDelegatei2>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del(ref a):X}");
        }
        // Pass2.5
        { // fake instance binding
            //var ti = new TestDelegateChildClass(); we want to call to a mismatching target.
            var mi = typeof(TestChildClass).GetMethods(nameof(TestChildClass.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = "".CreateDelegateUnsafe<TestDelegatei2>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del(ref a):X}");
        }
        // Pass3
        { // fake generic instance binding
            var inst = new TestFieldsClass<Guid>(); // new inst to generate generic context. (IL2CPP)
            //typeof(TestFieldsClass<Guid>).GetAllFields(); // or this, also force generate generic context. (IL2CPP)
            var mi = typeof(TestFieldsClass<Guid>).GetMethods(nameof(TestFieldsClass<Guid>.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = "".CreateDelegateUnsafe<TestDelegatei2>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del(ref a):X}");
        }
        // Pass4
        { // bind to System.Func
            var mi = typeof(TestFieldsClass<Guid>).GetMethods(nameof(TestFieldsClass<Guid>.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = "".CreateDelegateUnsafe<Func<IntPtr, long>>(mi);
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del(r.Address):X}");
        }
        // Pass5
        { // struct instance method
            var mi = typeof(TestFieldsStruct).GetMethod(nameof(TestFieldsStruct.GetField0));
            var inst = new TestFieldsStruct(3, 4, 5, 6, 7, 8);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            Console.WriteLine(del());
        }
        // Pass6
        { // generic struct instance method
            var mi = typeof(TestFieldsStruct<int>).GetMethod(nameof(TestFieldsStruct<int>.GetField0));
            var inst = new TestFieldsStruct<int>(4, 5, 6, 7, 8, 9);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            Console.WriteLine(del());
        }
        // Pass7
        { // change target
            var mi = typeof(TestFieldsStruct<int>).GetMethod(nameof(TestFieldsStruct<int>.GetField0));
            var inst = new TestFieldsStruct<int>(4, 5, 6, 7, 8, 9);
            var del = inst.CreateDelegateUnsafe<Func<int>>(mi);
            var inst2 = new TestFieldsStruct<int>(40, 50, 60, 70, 80, 90);
            del.ChangeTarget(inst2);
            Console.WriteLine(del());
        }
        Console.WriteLine("---------------------------------------------------------------------");
        #endregion
    }

    delegate long TestDelegatei1();
    //delegate long TestDelegate11(ref int r);
    delegate long TestDelegatei2(ref int r);
    //delegate long TestDelegate21(object thiz, ref int r);
    delegate void ConstructorDelegate1(ref UnsafeEx.ConstructorToken obj, int p1, int p2, int p3, int p4, int p5, int p6);

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

        public int GetField0()
        {
            return Field0;
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

        public T GetField0()
        {
            return Field0;
        }
    }
}
