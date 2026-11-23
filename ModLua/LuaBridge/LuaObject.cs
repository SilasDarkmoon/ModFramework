using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mod.Lua
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct LuaObject : IDisposable
    {
        public IntPtr TypeHandle;
        public ulong Flags;
        public IntPtr ExtraHandle;
        public IntPtr ObjectHandle;

        public static readonly int Size = Marshal.SizeOf<LuaObject>();

        public object TypeProcessor => TypeHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(TypeHandle).Target;
        public object ExtraAttached
        {
            get => ExtraHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(ExtraHandle).Target;
            set
            {
                var current = ExtraHandle;
                if (current != IntPtr.Zero)
                {
                    var gchandle = GCHandle.FromIntPtr(current);
                    if (gchandle.Target is IDisposable disp)
                    {
                        disp.Dispose();
                    }
                    gchandle.Free();
                }
                ExtraHandle = value == null ? IntPtr.Zero : GCHandle.ToIntPtr(GCHandle.Alloc(value));
            }
        }
        public object TargetObject => ObjectHandle == IntPtr.Zero ? null : GCHandle.FromIntPtr(ObjectHandle).Target;

        public LuaObject(object obj)
        {
            TypeHandle = IntPtr.Zero;
            Flags = 0;
            ExtraHandle = IntPtr.Zero;
            ObjectHandle = obj == null ? IntPtr.Zero : GCHandle.ToIntPtr(GCHandle.Alloc(obj));
        }
        public LuaObject(IntPtr typeHandle, object obj)
        {
            TypeHandle = typeHandle;
            Flags = 0;
            ExtraHandle = IntPtr.Zero;
            ObjectHandle = obj == null ? IntPtr.Zero : GCHandle.ToIntPtr(GCHandle.Alloc(obj));
        }
        public void Dispose()
        {
            // Notice: TypeHandle is global instance, should not be freed.
            //if (TypeHandle != IntPtr.Zero)
            //{
            //    GCHandle.FromIntPtr(TypeHandle).Free();
            //    TypeHandle = IntPtr.Zero;
            //}
            if (ExtraHandle != IntPtr.Zero)
            {
                var gchandle = GCHandle.FromIntPtr(ExtraHandle);
                if (gchandle.Target is IDisposable disp)
                {
                    disp.Dispose();
                }
                gchandle.Free();
                ExtraHandle = IntPtr.Zero;
            }
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
            if (ExtraHandle != IntPtr.Zero)
            {
                var gchandle = GCHandle.FromIntPtr(ExtraHandle);
                if (gchandle.Target is IDisposable disp)
                {
                    disp.Dispose();
                }
                gchandle.Free();
                ExtraHandle = IntPtr.Zero;
            }
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
