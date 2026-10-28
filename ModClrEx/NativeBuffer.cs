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
        public void Resize(int size)
        {
            if (_Ptr == IntPtr.Zero)
            {
                throw new ObjectDisposedException(nameof(NativeBuffer));
            }
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than or equal to 0.");
            }
            _Ptr = System.Runtime.InteropServices.Marshal.ReAllocHGlobal(_Ptr, (IntPtr)size);
            _Size = size;
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

        public bool IsNull => _Ptr == IntPtr.Zero;
        public bool IsValid => _Ptr != IntPtr.Zero && _Size > 0;
    }
    public struct NativeOrStackBuffer : IDisposable
    {
        private NativeBuffer _NativeBuffer;
        private NativeBuffer _StackBuffer;
        public bool IsNative => !_NativeBuffer.IsNull;

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
    public struct NativeMapper<T> : IDisposable
    {
        private NativeBuffer _Buffer;
        public ref T Inner => ref UnsafeEx.ToRef<T>(_Buffer.Ptr);
        public T Value
        {
            get => _Buffer.IsNull ? default(T) : Inner;
            set => Init(value);
        }
        public bool IsValid => !_Buffer.IsNull;
        public void Init()
        {
            if (_Buffer.IsNull)
            {
                _Buffer = new NativeBuffer(UnsafeEx.Size<T>());
            }
        }
        public void Init(T value)
        {
            Init();
            Inner = value;
        }
        public NativeMapper(T value)
        {
            _Buffer = new NativeBuffer(UnsafeEx.Size<T>());
            Inner = value;
        }
        public void Dispose()
        {
            _Buffer.Dispose();
        }
    }
    public class NativeBox<T> : IDisposable
    {
        private NativeMapper<T> _Mapper;
        public ref T Inner => ref _Mapper.Inner;
        public T Value
        {
            get => _Mapper.Value;
            set
            {
                if (_Mapper.IsValid)
                {
                    _Mapper.Value = value;
                }
            }
        }
        public bool IsValid => _Mapper.IsValid;
        public NativeBox()
        {
            _Mapper = new NativeMapper<T>();
            _Mapper.Init();
        }
        public void Dispose()
        {
            _Mapper.Dispose();
            GC.SuppressFinalize(this);
        }
        ~NativeBox()
        {
            _Mapper.Dispose();
        }
    }
}
