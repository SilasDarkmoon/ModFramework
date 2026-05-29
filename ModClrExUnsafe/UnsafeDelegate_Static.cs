using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mod.LowLevel
{
    public abstract class UnsafeDelegate : ICloneable
    {
        protected MethodBase _Method;
        public MethodBase Method => _Method;
        protected PointerDelegate _Inner;

        protected UnsafeDelegate(MethodBase mi)
        {
            if (mi == null) throw new ArgumentNullException(nameof(mi), $"Must provide method to bind {this.GetType().Name}!");
            _Method = mi;
            if (!RuntimeInfo.IsIl2Cpp)
            {
                RuntimeHelpers.PrepareMethod(mi.MethodHandle);
            }
        }
        protected UnsafeDelegate Clone()
        {
            var clone = MemberwiseClone() as UnsafeDelegate;
            clone._Inner = (PointerDelegate)((ICloneable)_Inner).Clone();
            return clone;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
    }
    public class StaticAction : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction(UnsafeEx.CreateDelegateUnsafe<Action>(mi));
            }
            else
            {
                _Inner = new PointerAction(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(Array.Empty<Type>(), withThis: true))
        {
        }
        public void Invoke()
        {
            ((PointerAction)_Inner).Invoke();
        }
        public new StaticAction Clone()
        {
            return base.Clone() as StaticAction;
        }
    }
    public class StaticAction<U1> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1>(UnsafeEx.CreateDelegateUnsafe<Action<U1>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) }, withThis: true))
        {
        }
        public void Invoke(U1 p1)
        {
            ((PointerAction<U1>)_Inner).Invoke(p1);
        }
        public new StaticAction<U1> Clone()
        {
            return base.Clone() as StaticAction<U1>;
        }
    }
    public class StaticAction<U1, U2> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2)
        {
            ((PointerAction<U1, U2>)_Inner).Invoke(p1, p2);
        }
        public new StaticAction<U1, U2> Clone()
        {
            return base.Clone() as StaticAction<U1, U2>;
        }
    }
    public class StaticAction<U1, U2, U3> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3)
        {
            ((PointerAction<U1, U2, U3>)_Inner).Invoke(p1, p2, p3);
        }
        public new StaticAction<U1, U2, U3> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3>;
        }
    }
    public class StaticAction<U1, U2, U3, U4> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            ((PointerAction<U1, U2, U3, U4>)_Inner).Invoke(p1, p2, p3, p4);
        }
        public new StaticAction<U1, U2, U3, U4> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            ((PointerAction<U1, U2, U3, U4, U5>)_Inner).Invoke(p1, p2, p3, p4, p5);
        }
        public new StaticAction<U1, U2, U3, U4, U5> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6>)_Inner).Invoke(p1, p2, p3, p4, p5, p6);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public StaticAction(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(UnsafeEx.CreateDelegateUnsafe<Action<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>(mi));
            }
            else
            {
                _Inner = new PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticAction(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, withThis: true))
        {
        }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16);
        }
        public new StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return base.Clone() as StaticAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }

    public class StaticFunc<R> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R>(UnsafeEx.CreateDelegateUnsafe<Func<R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(Array.Empty<Type>(), returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke()
        {
            return ((PointerFunc<R>)_Inner).Invoke();
        }
        public new StaticFunc<R> Clone()
        {
            return base.Clone() as StaticFunc<R>;
        }
    }
    public class StaticFunc<R, U1> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1>(UnsafeEx.CreateDelegateUnsafe<Func<U1, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) } , returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1)
        {
            return ((PointerFunc<R, U1>)_Inner).Invoke(p1);
        }
        public new StaticFunc<R, U1> Clone()
        {
            return base.Clone() as StaticFunc<R, U1>;
        }
    }
    public class StaticFunc<R, U1, U2> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2)
        {
            return ((PointerFunc<R, U1, U2>)_Inner).Invoke(p1, p2);
        }
        public new StaticFunc<R, U1, U2> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2>;
        }
    }
    public class StaticFunc<R, U1, U2, U3> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3)
        {
            return ((PointerFunc<R, U1, U2, U3>)_Inner).Invoke(p1, p2, p3);
        }
        public new StaticFunc<R, U1, U2, U3> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            return ((PointerFunc<R, U1, U2, U3, U4>)_Inner).Invoke(p1, p2, p3, p4);
        }
        public new StaticFunc<R, U1, U2, U3, U4> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5>)_Inner).Invoke(p1, p2, p3, p4, p5);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6>)_Inner).Invoke(p1, p2, p3, p4, p5, p6);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public StaticFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(UnsafeEx.CreateDelegateUnsafe<Func<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, R>>(mi));
            }
            else
            {
                _Inner = new PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16);
        }
        public new StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return base.Clone() as StaticFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }

    public class StaticRefFunc<R> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(Array.Empty<Type>(), returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke()
        {
            return ref ((PointerRefFunc<R>)_Inner).Invoke();
        }
        public new StaticRefFunc<R> Clone()
        {
            return base.Clone() as StaticRefFunc<R>;
        }
    }
    public class StaticRefFunc<R, U1> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1)
        {
            return ref ((PointerRefFunc<R, U1>)_Inner).Invoke(p1);
        }
        public new StaticRefFunc<R, U1> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1>;
        }
    }
    public class StaticRefFunc<R, U1, U2> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2)
        {
            return ref ((PointerRefFunc<R, U1, U2>)_Inner).Invoke(p1, p2);
        }
        public new StaticRefFunc<R, U1, U2> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3>)_Inner).Invoke(p1, p2, p3);
        }
        public new StaticRefFunc<R, U1, U2, U3> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4>)_Inner).Invoke(p1, p2, p3, p4);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5>)_Inner).Invoke(p1, p2, p3, p4, p5);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6>)_Inner).Invoke(p1, p2, p3, p4, p5, p6);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public StaticRefFunc(MethodBase mi) : base(mi)
        {
            if (RuntimeInfo.IsIl2Cpp)
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(UnsafeEx.CreateDelegateUnsafe<RefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>>(mi));
            }
            else
            {
                _Inner = new PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>(mi.MethodHandle.GetFunctionPointer());
            }
        }
        public StaticRefFunc(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(p1, p2, p3, p4, p5, p6, p7, p8, p9, p10, p11, p12, p13, p14, p15, p16);
        }
        public new StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return base.Clone() as StaticRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }
}
