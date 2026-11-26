using System;
using System.Collections.Generic;
using System.Text;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    internal static partial class LuaExporter
    {
        public static readonly lua.CFunction LuaFuncCommonGC = new lua.CFunction(LuaMetaCommonGC);
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        public static int LuaMetaCommonGC(lua.State l)
        {
            var lobj = l.GetUserData(1);
            var attach = lobj.GetTargetAndDispose<IDisposable>();
            attach?.Dispose();
            return 0;
        }
    }
}
