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
                object CreateDelegate(Type delType, object target, MethodBase method);
            }

            private abstract class UnsafeDelegateWrapperBase : IDelegateWrapper
            {
                public abstract object CreateDelegate(Type delType, MethodBase method);
                public abstract object CreateDelegate(Type delType, object target, MethodBase method);
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

                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

                public override object CreateDelegate(Type delType, MethodBase method)
                {
                    return CreateDelegate(delType, null, method);
                }
                public override object CreateDelegate(Type delType, object target, MethodBase method)
                {
                    try
                    {
                        RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    }
                    catch (ArgumentException)
                    { // very rare case: .NET framework Generic Delegate's ctor.
                        var gpars = method.ReflectedType.GetGenericArguments();
                        RuntimeTypeHandle[] gparhandles = new RuntimeTypeHandle[gpars.Length];
                        for (int i = 0; i < gpars.Length; ++i)
                        {
                            gparhandles[i] = gpars[i].TypeHandle;
                        }
                        RuntimeHelpers.PrepareMethod(method.MethodHandle, gparhandles);
                    }
                    var fn = method.MethodHandle.GetFunctionPointer();

                    var condeli = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcondel = condeli.MethodHandle;
                    if (RuntimeInfo.IsNetFramework && delType.GetGenericArguments() is Type[] congpars && congpars.Length > 0)
                    {
                        RuntimeTypeHandle[] gparhandles = new RuntimeTypeHandle[congpars.Length];
                        for (int i = 0; i < congpars.Length; ++i)
                        {
                            gparhandles[i] = congpars[i].TypeHandle;
                        }
                        RuntimeHelpers.PrepareMethod(hcondel, gparhandles);
                    }
                    else
                    {
                        RuntimeHelpers.PrepareMethod(hcondel);
                    }
                    var fncondel = hcondel.GetFunctionPointer();
                    var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                    delcon.GetField(fi_methodPtrAux) = fncondel;
                    delcon.GetField(fi_methodBase) = condeli;
                    delcon.GetField(fi_target) = delcon;

                    var rvdel = (Delegate)UnsafeEx.GetUninitializedObject(delType);
                    delcon(rvdel, target, fn);
                    rvdel.GetField(fi_methodPtrAux) = fn; // reset the methodPtrAux to avoid virtual searching.
                    return rvdel;
                }
            }

            private class UnsafeDelegateWrapperNativeAOT : UnsafeDelegateWrapperBase
            {
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
                }

                public override object CreateDelegate(Type delType, MethodBase method)
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
                public override object CreateDelegate(Type delType, object target, MethodBase method)
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();
                    var rvdel = _UnsafeCreateDelegateFunc(delType.TypeHandle.Value, fn, target, false, false);
                    return rvdel;
                }
            }

            private class UnsafeDelegateWrapperMono : UnsafeDelegateWrapperBase
            {
#pragma warning disable IDE1006 // 命名样式
                protected static FieldIndicator<IntPtr> FI_Delegate_method_ptr;
                protected static FieldIndicator<IntPtr> FI_Delegate_method;
                protected static FieldIndicator<IntPtr> FI_Delegate_invoke_impl;
                protected static FieldIndicator<object> FI_Delegate_method_info;
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
                        else if (field.Name == "method_info")
                        {
                            FI_Delegate_method_info = field.GetFieldOffset();
                        }
                    }

                    var coni = typeof(DelegateCtorDel).GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcon = coni.MethodHandle;
                    RuntimeHelpers.PrepareMethod(hcon);
                    _DelegateCtorFuncTemplate = (DelegateCtorDel)coni.Invoke(new[] { (object)null, (object)hcon.GetFunctionPointer() });
                }

                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

                public override object CreateDelegate(Type delType, MethodBase method)
                {
                    return CreateDelegate(delType, null, method);
                }
                public override object CreateDelegate(Type delType, object target, MethodBase method)
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();

                    var condeli = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcondel = condeli.MethodHandle;
                    RuntimeHelpers.PrepareMethod(hcondel);
                    var fncondel = hcondel.GetFunctionPointer();
                    var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                    delcon.GetField(FI_Delegate_method_ptr) = fncondel;

                    var rvdel = (Delegate)UnsafeEx.GetUninitializedObject(delType);
                    delcon(rvdel, target, IntPtr.Zero + 1); // use 1 as fn address to force it generate a static call
                    rvdel.GetField(FI_Delegate_method_ptr) = fn;
                    rvdel.GetField(FI_Delegate_method) = IntPtr.Zero;
                    rvdel.GetField(FI_Delegate_method_info) = null;
                    return rvdel;
                }
            }

            private class UnsafeDelegateWrapperIl2Cpp : UnsafeDelegateWrapperBase
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

                    public bool is_static
                    {
                        get => (flags & 0x0010) != 0;
                        set => _ = value ? flags |= 0x0010 : (flags &= unchecked((ushort)~0x0010));
                    }

                    public bool is_generic
                    {
                        get => (exflags & 1) != 0;
                        set => _ = value ? (exflags |= 1) : (exflags &= unchecked((byte)~1));
                    }
                    public bool is_inflated
                    {
                        get => (exflags & (1 << 1)) != 0;
                        set => _ = value ? (exflags |= (1 << 1)) : (exflags &= unchecked((byte)~(1 << 1)));
                    }
                    public bool wrapper_type
                    {
                        get => (exflags & (1 << 2)) != 0;
                        set => _ = value ? (exflags |= (1 << 2)) : (exflags &= unchecked((byte)~(1 << 2)));
                    }
                    public bool has_full_generic_sharing_signature
                    {
                        get => (exflags & (1 << 3)) != 0;
                        set => _ = value ? (exflags |= (1 << 3)) : (exflags &= unchecked((byte)~(1 << 3)));
                    }
                }
                [StructLayout(LayoutKind.Sequential)]
                public struct HybridClrMethodInfo
                {
                    public Il2CppMethodInfo il2CppInfo;

                    public bool initInterpCallMethodPointer
                    {
                        get => (il2CppInfo.exflags & (1 << 4)) != 0;
                        set => _ = value ? (il2CppInfo.exflags |= (1 << 4)) : (il2CppInfo.exflags &= unchecked((byte)~(1 << 4)));
                    }
                    public bool isInterpterImpl
                    {
                        get => (il2CppInfo.exflags & (1 << 5)) != 0;
                        set => _ = value ? (il2CppInfo.exflags |= (1 << 5)) : (il2CppInfo.exflags &= unchecked((byte)~(1 << 5)));
                    }

                    public IntPtr interpData;
                    public IntPtr methodPointerCallByInterp;
                    public IntPtr virtualMethodPointerCallByInterp;
                }
                [StructLayout(LayoutKind.Sequential)]
                public struct Il2CppObjectHeader
                {
                    public IntPtr _Class;
                    public IntPtr _Lock;
                }
                [StructLayout(LayoutKind.Sequential)]
                public struct Il2CppDelegate
                {
                    public Il2CppObjectHeader _Header;
                    public IntPtr method_ptr;
                    public IntPtr invoke_impl;
                    public IntPtr target;
                    public IntPtr method;
                    public IntPtr delegate_trampoline;
                    public IntPtr extraArg;
                    public IntPtr invoke_impl_this;

                    public IntPtr interp_method;
                    public IntPtr interp_invoke_impl;
                    public IntPtr method_info;
                    public IntPtr original_method_info;
                    public IntPtr data;

                    public bool method_is_virtual;
                }

                public override object CreateDelegate(Type delType, MethodBase method)
                {
                    var con = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    RawRef rconnmi = new RawRef() { Address = con.MethodHandle.Value };
                    if (RuntimeInfo.IsHybridCLREnabled)
                    { // hybrid clr
                        var nconmi = rconnmi.GetRef<HybridClrMethodInfo>();
                        if (nconmi.il2CppInfo.methodPointer == IntPtr.Zero || nconmi.isInterpterImpl)
                        {
                            return _CreateDelegateInternalFunc(delType, null, method, false);
                        }
                    }
                    //else
                    {
                        var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                        delcon.GetField(_FI_Delegate_method_ptr) = rconnmi.GetRef<Il2CppMethodInfo>().methodPointer;
                        delcon.GetField(_FI_Delegate_method) = rconnmi.Address;
                        delcon.GetField(_FI_Delegate_method_code) = delcon;

                        var rvdel = UnsafeEx.GetUninitializedObject(delType);
                        delcon(rvdel, null, method.MethodHandle.Value);

                        if (!method.IsStatic && method.ReflectedType.IsValueType)
                        {
                            ref Il2CppDelegate ndel = ref ConvertPretend<Il2CppDelegate>(rvdel);
                            ref var nmi = ref RawRef.Of(method.MethodHandle.Value).GetRef<Il2CppMethodInfo>();
                            ndel.method_ptr = nmi.methodPointer;
                        }

                        return rvdel;
                    }
                }
                public override object CreateDelegate(Type delType, object target, MethodBase method)
                {
                    var con = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    RawRef rconnmi = new RawRef() { Address = con.MethodHandle.Value };
                    if (RuntimeInfo.IsHybridCLREnabled)
                    { // hybrid clr
                        var nconmi = rconnmi.GetRef<HybridClrMethodInfo>();
                        if (nconmi.il2CppInfo.methodPointer == IntPtr.Zero || nconmi.isInterpterImpl)
                        {
                            var rvdel = _CreateDelegateInternalFunc(delType, null, method, false);
                            rvdel.GetField(_FI_Delegate_m_target) = target;
                            rvdel.GetField(_FI_Delegate_method_code) = target;
                            return rvdel;
                        }
                    }
                    //else
                    {
                        var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                        delcon.GetField(_FI_Delegate_method_ptr) = rconnmi.GetRef<Il2CppMethodInfo>().methodPointer;
                        delcon.GetField(_FI_Delegate_method) = rconnmi.Address;
                        delcon.GetField(_FI_Delegate_method_code) = delcon;

                        var rvdel = UnsafeEx.GetUninitializedObject(delType);
                        delcon(rvdel, target, method.MethodHandle.Value);
                        return rvdel;
                    }
                }
            }

            public static Delegate CreateDelegateUnsafe(Type delType, MethodBase method)
            {
                return (Delegate)_UnsafeWrapper.CreateDelegate(delType, method);
            }
            public static Delegate CreateDelegateUnsafe(Type delType, object target, MethodBase method)
            {
                return (Delegate)_UnsafeWrapper.CreateDelegate(delType, target, method);
            }

            public static bool IsInInterpter(MethodBase method)
            {
                switch (RuntimeInfo.Runtime)
                {
                    case RuntimeInfo.RuntimeCategory.NetCore:
                    case RuntimeInfo.RuntimeCategory.NetFramework:
                    case RuntimeInfo.RuntimeCategory.Mono:
                    case RuntimeInfo.RuntimeCategory.UnityMono:
                        return true;
                    case RuntimeInfo.RuntimeCategory.NativeAOT:
                        return false;
                    case RuntimeInfo.RuntimeCategory.UnityIl2Cpp:
                        {
                            if (!RuntimeInfo.IsHybridCLREnabled) return false;
                            return RawRef.Of(method.MethodHandle.Value).GetRef<UnsafeDelegateWrapperIl2Cpp.HybridClrMethodInfo>().isInterpterImpl;
                        }
                }
                return false;
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
        public static T CreateDelegateUnsafe<T>(this object target, MethodBase method) where T : Delegate
        {
            return (T)DelegateExImp.CreateDelegateUnsafe(typeof(T), target, method);
        }
        public static bool IsGenericParameterCompatiable(Type gtype, Type itype)
        {
            if (gtype.IsGenericParameter)
            {
                if ((gtype.GenericParameterAttributes & GenericParameterAttributes.NotNullableValueTypeConstraint) != 0 && !itype.IsValueType)
                {
                    return false;
                }
                if ((gtype.GenericParameterAttributes & GenericParameterAttributes.ReferenceTypeConstraint) != 0 && itype.IsValueType)
                {
                    return false;
                }
                if (gtype.GetGenericParameterConstraints() is Type[] gparents && gparents.Length > 0)
                {
                    foreach (var gparent in gparents)
                    {
                        if (!gparent.IsAssignableFrom(itype))
                        {
                            return false;
                        }
                    }
                }
                // TODO: recursively check generic parameter if gtype and itype themselves are generics.
                return true;
            }
            else if (gtype.ContainsGenericParameters)
            {
                if (gtype.HasElementType != itype.HasElementType) return false;
                if (gtype.HasElementType) return IsGenericParameterCompatiable(gtype.GetElementType(), itype.GetElementType());
                var gpars = gtype.GetGenericArguments();
                var ipars = itype.GetGenericArguments();
                if (gpars.Length != ipars.Length) return false;
                for (int i = 0; i < gpars.Length; ++i)
                {
                    if (!IsGenericParameterCompatiable(gpars[i], ipars[i])) return false;
                }
                return true;
            }
            else
            {
                return RawSize(gtype) == RawSize(itype);
            }
        }
        public static MethodBase FindOverload(this IEnumerable<MethodBase> methods, Type[] paramTypes, Type returnType = null, bool withThis = false, bool checkGeneric = false)
        {
            List<(MethodBase, Type[])> pendingMethods = new List<(MethodBase, Type[])>();

            // 1, strict
            foreach (var m in methods)
            {
            BeginStrictCheckMethod:
                var mpars = m.GetParameters();
                var mparlen = mpars.Length;
                bool isInstance = !m.IsStatic;
                if (isInstance && withThis) ++mparlen;
                if (paramTypes.Length != mparlen)
                    continue;
                if (returnType != null)
                {
                    Type mrtype;
                    bool mrvoid = false, rtvoid = false;
                    if (!(m is MethodInfo mi))
                    {
                        mrtype = typeof(void);
                    }
                    else
                    {
                        mrtype = mi.ReturnType;
                    }
                    mrvoid = mrtype == typeof(void) || mrtype == typeof(VoidReturn) || mrtype == null;
                    rtvoid = returnType == typeof(void) || returnType == typeof(VoidReturn);
                    if (mrvoid != rtvoid)
                    {
                        continue;
                    }
                    if (checkGeneric && (mrtype.IsGenericParameter || mrtype.ContainsGenericParameters))
                    {
                        if (!IsGenericParameterCompatiable(mrtype, returnType))
                        {
                            continue;
                        }
                    }
                    else
                    {
                        if (mrtype != returnType && !returnType.IsAssignableFrom(mrtype) && RawSize(returnType) != RawSize(mrtype))
                        {
                            continue;
                        }
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
                        mpar = m.ReflectedType;
                        if (mpar.IsValueType) mpar = mpar.MakeByRefType();
                        mparTypes[0] = mpar;
                    }
                    else
                    {
                        var index = i;
                        if (isInstance && withThis) --index;
                        mpar = mparTypes[i] = mpars[index].ParameterType;
                    }
                    if (checkGeneric && (mpar.IsGenericParameter || mpar.ContainsGenericParameters))
                    {
                        if (IsGenericParameterCompatiable(mpar, par))
                        {
                            mparTypes[i] = par;
                            continue;
                        }
                        else
                        {
                            goto BeginStrictCheckMethod;
                        }
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
                for (int i = 0; i < paramTypes.Length; ++i)
                {
                    var par = paramTypes[i];
                    var mpar = mpars[i];
                    if (RawSize(par) != RawSize(mpar))
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
                for (int i = 0; i < paramTypes.Length; ++i)
                {
                    var par = paramTypes[i];
                    var mpar = mpars[i];
                    if (RawSize(par) != RawSize(mpar))
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
        public static MethodInfo[] GetGenericMethods(this Type type, string name)
        {
            var allmethods = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);
            List<MethodInfo> foundMethods = new List<MethodInfo>();
            foreach (var m in allmethods)
            {
                if (m.Name == name && m.IsGenericMethod)
                {
                    foundMethods.Add(m);
                }
            }
            return foundMethods.ToArray();
        }
        public static MethodInfo[] GetGenericMethods(this IEnumerable<MethodBase> methods)
        {
            List<MethodInfo> foundMethods = new List<MethodInfo>();
            foreach (var m in methods)
            {
                if (m.IsGenericMethod)
                {
                    foundMethods.Add((MethodInfo)m);
                }
            }
            return foundMethods.ToArray();
        }
        public static MethodBase MakeGenericMethod(this MethodBase method, params Type[] types)
        {
            if (!(method is MethodInfo mi)) return method;
            if (!mi.ContainsGenericParameters) return method;
            return mi.MakeGenericMethod(types);
        }
        public static Type[] GuessGenericParameters(this MethodBase method, Type[] paramTypes, Type returnType = null, bool withThis = false)
        {
            if (!method.IsGenericMethod) return Array.Empty<Type>();
            var gpars = method.GetGenericArguments();
            Dictionary<Type, int> gparmap = new Dictionary<Type, int>();
            for (int i = 0; i < gpars.Length; ++i)
            {
                gparmap[gpars[i]] = i;
            }
            var guessed = new Type[gpars.Length];
            if (returnType != null)
            {
                GuessGenericParameters(gparmap, guessed, ((MethodInfo)method).ReturnType, returnType);
            }
            var mpars = method.GetParameters();
            bool isInstance = !method.IsStatic;
            for (int i = 0; i < paramTypes.Length; ++i)
            {
                var par = paramTypes[i];
                Type mpar;
                if (i == 0 && isInstance && withThis)
                {
                    mpar = method.ReflectedType;
                    if (mpar.IsGenericType) mpar = mpar.MakeByRefType();
                    GuessGenericParameters(gparmap, guessed, mpar, par);
                }
                else
                {
                    var index = i;
                    if (isInstance && withThis) --index;
                    GuessGenericParameters(gparmap, guessed, mpars[index].ParameterType, par);
                }
            }
            return guessed;
        }
        private static void GuessGenericParameters(Dictionary<Type, int> gparmap, Type[] guessed, Type gtype, Type itype)
        {
            if (gtype.IsGenericParameter)
            {
                guessed[gparmap[gtype]] = itype;
            }
            else if (gtype.ContainsGenericParameters)
            {
                if (gtype.HasElementType && itype.HasElementType)
                {
                    GuessGenericParameters(gparmap, guessed, gtype.GetElementType(), itype.GetElementType());
                }
                var gpars = gtype.GetGenericArguments();
                var ipars = itype.GetGenericArguments();
                for (int i = 0; i < gpars.Length && i < ipars.Length; ++i)
                {
                    GuessGenericParameters(gparmap, guessed, gpars[i], ipars[i]);
                }
            }
        }

        public static bool IsInInterpter(this MethodBase method)
        {
            return DelegateExImp.IsInInterpter(method);
        }
        #endregion
    }
}
