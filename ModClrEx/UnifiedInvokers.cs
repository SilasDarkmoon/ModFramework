using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mod
{
    #region StaticInvoker
    public class StaticInvoker<R> : UnifiedInvoker, IUnifiedFuncInvoker<R>
    {
        protected PointerFunc<R> Inner
        {
            get => (PointerFunc<R>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action>(method);
                    _FreeInvokable = new PointerFunc<R>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<R>>(method);
                    _FreeInvokable = new PointerFunc<R>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R>(fn);
            }
        }
        protected static Type[] _ParamTypes = Array.Empty<Type>();
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke()
        {
            return Inner.Invoke();
        }
    }
    public class StaticInvoker<R, U1, P1> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1>
    {
        protected PointerFunc<R, U1> Inner
        {
            get => (PointerFunc<R, U1>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsUnity)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1>>(method);
                    _FreeInvokable = new PointerFunc<R, U1>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1)
        {
            return Inner.Invoke(in p1);
        }
    }
    public class StaticInvoker<R, U1, U2, P1, P2> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2>
    {
        protected PointerFunc<R, U1, U2> Inner
        {
            get => (PointerFunc<R, U1, U2>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2)
        {
            return Inner.Invoke(in p1, in p2);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, P1, P2, P3> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3>
    {
        protected PointerFunc<R, U1, U2, U3> Inner
        {
            get => (PointerFunc<R, U1, U2, U3>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3)
        {
            return Inner.Invoke(in p1, in p2, in p3);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4>
    {
        protected PointerFunc<R, U1, U2, U3, U4> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
    }
    public class StaticInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> : UnifiedInvoker, IUnifiedFuncInvoker<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>
    {
        protected PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Inner
        {
            get => (PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_FreeInvokable;
            set => _FreeInvokable = value;
        }
        public StaticInvoker(MethodInfo method)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                if (method.ReturnType == typeof(void))
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(del);
                }
                else
                {
                    var del = UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>(method);
                    _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(del);
                }
            }
            else
            {
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();
                _FreeInvokable = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(fn);
            }
        }
        protected static Type[] _ParamTypes = { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) };
        public StaticInvoker(Type t, string methodName)
            : this((MethodInfo)UnsafeEx.FindMethod(UnsafeEx.GetMethods(t, methodName), _ParamTypes, withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            return Inner.Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
    }
    #endregion
}
