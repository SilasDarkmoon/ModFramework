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
        public static T ConvertObjectRef<T>(object source) where T : class
        {
            RawRef r = new RawRef();
            r.SetRef(ref source);
            ref T rrv = ref r.GetRef<T>();
            return rrv;
        }
        public static ref T ConvertStructRef<F, T>(this ref F source) where F : struct where T : struct
        {
            return ref ConvertRefCommon<F, T>(ref source);
        }
        /// <summary>
        /// Pretend that the ref of a struct is an object
        /// </summary>
        public static T ConvertPretend<F, T>(ref F from) where F : struct where T : class
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRef(ref from);
                var preaddr = r.Address;
                var temprv = r.GetRefObj();
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
        public static ref T ConvertPretend<T>(object from) where T : struct
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
        public static T ConvertToOuterObject<F, T>(ref F from) where T : class
        {
            RawRef r = new RawRef();
            for (; ; )
            {
                r.SetRef(ref from);
                var preaddr = r.Address;
                r.Address -= GetObjectHeaderSize();
                var temprv = r.GetRefObj();
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
        public static ref T ConvertToInnerRef<T>(this object from) where T : struct
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

        //public static D Overwrite<D, S>(ref D dst, S src)
        //{
        //    //ref byte dstRef, srcRef;

        //    return dst;
        //}
    }
}
