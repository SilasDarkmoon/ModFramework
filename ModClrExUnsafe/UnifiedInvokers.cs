using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mod.LowLevel
{
    #region UnifiedDelegates
    public delegate ref R UnifiedRefFunc<R>(out R r);
    public delegate ref R UnifiedRefFunc<R, P1>(out R r, in P1 p1);
    public delegate ref R UnifiedRefFunc<R, P1, P2>(out R r, in P1 p1, in P2 p2);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3>(out R r, in P1 p1, in P2 p2, in P3 p3);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15);
    public delegate ref R UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(out R r, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16);

    public delegate R UnifiedFunc<R>();
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
        public static ref T ToRef<T>(this ref ByRefParam r)
        {
            return ref ConvertRef<ByRefParam, T>(ref r);
        }

        public static R Invoke<R>(this UnifiedRefFunc<R> ufunc) { return ufunc(out _); }
        public static R Invoke<R, P1>(this UnifiedRefFunc<R, P1> ufunc, in P1 p1) { return ufunc(out _, in p1); }
        public static R Invoke<R, P1, P2>(this UnifiedRefFunc<R, P1, P2> ufunc, in P1 p1, in P2 p2) { return ufunc(out _, in p1, in p2); }
        public static R Invoke<R, P1, P2, P3>(this UnifiedRefFunc<R, P1, P2, P3> ufunc, in P1 p1, in P2 p2, in P3 p3) { return ufunc(out _, in p1, in p2, in p3); }
        public static R Invoke<R, P1, P2, P3, P4>(this UnifiedRefFunc<R, P1, P2, P3, P4> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4) { return ufunc(out _, in p1, in p2, in p3, in p4); }
        public static R Invoke<R, P1, P2, P3, P4, P5>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15); }
        public static R Invoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> ufunc, in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) { return ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16); }
        public static UnifiedFunc<R> AsUnifiedFunc<R>(this UnifiedRefFunc<R> ufunc) { return () => ufunc(out _); }
        public static UnifiedFunc<R, P1> AsUnifiedFunc<R, P1>(this UnifiedRefFunc<R, P1> ufunc) { return (in P1 p1) => ufunc(out _, in p1); }
        public static UnifiedFunc<R, P1, P2> AsUnifiedFunc<R, P1, P2>(this UnifiedRefFunc<R, P1, P2> ufunc) { return (in P1 p1, in P2 p2) => ufunc(out _, in p1, in p2); }
        public static UnifiedFunc<R, P1, P2, P3> AsUnifiedFunc<R, P1, P2, P3>(this UnifiedRefFunc<R, P1, P2, P3> ufunc) { return (in P1 p1, in P2 p2, in P3 p3) => ufunc(out _, in p1, in p2, in p3); }
        public static UnifiedFunc<R, P1, P2, P3, P4> AsUnifiedFunc<R, P1, P2, P3, P4>(this UnifiedRefFunc<R, P1, P2, P3, P4> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4) => ufunc(out _, in p1, in p2, in p3, in p4); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5> AsUnifiedFunc<R, P1, P2, P3, P4, P5>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5) => ufunc(out _, in p1, in p2, in p3, in p4, in p5); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15); }
        public static UnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> AsUnifiedFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> ufunc) { return (in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) => ufunc(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16); }

        private static IntPtr GetCalliHandle(this MethodBase method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                return method.MethodHandle.Value;
            }
            else
            {
                return method.MethodHandle.GetFunctionPointer();
            }
        }
        public static PointerFunc<R> CreateFreeInvokable<R>(this MethodBase method) { return new PointerFunc<R>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1> CreateFreeInvokable<R, U1>(this MethodBase method) { return new PointerFunc<R, U1>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2> CreateFreeInvokable<R, U1, U2>(this MethodBase method) { return new PointerFunc<R, U1, U2>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3> CreateFreeInvokable<R, U1, U2, U3>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4> CreateFreeInvokable<R, U1, U2, U3, U4>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5> CreateFreeInvokable<R, U1, U2, U3, U4, U5>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(method.GetCalliHandle()); }
        public static PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this MethodBase method) { return new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(method.GetCalliHandle()); }

#pragma warning disable CS9073
        public static UnifiedRefFunc<R> CreateDelegateUnified<R>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R>().Invoke;
        }
        public static UnifiedRefFunc<R, P1> CreateDelegateUnified<R, U1, P1>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2> CreateDelegateUnified<R, U1, U2, P1, P2>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3> CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4> CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5> CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>().Invoke;
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this MethodBase method)
        {
            return method.CreateFreeInvokable<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>().Invoke;
        }
#pragma warning restore CS9073
    }
    #endregion

    #region Invokers
    public static partial class UnsafeEx
    {
        public static UnifiedRefFunc<R> CreateInvoker<R>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(Array.Empty<Type>(), withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R>();
        }
        public static UnifiedRefFunc<R, P1> CreateInvoker<R, U1, P1>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1) } , withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, P1>();
        }
        public static UnifiedRefFunc<R, P1, P2> CreateInvoker<R, U1, U2, P1, P2>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2) } , withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, P1, P2>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3> CreateInvoker<R, U1, U2, U3, P1, P2, P3>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4> CreateInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5> CreateInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> CreateInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Type type, string methodName)
        {
            var method = type.GetMethods(methodName).FindOverload(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, withThis: true);
            if (method == null) return null;
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
        }

        public static UnifiedRefFunc<R> CreateInstanceInvoker<R, P1>(this P1 thiz, UnifiedRefFunc<R, P1> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2> CreateInstanceInvoker<R, P1, P2>(this P1 thiz, UnifiedRefFunc<R, P1, P2> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3> CreateInstanceInvoker<R, P1, P2, P3>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4> CreateInstanceInvoker<R, P1, P2, P3, P4>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5> CreateInstanceInvoker<R, P1, P2, P3, P4, P5>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInstanceInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this P1 thiz, UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> func)
        {
            return DelegateCurry.Curry(func, thiz);
        }

        public static UnifiedRefFunc<R> CreateInstanceInvoker<R, U1, P1>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, P1>(methodName));
        }
        public static UnifiedRefFunc<R, P2> CreateInstanceInvoker<R, U1, U2, P1, P2>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, P1, P2>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3> CreateInstanceInvoker<R, U1, U2, U3, P1, P2, P3>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, P1, P2, P3>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4> CreateInstanceInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(methodName));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this P1 target, string methodName)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(methodName));
        }

        public static UnifiedRefFunc<R> CreateCustructorInvoker<R>(this Type type)
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
                        return (out R result) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                        return (out R result) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, R>();
                        return (out R result) =>
                        {
                            result = default(R);
                            rawdel(out _, in result);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                        return (out R result) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0));
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, R>();
                return (out R result) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2> CreateCustructorInvoker<R, U2, P2>(this Type type)
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
                        return (out R result, in P2 p2) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                        return (out R result, in P2 p2) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, R, P2>();
                        return (out R result, in P2 p2) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                        return (out R result, in P2 p2) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, R, P2>();
                return (out R result, in P2 p2) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3> CreateCustructorInvoker<R, U2, U3, P2, P3>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                        return (out R result, in P2 p2, in P3 p3) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, R, P2, P3>();
                        return (out R result, in P2 p2, in P3 p3) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                        return (out R result, in P2 p2, in P3 p3) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, R, P2, P3>();
                return (out R result, in P2 p2, in P3 p3) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4> CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, R, P2, P3, P4>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, R, P2, P3, P4>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5> CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, R, P2, P3, P4, P5>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, R, P2, P3, P4, P5, P6>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, R, P2, P3, P4, P5, P6, P7>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, R, P2, P3, P4, P5, P6, P7, P8>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, R, P2, P3, P4, P5, P6, P7, P8, P9>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, R, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
                    return ref result;
                };
            }
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Type type)
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
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            result = default(R);
                            var r = RawRef.Of(ref result);
                            r.Address -= GetObjectHeaderSize();
                            rawdel(out _, r.Address, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return ref result;
                        };
                    }
                }
                else
                {
                    if (typeof(R).IsValueType)
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, ByRefParam, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            result = default(R);
                            rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return ref result;
                        };
                    }
                    else
                    {
                        var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                        return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                        {
                            result = (R)GetUninitializedObject(type);
                            rawdel(out _, in result.GetField<R>(0), in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                            return ref result;
                        };
                    }
                }
            }
            else
            {
                var rawdel = ctor.CreateDelegateUnified<VoidReturn, R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
                return (out R result, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16) =>
                {
                    result = (R)GetUninitializedObject(type);
                    rawdel(out _, in result, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
                    return ref result;
                };
            }
        }

        public static UnifiedRefFunc<R> CreateCustructorInvoker<R>()
        {
            return CreateCustructorInvoker<R>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2> CreateCustructorInvoker<R, U2, P2>()
        {
            return CreateCustructorInvoker<R, U2, P2>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3> CreateCustructorInvoker<R, U2, U3, P2, P3>()
        {
            return CreateCustructorInvoker<R, U2, U3, P2, P3>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4> CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, P2, P3, P4>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5> CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, P2, P3, P4, P5>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, P2, P3, P4, P5, P6>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, P2, P3, P4, P5, P6, P7>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, P2, P3, P4, P5, P6, P7, P8>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, P2, P3, P4, P5, P6, P7, P8, P9>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, P2, P3, P4, P5, P6, P7, P8, P9, P10>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(typeof(R));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>()
        {
            return CreateCustructorInvoker<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(typeof(R));
        }
    }
    #endregion
}
