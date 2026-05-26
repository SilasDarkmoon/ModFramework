using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
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

        //public static ref IntPtr GetTypeHandleRef(this object obj)
        //{
        //    var offset = GetObjectHeaderSize() - IntPtr.Size;
        //    for (; ; )
        //    {
        //        RawRef refObj = default;
        //        refObj.SetRefObj(obj);
        //        var objAddress = refObj.Address;
        //        var handleAddress = (IntPtr)((ulong)objAddress + (ulong)offset);
        //        RawRef refHandle = default;
        //        refHandle.Address = handleAddress;
        //        ref IntPtr refResult = ref refHandle.GetRef<IntPtr>();
        //        RawRef refCheck = default;
        //        refCheck.SetRefObj(obj);
        //        var checkAddress = refCheck.Address;
        //        if (checkAddress == objAddress)
        //        {
        //            return ref refResult;
        //        }
        //    }
        //}

        public static FieldOffset GetFieldOffset<T>(this object obj, ref T field)
        {
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRefObj(obj);
                var objAddress = refObj.Address;
                RawRef refField = default;
                refField.SetRef(ref field);
                var fieldAddress = refField.Address;
                var offset = (int)((ulong)fieldAddress - (ulong)objAddress) - GetObjectHeaderSize();
                RawRef refCheck = default;
                refCheck.SetRefObj(obj);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return offset;
                }
            }
        }

        public static ref T GetFieldRef<T>(this object obj, int offset)
        {
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRefObj(obj);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((ulong)objAddress + (ulong)offset + (ulong)GetObjectHeaderSize());
                RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                RawRef refCheck = default;
                refCheck.SetRefObj(obj);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }

        public static FieldOffset GetFieldOffset<S, T>(this ref S parent, ref T field) where S : struct
        {
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRef(ref parent);
                var objAddress = refObj.Address;
                RawRef refField = default;
                refField.SetRef(ref field);
                var fieldAddress = refField.Address;
                var offset = (int)((ulong)fieldAddress - (ulong)objAddress);
                RawRef refCheck = default;
                refCheck.SetRef(ref parent);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return offset;
                }
            }
        }

        public static ref T GetFieldRef<S, T>(this ref S parent, int offset) where S : struct
        {
            for (; ; )
            {
                RawRef refObj = default;
                refObj.SetRef(ref parent);
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((ulong)objAddress + (ulong)offset);
                RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                RawRef refCheck = default;
                refCheck.SetRef(ref parent);
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }

        public static FieldOffset GetFieldOffsetCommon<S, T>(ref S parent, ref T field)
        {
            bool isParentValueType = typeof(S).IsValueType;
            for (; ; )
            {
                RawRef refObj = default;
                if (isParentValueType)
                {
                    refObj.SetRef(ref parent);
                }
                else
                {
                    refObj.SetRefObj(parent);
                }
                var objAddress = refObj.Address;
                RawRef refField = default;
                refField.SetRef(ref field);
                var fieldAddress = refField.Address;
                var offset = (int)((ulong)fieldAddress - (ulong)objAddress);
                RawRef refCheck = default;
                if (isParentValueType)
                {
                    refCheck.SetRef(ref parent);
                }
                else
                {
                    refCheck.SetRefObj(parent);
                }
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    if (isParentValueType)
                    {
                        return offset;
                    }
                    else
                    {
                        return offset - GetObjectHeaderSize();
                    }
                }
            }
        }

        public static ref T GetFieldRefCommon<S, T>(ref S parent, int offset)
        {
            bool isParentValueType = typeof(S).IsValueType;
            ulong extraOffset = 0;
            if (!isParentValueType)
            {
                extraOffset = (ulong)GetObjectHeaderSize();
            }
            for (; ; )
            {
                RawRef refObj = default;
                if (isParentValueType)
                {
                    refObj.SetRef(ref parent);
                }
                else
                {
                    refObj.SetRefObj(parent);
                }
                var objAddress = refObj.Address;
                var fieldAddress = (IntPtr)((ulong)objAddress + (ulong)offset + extraOffset);
                RawRef refField = default;
                refField.Address = fieldAddress;
                ref T refResult = ref refField.GetRef<T>();
                RawRef refCheck = default;
                if (isParentValueType)
                {
                    refCheck.SetRef(ref parent);
                }
                else
                {
                    refCheck.SetRefObj(parent);
                }
                var checkAddress = refCheck.Address;
                if (checkAddress == objAddress)
                {
                    return ref refResult;
                }
            }
        }

        public static ref byte GetHeadRef(this object obj)
        {
            return ref GetFieldRef<byte>(obj, 0);
        }

        public static ref byte GetHeadRef<S>(this ref S obj) where S : struct
        {
            return ref GetFieldRef<S, byte>(ref obj, 0);
        }

        public static ref byte GetHeadRefCommon<S>(ref S obj)
        {
            return ref GetFieldRefCommon<S, byte>(ref obj, 0);
        }

        private static class FieldInfoPlatDep
        {
        }

        private abstract class SizeMeasurer
        {
            public static readonly Dictionary<RuntimeTypeHandle, int> _Cache = new Dictionary<RuntimeTypeHandle, int>();

            public abstract int GetSize();

            public static int GetSize(Type type)
            {
                lock (_Cache)
                {
                    if (_Cache.TryGetValue(type.TypeHandle, out var size))
                    {
                        return size;
                    }
                }
                if (type.IsByRef || type.IsPointer)
                {
                    int size = IntPtr.Size;
                }
                if (type.IsValueType)
                {
                    int size = -1;
                    switch (Type.GetTypeCode(type))
                    {
                        case TypeCode.Empty: size = 0; break;
                        case TypeCode.Boolean: size = 1; break;
                        case TypeCode.Char: size = 2; break;
                        case TypeCode.SByte: size = 1; break;
                        case TypeCode.Byte: size = 1; break;
                        case TypeCode.Int16: size = 2; break;
                        case TypeCode.UInt16: size = 2; break;
                        case TypeCode.Int32: size = 4; break;
                        case TypeCode.UInt32: size = 4; break;
                        case TypeCode.Int64: size = 8; break;
                        case TypeCode.UInt64: size = 8; break;
                        case TypeCode.Single: size = 4; break;
                        case TypeCode.Double: size = 8; break;
                        case TypeCode.Decimal: size = 16; break;
                    }

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
                            _Cache[type.TypeHandle] = size;
                        }
                        return size;
                    }

                    //var measurerType = typeof(SizeMeasurer<>).MakeGenericType(type);
                    //var measurer = (SizeMeasurer)Activator.CreateInstance(measurerType);
                    //return measurer.GetSize();
                }
                //else
                {
                    int size = 0;
                    int offset = -1;
                    var fields = GetAllFields(type);
                    foreach (var field in fields)
                    {
                        var foffset = GetFieldOffset(field);
                        if (foffset > offset)
                        {
                            offset = foffset;
                            size = foffset;
                            var fieldType = field.FieldType;
                            if (fieldType.IsValueType && !fieldType.IsPointer && !fieldType.IsByRef)
                            {
                                size += GetSize(fieldType);
                            }
                            else
                            {
                                size += IntPtr.Size;
                            }
                        }
                    }
                    lock (_Cache)
                    {
                        _Cache[type.TypeHandle] = size;
                    }
                    return size;
                }
            }
            public static int GetSize<T>()
            {
                var type = typeof(T);
                if (type.IsValueType)
                {
                    int size;
                    lock (_Cache)
                    {
                        if (_Cache.TryGetValue(type.TypeHandle, out size))
                        {
                            return size;
                        }
                    }
                    unsafe
                    {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                        size = sizeof(T);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                    }
                    lock (_Cache)
                    {
                        _Cache[type.TypeHandle] = size;
                    }
                    return size;
                }
                else
                {
                    return GetSize(type);
                }
            }
        }

        private class SizeMeasurer<T> : SizeMeasurer where T : struct
        {
            public override int GetSize()
            {
                unsafe
                {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                    return sizeof(T);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                }
            }

            public SizeMeasurer()
            {
                lock (_Cache)
                {
                    _Cache[typeof(T).TypeHandle] = GetSize();
                }
            }
        }

        public static int Size(this Type t)
        {
            return SizeMeasurer.GetSize(t);
        }
        public static int Size<T>()
        {
            return SizeMeasurer.GetSize<T>();
        }
        public static int Size(this object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return Size(obj.GetType());
        }
        public static int Size<T>(ref T obj)
        {
            if (typeof(T).IsValueType)
            {
                return Size<T>();
            }
            else
            {
                return Size(obj);
            }
        }
        public static int Size<T>(this T obj)
        {
            if (typeof(T).IsValueType)
            {
                return Size<T>();
            }
            else
            {
                return Size(obj);
            }
        }

        private static readonly Dictionary<RuntimeFieldHandle, int> _FieldOffsetCache = new Dictionary<RuntimeFieldHandle, int>();
        public static FieldOffset GetFieldOffset(this FieldInfo fi)
        {
            int offset = 0;
            lock (_FieldOffsetCache)
            {
                if (_FieldOffsetCache.TryGetValue(fi.FieldHandle, out offset))
                {
                    return offset;
                }
            }
            try
            {
                return offset = PlatDep.GetFieldOffset(fi);
            }
            finally
            {
                lock (_FieldOffsetCache)
                {
                    _FieldOffsetCache[fi.FieldHandle] = offset;
                }
            }
        }
        public static ref T GetFieldRef<T>(this object target, FieldInfo fieldInfo)
        {
            if (target == null)
            {
                throw new ArgumentNullException(nameof(target));
            }
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }
            var offset = GetFieldOffset(fieldInfo);
            return ref GetFieldRef<T>(target, offset);
        }
        public static ref T GetFieldRef<S, T>(this ref S target, FieldInfo fieldInfo) where S : struct
        {
            if (fieldInfo == null)
            {
                throw new ArgumentNullException(nameof(fieldInfo));
            }
            var offset = GetFieldOffset(fieldInfo);
            return ref GetFieldRef<S, T>(ref target, offset);
        }
        public static ref T GetFieldRefCommon<S, T>(ref S target, FieldInfo fieldInfo)
        {
            if (!typeof(S).IsValueType)
            {
                return ref GetFieldRef<T>(target, fieldInfo);
            }
            else
            {
                if (fieldInfo == null)
                {
                    throw new ArgumentNullException(nameof(fieldInfo));
                }
                var offset = GetFieldOffset(fieldInfo);
                return ref GetFieldRefCommon<S, T>(ref target, offset);
            }
        }
        public static T GetFieldValue<T>(this object target, FieldInfo fieldInfo)
        {
            return GetFieldRef<T>(target, fieldInfo);
        }
        public static T GetFieldValue<S, T>(this S target, FieldInfo fieldInfo) where S : struct
        {
            return GetFieldRef<S, T>(ref target, fieldInfo);
        }
        public static T GetFieldValueCommon<S, T>(this S target, FieldInfo fieldInfo)
        {
            return GetFieldRefCommon<S, T>(ref target, fieldInfo);
        }
        public static void SetFieldValue<T>(this object target, FieldInfo fieldInfo, T value)
        {
            GetFieldRef<T>(target, fieldInfo) = value;
        }
        public static void SetFieldValue<S, T>(this ref S target, FieldInfo fieldInfo, T value) where S : struct
        {
            GetFieldRef<S, T>(ref target, fieldInfo) = value;
        }
        public static void SetFieldValueCommon<S, T>(ref S target, FieldInfo fieldInfo, T value)
        {
            GetFieldRefCommon<S, T>(ref target, fieldInfo) = value;
        }

        public static FieldInfo[] GetAllFields(this Type type)
        {
            List<FieldInfo> list = new List<FieldInfo>();
            var curType = type;
            while (curType != null)
            {
                list.AddRange(curType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
                curType = curType.BaseType;
            }
            return list.ToArray();
        }

        /// <remarks>
        /// No cache! Can be quite slow.
        /// If offset is out of the class, it will return null.
        /// Offset may be in the middle of a field, so the returned FieldInfo start ahead of the offset.
        /// </remark>
        public static FieldInfo GetFieldInfo(this Type type, int offset)
        {
            var fields = GetAllFields(type);
            foreach (var field in fields)
            {
                int foffset = GetFieldOffset(field);
                if (offset == foffset)
                {
                    return field;
                }
                else if (offset < foffset)
                {
                    continue;
                }
                int delta;
                var fieldType = field.FieldType;
                if (fieldType.IsValueType)
                {
                    delta = Size(fieldType);
                }
                else
                {
                    delta = IntPtr.Size;
                }
                if (offset < foffset + delta)
                {
                    return field;
                }
            }
            return null;
        }
        public static FieldInfo GetFieldInfo(this object target, int offset)
        {
            return GetFieldInfo(target.GetType(), offset);
        }
        public static FieldInfo GetFieldInfo<S>(this ref S target, int offset) where S : struct
        {
            return GetFieldInfo(typeof(S), offset);
        }
        public static FieldInfo GetFieldInfo<S>(this S target, int offset)
        {
            if (typeof(S).IsValueType)
            {
                return GetFieldInfo(typeof(S), offset);
            }
            else
            {
                return GetFieldInfo(target.GetType(), offset);
            }
        }
        public static FieldInfo GetFieldInfo<T>(this object target, ref T field)
        {
            return GetFieldInfo(target, GetFieldOffset(target, ref field));
        }
        public static FieldInfo GetFieldInfo<S, T>(this ref S target, ref T field) where S : struct
        {
            return GetFieldInfo(ref target, GetFieldOffset(ref target, ref field));
        }
        public static FieldInfo GetFieldInfoCommon<S, T>(ref S target, ref T field)
        {
            if (typeof(S).IsValueType)
            {
                return GetFieldInfo(target, GetFieldOffsetCommon(ref target, ref field));
            }
            else
            {
                return GetFieldInfo(target, ref field);
            }
        }

        public static ref T GetField<S, T>(this ref S target, FieldIndicator<T> fi) where S : struct
        {
            return ref GetFieldRef<S, T>(ref target, fi.Offset);
        }
        public static ref T GetField<T>(this object target, FieldIndicator<T> fi)
        {
            return ref GetFieldRef<T>(target, fi.Offset);
        }
        public static FieldIndicator<T> OfType<T>(this FieldInfo fieldInfo)
        {
            return new FieldIndicator<T>(fieldInfo);
        }
        public static FieldIndicator<T> OfType<T>(this FieldOffset fo)
        {
            return fo;
        }
    }

    public interface IFieldOffset
    {
        int Offset { get; }
    }
    public struct FieldOffset : IFieldOffset, IEquatable<FieldOffset>, IEquatable<int>, IComparable<FieldOffset>, IComparable<int>
    {
        private readonly int _Offset;
        public int Offset => _Offset;

        public FieldOffset(int offset)
        {
            _Offset = offset;
        }
        public static implicit operator FieldOffset(int offset)
        {
            return new FieldOffset(offset);
        }
        public static implicit operator int(FieldOffset fieldOffset)
        {
            return fieldOffset._Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldOffset fi) return _Offset == fi._Offset;
            else if (obj is int i) return _Offset == i;
            else if (obj is IFieldOffset fo) return _Offset == fo.Offset;
            return int.Equals(_Offset, obj);
        }
        public override int GetHashCode()
        {
            return _Offset.GetHashCode();
        }
        public override string ToString()
        {
            return _Offset.ToString();
        }

        public bool Equals(FieldOffset other)
        {
            return _Offset == other._Offset;
        }
        public bool Equals(int other)
        {
            return _Offset == other;
        }
        public int CompareTo(FieldOffset other)
        {
            return _Offset.CompareTo(other._Offset);
        }
        public int CompareTo(int other)
        {
            return _Offset.CompareTo(other);
        }
    }

    public struct FieldIndicator<T> : IFieldOffset, IEquatable<FieldIndicator<T>>, IEquatable<FieldOffset>, IEquatable<int>, IComparable<FieldIndicator<T>>, IComparable<FieldOffset>, IComparable<int>
    {
        private readonly int _Offset;
        public int Offset => _Offset;

        public FieldIndicator(FieldInfo fi)
        {
            _Offset = fi.GetFieldOffset();
        }
        public FieldIndicator(int offset)
        {
            _Offset = offset;
        }
        public static implicit operator FieldIndicator<T>(int offset)
        {
            return new FieldIndicator<T>(offset);
        }
        public static implicit operator int(FieldIndicator<T> fieldOffset)
        {
            return fieldOffset._Offset;
        }
        public static implicit operator FieldIndicator<T>(FieldOffset offset)
        {
            return new FieldIndicator<T>(offset);
        }
        public static implicit operator FieldOffset(FieldIndicator<T> fieldOffset)
        {
            return fieldOffset._Offset;
        }

        public override bool Equals(object obj)
        {
            if (obj is FieldIndicator<T> fi) return _Offset == fi._Offset;
            else if (obj is int i) return _Offset == i;
            else if (obj is IFieldOffset fo) return _Offset == fo.Offset;
            return int.Equals(_Offset, obj);
        }
        public override int GetHashCode()
        {
            return _Offset.GetHashCode();
        }
        public override string ToString()
        {
            return _Offset.ToString();
        }

        public bool Equals(FieldIndicator<T> other)
        {
            return _Offset == other.Offset;
        }
        public bool Equals(FieldOffset other)
        {
            return _Offset == other.Offset;
        }
        public bool Equals(int other)
        {
            return _Offset == other;
        }
        public int CompareTo(FieldIndicator<T> other)
        {
            return _Offset.CompareTo(other.Offset);
        }
        public int CompareTo(FieldOffset other)
        {
            return _Offset.CompareTo(other.Offset);
        }
        public int CompareTo(int other)
        {
            return _Offset.CompareTo(other);
        }
    }
}
