using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static class DelegateEx
    {
        #region UnsafeDelegateWrapper
        private static IDelegateWrapper _UnsafeWrapper;
        static DelegateEx()
        {
            if (RuntimeInfo.IsNativeAOT)
            {
                _UnsafeWrapper = new UnsafeDelegateWrapperNativeAOT();
            }
            else if (RuntimeInfo.IsMono)
            {
                _UnsafeWrapper = new UnsafeDelegateWrapperMono();
            }
            else if (RuntimeInfo.IsIl2Cpp)
            {
                _UnsafeWrapper = new UnsafeDelegateWrapperIl2Cpp();
            }
            else
            {
                _UnsafeWrapper = new UnsafeDelegateWrapperCore();
            }
        }
        public static Delegate CreateDelegateUnsafe(Type delType, MethodBase method)
        {
            return (Delegate)_UnsafeWrapper.CreateDelegate(delType, method);
        }
        public static T CreateDelegateUnsafe<T>(this MethodBase method) where T : Delegate
        {
            return (T)_UnsafeWrapper.CreateDelegate(typeof(T), method);
        }

        public interface IDelegateWrapper
        {
            object CreateDelegate(Type delType, MethodBase method);
        }

        public abstract class UnsafeDelegateWrapperBase : IDelegateWrapper
        {
            private static void DummyFunc0() { }
            private delegate void DummyDel0();
            private static void DummyFunc1(byte p1) { }
            private delegate void DummyDel1(byte p1);
            private static void DummyFunc2(byte p1, byte p2) { }
            private delegate void DummyDel2(byte p1, byte p2);
            private static void DummyFunc3(byte p1, byte p2, byte p3) { }
            private delegate void DummyDel3(byte p1, byte p2, byte p3);
            private static void DummyFunc4(byte p1, byte p2, byte p3, byte p4) { }
            private delegate void DummyDel4(byte p1, byte p2, byte p3, byte p4);
            private static void DummyFunc5(byte p1, byte p2, byte p3, byte p4, byte p5) { }
            private delegate void DummyDel5(byte p1, byte p2, byte p3, byte p4, byte p5);
            private static void DummyFunc6(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6) { }
            private delegate void DummyDel6(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6);
            private static void DummyFunc7(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7) { }
            private delegate void DummyDel7(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7);
            private static void DummyFunc8(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8) { }
            private delegate void DummyDel8(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8);
            private static void DummyFunc9(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9) { }
            private delegate void DummyDel9(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9);
            private static void DummyFunc10(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10) { }
            private delegate void DummyDel10(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10);
            private static void DummyFunc11(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11) { }
            private delegate void DummyDel11(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11);
            private static void DummyFunc12(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12) { }
            private delegate void DummyDel12(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12);
            private static void DummyFunc13(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13) { }
            private delegate void DummyDel13(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13);
            private static void DummyFunc14(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14) { }
            private delegate void DummyDel14(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14);
            private static void DummyFunc15(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15) { }
            private delegate void DummyDel15(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15);
            private static void DummyFunc16(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15, byte p16) { }
            private delegate void DummyDel16(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15, byte p16);

            protected static Delegate[] _DummyDelTemplates = new Delegate[]
            {
                (DummyDel0)DummyFunc0,
                (DummyDel1)DummyFunc1,
                (DummyDel2)DummyFunc2,
                (DummyDel3)DummyFunc3,
                (DummyDel4)DummyFunc4,
                (DummyDel5)DummyFunc5,
                (DummyDel6)DummyFunc6,
                (DummyDel7)DummyFunc7,
                (DummyDel8)DummyFunc8,
                (DummyDel9)DummyFunc9,
                (DummyDel10)DummyFunc10,
                (DummyDel11)DummyFunc11,
                (DummyDel12)DummyFunc12,
                (DummyDel13)DummyFunc13,
                (DummyDel14)DummyFunc14,
                (DummyDel15)DummyFunc15,
                (DummyDel16)DummyFunc16,
            };

            public abstract object CreateDelegate(Type delType, MethodBase method);

            private static Type _TypeDynamicMethod;
            private static ConstructorInfo _ConDynamicMethod;
            private static MethodInfo _MiGetILGenerator;
            private static Type _TypeILGenerator;
            private static MethodInfo _MiEmit1;
            private static MethodInfo _MiEmit2;

            private static ConstructorInfo _ExceptionCtor;

            static UnsafeDelegateWrapperBase()
            {
                _TypeDynamicMethod = typeof(System.Reflection.Emit.OpCode).Assembly.GetType("System.Reflection.Emit.DynamicMethod");
                _ConDynamicMethod = _TypeDynamicMethod.GetConstructor(new Type[] { typeof(string), typeof(Type), typeof(Type[]) });
                _MiGetILGenerator = _TypeDynamicMethod.GetMethod("GetILGenerator", Array.Empty<Type>());
                _TypeILGenerator = typeof(System.Reflection.Emit.OpCode).Assembly.GetType("System.Reflection.Emit.ILGenerator");
                _MiEmit1 = _TypeILGenerator.GetMethod("Emit", new[] { typeof(OpCode) });
                _MiEmit2 = _TypeILGenerator.GetMethod("Emit", new[] { typeof(OpCode), typeof(ConstructorInfo) });

                _ExceptionCtor = typeof(Exception).GetConstructor(Array.Empty<Type>());
            }
            protected Delegate CreateTemplateDelegateDynamic(Type delType)
            {
                List<Type> pars = new List<Type>();
                var imi = delType.GetMethod("Invoke");
                foreach (var par in imi.GetParameters())
                {
                    pars.Add(par.ParameterType);
                }
                var dm = _ConDynamicMethod.Invoke(new object[] { delType.Name + "_dyn", imi.ReturnType, pars.ToArray() });
                var il = _MiGetILGenerator.Invoke(dm, Array.Empty<Type>());
                _MiEmit2.Invoke(il, new object[] { OpCodes.Newobj, _ExceptionCtor });
                _MiEmit1.Invoke(il, new object[] { OpCodes.Throw });
                var mdm = (MethodInfo)dm;
                return mdm.CreateDelegate(delType);
            }
        }

        public class UnsafeDelegateWrapperCore : UnsafeDelegateWrapperBase
        {
            [StructLayout(LayoutKind.Sequential)]
            protected struct ObjectHeader
            {
                public IntPtr TypeHandle;
                public IntPtr SyncAddress;
            }

#pragma warning disable IDE1006 // 命名样式
            protected static FieldIndicator<IntPtr> fi_methodPtr;
            protected static FieldIndicator<IntPtr> fi_methodPtrAux;
            protected static FieldIndicator<object> fi_target;
            protected static FieldIndicator<object> fi_methodBase;
#pragma warning restore IDE1006 // 命名样式

            static UnsafeDelegateWrapperCore()
            {
                foreach (var field in typeof(Action).GetAllFields())
                {
                    if (field.Name == "_methodPtr")
                    {
                        fi_methodPtr = field.GetFieldOffset();
                    }
                    else if (field.Name == "_methodPtrAux")
                    {
                        fi_methodPtrAux = field.GetFieldOffset();
                    }
                    else if (field.Name == "_target")
                    {
                        fi_target = field.GetFieldOffset();
                    }
                    else if (field.Name == "_methodBase")
                    {
                        fi_methodBase = field.GetFieldOffset();
                    }
                }
            }

            Dictionary<int, Delegate> _DynamicTemplates = new Dictionary<int, Delegate>();

            public override object CreateDelegate(Type delType, MethodBase method)
            {
                var delParamCount = method.GetParameters().Length;
                if (!method.IsStatic) ++delParamCount;
                if (delParamCount <= _DummyDelTemplates.Length)
                {
                    Delegate template = _DummyDelTemplates[delParamCount];
                    object rvdel = template.Clone();
                    ref var oheader = ref rvdel.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
                    oheader.TypeHandle = delType.TypeHandle.Value;

                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();
                    rvdel.GetField(fi_methodPtrAux) = fn;
                    rvdel.GetField(fi_methodBase) = method;
                    rvdel.GetField(fi_target) = rvdel;
                    return rvdel;
                }
                else
                {
                    if (RuntimeInfo.IsDynamicCodeEnabled)
                    {
                        Delegate template = null;
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates.TryGetValue(delParamCount, out template);
                        }
                        if (template == null)
                        {
                            template = CreateTemplateDelegateDynamic(delType);
                            lock (_DynamicTemplates)
                            {
                                _DynamicTemplates[delParamCount] = template;
                            }
                        }
                        var rvdel = template.Clone();
                        ref var oheader = ref rvdel.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
                        oheader.TypeHandle = delType.TypeHandle.Value;

                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();
                        rvdel.GetField(fi_methodPtrAux) = fn;
                        rvdel.GetField(fi_methodBase) = method;
                        rvdel.GetField(fi_target) = rvdel;
                        return rvdel;
                    }
                    else
                    {
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();
                        return Marshal.GetDelegateForFunctionPointer(fn, delType); // Notice: Dangerous
                    }
                }
            }
        }

        public class UnsafeDelegateWrapperNativeAOT : IDelegateWrapper
        {
            //[StructLayout(LayoutKind.Sequential)]
            //private struct DelegateFields
            //{
            //    public IntPtr _firstParameter;
            //    public IntPtr _helperObject;
            //    public IntPtr _extraFunctionPointerOrData;
            //    public IntPtr _functionPointer;
            //}
            //protected static FieldIndicator<object> fi_firstParameter;
            //protected static FieldIndicator<object> fi_helperObject;
            //protected static FieldIndicator<IntPtr> fi_extraFunctionPointerOrData;
            //protected static FieldIndicator<IntPtr> fi_functionPointer;

            //[StructLayout(LayoutKind.Sequential)]
            //protected struct ObjectHeader
            //{
            //    public IntPtr TypeHandle;
            //}

            private delegate Delegate CreateDelegateWithoutSignatureValidationDel(MethodInfo thiz, Type delType, object target, bool isStatic, bool isOpen);
            private static CreateDelegateWithoutSignatureValidationDel _CreateDelegateWithoutSignatureValidationFunc;
            private delegate Delegate UnsafeCreateDelegateDel(IntPtr typeHandle, IntPtr realfn, object target, bool isStatic, bool isOpen);
            private static UnsafeCreateDelegateDel _UnsafeCreateDelegateFunc;

            static UnsafeDelegateWrapperNativeAOT()
            {
                var cmi = typeof(object).GetMethod("ToString").GetType().GetMethod("CreateDelegateWithoutSignatureValidation", BindingFlags.Instance | BindingFlags.NonPublic);
                _CreateDelegateWithoutSignatureValidationFunc = (CreateDelegateWithoutSignatureValidationDel)cmi.Invoke(cmi, new object[] { typeof(CreateDelegateWithoutSignatureValidationDel), null, false, true });
                MethodInfo cdmi = null;
                foreach (var mi in typeof(Delegate).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                {
                    var pars = mi.GetParameters();
                    if (pars.Length == 5 && pars[0].ParameterType.IsPointer)
                    {
                        cdmi = mi;
                    }
                }
                _UnsafeCreateDelegateFunc = (UnsafeCreateDelegateDel)_CreateDelegateWithoutSignatureValidationFunc(cdmi, typeof(UnsafeCreateDelegateDel), null, true, true);

                //DelegateFields dummy = default;
                //fi_firstParameter = dummy.GetFieldOffset(ref dummy._firstParameter);
                //fi_helperObject = dummy.GetFieldOffset(ref dummy._helperObject);
                //fi_extraFunctionPointerOrData = dummy.GetFieldOffset(ref dummy._extraFunctionPointerOrData);
                //fi_functionPointer = dummy.GetFieldOffset(ref dummy._functionPointer);
            }

            public object CreateDelegate(Type delType, MethodBase method)
            {
                if (method is MethodInfo mi)
                {
                    if (method.IsStatic)
                    {
                        var rvdel = _CreateDelegateWithoutSignatureValidationFunc(mi, delType, null, true, true);
                        return rvdel;
                    }
                    else
                    {
                        if (method.IsVirtual)
                        {
                            RuntimeHelpers.PrepareMethod(method.MethodHandle);
                            var fn = method.MethodHandle.GetFunctionPointer();
                            var rvdel = _UnsafeCreateDelegateFunc(delType.TypeHandle.Value, fn, null, true, true);
                            return rvdel;
                        }
                        else
                        {
                            var rvdel = _CreateDelegateWithoutSignatureValidationFunc(mi, delType, null, false, true);
                            return rvdel;
                        }
                    }
                }
                else
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();
                    var rvdel = _UnsafeCreateDelegateFunc(delType.TypeHandle.Value, fn, null, true, true);
                    return rvdel;
                }
            }

            //private delegate object CreateObjectDelegate(IntPtr typeHandle);
            //private static object CreateUninitializedDelegate(Type delType)
            //{
            //    var trl = Type.GetType("System.Runtime.RuntimeImports, System.Private.CoreLib");
            //    var minew = trl.GetMethod("RhNewObject", BindingFlags.Static | BindingFlags.NonPublic);
            //    RuntimeHelpers.PrepareMethod(minew.MethodHandle);
            //    var fnnew = minew.MethodHandle.GetFunctionPointer();
            //    //var _RhNewObject_del = (CreateObjectDelegate)Marshal.GetDelegateForFunctionPointer(fnnew, typeof(CreateObjectDelegate));
            //    ////_RhNewObject_del = (Func<IntPtr, object>)Delegate.CreateDelegate(typeof(Func<IntPtr, object>), minew);
            //    //return _RhNewObject_del(delType.TypeHandle.Value);
            //    return minew.Invoke(null, new object[] { delType.TypeHandle.Value });
            //}
        }

        public class UnsafeDelegateWrapperMono : UnsafeDelegateWrapperBase
        {
#pragma warning disable IDE1006 // 命名样式
            protected static FieldIndicator<IntPtr> FI_Delegate_method_ptr;
            protected static FieldIndicator<IntPtr> FI_Delegate_method;
            protected static FieldIndicator<IntPtr> FI_Delegate_invoke_impl;
#pragma warning restore IDE1006 // 命名样式

            static UnsafeDelegateWrapperMono()
            {
                foreach (var field in typeof(Action).GetAllFields())
                {
                    if (field.Name == "method")
                    {
                        FI_Delegate_method = field.GetFieldOffset();
                    }
                    else if (field.Name == "method_ptr")
                    {
                        FI_Delegate_method_ptr = field.GetFieldOffset();
                    }
                    else if (field.Name == "invoke_impl")
                    {
                        FI_Delegate_invoke_impl = field.GetFieldOffset();
                    }
                }

                var comi = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject");
                _CreateUninitializedObjectDelFunc = (CreateUninitializedObjectDel)Delegate.CreateDelegate(typeof(CreateUninitializedObjectDel), comi);
            }

            private static Dictionary<int, Delegate> _DynamicTemplates = new Dictionary<int, Delegate>();

            public override object CreateDelegate(Type delType, MethodBase method)
            {
                var delParamCount = method.GetParameters().Length;
                if (!method.IsStatic) ++delParamCount;
                if (delParamCount <= _DummyDelTemplates.Length)
                {
                    Delegate template = _DummyDelTemplates[delParamCount];
                    object rvdel = CreateUninitializedObject(delType);
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();
                    rvdel.GetField(FI_Delegate_method_ptr) = fn;
                    rvdel.GetField(FI_Delegate_invoke_impl) = template.GetField(FI_Delegate_invoke_impl);
                    return rvdel;
                }
                else
                {
                    Delegate template = null;
                    lock (_DynamicTemplates)
                    {
                        _DynamicTemplates.TryGetValue(delParamCount, out template);
                    }
                    if (template == null)
                    {
                        template = CreateTemplateDelegateDynamic(delType);
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates[delParamCount] = template;
                        }
                    }
                    object rvdel = CreateUninitializedObject(delType);
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();
                    rvdel.GetField(FI_Delegate_method_ptr) = fn;
                    rvdel.GetField(FI_Delegate_invoke_impl) = template.GetField(FI_Delegate_invoke_impl);
                    return rvdel;
                }
            }

            private delegate object CreateUninitializedObjectDel(Type type);
            private static CreateUninitializedObjectDel _CreateUninitializedObjectDelFunc;
            private static object CreateUninitializedObject(Type type)
            {
                return _CreateUninitializedObjectDelFunc(type);
            }
        }

        public class UnsafeDelegateWrapperIl2Cpp : IDelegateWrapper
        {
#pragma warning disable IDE1006 // 命名样式
            protected static FieldIndicator<IntPtr> FI_Delegate_method_ptr;
            protected static FieldIndicator<IntPtr> FI_Delegate_method;
            protected static FieldIndicator<IntPtr> FI_Delegate_invoke_impl;
            protected static FieldIndicator<object> FI_Delegate_method_code;
            protected static FieldIndicator<object> FI_Delegate_m_target;
#pragma warning restore IDE1006 // 命名样式

            private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
            private static DelegateCtorDel _DelegateCtorFuncTemplate;
            private static MethodInfo _MiGetUninitializedObject;

            static UnsafeDelegateWrapperIl2Cpp()
            {
                foreach (var field in typeof(Action).GetAllFields())
                {
                    if (field.Name == "method")
                    {
                        FI_Delegate_method = field.GetFieldOffset();
                    }
                    else if (field.Name == "method_ptr")
                    {
                        FI_Delegate_method_ptr = field.GetFieldOffset();
                    }
                    else if (field.Name == "invoke_impl")
                    {
                        FI_Delegate_invoke_impl = field.GetFieldOffset();
                    }
                    else if (field.Name == "method_code")
                    {
                        FI_Delegate_method_code = field.GetFieldOffset();
                    }
                    else if (field.Name == "m_target")
                    {
                        FI_Delegate_m_target = field.GetFieldOffset();
                    }
                }

                var comi = typeof(object).Assembly.GetType("System.Runtime.Remoting.Activation.ActivationServices").GetMethod("AllocateUninitializedClassInstance");
                //var comi = typeof(RuntimeHelpers).GetMethod("GetUninitializedObject");
                _MiGetUninitializedObject = comi;
                _CreateUninitializedObjectDelFunc = (CreateUninitializedObjectDel)Delegate.CreateDelegate(typeof(CreateUninitializedObjectDel), comi);

                var coni = typeof(DelegateCtorDel).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                _DelegateCtorFuncTemplate = (DelegateCtorDel)coni.Invoke(new[] { (object)null, (object)coni.MethodHandle.Value });
            }

            [StructLayout(LayoutKind.Sequential)]
            public struct Il2CppMethodInfo
            {
                public IntPtr methodPointer;
                public IntPtr virtualMethodPointer;
                public IntPtr invoker_method;
            }

            public object CreateDelegate(Type delType, MethodBase method)
            {
                var con = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                RawRef rconnmi = new RawRef() { Address = con.MethodHandle.Value };
                var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                delcon.GetField(FI_Delegate_method_ptr) = rconnmi.GetRef<Il2CppMethodInfo>().methodPointer;
                delcon.GetField(FI_Delegate_method) = rconnmi.Address;
                delcon.GetField(FI_Delegate_method_code) = delcon;
                var rvdel = CreateUninitializedObject(delType);
                if (method.IsConstructor)
                {
                    delcon(rvdel, null, _MiGetUninitializedObject.MethodHandle.Value);
                    RawRef rmi = new RawRef() { Address = method.MethodHandle.Value };
                    rvdel.GetField(FI_Delegate_method_ptr) = rmi.GetRef<Il2CppMethodInfo>().methodPointer;
                    //rvdel.GetField(FI_Delegate_method) = rmi.Address;
                    rvdel.GetField(FI_Delegate_method_code) = rvdel;
                    return rvdel;
                }
                else
                {
                    delcon(rvdel, null, method.MethodHandle.Value);
                }
                return rvdel;
            }

            private delegate object CreateUninitializedObjectDel(Type type);
            private static CreateUninitializedObjectDel _CreateUninitializedObjectDelFunc;
            private static object CreateUninitializedObject(Type type)
            {
                
                return _CreateUninitializedObjectDelFunc(type);
            }
        }
        #endregion
    }

    public interface IUnsafeDelegate
    {
        void Bind(MethodInfo method);
        MethodInfo Method { get; }
    }
    public class UnsafeAction : IUnsafeDelegate
    {
        public MethodInfo Method => throw new NotImplementedException();

        public void Bind(MethodInfo method)
        {
            throw new NotImplementedException();
        }
    }
}
