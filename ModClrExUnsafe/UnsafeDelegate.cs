using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mod.LowLevel
{
    public abstract class UnsafeDelegate : ICloneable
    {
        protected MethodBase _Method;
        public MethodBase Method => _Method;
        protected UnsafeDelegate(MethodBase mi)
        {
            _Method = mi;
        }
        object ICloneable.Clone()
        {
            return MemberwiseClone();
        }
    }
    public class UnsafeAction : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke()
        {
            throw new NotImplementedException();
        }
        public UnsafeAction Clone()
        {
            return MemberwiseClone() as UnsafeAction;
        }
    }
    public class UnsafeAction<U1> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1>(in P1 p1)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1>;
        }
    }
    public class UnsafeAction<U1, U2> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2>(in P1 p1, in P2 p2)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2>;
        }
    }
    public class UnsafeAction<U1, U2, U3> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public UnsafeAction(MethodBase mi) : base(mi) { }
        public void Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            throw new NotImplementedException();
        }
        public void Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            throw new NotImplementedException();
        }
        public UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return MemberwiseClone() as UnsafeAction<U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }

    public class UnsafeFunc<R> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke()
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R>;
        }
    }
    public class UnsafeFunc<R, U1> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1>(in P1 p1)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1>;
        }
    }
    public class UnsafeFunc<R, U1, U2> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2>(in P1 p1, in P2 p2)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public UnsafeFunc(MethodBase mi) : base(mi) { }
        public R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            throw new NotImplementedException();
        }
        public R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            throw new NotImplementedException();
        }
        public UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return MemberwiseClone() as UnsafeFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }

    public class UnsafeRefFunc<R> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke()
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R>;
        }
    }
    public class UnsafeRefFunc<R, U1> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1>(in P1 p1)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2>(in P1 p1, in P2 p2)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3>(in P1 p1, in P2 p2, in P3 p3)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4>(in P1 p1, in P2 p2, in P3 p3, in P4 p4)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15>;
        }
    }
    public class UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> : UnsafeDelegate
    {
        public UnsafeRefFunc(MethodBase mi) : base(mi) { }
        public ref R Invoke(U1 p1, U2 p2, U3 p3, U4 p4, U5 p5, U6 p6, U7 p7, U8 p8, U9 p9, U10 p10, U11 p11, U12 p12, U13 p13, U14 p14, U15 p15, U16 p16)
        {
            throw new NotImplementedException();
        }
        public ref R Invoke<P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(in P1 p1, in P2 p2, in P3 p3, in P4 p4, in P5 p5, in P6 p6, in P7 p7, in P8 p8, in P9 p9, in P10 p10, in P11 p11, in P12 p12, in P13 p13, in P14 p14, in P15 p15, in P16 p16)
        {
            throw new NotImplementedException();
        }
        public UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16> Clone()
        {
            return MemberwiseClone() as UnsafeRefFunc<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16>;
        }
    }
}
