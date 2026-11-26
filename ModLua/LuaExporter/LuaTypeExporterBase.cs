using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Mod.Lua
{
    public interface ILuaHandle
    {
        IntPtr R { get; }
    }
    public interface ILuaTypeRef : ILuaHandle
    {
        Type T { get; }
        // For lua to reg type metatable. Normally set to T.FullName. Set to null to avoid auto reg metatable.
        PCString MetaName { get; }
    }
    public abstract class SelfHandled : ILuaHandle
    {
        protected IntPtr _R;
        public IntPtr R => _R;
        protected internal SelfHandled()
        {
            _R = GCHandle.ToIntPtr(GCHandle.Alloc(this));
        }
    }

    public partial class LuaTypeExporter
    {

    }
}
