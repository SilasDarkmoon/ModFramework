using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        #region UnsafeDelegateWrapper
        private static class DelegateExImp
        {
            private static IDelegateWrapper _UnsafeWrapper;

            static DelegateExImp()
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

            private interface IDelegateWrapper
            {
                object CreateDelegate(Type delType, MethodBase method);
            }

            private abstract class UnsafeDelegateWrapperBase : IDelegateWrapper
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
            }

            private class UnsafeDelegateWrapperCore : UnsafeDelegateWrapperBase
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

                    var coni = typeof(DelegateCtorDel).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcon = coni.MethodHandle;
                    RuntimeHelpers.PrepareMethod(hcon);
                    _DelegateCtorFuncTemplate = (DelegateCtorDel)coni.Invoke(new[] { (object)null, (object)hcon.GetFunctionPointer() });
                }

                protected static Dictionary<int, Delegate> _DynamicTemplates = new Dictionary<int, Delegate>();
                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

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
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();

                        Delegate template = null;
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates.TryGetValue(delParamCount, out template);
                        }
                        object rvdel;
                        if (template != null)
                        {
                            rvdel = template.Clone();
                            ref var oheader = ref rvdel.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
                            oheader.TypeHandle = delType.TypeHandle.Value;

                            rvdel.GetField(fi_methodPtrAux) = fn;
                            rvdel.GetField(fi_methodBase) = method;
                            rvdel.GetField(fi_target) = rvdel;
                            return rvdel;
                        }

                        var condeli = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                        var hcondel = condeli.MethodHandle;
                        RuntimeHelpers.PrepareMethod(hcondel);
                        var fncondel = hcondel.GetFunctionPointer();
                        var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                        delcon.GetField(fi_methodPtrAux) = fncondel;
                        delcon.GetField(fi_methodBase) = condeli;
                        delcon.GetField(fi_target) = delcon;

                        template = (Delegate)UnsafeEx.GetUninitializedObject(delType);
                        delcon(template, null, fn);
                        template.GetField(fi_methodPtrAux) = fn; // reset the methodPtrAux to avoid virtual searching.
                        rvdel = template.Clone();
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates[delParamCount] = template;
                        }
                        return rvdel;
                    }
                }
            }

            private class UnsafeDelegateWrapperNativeAOT : IDelegateWrapper
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
                    MethodInfo cdmi = null;
                    foreach (var mi in typeof(Delegate).GetMethods(BindingFlags.Static | BindingFlags.NonPublic))
                    {
                        var pars = mi.GetParameters();
                        if (pars.Length == 5 && pars[0].ParameterType.IsPointer)
                        {
                            if (mi.Name == "CreateDelegate")
                            {
                                cdmi = mi;
                            }
                        }
                    }
                    var cmi = cdmi.GetType().GetMethod("CreateDelegateWithoutSignatureValidation", BindingFlags.Instance | BindingFlags.NonPublic);
                    _CreateDelegateWithoutSignatureValidationFunc = (CreateDelegateWithoutSignatureValidationDel)cmi.Invoke(cmi, new object[] { typeof(CreateDelegateWithoutSignatureValidationDel), null, false, true });
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
                    else // Constructor
                    {
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();
                        var rvdel = _UnsafeCreateDelegateFunc(delType.TypeHandle.Value, fn, null, true, true);
                        return rvdel;
                    }
                }
            }

            private class UnsafeDelegateWrapperMono : UnsafeDelegateWrapperBase
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

                    var coni = typeof(DelegateCtorDel).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcon = coni.MethodHandle;
                    RuntimeHelpers.PrepareMethod(hcon);
                    _DelegateCtorFuncTemplate = (DelegateCtorDel)coni.Invoke(new[] { (object)null, (object)hcon.GetFunctionPointer() });
                }

                private static Dictionary<int, Delegate> _DynamicTemplates = new Dictionary<int, Delegate>();
                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

                public override object CreateDelegate(Type delType, MethodBase method)
                {
                    var delParamCount = method.GetParameters().Length;
                    if (!method.IsStatic) ++delParamCount;
                    if (delParamCount <= _DummyDelTemplates.Length)
                    {
                        Delegate template = _DummyDelTemplates[delParamCount];
                        object rvdel = UnsafeEx.GetUninitializedObject(delType);
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();
                        rvdel.GetField(FI_Delegate_method_ptr) = fn;
                        rvdel.GetField(FI_Delegate_invoke_impl) = template.GetField(FI_Delegate_invoke_impl);
                        return rvdel;
                    }
                    else
                    {
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                        var fn = method.MethodHandle.GetFunctionPointer();

                        Delegate template = null;
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates.TryGetValue(delParamCount, out template);
                        }
                        object rvdel;
                        if (template != null)
                        {
                            rvdel = UnsafeEx.GetUninitializedObject(delType);
                            rvdel.GetField(FI_Delegate_method_ptr) = fn;
                            rvdel.GetField(FI_Delegate_invoke_impl) = template.GetField(FI_Delegate_invoke_impl);
                            return rvdel;
                        }

                        var condeli = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                        var hcondel = condeli.MethodHandle;
                        RuntimeHelpers.PrepareMethod(hcondel);
                        var fncondel = hcondel.GetFunctionPointer();
                        var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                        delcon.GetField(FI_Delegate_method_ptr) = fncondel;

                        template = (Delegate)UnsafeEx.GetUninitializedObject(delType);
                        delcon(template, null, fn);
                        template.GetField(FI_Delegate_method_ptr) = fn;
                        template.GetField(FI_Delegate_method) = IntPtr.Zero;
                        rvdel = template.Clone();
                        lock (_DynamicTemplates)
                        {
                            _DynamicTemplates[delParamCount] = template;
                        }
                        return rvdel;
                    }
                }
            }

            private class UnsafeDelegateWrapperIl2Cpp : IDelegateWrapper
            {
#pragma warning disable IDE1006 // 命名样式
                protected static FieldIndicator<IntPtr> _FI_Delegate_method_ptr;
                protected static FieldIndicator<IntPtr> _FI_Delegate_method;
                protected static FieldIndicator<IntPtr> _FI_Delegate_invoke_impl;
                protected static FieldIndicator<object> _FI_Delegate_method_code;
                protected static FieldIndicator<object> _FI_Delegate_m_target;
#pragma warning restore IDE1006 // 命名样式

                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

                private static MethodInfo _MiDummyStatic;

                private delegate Delegate CreateDelegateInternalDel(Type type, object target, MethodBase method, bool throwOnError);
                private static CreateDelegateInternalDel _CreateDelegateInternalFunc;

                static UnsafeDelegateWrapperIl2Cpp()
                {
                    foreach (var field in typeof(Action).GetAllFields())
                    {
                        if (field.Name == "method")
                        {
                            _FI_Delegate_method = field.GetFieldOffset();
                        }
                        else if (field.Name == "method_ptr")
                        {
                            _FI_Delegate_method_ptr = field.GetFieldOffset();
                        }
                        else if (field.Name == "invoke_impl")
                        {
                            _FI_Delegate_invoke_impl = field.GetFieldOffset();
                        }
                        else if (field.Name == "method_code")
                        {
                            _FI_Delegate_method_code = field.GetFieldOffset();
                        }
                        else if (field.Name == "m_target")
                        {
                            _FI_Delegate_m_target = field.GetFieldOffset();
                        }
                    }

                    var coni = typeof(DelegateCtorDel).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    _DelegateCtorFuncTemplate = (DelegateCtorDel)coni.Invoke(new[] { (object)null, (object)coni.MethodHandle.Value });

                    Action actDummy = DummyStatic;
                    _MiDummyStatic = actDummy.Method;

                    var micdi = typeof(Delegate).GetMethod("CreateDelegate_internal", BindingFlags.Static | BindingFlags.NonPublic);
                    _CreateDelegateInternalFunc = (CreateDelegateInternalDel)micdi.Invoke(null, new object[] { typeof(CreateDelegateInternalDel), null, micdi, false });
                }

                [StructLayout(LayoutKind.Sequential)]
                public struct Il2CppMethodInfo
                {
                    public IntPtr methodPointer;
                    public IntPtr virtualMethodPointer;
                    public IntPtr invoker_method;

                    public IntPtr name;
                    public IntPtr klass;
                    public IntPtr return_type;
                    public IntPtr parameters;

                    public IntPtr umethodMetadataHandle;
                    public IntPtr ugenericContainerHandle;

                    public uint token;
                    public ushort flags;
                    public ushort iflags;
                    public ushort slot;
                    public byte parameters_count;
                    public byte exflags;
                }

                public object CreateDelegate(Type delType, MethodBase method)
                {
                    var con = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    RawRef rconnmi = new RawRef() { Address = con.MethodHandle.Value };
                    if (RuntimeInfo.IsHybridCLREnabled)
                    { // hybrid clr
                        return _CreateDelegateInternalFunc(delType, null, method, false);
                    }
                    else
                    {
                        var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                        delcon.GetField(_FI_Delegate_method_ptr) = rconnmi.GetRef<Il2CppMethodInfo>().methodPointer;
                        delcon.GetField(_FI_Delegate_method) = rconnmi.Address;
                        delcon.GetField(_FI_Delegate_method_code) = delcon;
                        var rvdel = UnsafeEx.GetUninitializedObject(delType);
                        if (method.IsConstructor)
                        {
                            RawRef rmid = new RawRef() { Address = _MiDummyStatic.MethodHandle.Value };
                            Il2CppMethodInfo cloneddesc = rmid.GetRef<Il2CppMethodInfo>();
                            rmid.SetRef(ref cloneddesc);

                            RawRef rmi = new RawRef() { Address = method.MethodHandle.Value };
                            cloneddesc.parameters_count = (byte)(rmi.GetRef<Il2CppMethodInfo>().parameters_count + 1);

                            delcon(rvdel, null, rmid.Address);

                            rvdel.GetField(_FI_Delegate_method_ptr) = rmi.GetRef<Il2CppMethodInfo>().methodPointer;
                            rvdel.GetField(_FI_Delegate_method) = rmi.Address;
                            rvdel.GetField(_FI_Delegate_method_code) = rvdel;
                            return rvdel;
                        }
                        else
                        {
                            delcon(rvdel, null, method.MethodHandle.Value);
                        }
                        return rvdel;
                    }
                }

                private static void DummyStatic() { }
            }

            public static Delegate CreateDelegateUnsafe(Type delType, MethodBase method)
            {
                return (Delegate)_UnsafeWrapper.CreateDelegate(delType, method);
            }
        }
        public static Delegate CreateDelegateUnsafe(Type delType, MethodBase method)
        {
            return DelegateExImp.CreateDelegateUnsafe(delType, method);
        }
        public static T CreateDelegateUnsafe<T>(this MethodBase method) where T : Delegate
        {
            return (T)DelegateExImp.CreateDelegateUnsafe(typeof(T), method);
        }
        public static MethodBase FindMethod(this IEnumerable<MethodBase> methods, Type[] paramTypes, Type returnType = null, bool withThis = false)
        {
            List<(MethodBase, Type[])> pendingMethods = new List<(MethodBase, Type[])>();

            // 1, strict
            foreach (var m in methods)
            {
                var mpars = m.GetParameters();
                var mparlen = mpars.Length;
                bool isInstance = !m.IsStatic;
                if (isInstance && withThis) ++mparlen;
                if (paramTypes.Length != mparlen)
                    continue;
                if (returnType != null)
                {
                    Type mrtype;
                    if (!(m is MethodInfo mi))
                    {
                        mrtype = typeof(void);
                    }
                    else
                    {
                        mrtype = mi.ReturnType;
                    }
                    if (mrtype != returnType && !returnType.IsAssignableFrom(mrtype))
                    {
                        continue;
                    }
                }
                var mparTypes = new Type[mparlen];
                bool nomatch = false;
                for (int i = 0; i < paramTypes.Length; ++i)
                {
                    var par = paramTypes[i];
                    Type mpar;
                    if (i == 0 && isInstance && withThis)
                    {
                        mpar = m.DeclaringType;
                        mparTypes[0] = mpar;
                    }
                    else
                    {
                        var index = i;
                        if (isInstance && withThis) --index;
                        mpar = mparTypes[index] = mpars[index].ParameterType;
                    }
                    if (!mpar.IsAssignableFrom(par))
                    {
                        nomatch = true;
                    }
                }
                if (!nomatch)
                {
                    return m;
                }
                pendingMethods.Add((m, mparTypes));
            }
            // 2, public compatible
            foreach (var (m, mpars) in pendingMethods)
            {
                if (!m.IsPublic)
                    continue;
                bool nomatch = false;
                for (int i = 1; i < paramTypes.Length; ++i)
                {
                    var par = paramTypes[i];
                    var mpar = mpars[i];
                    if (Size(par) != Size(mpar))
                    {
                        nomatch = true;
                        break;
                    }
                }
                if (!nomatch)
                {
                    return m;
                }
            }
            // 3, non-public compatible
            foreach (var (m, mpars) in pendingMethods)
            {
                if (m.IsPublic)
                    continue;
                bool nomatch = false;
                for (int i = 1; i < paramTypes.Length; ++i)
                {
                    var par = paramTypes[i];
                    var mpar = mpars[i];
                    if (Size(par) != Size(mpar))
                    {
                        nomatch = true;
                        break;
                    }
                }
                if (!nomatch)
                {
                    return m;
                }
            }
            return null;
        }
        public static MethodInfo[] GetMethods(this Type type, string name)
        {
            var allmethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            List<MethodInfo> foundMethods = new List<MethodInfo>();
            foreach (var m in allmethods)
            {
                if (m.Name == name)
                {
                    foundMethods.Add(m);
                }
            }
            return foundMethods.ToArray();
        }
        #endregion
    }
}
