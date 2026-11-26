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

        private static int _ArrayFirstElementOffset;
        public static int GetArrayFirstElementOffset()
        {
            if (_ArrayFirstElementOffset <= 0)
            {
                var arr = new int[1];
                for (; ; )
                {
                    RawRef refArr = default;
                    refArr.SetRefObj(arr);
                    var addArr = refArr.Address;
                    RawRef refElem = default;
                    refElem.SetRef(ref arr[0]);
                    var addElem = refElem.Address;
                    var offset = (int)((ulong)addElem - (ulong)addArr);
                    RawRef refCheck = default;
                    refCheck.SetRefObj(arr);
                    var addCheck = refCheck.Address;
                    if (addCheck == addArr)
                    {
                        _ArrayFirstElementOffset = offset - GetObjectHeaderSize();
                        break;
                    }
                }
            }
            return _ArrayFirstElementOffset;
        }

        // Calibration twin of GetArrayFirstElementOffset for strings: the
        // first char's offset, already minus the object header (the cached
        // array value is adjusted the same way in its calibration). The
        // fixed-char probe adapts to every runtime - IL2CPP and HybridCLR
        // hot strings included - no C API needed.
        private static int _StringFirstCharOffset;
        private static int GetStringFirstCharOffset()
        {
            if (_StringFirstCharOffset <= 0)
            {
                var s = "a";
                long addC, addS;
                unsafe
                {
                    fixed (char* p = s)
                    {
                        addC = (long)p;
                        addS = (long)RawRef.Of(s).Address;
                    }
                }
                var offset = (int)(addC - addS);
                _StringFirstCharOffset = offset - GetObjectHeaderSize();
            }
            return _StringFirstCharOffset;
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

        public static ref byte GetFirstElementRef(this Array arr)
        {
            if (arr == null)
            {
                return ref Ref.GetEmptyRef<byte>();
            }
            return ref GetFieldRef<byte>(arr, GetArrayFirstElementOffset());
        }
        public static ref T GetFirstElementRef<T>(this Array arr)
        {
            ref byte firstElemRef = ref GetFirstElementRef(arr);
            return ref ConvertRef<byte, T>(ref firstElemRef);
        }
        public static ref T GetFirstElementRef<T>(this T[] arr)
        {
            return ref GetFirstElementRef<T>((Array)arr);
        }
        public static ref char GetFirstElementRef(this string str)
        {
            if (str == null)
            {
                return ref Ref.GetEmptyRef<char>();
            }
            return ref GetFieldRef<char>(str, GetStringFirstCharOffset());
        }

        private static class SizeMeasurer
        {
            public static readonly Dictionary<RuntimeTypeHandle, int> _Cache = new Dictionary<RuntimeTypeHandle, int>();
            private static readonly Dictionary<RuntimeTypeHandle, (bool blittable, bool hasObjectField, int pack)> _CacheForPack = new Dictionary<RuntimeTypeHandle, (bool blittable, bool hasObjectField, int pack)>();

            public static (bool blittable, bool hasObjectField, int pack) GetBlittablePack(Type type)
            {
                if (type.IsByRef || type.IsPointer)
                {
                    return (true, false, IntPtr.Size);
                }
                lock (_CacheForPack)
                {
                    if (_CacheForPack.TryGetValue(type.TypeHandle, out var packInfo))
                    {
                        return packInfo;
                    }
                }
                if (type.IsValueType)
                {
                    int size = -1;
                    TypeCode typeCode = Type.GetTypeCode(type);
                    switch (typeCode)
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
                        // The 8-byte scalars: CoreCLR's 32-bit runtimes align
                        // them to 4 (measured: G<string> = 12 on net48/net10
                        // x86 - the long tail sits right behind the reference),
                        // Mono keeps the natural 8 (measured: Unity Mono x86
                        // G<string> = 16). IL2CPP x86 is unmeasured, it keeps
                        // the historical 8.
                        case TypeCode.Int64:
                        case TypeCode.UInt64:
                        case TypeCode.DateTime:
                            size = (IntPtr.Size >= 8 || RuntimeInfo.IsMono || RuntimeInfo.IsIl2Cpp) ? 8 : 4;
                            break;
                        case TypeCode.Single: size = 4; break;
                        case TypeCode.Double: size = 8; break;
                        case TypeCode.Decimal: size = 4; break;
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
                        lock (_CacheForPack)
                        {
                            return _CacheForPack[type.TypeHandle] = (true, false, size);
                        }
                    }
                }
                //else
                {
                    int autopack = 1;
                    bool isblittable = type.IsValueType;
                    bool hasObjectField = false;
                    var fields = GetAllFields(type);
                    foreach (var field in fields)
                    {
                        var fieldType = field.FieldType;
                        if (fieldType.IsPointer || fieldType.IsByRef)
                        {
                            autopack = Math.Max(autopack, IntPtr.Size);
                        }
                        else if (fieldType.IsValueType)
                        {
                            (var fieldisblittable, var fieldHasChildObjectField, var fieldpack) = GetBlittablePack(fieldType);
                            isblittable = isblittable && fieldisblittable;
                            hasObjectField = hasObjectField || fieldHasChildObjectField;
                            autopack = Math.Max(autopack, fieldpack);
                        }
                        else
                        {
                            isblittable = false;
                            hasObjectField = true;
                            autopack = Math.Max(autopack, IntPtr.Size);
                        }
                    }
                    // The returned pack is the alignment the runtime rounds the
                    // walked field extent up to (A). Baseline for every layout
                    // kind is the largest field alignment (N) - the traversal
                    // above has already computed it as `autopack`.
                    int pack = autopack;
                    var sla = type.StructLayoutAttribute;
                    bool sequential = sla == null ? type.IsValueType : sla.Value == LayoutKind.Sequential;
                    int attrPack = sla?.Pack ?? 0;
                    if (sequential && attrPack > 0)
                    {
                        if (hasObjectField)
                        {
                            // Measured (Mono x64/x86, IL2CPP; CoreCLR discards
                            // the layout, which coincides on both bitnesses): a
                            // declared Pack caps the alignment of the VALUE
                            // fields, but GC-reference slots keep pointer size -
                            // the pointer floor sits OUTSIDE the cap
                            // (SeqP2RefMid Pack=2: A = 4 on x86 / 8 on x64, not
                            // 2 and not N=8).
                            pack = Math.Max(IntPtr.Size, Math.Min(autopack, attrPack));
                        }
                        else
                        {
                            // Cap semantics everywhere the Pack is honored:
                            // min(Pack, natural) - a Pack above the natural
                            // alignment never inflates it (P16 measured: the
                            // long lands at offset 4).
                            pack = Math.Min(autopack, attrPack);
                        }
                    }
                    // Explicit: offsets are hand-declared - the runtime ignores
                    // the declared Pack for placement AND the trailing round
                    // (measured: extent 9 with a long rounds to 16 = N).
                    // Auto: the layout engine regenerates everything, the Pack
                    // means nothing. Both keep A = N, hence no cap above.

                    lock (_CacheForPack)
                    {
                        return _CacheForPack[type.TypeHandle] = (isblittable, hasObjectField, pack);
                    }
                }
            }
            public static int GetPack(Type type)
            {
                return GetBlittablePack(type).pack;
            }

            // Whether the runtime applies a declared StructLayoutAttribute.Size
            // as a floor on this type's layout. Measured per
            // runtime/layout/blittability (SizeofMatrixProbe [Y] matrix):
            //   Mono / IL2CPP: never dropped - any layout kind, blittable or
            //     not, value type or class. (IL2CPP reports the computed size
            //     instead of the declared one for value types, which is even
            //     better: it IS the exact runtime size. HybridCLR hot types
            //     lie in both directions - that needs the il2cpp C API route,
            //     not this walk.)
            //   CoreCLR family: Sequential - only while the layout survives:
            //     GC-reference fields discard the whole ClassLayout (auto
            //     layout), the declared Size goes with it; Explicit - value
            //     types always honor it, classes only on .NET Framework and
            //     only without refs (.NET Core/5+ ignores it for classes
            //     outright); Auto - never.
            private static bool HonorsDeclaredSize(Type type, StructLayoutAttribute sla, bool hasObjectField)
            {
                if (RuntimeInfo.IsMono || RuntimeInfo.IsIl2Cpp)
                {
                    return true;
                }
                var kind = sla == null ? (type.IsValueType ? LayoutKind.Sequential : LayoutKind.Auto) : sla.Value;
                switch (kind)
                {
                    case LayoutKind.Sequential:
                        return !hasObjectField;
                    case LayoutKind.Explicit:
                        return type.IsValueType || (RuntimeInfo.IsNetFramework && !hasObjectField);
                    default:
                        return false;
                }
            }

            public static int GetSizeByFields(Type type)
            {
                int size = 0;
                var fields = GetAllFields(type);
                foreach (var field in fields)
                {
                    var foffset = GetFieldOffset(field);
                    var fieldType = field.FieldType;
                    // The extent is the largest field END, not the end of
                    // the field with the largest offset: overlapping
                    // (explicit/union) layouts would be cut short otherwise
                    // ({byte@0, long@0} -> 1 instead of 8).
                    int end;
                    if (fieldType.IsValueType && !fieldType.IsPointer && !fieldType.IsByRef)
                    {
                        end = foffset + GetSize(fieldType);
                    }
                    else
                    {
                        end = foffset + IntPtr.Size;
                    }
                    if (end > size)
                    {
                        size = end;
                    }
                }
                var packInfo = GetBlittablePack(type);
                if (packInfo.pack > 0)
                {
                    size = (size + packInfo.pack - 1) / packInfo.pack * packInfo.pack;
                }
                // A declared StructLayoutAttribute.Size is a floor the
                // runtime applies AFTER the trailing round - it is never
                // re-rounded (Size=9 -> 9, not 16) - but only where the
                // runtime honors it at all.
                var sla = type.StructLayoutAttribute;
                int attrSize = sla?.Size ?? 0;
                if (attrSize > 0 && HonorsDeclaredSize(type, sla, packInfo.hasObjectField))
                {
                    size = Math.Max(size, attrSize);
                }
                lock (_Cache)
                {
                    _Cache[type.TypeHandle] = size;
                }
                return size;
            }
            public static int GetSize(Type type)
            {
                if (type.IsByRef || type.IsPointer)
                {
                    return IntPtr.Size;
                }
                lock (_Cache)
                {
                    if (_Cache.TryGetValue(type.TypeHandle, out var size))
                    {
                        return size;
                    }
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
                        case TypeCode.DateTime: size = 8; break;
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
                }
                //else
                {
                    return GetSizeByFields(type);
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

            private static Func<int> _RawSizeStub = RawSize<int>;
            public static int RawSizePlatDep(Type type)
            {
                if (type.IsByRef || type.IsPointer || !type.IsValueType)
                {
                    return IntPtr.Size;
                }
                int size = -1;
                lock (_Cache)
                {
                    if (_Cache.TryGetValue(type.TypeHandle, out size))
                    {
                        return size;
                    }
                }
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
                    case TypeCode.DateTime: size = 8; break;
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

                return GetSizePlatDepByGenericOrFallback(type);
            }

            public static int RawSizeByGeneric(Type type)
            {
                try
                {
                    var gmethod = _RawSizeStub.Method.GetGenericMethodDefinition();
                    var imethod = gmethod.MakeGenericMethod(type);
                    Func<int> ifunc = (Func<int>)Delegate.CreateDelegate(typeof(Func<int>), imethod);
                    int size = ifunc();
                    lock (_Cache)
                    {
                        _Cache[type.TypeHandle] = size;
                    }
                    return size;
                }
                catch
                {
                    return -1;
                }
            }

            public static int GetSizePlatDep(Type type)
            {
                if (type.IsByRef || type.IsPointer)
                {
                    return IntPtr.Size;
                }
                lock (_Cache)
                {
                    if (_Cache.TryGetValue(type.TypeHandle, out var size))
                    {
                        return size;
                    }
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
                        case TypeCode.DateTime: size = 8; break;
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
                }
                //else
                {
                    return GetSizePlatDepByGenericOrFallback(type);
                }
            }

            public static int GetSizePlatDepByGenericOrFallback(Type type)
            {
                int size = -1;
                if (type.IsValueType)
                {
                    if ((size = RawSizeByGeneric(type)) >= 0)
                    {
                        return size;
                    }
                }
                if (RuntimeInfo.IsIl2Cpp)
                {
                    // import func from il2cpp c library
                }
                else if (RuntimeInfo.IsNativeAOT)
                {
                    // alloc on gc heap, and calculate allocated delta.
                }
                return GetSizeByFields(type);
            }
        }

        public static int Size(this Type t)
        {
            return SizeMeasurer.GetSizePlatDep(t);
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
            else if (obj is string str)
            {
                // Variable length: the chars (including the terminator) live
                // in the instance, the type walk cannot see them. The
                // calibrated first-char offset is header-adjusted (same frame
                // as the array route below).
                return GetStringFirstCharOffset() + (str.Length + 1) * 2;
            }
            else if (obj is Array arr)
            {
                // Variable length: elements behind the calibrated
                // first-element offset. The stride is the element size
                // rounded up to the element's own alignment (a Pack=1 struct
                // array strides by its raw size); reference elements are one
                // pointer each. NOTE: rank > 1 arrays store their dimension
                // bounds between the length and the data - the SZ-calibrated
                // offset misses that, multi-dim results are a lower bound.
                var elemType = arr.GetType().GetElementType();
                int elemSize = elemType.IsValueType ? Size(elemType) : IntPtr.Size;
                int align = SizeMeasurer.GetPack(elemType);
                if (align > 1)
                {
                    elemSize = (elemSize + align - 1) / align * align;
                }
                return GetArrayFirstElementOffset() + arr.Length * elemSize;
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
                // Cast to object explicitly: a bare Size(obj) call would re-bind
                // to this same generic overload and recurse forever.
                return Size((object)obj);
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
                // Same as above: without the cast to object the call re-binds
                // to this overload itself and blows the stack.
                return Size((object)obj);
            }
        }

        public static int RawSize(this Type t)
        {
            return SizeMeasurer.RawSizePlatDep(t);
        }
        public static int RawSize<T>()
        {
            unsafe
            {
#pragma warning disable CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
                return sizeof(T);
#pragma warning restore CS8500 // 这会获取托管类型的地址、获取其大小或声明指向它的指针
            }
        }

        public static bool IsBlittable(this Type t)
        {
            return SizeMeasurer.GetBlittablePack(t).blittable;
        }
        public static bool IsBlittable<T>()
        {
            return IsBlittable(typeof(T));
        }
        public static bool HasObjectField(this Type t)
        {
            return SizeMeasurer.GetBlittablePack(t).hasObjectField;
        }
        public static bool HasObjectField<T>()
        {
            return HasObjectField(typeof(T));
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
                return offset = FieldPlatDep.GetFieldOffset(fi);
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

    public static class TypeIndicator
    {
        public static TypeIndicator<T> Of<T>()
        {
            return new TypeIndicator<T>();
        }
    }
    public struct TypeIndicator<T> { }

    public static partial class UnsafeEx
    {
        private static partial class FieldPlatDep
        {
            private interface IPlatDep
            {
                int GetFieldOffset(FieldInfo field);
                //FieldInfo[] GetAllFields(Type type);
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

                //public FieldInfo[] GetAllFields(Type type)
                //{
                //    if (typeof(Delegate).IsAssignableFrom(type))
                //    {
                //        HashSet<string> fieldNames = new HashSet<string>();
                //        List<FieldInfo> fields = new List<FieldInfo>();
                //        var curType = type;
                //        while (curType != null)
                //        {
                //            var curFields = curType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //            foreach (var field in curFields)
                //            {
                //                if (fieldNames.Add(field.Name))
                //                {
                //                    fields.Add(field);
                //                }
                //            }
                //            curType = curType.BaseType;
                //        }
                //        return fields.ToArray();
                //    }

                //    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //}
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

                //public FieldInfo[] GetAllFields(Type type)
                //{
                //    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //}
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
                //public FieldInfo[] GetAllFields(Type type)
                //{
                //    return type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                //}
            }

            static FieldPlatDep()
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

            //public static FieldInfo[] GetAllFields(Type type)
            //{
            //    return _PlatDep.GetAllFields(type);
            //}
        }
    }
}
