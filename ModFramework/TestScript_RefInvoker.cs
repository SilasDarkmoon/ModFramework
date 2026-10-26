using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;

public static class TestScript_RefInvoker
{
    public static void Test()
    {
        int a = 42;
        UnsafeEx.RefInvoke(in a, ra =>
        {
            ra.Value = 43;
        });
        Console.WriteLine($"a = {a}"); // 输出: a = 43

        var result = UnsafeEx.RefInvoke(in a, ra =>
        {
            return ra.Value * 2;
        });
        Console.WriteLine($"result = {result}"); // 输出: result = 86

        ref var ra2 = ref UnsafeEx.RefInvoke(in a, ra => ref ra.GetRef());
        ++ra2;
        Console.WriteLine($"ra2 = {ra2}"); // 输出: ra2 = 44
        Console.WriteLine($"a = {a}"); // 输出: a = 44
    }
}
