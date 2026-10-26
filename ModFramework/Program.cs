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

class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectHeader
    {
        public IntPtr TypeHandle;
        public IntPtr SyncAddress;
    }

    public static void Main()
    {
        //TestScript.Test();
        //TestScript_UnsafeInstanceDelegate.Test();
        //TestScript_DelegateCurry.Test();
        //TestScript_WeakEvent.Test();
        //TestScript_RefStructs.Test();
        TestScript_RefInvoker.Test();
    }
}
