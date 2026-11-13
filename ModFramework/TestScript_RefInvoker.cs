using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;

public static class TestScript_RefInvoker
{
    public static void Test()
    {
        int a = 42;
        Action<LocalRef<int>> write = ra =>
        {
            ra.Value = 43;
        };
        write.RefInvoker().Invoke(in a);
        Console.WriteLine($"a = {a}"); // 输出: a = 43

        Func<LocalRef<int>, int> dbl = ra => ra.Value * 2;
        var result = dbl.RefInvoker().Invoke(in a);
        Console.WriteLine($"result = {result}"); // 输出: result = 86

        RefFunc<LocalRef<int>, int> getref = ra => ref ra.GetRef();
        var inv = getref.RefInvoker();
        ref var ra2 = ref ((IFreeInvokableFunc1<int>)inv).Invoke(out _, in a);
        ++ra2;
        Console.WriteLine($"ra2 = {ra2}"); // 输出: ra2 = 44
        Console.WriteLine($"a = {a}"); // 输出: a = 44
    }
}
