using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    internal class LuaObjectPool
    {
        public readonly LuaGlobalAttachment AttachmentManager;
        public LuaObjectPool(LuaGlobalAttachment attachman)
        {
            AttachmentManager = attachman;
        }

        private Dictionary<IntPtr, object> _Pool = new Dictionary<IntPtr, object>();
        private Dictionary<object, IntPtr> _ReversePool = new Dictionary<object, IntPtr>();

        public void Reg(IntPtr id, object obj)
        {
            Unreg(id);
            Unreg(obj);
            _Pool[id] = obj;
            _ReversePool[obj] = id;
        }
        public void Unreg(IntPtr id)
        {
            if (_Pool.TryGetValue(id, out var obj))
            {
                _Pool.Remove(id);
                if (obj != null)
                {
                    _ReversePool.Remove(obj);
                }
            }
        }
        public void Unreg(object obj)
        {
            if (_ReversePool.TryGetValue(obj, out var id))
            {
                _ReversePool.Remove(obj);
                _Pool.Remove(id);
            }
        }
        internal object TryGet(IntPtr id)
        {
            _Pool.TryGetValue(id, out var obj);
            return obj;
        }
        public object TryGetPooled(lua.State l, int index, out IntPtr id)
        {
            id = l.topointer(index);
            return TryGet(id);
        }
        internal void PushPoolTable(lua.State l)
        {
            l.pushlightuserdata(lua.Consts._UKEY_OBJECT_POOL);
            l.rawget(lua.REGISTRYINDEX);
            if (!l.istable(-1))
            {
                l.pop(1);
                l.newtable();
                l.pushlightuserdata(lua.Consts._UKEY_OBJECT_POOL);
                l.pushvalue(-2);
                l.newtable();
                l.pushlstring(lua.Consts.SKEY_MODE.CString);
                l.pushlstring(lua.Consts.STR_V.CString);
                l.rawset(-3);
                l.setmetatable(-2);
                l.rawset(lua.REGISTRYINDEX);
            }
        }
        public IntPtr TryPushPooled(lua.State l, object obj)
        {
            if (_ReversePool.TryGetValue(obj, out var id))
            {
                PushPoolTable(l);
                l.pushlightuserdata(id);
                l.rawget(-2);
                if (l.isnoneornil(-1))
                {
                    _ReversePool.Remove(obj);
                    _Pool.Remove(id);
                    l.pop(2);
                    return IntPtr.Zero;
                }
                else
                {
                    l.remove(-2);
                    return id;
                }
            }
            return IntPtr.Zero;
        }
        public void Reg(lua.State l, object obj, int index)
        {
            using (l.Scope())
            {
                var id = l.topointer(index);
                Reg(id, obj);
                var nindex = l.absindex(index);
                PushPoolTable(l);
                l.pushlightuserdata(id);
                l.pushvalue(nindex);
                l.rawset(-3);
            }
        }
    }
}
