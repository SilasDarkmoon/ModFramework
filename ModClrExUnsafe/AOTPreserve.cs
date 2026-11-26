using System;
using System.Diagnostics.CodeAnalysis;

namespace Mod.LowLevel
{
    // Central rooting table for reflection-reachable members under NativeAOT (ILC)
    // and managed stripping (ILLink / UnityLinker). Triggered automatically from
    // RuntimeInfo's static constructor; ModFramework's Main additionally calls
    // Prepare() explicitly so rooting never hinges on implicit reachability alone.
    // NOTE: ILC only processes these attributes when the host member (Prepare) is
    // itself reachable in the compile graph.
    public static class AOTPreserve
    {
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.FieldInfos.NativeFormat.NativeFormatRuntimeFieldInfo", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.MethodInfos.RuntimeMethodInfo", "System.Private.CoreLib")]
        // 10.0.15 trimmer no longer keeps derived overrides through the base-class
        // DLA alone: UnsafeDelegateWrapperNativeAOT cctor reflects on the runtime
        // cdmi type RuntimeNamedMethodInfo`1 (typeof(minew) is the DERIVED type),
        // so its override chain must be rooted explicitly.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Delegate", "System.Private.CoreLib")]

        // SizeMeasurer (FieldEx.cs) resolves the allocation counter BY NAME on
        // NativeAOT (the netstandard2.0 contract has no such API) - keep the
        // method's reflection metadata or the counter falls back to the walk.
        [DynamicDependency("GetAllocatedBytesForCurrentThread()", "System.GC", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.ByRefParam", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.ByRefLocalSlot", "PointerDelegate")]

        // NativeAOT wrappers of CreateDelegateUnsafe (nested classes + nested
        // delegates; ILC keeps their reflection data via these entries).
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT+CreateDelegateWithoutSignatureValidationDel", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT+UnsafeCreateDelegateDel", "ModClrExUnsafe")]

        // CurryWrapper family (nested inside UnsafeEx; an earlier revision
        // pointed at a nonexistent Mod.DelegateCurry).
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`2", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`3", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`4", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`5", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`6", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`7", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`8", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`9", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`10", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`11", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`12", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`13", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`14", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`15", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`16", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`17", "ModClrExUnsafe")]

        // RefFunc family (byref-returning delegates; core of the RefFunc path).
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`1", "PointerDelegate")]
        // RefInvoker wrapper family (Generator-woven Invoke bodies reach these through
        // interface dispatch the trimmer cannot see; AOT strips them at publish time
        // and the runtime throws TypeLoadException from FuncRefInvoker`1.Invoke)
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FuncRefInvoker1`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`17", "PointerDelegate")]

        // FreeFunc family.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`17", "PointerDelegate")]

        // PointerFunc family.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`17", "PointerDelegate")]

        // StaticFieldPlatDep (FieldExStatic.cs) reflects into the NativeAOT
        // FieldAccessor chain to rebuild static field addresses. Root every
        // member it reads, otherwise the trimmer drops the reflection metadata
        // and the runtime throws PlatformNotSupportedException from GetEntry.
        // Verified with Probes/StaticFieldAddressProbe (NativeAOT publish).
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.FieldInfos.RuntimeFieldInfo", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Core.Execution.FieldAccessor", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.StaticFieldAccessor", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.WritableStaticFieldAccessor", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.RegularStaticFieldAccessor", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.ValueTypeFieldAccessorForStaticFields", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.ReferenceTypeFieldAccessorForStaticFields", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Reflection.Execution.FieldAccessors.PointerTypeFieldAccessorForStaticFields", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Runtime.FieldTableFlags", "System.Private.Reflection.Execution")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Internal.Runtime.Augments.RuntimeAugments", "System.Private.CoreLib")]

        // ModFramework app test stubs (plain "Required All" entries migrated from
        // ModFramework/Rd.xml). Generic instantiation entries and <GenericArgument>
        // directives have no attribute equivalent and stay in Rd.xml.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestFieldsStruct", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestFieldsClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestFieldsStruct`1", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestFieldsClass`1", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestBaseClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript+TestChildClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestFieldsStruct", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestFieldsClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestFieldsStruct`1", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestFieldsClass`1", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestBaseClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_UnsafeInstanceDelegate+TestChildClass", "ModFramework")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "TestScript_WeakEvent+<>c__DisplayClass1_0", "ModFramework")]

        public static void Prepare() { }

        // Unity/IL2CPP counterpart of the DynamicDependency table above.
        // UnityLinker (managed stripping) matches PreserveAttribute by TYPE NAME
        // only, so this attribute self-roots Preserve() unconditionally, and every
        // reference inside the body enters the mark closure. The function is never
        // called; under NativeAOT it is unreachable and gets compiled away, so it
        // costs nothing outside Unity builds.
        // typeof roots the TYPE DEFINITIONS (the reflection targets). Member-level
        // survival relies on the normal call graph: UnityLinker keeps interface
        // dispatch conservatively (unlike ILC), and the Generator-woven Invoke
        // bodies plus the public API chain (DelegateCurry / CreateDelegateUnified)
        // statically reference the implementations. Runtime-internal types of
        // System.Private.CoreLib listed above do not exist on IL2CPP (different
        // reflection backend) and are intentionally NOT mirrored here.
        // NOTE: member-level behavior under High stripping needs a Unity build
        // to verify; if members get stripped, fall back to link.xml.
        [UnityEngine.Scripting.Preserve]
        private static void Preserve()
        {
            // PointerDelegate cross-assembly families (mirror of the entries above).
            _ = typeof(ByRefParam);
            _ = typeof(ByRefLocalSlot);
            _ = typeof(FuncRefInvoker1<>);
            _ = typeof(RefFunc<>);
            _ = typeof(RefFunc<,>);
            _ = typeof(RefFunc<,,>);
            _ = typeof(RefFunc<,,,>);
            _ = typeof(RefFunc<,,,,>);
            _ = typeof(RefFunc<,,,,,>);
            _ = typeof(RefFunc<,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,,,,,>);
            _ = typeof(RefFunc<,,,,,,,,,,,,,,,>);
            _ = typeof(FreeFunc<>);
            _ = typeof(FreeFunc<,>);
            _ = typeof(FreeFunc<,,>);
            _ = typeof(FreeFunc<,,,>);
            _ = typeof(FreeFunc<,,,,>);
            _ = typeof(FreeFunc<,,,,,>);
            _ = typeof(FreeFunc<,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,,,,,>);
            _ = typeof(FreeFunc<,,,,,,,,,,,,,,,>);
            _ = typeof(PointerFunc<>);
            _ = typeof(PointerFunc<,>);
            _ = typeof(PointerFunc<,,>);
            _ = typeof(PointerFunc<,,,>);
            _ = typeof(PointerFunc<,,,,>);
            _ = typeof(PointerFunc<,,,,,>);
            _ = typeof(PointerFunc<,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,,,,,>);
            _ = typeof(PointerFunc<,,,,,,,,,,,,,,,>);
        }
    }
}

// Downlevel definitions (this assembly targets netstandard2.0 only).
// ILC and ILLink recognize these by full type name only -- the defining assembly
// and visibility are irrelevant. Constructor signatures and enum values must stay
// byte-compatible with the official .NET 5+ definitions: the tools decode the
// attribute blob positionally.
namespace System.Diagnostics.CodeAnalysis
{
    [Flags]
    internal enum DynamicallyAccessedMemberTypes
    {
        All = -1,
        None = 0,
        PublicParameterlessConstructor = 1,
        PublicConstructors = 3 | PublicParameterlessConstructor,
        NonPublicConstructors = 4,
        PublicMethods = 8,
        NonPublicMethods = 16,
        PublicFields = 32,
        NonPublicFields = 64,
        PublicNestedTypes = 128,
        NonPublicNestedTypes = 256,
        PublicProperties = 512,
        NonPublicProperties = 1024,
        PublicEvents = 2048,
        NonPublicEvents = 4096,
        Interfaces = 8192,
    }

    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Constructor | AttributeTargets.Field,
        Inherited = false, AllowMultiple = true)]
    internal sealed class DynamicDependencyAttribute : Attribute
    {
        public DynamicDependencyAttribute(string memberSignature) { MemberSignature = memberSignature; }
        public DynamicDependencyAttribute(string memberSignature, Type type) { MemberSignature = memberSignature; Type = type; }
        public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
        { MemberSignature = memberSignature; TypeName = typeName; AssemblyName = assemblyName; }
        public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type) { MemberTypes = memberTypes; Type = type; }
        public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
        { MemberTypes = memberTypes; TypeName = typeName; AssemblyName = assemblyName; }

        public DynamicallyAccessedMemberTypes MemberTypes { get; }
        public string MemberSignature { get; }
        public Type Type { get; }
        public string TypeName { get; }
        public string AssemblyName { get; }
    }
}

// Unity's [Preserve] for IL2CPP managed stripping (UnityLinker). The official
// UnityEngine.Scripting.PreserveAttribute is a pure marker: no constructor
// parameters, no fields, no properties (Unity 6/7 scripting API docs) --
// nothing to add here, and UnityLinker never reads the attribute contents
// anyway: it matches the TYPE NAME only. Keep the UnityEngine.Scripting
// namespace so usages read identically to real Unity code ([Preserve] resolves
// to this internal type in the ns2.0 build).
// WARNING: this file must ship as a compiled dll, never be compiled inside a
// Unity project -- there the name would collide with the official attribute
// in the merged System.Diagnostics.CodeAnalysis-style namespace (CS0104).
namespace UnityEngine.Scripting
{
    internal class PreserveAttribute : Attribute
    {
    }
}
