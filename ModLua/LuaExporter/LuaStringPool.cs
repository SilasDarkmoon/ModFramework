using Mod.LowLevel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using lua = Mod.Lua.LuaLib;

namespace Mod.Lua
{
    internal class LuaStringPool
    {
        public readonly LuaGlobalAttachment AttachmentManager;
        public LuaStringPool(LuaGlobalAttachment attachman)
        {
            AttachmentManager = attachman;
        }

        public void PushString(lua.State l, PCString pcstring)
        {
            if (AttachmentManager.ObjectPool.TryPushPooled(l, pcstring) == IntPtr.Zero)
            {
                l.pushlstring(pcstring.CString);
                AttachmentManager.ObjectPool.Reg(l, pcstring, -1);
            }
        }

        private class PooledStringItem : IDisposable
        {
            public LuaStringPool Pool;
            public IntPtr Key;
            public WeakReference<string> Value;

            public PooledStringItem(LuaStringPool pool, IntPtr key, WeakReference<string> value)
            {
                Pool = pool;
                Key = key;
                Value = value;
            }

            private void Dispose(bool directDispose)
            {
                if (directDispose)
                {
                    Pool._ReversePool.Remove(Key);
                }
                else
                {
                    Pool._ToBeCleanedUp.Enqueue(this);
                    Pool._NeedCleanup = true;
                }
            }
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }
            ~PooledStringItem()
            {
                Dispose(false);
            }
        }
        private ConditionalWeakTable<string, PooledStringItem> _StringPool = new ConditionalWeakTable<string, PooledStringItem>();
        private Dictionary<IntPtr, WeakReference<string>> _ReversePool = new Dictionary<IntPtr, WeakReference<string>>();
        private bool _NeedCleanup = false;
        private ConcurrentQueue<PooledStringItem> _ToBeCleanedUp = new ConcurrentQueue<PooledStringItem>();
        // Push an string with cache. Only for large strings that will transfer between lua-C# frequently. Normally, the cache is weak.
        public void PushString(lua.State l, string str)
        {
            if (PCString.TryGet(str) is PCString pcstring)
            {
                PushString(l, pcstring);
            }
            else
            {
                if (_NeedCleanup)
                {
                    _NeedCleanup = false;
                    while (_ToBeCleanedUp.TryDequeue(out var deaditem))
                    {
                        if (_ReversePool.TryGetValue(deaditem.Key, out var currentitem) && deaditem.Value == currentitem)
                        {
                            _ReversePool.Remove(deaditem.Key);
                        }
                    }
                }
                if (_StringPool.TryGetValue(str, out var pobj))
                {
                    var id = pobj.Key;
                    if (!pobj.Value.TryGetTarget(out _))
                    {
                        pobj.Value.SetTarget(str);
                    }
                    AttachmentManager.ObjectPool.PushPoolTable(l);
                    l.pushlightuserdata(id);
                    l.rawget(-2);
                    if (l.isstring(-1))
                    {
                        l.remove(-2);
                        return;
                    }
                    else
                    {
                        l.pop(1);
                        l.pushlstringn(new CStringSpan(str));
                        l.pushlightuserdata(id);
                        l.pushvalue(-2);
                        l.rawset(-4);
                        l.remove(-2);
                        return;
                    }
                }
                else
                {
                    AttachmentManager.ObjectPool.PushPoolTable(l);
                    l.pushlstringn(new CStringSpan(str));
                    var id = l.topointer(-1);
                    l.pushlightuserdata(id);
                    l.pushvalue(-2);
                    l.rawset(-4);
                    PooledStringItem item = new PooledStringItem(this, id, new WeakReference<string>(str));
                    _StringPool.Remove(str);
                    _StringPool.Add(str, item);
                    _ReversePool[id] = item.Value;
                    l.remove(-2);
                    return;
                }
            }
        }
        // Only for large strings that will transfer between lua-C# frequently.
        public string GetString(lua.State l, int index)
        {
            using (l.Scope())
            {
                var realindex = l.absindex(index);
                var ltype = l.type(realindex);
                if (ltype == lua.TSTRING)
                {
                }
                else if (ltype == lua.TTABLE)
                {
                    l.pushlightuserdata(lua.Consts._UKEY_TARGET_OBJECT);
                    l.rawget(realindex);
                    if (l.type(-1) != lua.TSTRING)
                    { // Do we need recursively check whether it is a table?
                        return null;
                    }
                    realindex = l.absindex(-1);
                }
                else
                {
                    return null;
                }
                var id = l.topointer(realindex);
                if (AttachmentManager.ObjectPool.TryGet(id) is object strobj)
                {
                    return strobj.ToString();
                }
                if (_ReversePool.TryGetValue(id, out var wr) && wr.TryGetTarget(out var str))
                {
                    return str;
                }
                str = l.tolstring(realindex).ToString();

                AttachmentManager.ObjectPool.PushPoolTable(l);
                l.pushlightuserdata(id);
                l.pushvalue(realindex);
                l.rawset(-3);

                PooledStringItem item = new PooledStringItem(this, id, new WeakReference<string>(str));
                _StringPool.Remove(str);
                _StringPool.Add(str, item);
                _ReversePool[id] = item.Value;
                return str;
            }
        }
        public void MarkPooled(lua.State l, int index)
        {
            var realindex = l.absindex(index);
            l.newtable();
            l.pushlightuserdata(lua.Consts._UKEY_TARGET_OBJECT);
            l.pushvalue(realindex);
            l.rawset(-3);
        }
        // Bind a lua-string with a C# String object (as an userdata).
        public void PoolLongTerm(lua.State l, int index, string str)
        {

        }
    }
}
