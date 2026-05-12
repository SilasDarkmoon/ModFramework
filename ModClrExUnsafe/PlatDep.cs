using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        private static partial class PlatDep
        {
            private static bool _IsUnity;
            static PlatDep()
            {
                _IsUnity = Type.GetType("UnityEngine.Application, UnityEngine.CoreModule") != null;
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct FieldDesc
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

            [StructLayout(LayoutKind.Sequential)]
            public struct NativeIl2CppFieldInfo
            {
#pragma warning disable IDE1006 // 命名样式
                public IntPtr declaringType;
                public IntPtr name;
                public IntPtr type;
                public uint offset;
                public uint attributes;
                public uint token;
                public uint defaultValueIndex;
                public IntPtr parentGenericClass;
#pragma warning restore IDE1006 // 命名样式
            }

            public static int GetFieldOffset(FieldInfo field)
            {
                if (_IsUnity)
                {
                    var handle = field.FieldHandle;
                    RawRef r = new RawRef();
                    r.Address = handle.Value;
                    var fd = r.GetValue<NativeIl2CppFieldInfo>();
                    return (int)fd.offset - GetObjectHeaderSize();

                }
                else
                {
                    var handle = field.FieldHandle;
                    RawRef r = new RawRef();
                    r.Address = handle.Value;
                    var fd = r.GetValue<FieldDesc>();
                    return fd.Offset;
                }
            }

            public static FieldInfo[] GetAllFields(Type type)
            {
                if (_IsUnity)
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
                }

                return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }
        }
    }
}
