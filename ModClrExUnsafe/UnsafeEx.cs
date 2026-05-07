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
        public static ReadOnlySpan<T> AsReadOnlySpan<T>(IntPtr address, int byteLength)
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                return new ReadOnlySpan<T>(address.ToPointer(), byteLength / sizeof(T));
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }
        public static Span<byte> AsCStringSpan(this IntPtr address)
        {
            var length = StrLen(address);
            return AsSpan<byte>(address, length);
        }
        public static ReadOnlySpan<byte> AsReadOnlyCStringSpan(this IntPtr address)
        {
            var length = StrLen(address);
            return AsReadOnlySpan<byte>(address, length);
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

        public static int StrLen(Span<byte> str)
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
    }
}
