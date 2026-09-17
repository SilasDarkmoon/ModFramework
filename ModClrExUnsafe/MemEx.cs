using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        private unsafe struct MemCopyHelper1024
        {
            public fixed byte Buffer[1024];
        }
        private unsafe struct MemCopyHelper512
        {
            public fixed byte Buffer[512];
        }
        private unsafe struct MemCopyHelper256
        {
            public fixed byte Buffer[256];
        }
        private unsafe struct MemCopyHelper128
        {
            public fixed byte Buffer[128];
        }
        private unsafe struct MemCopyHelper64
        {
            public fixed byte Buffer[64];
        }
        private unsafe struct MemCopyHelper32
        {
            public fixed byte Buffer[32];
        }
        private unsafe struct MemCopyHelper16
        {
            public fixed byte Buffer[16];
        }
        private unsafe struct MemCopyHelper7
        {
            public fixed byte Buffer[7];
        }
        private unsafe struct MemCopyHelper6
        {
            public fixed byte Buffer[6];
        }
        private unsafe struct MemCopyHelper5
        {
            public fixed byte Buffer[5];
        }
        private unsafe struct MemCopyHelper4
        {
            public fixed byte Buffer[4];
        }
        private unsafe struct MemCopyHelper3
        {
            public fixed byte Buffer[3];
        }
        private unsafe struct MemCopyHelper2
        {
            public fixed byte Buffer[2];
        }
        private unsafe struct MemCopyHelper1
        {
            public fixed byte Buffer[1];
        }

        public static void MemCopy(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            int offset = (int)OffsetBetween(ref dst, ref src);
            if (srcStart + cnt > offset + dstStart)
            {
                MemCopyRight1024(ref dst, dstStart, ref src, srcStart, cnt);
            }
            else
            {
                MemCopy1024(ref dst, dstStart, ref src, srcStart, cnt);
            }
        }
        private static void MemCopy1024(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 1024)
            {
                ref MemCopyHelper1024 dstBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref dst, dstStart);
                ref MemCopyHelper1024 srcBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 1024;
                srcStart += 1024;
                cnt -= 1024;
            }
            MemCopy512(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy512(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 512)
            {
                ref MemCopyHelper512 dstBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref dst, dstStart);
                ref MemCopyHelper512 srcBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 512;
                srcStart += 512;
                cnt -= 512;
            }
            MemCopy256(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy256(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 256)
            {
                ref MemCopyHelper256 dstBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref dst, dstStart);
                ref MemCopyHelper256 srcBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 256;
                srcStart += 256;
                cnt -= 256;
            }
            MemCopy128(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy128(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 128)
            {
                ref MemCopyHelper128 dstBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref dst, dstStart);
                ref MemCopyHelper128 srcBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 128;
                srcStart += 128;
                cnt -= 128;
            }
            MemCopy64(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy64(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 64)
            {
                ref MemCopyHelper64 dstBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref dst, dstStart);
                ref MemCopyHelper64 srcBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 64;
                srcStart += 64;
                cnt -= 64;
            }
            MemCopy32(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy32(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 32)
            {
                ref MemCopyHelper32 dstBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref dst, dstStart);
                ref MemCopyHelper32 srcBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 32;
                srcStart += 32;
                cnt -= 32;
            }
            MemCopy16(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy16(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 16)
            {
                ref MemCopyHelper16 dstBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref dst, dstStart);
                ref MemCopyHelper16 srcBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 16;
                srcStart += 16;
                cnt -= 16;
            }
            MemCopy8(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopy8(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 8)
            {
                ref ulong dstBuffer = ref GetFieldRef<byte, ulong>(ref dst, dstStart);
                ref ulong srcBuffer = ref GetFieldRef<byte, ulong>(ref src, srcStart);
                dstBuffer = srcBuffer;
                dstStart += 8;
                srcStart += 8;
                cnt -= 8;
            }
            MemCopyLE7(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyLE7<T>(ref byte dst, int dstStart, ref byte src, int srcStart) where T : struct
        {
            ref T dstBuffer = ref GetFieldRef<byte, T>(ref dst, dstStart);
            ref T srcBuffer = ref GetFieldRef<byte, T>(ref src, srcStart);
            dstBuffer = srcBuffer;
        }
        private static void MemCopyLE7(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            switch (cnt)
            {
                case 7:
                    MemCopyLE7<MemCopyHelper7>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 6:
                    MemCopyLE7<MemCopyHelper6>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 5:
                    MemCopyLE7<MemCopyHelper5>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 4:
                    MemCopyLE7<MemCopyHelper4>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 3:
                    MemCopyLE7<MemCopyHelper3>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 2:
                    MemCopyLE7<MemCopyHelper2>(ref dst, dstStart, ref src, srcStart);
                    break;
                case 1:
                    MemCopyLE7<MemCopyHelper1>(ref dst, dstStart, ref src, srcStart);
                    break;
            }
        }
        private static void MemCopyRight1024(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 1024)
            {
                ref MemCopyHelper1024 dstBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref dst, dstStart + cnt - 1024);
                ref MemCopyHelper1024 srcBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref src, srcStart + cnt - 1024);
                dstBuffer = srcBuffer;
                cnt -= 1024;
            }
            MemCopyRight512(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight512(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 512)
            {
                ref MemCopyHelper512 dstBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref dst, dstStart + cnt - 512);
                ref MemCopyHelper512 srcBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref src, srcStart + cnt - 512);
                dstBuffer = srcBuffer;
                cnt -= 512;
            }
            MemCopyRight256(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight256(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 256)
            {
                ref MemCopyHelper256 dstBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref dst, dstStart + cnt - 256);
                ref MemCopyHelper256 srcBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref src, srcStart + cnt - 256);
                dstBuffer = srcBuffer;
                cnt -= 256;
            }
            MemCopyRight128(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight128(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 128)
            {
                ref MemCopyHelper128 dstBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref dst, dstStart + cnt - 128);
                ref MemCopyHelper128 srcBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref src, srcStart + cnt - 128);
                dstBuffer = srcBuffer;
                cnt -= 128;
            }
            MemCopyRight64(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight64(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 64)
            {
                ref MemCopyHelper64 dstBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref dst, dstStart + cnt - 64);
                ref MemCopyHelper64 srcBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref src, srcStart + cnt - 64);
                dstBuffer = srcBuffer;
                cnt -= 64;
            }
            MemCopyRight32(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight32(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 32)
            {
                ref MemCopyHelper32 dstBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref dst, dstStart + cnt - 32);
                ref MemCopyHelper32 srcBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref src, srcStart + cnt - 32);
                dstBuffer = srcBuffer;
                cnt -= 32;
            }
            MemCopyRight16(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight16(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 16)
            {
                ref MemCopyHelper16 dstBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref dst, dstStart + cnt - 16);
                ref MemCopyHelper16 srcBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref src, srcStart + cnt - 16);
                dstBuffer = srcBuffer;
                cnt -= 16;
            }
            MemCopyRight8(ref dst, dstStart, ref src, srcStart, cnt);
        }
        private static void MemCopyRight8(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            while (cnt >= 8)
            {
                ref ulong dstBuffer = ref GetFieldRef<byte, ulong>(ref dst, dstStart + cnt - 8);
                ref ulong srcBuffer = ref GetFieldRef<byte, ulong>(ref src, srcStart + cnt - 8);
                dstBuffer = srcBuffer;
                cnt -= 8;
            }
            MemCopyRightLE7(ref dst, dstStart, ref src, srcStart, cnt);
        }
        // Tail segment for the backward (Right) chain. That chain is entered whenever
        // the source end runs past the destination start, so it also carries plain
        // left-shift overlaps and non-overlapping moves. Only for a right-shift
        // overlap (destination after source) does the <=7-byte struct assignment get
        // lowered by the runtime into ascending piecewise copies (measured to behave
        // like memcpy, not memmove), so only that case must be moved in reverse order
        // here. Doing so for a left-shift overlap would rewrite source bytes that have
        // not been read yet; everything else falls back to the forward LE7 path.
        private static void MemCopyRightLE7(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt)
        {
            long dstAbs = (long)OffsetBetween(ref dst, ref src) + dstStart;
            if (dstAbs > srcStart && srcStart + cnt > dstAbs)
            {
                for (int i = cnt - 1; i >= 0; i--)
                {
                    GetFieldRef<byte, byte>(ref dst, dstStart + i) = GetFieldRef<byte, byte>(ref src, srcStart + i);
                }
                return;
            }
            MemCopyLE7(ref dst, dstStart, ref src, srcStart, cnt);
        }

        public static void MemCopy(object dst, int dstStart, object src, int srcStart, int cnt)
        {
            MemCopy(ref GetHeadRef(dst), dstStart, ref GetHeadRef(src), srcStart, cnt);
        }
        public static void MemCopy<D, S>(ref D dst, int dstStart, ref S src, int srcStart, int cnt) where D : struct where S : struct
        {
            MemCopy(ref GetHeadRef(ref dst), dstStart, ref GetHeadRef(ref src), srcStart, cnt);
        }
        public static void MemCopy<S>(object dst, int dstStart, ref S src, int srcStart, int cnt) where S : struct
        {
            MemCopy(ref GetHeadRef(dst), dstStart, ref GetHeadRef(ref src), srcStart, cnt);
        }
        public static void MemCopy<D>(ref D dst, int dstStart, object src, int srcStart, int cnt) where D : struct
        {
            MemCopy(ref GetHeadRef(ref dst), dstStart, ref GetHeadRef(src), srcStart, cnt);
        }

        public static void MemCopy(object dst, int dstStart, object src, int srcStart)
        {
            MemCopy(dst, dstStart, src, srcStart, Math.Min(Size(dst) - dstStart, Size(src) - srcStart));
        }
        public static void MemCopy<D, S>(ref D dst, int dstStart, ref S src, int srcStart) where D : struct where S : struct
        {
            MemCopy(ref dst, dstStart, ref src, srcStart, Math.Min(Size<D>() - dstStart, Size<S>() - srcStart));
        }
        public static void MemCopy<S>(object dst, int dstStart, ref S src, int srcStart) where S : struct
        {
            MemCopy(dst, dstStart, ref src, srcStart, Math.Min(Size(dst) - dstStart, Size<S>() - srcStart));
        }
        public static void MemCopy<D>(ref D dst, int dstStart, object src, int srcStart) where D : struct
        {
            MemCopy(ref dst, dstStart, src, srcStart, Math.Min(Size<D>() - dstStart, Size(src) - srcStart));
        }

        public static void MemCopy(object dst, object src, int cnt)
        {
            MemCopy(dst, 0, src, 0, cnt);
        }
        public static void MemCopy<D, S>(ref D dst, ref S src, int cnt) where D : struct where S : struct
        {
            MemCopy(ref dst, 0, ref src, 0, cnt);
        }
        public static void MemCopy<S>(object dst, ref S src, int cnt) where S : struct
        {
            MemCopy(dst, 0, ref src, 0, cnt);
        }
        public static void MemCopy<D>(ref D dst, object src, int cnt) where D : struct
        {
            MemCopy(ref dst, 0, src, 0, cnt);
        }

        public static void MemCopy(object dst, object src)
        {
            MemCopy(dst, 0, src, 0, Math.Min(Size(dst), Size(src)));
        }
        public static void MemCopy<D, S>(ref D dst, ref S src) where D : struct where S : struct
        {
            MemCopy(ref dst, 0, ref src, 0, Math.Min(Size<D>(), Size<S>()));
        }
        public static void MemCopy<S>(object dst, ref S src) where S : struct
        {
            MemCopy(dst, 0, ref src, 0, Math.Min(Size(dst), Size<S>()));
        }
        public static void MemCopy<D>(ref D dst, object src) where D : struct
        {
            MemCopy(ref dst, 0, src, 0, Math.Min(Size<D>(), Size(src)));
        }

        public static void MemSet(ref byte dst, int offset, byte value, int cnt)
        {
            MemSet1024(ref dst, offset, value, cnt);
        }
        private static void MemSet1024(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper1024 valueStruct = new MemCopyHelper1024();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 1024); }
            valueSpan.Fill(value);
            while (cnt >= 1024)
            {
                ref MemCopyHelper1024 dstBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 1024;
                cnt -= 1024;
            }
            MemSet512(ref dst, offset, value, cnt);
        }
        private static void MemSet512(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper512 valueStruct = new MemCopyHelper512();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 512); }
            valueSpan.Fill(value);
            while (cnt >= 512)
            {
                ref MemCopyHelper512 dstBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 512;
                cnt -= 512;
            }
            MemSet256(ref dst, offset, value, cnt);
        }
        private static void MemSet256(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper256 valueStruct = new MemCopyHelper256();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 256); }
            valueSpan.Fill(value);
            while (cnt >= 256)
            {
                ref MemCopyHelper256 dstBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 256;
                cnt -= 256;
            }
            MemSet128(ref dst, offset, value, cnt);
        }
        private static void MemSet128(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper128 valueStruct = new MemCopyHelper128();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 128); }
            valueSpan.Fill(value);
            while (cnt >= 128)
            {
                ref MemCopyHelper128 dstBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 128;
                cnt -= 128;
            }
            MemSet64(ref dst, offset, value, cnt);
        }
        private static void MemSet64(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper64 valueStruct = new MemCopyHelper64();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 64); }
            valueSpan.Fill(value);
            while (cnt >= 64)
            {
                ref MemCopyHelper64 dstBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 64;
                cnt -= 64;
            }
            MemSet32(ref dst, offset, value, cnt);
        }
        private static void MemSet32(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper32 valueStruct = new MemCopyHelper32();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 32); }
            valueSpan.Fill(value);
            while (cnt >= 32)
            {
                ref MemCopyHelper32 dstBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 32;
                cnt -= 32;
            }
            MemSet16(ref dst, offset, value, cnt);
        }
        private static void MemSet16(ref byte dst, int offset, byte value, int cnt)
        {
            MemCopyHelper16 valueStruct = new MemCopyHelper16();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(valueStruct.Buffer, 16); }
            valueSpan.Fill(value);
            while (cnt >= 16)
            {
                ref MemCopyHelper16 dstBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 16;
                cnt -= 16;
            }
            MemSet8(ref dst, offset, value, cnt);
        }
        private static void MemSet8(ref byte dst, int offset, byte value, int cnt)
        {
            ulong valueStruct = new ulong();
            Span<byte> valueSpan;
            unsafe { valueSpan = new Span<byte>(&valueStruct, 8); }
            valueSpan.Fill(value);
            while (cnt >= 8)
            {
                ref ulong dstBuffer = ref GetFieldRef<byte, ulong>(ref dst, offset);
                dstBuffer = valueStruct;
                offset += 8;
                cnt -= 8;
            }
            MemSetLE7(ref dst, offset, value, cnt);
        }
        private static void MemSetLE7<T>(ref byte dst, int offset, byte value) where T : struct
        {
            var valueStruct = new T();
            Span<byte> valueSpan;
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            unsafe { valueSpan = new Span<byte>(&valueStruct, sizeof(T)); }
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            valueSpan.Fill(value);
            ref T dstBuffer = ref GetFieldRef<byte, T>(ref dst, offset);
            dstBuffer = valueStruct;
        }
        private static void MemSetLE7(ref byte dst, int offset, byte value, int cnt)
        {
            switch (cnt)
            {
                case 7:
                    MemSetLE7<MemCopyHelper7>(ref dst, offset, value);
                    break;
                case 6:
                    MemSetLE7<MemCopyHelper6>(ref dst, offset, value);
                    break;
                case 5:
                    MemSetLE7<MemCopyHelper5>(ref dst, offset, value);
                    break;
                case 4:
                    MemSetLE7<MemCopyHelper4>(ref dst, offset, value);
                    break;
                case 3:
                    MemSetLE7<MemCopyHelper3>(ref dst, offset, value);
                    break;
                case 2:
                    MemSetLE7<MemCopyHelper2>(ref dst, offset, value);
                    break;
                case 1:
                    MemSetLE7<MemCopyHelper1>(ref dst, offset, value);
                    break;
            }
        }

        public static void MemSet(object dst, int offset, byte value, int cnt)
        {
            MemSet(ref GetHeadRef(dst), offset, value, cnt);
        }
        public static void MemSet<D>(ref D dst, int offset, byte value, int cnt) where D : struct
        {
            MemSet(ref GetHeadRef(ref dst), offset, value, cnt);
        }

        public static void MemSet(object dst, int offset, byte value)
        {
            MemSet(dst, offset, value, Size(dst) - offset);
        }
        public static void MemSet<D>(ref D dst, int offset, byte value) where D : struct
        {
            MemSet(ref dst, offset, value, Size<D>() - offset);
        }

        public static void MemSet(object dst, byte value, int cnt)
        {
            MemSet(dst, 0, value, cnt);
        }
        public static void MemSet<D>(ref D dst, byte value, int cnt) where D : struct
        {
            MemSet(ref dst, 0, value, cnt);
        }

        public static void MemSet(object dst, byte value)
        {
            MemSet(dst, 0, value, Size(dst));
        }
        public static void MemSet<D>(ref D dst, byte value) where D : struct
        {
            MemSet(ref dst, 0, value, Size<D>());
        }

        public static void MemClean(ref byte dst, int offset, int cnt)
        {
            MemClean1024(ref dst, offset, cnt);
        }
        private static void MemClean1024(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 1024)
            {
                ref MemCopyHelper1024 dstBuffer = ref GetFieldRef<byte, MemCopyHelper1024>(ref dst, offset);
                dstBuffer = default;
                offset += 1024;
                cnt -= 1024;
            }
            MemClean512(ref dst, offset, cnt);
        }
        private static void MemClean512(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 512)
            {
                ref MemCopyHelper512 dstBuffer = ref GetFieldRef<byte, MemCopyHelper512>(ref dst, offset);
                dstBuffer = default;
                offset += 512;
                cnt -= 512;
            }
            MemClean256(ref dst, offset, cnt);
        }
        private static void MemClean256(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 256)
            {
                ref MemCopyHelper256 dstBuffer = ref GetFieldRef<byte, MemCopyHelper256>(ref dst, offset);
                dstBuffer = default;
                offset += 256;
                cnt -= 256;
            }
            MemClean128(ref dst, offset, cnt);
        }
        private static void MemClean128(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 128)
            {
                ref MemCopyHelper128 dstBuffer = ref GetFieldRef<byte, MemCopyHelper128>(ref dst, offset);
                dstBuffer = default;
                offset += 128;
                cnt -= 128;
            }
            MemClean64(ref dst, offset, cnt);
        }
        private static void MemClean64(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 64)
            {
                ref MemCopyHelper64 dstBuffer = ref GetFieldRef<byte, MemCopyHelper64>(ref dst, offset);
                dstBuffer = default;
                offset += 64;
                cnt -= 64;
            }
            MemClean32(ref dst, offset, cnt);
        }
        private static void MemClean32(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 32)
            {
                ref MemCopyHelper32 dstBuffer = ref GetFieldRef<byte, MemCopyHelper32>(ref dst, offset);
                dstBuffer = default;
                offset += 32;
                cnt -= 32;
            }
            MemClean16(ref dst, offset, cnt);
        }
        private static void MemClean16(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 16)
            {
                ref MemCopyHelper16 dstBuffer = ref GetFieldRef<byte, MemCopyHelper16>(ref dst, offset);
                dstBuffer = default;
                offset += 16;
                cnt -= 16;
            }
            MemClean8(ref dst, offset, cnt);
        }
        private static void MemClean8(ref byte dst, int offset, int cnt)
        {
            while (cnt >= 8)
            {
                ref ulong dstBuffer = ref GetFieldRef<byte, ulong>(ref dst, offset);
                dstBuffer = 0;
                offset += 8;
                cnt -= 8;
            }
            MemCleanLE7(ref dst, offset, cnt);
        }
        private static void MemCleanLE7<T>(ref byte dst, int offset) where T : struct
        {
            ref T dstBuffer = ref GetFieldRef<byte, T>(ref dst, offset);
            dstBuffer = default;
        }
        private static void MemCleanLE7(ref byte dst, int offset, int cnt)
        {
            switch (cnt)
            {
                case 7:
                    MemCleanLE7<MemCopyHelper7>(ref dst, offset);
                    break;
                case 6:
                    MemCleanLE7<MemCopyHelper6>(ref dst, offset);
                    break;
                case 5:
                    MemCleanLE7<MemCopyHelper5>(ref dst, offset);
                    break;
                case 4:
                    MemCleanLE7<MemCopyHelper4>(ref dst, offset);
                    break;
                case 3:
                    MemCleanLE7<MemCopyHelper3>(ref dst, offset);
                    break;
                case 2:
                    MemCleanLE7<MemCopyHelper2>(ref dst, offset);
                    break;
                case 1:
                    MemCleanLE7<MemCopyHelper1>(ref dst, offset);
                    break;
            }
        }

        public static void MemClean(object dst, int offset, int cnt)
        {
            MemClean(ref GetHeadRef(dst), offset, cnt);
        }
        public static void MemClean<D>(ref D dst, int offset, int cnt) where D : struct
        {
            MemClean(ref GetHeadRef(ref dst), offset, cnt);
        }

        public static void MemClean(object dst, int offset)
        {
            MemClean(dst, offset, Size(dst) - offset);
        }
        public static void MemClean<D>(ref D dst, int offset) where D : struct
        {
            MemClean(ref dst, offset, Size<D>() - offset);
        }

        public static void MemCleanSize(object dst, int cnt)
        {
            MemClean(dst, 0, cnt);
        }
        public static void MemCleanSize<D>(ref D dst, int cnt) where D : struct
        {
            MemClean(ref dst, 0, cnt);
        }

        public static void MemClean(object dst)
        {
            MemClean(dst, 0, Size(dst));
        }
        public static void MemClean<D>(ref D dst) where D : struct
        {
            MemClean(ref dst, 0, Size<D>());
        }

        public static ref byte AddByteOffset(this ref byte addr, IntPtr offset)
        {
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRef(ref addr);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((long)objAddress + (long)offset);
                RawRef refField = default;
                refField.Address = fieldAddress;
                ref byte refResult = ref refField.GetRef<byte>();
                RawRef refCheck = default;
                refCheck.SetRef(ref addr);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }
        public static ref T AddOffset<T>(ref T addr, int offset)
        {
            long realOffset = offset;
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                realOffset *= sizeof(T);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRef(ref addr);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((long)objAddress + realOffset);
                RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                RawRef refCheck = default;
                refCheck.SetRef(ref addr);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }
        public static IntPtr OffsetBetween<D, S>(ref D rd, ref S rs)
        {
            for (; ; )
            {
                RawRef refd = default;
                refd.SetRef(ref rd);
                var daddr = refd.Address;

                RawRef refs = default;
                refs.SetRef(ref rs);
                var saddr = refs.Address;

                var offset = (IntPtr)((long)daddr - (long)saddr);

                refd.SetRef(ref rd);
                refs.SetRef(ref rs);
                if (daddr == refd.Address && saddr == refs.Address)
                {
                    return offset;
                }
            }
        }
    }
}
