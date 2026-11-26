using Mod.LowLevel;
using System;
using System.Runtime.InteropServices;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Pins the Size/RawSize semantics measured by the SizeofMatrixProbe /
    /// LayoutSizeProbe matrices (six runtimes). The values asserted here are
    /// the cross-runtime-stable ones; runtime-divergent rows (Auto + declared
    /// Size, hot-type edge cases) stay in the probes.
    /// </summary>
    public class SizeSemanticsTests
    {
        // string data = first-char offset (the length field, 4 on every
        // measured runtime/bitness - the calibration absorbs the layout
        // differences) + (length + 1) * 2 chars incl. terminator.
        private const int StringFirstCharOffset = 4;
        // SZ array data = first-element offset (length field region, 8 on
        // every measured runtime/bitness) + length * stride.
        private const int ArrayFirstElemOffset = 8;

        private class TestC { public int A; public object O; }

        [StructLayout(LayoutKind.Explicit, Size = 32)]
        private struct Explicit32 { [FieldOffset(0)] public byte A; }

        [StructLayout(LayoutKind.Sequential, Size = 32)]
        private struct SeqSz32 { public byte A; }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct Pack1 { public byte A; public long B; }   // sizeof 9

        [StructLayout(LayoutKind.Sequential)]
        private struct S16 { public long A; public long B; }

        [Fact]
        public void Size_ValueType_IsInstanceSize()
        {
            Assert.Equal(4, UnsafeEx.Size(typeof(int)));
            Assert.Equal(16, UnsafeEx.Size(typeof(S16)));
            Assert.Equal(9, UnsafeEx.Size(typeof(Pack1)));
        }

        [Fact]
        public void Size_Generic_MatchesTypeForm()
        {
            Assert.Equal(UnsafeEx.Size(typeof(S16)), UnsafeEx.Size<S16>());
            Assert.Equal(UnsafeEx.Size(typeof(Pack1)), UnsafeEx.Size<Pack1>());
        }

        [Fact]
        public void Size_DeclaredSize_IsHonored()
        {
            // Explicit value types honor StructLayoutAttribute.Size on every
            // runtime; Sequential+blittable does too (CoreCLR drops the whole
            // ClassLayout only with GC-reference fields).
            Assert.Equal(32, UnsafeEx.Size(typeof(Explicit32)));
            Assert.Equal(32, UnsafeEx.Size(typeof(SeqSz32)));
        }

        [Fact]
        public void Size_Class_IsHeaderStrippedPayload()
        {
            // {int A; object O}: extent 4 + ptr, rounded to the pointer
            // alignment -> 16 on x64, 8 on x86.
            int expected = IntPtr.Size == 8 ? 16 : 8;
            Assert.Equal(expected, UnsafeEx.Size(typeof(TestC)));
        }

        [Fact]
        public void Size_NoInstanceFields_IsZero()
        {
            Assert.Equal(0, UnsafeEx.Size(typeof(object)));
        }

        [Fact]
        public void Size_StringType_IsFixedPartOnly()
        {
            // The TYPE route reports the fixed part (length + first char);
            // the content lives in instances - Size(obj) measures that.
            Assert.Equal(8, UnsafeEx.Size(typeof(string)));
        }

        [Fact]
        public void Size_StringInstance_MeasuresContent()
        {
            Assert.Equal(StringFirstCharOffset + 4 * 2, UnsafeEx.Size((object)"abc"));
            // empty: length field + terminator only
            Assert.Equal(StringFirstCharOffset + 2, UnsafeEx.Size((object)string.Empty));
            // +2 per char, no rounding steps in between
            Assert.Equal(StringFirstCharOffset + 34 * 2, UnsafeEx.Size((object)new string('a', 33)));
            Assert.Equal(2,
                UnsafeEx.Size((object)new string('a', 34)) - UnsafeEx.Size((object)new string('a', 33)));
        }

        [Fact]
        public void Size_Array_MeasuresElements()
        {
            Assert.Equal(ArrayFirstElemOffset + 10 * 4, UnsafeEx.Size((object)new int[10]));
            Assert.Equal(ArrayFirstElemOffset, UnsafeEx.Size((object)new int[0]));
            // reference elements are one pointer each
            Assert.Equal(ArrayFirstElemOffset + 3 * IntPtr.Size, UnsafeEx.Size((object)new string[3]));
            // a Pack=1 struct array strides by its raw size (9)
            Assert.Equal(ArrayFirstElemOffset + 4 * 9, UnsafeEx.Size((object)new Pack1[4]));
        }

        [Fact]
        public void Size_MultiDimArray_IsLowerBound()
        {
            // rank > 1: the per-dimension bounds block between the length and
            // the data is NOT counted - the documented lower bound (the real
            // payload is larger, e.g. +16 on CoreCLR x64 for rank 2).
            Assert.Equal(ArrayFirstElemOffset + 12 * 4, UnsafeEx.Size((object)new int[3, 4]));
        }

        [Fact]
        public void Size_BoxedValue_EqualsTypeSize()
        {
            Assert.Equal(9, UnsafeEx.Size((object)default(Pack1)));
            Assert.Equal(16, UnsafeEx.Size((object)default(S16)));
        }

        [Fact]
        public void Size_FixedInstance_EqualsTypeSize()
        {
            var c = new TestC();
            Assert.Equal(UnsafeEx.Size(typeof(TestC)), UnsafeEx.Size(c));
        }

        [Fact]
        public void Size_Null_IsZero()
        {
            Assert.Equal(0, UnsafeEx.Size((object)null));
        }

        [Fact]
        public void RawSize_Type_IsRawSizeof()
        {
            Assert.Equal(4, UnsafeEx.RawSize(typeof(int)));
            Assert.Equal(9, UnsafeEx.RawSize(typeof(Pack1)));
            Assert.Equal(32, UnsafeEx.RawSize(typeof(Explicit32)));
            Assert.Equal(IntPtr.Size, UnsafeEx.RawSize(typeof(string)));
            Assert.Equal(IntPtr.Size, UnsafeEx.RawSize(typeof(TestC)));
        }

        [Fact]
        public void RawSize_Generic_IsRawSizeof()
        {
            Assert.Equal(9, UnsafeEx.RawSize<Pack1>());
            Assert.Equal(IntPtr.Size, UnsafeEx.RawSize<string>());
            Assert.Equal(IntPtr.Size, UnsafeEx.RawSize<object>());
        }
    }
}
