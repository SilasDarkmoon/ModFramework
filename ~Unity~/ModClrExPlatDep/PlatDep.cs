using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static class PlatDep
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct NativeIl2CppFieldInfo
        {
            public IntPtr declaringType;
            public IntPtr name;
            public IntPtr type;
            public uint offset;
            public uint attributes;
            public uint token;
            public uint defaultValueIndex;
            public IntPtr parentGenericClass;
        }

        private class ObjectHeaderStub
        {
            public int StubField;
        }
        private static int _ObjectHeaderSize;

        private static int GetObjectHeaderSize()
        {
            if (_ObjectHeaderSize <= 0)
            {
                var o = new ObjectHeaderStub();
                for (; ; )
                {
                    RawRef refO = default;
                    refO.SetRefObj(o);
                    var addo = refO.Address;
                    RawRef refF = default;
                    refF.SetRef(ref o.StubField);
                    var addf = refF.Address;
                    var headerSize = (int)((ulong)addf - (ulong)addo);
                    RawRef refC = default;
                    refC.SetRefObj(o);
                    var addc = refC.Address;
                    if (addc == addo)
                    {
                        _ObjectHeaderSize = headerSize;
                        break;
                    }
                }
            }
            return _ObjectHeaderSize;
        }

        public static int GetFieldOffset(FieldInfo field)
        {
            var handle = field.FieldHandle;
            RawRef r = new RawRef();
            r.Address = handle.Value;
            var fd = r.GetValue<NativeIl2CppFieldInfo>();

            return (int)fd.offset - GetObjectHeaderSize();
        }

        public static FieldInfo[] GetAllFields(Type type)
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
}
