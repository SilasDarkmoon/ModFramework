using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static ref T ConvertRefCommon<F, T>(ref F from)
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRef(ref from);
                var preaddr = r.Address;
                ref T rv = ref r.GetRef<T>();
                r.SetRef(ref from);
                var postaddr = r.Address;
                if (postaddr == preaddr)
                {
                    return ref rv;
                }
            }
        }
        public static T ConvertObjectRef<T>(this object source)
        {
            RawRef r = new RawRef();
            r.SetRef(ref source);
            ref T rrv = ref r.GetRef<T>();
            return rrv;
        }
        public static ref T ConvertStructRef<F, T>(this ref F source) where F : struct
        {
            return ref ConvertRefCommon<F, T>(ref source);
        }
        /// <summary>
        /// Pretend that the ref of a struct is an object
        /// </summary>
        public static T ConvertPretend<F, T>(ref F from)
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRef(ref from);
                var preaddr = r.Address;
                object temprv = r.GetRefObj();
                r.SetRef(ref from);
                var postaddr = r.Address;
                if (postaddr == preaddr)
                {
                    return ConvertObjectRef<T>(temprv);
                }
            }
        }
        /// <summary>
        /// Pretend that the object is a ref to a struct.
        /// </summary>
        public static ref T ConvertPretend<T>(object from)
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRefObj(from);
                var preaddr = r.Address;
                ref T rv = ref r.GetRef<T>();
                r.SetRefObj(from);
                var postaddr = r.Address;
                if (postaddr == preaddr)
                {
                    return ref rv;
                }
            }
        }
        /// <summary>
        /// Suppose ref F from is pointing to the first field of an object, return its container object. This func will move the returned pointer -ObjectHeaderSize.
        /// </summary>
        /// <remarks>
        /// extra danger: if Address-ObjectHeaderSize is outside the containing object of "from" on heap, returned value will be wild pointer after GC moved the container of "from"
        /// </remarks>
        public static T ConvertToOuterObject<F, T>(ref F from)
        {
            RawRef r = new RawRef();
            var headerSize = GetObjectHeaderSize();
            for (; ; )
            {
                r.SetRef(ref from);
                var preaddr = r.Address;
                r.Address -= headerSize;
                object temprv = r.GetRefObj();
                r.SetRef(ref from);
                var postaddr = r.Address;
                if (postaddr == preaddr)
                {
                    return ConvertObjectRef<T>(temprv);
                }
            }
        }
        /// <summary>
        /// Pretend that the object is a ref to a struct, will move the returned pointer +ObjectHeaderSize to point to the first field in the object.
        /// </summary>
        public static ref T ConvertToInnerRef<T>(this object from)
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRefObj(from);
                var preaddr = r.Address;
                r.Address += GetObjectHeaderSize();
                ref T rv = ref r.GetRef<T>();
                r.SetRefObj(from);
                var postaddr = r.Address;
                if (postaddr == preaddr)
                {
                    return ref rv;
                }
            }
        }

        public static object ConvertedClone(this object src, Type totype)
        {
            if (src == null) return null;
            object rv = GetUninitializedObject(totype);
            MemCopy(rv, src);
            return rv;
        }
        public static T ConvertedClone<T>(this object src)
        {
            if (src == null) return default(T);
            T rv = GetUninitialized<T>();
            MemCopy(ref GetHeadRefCommon(ref rv), 0, ref src.GetHeadRef(), 0, Math.Min(Size<T>(), Size(src)));
            return rv;
        }
        public static T ConvertedClone<S, T>(this S src)
        {
            if (!typeof(S).IsValueType)
            {
                if (src == null) return default(T);
            }
            T rv = GetUninitialized<T>();
            MemCopy(ref GetHeadRefCommon(ref rv), 0, ref GetHeadRefCommon(ref src), 0, Math.Min(Size<T>(), Size<S>()));
            return rv;
        }
        public static T ConvertedClone<S, T>(this S src, TypeIndicator<T> indicator)
        {
            return ConvertedClone<S, T>(src);
        }
        public static T ConvertedClone<S, T>(this ref S src) where S : struct
        {
            T rv = GetUninitialized<T>();
            MemCopy(ref GetHeadRefCommon(ref rv), 0, ref src.GetHeadRef(), 0, Math.Min(Size<T>(), Size<S>()));
            return rv;
        }
        public static T ConvertedClone<S, T>(this ref S src, TypeIndicator<T> indicator) where S : struct
        {
            return ConvertedClone<S, T>(ref src);
        }
    }
}
