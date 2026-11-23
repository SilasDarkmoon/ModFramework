using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    public static partial class LuaLib
    {
        public static IntPtr Indicator(this lua.State l)
        {
            l.pushvalue(lua.REGISTRYINDEX);
            var p = l.topointer(-1);
            l.pop(1);
            return p;
        }

        public struct LuaScope : IDisposable
        {
            private lua.State _L;
            private int _Top;

            public lua.State L => _L;
            public int Top => _Top;

            public LuaScope(lua.State l)
            {
                _L = l;
                _Top = l.gettop();
            }
            public void Dispose()
            {
                if (_L.IsValid)
                {
                    _L.settop(_Top);
                }
            }
        }
        public static LuaScope Scope(this lua.State l)
        {
            return new LuaScope(l);
        }

        internal static LuaObject GetUserData(this lua.State l, int index)
        {
            IntPtr addr = l.touserdata(index);
            return RawRef.Of(addr).GetValue<LuaObject>();
        }
        public static T GetUserData<T>(this lua.State l, int index)
        {
            var lobj = l.GetUserData(index);
            if (lobj.TargetObject is T rv)
            {
                return rv;
            }
            else
            {
                return default;
            }
        }
    }
}
