using System;
using System.Collections.Generic;
using System.Text;

namespace Mod
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
            MemCopy1024(ref dst, dstStart, ref src, srcStart, cnt);
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
            MemCopy(dst, dstStart, src, srcStart, Math.Min(SizeOf(dst) - dstStart, SizeOf(src) - srcStart));
        }
        public static void MemCopy<D, S>(ref D dst, int dstStart, ref S src, int srcStart) where D : struct where S : struct
        {
            MemCopy(ref dst, dstStart, ref src, srcStart, Math.Min(SizeOf<D>() - dstStart, SizeOf<S>() - srcStart));
        }
        public static void MemCopy<S>(object dst, int dstStart, ref S src, int srcStart) where S : struct
        {
            MemCopy(dst, dstStart, ref src, srcStart, Math.Min(SizeOf(dst) - dstStart, SizeOf<S>() - srcStart));
        }
        public static void MemCopy<D>(ref D dst, int dstStart, object src, int srcStart) where D : struct
        {
            MemCopy(ref dst, dstStart, src, srcStart, Math.Min(SizeOf<D>() - dstStart, SizeOf(src) - srcStart));
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
            MemCopy(dst, 0, src, 0, Math.Min(SizeOf(dst), SizeOf(src)));
        }
        public static void MemCopy<D, S>(ref D dst, ref S src) where D : struct where S : struct
        {
            MemCopy(ref dst, 0, ref src, 0, Math.Min(SizeOf<D>(), SizeOf<S>()));
        }
        public static void MemCopy<S>(object dst, ref S src) where S : struct
        {
            MemCopy(dst, 0, ref src, 0, Math.Min(SizeOf(dst), SizeOf<S>()));
        }
        public static void MemCopy<D>(ref D dst, object src) where D : struct
        {
            MemCopy(ref dst, 0, src, 0, Math.Min(SizeOf<D>(), SizeOf(src)));
        }
    }
}
