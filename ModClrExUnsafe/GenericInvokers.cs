using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml.Linq;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        private static Type GetRealType<U, P>()
        {
            var ut = typeof(U);
            if (ut == typeof(ByRefParam))
            {
                return typeof(P).MakeByRefType();
            }
            else
            {
                return ut;
            }
        }
        public static UnifiedRefFunc<R> CreateGenericInvoker<R>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = Array.Empty<Type>();
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R>();
        }
        public static UnifiedRefFunc<R, P1> CreateGenericInvoker<R, U1, P1>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, P1>();
        }
        public static UnifiedRefFunc<R, P1, P2> CreateGenericInvoker<R, U1, U2, P1, P2>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, P1, P2>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3> CreateGenericInvoker<R, U1, U2, U3, P1, P2, P3>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4> CreateGenericInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5> CreateGenericInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>(), GetRealType<U15, P15>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this MethodBase gmethod, Type[] genericTypes = null)
        {
            var rtype = typeof(R);
            var guessrtype = rtype != typeof(VoidReturn) && rtype != typeof(ByRefParam) ? rtype : null;
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>(), GetRealType<U15, P15>(), GetRealType<U16, P16>() };
            if (genericTypes == null)
            {
                var inferred = new List<Type>();
                FindOverload(new[] { gmethod }, utypes, guessrtype, withThis: true, checkGeneric: true, genericParameters: inferred);
                genericTypes = inferred.ToArray();
            }
            var method = gmethod.MakeGenericMethod(genericTypes);
            return method.CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
        }

        public static UnifiedRefFunc<R> CreateGenericInvoker<R>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = Array.Empty<Type>();
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R>();
        }
        public static UnifiedRefFunc<R, P1> CreateGenericInvoker<R, U1, P1>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, P1>();
        }
        public static UnifiedRefFunc<R, P1, P2> CreateGenericInvoker<R, U1, U2, P1, P2>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, P1, P2>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3> CreateGenericInvoker<R, U1, U2, U3, P1, P2, P3>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, P1, P2, P3>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4> CreateGenericInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, P1, P2, P3, P4>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5> CreateGenericInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>(), GetRealType<U15, P15>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>();
        }
        public static UnifiedRefFunc<R, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this Type type, string methodName, Type[] genericTypes = null)
        {
            var utypes = new[] { GetRealType<U1, P1>(), GetRealType<U2, P2>(), GetRealType<U3, P3>(), GetRealType<U4, P4>(), GetRealType<U5, P5>(), GetRealType<U6, P6>(), GetRealType<U7, P7>(), GetRealType<U8, P8>(), GetRealType<U9, P9>(), GetRealType<U10, P10>(), GetRealType<U11, P11>(), GetRealType<U12, P12>(), GetRealType<U13, P13>(), GetRealType<U14, P14>(), GetRealType<U15, P15>(), GetRealType<U16, P16>() };
            List<Type> inferred = genericTypes == null ? new List<Type>() : null;
            var gmethod = FindOverload(type.GetGenericMethods(methodName), utypes, withThis: true, checkGeneric: true, genericParameters: inferred);
            if (genericTypes == null) genericTypes = inferred.ToArray();
            return gmethod.MakeGenericMethod(genericTypes).CreateDelegateUnified<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>();
        }

        public static UnifiedRefFunc<R> CreateGenericInstanceInvoker<R, U1, P1>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, P1>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2> CreateGenericInstanceInvoker<R, U1, U2, P1, P2>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, P1, P2>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3> CreateGenericInstanceInvoker<R, U1, U2, U3, P1, P2, P3>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, P1, P2, P3>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, P1, P2, P3, P4>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, P1, P2, P3, P4, P5>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, P1, P2, P3, P4, P5, P6>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, P1, P2, P3, P4, P5, P6, P7>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, P1, P2, P3, P4, P5, P6, P7, P8>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, P1, P2, P3, P4, P5, P6, P7, P8, P9>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15>(methodName, genericTypes));
        }
        public static UnifiedRefFunc<R, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16> CreateGenericInstanceInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(this P1 target, string methodName, Type[] genericTypes = null)
        {
            return CreateInstanceInvoker(target, target.GetType().CreateGenericInvoker<R, U1, U2, U3, U4, U5, U6, U7, U8, U9, U10, U11, U12, U13, U14, U15, U16, P1, P2, P3, P4, P5, P6, P7, P8, P9, P10, P11, P12, P13, P14, P15, P16>(methodName, genericTypes));
        }
    }
}
