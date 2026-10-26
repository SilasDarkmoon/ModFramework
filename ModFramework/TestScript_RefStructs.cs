using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;

public static class TestScript_RefStructs
{
    private static IntPtr PypassIntPtr(IntPtr ptr) => ptr;
    private delegate ref byte ConvertSpanToRefByteDel(ref Span<int> span);

    public static void Test()
    {
        Span<int> span = stackalloc int[10];
        IntPtr ptr;
        unsafe { ptr = (IntPtr)(void*)&span; }
        Console.WriteLine($"Span<int> address: {ptr.ToString("X")}");

        Func<IntPtr, IntPtr> funcpypass = PypassIntPtr;
        var del = UnsafeEx.CreateDelegateUnsafe<ConvertSpanToRefByteDel>(funcpypass.Method);
        ref byte r = ref del(ref span);
        Console.WriteLine($"Ref<byte> address: {RawRef.Of(ref r).Address.ToString("X")}");
    }
}
