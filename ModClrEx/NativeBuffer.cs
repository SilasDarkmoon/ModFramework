using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Mod
{
    public struct NativeBuffer : IDisposable
    {
        private IntPtr _Ptr;
        private int _Size;
        public IntPtr Ptr => _Ptr;
        public int Size => _Size;

        public NativeBuffer(int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than or equal to 0.");
            }
            _Ptr = System.Runtime.InteropServices.Marshal.AllocHGlobal(size);
            _Size = size;
        }
        public NativeBuffer(IntPtr ptr, int size)
        {
            _Ptr = ptr;
            _Size = size;
        }
        public void Dispose()
        {
            if (_Ptr != IntPtr.Zero)
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(_Ptr);
                _Ptr = IntPtr.Zero;
                _Size = 0;
            }
        }

        public Span<T> ToSpan<T>()
        {
            return UnsafeEx.AsSpan<T>(_Ptr, _Size);
        }
        public static implicit operator IntPtr(NativeBuffer buffer)
        {
            return buffer._Ptr;
        }
        public static implicit operator Span<byte>(NativeBuffer buffer)
        {
            return buffer.ToSpan<byte>();
        }
    }
    public struct NativeOrStackBuffer : IDisposable
    {
        private NativeBuffer _NativeBuffer;
        private NativeBuffer _StackBuffer;
        public bool IsNative => _StackBuffer.Ptr == IntPtr.Zero;

        public NativeOrStackBuffer(int size)
        {
            _NativeBuffer = new NativeBuffer(size);
            _StackBuffer = default;
        }
        public NativeOrStackBuffer(Span<byte> stackBuffer)
        {
            _NativeBuffer = default;
            _StackBuffer = new NativeBuffer(UnsafeEx.GetAddress(stackBuffer), stackBuffer.Length);
        }

        public void Dispose()
        {
            if (IsNative)
            {
                _NativeBuffer.Dispose();
            }
            // stack buffer does not need to be disposed
        }

        public Span<T> ToSpan<T>()
        {
            if (IsNative)
                return _NativeBuffer.ToSpan<T>();
            else
                return _StackBuffer.ToSpan<T>();
        }
        public static implicit operator IntPtr(NativeOrStackBuffer buffer)
        {
            if (buffer.IsNative)
                return buffer._NativeBuffer;
            else
                return buffer._StackBuffer;
        }
        public static implicit operator Span<byte>(NativeOrStackBuffer buffer)
        {
            return buffer.ToSpan<byte>();
        }
    }
}
