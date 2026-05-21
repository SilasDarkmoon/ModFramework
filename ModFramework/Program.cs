using Mod;
using Mod.LowLevel;
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

class Program
{
#if NET10_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.AllFields, type: typeof(TestFieldsClass))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.AllFields, type: typeof(TestFieldsStruct))]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
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
        //#region Test For StringPool
        //string[] strs = new string[1024 * 1024 * 2];
        //for (int i = 0; i < 1024 * 1024; ++i)
        //{
        //    strs[i] = strs[i + 1024 * 1024] = Guid.NewGuid().ToString();
        //}
        //string[] strsNonPooled = new string[1024 * 1024];
        //for (int i = 0; i < 1024 * 1024; ++i)
        //{
        //    strsNonPooled[i] = i.ToString();
        //}

        //var pool1 = new StringPoolConcurrent();
        //var pool2 = new StringPoolLocked();
        //var pool3 = new StringPoolConcurrent();
        //var pool4 = new StringPoolLocked();

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    pool1.Push(str);
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool1: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    pool2.Push(str);
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool2: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    pool3.Push(str);
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool3: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    pool4.Push(str);
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool4: " + stopwatch.Elapsed);

        ////-------------------------------------------------------

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    if (pool1.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool1 should contain all strs");
        //    }
        //}
        //foreach (var str in strsNonPooled)
        //{
        //    if (pool1.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool1 should not contain any strsNonPooled");
        //    }
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool1 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    if (pool2.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool2 should contain all strs");
        //    }
        //}
        //foreach (var str in strsNonPooled)
        //{
        //    if (pool2.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool2 should not contain any strsNonPooled");
        //    }
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool2 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    if (pool3.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool3 should contain all strs");
        //    }
        //});
        //Parallel.ForEach(strsNonPooled, str =>
        //{
        //    if (pool3.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool3 should not contain any strsNonPooled");
        //    }
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool3 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    if (pool4.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool4 should contain all strs");
        //    }
        //});
        //Parallel.ForEach(strsNonPooled, str =>
        //{
        //    if (pool4.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool4 should not contain any strsNonPooled");
        //    }
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool4 check pooled: " + stopwatch.Elapsed);
        //#endregion

        //#region Test For WeakAttachment
        //WeakAttachment.Key key = "test_key";
        //Test1 test1 = new Test1();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //test1.exFields ??= new ExFields();
        //test1.exFields.Value = i;
        //if (test1.exFields.Value != i)
        //{
        //throw new Exception("test1.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("raw field access: " + stopwatch.Elapsed);

        //Test2 test2 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test2.GetOrCreateAttachment<ExFields>(key);
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test2.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("att field access: " + stopwatch.Elapsed);

        //Test2 test3 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test3.GetOrCreateAttachment<ExFields>("test_Key_Unpooled");
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test3.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("str field access: " + stopwatch.Elapsed);

        //Test2 test4 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test4.GetOrCreateAttachment<ExFields>();
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test4.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("typ field access: " + stopwatch.Elapsed);

        //class Test1
        //{
        //    public ExFields? exFields;
        //}
        //class Test2
        //{
        //}
        //class ExFields
        //{
        //    public int Value { get; set; }
        //}
        //#endregion

        //#region Test For UnsafeAccessor
        //Console.WriteLine(RuntimeInfo.Runtime);
        //Console.WriteLine(UnsafeEx.GetObjectHeaderSize());

        //var odel = new Action<int>(value =>
        //{
        //    Console.WriteLine("Received event with value: " + value);
        //});
        //var ndel = new TestEventTrigger.TestEventHandler(value => { });

        //TestFieldsClass t = new TestFieldsClass();
        //foreach (var field in typeof(TestFieldsClass).GetAllFields())
        //{
        //    Console.WriteLine(t.GetField(field.OfType<int>()));
        //}

        //TestFieldsStruct ts = new TestFieldsStruct(0, 1, 2, 3, 4, 5);
        //foreach (var field in typeof(TestFieldsStruct).GetAllFields())
        //{
        //    Console.WriteLine(t.GetField(field.OfType<int>()));
        //}
        //#endregion

        #region Test For Delegate
        var mi1 = typeof(Program).GetMethod("TestMethod1", BindingFlags.NonPublic | BindingFlags.Static);
        RuntimeHelpers.PrepareMethod(mi1.MethodHandle);
        var p1 = mi1.MethodHandle.GetFunctionPointer();
        var del2 = Marshal.GetDelegateForFunctionPointer<TestDelegate2>(p1);
        int a = 0;
        RawRef rr = new RawRef();
        rr.SetRef(ref a);
        Console.WriteLine(rr.Address);
        Console.WriteLine(del2(ref a));

        var ti = new TestDelegateChildClass();
        var mi2 = typeof(TestDelegateClass).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        RuntimeHelpers.PrepareMethod(mi2.MethodHandle);
        var p2 = mi2.MethodHandle.GetFunctionPointer();
        var deli2 = Marshal.GetDelegateForFunctionPointer<TestDelegate3>(p2);
        Console.WriteLine(deli2(null, ref a));

        var mig = typeof(TestDelegateGenericClass<int>).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        RuntimeHelpers.PrepareMethod(mig.MethodHandle);
        var pgg = mig.MethodHandle.GetFunctionPointer();
        var delg = Marshal.GetDelegateForFunctionPointer<TestDelegate3>(pgg);
        Console.WriteLine(delg.Invoke(null, ref a));
        #endregion
    }

    private static long TestMethod1(IntPtr p)
    {
        return p.ToInt64() + 1;
    }
    private static long TestMethod2(ref int r)
    {
        RawRef rr = new RawRef();
        rr.SetRef(ref r);
        return rr.Address.ToInt64() + 1;
    }
}

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
    DynamicallyAccessedMemberTypes.AllFields)]
#endif
class TestFieldsClass
{
    public int Field0 = 0;
    public int Field1 = 1;
    public int Field2 = 2;
    public int Field3 = 3;
    public int Field4 = 4;
    public int Field5 = 5;
}
#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.AllFields)]
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
