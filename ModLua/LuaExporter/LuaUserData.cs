using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mod.Lua
{
    [Flags]
    internal enum LuaUserDataFlags
    {
        ValueType = 0,
        Object = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LuaUserDataHeader
    {
        public int UserDataSize;
        public uint Flags;
        public IntPtr TypeHandle;

        public static readonly int Size = Marshal.SizeOf<LuaUserDataHeader>();
        public LuaUserDataHeader(int size)
        {
            UserDataSize = size > 0 ? size : Size;
            Flags = 0;
            TypeHandle = default;
        }

        public ILuaTypeRef TypeHandler => TypeHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(TypeHandle).Target as ILuaTypeRef;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LuaUserDataValueType<T>
    {
        public LuaUserDataHeader Header;
        public T Data;

        public static readonly int Size = LuaUserDataHeader.Size + UnsafeEx.Size<T>();
        public LuaUserDataValueType(T value)
        {
            Header = new LuaUserDataHeader(Size) { Flags = (uint)LuaUserDataFlags.ValueType };
            Data = value;
        }

        public ILuaTypeRef TypeHandler => Header.TypeHandler;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal struct LuaUserData : IDisposable
    {
        public LuaUserDataHeader Header;
        public IntPtr ObjectHandle;

        public static readonly int Size = LuaUserDataHeader.Size + IntPtr.Size;

        public ILuaTypeRef TypeHandler => Header.TypeHandler;
        public object TargetObject => ObjectHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(ObjectHandle).Target;

        public LuaUserData(object obj)
        {
            Header = new LuaUserDataHeader(Size) { Flags = (uint)LuaUserDataFlags.Object };
            ObjectHandle = obj == null ? IntPtr.Zero : GCHandle.ToIntPtr(GCHandle.Alloc(obj));
        }
        public LuaUserData(IntPtr typeHandle, object obj) : this(obj)
        {
            Header.TypeHandle = typeHandle;
        }
        public void Dispose()
        {
            if (ObjectHandle != IntPtr.Zero)
            {
                var gchandle = GCHandle.FromIntPtr(ObjectHandle);
                // Notice: the object's dispose should be managed outside the lua wrapper.
                //if (gchandle.Target is IDisposable disp)
                //{
                //    disp.Dispose();
                //}
                gchandle.Free();
                ObjectHandle = IntPtr.Zero;
            }
        }
        public object GetTargetAndDispose()
        {
            object target = null;
            if (ObjectHandle != IntPtr.Zero)
            {
                var gchandle = GCHandle.FromIntPtr(ObjectHandle);
                target = gchandle.Target;
                gchandle.Free();
                ObjectHandle = IntPtr.Zero;
            }
            return target;
        }
        public T GetTargetAndDispose<T>()
        {
            if (GetTargetAndDispose() is T rv)
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
