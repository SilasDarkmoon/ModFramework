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

                private delegate void DelegateCtorDel(object delObj, object target, IntPtr methodHandle);
                private static DelegateCtorDel _DelegateCtorFuncTemplate;

                public override object CreateDelegate(Type delType, MethodBase method)
                {
                    RuntimeHelpers.PrepareMethod(method.MethodHandle);
                    var fn = method.MethodHandle.GetFunctionPointer();

                    var condeli = delType.GetConstructor(new[] { typeof(object), typeof(IntPtr) });
                    var hcondel = condeli.MethodHandle;
                    RuntimeHelpers.PrepareMethod(hcondel);
                    var fncondel = hcondel.GetFunctionPointer();
                    var delcon = (DelegateCtorDel)_DelegateCtorFuncTemplate.Clone();
                    delcon.GetField(fi_methodPtrAux) = fncondel;
                    delcon.GetField(fi_methodBase) = condeli;
                    delcon.GetField(fi_target) = delcon;

                    var rvdel = (Delegate)UnsafeEx.GetUninitializedObject(delType);
                    delcon(rvdel, null, fn);
                    rvdel.GetField(fi_methodPtrAux) = fn; // reset the methodPtrAux to avoid virtual searching.
                    return rvdel;
                }
            }

            private class UnsafeDelegateWrapperNativeAOT : IDelegateWrapper
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
                        delcon(rvdel, null, IntPtr.Zero + 1); // use 1 as fn address to force it generate a static call
                        rvdel.GetField(FI_Delegate_method_ptr) = fn;
                        rvdel.GetField(FI_Delegate_method) = IntPtr.Zero;
                        rvdel.GetField(FI_Delegate_method_info) = null;
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
                    if (mrvoid ^ rtvoid)
                    {
                        continue;
                    }
                    if (mrtype != returnType && !returnType.IsAssignableFrom(mrtype) && RawSize(returnType) != RawSize(mrtype))
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
                for (int i = 1; i < paramTypes.Length; ++i)
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
        #endregion
    }
}
