using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public static class DelegateCurry
    {
        public static Func<R> Curry<R, U1>(this Func<U1, R> func, U1 target)
        {
            return new CurryWrapper<R, U1>(func) { Target = target }.Invoke;
        }
        public static Func<U2, R> Curry<R, U1, U2>(this Func<U1, U2, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, R> Curry<R, U1, U2, U3>(this Func<U1, U2, U3, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, R> Curry<R, U1, U2, U3, U4>(this Func<U1, U2, U3, U4, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, R> Curry<R, U1, U2, U3, U4, U5>(this Func<U1, U2, U3, U4, U5, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, R> Curry<R, U1, U2, U3, U4, U5, U6>(this Func<U1, U2, U3, U4, U5, U6, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, R> Curry<R, U1, U2, U3, U4, U5, U6, U7>(this Func<U1, U2, U3, U4, U5, U6, U7, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(func) { Target = target }.Invoke;
        }
        public static Func<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(func) { Target = target }.Invoke;
        }

        public static Action Curry<U1>(this Action<U1> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2> Curry<U1, U2>(this Action<U1, U2> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3> Curry<U1, U2, U3>(this Action<U1, U2, U3> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4> Curry<U1, U2, U3, U4>(this Action<U1, U2, U3, U4> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5> Curry<U1, U2, U3, U4, U5>(this Action<U1, U2, U3, U4, U5> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6> Curry<U1, U2, U3, U4, U5, U6>(this Action<U1, U2, U3, U4, U5, U6> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7> Curry<U1, U2, U3, U4, U5, U6, U7>(this Action<U1, U2, U3, U4, U5, U6, U7> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8> Curry<U1, U2, U3, U4, U5, U6, U7, U8>(this Action<U1, U2, U3, U4, U5, U6, U7, U8> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(act) { Target = target }.InvokeNoReturn;
        }
        public static Action<U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Curry<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act, U1 target)
        {
            return new CurryWrapper<VoidReturn, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(act) { Target = target }.InvokeNoReturn;
        }

        public static Type GetCompatible‌UnderlyingType(Type ptype)
        {
            if (ptype.IsByRef)
            {
                return typeof(ByRefParam);
            }
            else if (ptype == typeof(void))
            {
                return typeof(VoidReturn);
            }
            else
            {
                return ptype;
            }
        }

        public static Delegate Curry(this Delegate odel, object target, Type ddeltype)
        {
            var miodel = odel.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var opars = miodel.GetParameters();
            var oparcnt = opars.Length;
            var upars = new Type[oparcnt + 1];
            upars[0] = GetCompatible‌UnderlyingType(miodel.ReturnType);
            for (int i = 0; i < oparcnt; ++i)
            {
                upars[i + 1] = GetCompatible‌UnderlyingType(opars[i].ParameterType);
            }
            var gtype = _CurryWrappers[oparcnt];
            var wtype = gtype.MakeGenericType(upars);
            var wrapper = UnsafeEx.GetUninitializedObject(wtype) as CurryWrapper;
            wrapper.SetDelegate(odel);
            wrapper.SetTarget(target);
            string mname = miodel.ReturnType == typeof(void) ? "InvokeNoReturn" : "Invoke";
            var miwrapper = wtype.GetMethod(mname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return UnsafeEx.CreateDelegateUnsafe(ddeltype, wrapper, miwrapper);
        }
        public static Delegate Curry(this Delegate odel, object target)
        {
            var miodel = odel.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            bool noreturn = miodel.ReturnType == typeof(void);
            var opars = miodel.GetParameters();
            var oparcnt = opars.Length;
            var upars = new Type[oparcnt + 1];
            upars[0] = GetCompatible‌UnderlyingType(miodel.ReturnType);
            for (int i = 0; i < oparcnt; ++i)
            {
                upars[i + 1] = GetCompatible‌UnderlyingType(opars[i].ParameterType);
            }
            var gtype = _CurryWrappers[oparcnt];
            var wtype = gtype.MakeGenericType(upars);
            var wrapper = UnsafeEx.GetUninitializedObject(wtype) as CurryWrapper;
            wrapper.SetDelegate(odel);
            wrapper.SetTarget(target);
            string mname = noreturn ? "InvokeNoReturn" : "Invoke";
            var miwrapper = wtype.GetMethod(mname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);

            var gdtype = noreturn ? _ActionTypes[oparcnt - 1] : _FuncTypes[oparcnt - 1];
            var gutypes = new Type[oparcnt - (noreturn ? 1 : 0)];
            for (int i = 1; i < oparcnt; ++i)
            {
                gutypes[i - 1] = upars[i + 1];
            }
            if (!noreturn)
            {
                gutypes[gutypes.Length - 1] = upars[0];
            }
            var ddeltype = gdtype.MakeGenericType(gutypes);

            return UnsafeEx.CreateDelegateUnsafe(ddeltype, wrapper, miwrapper);
        }
        public static DDel Curry<DDel>(this Delegate odel, object target) where DDel : Delegate
        {
            return (DDel)Curry(odel, target, typeof(DDel));
        }
        public static DDel Curry<DDel, U1>(this Delegate odel, U1 target) where DDel : Delegate
        {
            var miodel = odel.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var opars = miodel.GetParameters();
            var oparcnt = opars.Length;
            var upars = new Type[oparcnt + 1];
            upars[0] = GetCompatible‌UnderlyingType(miodel.ReturnType);
            for (int i = 0; i < oparcnt; ++i)
            {
                upars[i + 1] = GetCompatible‌UnderlyingType(opars[i].ParameterType);
            }
            var gtype = _CurryWrappers[oparcnt];
            var wtype = gtype.MakeGenericType(upars);
            var wrapper = UnsafeEx.GetUninitializedObject(wtype) as CurryWrapper<U1>;
            wrapper.SetDelegate(odel);
            wrapper.Target = target;
            string mname = miodel.ReturnType == typeof(void) ? "InvokeNoReturn" : "Invoke";
            var miwrapper = wtype.GetMethod(mname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return UnsafeEx.CreateDelegateUnsafe<DDel>(wrapper, miwrapper);
        }

        private static Type[] _CurryWrappers =
        {
            null,
            typeof(CurryWrapper<,>),
            typeof(CurryWrapper<,,>),
            typeof(CurryWrapper<,,,>),
            typeof(CurryWrapper<,,,,>),
            typeof(CurryWrapper<,,,,,>),
            typeof(CurryWrapper<,,,,,,>),
            typeof(CurryWrapper<,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,,,,,>),
            typeof(CurryWrapper<,,,,,,,,,,,,,,,,>),
        };
        internal static Type[] _ActionTypes =
        {
            typeof(Action),
            typeof(Action<>),
            typeof(Action<,>),
            typeof(Action<,,>),
            typeof(Action<,,,>),
            typeof(Action<,,,,>),
            typeof(Action<,,,,,>),
            typeof(Action<,,,,,,>),
            typeof(Action<,,,,,,,>),
            typeof(Action<,,,,,,,,>),
            typeof(Action<,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,>),
            typeof(Action<,,,,,,,,,,,,,,,>),
        };
        internal static Type[] _FuncTypes =
        {
            typeof(Func<>),
            typeof(Func<,>),
            typeof(Func<,,>),
            typeof(Func<,,,>),
            typeof(Func<,,,,>),
            typeof(Func<,,,,,>),
            typeof(Func<,,,,,,>),
            typeof(Func<,,,,,,,>),
            typeof(Func<,,,,,,,,>),
            typeof(Func<,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,>),
            typeof(Func<,,,,,,,,,,,,,,,,>),
        };

        private abstract class CurryWrapper
        {
            public abstract object GetTarget();
            public abstract bool SetTarget(object target);
            public abstract void SetDelegate(Delegate del);
        }
        private abstract class CurryWrapper<U1> : CurryWrapper
        {
            public U1 Target;
            public override object GetTarget()
            {
                return Target;
            }
            public override bool SetTarget(object target)
            {
                if (target == null)
                {
                    Target = default;
                    return true;
                }
                if (target is U1)
                {
                    Target = (U1)target;
                    return true;
                }
                return false;
            }
        }
        private class CurryWrapper<R, U1> : CurryWrapper<U1>
        {
            private Func<U1, R> _InnerFunc;
            private Action<U1> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn()
            {
                _InnerAct(Target);
            }
            public R Invoke()
            {
                return _InnerFunc(Target);
            }
        }
        private class CurryWrapper<R, U1, U2> : CurryWrapper<U1>
        {
            private Func<U1, U2, R> _InnerFunc;
            private Action<U1, U2> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2)
            {
                _InnerAct(Target, u2);
            }
            public R Invoke(U2 u2)
            {
                return _InnerFunc(Target, u2);
            }
        }
        private class CurryWrapper<R, U1, U2, U3> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, R> _InnerFunc;
            private Action<U1, U2, U3> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3)
            {
                _InnerAct(Target, u2, u3);
            }
            public R Invoke(U2 u2, U3 u3)
            {
                return _InnerFunc(Target, u2, u3);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, R> _InnerFunc;
            private Action<U1, U2, U3, U4> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4)
            {
                _InnerAct(Target, u2, u3, u4);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4)
            {
                return _InnerFunc(Target, u2, u3, u4);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5)
            {
                _InnerAct(Target, u2, u3, u4, u5);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5)
            {
                return _InnerFunc(Target, u2, u3, u4, u5);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : CurryWrapper<U1>
        {
            private Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> _InnerFunc;
            private Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> _InnerAct;

            public override void SetDelegate(Delegate del)
            {
                if (typeof(R) == typeof(VoidReturn))
                {
                    _InnerAct = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>();
                }
                else
                {
                    _InnerFunc = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>();
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
            {
                _InnerAct = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
            {
                _InnerFunc = func;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                _InnerAct(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                return _InnerFunc(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
            }
        }
    }
}
