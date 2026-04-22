using Mod;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

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
var stub = new DelegateStub();
var offsetTarget = UnsafeEx.GetFieldOffset(stub, ref stub.Target);
var offsetMethod = UnsafeEx.GetFieldOffset(stub, ref stub.Method);

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

UnsafeEx.MemCopy(ndel, odel);
TestEventTrigger.OnTestEventWithCustomDelegate += ndel;
TestEventTrigger.TriggerWithCustomDelegate(18);

//var fields = odel.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
//foreach (var field in fields)
//{
//    Console.WriteLine(field);
//    var handle = field.FieldHandle;
//    Console.WriteLine(handle.Value);
//}
//Console.WriteLine(odel.GetType().TypeHandle.Value);

//Console.WriteLine(FieldExt.GetFieldOffset(typeof(DelegateStub).GetField("Method")));
//Console.WriteLine(offsetMethod);

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
