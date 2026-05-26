using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        #region GetUninitializedObject
        public static object GetUninitializedObject(this Type t)
        {
            return ObjectCreatorImp.GetUninitializedObject(t);
        }

        public static T GetUninitialized<T>()
        {
            var type = typeof(T);
            if (type.IsValueType)
            {
                return default(T);
            }
            else
            {
                return (T)GetUninitializedObject(type);
            }
        }
        public static T GetUninitializedStruct<T>() where T : struct
        {
            return default(T);
        }
        public static T GetUninitializedObject<T>() where T : class
        {
            return (T)GetUninitializedObject(typeof(T));
        }

        private interface IObjectCreator
        {
            object GetUninitializedObject(Type t);
        }

        private static class ObjectCreatorImp
        {
            private static IObjectCreator _Creator;
            static ObjectCreatorImp()
            {
                switch (RuntimeInfo.Runtime)
                {
                    case RuntimeInfo.RuntimeCategory.NetCore:
                    case RuntimeInfo.RuntimeCategory.NetFramework:
                        _Creator = new ObjectCreatorCore();
                        break;
                    case RuntimeInfo.RuntimeCategory.Mono:
                    case RuntimeInfo.RuntimeCategory.UnityMono:
                        _Creator = new ObjectCreatorMono();
                        break;
                    case RuntimeInfo.RuntimeCategory.NativeAOT:
                        _Creator = new ObjectCreatorNativeAOT();
                        break;
                    case RuntimeInfo.RuntimeCategory.UnityIl2Cpp:
                        _Creator = new ObjectCreatorIl2Cpp();
                        break;
                }
            }

            public static object GetUninitializedObject(Type t)
            {
                return _Creator.GetUninitializedObject(t);
            }
        }

        private class ObjectCreatorCore : IObjectCreator
        {
            private static Action _DummyTemplate = DummyFunc;
            private static void DummyFunc() { }

            [StructLayout(LayoutKind.Sequential)]
            private struct ObjectHeader
            {
                public IntPtr TypeHandle;
                public IntPtr SyncAddress;
            }

            private delegate object CreateUninitializedObjectDel(Type type);
            private static CreateUninitializedObjectDel _CreateUninitializedObjectDelFunc;

            static ObjectCreatorCore()
            {
                var comi = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject");
                if (comi != null)
                {
                    _CreateUninitializedObjectDelFunc = (CreateUninitializedObjectDel)Delegate.CreateDelegate(typeof(CreateUninitializedObjectDel), comi);
                }
            }

            public object GetUninitializedObject(Type t)
            {
                if (typeof(Delegate).IsAssignableFrom(t))
                {
                    var rv = _DummyTemplate.Clone();
                    ref var oheader = ref rv.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
                    oheader.TypeHandle = t.TypeHandle.Value;
                    MemClean(rv);
                    return rv;
                }
                else
                {
                    if (_CreateUninitializedObjectDelFunc == null)
                    {
                        return FormatterServices.GetUninitializedObject(t);
                    }
                    else
                    {
                        return _CreateUninitializedObjectDelFunc(t);
                    }
                }
            }
        }

        private class ObjectCreatorMono : IObjectCreator
        {
            private delegate object CreateUninitializedObjectDel(Type type);
            private static CreateUninitializedObjectDel _CreateUninitializedObjectDelFunc;

            static ObjectCreatorMono()
            {
                var comi = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject");
                if (comi != null)
                {
                    _CreateUninitializedObjectDelFunc = (CreateUninitializedObjectDel)Delegate.CreateDelegate(typeof(CreateUninitializedObjectDel), comi);
                }
            }

            public object GetUninitializedObject(Type t)
            {
                if (_CreateUninitializedObjectDelFunc == null)
                {
                    return FormatterServices.GetUninitializedObject(t);
                }
                else
                {
                    return _CreateUninitializedObjectDelFunc(t);
                }
            }
        }

        private class ObjectCreatorNativeAOT : IObjectCreator
        {
            private delegate object RhNewObjectDel(IntPtr typeHandle);
            private static RhNewObjectDel _RhNewObjectFunc;

            private delegate Delegate CreateDelegateWithoutSignatureValidationDel(MethodInfo thiz, Type delType, object target, bool isStatic, bool isOpen);
            private static CreateDelegateWithoutSignatureValidationDel _CreateDelegateWithoutSignatureValidationFunc;

            static ObjectCreatorNativeAOT()
            {

                var trl = Type.GetType("System.Runtime.RuntimeImports, System.Private.CoreLib");
                var minew = trl.GetMethod("RhNewObject", BindingFlags.Static | BindingFlags.NonPublic);
                var cmi = minew.GetType().GetMethod("CreateDelegateWithoutSignatureValidation", BindingFlags.Instance | BindingFlags.NonPublic);
                _CreateDelegateWithoutSignatureValidationFunc = (CreateDelegateWithoutSignatureValidationDel)cmi.Invoke(cmi, new object[] { typeof(CreateDelegateWithoutSignatureValidationDel), null, false, true });
                _RhNewObjectFunc = (RhNewObjectDel)_CreateDelegateWithoutSignatureValidationFunc(minew, typeof(RhNewObjectDel), null, true, true);
            }

            public object GetUninitializedObject(Type t)
            {
                return _RhNewObjectFunc(t.TypeHandle.Value);
            }
        }

        private class ObjectCreatorIl2Cpp : IObjectCreator
        {
            private delegate object CreateUninitializedObjectDel(Type type);
            private static CreateUninitializedObjectDel _CreateUninitializedObjectDelFunc;

            static ObjectCreatorIl2Cpp()
            {
                var comi = typeof(object).Assembly.GetType("System.Runtime.Remoting.Activation.ActivationServices").GetMethod("AllocateUninitializedClassInstance");
                //var comi = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject");
                _CreateUninitializedObjectDelFunc = (CreateUninitializedObjectDel)Delegate.CreateDelegate(typeof(CreateUninitializedObjectDel), comi);
            }

            public object GetUninitializedObject(Type t)
            {
                return _CreateUninitializedObjectDelFunc(t);
            }
        }
        #endregion
    }

    public static partial class UnsafeEx
    {
        #region MemberwiseClone
        public static object MemberwiseClone(this object obj)
        {
            return MemberwiseCloneImp.MemberwiseClone(obj);
        }

        private static class MemberwiseCloneImp
        {
            private delegate object MemberwiseCloneDel(object obj);
            private static MemberwiseCloneDel _MemberwiseCloneFunc;

            static MemberwiseCloneImp()
            {
                var mic = typeof(object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic);
                _MemberwiseCloneFunc = (MemberwiseCloneDel)Delegate.CreateDelegate(typeof(MemberwiseCloneDel), mic);
            }

            public static object MemberwiseClone(object obj)
            {
                return _MemberwiseCloneFunc(obj);
            }
        }
        #endregion
    }

    #region Activator
    public static partial class UnsafeEx
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct AllocatedObject<T>
        {
            private IntPtr _Preserved0;
            private IntPtr _Preserved1;
            internal T _Value;

            public AllocatedObject(T value)
            {
                _Preserved0 = default;
                _Preserved1 = default;
                _Value = value;
            }

            public T Value => _Value;

            public static implicit operator T(AllocatedObject<T> a)
            {
                return a.Value;
            }
            public static implicit operator AllocatedObject<T>(T val)
            {
                return new AllocatedObject<T>(val);
            }
        }
        [StructLayout(LayoutKind.Sequential)]
        public struct ConstructorToken
        {
        }

        public class Constructor
        {
            private Type _TargetType;
            internal Delegate _ConDel;

            public Constructor(ConstructorInfo con, Type delType)
            {
                CheckArguments(con, delType);
                _ConDel = CreateDelegateUnsafe(delType, con);
                _TargetType = con.DeclaringType;
            }

            public static void CheckArguments(ConstructorInfo con, Type delType)
            {
                if (con == null) throw new ArgumentNullException(nameof(con), "Please provide a constructor!");
                if (delType == null) throw new ArgumentNullException(nameof(delType), "Please provide a Delegate Type to bind constructor to!");
                if (!typeof(Delegate).IsAssignableFrom(delType)) throw new ArgumentException("the delType must be a Delegate Type!", nameof(delType));
                var miInvoke = delType.GetMethod("Invoke");
                var pars = miInvoke.GetParameters();
                if (pars.Length < 1) throw new ArgumentException("the delType must takes a 'ref ConstructorToken' as its first parameter!", nameof(delType));
                var par0 = pars[0].ParameterType;
                if (!par0.IsByRef) throw new ArgumentException("the delType must takes a 'ref ConstructorToken' as its first parameter!", nameof(delType));
                var par0elem = par0.GetElementType();
                if (par0elem != typeof(ConstructorToken)) throw new ArgumentException("the delType must takes a 'ref ConstructorToken' as its first parameter!", nameof(delType));
            }

            public ref ConstructorToken Alloc(out object obj)
            {
                obj = GetUninitializedObject(_TargetType);
                RawRef r = new RawRef();
                RawRef rrv = new RawRef();
                for (; ; )
                {
                    r.SetRefObj(obj);
                    var preaddr = r.Address;
                    rrv.Address = preaddr;
                    if (!RuntimeInfo.IsNativeAOT && _TargetType.IsValueType)
                    {
                        rrv.Address += GetObjectHeaderSize();
                    }
                    ref var rv = ref rrv.GetRef<ConstructorToken>();
                    r.SetRefObj(obj);
                    var postaddr = r.Address;
                    if (preaddr == postaddr)
                    {
                        return ref rv;
                    }
                }
            }
            public ref ConstructorToken AllocWrapped<T>(out AllocatedObject<T> obj)
            {
                //if (!typeof(T).IsAssignableFrom(_TargetType))
                //{
                //    throw new ArgumentException($"Please provide correct T to receive value of {_TargetType}", nameof(T));
                //}
                obj = new AllocatedObject<T>();
                if (!typeof(T).IsValueType)
                {
                    var offset = obj.GetFieldOffset(ref obj._Value).OfType<object>();
                    ref var rinner = ref obj.GetField(offset);
                    return ref Alloc(out rinner);
                }
                RawRef r = new RawRef();
                RawRef rrv = new RawRef();
                for (; ; )
                {
                    r.SetRef(ref obj._Value);
                    var preaddr = r.Address;
                    rrv.Address = preaddr;
                    if (RuntimeInfo.IsNativeAOT)
                    {
                        rrv.Address -= GetObjectHeaderSize();
                    }
                    ref var rv = ref rrv.GetRef<ConstructorToken>();
                    r.SetRef(ref obj._Value);
                    var postaddr = r.Address;
                    if (preaddr == postaddr)
                    {
                        return ref rv;
                    }
                }
            }
            /// <summary>
            /// if T is ValueType, please make sure it is on the evaluation stack.
            /// </summary>
            public ref ConstructorToken Alloc<T>(out T val)
            {
                val = default;
                if (!typeof(T).IsValueType)
                {
                    ref var obj = ref ConvertRefCommon<T, object>(ref val);
                    return ref Alloc(out obj);
                }
                RawRef r = new RawRef();
                RawRef rrv = new RawRef();
                for (; ; )
                {
                    r.SetRef(ref val);
                    var preaddr = r.Address;
                    rrv.Address = preaddr;
                    if (RuntimeInfo.IsNativeAOT)
                    {
                        rrv.Address -= GetObjectHeaderSize();
                    }
                    ref var rv = ref rrv.GetRef<ConstructorToken>();
                    r.SetRef(ref val);
                    var postaddr = r.Address;
                    if (preaddr == postaddr)
                    {
                        return ref rv;
                    }
                }
            }

            public DelType Init<DelType>() where DelType : Delegate
            {
                if (typeof(DelType) != _ConDel.GetType())
                {
                    throw new ArgumentException("Must pass same DelType for Init<>() as Activator's ctor", nameof(DelType));
                }
                return (DelType)_ConDel;
            }

            public static ConstructorInfo Find(Type targetType, Type delType)
            {
                var miInvoke = delType.GetMethod("Invoke");
                var pars = miInvoke.GetParameters();
                var allConstructors = targetType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                // 1, strict
                foreach (var con in allConstructors)
                {
                    var conpars = con.GetParameters();
                    if (pars.Length != conpars.Length + 1)
                        continue;
                    bool nomatch = false;
                    for (int i = 1; i < pars.Length; ++i)
                    {
                        var par = pars[i].ParameterType;
                        var conpar = conpars[i - 1].ParameterType;
                        if (par != conpar)
                        {
                            nomatch = true;
                            break;
                        }
                    }
                    if (!nomatch)
                    {
                        return con;
                    }
                }
                // 2, public compatible
                foreach (var con in allConstructors)
                {
                    if (!con.IsPublic)
                        continue;
                    var conpars = con.GetParameters();
                    if (pars.Length != conpars.Length + 1)
                        continue;
                    bool nomatch = false;
                    for (int i = 1; i < pars.Length; ++i)
                    {
                        var par = pars[i].ParameterType;
                        var conpar = conpars[i - 1].ParameterType;
                        if (Size(par) != Size(conpar))
                        {
                            nomatch = true;
                            break;
                        }
                    }
                    if (!nomatch)
                    {
                        return con;
                    }
                }
                // 3, non-public compatible
                foreach (var con in allConstructors)
                {
                    if (con.IsPublic)
                        continue;
                    var conpars = con.GetParameters();
                    if (pars.Length != conpars.Length + 1)
                        continue;
                    bool nomatch = false;
                    for (int i = 1; i < pars.Length; ++i)
                    {
                        var par = pars[i].ParameterType;
                        var conpar = conpars[i - 1].ParameterType;
                        if (Size(par) != Size(conpar))
                        {
                            nomatch = true;
                            break;
                        }
                    }
                    if (!nomatch)
                    {
                        return con;
                    }
                }
                return null;
            }
        }
        public class Constructor<TTarget, TDelegate> where TDelegate : Delegate
        {
            private Constructor _Inner;
            public Constructor()
            {
                var con = Constructor.Find(typeof(TTarget), typeof(TDelegate));
                _Inner = new Constructor(con, typeof(TDelegate));
            }
            /// <summary>
            /// Sometimes, the target type is non public. But we can declare a compatible class/struct to pretend it. Mostly used for ValueTypes.
            /// </summary>
            public Constructor(Type realTargetType)
            {
                var con = Constructor.Find(realTargetType, typeof(TDelegate));
                _Inner = new Constructor(con, typeof(TDelegate));
            }

            public ref ConstructorToken AllocWrapped(out AllocatedObject<TTarget> obj)
            {
                return ref _Inner.AllocWrapped(out obj);
            }
            public ref ConstructorToken Alloc(out TTarget val)
            {
                return ref _Inner.Alloc(out val);
            }
            public TDelegate Init => (TDelegate)_Inner._ConDel;
        }
        /// <summary>
        /// Sometimes, the target type is non public. in this condition, use this.
        /// </summary>
        public class Constructor<TDelegate> where TDelegate : Delegate
        {
            private Constructor _Inner;
            public Constructor(Type targetType)
            {
                var con = Constructor.Find(targetType, typeof(TDelegate));
                _Inner = new Constructor(con, typeof(TDelegate));
            }

            public ref ConstructorToken Alloc(out object val)
            {
                return ref _Inner.Alloc(out val);
            }
            public TDelegate Init => (TDelegate)_Inner._ConDel;
        }
    }
    #endregion
}
