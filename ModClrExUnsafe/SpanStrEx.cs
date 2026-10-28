using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static Span<T> AsSpan<T>(IntPtr address, int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                return new Span<T>(address.ToPointer(), byteLength / sizeof(T));
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static Span<T> AsSpan<T>(ref T r, int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                Span<T> span;
                for (; ; )
                {
                    var rr = RawRef.Of(ref r);
                    var addrprev = rr.Address;
                    span = new Span<T>(addrprev.ToPointer(), byteLength / sizeof(T));
                    rr.SetRef(ref r);
                    var addrpost = rr.Address;
                    if (addrprev == addrpost)
                    {
                        return span;
                    }
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(IntPtr address, int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                return new ReadOnlySpan<T>(address.ToPointer(), byteLength / sizeof(T));
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(ref T r, int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                ReadOnlySpan<T> span;
                for (; ; )
                {
                    var rr = RawRef.Of(ref r);
                    var addrprev = rr.Address;
                    span = new ReadOnlySpan<T>(addrprev.ToPointer(), byteLength / sizeof(T));
                    rr.SetRef(ref r);
                    var addrpost = rr.Address;
                    if (addrprev == addrpost)
                    {
                        return span;
                    }
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static Span<byte> AsCStringSpan(this IntPtr address)
        {
            var length = StrLen(address);
            return AsSpan<byte>(address, length);
        }
        public static Span<byte> AsCStringSpan(ref byte address)
        {
            var length = StrLen(ref address);
            return AsSpan<byte>(ref address, length);
        }
        public static ReadOnlySpan<byte> AsReadOnlyCStringSpan(this IntPtr address)
        {
            var length = StrLen(address);
            return AsReadOnlySpan<byte>(address, length);
        }
        public static ReadOnlySpan<byte> AsReadOnlyCStringSpan(ref byte address)
        {
            var length = StrLen(ref address);
            return AsReadOnlySpan<byte>(ref address, length);
        }
        public static IntPtr GetAddress<T>(this Span<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return (IntPtr)ptr;
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static (IntPtr address, int byteLength) GetAddressAndLength<T>(this Span<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return ((IntPtr)ptr, span.Length * sizeof(T));
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static IntPtr GetAddress<T>(this ReadOnlySpan<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return (IntPtr)ptr;
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static (IntPtr address, int byteLength) GetAddressAndLength<T>(this ReadOnlySpan<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return ((IntPtr)ptr, span.Length * sizeof(T));
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }

        public static IntPtr GetAddress<T>(ref T r)
        {
            return RawRef.Of(ref r).Address;
        }
        public static ref T ToRef<T>(IntPtr addr)
        {
            return ref RawRef.Of(addr).GetRef<T>();
        }

        public static bool IsNull<T>(this Span<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                Span<T>* pspan = &span;
                return *(IntPtr*)(void*)pspan == IntPtr.Zero;
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ref T GetRef<T>(this Span<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return ref RawRef.Of((IntPtr)ptr).GetRef<T>();
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ref T GetRefAndLength<T>(this Span<T> span, out int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                byteLength = span.Length * sizeof(T);
                fixed (T* ptr = span)
                {
                    return ref RawRef.Of((IntPtr)ptr).GetRef<T>();
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static bool IsNull<T>(this ReadOnlySpan<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                ReadOnlySpan<T>* pspan = &span;
                return *(IntPtr*)(void*)pspan == IntPtr.Zero;
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ref T GetRef<T>(this ReadOnlySpan<T> span)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                fixed (T* ptr = span)
                {
                    return ref RawRef.Of((IntPtr)ptr).GetRef<T>();
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static ref T GetRefAndLength<T>(this ReadOnlySpan<T> span, out int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                byteLength = span.Length * sizeof(T);
                fixed (T* ptr = span)
                {
                    return ref RawRef.Of((IntPtr)ptr).GetRef<T>();
                }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }

        public static int StrLen(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                return 0;
            }
            unsafe
            {
                var ptr = (byte*)address.ToPointer();
                for (int i = 0; ; ++i)
                {
                    if (ptr[i] == 0)
                    {
                        return i;
                    }
                }
            }
        }
        public static int StrLen(ref byte r)
        {
            if (Ref.IsEmpty(ref r))
            {
                return 0;
            }
            unsafe
            {
                fixed (byte* ptr = &r)
                {
                    for (int i = 0; ; ++i)
                    {
                        if (ptr[i] == 0)
                        {
                            return i;
                        }
                    }
                }
            }
        }

        public static int StrLen(IntPtr address, int maxsize)
        {
            if (address == IntPtr.Zero)
            {
                return 0;
            }
            unsafe
            {
                var ptr = (byte*)address.ToPointer();
                for (int i = 0; i < maxsize; ++i)
                {
                    if (ptr[i] == 0)
                    {
                        return i;
                    }
                }
                return maxsize;
            }
        }
        public static int StrLen(ref byte r, int maxsize)
        {
            if (Ref.IsEmpty(ref r))
            {
                return 0;
            }
            unsafe
            {
                fixed (byte* ptr = &r)
                {
                    for (int i = 0; i < maxsize; ++i)
                    {
                        if (ptr[i] == 0)
                        {
                            return i;
                        }
                    }
                    return maxsize;
                }
            }
        }

        public static int StrLen(ReadOnlySpan<byte> str)
        {
            for (int i = 0; i < str.Length; ++i)
            {
                if (str[i] == 0)
                {
                    return i;
                }
            }
            return str.Length;
        }

        public static Span<byte> Append(this Span<byte> span, byte value, bool throwOnError = false)
        {
            var len = StrLen(span);
            if (len >= span.Length)
            {
                if (throwOnError)
                {
                    throw new ArgumentException("Span is full", nameof(span));
                }
                return span;
            }
            span[len] = value;
            return span;
        }
        public static Span<byte> Append(this Span<byte> span, ReadOnlySpan<byte> value, bool throwOnError = false)
        {
            var len = StrLen(span);
            if (len + value.Length > span.Length)
            {
                if (throwOnError)
                {
                    throw new ArgumentException("Span has insufficient space", nameof(span));
                }
                return span;
            }
            value.CopyTo(span.Slice(len));
            return span;
        }
    }
}
