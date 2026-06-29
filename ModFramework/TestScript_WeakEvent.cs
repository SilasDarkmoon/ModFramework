using Mod;
using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

public static class TestScript_WeakEvent
{
    public static void Test()
    {
#if NET7_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif

        #region Test For WeakEvent
        TestSub1();
        TestSub2();
        #endregion
    }
    private static void TestSub1()
    {
        TestEvent += TestMethod;
        _Instance = new TestClass() { x = 10 };
        TestEvent += _Instance.TestMethod;
        var inst = new TestClass() { x = 100 };
        if (RuntimeInfo.IsNativeAOT)
        { // on NativeAOT, it is recommanded to explicitly use WeakRef.
            var winst = inst.WeakRef();
            TestEvent += y => { if (winst.Value is TestClass value) Console.WriteLine($"Lambda => {value.x - y}"); };
        }
        else
        {
            TestEvent += y => Console.WriteLine($"Lambda => {inst.x - y}");
        }
        var inst2 = new TestClass() { x = 200 };
        TestEvent += y => Console.WriteLine($"Lambda2 => {inst2.x - y}"); // When use lambda on NativeAOT, we should add closure-target's type to RD.xml (TestScript_WeakEvent+&lt;&gt;c__DisplayClass1_0)
        _TestEvent.TrigEvent(6);

        Console.WriteLine("---------------------------------------------------------------------------------------");

        TestEvent -= TestMethod;
        TestEvent -= _Instance.TestMethod;
        _TestEvent.TrigEvent(6);

        Console.WriteLine("---------------------------------------------------------------------------------------");

        TestEvent += TestMethod;
        TestEvent += _Instance.TestMethod;
        _TestEvent.TrigEvent(6);

        Console.WriteLine("-------------------------------Waiting For GC------------------------------------------");
        inst = null;
        _Instance = null;
        System.Threading.Thread.Sleep(2000);
    }
    private static void TestSub2()
    {
        _Instance = null;
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        System.GC.Collect(2, GCCollectionMode.Forced, true, true);
        System.GC.WaitForFullGCComplete();
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        System.GC.Collect(2, GCCollectionMode.Forced, true, true);
        System.GC.WaitForFullGCComplete();
        System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
        System.GC.Collect(2, GCCollectionMode.Forced, true, true);
        System.GC.WaitForFullGCComplete();
        System.Threading.Thread.Sleep(2000);
        Console.WriteLine("---------------------------------------------------------------------------------------");

        _TestEvent.TrigEvent(6);
    }

    private static WeakDelegate.WeakEvent<VoidReturn, int> _TestEvent = new WeakDelegate.WeakEvent<VoidReturn, int>();
    public delegate void TestEventHandler(int x);
    public static event TestEventHandler TestEvent
    {
        add
        {
            _TestEvent.AddListener(value);
        }
        remove
        {
            _TestEvent.RemoveListener(value);
        }
    }

    private static TestClass _Instance;

    public static void TestMethod(int y)
    {
        Console.WriteLine($"{nameof(TestScript_WeakEvent)}.{nameof(TestMethod)} => {y * y}");
    }

    public class TestClass
    {
        public int x;
        public void TestMethod(int y)
        {
            Console.WriteLine($"{nameof(TestClass)}.{nameof(TestMethod)} => {x * y}");
        }
    }
}
