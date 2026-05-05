using Mod;
using Mod.LowLevel;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
    public static void Main()
    {

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

        #region Test For UnsafeAccessor
        Console.WriteLine(UnsafeEx.GetObjectHeaderSize());

        var odel = new Action<int>(value =>
        {
            Console.WriteLine("Received event with value: " + value);
        });
        var ndel = new TestEventTrigger.TestEventHandler(value => { });

        //Console.WriteLine(FieldExt.GetFieldRef<object>(odel, offsetTarget));
        //Console.WriteLine(FieldExt.GetFieldRef<IntPtr>(odel, offsetMethod));
        //ref object ntarget = ref FieldExt.GetFieldRef<object>(ndel, offsetTarget);
        //ref IntPtr nmethod = ref FieldExt.GetFieldRef<IntPtr>(ndel, offsetMethod);
        //Console.WriteLine(ntarget);
        //Console.WriteLine(nmethod);
        //ntarget = FieldExt.GetFieldRef<object>(odel, offsetTarget);
        //nmethod = FieldExt.GetFieldRef<IntPtr>(odel, offsetMethod);

        foreach (var fi in ndel.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
        {
            Console.WriteLine(fi);
        }

        ref object ntarget = ref ndel.GetField(DelegateEx.FieldTarget);
        ntarget = odel.GetField(DelegateEx.FieldTarget);
        ref IntPtr nmethod = ref ndel.GetField(DelegateEx.FieldMethodPtr);
        nmethod = odel.GetField(DelegateEx.FieldMethodPtr);

        TestEventTrigger.OnTestEventWithCustomDelegate += ndel;
        TestEventTrigger.TriggerWithCustomDelegate(18);

        var fields = odel.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (var field in fields)
        {
            Console.WriteLine(field);
            var handle = field.FieldHandle;
            Console.WriteLine(handle.Value);
            RawRef r = new RawRef();
            r.Address = handle.Value;
            var fd = r.GetValue<FieldDesc>();
            Console.WriteLine(fd.Offset);
        }

        //Console.WriteLine(FieldExt.GetFieldOffset(typeof(DelegateStub).GetField("Method")));
        //Console.WriteLine(offsetMethod);

        A a = new A();
        ref B rb = ref a.b;

        RawRef rtest = new RawRef();
        rtest.SetRefObj(odel);
        var o = rtest.GetRefObj();
        Console.WriteLine(o);

        //ObjectLayoutInspector.TypeLayout.PrintLayout<A>();
        //ObjectLayoutInspector.TypeLayout.PrintLayout<C>();
        //ObjectLayoutInspector.TypeLayout.PrintLayout<D>();

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

#endregion

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
