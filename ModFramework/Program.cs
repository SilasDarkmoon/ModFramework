using Mod;
using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#if NETCOREAPP
using SpanStubChar = Mod.LowLevel.SpanStubModern<char>;
#else
using SpanStubChar = Mod.LowLevel.SpanStubOld<char>;
#endif

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectHeader
    {
        public IntPtr TypeHandle;
        public IntPtr SyncAddress;
    }

#if NET10_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, type: typeof(TestFieldsClass))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, type: typeof(TestFieldsStruct))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.All, type: typeof(Program))]
    //[RequiresDynamicCode("Calls System.Reflection.MethodInfo.MakeGenericMethod(params Type[])")]
#endif
    public static void Main()
    {
#if NET10_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif
        var stopwatch = Stopwatch.StartNew();

        Console.WriteLine($"Object Header Size: {UnsafeEx.GetObjectHeaderSize()}");
        #region Test For Object Creator
        Console.WriteLine("---------- Test For Object Creator ----------");
        Console.WriteLine(typeof(Action).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(Action<int>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(StringBuilder).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(List<int>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsClass).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestDelegateGenericClass<IntPtr>).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(TestFieldsStruct).GetUninitializedObject().GetType());
        Console.WriteLine(typeof(ValueTuple<IntPtr, IntPtr>).GetUninitializedObject().GetType());
        #endregion

        #region Test For Delegate
        Console.WriteLine("---------- Test For Delegate ----------");
        var mi1 = typeof(Program).GetMethod("TestMethod1", BindingFlags.NonPublic | BindingFlags.Static);
        TestDelegate2 del2 = mi1.CreateDelegateUnsafe<TestDelegate2>();
        int a = 0;
        RawRef rr = new RawRef();
        rr.SetRef(ref a);
        Console.WriteLine(rr.Address);
        Console.WriteLine(del2(ref a));

        var ti = new TestDelegateChildClass();
        var mi2 = typeof(TestDelegateClass).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        var deli2 = mi2.CreateDelegateUnsafe<TestDelegate3>();
        Console.WriteLine(deli2("", ref a));

        var mig = typeof(TestDelegateGenericClass<int>).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        var delg = mig.CreateDelegateUnsafe<TestDelegate3>();
        Console.WriteLine(delg.Invoke("", ref a));

        var delg2 = mig.CreateDelegateUnsafe<Func<object, IntPtr, long>>();
        Console.WriteLine(delg2.Invoke("", (IntPtr)(666666)));

        Console.WriteLine("---------------------------------------------------------------------");

        var con1 = new UnsafeEx.Constructor<TestFieldsStruct, ConstructorDelegate1>();
        con1.Init(ref con1.Alloc(out var inst1), 9, 8, 7, 6, 5, 4);
        Console.WriteLine($"{inst1.Field0}, {inst1.Field1}, {inst1.Field2}, {inst1.Field3}, {inst1.Field4}, {inst1.Field5}");

        var con2 = new UnsafeEx.Constructor<TestFieldsClass, ConstructorDelegate1>();
        con2.Init(ref con2.Alloc(out var inst2), 2, 3, 4, 5, 6, 7);
        Console.WriteLine($"{inst2.Field0}, {inst2.Field1}, {inst2.Field2}, {inst2.Field3}, {inst2.Field4}, {inst2.Field5}");
        Console.WriteLine("---------------------------------------------------------------------");
        #endregion

        #region Test For PointerDelegate
        IntPtr lpda = (IntPtr)10;
        int lpdi = (int)100;
        long lpdb = 300;
        // Pass1
        {
            var invoker = new StaticInvoker<IntPtr, IntPtr, IntPtr>(typeof(Program), "TestMethod2");
            Console.WriteLine(invoker.Invoke(lpda));
        }
        // Pass2:
        {
            var invoker = new StaticInvoker<IntPtr, ByRefParam, IntPtr>(typeof(Program), "TestMethod2");
            Console.WriteLine(invoker.Invoke(in lpda));
        }
        // Pass5
        {
            var invoker = new StaticInvoker<ByRefParam, ByRefParam, int>(typeof(Program), "TestFunc5");
            Console.WriteLine(invoker.Invoke(in lpdi).ToRef<int>());
        }
        // Pass6
        {
            var invoker = new StaticInvoker<VoidReturn, IntPtr, ByRefParam>(typeof(Program), "TestFunc6");
            Console.WriteLine(invoker.Invoke(lpdi.ToFakeObj()));
            Console.WriteLine(lpdi);
        }
        // Pass7
        {
            var invoker = new StaticInvoker<SpanStubChar, string, string>(typeof(Program), "TestFunc7");
            string str = "Hello, World!";
            var span = invoker.Invoke(str).ConvertToReadOnlySpan();
            foreach (var ch in span)
            {
                Console.Write(ch);
            }
            Console.WriteLine();
        }
        // Pass8
        {
            var invoker = new StaticInvoker<string, SpanStubChar, SpanStubChar>(typeof(Program), "TestFunc8");
            Span<char> span = stackalloc char[] { 'H', 'e', 'l', 'l', 'o' };
            var str = invoker.Invoke(span);
            Console.WriteLine(str);
        }
        #endregion
        Console.WriteLine(UnsafeEx.Size(typeof(Span<byte>)));
        // Pass7
        {
            var invoker = UnsafeEx.CreateDelegateUnsafe<Func<string, SpanStubModern<char>>>(UnsafeEx.FindMethod(typeof(Program).GetMethods("TestFunc7"), new[] { typeof(string) }));
            //var invoker = new StaticInvoker<SpanStubModern<char>, string, string>(typeof(TestScript), "TestFunc7");
            string str = "Hello, World!";
            var span = invoker.Invoke(str).ConvertToReadOnlySpan();
            Console.WriteLine(span.GetAddress());
            foreach (var ch in span)
            {
                Console.Write(ch);
            }
            Console.WriteLine();
        }
    }

    private static long TestMethod1(IntPtr p)
    {
        return p.ToInt64() + 1;
    }
    public static long TestMethod2(ref int r)
    {
        RawRef rr = new RawRef();
        rr.SetRef(ref r);
        return rr.Address.ToInt64() + 2;
    }
    private static void DummyMethod2(byte a)
    {

    }
    public static ref int TestFunc5(ref int r)
    {
        ++r;
        return ref r;
    }
    public static void TestFunc6(ref int r)
    {
        Console.WriteLine("in TestFunc6");
        ++r;
    }
    public static ReadOnlySpan<char> TestFunc7(string str)
    {
        ReadOnlySpan<char> rv = str.AsSpan();
        return rv;
    }
    public static string TestFunc8(ReadOnlySpan<char> span)
    {
        return new string(span.ToArray());
    }
}

[StructLayout(LayoutKind.Sequential, Pack = 0)]
public struct TestPack
{
    //public int n;
    //public object o;
    public DateTime d;
    public byte b;
}

delegate void ConstructorDelegate1(ref UnsafeEx.ConstructorToken obj, int p1, int p2, int p3, int p4, int p5, int p6);

delegate void DummyDelegate2(byte a);
delegate void DummyDelegate3(byte a, byte b);

delegate TestDelegate1 CreateTestDelegate1Del(object obj, IntPtr method);
delegate long TestDelegate1(IntPtr p);
delegate long TestDelegate2(ref int r);

delegate long TestDelegate3(object thiz, ref int r);

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateClass
{
    public virtual long TestMethod(IntPtr p)
    {
        Console.WriteLine("TestDelegateClass.TestMethod");
        return p.ToInt64() + 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateChildClass : TestDelegateClass
{
    public override long TestMethod(IntPtr p)
    {
        Console.WriteLine("TestDelegateChildClass.TestMethod");
        return p.ToInt64() - 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateGenericClass<T>
{
    public long TestMethod(ref T r)
    {
        RawRef rr = new RawRef();
        rr.SetRef(ref r);
        return rr.Address.ToInt64() + 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
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
#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
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
}

class DelegateStub
{
    public object Target;
    public object MethodBase;
    public IntPtr Method;
}
static class TestEventTrigger
{
    public static event Action<int> OnTestEvent;
    public static void Trigger(int value)
    {
        OnTestEvent?.Invoke(value);
    }

    public delegate void TestEventHandler(int value);
    public static event TestEventHandler OnTestEventWithCustomDelegate;
    public static void TriggerWithCustomDelegate(int value)
    {
        OnTestEventWithCustomDelegate?.Invoke(value);
    } 
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct FieldDesc
{
    [FieldOffset(0)] private readonly void* m_pMTOfEnclosingClass;
    [FieldOffset(8)] private readonly uint m_dword1;
    [FieldOffset(12)] private readonly uint m_dword2;

    public int Offset => (int)(m_dword2 & 0x07FFFFFF);
    public int MB => (int)(m_dword1 & 0x00FFFFFF);
    public bool IsStatic => (m_dword1 & (1u << 24)) != 0;
    public bool IsThreadLocal => (m_dword1 & (1u << 25)) != 0;
    public bool IsRVA => (m_dword1 & (1u << 26)) != 0;
    public int Protection => (int)((m_dword1 >> 27) & 0x7);
    public bool RequiresFullMbValue => (m_dword1 & (1u << 30)) != 0;
    public int Type => (int)((m_dword2 >> 27) & 0x1F);
}

[StructLayout(LayoutKind.Sequential, Pack = 64)]
struct A
{
    public int x;
    public B b;   // 嵌套 struct
    public long y;
    public object aobj;
}

struct B
{
    public float f;
    public object obj;
}

[StructLayout( LayoutKind.Sequential, Pack = 1)]
struct C
{
    public byte a;
    public double c;
}

struct D
{
    public byte b;
    public E e;
}

[StructLayout(LayoutKind.Sequential, Pack = 128)]
struct E
{
    public short c;
    public double d;
    public F f;
}

unsafe struct F
{
    public fixed byte Buffer[128];
}

//#region Test For WeakDelegate

//var odel = new Action<int>(value =>
//{
//    Console.WriteLine("Received event with value: " + value);
//});
//var ndel = new TestEventTrigger.TestEventHandler(value => { });

//var omf = odel.GetType().GetField("_method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var otf = odel.GetType().GetField("_target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var nmf = ndel.GetType().GetField("_method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var ntf = ndel.GetType().GetField("_target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//var nndel = System.Runtime.CompilerServices.Unsafe.As<TestEventTrigger.TestEventHandler>(odel);

//TestEventTrigger.OnTestEventWithCustomDelegate += nndel;
//TestEventTrigger.TriggerWithCustomDelegate(18);

//static class TestEventTrigger
//{
//    public static event Action<int> OnTestEvent;

//    public delegate void TestEventHandler(int value);

//    public static event TestEventHandler OnTestEventWithCustomDelegate;
//    public static void Trigger(int value)
//    {
//        OnTestEvent?.Invoke(value);
//    }

//    public static void TriggerWithCustomDelegate(int value)
//    {
//        OnTestEventWithCustomDelegate?.Invoke(value);
//    }

//    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_target")]
//    public static void TestAccessor()
//    {

//    }
//}
//#endregion
