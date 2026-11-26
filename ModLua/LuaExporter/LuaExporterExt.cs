using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    public static partial class LuaLib
    {
        internal static ref LuaUserDataHeader GetUserDataHeader(this lua.State l, int index)
        {
            IntPtr addr = l.touserdata(index);
            return ref RawRef.Of(addr).GetRef<LuaUserDataHeader>();
        }

        internal static LuaUserData GetUserData(this lua.State l, int index)
        {
            ref var header = ref GetUserDataHeader(l, index);
            if ((header.Flags & (uint)LuaUserDataFlags.Object) != 0)
            {
                return RawRef.Of(ref header).GetValue<LuaUserData>();
            }
            else
            {
                return default;
            }
        }
        internal static void GetUserDataValueType<T>(this lua.State l, int index, out LuaUserDataValueType<T> ud)
        {
            ref var header = ref GetUserDataHeader(l, index);
            if ((header.Flags & (uint)LuaUserDataFlags.Object) == 0)
            {
                if (LuaUserDataValueType<T>.Size == header.UserDataSize)
                {
                    ud = RawRef.Of(ref header).GetValue<LuaUserDataValueType<T>>();
                    return;
                }
            }
            ud = default;
        }
        internal static LuaUserDataValueType<T> GetUserDataValueType<T>(this lua.State l, int index)
        {
            GetUserDataValueType<T>(l, index, out var ud);
            return ud;
        }
        public static T GetUserData<T>(this lua.State l, int index)
        {
            ref var header = ref GetUserDataHeader(l, index);
            if ((header.Flags & (uint)LuaUserDataFlags.Object) != 0)
            {
                var lobj = RawRef.Of(ref header).GetValue<LuaUserData>();
                if (lobj.TargetObject is T rv)
                {
                    return rv;
                }
                else
                {
                    return default;
                }
            }
            else
            {
                if (LuaUserDataValueType<T>.Size == header.UserDataSize)
                {
                    var ud = RawRef.Of(ref header).GetValue<LuaUserDataValueType<T>>();
                    return ud.Data;
                }
                else
                {
                    return default;
                }
            }
        }
        public static ILuaTypeRef GetUserDataTypeHandler(this lua.State l, int index)
        {
            var header = GetUserDataHeader(l, index);
            return header.TypeHandler;
        }
        public static Type GetUserDataType(this lua.State l, int index)
        {
            return GetUserDataTypeHandler(l, index)?.T;
        }

        public static void PushUserData(this lua.State l, object obj, ILuaTypeRef typehandler)
        {

        }
    }
}
