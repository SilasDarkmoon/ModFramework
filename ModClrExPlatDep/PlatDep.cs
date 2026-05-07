using System;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static class PlatDep
    {
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

        public static int GetFieldOffset(FieldInfo field)
        {
            var handle = field.FieldHandle;
            RawRef r = new RawRef();
            r.Address = handle.Value;
            var fd = r.GetValue<FieldDesc>();
            return fd.Offset;
        }

        public static FieldInfo[] GetAllFields(Type type)
        {
            return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        }
    }
}
