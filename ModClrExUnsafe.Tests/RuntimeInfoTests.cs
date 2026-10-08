using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// The dotnet-test-verifiable half of RuntimeInfo (PlatDep.cs). Most
    /// branches (Unity / IL2CPP / Mono / NativeAOT) need their own runtimes and
    /// are out of reach here; under the net10 JIT test host the expected
    /// detection is: plain .NET (Core) with dynamic code available.
    /// </summary>
    public class RuntimeInfoTests
    {
        [Fact]
        public void DotNetHost_Detection()
        {
            Assert.True(RuntimeInfo.IsNetCore);
            Assert.False(RuntimeInfo.IsNetFramework);
            Assert.False(RuntimeInfo.IsUnity);
            Assert.False(RuntimeInfo.IsIl2Cpp);
            Assert.False(RuntimeInfo.IsMono);
            Assert.False(RuntimeInfo.IsNativeAOT);
            Assert.False(RuntimeInfo.IsHybridCLREnabled);
            Assert.Equal(RuntimeInfo.RuntimeCategory.NetCore, RuntimeInfo.Runtime);
        }

        [Fact]
        public void DotNetHost_DynamicCodeEnabled()
        {
            // the test host runs with JIT and full System.Reflection.Emit
            Assert.True(RuntimeInfo.IsDynamicCodeEnabled);
        }

        [Fact]
        public void IsAOT_IsExactlyNativeAotOrIl2Cpp()
        {
            // documented composition: IsAOT == IsNativeAOT || IsIl2Cpp
            Assert.Equal(RuntimeInfo.IsNativeAOT || RuntimeInfo.IsIl2Cpp, RuntimeInfo.IsAOT);
            // and on this host both are false, so AOT must be false too
            Assert.False(RuntimeInfo.IsAOT);
        }
    }
}
