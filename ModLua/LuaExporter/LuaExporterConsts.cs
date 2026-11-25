using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.Lua
{
    public static partial class LuaLib
    {
        public static partial class Consts
        {
            internal static readonly IntPtr _UKEY_MAIN_ATTACHMENT = new IntPtr(1025);
            internal static readonly IntPtr _UKEY_OBJECT_POOL = new IntPtr(1026);
            internal static readonly IntPtr _UKEY_TYPE_HANDLE = new IntPtr(1041);
            internal static readonly IntPtr _UKEY_TARGET_OBJECT = new IntPtr(1042);

            public static readonly PCString SKEY_GC = PCString.GetOrCreate("__gc");
            public static readonly PCString SKEY_MODE = PCString.GetOrCreate("__mode");
            public static readonly PCString STR_V = PCString.GetOrCreate("v");
        }
    }
}
