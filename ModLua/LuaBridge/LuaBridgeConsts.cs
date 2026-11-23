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

            public static readonly PCString SKEY_GC = PCString.GetOrCreate("__gc");
        }
    }
}
