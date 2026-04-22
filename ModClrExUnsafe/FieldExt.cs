using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Mod
{
    public static class FieldExt
    {
        private class ObjectHeaderStub
        {
            public int StubField;
        }
        private static int _ObjectHeaderSize;

        public static int GetObjectHeaderSize()
        {
            if (_ObjectHeaderSize <= 0)
            {
                var o = new ObjectHeaderStub();
                for (; ; )
                {
                    Capstones.ByRefUtils.RawRef refO = default;
                    refO.SetRefObj(o);
                    var addo = refO.Address;
                    Capstones.ByRefUtils.RawRef refF = default;
                    refF.SetRef(ref o.StubField);
                    var addf = refF.Address;
                    var headerSize = (int)((ulong)addf - (ulong)addo);
                    Capstones.ByRefUtils.RawRef refC = default;
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

        public static int GetFieldOffset<T>(object obj, ref T field)
        {
            for (; ; )
            {
                Capstones.ByRefUtils.RawRef refObj = default;
                refObj.SetRefObj(obj);
                var objAddress = refObj.Address;
                Capstones.ByRefUtils.RawRef refField = default;
                refField.SetRef(ref field);
                var fieldAddress = refField.Address;
                var offset = (int)((ulong)fieldAddress - (ulong)objAddress) - GetObjectHeaderSize();
                Capstones.ByRefUtils.RawRef refCheck = default;
                refCheck.SetRefObj(obj);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return offset;
                }
            }
        }

        public static ref T GetFieldRef<T>(object obj, int offset)
        {
            for (; ; )
            {
                Capstones.ByRefUtils.RawRef refObj = default;
                refObj.SetRefObj(obj);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((ulong)objAddress + (ulong)offset + (ulong)GetObjectHeaderSize());
                Capstones.ByRefUtils.RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                Capstones.ByRefUtils.RawRef refCheck = default;
                refCheck.SetRefObj(obj);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }

        public static int GetFieldOffset<S, T>(ref S parent, ref T field) where S : struct
        {
            for (; ; )
            {
                Capstones.ByRefUtils.RawRef refObj = default;
                refObj.SetRef(ref parent);
                var objAddress = refObj.Address;
                Capstones.ByRefUtils.RawRef refField = default;
                refField.SetRef(ref field);
                var fieldAddress = refField.Address;
                var offset = (int)((ulong)fieldAddress - (ulong)objAddress);
                Capstones.ByRefUtils.RawRef refCheck = default;
                refCheck.SetRef(ref parent);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return offset;
                }
            }
        }

        public static ref T GetFieldRef<S, T>(ref S parent, int offset) where S : struct
        {
            for (; ; )
            {
                Capstones.ByRefUtils.RawRef refObj = default;
                refObj.SetRef(ref parent);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((ulong)objAddress + (ulong)offset);
                Capstones.ByRefUtils.RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                Capstones.ByRefUtils.RawRef refCheck = default;
                refCheck.SetRef(ref parent);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }

        public static ref byte GetHeadRef(object obj)
        {
            return ref GetFieldRef<byte>(obj, 0);
        }

        public static ref byte GetHeadRef<S>(ref S obj) where S : struct
        {
            return ref GetFieldRef<S, byte>(ref obj, 0);
        }

        private abstract class SizeMeasurer
        {
            public static Dictionary<Type, int> _Cache = new Dictionary<Type, int>();

            public abstract int GetSize();

            public static int GetSize(Type type)
            {
                lock (_Cache)
                {
                    if (_Cache.TryGetValue(type, out var size))
                    {
                        return size;
                    }
                }
                if (type.IsValueType)
                {
                    int size = Type.GetTypeCode(type) switch
                    {
                        TypeCode.Empty => 0,
                        TypeCode.Boolean => 1,
                        TypeCode.Char => 2,
                        TypeCode.SByte => 1,
                        TypeCode.Byte => 1,
                        TypeCode.Int16 => 2,
                        TypeCode.UInt16 => 2,
                        TypeCode.Int32 => 4,
                        TypeCode.UInt32 => 4,
                        TypeCode.Int64 => 8,
                        TypeCode.UInt64 => 8,
                        TypeCode.Single => 4,
                        TypeCode.Double => 8,
                        TypeCode.Decimal => 16,
                        _ => -1
                    };

                    if (size < 0)
                    {
                        if (type == typeof(IntPtr) || type == typeof(UIntPtr))
                        {
                            size = IntPtr.Size;
                        }
                    }
                    if (size >= 0)
                    {
                        lock (_Cache)
                        {
                            _Cache[type] = size;
                        }
                        return size;
                    }

                    //var measurerType = typeof(SizeMeasurer<>).MakeGenericType(type);
                    //var measurer = (SizeMeasurer)Activator.CreateInstance(measurerType);
                    //return measurer.GetSize();
                }
                //else
                {
                    var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    int size = 0;
                    foreach (var field in fields)
                    {
                        var fieldType = field.FieldType;
                        if (fieldType.IsValueType)
                        {
                            size += GetSize(fieldType);
                        }
                        else
                        {
                            size += IntPtr.Size;
                        }
                    }
                    lock (_Cache)
                    {
                        _Cache[type] = size;
                    }
                    return size;
                }
            }
        }

        private class SizeMeasurer<T> : SizeMeasurer where T : struct
        {
            public override int GetSize()
            {
                unsafe
                {
                    return sizeof(T);
                }
            }

            public SizeMeasurer()
            {
                lock (_Cache)
                {
                    _Cache[typeof(T)] = GetSize();
                }
            }
        }

        public static int SizeOf(Type t)
        {
            return SizeMeasurer.GetSize(t);
        }
        public static int SizeOf<T>()
        {
            return SizeMeasurer.GetSize(typeof(T));
        }
        public static int SizeOf(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return SizeOf(obj.GetType());
        }

        public static int GetFieldOffset(FieldInfo fi)
        {
            var type = fi.ReflectedType;
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            int offset = 0;
            foreach (var field in fields)
            {
                if (field == fi)
                {
                    return offset;
                }
                var fieldType = field.FieldType;
                if (fieldType.IsValueType)
                {
                    offset += SizeOf(fieldType);
                }
                else
                {
                    offset += IntPtr.Size;
                }
            }
            return offset;
        }
    }
}
