using System.Linq;
using Mod.LowLevel;
using Xunit;
using SysRI = System.Runtime.InteropServices.RuntimeInformation;
using SysOS = System.Runtime.InteropServices.OSPlatform;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// RuntimeInfo.Platform / derived flags tests (non-Unity paths; the Unity
    /// branch is exercised on device/editor runs, not under dotnet test).
    /// </summary>
    public class RuntimeInfoPlatformTests
    {
        [Fact]
        public void Platform_IsKnown_UnderDotNet()
        {
            if (RuntimeInfo.IsUnity) return; // not reachable under dotnet test
            Assert.NotEqual(RuntimeInfo.OSPlatform.Unknown, RuntimeInfo.Platform);
        }

        [Fact]
        public void Platform_MatchesSystemRuntimeInformation()
        {
            if (RuntimeInfo.IsUnity) return;
            // On plain dotnet hosts (no Xamarin assemblies) our Platform must
            // agree with the BCL's own OS detection.
            if (SysRI.IsOSPlatform(SysOS.Windows))
                Assert.Equal(RuntimeInfo.OSPlatform.Windows, RuntimeInfo.Platform);
            else if (SysRI.IsOSPlatform(SysOS.OSX))
                Assert.Equal(RuntimeInfo.OSPlatform.Mac, RuntimeInfo.Platform);
            else if (SysRI.IsOSPlatform(SysOS.Linux))
                Assert.Equal(RuntimeInfo.OSPlatform.Linux, RuntimeInfo.Platform);
        }

        [Fact]
        public void DerivedFlags_ConsistentWithPlatform()
        {
            var p = RuntimeInfo.Platform;
            Assert.Equal(
                p == RuntimeInfo.OSPlatform.iOS || p == RuntimeInfo.OSPlatform.Android,
                RuntimeInfo.IsMobileDevice);
            Assert.Equal(
                p == RuntimeInfo.OSPlatform.Windows || p == RuntimeInfo.OSPlatform.Mac || p == RuntimeInfo.OSPlatform.Linux,
                RuntimeInfo.IsStandalone);
            // Mobile and standalone are mutually exclusive by construction.
            Assert.False(RuntimeInfo.IsMobileDevice && RuntimeInfo.IsStandalone);
        }

        [Fact]
        public void Runtime_NeverReportsUnityUnderDotNet()
        {
            if (RuntimeInfo.IsUnity) return;
            var unityCategories = new[]
            {
                RuntimeInfo.RuntimeCategory.UnityMono, RuntimeInfo.RuntimeCategory.UnityIl2Cpp
            };
            Assert.DoesNotContain(RuntimeInfo.Runtime, unityCategories);
            Assert.False(RuntimeInfo.IsIl2Cpp);
        }

        [Fact]
        public void Runtime_CategoryIsOneOfTheKnownSet()
        {
            var all = new[]
            {
                RuntimeInfo.RuntimeCategory.NetFramework, RuntimeInfo.RuntimeCategory.NetCore,
                RuntimeInfo.RuntimeCategory.NativeAOT, RuntimeInfo.RuntimeCategory.Mono,
                RuntimeInfo.RuntimeCategory.UnityMono, RuntimeInfo.RuntimeCategory.UnityIl2Cpp
            };
            Assert.Contains(RuntimeInfo.Runtime, all);
        }
    }
}
