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

        private static RuntimeCategory _Runtime;
        public static RuntimeCategory Runtime => _Runtime;

        private static bool _IsUnity;
        public static bool IsUnity => _IsUnity;

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

        public static bool IsAOT => _IsNativeAOT || _IsIl2Cpp;

        static RuntimeInfo()
        {
            _IsUnity = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule") != null;
            if (_IsUnity)
            {
                _IsIl2Cpp = CheckIsIl2Cpp();
                if (_IsIl2Cpp)
                {
                    _Runtime = RuntimeCategory.UnityIl2Cpp;
                }
                else
                {
                    _IsMono = true;
                    _Runtime = RuntimeCategory.UnityMono;
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
            }
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

        private static bool CheckIsNativeAOT()
        {
            try
            {
                var fi = GetField(typeof(TrimThisClassOnAOT), "Field");
                return fi == null;
            }
            catch (NotSupportedException)
            {
                return true;
            }
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

    public static partial class UnsafeEx
    {
        private static partial class PlatDep
        {
            private interface IPlatDep
            {
                int GetFieldOffset(FieldInfo field);
                FieldInfo[] GetAllFields(Type type);
            }
            private static IPlatDep _PlatDep;

            private class PlatDepUnity : IPlatDep
            {
                [StructLayout(LayoutKind.Sequential)]
                private struct NativeIl2CppFieldInfo
                {
#pragma warning disable IDE1006 // 命名样式
                    public IntPtr declaringType;
                    public IntPtr name;
                    public IntPtr type;
                    public uint offset;
                    //public uint attributes;
                    //public uint token;
                    //public uint defaultValueIndex;
                    //public IntPtr parentGenericClass;
#pragma warning restore IDE1006 // 命名样式
                }

                public int GetFieldOffset(FieldInfo field)
                {
                    var handle = field.FieldHandle;
                    RawRef r = new RawRef();
                    r.Address = handle.Value;
                    var fd = r.GetValue<NativeIl2CppFieldInfo>();
                    return (int)fd.offset - GetObjectHeaderSize();
                }

                public FieldInfo[] GetAllFields(Type type)
                {
                    if (typeof(Delegate).IsAssignableFrom(type))
                    {
                        HashSet<string> fieldNames = new HashSet<string>();
                        List<FieldInfo> fields = new List<FieldInfo>();
                        var curType = type;
                        while (curType != null)
                        {
                            var curFields = curType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                            foreach (var field in curFields)
                            {
                                if (fieldNames.Add(field.Name))
                                {
                                    fields.Add(field);
                                }
                            }
                            curType = curType.BaseType;
                        }
                        return fields.ToArray();
                    }

                    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            private class PlatDepDotNet : IPlatDep
            {
                [StructLayout(LayoutKind.Sequential)]
                private struct FieldDesc
                {
#pragma warning disable IDE1006 // 命名样式
                    private readonly IntPtr m_pMTOfEnclosingClass;
                    private readonly uint m_dword1;
                    private readonly uint m_dword2;
#pragma warning restore IDE1006 // 命名样式

                    public int Offset => (int)(m_dword2 & 0x07FFFFFF);
                    public int MB => (int)(m_dword1 & 0x00FFFFFF);
                    public bool IsStatic => (m_dword1 & (1u << 24)) != 0;
                    public bool IsThreadLocal => (m_dword1 & (1u << 25)) != 0;
                    public bool IsRVA => (m_dword1 & (1u << 26)) != 0;
                    public int Protection => (int)((m_dword1 >> 27) & 0x7);
                    public bool RequiresFullMbValue => (m_dword1 & (1u << 30)) != 0;
                    public int Type => (int)((m_dword2 >> 27) & 0x1F);
                }

                public int GetFieldOffset(FieldInfo field)
                {
                    var handle = field.FieldHandle;
                    RawRef r = new RawRef();
                    r.Address = handle.Value;
                    var fd = r.GetValue<FieldDesc>();
                    return fd.Offset;
                }

                public FieldInfo[] GetAllFields(Type type)
                {
                    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            private class PlatDepNativeAOT : IPlatDep
            {
                private static Func<int> _DelGetOffsetTemplate;
                [ThreadStatic] private static Func<int> _DelGetOffset;

                public int GetFieldOffset(FieldInfo field)
                {
                    // TODO: Must add [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.FieldInfos.NativeFormat.NativeFormatRuntimeFieldInfo", "System.Private.CoreLib")] somewhere.
                    if (_DelGetOffset == null)
                    {
                        if (_DelGetOffsetTemplate == null)
                        {
                            var prop = field.GetType().GetProperty("Offset", BindingFlags.NonPublic | BindingFlags.Instance);
                            var gm = prop.GetGetMethod(true);
                            _DelGetOffset = _DelGetOffsetTemplate = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), field, gm);
                            return _DelGetOffset();
                        }
                        else
                        {
                            _DelGetOffset = (Func<int>)_DelGetOffsetTemplate.Clone();
                            _DelGetOffset.GetField<object>(0) = field;
                            return _DelGetOffset();
                        }
                    }
                    else
                    {
                        _DelGetOffset.GetField<object>(0) = field;
                        return _DelGetOffset();
                    }
                }
                public FieldInfo[] GetAllFields(Type type)
                {
                    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                }
            }

            static PlatDep()
            {
                if (RuntimeInfo.IsUnity)
                {
                    _PlatDep = new PlatDepUnity();
                }
                else
                {
                    if (RuntimeInfo.IsNativeAOT)
                    {
                        _PlatDep = new PlatDepNativeAOT();
                    }
                    else if (RuntimeInfo.IsMono)
                    {
                        _PlatDep = new PlatDepUnity();
                    }
                    else
                    {
                        _PlatDep = new PlatDepDotNet();
                    }
                }
            }

            public static int GetFieldOffset(FieldInfo field)
            {
                return _PlatDep.GetFieldOffset(field);
            }

            public static FieldInfo[] GetAllFields(Type type)
            {
                return _PlatDep.GetAllFields(type);
            }
        }
    }
}
