using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Mod.LowLevel
{
    #region UnifiedDelegates
    public delegate R UnifiedFunc<R>(); // same to the Func<R>
    public delegate R UnifiedFunc<R, P1>(in P1 p1);
    public delegate R UnifiedFunc<R, P1, P2>(in P1 p1, in P2 p2);
    public delegate R UnifiedFunc<R, P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15);
    public delegate R UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16);

    public static partial class UnsafeEx
    {
        private static void ConvertParam<U, P>(out U u, in P p)
        {
            if (typeof(U) == typeof(ByRefParam))
            {
                RawRef ru = RawRef.Of(ref Ref.IgnoreOut(out u));
                ru.GetRef<RawRef>().SetRef(ref Ref.Unprotect(in p));
            }
            else
            {
                RawRef ru = RawRef.Of(ref Ref.IgnoreOut(out u));
                ru.GetRef<P>() = p;
            }
        }
        public static UnifiedFunc<R> CreateDelegateUnified<R>(this MethodBase method)
        {
            return CreateDelegateUnsafe<UnifiedFunc<R>>(method);
        }
        public static UnifiedFunc<R, P1> CreateDelegateUnified<R, U1, P1>(this MethodBase method)
        {
            Func<U1, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1> adel = CreateDelegateUnsafe<Action<U1>>(method);
                udel = (u1) => { adel(u1); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, R>>(method);
            }

            return (in P1 p1) =>
            {
                ConvertParam(out U1 u1, in p1);
                return udel(u1);
            };
        }
        public static UnifiedFunc<R, P1, P2> CreateDelegateUnified<R, U1, U2, P1, P2>(this MethodBase method)
        {
            Func<U1, U2, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2> adel = CreateDelegateUnsafe<Action<U1, U2>>(method);
                udel = (u1, u2) => { adel(u1, u2); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, R>>(method);
            }

            return (in P1 p1, in P2 p2) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                return udel(u1, u2);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3> CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>(this MethodBase method)
        {
            Func<U1, U2, U3, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3> adel = CreateDelegateUnsafe<Action<U1, U2, U3>>(method);
                udel = (u1, u2, u3) => { adel(u1, u2, u3); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                return udel(u1, u2, u3);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4> CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4>>(method);
                udel = (u1, u2, u3, u4) => { adel(u1, u2, u3, u4); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                return udel(u1, u2, u3, u4);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5> CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5>>(method);
                udel = (u1, u2, u3, u4, u5) => { adel(u1, u2, u3, u4, u5); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                return udel(u1, u2, u3, u4, u5);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6>>(method);
                udel = (u1, u2, u3, u4, u5, u6) => { adel(u1, u2, u3, u4, u5, u6); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                return udel(u1, u2, u3, u4, u5, u6);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7) => { adel(u1, u2, u3, u4, u5, u6, u7); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                return udel(u1, u2, u3, u4, u5, u6, u7);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8) => { adel(u1, u2, u3, u4, u5, u6, u7, u8); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                ConvertParam(out U12 u12, in p12);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                ConvertParam(out U12 u12, in p12);
                ConvertParam(out U13 u13, in p13);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                ConvertParam(out U12 u12, in p12);
                ConvertParam(out U13 u13, in p13);
                ConvertParam(out U14 u14, in p14);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                ConvertParam(out U12 u12, in p12);
                ConvertParam(out U13 u13, in p13);
                ConvertParam(out U14 u14, in p14);
                ConvertParam(out U15 u15, in p15);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
            };
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this MethodBase method)
        {
            Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> udel;
            if (method.IsConstructor || (method is MethodInfo mi && mi.ReturnType == typeof(void)))
            { // no return.
                Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> adel = CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>(method);
                udel = (u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16) => { adel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16); return default(R); };
            }
            else
            {
                udel = CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>(method);
            }

            return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
            {
                ConvertParam(out U1 u1, in p1);
                ConvertParam(out U2 u2, in p2);
                ConvertParam(out U3 u3, in p3);
                ConvertParam(out U4 u4, in p4);
                ConvertParam(out U5 u5, in p5);
                ConvertParam(out U6 u6, in p6);
                ConvertParam(out U7 u7, in p7);
                ConvertParam(out U8 u8, in p8);
                ConvertParam(out U9 u9, in p9);
                ConvertParam(out U10 u10, in p10);
                ConvertParam(out U11 u11, in p11);
                ConvertParam(out U12 u12, in p12);
                ConvertParam(out U13 u13, in p13);
                ConvertParam(out U14 u14, in p14);
                ConvertParam(out U15 u15, in p15);
                ConvertParam(out U16 u16, in p16);
                return udel(u1, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
            };
        }

        //public static Delegate CreateDelegateUnified(this MethodBase method)
        //{
        //    // maybe not neccessary.
        //}
    }
    #endregion

    #region Invokers
    public static partial class UnsafeEx
    {
        public static UnifiedFunc<R> CreateInvoker<R>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(Array.Empty<Type>(), withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R>();
        }
        public static UnifiedFunc<R, P1> CreateInvoker<R, U1, P1>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1) } , withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, P1>();
        }
        public static UnifiedFunc<R, P1, P2> CreateInvoker<R, U1, U2, P1, P2>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2) } , withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, P1, P2>();
        }
        public static UnifiedFunc<R, P1, P2, P3> CreateInvoker<R, U1, U2, U3, P1, P2, P3>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4> CreateInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5> CreateInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6> CreateInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
        }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
        }

        public static UnifiedFunc<R> CreateInstanceInvoker<R, P1>(this P1 thiz, UnifiedFunc<R, P1> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return () => func(in target.GetField<P1>(0));
            }
            else
            {
                P1 othiz = thiz;
                return () => func(in othiz);
            }
        }
        public static UnifiedFunc<R, P2> CreateInstanceInvoker<R, P1, P2>(this P1 thiz, UnifiedFunc<R, P1, P2> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2) => func(in target.GetField<P1>(0), in p2);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2) => func(in othiz, in p2);
            }
        }
        public static UnifiedFunc<R, P2, P3> CreateInstanceInvoker<R, P1, P2, P3>(this P1 thiz, UnifiedFunc<R, P1, P2, P3> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3) => func(in target.GetField<P1>(0), in p2, in p3);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3) => func(in othiz, in p2, in p3);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4> CreateInstanceInvoker<R, P1, P2, P3, P4>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4) => func(in target.GetField<P1>(0), in p2, in p3, in p4);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4) => func(in othiz, in p2, in p3, in p4);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5> CreateInstanceInvoker<R, P1, P2, P3, P4, P5>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) => func(in othiz, in p2, in p3, in p4, in p5);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) => func(in othiz, in p2, in p3, in p4, in p5, in p6);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this P1 thiz, UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> func)
        {
            if (typeof(P1).IsValueType)
            {
                object target = thiz; // box it
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) => func(in target.GetField<P1>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
            }
            else
            {
                P1 othiz = thiz;
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) => func(in othiz, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
            }
        }

        public static UnifiedFunc<R> CreateInstanceInvoker<R, U1, P1>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, P1>(methodName));
        }
        public static UnifiedFunc<R, P2> CreateInstanceInvoker<R, U1, U2, P1, P2>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, P1, P2>(methodName));
        }
        public static UnifiedFunc<R, P2, P3> CreateInstanceInvoker<R, U1, U2, U3, P1, P2, P3>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, P1, P2, P3>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4> CreateInstanceInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(methodName));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(methodName));
        }

        public static UnifiedFunc<R> CreateCustructorInvoker<R>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(Array.Empty<Type>());
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, IntPtr>();
                        return () =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                        return () =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, R>();
                        return () =>
                        {
                            R result = default(R);
                            rawdel(in result);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                        return () =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0));
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                return () =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2> CreateCustructorInvoker<R, U2, P2>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, IntPtr, P2>();
                        return (in P2 p2) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                        return (in P2 p2) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, R, P2>();
                        return (in P2 p2) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                        return (in P2 p2) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                return (in P2 p2) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3> CreateCustructorInvoker<R, U2, U3, P2, P3>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, IntPtr, P2, P3>();
                        return (in P2 p2, in P3 p3) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                        return (in P2 p2, in P3 p3) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, R, P2, P3>();
                        return (in P2 p2, in P3 p3) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                        return (in P2 p2, in P3 p3) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                return (in P2 p2, in P3 p3) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4> CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, IntPtr, P2, P3, P4>();
                        return (in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                        return (in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, R, P2, P3, P4>();
                        return (in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                        return (in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                return (in P2 p2, in P3 p3, in P4 p4) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5> CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, IntPtr, P2, P3, P4, P5>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, IntPtr, P2, P3, P4, P5, P6>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, IntPtr, P2, P3, P4, P5, P6, P7>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, IntPtr, P2, P3, P4, P5, P6, P7, P8>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                    return result;
                };
            }
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Type type)
        {
            if (!typeof(R).IsAssignableFrom(type)) throw new ArgumentException($"{typeof(R).Name} is not parent of {type.Name}.", nameof(R));
            var ctor = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).FindOverload(new[] { typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) });
            if (ctor == null) return null;
            if (type.IsValueType)
            {
                if (RuntimeInfo.IsNativeAOT)
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, IntPtr, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, IntPtr, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            R result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            R result = default(R);
                            rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            var result = (R)GetUninitializedObject(type);
                            rawdel(in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                return (in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                {
                    var result = (R)GetUninitializedObject(type);
                    rawdel(in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                    return result;
                };
            }
        }

        public static UnifiedFunc<R> CreateCustructorInvoker<R>()
        {
            return CreateCustructorInvoker<R>(typeof(R));
        }
        public static UnifiedFunc<R, P2> CreateCustructorInvoker<R, U2, P2>()
        {
            return CreateCustructorInvoker<R, U2, P2>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3> CreateCustructorInvoker<R, U2, U3, P2, P3>()
        {
            return CreateCustructorInvoker<R, U2, U3, P2, P3>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4> CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5> CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(typeof(R));
        }
        public static UnifiedFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(typeof(R));
        }
    }
    #endregion
}
