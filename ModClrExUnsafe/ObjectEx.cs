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
        public static object GetUninitializedObject(this Type t)
        {
            return ObjectCreatorImp.GetUninitializedObject(t);
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
    }
}
