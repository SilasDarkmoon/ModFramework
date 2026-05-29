using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;

namespace Mod.LowLevel
{
    public class StaticActionWithRef<U1, P1> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1)
        {
            ((PointerAction<U1>)_Inner).Invoke(in p1);
        }
        public new StaticActionWithRef<U1, P1> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, P1>;
        }
    }
    public class StaticActionWithRef<U1, U2, P1, P2> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2)
        {
            ((PointerAction<U1, U2>)_Inner).Invoke(in p1, in p2);
        }
        public new StaticActionWithRef<U1, U2, P1, P2> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, P1, P2>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, P1, P2, P3> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3)
        {
            ((PointerAction<U1, U2, U3>)_Inner).Invoke(in p1, in p2, in p3);
        }
        public new StaticActionWithRef<U1, U2, U3, P1, P2, P3> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, P1, P2, P3>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, P1, P2, P3, P4> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            ((PointerAction<U1, U2, U3, U4>)_Inner).Invoke(in p1, in p2, in p3, in p4);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, P1, P2, P3, P4> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, P1, P2, P3, P4>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            ((PointerAction<U1, U2, U3, U4, U5>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>;
        }
    }
    public class StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> : UnsafeDelegate
    {
        public StaticActionWithRef(MethodBase mi) : base(mi)
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
        public StaticActionWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, withThis: true))
        {
        }
        public void Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            ((PointerAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
        public new StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> Clone()
        {
            return base.Clone() as StaticActionWithRef<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>;
        }
    }

    public class StaticFuncWithRef<R, U1, P1> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) } , returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1)
        {
            return ((PointerFunc<R, U1>)_Inner).Invoke(in p1);
        }
        public new StaticFuncWithRef<R, U1, P1> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, P1>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, P1, P2> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2)
        {
            return ((PointerFunc<R, U1, U2>)_Inner).Invoke(in p1, in p2);
        }
        public new StaticFuncWithRef<R, U1, U2, P1, P2> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, P1, P2>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, P1, P2, P3> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3)
        {
            return ((PointerFunc<R, U1, U2, U3>)_Inner).Invoke(in p1, in p2, in p3);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, P1, P2, P3> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, P1, P2, P3>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            return ((PointerFunc<R, U1, U2, U3, U4>)_Inner).Invoke(in p1, in p2, in p3, in p4);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>;
        }
    }
    public class StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> : UnsafeDelegate
    {
        public StaticFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, returnType: typeof(R), withThis: true))
        {
        }
        public R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            return ((PointerFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
        public new StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> Clone()
        {
            return base.Clone() as StaticFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>;
        }
    }

    public class StaticRefFuncWithRef<R, U1, P1> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1)
        {
            return ref ((PointerRefFunc<R, U1>)_Inner).Invoke(in p1);
        }
        public new StaticRefFuncWithRef<R, U1, P1> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, P1>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, P1, P2> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2)
        {
            return ref ((PointerRefFunc<R, U1, U2>)_Inner).Invoke(in p1, in p2);
        }
        public new StaticRefFuncWithRef<R, U1, U2, P1, P2> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, P1, P2>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, P1, P2, P3> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3>)_Inner).Invoke(in p1, in p2, in p3);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, P1, P2, P3> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, P1, P2, P3>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4>)_Inner).Invoke(in p1, in p2, in p3, in p4);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, P1, P2, P3, P4>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in U15 p15)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>;
        }
    }
    public class StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> : UnsafeDelegate
    {
        public StaticRefFuncWithRef(MethodBase mi) : base(mi)
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
        public StaticRefFuncWithRef(Type type, string mname) : this(type.GetMethods(mname).FindMethod(new[] { typeof(U1), typeof(U2), typeof(U3), typeof(U4), typeof(U5), typeof(U6), typeof(U7), typeof(U8), typeof(U9), typeof(U10), typeof(U11), typeof(U12), typeof(U13), typeof(U14), typeof(U15), typeof(U16) }, returnType: typeof(R).MakeByRefType(), withThis: true))
        {
        }
        public ref R Invoke(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in U15 p15, in P16 p16)
        {
            return ref ((PointerRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>)_Inner).Invoke(in p1, in p2, in p3, in p4, in p5, in p6, in p7, in p8, in p9, in p10, in p11, in p12, in p13, in p14, in p15, in p16);
        }
        public new StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> Clone()
        {
            return base.Clone() as StaticRefFuncWithRef<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>;
        }
    }
}
