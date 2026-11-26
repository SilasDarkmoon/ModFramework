using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    internal partial class LuaGlobalAttachment : IDisposable
    {
        [ThreadStatic] private static Dictionary<IntPtr, LuaGlobalAttachment> _Cache;

        public static LuaGlobalAttachment Get(lua.State l)
        {
            Dictionary<IntPtr, LuaGlobalAttachment> cache;
            if (_Cache == null)
            {
                cache = _Cache = new Dictionary<IntPtr, LuaGlobalAttachment>();
            }
            else
            {
                cache = _Cache;
            }

            using (l.Scope())
            {
                l.pushvalue(lua.REGISTRYINDEX);
                var id = l.topointer(-1);
                if (cache.TryGetValue(id, out LuaGlobalAttachment attachment))
                {
                    if (attachment._IsClosed)
                    {
                        _Cache.Remove(id);
                    }
                    else
                    {
                        return attachment;
                    }
                }

                var ukey = lua.Consts._UKEY_MAIN_ATTACHMENT;
                l.pushlightuserdata(ukey);
                l.rawget(-2);
                if (l.type(-1) == lua.TUSERDATA)
                {
                    attachment = l.GetUserData<LuaGlobalAttachment>(-1);
                }
                if (attachment == null || attachment._IsClosed)
                {
                    attachment = new LuaGlobalAttachment(l);
                    l.pop(1);
                    l.pushlightuserdata(ukey);
                    var attachaddr = l.newuserdata(LuaUserData.Size);
                    var lobj = new LuaUserData(attachment);
                    RawRef.Of(attachaddr).GetRef<LuaUserData>() = lobj;
                    l.newtable();
                    l.pushlstringn(lua.Consts.SKEY_GC.CString);
                    l.pushcfunction(LuaExporter.LuaFuncCommonGC);
                    l.rawset(-3);
                    l.setmetatable(-2);
                    l.rawset(-3);
                }
                cache[id] = attachment;
                return attachment;
            }
        }

        private lua.State _L;
        private bool _IsClosed;
        public bool IsClosed => _IsClosed;
        public LuaGlobalAttachment(lua.State l)
        {
            _L = l;
            ObjectPool = new LuaObjectPool(this);
            StringPool = new LuaStringPool(this);
        }

        public void Dispose()
        {
            if (!_IsClosed)
            {
                _IsClosed = true;

                var id = _L.Indicator();
                var cache = _Cache;
                if (cache != null)
                {
                    if (cache.TryGetValue(id, out var old) && old == this)
                    {
                        cache.Remove(id);
                    }
                }
            }
        }

        public readonly LuaObjectPool ObjectPool;
        public readonly LuaStringPool StringPool;
    }
}
