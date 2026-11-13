using System;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// NativeFuncLoader tests. The Windows branch is exercised here; the
    /// macOS/Linux (dlopen/dlsym) and iOS (RTLD_DEFAULT / __Internal)
    /// branches need their platforms - the VarargProbe project covers them
    /// on device.
    /// </summary>
    public class NativeFuncLoaderTests
    {
        [Fact]
        public void Resolves_KnownSystemExport()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            // kernel32!GetProcAddress is guaranteed to exist on Windows.
            FunctionPointer p = NativeFuncLoader.LoadNativeFunction("kernel32", "GetProcAddress");
            Assert.NotEqual(IntPtr.Zero, p.Pfn);
            Assert.True(p.IsUnmanaged); // resolved symbols are native code
        }

        [Fact]
        public void Miss_UnknownSymbol_ReturnsDefault()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            FunctionPointer p = NativeFuncLoader.LoadNativeFunction("kernel32", "No__Such__Export__12345");
            Assert.Equal(IntPtr.Zero, p.Pfn);
        }

        [Fact]
        public void Miss_UnknownLibrary_ReturnsDefault()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            FunctionPointer p = NativeFuncLoader.LoadNativeFunction("No__Such__Lib__12345", "GetProcAddress");
            Assert.Equal(IntPtr.Zero, p.Pfn);
        }

        [Fact]
        public void Miss_EmptyArguments_ReturnsDefault()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            Assert.Equal(IntPtr.Zero, NativeFuncLoader.LoadNativeFunction("kernel32", "").Pfn);
            Assert.Equal(IntPtr.Zero, NativeFuncLoader.LoadNativeFunction("kernel32", null).Pfn);
            Assert.Equal(IntPtr.Zero, NativeFuncLoader.LoadNativeFunction(null, "GetProcAddress").Pfn);
        }

        [Fact]
        public void Resolves_TheSamePointerTwice()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            FunctionPointer p1 = NativeFuncLoader.LoadNativeFunction("kernel32", "GetProcAddress");
            FunctionPointer p2 = NativeFuncLoader.LoadNativeFunction("kernel32", "GetProcAddress");
            Assert.Equal(p1.Pfn, p2.Pfn); // stable address across calls
        }

        [Fact]
        public void ImplicitConversion_ToIntPtr()
        {
            if (RuntimeInfo.Platform != RuntimeInfo.OSPlatform.Windows) return;
            FunctionPointer p = NativeFuncLoader.LoadNativeFunction("kernel32", "GetProcAddress");
            IntPtr raw = p; // implicit operator IntPtr
            Assert.Equal(p.Pfn, raw);
        }
    }
}
