using Mod.LowLevel;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for SpanStrEx.cs: the Span/address bridge (AsSpan / GetAddress / GetRef)
    /// and the nul-terminated string helpers (StrLen / Append).
    ///
    /// <para>
    /// StrLen(IntPtr) / StrLen(ref byte) are unbounded scans - every test feeding them
    /// guarantees a nul terminator. The maxsize overloads return maxsize when no nul is
    /// found within the window. Append treats the span as a nul-terminated buffer and
    /// does NOT write a trailing nul after the appended bytes.
    /// </para>
    /// </summary>
    public class SpanStrExTests
    {
        // =====================================================================
        // AsSpan / AsReadOnlySpan over IntPtr
        // =====================================================================

        [Fact]
        public void AsSpan_IntPtr_RoundTrip()
        {
            IntPtr ptr = Marshal.AllocHGlobal(16);
            try
            {
                for (int i = 0; i < 16; i++) Marshal.WriteByte(ptr, i, (byte)(i + 1));

                var span = UnsafeEx.AsSpan<byte>(ptr, 16);

                for (int i = 0; i < 16; i++) Assert.Equal((byte)(i + 1), span[i]);
                Assert.Equal(ptr, span.GetAddress());          // address round-trips
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void AsSpan_IntPtr_TypedElements()
        {
            IntPtr ptr = Marshal.AllocHGlobal(sizeof(int) * 4);
            try
            {
                int[] values = { 10, 20, 30, 40 };
                Marshal.Copy(values, 0, ptr, 4);

                var span = UnsafeEx.AsSpan<int>(ptr, sizeof(int) * 4);

                for (int i = 0; i < 4; i++) Assert.Equal(values[i], span[i]);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void AsReadOnlySpan_IntPtr_ReadsContent()
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            try
            {
                for (int i = 0; i < 8; i++) Marshal.WriteByte(ptr, i, (byte)('a' + i));

                var span = UnsafeEx.AsReadOnlySpan<byte>(ptr, 8);

                Assert.Equal("abcdefgh", System.Text.Encoding.ASCII.GetString(span));
                Assert.Equal(ptr, span.GetAddress());
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        // =====================================================================
        // AsSpan over ref
        // =====================================================================

        [Fact]
        public void AsSpan_Ref_OverArray()
        {
            var arr = new byte[8];
            for (int i = 0; i < 8; i++) arr[i] = (byte)(i * 2);

            var span = UnsafeEx.AsSpan<byte>(ref arr[0], 8);

            Assert.Equal(8, span.Length);
            for (int i = 0; i < 8; i++) Assert.Equal((byte)(i * 2), span[i]);
        }

        // =====================================================================
        // AsCStringSpan family - length stops at the nul
        // =====================================================================

        [Fact]
        public void AsCStringSpan_IntPtr_StopsAtNul()
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            try
            {
                byte[] data = { (byte)'a', (byte)'b', (byte)'c', 0, (byte)'d', (byte)'e', 0, 0 };
                Marshal.Copy(data, 0, ptr, 8);

                var span = UnsafeEx.AsCStringSpan(ptr);

                Assert.Equal(3, span.Length);                   // stops at first nul, excludes it
                Assert.Equal("abc", System.Text.Encoding.ASCII.GetString(span));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void AsCStringSpan_Ref_StopsAtNul()
        {
            var arr = new byte[] { (byte)'x', (byte)'y', 0, (byte)'z' };

            var span = UnsafeEx.AsCStringSpan(ref arr[0]);

            Assert.Equal(2, span.Length);
            Assert.Equal("xy", System.Text.Encoding.ASCII.GetString(span));
        }

        [Fact]
        public void AsReadOnlyCStringSpan_StopsAtNul()
        {
            IntPtr ptr = Marshal.AllocHGlobal(4);
            try
            {
                Marshal.Copy(new byte[] { (byte)'h', (byte)'i', 0, (byte)'!' }, 0, ptr, 4);

                var span = ptr.AsReadOnlyCStringSpan();

                Assert.Equal(2, span.Length);
                Assert.Equal("hi", System.Text.Encoding.ASCII.GetString(span));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        // =====================================================================
        // GetAddress / GetAddressAndLength / GetRef / GetRefAndLength
        // =====================================================================

        [Fact]
        public void GetAddress_And_Length_MatchFixedPointer()
        {
            var arr = new int[5] { 1, 2, 3, 4, 5 };
            var span = arr.AsSpan();

            unsafe
            {
                fixed (int* p = arr)                  // pin first: GetAddressAndLength must not observe a moved array
                {
                    var (addr, byteLength) = span.GetAddressAndLength();

                    Assert.Equal(sizeof(int) * 5, byteLength);
                    Assert.Equal((IntPtr)p, addr);
                }
            }
        }

        [Fact]
        public void GetRef_WritesThroughToTheArray()
        {
            var arr = new int[] { 1, 2, 3 };
            var span = arr.AsSpan();

            ref int first = ref span.GetRef();
            first = 99;

            Assert.Equal(99, arr[0]);                           // the ref aliases the array slot
        }

        [Fact]
        public void GetRefAndLength_CombinesBoth()
        {
            var arr = new byte[] { 7, 8, 9, 10 };
            var span = arr.AsSpan();

            ref byte first = ref span.GetRefAndLength(out int byteLength);

            Assert.Equal(4, byteLength);
            Assert.Equal(7, first);
            first = 70;
            Assert.Equal(70, arr[0]);
        }

        [Fact]
        public void GetAddress_ReadOnlySpan_MatchesWritable()
        {
            var arr = new byte[] { 1, 2, 3 };
            var ro = (ReadOnlySpan<byte>)arr;

            unsafe
            {
                fixed (byte* p = arr)                 // pin first: two address reads must not straddle a GC move
                {
                    Assert.Equal((IntPtr)p, arr.AsSpan().GetAddress());
                    Assert.Equal((IntPtr)p, ro.GetAddress());
                }
            }
        }

        // =====================================================================
        // StrLen family
        // =====================================================================

        [Fact]
        public void StrLen_IntPtr_Basic()
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            try
            {
                Marshal.Copy(new byte[] { (byte)'h', (byte)'e', 0, (byte)'x' }, 0, ptr, 4);
                Assert.Equal(2, UnsafeEx.StrLen(ptr));
                Assert.Equal(0, UnsafeEx.StrLen(IntPtr.Zero));  // null pointer is empty
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void StrLen_Ref_Basic()
        {
            var arr = new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, (byte)'d' };
            Assert.Equal(3, UnsafeEx.StrLen(ref arr[0]));
        }

        [Fact]
        public void StrLen_IntPtr_MaxSize_TruncatesWhenNoNul()
        {
            IntPtr ptr = Marshal.AllocHGlobal(8);
            try
            {
                Marshal.Copy(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }, 0, ptr, 8);   // no nul at all

                Assert.Equal(4, UnsafeEx.StrLen(ptr, 4));      // clamped to maxsize
                Assert.Equal(8, UnsafeEx.StrLen(ptr, 8));

                Marshal.WriteByte(ptr, 2, 0);                   // nul inside the window
                Assert.Equal(2, UnsafeEx.StrLen(ptr, 8));
                Assert.Equal(0, UnsafeEx.StrLen(IntPtr.Zero, 8));
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void StrLen_Ref_MaxSize_TruncatesWhenNoNul()
        {
            var arr = new byte[] { 1, 2, 3, 4, 5 };             // no nul

            Assert.Equal(3, UnsafeEx.StrLen(ref arr[0], 3));   // clamped
            arr[1] = 0;
            Assert.Equal(1, UnsafeEx.StrLen(ref arr[0], 5));   // nul found inside window
        }

        [Fact]
        public void StrLen_Span_StopsAtNulOrFullLength()
        {
            var withNul = new byte[] { (byte)'a', 0, (byte)'b' };
            Assert.Equal(1, UnsafeEx.StrLen(withNul));          // stops at the nul

            var withoutNul = new byte[] { 1, 2, 3 };
            Assert.Equal(3, UnsafeEx.StrLen(withoutNul));       // no nul: full length
        }

        // =====================================================================
        // Append family - nul-terminated buffer semantics, no trailing nul written
        // =====================================================================

        [Fact]
        public void Append_Byte_WritesAtNulPosition()
        {
            var buf = new byte[] { (byte)'a', (byte)'b', 0, 0 };

            var result = buf.AsSpan().Append((byte)'c');

            Assert.Equal((byte)'c', result[2]);                 // written where the nul was
            Assert.Equal(0, result[3]);                         // the byte after stays untouched
            Assert.Equal((byte)'c', buf[2]);                    // writes through to the array
            Assert.Equal(3, UnsafeEx.StrLen(result));           // "abc" now has no nul inside: full length
        }

        [Fact]
        public void Append_Byte_Full_SilentOrThrowing()
        {
            var full = new byte[] { 1, 2, 3 };                  // no nul: StrLen == Length == full

            var silent = full.AsSpan().Append((byte)9);         // throwOnError = false
            Assert.Equal(new byte[] { 1, 2, 3 }, silent.ToArray());   // unchanged

            Assert.Throws<ArgumentException>(() => full.AsSpan().Append((byte)9, throwOnError: true));
        }

        [Fact]
        public void Append_Span_CopiesAtNulPosition()
        {
            var buf = new byte[] { (byte)'a', 0, 0, 0, 0 };

            var result = buf.AsSpan().Append(new byte[] { (byte)'b', (byte)'c' });

            Assert.Equal(new byte[] { (byte)'a', (byte)'b', (byte)'c', 0, 0 }, result.ToArray());
        }

        [Fact]
        public void Append_Span_Insufficient_SilentOrThrowing()
        {
            var buf = new byte[] { (byte)'a', 0, 0 };           // len 1, room 2

            var silent = buf.AsSpan().Append(new byte[] { 1, 2, 3 });   // needs 3 > 2
            Assert.Equal(new byte[] { (byte)'a', 0, 0 }, silent.ToArray());   // unchanged

            Assert.Throws<ArgumentException>(() =>
                buf.AsSpan().Append(new byte[] { 1, 2, 3 }, throwOnError: true));
        }

        [Fact]
        public void Append_Span_ExactlyFits()
        {
            var buf = new byte[] { 0, 0, 0 };                   // empty, room 3

            var result = buf.AsSpan().Append(new byte[] { 1, 2, 3 });   // exactly 3

            Assert.Equal(new byte[] { 1, 2, 3 }, result.ToArray());
        }
    }
}
