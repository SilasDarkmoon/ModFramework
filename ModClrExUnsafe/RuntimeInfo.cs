using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static class RuntimeInfo
    {
        public enum RuntimeCategory
        {
            NetFramework,
            NetCore,
            NativeAOT,
            Mono,
            UnityMono,
            UnityIl2Cpp,
        }
        public enum OSPlatform
        {
            Unknown = 0,
            Windows,
            Linux,
            iOS,
            Android,
            Mac,
        }
        public enum InstructionArchitecture
        {
            Unknown = 0,
            X86,
            X64,
            Arm64,
        }

        private static RuntimeCategory _Runtime;
        public static RuntimeCategory Runtime => _Runtime;

        private static OSPlatform _Platform;
        public static OSPlatform Platform => _Platform;

        public static bool IsMobileDevice => _Platform == OSPlatform.iOS || _Platform == OSPlatform.Android;
        public static bool IsStandalone => _Platform == OSPlatform.Windows || _Platform == OSPlatform.Mac || _Platform == OSPlatform.Linux;

        private static InstructionArchitecture _RuntimeArch;
        private static InstructionArchitecture _DeviceArch;
        public static InstructionArchitecture RuntimeArchitecture => _RuntimeArch;
        public static InstructionArchitecture DeviceArchitecture => _DeviceArch;

        private static bool _IsUnity;
        public static bool IsUnity => _IsUnity;

        private static bool _IsEditor;
        public static bool IsEditor => _IsEditor;

        private static bool _IsIl2Cpp;
        public static bool IsIl2Cpp => _IsIl2Cpp;

        private static bool _IsMono;
        public static bool IsMono => _IsMono;

        private static bool _IsNetFramework;
        public static bool IsNetFramework => _IsNetFramework;

        private static bool _IsNetCore;
        public static bool IsNetCore => _IsNetCore;

        private static bool _IsNativeAOT;
        public static bool IsNativeAOT => _IsNativeAOT;

        private static bool _IsDynamicCodeEnabled;
        public static bool IsDynamicCodeEnabled => _IsDynamicCodeEnabled;

        private static bool _IsHybridCLREnabled;
        public static bool IsHybridCLREnabled => _IsHybridCLREnabled;

        public static bool IsAOT => _IsNativeAOT || _IsIl2Cpp;

        static RuntimeInfo()
        {
            AOTPreserve.Prepare();
            _IsDynamicCodeEnabled = CheckIsDynamicCodeEnabled();
            Type unityApplicationType;
            _IsUnity = (unityApplicationType = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule")) != null;
            if (_IsUnity)
            {
                _IsIl2Cpp = CheckIsIl2Cpp();
                if (_IsIl2Cpp)
                {
                    _Runtime = RuntimeCategory.UnityIl2Cpp;
                    _IsHybridCLREnabled = CheckIsHybridCLREnabled();
                }
                else
                {
                    _Runtime = RuntimeCategory.UnityMono;
                    _IsMono = true;
                }
                var uplat = Convert.ToInt32(unityApplicationType.GetProperty("platform", BindingFlags.Static | BindingFlags.Public).GetValue(null));
                switch (uplat)
                {
                    case 0:
                        _Platform = OSPlatform.Mac;
                        _IsEditor = true;
                        break;
                    case 1:
                        _Platform = OSPlatform.Mac;
                        break;
                    case 2:
                        _Platform = OSPlatform.Windows;
                        break;
                    case 7:
                        _Platform = OSPlatform.Windows;
                        _IsEditor = true;
                        break;
                    case 8:
                        _Platform = OSPlatform.iOS;
                        break;
                    case 11:
                        _Platform = OSPlatform.Android;
                        break;
                    case 13:
                        _Platform = OSPlatform.Linux;
                        break;
                    case 16:
                        _Platform = OSPlatform.Linux;
                        _IsEditor = true;
                        break;
                    case 43:
                        _Platform = OSPlatform.Linux; // server
                        break;
                    case 44:
                        _Platform = OSPlatform.Windows; // server
                        break;
                    case 45:
                        _Platform = OSPlatform.Mac; // server
                        break;
                }
            }
            else
            {
                _IsMono = CheckIsMono();
                if (_IsMono)
                {
                    _Runtime = RuntimeCategory.Mono;
                }
                else
                {
                    _IsNetFramework = CheckIsNetFramework();
                    if (_IsNetFramework)
                    {
                        _Runtime = RuntimeCategory.NetFramework;
                    }
                    else
                    {
                        _IsNetCore = true;
                        _IsNativeAOT = CheckIsNativeAOT();
                        if (_IsNativeAOT)
                        {
                            _Runtime = RuntimeCategory.NativeAOT;
                        }
                        else
                        {
                            _Runtime = RuntimeCategory.NetCore;
                        }
                    }
                }
                _Platform = DetectNonUnityPlatform();
            }

            switch (RuntimeInformation.ProcessArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X86:
                    _RuntimeArch = InstructionArchitecture.X86;
                    break;
                case System.Runtime.InteropServices.Architecture.X64:
                    _RuntimeArch = InstructionArchitecture.X64;
                    break;
                case System.Runtime.InteropServices.Architecture.Arm64:
                    _RuntimeArch = InstructionArchitecture.Arm64;
                    break;
                default:
                    _RuntimeArch = InstructionArchitecture.Unknown;
                    break;
            }
            switch (RuntimeInformation.OSArchitecture)
            {
                case System.Runtime.InteropServices.Architecture.X86:
                    _DeviceArch = InstructionArchitecture.X86;
                    break;
                case System.Runtime.InteropServices.Architecture.X64:
                    _DeviceArch = InstructionArchitecture.X64;
                    break;
                case System.Runtime.InteropServices.Architecture.Arm64:
                    _DeviceArch = InstructionArchitecture.Arm64;
                    break;
                default:
                    _DeviceArch = InstructionArchitecture.Unknown;
                    break;
            }
        }

        // Non-Unity platform detection. Xamarin/.NET mobile runtimes report as
        // plain Darwin/Linux through RuntimeInformation, so their runtime
        // assemblies are probed first (same reflection pattern as the Unity
        // detection above). NOTE: System.Runtime.InteropServices.OSPlatform
        // must stay fully qualified - the nested OSPlatform enum shadows the
        // using-imported one inside this class.
        private static OSPlatform DetectNonUnityPlatform()
        {
            // Xamarin / .NET iOS: UIKit is present.
            if (Type.GetType("UIKit.UIApplication, UIKit") != null)
                return OSPlatform.iOS;
            // Xamarin / .NET Android: Mono.Android is present.
            if (Type.GetType("Android.App.Activity, Mono.Android") != null)
                return OSPlatform.Android;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Windows))
                return OSPlatform.Windows;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.OSX))
                return OSPlatform.Mac;
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(
                    System.Runtime.InteropServices.OSPlatform.Linux))
                return OSPlatform.Linux;
            return OSPlatform.Unknown;
        }

        private static bool CheckIsIl2Cpp()
        {
            try
            {
                var pfunc = typeof(List<int>).GetMethod("Clear").MethodHandle.GetFunctionPointer(); // On IL2CPP, it is impossible to get function pointer of generic method, it will throw NotSupportedException. On Mono/.NET, it will succeed.
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        private static bool CheckIsMono()
        {
            return Type.GetType("Mono.Runtime") != null;
        }

        private static bool CheckIsNetFramework()
        {
            return RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework", StringComparison.OrdinalIgnoreCase);
        }

        // One-sided detectors: each can only answer "yes, AOT" and neither can
        // answer "no" - this check runs after Mono/.NET Framework are excluded,
        // so "both silent" falls through to CoreCLR. The two legs fail in
        // OPPOSITE directions, which is what makes the union robust:
        //
        // 1) Identity: Internal.Runtime.Augments.RuntimeAugments exists ONLY in
        //    the AOT build of System.Private.CoreLib (CoreCLR has no such type,
        //    so a hit is structurally impossible on JIT runtimes). Probed
        //    2026-11-13 on .NET 10 (Probes/AotDetectProbe): its reflection
        //    metadata survives BOTH the default aggressive trimming and
        //    TrimMode=partial. NOT a documented guarantee (internal type +
        //    metadata survival are implementation details) - re-verify with the
        //    probe on new .NET versions. This leg is trim-mode-INDEPENDENT,
        //    which is the point: the legacy leg below cannot survive a
        //    metadata-preserving publish.
        //
        // 2) Legacy marker shape: with no [DynamicallyAccessedMembers]
        //    annotation on the GetField helper, the linker cannot prove the
        //    reflection target and generates no reflection data for the field,
        //    so GetField returns null on AOT (lean-RDG mode). Covers the
        //    hypothetical future where leg 1's metadata gets trimmed away.
        //    KNOWN HOLE (pre-existing, unchanged by this rewrite): a
        //    PublishTrimmed CoreCLR app trims the marker field too -> false
        //    "AOT" - do not gate JIT-only APIs on this check alone.
        private static bool CheckIsNativeAOT()
        {
            try
            {
                if (Type.GetType("Internal.Runtime.Augments.RuntimeAugments, System.Private.CoreLib") != null)
                {
                    return true;
                }
            }
            catch { }

            try
            {
                if (GetField(typeof(TrimThisClassOnAOT), "Field") == null)
                {
                    return true;
                }
            }
            catch (NotSupportedException)
            {
                return true;
            }
            return false;
        }

        private static bool CheckIsDynamicCodeEnabled()
        {
            try
            {
                var tdm = typeof(System.Reflection.Emit.OpCode).Assembly.GetType("System.Reflection.Emit.DynamicMethod");
                if (tdm == null)
                {
                    return false;
                }
                var con = tdm.GetConstructor(new Type[] { typeof(string), typeof(Type), typeof(Type[]) });
                if (con == null)
                {
                    return false;
                }
                var dm = con.Invoke(new object[] { "TestDynamicCodeEnable", typeof(void), Array.Empty<Type>() });
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static bool CheckIsHybridCLREnabled()
        {
            try
            {
                Assembly.Load(Array.Empty<byte>());
            }
            catch (NotSupportedException)
            {
                return false;
            }
            catch { }
            return true;
        }


        private class TrimThisClassOnAOT
        {
            public int Field;
        }
        private static FieldInfo GetField(Type t, string name)
        {
            return t.GetField(name);
        }
    }
}
