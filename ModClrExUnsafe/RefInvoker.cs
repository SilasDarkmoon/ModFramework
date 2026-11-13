using System;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static ActionRefInvoker1 RefInvoker<P1>(this Action<LocalRef<P1>> act)
        {
            return new ActionRefInvoker1(act.ChangeDelegateType<Action<ByRefLocalSlot>>());
        }
        public static ActionRefInvoker2 RefInvoker<P1, P2>(this Action<LocalRef<P1>, LocalRef<P2>> act)
        {
            return new ActionRefInvoker2(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker3 RefInvoker<P1, P2, P3>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>> act)
        {
            return new ActionRefInvoker3(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker4 RefInvoker<P1, P2, P3, P4>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>> act)
        {
            return new ActionRefInvoker4(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker5 RefInvoker<P1, P2, P3, P4, P5>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>> act)
        {
            return new ActionRefInvoker5(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker6 RefInvoker<P1, P2, P3, P4, P5, P6>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>> act)
        {
            return new ActionRefInvoker6(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker7 RefInvoker<P1, P2, P3, P4, P5, P6, P7>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>> act)
        {
            return new ActionRefInvoker7(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker8 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>> act)
        {
            return new ActionRefInvoker8(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker9 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>> act)
        {
            return new ActionRefInvoker9(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker10 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>> act)
        {
            return new ActionRefInvoker10(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker11 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>> act)
        {
            return new ActionRefInvoker11(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker12 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>> act)
        {
            return new ActionRefInvoker12(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker13 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>> act)
        {
            return new ActionRefInvoker13(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker14 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>> act)
        {
            return new ActionRefInvoker14(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker15 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>> act)
        {
            return new ActionRefInvoker15(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker16 RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>> act)
        {
            return new ActionRefInvoker16(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot>>());
        }
        public static ActionRefInvoker1<EX> RefInvoker<P1, EX>(this Action<LocalRef<P1>, EX> act)
        {
            return new ActionRefInvoker1<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker2<EX> RefInvoker<P1, P2, EX>(this Action<LocalRef<P1>, LocalRef<P2>, EX> act)
        {
            return new ActionRefInvoker2<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker3<EX> RefInvoker<P1, P2, P3, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, EX> act)
        {
            return new ActionRefInvoker3<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker4<EX> RefInvoker<P1, P2, P3, P4, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, EX> act)
        {
            return new ActionRefInvoker4<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker5<EX> RefInvoker<P1, P2, P3, P4, P5, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, EX> act)
        {
            return new ActionRefInvoker5<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker6<EX> RefInvoker<P1, P2, P3, P4, P5, P6, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, EX> act)
        {
            return new ActionRefInvoker6<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker7<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, EX> act)
        {
            return new ActionRefInvoker7<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker8<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, EX> act)
        {
            return new ActionRefInvoker8<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker9<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, EX> act)
        {
            return new ActionRefInvoker9<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker10<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, EX> act)
        {
            return new ActionRefInvoker10<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker11<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, EX> act)
        {
            return new ActionRefInvoker11<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker12<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, EX> act)
        {
            return new ActionRefInvoker12<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker13<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, EX> act)
        {
            return new ActionRefInvoker13<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker14<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, EX> act)
        {
            return new ActionRefInvoker14<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static ActionRefInvoker15<EX> RefInvoker<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, EX>(this Action<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, EX> act)
        {
            return new ActionRefInvoker15<EX>(act.ChangeDelegateType<Action<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX>>());
        }
        public static FuncRefInvoker1<R> RefInvoker<R, P1>(this Func<LocalRef<P1>, R> func)
        {
            return new FuncRefInvoker1<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker2<R> RefInvoker<R, P1, P2>(this Func<LocalRef<P1>, LocalRef<P2>, R> func)
        {
            return new FuncRefInvoker2<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker3<R> RefInvoker<R, P1, P2, P3>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, R> func)
        {
            return new FuncRefInvoker3<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker4<R> RefInvoker<R, P1, P2, P3, P4>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, R> func)
        {
            return new FuncRefInvoker4<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker5<R> RefInvoker<R, P1, P2, P3, P4, P5>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, R> func)
        {
            return new FuncRefInvoker5<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker6<R> RefInvoker<R, P1, P2, P3, P4, P5, P6>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, R> func)
        {
            return new FuncRefInvoker6<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker7<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, R> func)
        {
            return new FuncRefInvoker7<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker8<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, R> func)
        {
            return new FuncRefInvoker8<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker9<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, R> func)
        {
            return new FuncRefInvoker9<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker10<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, R> func)
        {
            return new FuncRefInvoker10<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker11<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, R> func)
        {
            return new FuncRefInvoker11<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker12<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, R> func)
        {
            return new FuncRefInvoker12<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker13<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, R> func)
        {
            return new FuncRefInvoker13<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker14<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, R> func)
        {
            return new FuncRefInvoker14<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker15<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, R> func)
        {
            return new FuncRefInvoker15<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker16<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>, R> func)
        {
            return new FuncRefInvoker16<R>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static FuncRefInvoker1<R, EX> RefInvoker<R, P1, EX>(this Func<LocalRef<P1>, EX, R> func)
        {
            return new FuncRefInvoker1<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker2<R, EX> RefInvoker<R, P1, P2, EX>(this Func<LocalRef<P1>, LocalRef<P2>, EX, R> func)
        {
            return new FuncRefInvoker2<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker3<R, EX> RefInvoker<R, P1, P2, P3, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, EX, R> func)
        {
            return new FuncRefInvoker3<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker4<R, EX> RefInvoker<R, P1, P2, P3, P4, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, EX, R> func)
        {
            return new FuncRefInvoker4<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker5<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, EX, R> func)
        {
            return new FuncRefInvoker5<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker6<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, EX, R> func)
        {
            return new FuncRefInvoker6<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker7<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, EX, R> func)
        {
            return new FuncRefInvoker7<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker8<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, EX, R> func)
        {
            return new FuncRefInvoker8<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker9<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, EX, R> func)
        {
            return new FuncRefInvoker9<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker10<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, EX, R> func)
        {
            return new FuncRefInvoker10<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker11<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, EX, R> func)
        {
            return new FuncRefInvoker11<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker12<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, EX, R> func)
        {
            return new FuncRefInvoker12<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker13<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, EX, R> func)
        {
            return new FuncRefInvoker13<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker14<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, EX, R> func)
        {
            return new FuncRefInvoker14<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static FuncRefInvoker15<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, EX>(this Func<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, EX, R> func)
        {
            return new FuncRefInvoker15<R, EX>(func.ChangeDelegateType<Func<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker1<R> RefInvoker<R, P1>(this RefFunc<LocalRef<P1>, R> func)
        {
            return new RefFuncRefInvoker1<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker2<R> RefInvoker<R, P1, P2>(this RefFunc<LocalRef<P1>, LocalRef<P2>, R> func)
        {
            return new RefFuncRefInvoker2<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker3<R> RefInvoker<R, P1, P2, P3>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, R> func)
        {
            return new RefFuncRefInvoker3<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker4<R> RefInvoker<R, P1, P2, P3, P4>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, R> func)
        {
            return new RefFuncRefInvoker4<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker5<R> RefInvoker<R, P1, P2, P3, P4, P5>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, R> func)
        {
            return new RefFuncRefInvoker5<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker6<R> RefInvoker<R, P1, P2, P3, P4, P5, P6>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, R> func)
        {
            return new RefFuncRefInvoker6<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker7<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, R> func)
        {
            return new RefFuncRefInvoker7<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker8<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, R> func)
        {
            return new RefFuncRefInvoker8<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker9<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, R> func)
        {
            return new RefFuncRefInvoker9<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker10<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, R> func)
        {
            return new RefFuncRefInvoker10<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker11<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, R> func)
        {
            return new RefFuncRefInvoker11<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker12<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, R> func)
        {
            return new RefFuncRefInvoker12<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker13<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, R> func)
        {
            return new RefFuncRefInvoker13<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker14<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, R> func)
        {
            return new RefFuncRefInvoker14<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker15<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, R> func)
        {
            return new RefFuncRefInvoker15<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker16<R> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, LocalRef<P16>, R> func)
        {
            return new RefFuncRefInvoker16<R>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, R>>());
        }
        public static RefFuncRefInvoker1<R, EX> RefInvoker<R, P1, EX>(this RefFunc<LocalRef<P1>, EX, R> func)
        {
            return new RefFuncRefInvoker1<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker2<R, EX> RefInvoker<R, P1, P2, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, EX, R> func)
        {
            return new RefFuncRefInvoker2<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker3<R, EX> RefInvoker<R, P1, P2, P3, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, EX, R> func)
        {
            return new RefFuncRefInvoker3<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker4<R, EX> RefInvoker<R, P1, P2, P3, P4, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, EX, R> func)
        {
            return new RefFuncRefInvoker4<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker5<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, EX, R> func)
        {
            return new RefFuncRefInvoker5<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker6<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, EX, R> func)
        {
            return new RefFuncRefInvoker6<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker7<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, EX, R> func)
        {
            return new RefFuncRefInvoker7<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker8<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, EX, R> func)
        {
            return new RefFuncRefInvoker8<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker9<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, EX, R> func)
        {
            return new RefFuncRefInvoker9<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker10<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, EX, R> func)
        {
            return new RefFuncRefInvoker10<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker11<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, EX, R> func)
        {
            return new RefFuncRefInvoker11<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker12<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, EX, R> func)
        {
            return new RefFuncRefInvoker12<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker13<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, EX, R> func)
        {
            return new RefFuncRefInvoker13<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker14<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, EX, R> func)
        {
            return new RefFuncRefInvoker14<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
        public static RefFuncRefInvoker15<R, EX> RefInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, EX>(this RefFunc<LocalRef<P1>, LocalRef<P2>, LocalRef<P3>, LocalRef<P4>, LocalRef<P5>, LocalRef<P6>, LocalRef<P7>, LocalRef<P8>, LocalRef<P9>, LocalRef<P10>, LocalRef<P11>, LocalRef<P12>, LocalRef<P13>, LocalRef<P14>, LocalRef<P15>, EX, R> func)
        {
            return new RefFuncRefInvoker15<R, EX>(func.ChangeDelegateType<RefFunc<ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, ByRefLocalSlot, EX, R>>());
        }
    }
}
