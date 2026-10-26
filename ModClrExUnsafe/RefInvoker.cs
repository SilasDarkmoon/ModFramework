using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static void RefInvoke<P1>(in P1 p1, Action<LocalRef<P1>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1);
        }
        public static void RefInvoke<P1, P2>(in P1 p1, in P2 p2, Action<LocalRef<P1>, LocalRef<P2>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2);
        }
        public static void RefInvoke<P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3);
        }
        public static void RefInvoke<P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
        public static void RefInvoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16, Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>> act)
        {
            var invoker = new FreeFunc<VoidReturn, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(act.ConvertedClone<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
            invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
        public static R RefInvoke<R, P1>(in P1 p1, Func<LocalRef<P1>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1);
        }
        public static R RefInvoke<R, P1, P2>(in P1 p1, in P2 p2, Func<LocalRef<P1>, LocalRef<P2>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2);
        }
        public static R RefInvoke<R, P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3);
        }
        public static R RefInvoke<R, P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
        public static R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16, Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>, R> func)
        {
            var invoker = new FreeFunc<R, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
            return invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
        public static ref R RefInvoke<R, P1>(in P1 p1, RefFunc<LocalRef<P1>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2>(in P1 p1, in P2 p2, RefFunc<LocalRef<P1>, LocalRef<P2>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15).ToRef<R>();
        }
        public static ref R RefInvoke<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16, RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>, R> func)
        {
            var invoker = new FreeFunc<ByRefParam, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>(func.ConvertedClone<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefParam>>());
            return ref invoker.Invoke(out _, in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16).ToRef<R>();
        }
    }
}
