using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;
#if UNITY_5_3_OR_NEWER
using SpanStubChar = Mod.LowLevel.SpanStubModern<char>;
#elif NETFRAMEWORK
using SpanStubChar = Mod.LowLevel.SpanStubOld<char>;
#else
using SpanStubChar = Mod.LowLevel.SpanStubModern<char>;
#endif

public static class TestScript
{
    public static void Test()
    {
#if NET7_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif

        Console.WriteLine($"Object Header Size: {UnsafeEx.GetObjectHeaderSize()}, Runtime: {RuntimeInfo.Runtime}{(RuntimeInfo.IsIl2Cpp ? (RuntimeInfo.IsHybridCLREnabled ? " with HybridCLR" : "") : "")}");
        #region Test For Object Creator
        Console.WriteLine("---------- Test For Object Creator ----------");
        Console.WriteLine(typeof(Action).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(Action<int>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(StringBuilder).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(List<int>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsClass).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsStruct).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsClass<Guid>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsStruct<Guid>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(ValueTuple<IntPtr, IntPtr>).GetUninitializedObject().GetType());
        #endregion

        #region Test For Delegate
        Console.WriteLine("---------- Test For Delegate ----------");
        // Pass1
        { // static binding
            var mi = typeof(TestScript).GetMethods(nameof(TestMethod1)).FindOverload(new[] { typeof(IntPtr) });
            TestDelegate11 del = mi.CreateDelegateUnsafe<TestDelegate11>();
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del(ref a):X}");
        }
        // Pass2
        { // instance binding
            //var ti = new TestDelegateChildClass(); we want to call to a mismatching target.
            var mi = typeof(TestBaseClass).GetMethods(nameof(TestBaseClass.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = mi.CreateDelegateUnsafe<TestDelegate21>();
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del("", ref a):X}");
        }
        // Pass2.5
        { // instance binding
            //var ti = new TestDelegateChildClass(); we want to call to a mismatching target.
            var mi = typeof(TestChildClass).GetMethods(nameof(TestChildClass.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = mi.CreateDelegateUnsafe<TestDelegate21>();
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del("", ref a):X}");
        }
        // Pass3
        { // generic instance binding
            var mi = typeof(TestFieldsClass<Guid>).GetMethods(nameof(TestFieldsClass<Guid>.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = mi.CreateDelegateUnsafe<TestDelegate21>();
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del("", ref a):X}");
        }
        // Pass4
        { // bind to System.Func
            var mi = typeof(TestFieldsClass<Guid>).GetMethods(nameof(TestFieldsClass<Guid>.TestMethod)).FindOverload(new[] { typeof(IntPtr) });
            var del = mi.CreateDelegateUnsafe<Func<object, IntPtr, long>>();
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {del("", r.Address):X}");
        }
        //Console.WriteLine("---------------------------------------------------------------------");
        Console.WriteLine("---------- Test For Constructor ----------");
        // Pass1
        {
            var con1 = new UnsafeEx.Constructor<TestFieldsStruct, ConstructorDelegate1>();
            con1.Init(ref con1.Alloc(out var inst1), 9, 8, 7, 6, 5, 4);
            Console.WriteLine($"{inst1.Field0}, {inst1.Field1}, {inst1.Field2}, {inst1.Field3}, {inst1.Field4}, {inst1.Field5}");
        }
        // Pass2
        {
            var con2 = new UnsafeEx.Constructor<TestFieldsClass, ConstructorDelegate1>();
            con2.Init(ref con2.Alloc(out var inst2), 2, 3, 4, 5, 6, 7);
            Console.WriteLine($"{inst2.Field0}, {inst2.Field1}, {inst2.Field2}, {inst2.Field3}, {inst2.Field4}, {inst2.Field5}");
        }
        //Console.WriteLine("---------------------------------------------------------------------");
        #endregion

        #region Test For Invokers
        Console.WriteLine("---------- Test For Invokers ----------");
        // Pass1
        { // static invoker
            var invoker = typeof(TestScript).CreateInvoker<long, IntPtr, IntPtr>(nameof(TestMethod1));
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {invoker(r.Address):X}");
        }
        // Pass2
        { // instance invoker
            TestChildClass inst = new TestChildClass();
            var invoker = inst.CreateInstanceInvoker<long, object, ByRefParam, object, int>(nameof(inst.TestMethod));
            int a = 12;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {invoker(in a):X}");
        }
        // Pass2.5
        { // open instance invoker
            //TestChildClass inst = new TestChildClass();
            var invoker = typeof(TestBaseClass).CreateInvoker<long, object, ByRefParam, object, int>(nameof(TestBaseClass.TestMethod));
            int a = 12;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {invoker("", in a):X}");
        }
        // value type open instance invoker
        {
            var inst = new TestFieldsStruct(9, 8, 7, 6, 5, 4);
            var invoker = typeof(TestFieldsStruct).CreateInvoker<VoidReturn, ByRefParam, int, TestFieldsStruct, int>(nameof(TestFieldsStruct.TestValueTypeInstanceMethod));
            invoker(in inst, 100);
            Console.WriteLine($"{inst.Field0}, {inst.Field1}, {inst.Field2}, {inst.Field3}, {inst.Field4}, {inst.Field5}");
        }
        // Pass3
        { // generic instance invoker
            TestFieldsClass<Guid> inst = new TestFieldsClass<Guid>();
            var invoker = inst.CreateInstanceInvoker<long, object, ByRefParam, object, int>(nameof(inst.TestMethod));
            int a = 13;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {invoker(in a):X}");
        }
        // Pass3.5
        { // generic open instance invoker
            //TestFieldsClass<Guid> inst = new TestFieldsClass<Guid>();
            var invoker = typeof(TestFieldsClass<Guid>).CreateInvoker<long, object, ByRefParam, object, int>(nameof(TestFieldsClass<Guid>.TestMethod));
            int a = 13;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{r} -> {invoker("", in a):X}");
        }
        // Pass4
        { // ref return
            var invoker = typeof(TestScript).CreateInvoker<ByRefParam, ByRefParam, int>(nameof(TestMethod2));
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{a} -> {invoker(in a).ToRef<int>()}");
        }
        // Pass5
        { // void return
            var invoker = typeof(TestScript).CreateInvoker<VoidReturn, ByRefParam, int>(nameof(TestMethod3));
            int a = 0;
            var r = RawRef.Of(ref a);
            Console.WriteLine($"{a} -> {invoker(in a)}{a}");
        }
        // Pass6
        { // Span return
            var invoker = typeof(TestScript).CreateInvoker<SpanStubChar, string, string>(nameof(TestSpan1));
            string str = "Hello, World!";
            var span = invoker.Invoke(str).ConvertToReadOnlySpan();
            foreach (var ch in span) { Console.Write(ch); }
            Console.WriteLine();
        }
        // Pass7
        { // Span argument
            var invoker = typeof(TestScript).CreateInvoker<string, SpanStubChar, SpanStubChar>(nameof(TestSpan2));
            Span<char> span = stackalloc char[] { 'H', 'e', 'l', 'l', 'o' };
            var str = invoker(span);
            Console.WriteLine(str);
        }
        Console.WriteLine("---------- Test For Ctor Invokers ----------");
        // Pass8
        { // struct ctor
            var ctor = typeof(TestFieldsStruct).CreateCustructorInvoker<TestFieldsStruct, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst10 = ctor(1, 1, 1, 1, 1, 1);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        // Pass9
        { // class ctor
            var ctor = typeof(TestFieldsClass).CreateCustructorInvoker<TestFieldsClass, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst10 = ctor(2, 2, 2, 2, 2, 2);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        // Pass10
        { // generic struct ctor
            var ctor = typeof(TestFieldsStruct<int>).CreateCustructorInvoker<TestFieldsStruct<int>, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst10 = ctor(3, 3, 3, 3, 3, 3);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        // Pass11
        { // generic struct ctor
            var ctor = typeof(TestFieldsClass<int>).CreateCustructorInvoker<TestFieldsClass<int>, int, int, int, int, int, int, int, int, int, int, int, int>();
            var inst10 = ctor(4, 4, 4, 4, 4, 4);
            Console.WriteLine($"{inst10.Field0}, {inst10.Field1}, {inst10.Field2}, {inst10.Field3}, {inst10.Field4}, {inst10.Field5}");
        }
        Console.WriteLine("---------- Test For Generic Invokers ----------");
        // Pass1
        { // generic method in normal struct
            var invoker = typeof(TestFieldsStruct).CreateGenericInvoker<IntPtr, ByRefParam, IntPtr, TestFieldsStruct, IntPtr>(nameof(TestFieldsStruct.TestGeneric));
            Console.WriteLine($"100 -> {invoker(default, (IntPtr)100)}");
        }
        // Pass2
        { // generic method in generic struct
            var invoker = typeof(TestFieldsStruct<long>).CreateGenericInvoker<long, ByRefParam, IntPtr, byte, IntPtr>(nameof(TestFieldsStruct<long>.TestGeneric));
            Console.WriteLine($"111 -> {invoker(default, (IntPtr)111)}");
        }
        #endregion

    }

    delegate long TestDelegate11(ref int r);
    delegate long TestDelegate21(object thiz, ref int r);
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
