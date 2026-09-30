using System;
using System.Collections.Generic;
using System.Reflection;

namespace Mod.LowLevel
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

        public static UnifiedRefFunc<R> Curry<R, U1>(this UnifiedRefFunc<R, U1> func, U1 target)
        {
            return new CurryWrapper<R, U1>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2> Curry<R, U1, U2>(this UnifiedRefFunc<R, U1, U2> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3> Curry<R, U1, U2, U3>(this UnifiedRefFunc<R, U1, U2, U3> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4> Curry<R, U1, U2, U3, U4>(this UnifiedRefFunc<R, U1, U2, U3, U4> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5> Curry<R, U1, U2, U3, U4, U5>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6> Curry<R, U1, U2, U3, U4, U5, U6>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7> Curry<R, U1, U2, U3, U4, U5, U6, U7>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(func) { Target = target }.UnifiedInvoke;
        }
        public static UnifiedRefFunc<R, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Curry<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(this UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> func, U1 target)
        {
            return new CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(func) { Target = target }.UnifiedInvoke;
        }

        public static D PostInvoke<D>(this D del, Action<object> onPostInvokeSyncTarget) where D : Delegate
        {
            if (del.Target is CurryWrapper wrapper)
            {
                wrapper.PostInvoke += onPostInvokeSyncTarget;
                return del;
            }
            else
            {
                throw new ArgumentException("The delegate you provide to PostInvoke must be a curried delegate!", nameof(del));
            }
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
        public static Type GetNonByRefType(Type ptype)
        {
            if (ptype.IsByRef)
            {
                return ptype.GetElementType();
            }
            else
            {
                return ptype;
            }
        }
        public static string GetInvokeMethodName(Type returnType)
        {
            if (returnType.IsByRef)
            {
                return "UnifiedInvoke";
            }
            else if (returnType == typeof(void))
            {
                return "InvokeNoReturn";
            }
            else
            {
                return "Invoke";
            }
        }

        private static (CurryWrapper wrapper, MethodInfo invokeMethod, MethodInfo oldDelegateMethodInfo) PrepareCurryWrapper(Delegate odel, object target)
        {
            var miodel = odel.GetType().GetMethod("Invoke", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            var opars = miodel.GetParameters();
            var oparcnt = opars.Length;
            var upars = new Type[oparcnt + 1];
            if (miodel.ReturnType.IsByRef)
            {
                upars[0] = GetNonByRefType(miodel.ReturnType);
                for (int i = 0; i < oparcnt; ++i)
                {
                    upars[i + 1] = GetNonByRefType(opars[i].ParameterType);
                }
            }
            else
            {
                upars[0] = GetCompatible‌UnderlyingType(miodel.ReturnType);
                for (int i = 0; i < oparcnt; ++i)
                {
                    upars[i + 1] = GetCompatible‌UnderlyingType(opars[i].ParameterType);
                }
            }
            var gtype = _CurryWrappers[oparcnt];
            var wtype = gtype.MakeGenericType(upars);
            var wrapper = UnsafeEx.GetUninitializedObject(wtype) as CurryWrapper;
            wrapper.SetDelegate(odel);
            wrapper.SetTarget(target);
            string mname = GetInvokeMethodName(miodel.ReturnType);
            var miwrapper = wtype.GetMethod(mname, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            return (wrapper, miwrapper, miodel);
        }
        public static Type GetCurriedDelegateType(Type returnType, ParameterInfo[] paramInfos)
        {
            Type[] gdtypes;
            Type[] gutypes;
            if (returnType.IsByRef)
            {
                gdtypes = _UnifiedFuncTypes;
                gutypes = new Type[paramInfos.Length];
                gutypes[0] = returnType.GetElementType();
                for (int i = 1; i < paramInfos.Length; ++i)
                {
                    gutypes[i] = GetNonByRefType(paramInfos[i].ParameterType);
                }
            }
            else if (returnType == typeof(void))
            {
                gdtypes = _ActionTypes;
                gutypes = new Type[paramInfos.Length - 1];
                for (int i = 1; i < paramInfos.Length; ++i)
                {
                    gutypes[i - 1] = GetCompatibleUnderlyingType(paramInfos[i].ParameterType);
                }
            }
            else
            {
                gdtypes = _FuncTypes;
                gutypes = new Type[paramInfos.Length];
                gutypes[paramInfos.Length - 1] = GetCompatibleUnderlyingType(returnType);
                for (int i = 1; i < paramInfos.Length; ++i)
                {
                    gutypes[i - 1] = GetCompatibleUnderlyingType(paramInfos[i].ParameterType);
                }
            }
            var gdtype = gdtypes[paramInfos.Length - 1];
            var ddeltype = gdtype.MakeGenericType(gutypes);
            return ddeltype;
        }
        public static Delegate Curry(this Delegate odel, object target, Type ddeltype)
        {
            var (wrapper, miwrapper, _) = PrepareCurryWrapper(odel, target);
            return UnsafeEx.CreateDelegateUnsafe(ddeltype, wrapper, miwrapper);
        }
        public static Delegate Curry(this Delegate odel, object target)
        {
            var (wrapper, miwrapper, miodel) = PrepareCurryWrapper(odel, target);
            var ddeltype = GetCurriedDelegateType(miodel.ReturnType, miodel.GetParameters());
            return UnsafeEx.CreateDelegateUnsafe(ddeltype, wrapper, miwrapper);
        }
        public static DDel Curry<DDel>(this Delegate odel, object target) where DDel : Delegate
        {
            return (DDel)Curry(odel, target, typeof(DDel));
        }
        public static DDel Curry<DDel, U1>(this Delegate odel, U1 target) where DDel : Delegate
        {
            var (wrapper, miwrapper, _) = PrepareCurryWrapper(odel, null);
            ((CurryWrapper<U1>)wrapper).Target = target;
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
        internal static Type[] _UnifiedFuncTypes =
        {
            typeof(UnifiedRefFunc<>),
            typeof(UnifiedRefFunc<,>),
            typeof(UnifiedRefFunc<,,>),
            typeof(UnifiedRefFunc<,,,>),
            typeof(UnifiedRefFunc<,,,,>),
            typeof(UnifiedRefFunc<,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,,,,,>),
            typeof(UnifiedRefFunc<,,,,,,,,,,,,,,,,>),
        };

        private abstract class CurryWrapper
        {
            public abstract object GetTarget();
            public abstract bool SetTarget(object target);
            public abstract void SetDelegate(Delegate del);
            public event Action<object> PostInvoke;
            protected void TrigPostInvoke()
            {
                PostInvoke?.Invoke(GetTarget());
            }
        }
        private abstract class CurryWrapper<U1> : CurryWrapper
        {
            public U1 Target;
            public Delegate InnerDel;
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
            public override void SetDelegate(Delegate del)
            {
                InnerDel = del;
            }
            protected CurryWrapper() { }
            protected CurryWrapper(Delegate del)
            {
                SetDelegate(del);
            }
        }
        private class CurryWrapper<R, U1> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1>) && !(del is Func<U1, R>) && !(del is UnifiedRefFunc<R, U1>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn()
            {
                try
                {
                    if (InnerDel is Action<U1> act)
                    {
                        act(Target);
                    }
                    else if (InnerDel is Func<U1, R> func)
                    {
                        func(Target);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1> uni)
                    {
                        uni(out _, in Target);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke()
            {
                try
                {
                    if (InnerDel is Action<U1> act)
                    {
                        act(Target);
                        return default;
                    }
                    else if (InnerDel is Func<U1, R> func)
                    {
                        return func(Target);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1> uni)
                    {
                        return uni(out _, in Target);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r)
            {
                try
                {
                    if (InnerDel is Action<U1> act)
                    {
                        act(Target);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, R> func)
                    {
                        r = func(Target);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1> uni)
                    {
                        return ref uni(out r, in Target);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2>) && !(del is Func<U1, U2, R>) && !(del is UnifiedRefFunc<R, U1, U2>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2)
            {
                try
                {
                    if (InnerDel is Action<U1, U2> act)
                    {
                        act(Target, u2);
                    }
                    else if (InnerDel is Func<U1, U2, R> func)
                    {
                        func(Target, u2);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2> uni)
                    {
                        uni(out _, in Target, in u2);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2)
            {
                try
                {
                    if (InnerDel is Action<U1, U2> act)
                    {
                        act(Target, u2);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, R> func)
                    {
                        return func(Target, u2);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2> uni)
                    {
                        return uni(out _, in Target, in u2);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2)
            {
                try
                {
                    if (InnerDel is Action<U1, U2> act)
                    {
                        act(Target, u2);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, R> func)
                    {
                        r = func(Target, u2);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2> uni)
                    {
                        return ref uni(out r, in Target, in u2);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3>) && !(del is Func<U1, U2, U3, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3> act)
                    {
                        act(Target, u2, u3);
                    }
                    else if (InnerDel is Func<U1, U2, U3, R> func)
                    {
                        func(Target, u2, u3);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3> uni)
                    {
                        uni(out _, in Target, in u2, in u3);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3> act)
                    {
                        act(Target, u2, u3);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, R> func)
                    {
                        return func(Target, u2, u3);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3> uni)
                    {
                        return uni(out _, in Target, in u2, in u3);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3> act)
                    {
                        act(Target, u2, u3);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, R> func)
                    {
                        r = func(Target, u2, u3);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4>) && !(del is Func<U1, U2, U3, U4, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4> act)
                    {
                        act(Target, u2, u3, u4);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, R> func)
                    {
                        func(Target, u2, u3, u4);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4> act)
                    {
                        act(Target, u2, u3, u4);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, R> func)
                    {
                        return func(Target, u2, u3, u4);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4> act)
                    {
                        act(Target, u2, u3, u4);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, R> func)
                    {
                        r = func(Target, u2, u3, u4);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5>) && !(del is Func<U1, U2, U3, U4, U5, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5> act)
                    {
                        act(Target, u2, u3, u4, u5);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, R> func)
                    {
                        func(Target, u2, u3, u4, u5);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5> act)
                    {
                        act(Target, u2, u3, u4, u5);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, R> func)
                    {
                        return func(Target, u2, u3, u4, u5);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5> act)
                    {
                        act(Target, u2, u3, u4, u5);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6>) && !(del is Func<U1, U2, U3, U4, U5, U6, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6> act)
                    {
                        act(Target, u2, u3, u4, u5, u6);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6> act)
                    {
                        act(Target, u2, u3, u4, u5, u6);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6> act)
                    {
                        act(Target, u2, u3, u4, u5, u6);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11, in U12 u12)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11, in U12 u12, in U13 u13)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11, in U12 u12, in U13 u13, in U14 u14)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11, in U12 u12, in U13 u13, in U14 u14, in U15 u15)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
        private class CurryWrapper<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : CurryWrapper<U1>
        {
            public override void SetDelegate(Delegate del)
            {
                base.SetDelegate(del);
                if (!(del is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>) && !(del is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>) && !(del is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>))
                {
                    if (typeof(R) == typeof(VoidReturn))
                    {
                        InnerDel = del.ChangeDelegateType<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>();
                    }
                    else
                    {
                        InnerDel = del.ChangeDelegateType<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>();
                    }
                }
            }
            public CurryWrapper() { }
            public CurryWrapper(Delegate del) : base(del) { }
            public CurryWrapper(Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
            {
                InnerDel = act;
            }
            public CurryWrapper(Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
            {
                InnerDel = func;
            }
            public CurryWrapper(UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> unified)
            {
                InnerDel = unified;
            }

            public void InvokeNoReturn(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
                    {
                        func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> uni)
                    {
                        uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15, in u16);
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public R Invoke(U2 u2, U3 u3, U4 u4, U5 u5, U6 u6, U7 u7, U8 u8, U9 u9, U10 u10, U11 u11, U12 u12, U13 u13, U14 u14, U15 u15, U16 u16)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                        return default;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
                    {
                        return func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> uni)
                    {
                        return uni(out _, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15, in u16);
                    }
                    else
                    {
                        return default;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
            public ref R UnifiedInvoke(out R r, in U2 u2, in U3 u3, in U4 u4, in U5 u5, in U6 u6, in U7 u7, in U8 u8, in U9 u9, in U10 u10, in U11 u11, in U12 u12, in U13 u13, in U14 u14, in U15 u15, in U16 u16)
            {
                try
                {
                    if (InnerDel is Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> act)
                    {
                        act(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                        r = default;
                        return ref r;
                    }
                    else if (InnerDel is Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R> func)
                    {
                        r = func(Target, u2, u3, u4, u5, u6, u7, u8, u9, u10, u11, u12, u13, u14, u15, u16);
                        return ref r;
                    }
                    else if (InnerDel is UnifiedRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> uni)
                    {
                        return ref uni(out r, in Target, in u2, in u3, in u4, in u5, in u6, in u7, in u8, in u9, in u10, in u11, in u12, in u13, in u14, in u15, in u16);
                    }
                    else
                    {
                        r = default;
                        return ref r;
                    }
                }
                finally
                {
                    TrigPostInvoke();
                }
            }
        }
    }
}
