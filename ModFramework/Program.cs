using Mod;
using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

//public interface IDelegateWrapper
//{
//    object CreateDelegateUnsafe(Type delType, MethodInfo method);
//}

//public abstract class UnsafeDelegateWrapperBase : IDelegateWrapper
//{
//    private static void DummyFunc0() { }
//    private delegate void DummyDel0();
//    private static void DummyFunc1(byte p1) { }
//    private delegate void DummyDel1(byte p1);
//    private static void DummyFunc2(byte p1, byte p2) { }
//    private delegate void DummyDel2(byte p1, byte p2);
//    private static void DummyFunc3(byte p1, byte p2, byte p3) { }
//    private delegate void DummyDel3(byte p1, byte p2, byte p3);
//    private static void DummyFunc4(byte p1, byte p2, byte p3, byte p4) { }
//    private delegate void DummyDel4(byte p1, byte p2, byte p3, byte p4);
//    private static void DummyFunc5(byte p1, byte p2, byte p3, byte p4, byte p5) { }
//    private delegate void DummyDel5(byte p1, byte p2, byte p3, byte p4, byte p5);
//    private static void DummyFunc6(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6) { }
//    private delegate void DummyDel6(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6);
//    private static void DummyFunc7(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7) { }
//    private delegate void DummyDel7(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7);
//    private static void DummyFunc8(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8) { }
//    private delegate void DummyDel8(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8);
//    private static void DummyFunc9(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9) { }
//    private delegate void DummyDel9(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9);
//    private static void DummyFunc10(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10) { }
//    private delegate void DummyDel10(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10);
//    private static void DummyFunc11(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11) { }
//    private delegate void DummyDel11(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11);
//    private static void DummyFunc12(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12) { }
//    private delegate void DummyDel12(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12);
//    private static void DummyFunc13(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13) { }
//    private delegate void DummyDel13(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13);
//    private static void DummyFunc14(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14) { }
//    private delegate void DummyDel14(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14);
//    private static void DummyFunc15(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15) { }
//    private delegate void DummyDel15(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15);
//    private static void DummyFunc16(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15, byte p16) { }
//    private delegate void DummyDel16(byte p1, byte p2, byte p3, byte p4, byte p5, byte p6, byte p7, byte p8, byte p9, byte p10, byte p11, byte p12, byte p13, byte p14, byte p15, byte p16);

//    private static object[] _DummyDelTemplates = new[]
//    {
//        (object)(DummyDel0)DummyFunc0,
//        (object)(DummyDel1)DummyFunc1,
//        (object)(DummyDel2)DummyFunc2,
//        (object)(DummyDel3)DummyFunc3,
//        (object)(DummyDel4)DummyFunc4,
//        (object)(DummyDel5)DummyFunc5,
//        (object)(DummyDel6)DummyFunc6,
//        (object)(DummyDel7)DummyFunc7,
//        (object)(DummyDel8)DummyFunc8,
//        (object)(DummyDel9)DummyFunc9,
//        (object)(DummyDel10)DummyFunc10,
//        (object)(DummyDel11)DummyFunc11,
//        (object)(DummyDel12)DummyFunc12,
//        (object)(DummyDel13)DummyFunc13,
//        (object)(DummyDel14)DummyFunc14,
//        (object)(DummyDel15)DummyFunc15,
//        (object)(DummyDel16)DummyFunc16,
//    };

//    private static Func<object>[] _DummyCreators = new[]
//    {
//        (Func<object>)(() => (DummyDel0)DummyFunc0),
//        (Func<object>)(() => (DummyDel1)DummyFunc1),
//        (Func<object>)(() => (DummyDel2)DummyFunc2),
//        (Func<object>)(() => (DummyDel3)DummyFunc3),
//        (Func<object>)(() => (DummyDel4)DummyFunc4),
//        (Func<object>)(() => (DummyDel5)DummyFunc5),
//        (Func<object>)(() => (DummyDel6)DummyFunc6),
//        (Func<object>)(() => (DummyDel7)DummyFunc7),
//        (Func<object>)(() => (DummyDel8)DummyFunc8),
//        (Func<object>)(() => (DummyDel9)DummyFunc9),
//        (Func<object>)(() => (DummyDel10)DummyFunc10),
//        (Func<object>)(() => (DummyDel11)DummyFunc11),
//        (Func<object>)(() => (DummyDel12)DummyFunc12),
//        (Func<object>)(() => (DummyDel13)DummyFunc13),
//        (Func<object>)(() => (DummyDel14)DummyFunc14),
//        (Func<object>)(() => (DummyDel15)DummyFunc15),
//        (Func<object>)(() => (DummyDel16)DummyFunc16),
//    };

//    [StructLayout(LayoutKind.Sequential)]
//    protected struct ObjectHeader
//    {
//        public IntPtr TypeHandle;
//        public IntPtr SyncAddress;
//    }

//    private static FieldIndicator<IntPtr> fi_methodPtr;
//    private static FieldIndicator<IntPtr> fi_methodPtrAux;

//    static UnsafeDelegateWrapperBase()
//    {
//        foreach (var field in typeof(Action).GetAllFields())
//        {
//            if (field.Name == "_methodPtr")
//            {
//                fi_methodPtr = field.GetFieldOffset();
//            }
//            else if (field.Name == "_methodPtrAux")
//            {
//                fi_methodPtrAux = field.GetFieldOffset();
//            }
//        }
//    }

//    public object CreateDelegateUnsafe(Type delType, MethodInfo method)
//    {
//        var delParamCount = method.GetParameters().Length;
//        if (!method.IsStatic) ++delParamCount;
//        if (delParamCount <= _DummyDelTemplates.Length)
//        {
//            var template = _DummyDelTemplates[delParamCount];
//            object rvdel;
//            if ((rvdel = NewDelegateObject(delType)) == null)
//            {
//                rvdel = _DummyCreators[delParamCount]();
//                ref var oheader = ref rvdel.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
//                oheader.TypeHandle = delType.TypeHandle.Value;
//            }

//            RuntimeHelpers.PrepareMethod(method.MethodHandle);
//            var fn = method.MethodHandle.GetFunctionPointer();
//            rvdel.GetField(fi_methodPtrAux) = fn;
//            rvdel.GetField(fi_methodPtr) = template.GetField(fi_methodPtr);
//            return rvdel;
//        }
//        else
//        { // TODO: use expression.
//            throw new NotImplementedException();
//        }
//    }

//    public abstract object NewDelegateObject(Type delType);
//}

//public class UnsafeDelegateWrapperNativeAOT : UnsafeDelegateWrapperBase
//{
//    private static Func<IntPtr, object> _RhNewObject_del;

//    static UnsafeDelegateWrapperNativeAOT()
//    {
//        var trl = Type.GetType("System.Runtime.RuntimeImports, System.Private.CoreLib");
//        var minew = trl.GetMethod("RhNewObject", BindingFlags.Static | BindingFlags.NonPublic);
//        _RhNewObject_del = (Func<IntPtr, object>)Delegate.CreateDelegate(typeof(Func<IntPtr, object>), minew);
//    }

//    public override object NewDelegateObject(Type delType)
//    {
//        RuntimeHelpers.RunClassConstructor(delType.TypeHandle);
//        return _RhNewObject_del(delType.TypeHandle.Value);
//    }
//}

//public class UnsafeDelegateWrapperCore : UnsafeDelegateWrapperBase
//{
//    private static void DummyFunc0() { }

//    public override object NewDelegateObject(Type delType)
//    {
//        var imi = delType.GetMethod("Invoke");
//        return Delegate.CreateDelegate(delType, null, imi);
//        //return null;
//    }
//}

public interface IDelegateWrapper
{
    object CreateDelegate(Type delType, MethodInfo method);
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

    public abstract object CreateDelegate(Type delType, MethodInfo method);
}

public class UnsafeDelegateWrapperCore : UnsafeDelegateWrapperBase
{
    [StructLayout(LayoutKind.Sequential)]
    protected struct ObjectHeader
    {
        public IntPtr TypeHandle;
        public IntPtr SyncAddress;
    }

    protected static FieldIndicator<IntPtr> fi_methodPtr;
    protected static FieldIndicator<IntPtr> fi_methodPtrAux;
    protected static FieldIndicator<object> fi_target;
    protected static FieldIndicator<object> fi_methodBase;

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

    public override object CreateDelegate(Type delType, MethodInfo method)
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
                RuntimeHelpers.PrepareMethod(method.MethodHandle);
                var fn = method.MethodHandle.GetFunctionPointer();

                Delegate template = null;
                lock (_DynamicTemplates)
                {
                    _DynamicTemplates.TryGetValue(delParamCount, out template);
                }
                if (template == null)
                {
                    List<Type> pars = new List<Type>();
                    var imi = delType.GetMethod("Invoke");
                    foreach (var par in imi.GetParameters())
                    {
                        pars.Add(par.ParameterType);
                    }
                    var dm = new DynamicMethod(delType.Name + "_dyn", imi.ReturnType, pars.ToArray());
                    var il = dm.GetILGenerator();
                    il.Emit(OpCodes.Ret);
                    template = dm.CreateDelegate(delType);
                    lock (_DynamicTemplates)
                    {
                        _DynamicTemplates[delParamCount] = template;
                    }
                }
                var rvdel = template.Clone();
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

public class UnsafeDelegateWrapperNativeAOT : UnsafeDelegateWrapperBase
{
    [StructLayout(LayoutKind.Sequential)]
    private struct DelegateFields
    {
        public IntPtr _firstParameter;
        public IntPtr _helperObject;
        public IntPtr _extraFunctionPointerOrData;
        public IntPtr _functionPointer;
    }
    protected static FieldIndicator<object> fi_firstParameter;
    protected static FieldIndicator<object> fi_helperObject;
    protected static FieldIndicator<IntPtr> fi_extraFunctionPointerOrData;
    protected static FieldIndicator<IntPtr> fi_functionPointer;

    [StructLayout(LayoutKind.Sequential)]
    protected struct ObjectHeader
    {
        public IntPtr TypeHandle;
    }

    private delegate Delegate CreateDelegateWithoutSignatureValidationDel(MethodInfo thiz, Type delType, object target, bool isStatic, bool isOpen);
    private static CreateDelegateWithoutSignatureValidationDel _CreateDelegateWithoutSignatureValidationFunc;
    static UnsafeDelegateWrapperNativeAOT()
    {
        var cmi = typeof(object).GetMethod("ToString").GetType().GetMethod("CreateDelegateWithoutSignatureValidation", BindingFlags.Instance | BindingFlags.NonPublic);
        _CreateDelegateWithoutSignatureValidationFunc = (CreateDelegateWithoutSignatureValidationDel)cmi.Invoke(cmi, new object[] { typeof(CreateDelegateWithoutSignatureValidationDel), null, false, true });

        DelegateFields dummy = default;
        fi_firstParameter = dummy.GetFieldOffset(ref dummy._firstParameter);
        fi_helperObject = dummy.GetFieldOffset(ref dummy._helperObject);
        fi_extraFunctionPointerOrData = dummy.GetFieldOffset(ref dummy._extraFunctionPointerOrData);
        fi_functionPointer = dummy.GetFieldOffset(ref dummy._functionPointer);
    }

    public override object CreateDelegate(Type delType, MethodInfo method)
    {
        if (method.IsStatic)
        {
            return _CreateDelegateWithoutSignatureValidationFunc(method, delType, null, true, true);
        }
        else
        {
            var obj = CreateUninitializedDelegate(method.ReflectedType);
            var rvdel = _CreateDelegateWithoutSignatureValidationFunc(method, delType, obj, true, true);
            Console.WriteLine(method.MethodHandle.GetFunctionPointer());
            Console.WriteLine($"{nameof(fi_firstParameter)}: {rvdel.GetField(fi_firstParameter)}");
            Console.WriteLine($"{nameof(fi_helperObject)}: {rvdel.GetField(fi_helperObject)}");
            Console.WriteLine($"{nameof(fi_extraFunctionPointerOrData)}: {rvdel.GetField(fi_extraFunctionPointerOrData)}");
            Console.WriteLine($"{nameof(fi_functionPointer)}: {rvdel.GetField(fi_functionPointer)}");
            return rvdel;
        }
        //var delParamCount = method.GetParameters().Length;
        //if (!method.IsStatic) ++delParamCount;
        //if (delParamCount <= _DummyDelTemplates.Length)
        //{
        //    Delegate template = _DummyDelTemplates[delParamCount];
        //    object rvdel = template.Clone();
        //    ref var oheader = ref rvdel.GetField<ObjectHeader>(-UnsafeEx.GetObjectHeaderSize());
        //    oheader.TypeHandle = delType.TypeHandle.Value;

        //    RuntimeHelpers.PrepareMethod(method.MethodHandle);
        //    var fn = method.MethodHandle.GetFunctionPointer();
        //    rvdel.GetField(fi_extraFunctionPointerOrData) = fn;
        //    rvdel.GetField(fi_functionPointer) = template.GetField(fi_functionPointer);
        //    rvdel.GetField(fi_helperObject) = null;
        //    rvdel.GetField(fi_firstParameter) = rvdel;
        //    return rvdel;
        //}
        //else
        //{
        //    //if (RuntimeInfo.IsDynamicCodeEnabled)
        //    //{
        //    //    RuntimeHelpers.PrepareMethod(method.MethodHandle);
        //    //    var fn = method.MethodHandle.GetFunctionPointer();

        //    //    Delegate template = null;
        //    //    lock (_DynamicTemplates)
        //    //    {
        //    //        _DynamicTemplates.TryGetValue(delParamCount, out template);
        //    //    }
        //    //    if (template == null)
        //    //    {
        //    //        List<Type> pars = new List<Type>();
        //    //        var imi = delType.GetMethod("Invoke");
        //    //        foreach (var par in imi.GetParameters())
        //    //        {
        //    //            pars.Add(par.ParameterType);
        //    //        }
        //    //        var dm = new DynamicMethod(delType.Name + "_dyn", imi.ReturnType, pars.ToArray());
        //    //        var il = dm.GetILGenerator();
        //    //        il.Emit(OpCodes.Ret);
        //    //        template = dm.CreateDelegate(delType);
        //    //        lock (_DynamicTemplates)
        //    //        {
        //    //            _DynamicTemplates[delParamCount] = template;
        //    //        }
        //    //    }
        //    //    var rvdel = template.Clone();
        //    //    rvdel.GetField(fi_methodPtrAux) = fn;
        //    //    rvdel.GetField(fi_methodBase) = method;
        //    //    rvdel.GetField(fi_target) = rvdel;
        //    //    return rvdel;
        //    //}
        //    //else
        //    {
        //        RuntimeHelpers.PrepareMethod(method.MethodHandle);
        //        var fn = method.MethodHandle.GetFunctionPointer();
        //        return Marshal.GetDelegateForFunctionPointer(fn, delType); // Notice: - Dangerous
        //    }
        //}
    }

    private delegate object CreateObjectDelegate(IntPtr typeHandle);
    private static object CreateUninitializedDelegate(Type delType)
    {
        var trl = Type.GetType("System.Runtime.RuntimeImports, System.Private.CoreLib");
        var minew = trl.GetMethod("RhNewObject", BindingFlags.Static | BindingFlags.NonPublic);
        RuntimeHelpers.PrepareMethod(minew.MethodHandle);
        var fnnew = minew.MethodHandle.GetFunctionPointer();
        //var _RhNewObject_del = (CreateObjectDelegate)Marshal.GetDelegateForFunctionPointer(fnnew, typeof(CreateObjectDelegate));
        ////_RhNewObject_del = (Func<IntPtr, object>)Delegate.CreateDelegate(typeof(Func<IntPtr, object>), minew);
        //return _RhNewObject_del(delType.TypeHandle.Value);
        return minew.Invoke(null, new object[] { delType.TypeHandle.Value });
    }
}

class Program
{
    [StructLayout(LayoutKind.Sequential)]
    private struct ObjectHeader
    {
        public IntPtr TypeHandle;
        public IntPtr SyncAddress;
    }

#if NET10_0_OR_GREATER
    [DynamicDependency(DynamicallyAccessedMemberTypes.AllFields, type: typeof(TestFieldsClass))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.AllFields, type: typeof(TestFieldsStruct))]
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    //[RequiresDynamicCode("Calls System.Reflection.MethodInfo.MakeGenericMethod(params Type[])")]
#endif
    public static void Main()
    {
#if NET10_0_OR_GREATER
        if (RuntimeInfo.IsNativeAOT)
        {
            NativeAOTHelper.Prepare();
        }
#endif
        var stopwatch = Stopwatch.StartNew();
        //#region Test For StringPool
        //string[] strs = new string[1024 * 1024 * 2];
        //for (int i = 0; i < 1024 * 1024; ++i)
        //{
        //    strs[i] = strs[i + 1024 * 1024] = Guid.NewGuid().ToString();
        //}
        //string[] strsNonPooled = new string[1024 * 1024];
        //for (int i = 0; i < 1024 * 1024; ++i)
        //{
        //    strsNonPooled[i] = i.ToString();
        //}

        //var pool1 = new StringPoolConcurrent();
        //var pool2 = new StringPoolLocked();
        //var pool3 = new StringPoolConcurrent();
        //var pool4 = new StringPoolLocked();

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    pool1.Push(str);
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool1: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    pool2.Push(str);
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool2: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    pool3.Push(str);
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool3: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    pool4.Push(str);
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool4: " + stopwatch.Elapsed);

        ////-------------------------------------------------------

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    if (pool1.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool1 should contain all strs");
        //    }
        //}
        //foreach (var str in strsNonPooled)
        //{
        //    if (pool1.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool1 should not contain any strsNonPooled");
        //    }
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool1 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //foreach (var str in strs)
        //{
        //    if (pool2.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool2 should contain all strs");
        //    }
        //}
        //foreach (var str in strsNonPooled)
        //{
        //    if (pool2.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool2 should not contain any strsNonPooled");
        //    }
        //}
        //stopwatch.Stop();
        //Console.WriteLine("pool2 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    if (pool3.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool3 should contain all strs");
        //    }
        //});
        //Parallel.ForEach(strsNonPooled, str =>
        //{
        //    if (pool3.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool3 should not contain any strsNonPooled");
        //    }
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool3 check pooled: " + stopwatch.Elapsed);

        //stopwatch.Restart();
        //Parallel.ForEach(strs, str =>
        //{
        //    if (pool4.IsPooled(str) < 0)
        //    {
        //        throw new Exception("pool4 should contain all strs");
        //    }
        //});
        //Parallel.ForEach(strsNonPooled, str =>
        //{
        //    if (pool4.IsPooled(str) >= 0)
        //    {
        //        throw new Exception("pool4 should not contain any strsNonPooled");
        //    }
        //});
        //stopwatch.Stop();
        //Console.WriteLine("pool4 check pooled: " + stopwatch.Elapsed);
        //#endregion

        //#region Test For WeakAttachment
        //WeakAttachment.Key key = "test_key";
        //Test1 test1 = new Test1();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //test1.exFields ??= new ExFields();
        //test1.exFields.Value = i;
        //if (test1.exFields.Value != i)
        //{
        //throw new Exception("test1.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("raw field access: " + stopwatch.Elapsed);

        //Test2 test2 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test2.GetOrCreateAttachment<ExFields>(key);
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test2.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("att field access: " + stopwatch.Elapsed);

        //Test2 test3 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test3.GetOrCreateAttachment<ExFields>("test_Key_Unpooled");
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test3.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("str field access: " + stopwatch.Elapsed);

        //Test2 test4 = new Test2();
        //stopwatch.Restart();
        //for (int i = 0; i < 1000000; i++)
        //{
        //var exFields = test4.GetOrCreateAttachment<ExFields>();
        //exFields.Value = i;
        //if (exFields.Value != i)
        //{
        //throw new Exception("test4.exFields.Value should be " + i);
        //}
        //}
        //stopwatch.Stop();
        //Console.WriteLine("typ field access: " + stopwatch.Elapsed);

        //class Test1
        //{
        //    public ExFields? exFields;
        //}
        //class Test2
        //{
        //}
        //class ExFields
        //{
        //    public int Value { get; set; }
        //}
        //#endregion

        //#region Test For UnsafeAccessor
        //Console.WriteLine(RuntimeInfo.Runtime);
        //Console.WriteLine(UnsafeEx.GetObjectHeaderSize());

        //var odel = new Action<int>(value =>
        //{
        //    Console.WriteLine("Received event with value: " + value);
        //});
        //var ndel = new TestEventTrigger.TestEventHandler(value => { });

        //TestFieldsClass t = new TestFieldsClass();
        //foreach (var field in typeof(TestFieldsClass).GetAllFields())
        //{
        //    Console.WriteLine(t.GetField(field.OfType<int>()));
        //}

        //TestFieldsStruct ts = new TestFieldsStruct(0, 1, 2, 3, 4, 5);
        //foreach (var field in typeof(TestFieldsStruct).GetAllFields())
        //{
        //    Console.WriteLine(t.GetField(field.OfType<int>()));
        //}
        //#endregion

        #region Test For Delegate
        IDelegateWrapper wrapper = null;
        if (RuntimeInfo.IsNativeAOT)
        {
            wrapper = new UnsafeDelegateWrapperNativeAOT();
        }
        else
        {
            wrapper = new UnsafeDelegateWrapperCore();
        }

        var mi1 = typeof(Program).GetMethod("TestMethod1", BindingFlags.NonPublic | BindingFlags.Static);
        TestDelegate2 del2 = (TestDelegate2)wrapper.CreateDelegate(typeof(TestDelegate2), mi1);
        int a = 0;
        RawRef rr = new RawRef();
        rr.SetRef(ref a);
        Console.WriteLine(rr.Address);
        Console.WriteLine(del2(ref a));

        var ti = new TestDelegateChildClass();
        var mi2 = typeof(TestDelegateClass).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        var deli2 = (TestDelegate3)wrapper.CreateDelegate(typeof(TestDelegate3), mi2);
        Console.WriteLine(deli2(ti, ref a));

        var mig = typeof(TestDelegateGenericClass<int>).GetMethod("TestMethod", BindingFlags.Public | BindingFlags.Instance);
        var delg = (TestDelegate3)wrapper.CreateDelegate(typeof(TestDelegate3), mig);
        Console.WriteLine(delg.Invoke(null, ref a));
        #endregion
    }

    private static long TestMethod1(IntPtr p)
    {
        return p.ToInt64() + 1;
    }
    private static long TestMethod2(ref int r)
    {
        RawRef rr = new RawRef();
        rr.SetRef(ref r);
        return rr.Address.ToInt64() + 2;
    }
    private static void DummyMethod2(byte a)
    {

    }
}

delegate void DummyDelegate2(byte a);
delegate void DummyDelegate3(byte a, byte b);

delegate TestDelegate1 CreateTestDelegate1Del(object obj, IntPtr method);
delegate long TestDelegate1(IntPtr p);
delegate long TestDelegate2(ref int r);

delegate long TestDelegate3(object thiz, ref int r);

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateClass
{
    public virtual long TestMethod(IntPtr p)
    {
        Console.WriteLine("TestDelegateClass.TestMethod");
        return p.ToInt64() + 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateChildClass : TestDelegateClass
{
    public override long TestMethod(IntPtr p)
    {
        Console.WriteLine("TestDelegateChildClass.TestMethod");
        return p.ToInt64() - 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.All)]
#endif
public class TestDelegateGenericClass<T>
{
    public long TestMethod(ref T r)
    {
        RawRef rr = new RawRef();
        rr.SetRef(ref r);
        return rr.Address.ToInt64() + 1;
    }
}

#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.AllFields)]
#endif
class TestFieldsClass
{
    public int Field0 = 0;
    public int Field1 = 1;
    public int Field2 = 2;
    public int Field3 = 3;
    public int Field4 = 4;
    public int Field5 = 5;
}
#if NET10_0_OR_GREATER
[DynamicallyAccessedMembers(
    DynamicallyAccessedMemberTypes.AllFields)]
#endif
struct TestFieldsStruct
{
    public int Field0;
    public int Field1;
    public int Field2;
    public int Field3;
    public int Field4;
    public int Field5;

    public TestFieldsStruct(int i0, int i1, int i2, int i3, int i4, int i5)
    {
        Field0 = i0;
        Field1 = i1;
        Field2 = i2;
        Field3 = i3;
        Field4 = i4;
        Field5 = i5;
    }
}

class DelegateStub
{
    public object Target;
    public object MethodBase;
    public IntPtr Method;
}
static class TestEventTrigger
{
    public static event Action<int> OnTestEvent;
    public static void Trigger(int value)
    {
        OnTestEvent?.Invoke(value);
    }

    public delegate void TestEventHandler(int value);
    public static event TestEventHandler OnTestEventWithCustomDelegate;
    public static void TriggerWithCustomDelegate(int value)
    {
        OnTestEventWithCustomDelegate?.Invoke(value);
    } 
}

[StructLayout(LayoutKind.Explicit)]
public unsafe struct FieldDesc
{
    [FieldOffset(0)] private readonly void* m_pMTOfEnclosingClass;
    [FieldOffset(8)] private readonly uint m_dword1;
    [FieldOffset(12)] private readonly uint m_dword2;

    public int Offset => (int)(m_dword2 & 0x07FFFFFF);
    public int MB => (int)(m_dword1 & 0x00FFFFFF);
    public bool IsStatic => (m_dword1 & (1u << 24)) != 0;
    public bool IsThreadLocal => (m_dword1 & (1u << 25)) != 0;
    public bool IsRVA => (m_dword1 & (1u << 26)) != 0;
    public int Protection => (int)((m_dword1 >> 27) & 0x7);
    public bool RequiresFullMbValue => (m_dword1 & (1u << 30)) != 0;
    public int Type => (int)((m_dword2 >> 27) & 0x1F);
}

[StructLayout(LayoutKind.Sequential, Pack = 64)]
struct A
{
    public int x;
    public B b;   // 嵌套 struct
    public long y;
    public object aobj;
}

struct B
{
    public float f;
    public object obj;
}

[StructLayout( LayoutKind.Sequential, Pack = 1)]
struct C
{
    public byte a;
    public double c;
}

struct D
{
    public byte b;
    public E e;
}

[StructLayout(LayoutKind.Sequential, Pack = 128)]
struct E
{
    public short c;
    public double d;
    public F f;
}

unsafe struct F
{
    public fixed byte Buffer[128];
}

//#region Test For WeakDelegate

//var odel = new Action<int>(value =>
//{
//    Console.WriteLine("Received event with value: " + value);
//});
//var ndel = new TestEventTrigger.TestEventHandler(value => { });

//var omf = odel.GetType().GetField("_method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var otf = odel.GetType().GetField("_target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var nmf = ndel.GetType().GetField("_method", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
//var ntf = ndel.GetType().GetField("_target", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

//var nndel = System.Runtime.CompilerServices.Unsafe.As<TestEventTrigger.TestEventHandler>(odel);

//TestEventTrigger.OnTestEventWithCustomDelegate += nndel;
//TestEventTrigger.TriggerWithCustomDelegate(18);

//static class TestEventTrigger
//{
//    public static event Action<int> OnTestEvent;

//    public delegate void TestEventHandler(int value);

//    public static event TestEventHandler OnTestEventWithCustomDelegate;
//    public static void Trigger(int value)
//    {
//        OnTestEvent?.Invoke(value);
//    }

//    public static void TriggerWithCustomDelegate(int value)
//    {
//        OnTestEventWithCustomDelegate?.Invoke(value);
//    }

//    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_target")]
//    public static void TestAccessor()
//    {

//    }
//}
//#endregion
