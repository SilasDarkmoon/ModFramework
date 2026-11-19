using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static ref byte GetStaticFieldAddress(FieldInfo field)
        {
            return ref StaticFieldPlatDep.GetStaticFieldAddress(field);
        }
        public static ref byte GetThreadStaticAddress(FieldInfo field)
        {
            return ref StaticFieldPlatDep.GetThreadStaticAddress(field);
        }

        // Diagnostic for the IL2CPP TLS support: how the thread-static storage
        // chain was derived on this runtime ("hint:..." = known-layout fast
        // path, "bfs:..." = empirical walk). Empty on other runtimes.
        private static string _tlsChainDescription = "";
        public static string TlsChainDescription { get { return _tlsChainDescription; } }

        private static partial class StaticFieldPlatDep
        {
            private interface IPlatDep
            {
                ref byte GetStaticFieldAddress(FieldInfo field); // NOTICE: cache the address in the implementation, when the address is fixed, or cache the getter wrapper.
                ref byte GetThreadStaticAddress(FieldInfo field); // NOTICE: cache the address in the implementation, when the address is fixed, or cache the getter wrapper.
            }
            private static IPlatDep _PlatDep;

            static StaticFieldPlatDep()
            {
                switch (RuntimeInfo.Runtime)
                {
                    case RuntimeInfo.RuntimeCategory.NetFramework:
                    case RuntimeInfo.RuntimeCategory.NetCore:
                    case RuntimeInfo.RuntimeCategory.Mono:
                    case RuntimeInfo.RuntimeCategory.UnityMono:
                        // Unity's editor and Mono players run MonoBleedingEdge JIT,
                        // so the same ldsflda DynamicMethod path applies. Mono
                        // rejects byref return types on DynamicMethod, so those
                        // runtimes use the IntPtr getter + double-read retry.
                        _PlatDep = new PlatDepJit();
                        break;
                    case RuntimeInfo.RuntimeCategory.NativeAOT:
                        _PlatDep = new PlatDepNativeAOT();
                        break;
                    case RuntimeInfo.RuntimeCategory.UnityIl2Cpp:
                        _PlatDep = new PlatDepIl2Cpp();
                        break;
                    default:
                        _PlatDep = new PlatDepUnsupported();
                        break;
                }
            }

            public static ref byte GetStaticFieldAddress(FieldInfo field)
            {
                Validate(field);
                return ref _PlatDep.GetStaticFieldAddress(field);
            }
            public static ref byte GetThreadStaticAddress(FieldInfo field)
            {
                Validate(field);
                return ref _PlatDep.GetThreadStaticAddress(field);
            }

            private static void Validate(FieldInfo field)
            {
                if (field == null)
                {
                    throw new ArgumentNullException(nameof(field));
                }
                if (!field.IsStatic)
                {
                    throw new ArgumentException("The field must be static.", nameof(field));
                }
                if (field.IsLiteral)
                {
                    throw new ArgumentException("A literal (const) field has no storage.", nameof(field));
                }
            }

            // Neither ldsflda nor the NativeAOT FieldAccessor guarantees that the
            // class constructor has run before the static storage is resolved.
            // Called once per field, when its getter/entry is first built.
            private static void EnsureClassConstructor(FieldInfo field)
            {
                var declaringType = field.DeclaringType;
                if (declaringType != null)
                {
                    RuntimeHelpers.RunClassConstructor(declaringType.TypeHandle);
                }
            }

            private static PropertyInfo FindProperty(Type type, string name)
            {
                for (var cur = type; cur != null; cur = cur.BaseType)
                {
                    var p = cur.GetProperty(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (p != null)
                    {
                        return p;
                    }
                }
                return null;
            }

            private static FieldInfo FindField(Type type, string name)
            {
                for (var cur = type; cur != null; cur = cur.BaseType)
                {
                    var f = cur.GetField(name, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    if (f != null)
                    {
                        return f;
                    }
                }
                return null;
            }

            private static MethodInfo FindStaticMethod(Type type, string name, params Type[] signature)
            {
                return type.GetMethod(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, signature, null);
            }

            // ------------------------------------------------------------------
            // JIT runtimes (CoreCLR / .NET Framework / Mono).
            //
            // A DynamicMethod emitting "ldsflda <field>; ret" lets the runtime
            // resolve the storage itself, so non-GC statics, GC statics, thread
            // statics, generic instantiations and private fields all work without
            // parsing any runtime-internal layout. CoreCLR's internal icall
            // RuntimeFieldHandle.GetStaticFieldAddress is NOT used: it does not
            // exist on .NET Framework/Mono, and on CoreCLR it silently returns a
            // garbage address (the ordinary static block) for [ThreadStatic].
            //
            // .NET Framework and Mono reject byref return types on DynamicMethod,
            // so they use "ldsflda; conv.u; ret" (IntPtr). CoreCLR supports the
            // byref shape and keeps the address as a tracked byref throughout.
            // The getter wrapper is cached per field; the address is NEVER cached,
            // because GC statics regions and TLS blocks are per-thread/movable.
            // ------------------------------------------------------------------
            private delegate ref byte RefByteGetter();
            private delegate IntPtr IntPtrGetter();

            private interface IFieldGetter
            {
                ref byte Get();
            }

            private sealed class ByRefFieldGetter : IFieldGetter
            {
                private readonly RefByteGetter _Getter;
                public ByRefFieldGetter(RefByteGetter getter) { _Getter = getter; }
                public ref byte Get() { return ref _Getter(); }
            }

            private sealed class IntPtrFieldGetter : IFieldGetter
            {
                private readonly IntPtrGetter _Getter;
                public IntPtrFieldGetter(IntPtrGetter getter) { _Getter = getter; }

                public ref byte Get()
                {
                    // The IntPtr snapshot is untracked while ToRef materializes
                    // the byref. Re-evaluate ldsflda afterwards: if the storage
                    // moved inside that window, rebuild from the fresh address
                    // instead of handing out a wild byref. Static slots do not
                    // move on the JIT runtimes today, but this keeps the bridge
                    // correct if that ever changes.
                    for (; ; )
                    {
                        var address = _Getter();
                        ref byte r = ref UnsafeEx.ToRef<byte>(address);
                        if (_Getter() == address)
                        {
                            return ref r;
                        }
                    }
                }
            }

            private static byte _ByRefProbeStorage;

            // DynamicMethod is absent from the netstandard2.0 reference surface,
            // although it exists at runtime on every JIT runtime we support.
            // Resolve it through reflection (the same approach RuntimeInfo uses
            // for its dynamic-code probe) instead of adding an Emit package.
            private static Type _DynamicMethodType;
            private static ConstructorInfo _DynamicMethodCtor;
            private static MethodInfo _GetILGeneratorMethod;
            private static MethodInfo _CreateDelegateMethod;
            private static MethodInfo _EmitFieldMethod;
            private static MethodInfo _EmitOpMethod;
            private static bool _EmitResolved;

            private static bool EnsureEmit()
            {
                if (_EmitResolved)
                {
                    return _DynamicMethodCtor != null;
                }
                _EmitResolved = true;
                try
                {
                    _DynamicMethodType = typeof(OpCode).Assembly.GetType("System.Reflection.Emit.DynamicMethod", false);
                    if (_DynamicMethodType == null)
                    {
                        // Some profiles keep DynamicMethod in a separate assembly
                        // (e.g. Unity Mono's netstandard profile).
                        _DynamicMethodType = Type.GetType("System.Reflection.Emit.DynamicMethod, System.Reflection.Emit.Lightweight", false);
                    }
                    if (_DynamicMethodType == null)
                    {
                        return false;
                    }
                    _DynamicMethodCtor = _DynamicMethodType.GetConstructor(new[] { typeof(string), typeof(Type), typeof(Type[]), typeof(Module), typeof(bool) });
                    _GetILGeneratorMethod = _DynamicMethodType.GetMethod("GetILGenerator", Type.EmptyTypes);
                    _CreateDelegateMethod = _DynamicMethodType.GetMethod("CreateDelegate", new[] { typeof(Type) });
                    if (_GetILGeneratorMethod != null)
                    {
                        var ilType = _GetILGeneratorMethod.ReturnType;
                        _EmitFieldMethod = ilType.GetMethod("Emit", new[] { typeof(OpCode), typeof(FieldInfo) });
                        _EmitOpMethod = ilType.GetMethod("Emit", new[] { typeof(OpCode) });
                    }
                    return _DynamicMethodCtor != null && _GetILGeneratorMethod != null && _CreateDelegateMethod != null
                        && _EmitFieldMethod != null && _EmitOpMethod != null;
                }
                catch
                {
                    return false;
                }
            }

            private static Delegate BuildGetterDelegate(FieldInfo field, Type returnType, bool convertToNativeInt, Type delegateType)
            {
                var dm = _DynamicMethodCtor.Invoke(new object[] { "StaticFieldAddress", returnType, Type.EmptyTypes, typeof(UnsafeEx).Module, true });
                var il = _GetILGeneratorMethod.Invoke(dm, null);
                _EmitFieldMethod.Invoke(il, new object[] { OpCodes.Ldsflda, field });
                if (convertToNativeInt)
                {
                    _EmitOpMethod.Invoke(il, new object[] { OpCodes.Conv_U });
                }
                _EmitOpMethod.Invoke(il, new object[] { OpCodes.Ret });
                return (Delegate)_CreateDelegateMethod.Invoke(dm, new object[] { delegateType });
            }

            private class PlatDepJit : IPlatDep
            {
                private static readonly bool _ByRefGetterSupported = ProbeByRefGetter();
                private static readonly Dictionary<FieldInfo, IFieldGetter> _Getters = new Dictionary<FieldInfo, IFieldGetter>();

                private static bool ProbeByRefGetter()
                {
                    if (!RuntimeInfo.IsDynamicCodeEnabled || !EnsureEmit())
                    {
                        return false;
                    }
                    try
                    {
                        var probeField = typeof(StaticFieldPlatDep).GetField("_ByRefProbeStorage", BindingFlags.Static | BindingFlags.NonPublic);
                        var getter = (RefByteGetter)BuildGetterDelegate(probeField, typeof(byte).MakeByRefType(), false, typeof(RefByteGetter));
                        ref byte r = ref getter();
                        return RawRef.Of(ref r).Address == RawRef.Of(ref _ByRefProbeStorage).Address;
                    }
                    catch
                    {
                        return false;
                    }
                }

                private static IFieldGetter GetGetter(FieldInfo field)
                {
                    lock (_Getters)
                    {
                        IFieldGetter cached;
                        if (_Getters.TryGetValue(field, out cached))
                        {
                            return cached;
                        }
                        EnsureClassConstructor(field);
                        var created = BuildGetter(field);
                        _Getters[field] = created;
                        return created;
                    }
                }

                private static IFieldGetter BuildGetter(FieldInfo field)
                {
                    if (!EnsureEmit())
                    {
                        // EnsureEmit is idempotent; reaching this with a null ctor
                        // would otherwise NRE below.
                        throw new PlatformNotSupportedException("System.Reflection.Emit.DynamicMethod is unavailable on this runtime.");
                    }
                    if (_ByRefGetterSupported)
                    {
                        try
                        {
                            var getter = (RefByteGetter)BuildGetterDelegate(field, typeof(byte).MakeByRefType(), false, typeof(RefByteGetter));
                            return new ByRefFieldGetter(getter);
                        }
                        catch (NotSupportedException) { }
                        catch (ArgumentException) { }
                    }
                    var ptrGetter = (IntPtrGetter)BuildGetterDelegate(field, typeof(IntPtr), true, typeof(IntPtrGetter));
                    return new IntPtrFieldGetter(ptrGetter);
                }

                public ref byte GetStaticFieldAddress(FieldInfo field)
                {
                    return ref GetGetter(field).Get();
                }
                public ref byte GetThreadStaticAddress(FieldInfo field)
                {
                    // ldsflda re-evaluates the current thread's TLS base on every
                    // call. A byref obtained on one thread must NOT be handed to
                    // another thread.
                    return ref GetGetter(field).Get();
                }
            }

            // ------------------------------------------------------------------
            // NativeAOT: dynamic code is unavailable, so the address is rebuilt
            // from the reflection FieldAccessor chain:
            //   RegularStaticFieldAccessor.StaticsBase / FieldOffset / FieldBase
            // FieldBase (Internal.Runtime.FieldTableFlags) selects the storage:
            //   NonGCStatic  -> StaticsBase + FieldOffset
            //   GCStatic     -> RuntimeAugments.LoadReferenceTypeField(StaticsBase)
            //                  yields the GC-statics region object; the field is
            //                  at (object reference + FieldOffset).
            //   ThreadStatic -> RuntimeAugments.GetThreadStaticBase(StaticsBase)
            //                  yields this thread's region object; same offset.
            // The region object is re-read on EVERY call: it is a movable GC
            // object and the TLS one is lazily allocated per thread.
            // ------------------------------------------------------------------
            private class PlatDepNativeAOT : IPlatDep
            {
                private delegate object LoadReferenceTypeFieldDel(IntPtr address);
                private delegate object GetThreadStaticBaseDel(IntPtr cookie);

                private const int StorageNonGC = 1;
                private const int StorageGC = 2;
                private const int StorageThread = 3;

                private sealed class Entry
                {
                    public IntPtr StaticsBase;
                    public int FieldOffset;
                    public int Storage;
                }

                private static readonly Dictionary<FieldInfo, Entry> _Entries = new Dictionary<FieldInfo, Entry>();
                private static readonly LoadReferenceTypeFieldDel _LoadReferenceTypeField;
                private static readonly GetThreadStaticBaseDel _GetThreadStaticBase;

                static PlatDepNativeAOT()
                {
                    var ra = Type.GetType("Internal.Runtime.Augments.RuntimeAugments, System.Private.CoreLib");
                    if (ra == null)
                    {
                        throw new PlatformNotSupportedException("NativeAOT RuntimeAugments is unavailable.");
                    }
                    var miLoad = FindStaticMethod(ra, "LoadReferenceTypeField", typeof(IntPtr));
                    var miTls = FindStaticMethod(ra, "GetThreadStaticBase", typeof(IntPtr));
                    if (miLoad == null || miTls == null)
                    {
                        throw new PlatformNotSupportedException("NativeAOT static field helpers were trimmed; root them through AOTPreserve.");
                    }
                    _LoadReferenceTypeField = (LoadReferenceTypeFieldDel)Delegate.CreateDelegate(typeof(LoadReferenceTypeFieldDel), miLoad);
                    _GetThreadStaticBase = (GetThreadStaticBaseDel)Delegate.CreateDelegate(typeof(GetThreadStaticBaseDel), miTls);
                }

                private static int ResolveStorage(object fieldBase)
                {
                    switch (fieldBase.ToString())
                    {
                        case "NonGCStatic": return StorageNonGC;
                        case "GCStatic": return StorageGC;
                        case "ThreadStatic": return StorageThread;
                    }
                    // Fall back to the numeric enum value on a future runtime.
                    return Convert.ToInt32(fieldBase);
                }

                private static Entry GetEntry(FieldInfo field)
                {
                    lock (_Entries)
                    {
                        Entry cached;
                        if (_Entries.TryGetValue(field, out cached))
                        {
                            return cached;
                        }
                        EnsureClassConstructor(field);
                        var propAccessor = FindProperty(field.GetType(), "FieldAccessor");
                        if (propAccessor == null)
                        {
                            throw new PlatformNotSupportedException("NativeAOT FieldAccessor property is unavailable.");
                        }
                        var accessor = propAccessor.GetValue(field, null);
                        var t = accessor.GetType();
                        var fStaticsBase = FindField(t, "<StaticsBase>k__BackingField");
                        var fFieldOffset = FindField(t, "<FieldOffset>k__BackingField");
                        var pFieldBase = FindProperty(t, "FieldBase");
                        if (fStaticsBase == null || fFieldOffset == null || pFieldBase == null)
                        {
                            throw new PlatformNotSupportedException("NativeAOT RegularStaticFieldAccessor members were trimmed; root them through AOTPreserve.");
                        }
                        var entry = new Entry
                        {
                            StaticsBase = (IntPtr)fStaticsBase.GetValue(accessor),
                            FieldOffset = Convert.ToInt32(fFieldOffset.GetValue(accessor)),
                            Storage = ResolveStorage(pFieldBase.GetValue(accessor, null)),
                        };
                        _Entries[field] = entry;
                        return entry;
                    }
                }

                private static ref byte GetRegionAddress(object region, int fieldOffset)
                {
                    // The GC-statics / TLS region is a movable managed object.
                    // Its address snapshot is untracked until ToRef turns it into
                    // an interior byref, so verify the region did not move in
                    // that window (same double-read retry as UnsafeEx.GetFieldRef).
                    // Once returned, the byref itself is tracked and relocated.
                    for (; ; )
                    {
                        RawRef r = default;
                        r.SetRefObj(region);
                        var regionBase = r.Address;
                        ref byte result = ref UnsafeEx.ToRef<byte>(IntPtr.Add(regionBase, fieldOffset));

                        RawRef check = default;
                        check.SetRefObj(region);
                        if (check.Address == regionBase)
                        {
                            GC.KeepAlive(region);
                            return ref result;
                        }
                        // Region moved materializing the byref; retry with its
                        // current address.
                    }
                }

                private static ref byte GetAddress(FieldInfo field)
                {
                    var entry = GetEntry(field);
                    switch (entry.Storage)
                    {
                        case StorageNonGC:
                            // Non-GC statics live in fixed module data and never move.
                            return ref UnsafeEx.ToRef<byte>(IntPtr.Add(entry.StaticsBase, entry.FieldOffset));
                        case StorageGC:
                            return ref GetRegionAddress(_LoadReferenceTypeField(entry.StaticsBase), entry.FieldOffset);
                        case StorageThread:
                            return ref GetRegionAddress(_GetThreadStaticBase(entry.StaticsBase), entry.FieldOffset);
                        default:
                            throw new PlatformNotSupportedException("Unknown NativeAOT static storage class: " + entry.Storage);
                    }
                }

                public ref byte GetStaticFieldAddress(FieldInfo field)
                {
                    return ref GetAddress(field);
                }
                public ref byte GetThreadStaticAddress(FieldInfo field)
                {
                    return ref GetAddress(field);
                }
            }

            // ------------------------------------------------------------------
            // Unity IL2CPP: no JIT and no runtime internals, but FieldHandle.Value
            // IS the native FieldInfo* ({name@0, type@8, parent@16, offset@24,
            // token@28}) and the official C API exposes the storage:
            //   address = il2cpp_class_get_static_field_data(parent) + offset
            // static_fields is pinned by the GC (AllocateFixed in Class::Init), so
            // the address is stable for the process lifetime. cctor / class init
            // go through RuntimeHelpers.RunClassConstructor in
            // EnsureClassConstructor - il2cpp_class_get_static_field_data itself
            // only reads klass->static_fields.
            //
            // [ThreadStatic] fields (offset == -1) live in per-thread blocks:
            //   Il2CppThread (== Thread.CurrentThread object)
            //     -> [Thread.internal_thread @ reflection offset] InternalThread
            //     -> [static_data @ derived offset] per-thread block-pointer array
            //     -> array[Il2CppClass.thread_static_fields_offset] block (pinned)
            //     -> block + per-field offset (derived per field, verified, cached)
            // None of these native offsets are exposed by the C API, so they are
            // DERIVED at runtime (first TLS use) instead of hardcoded: unique
            // cookies are written into two probe classes' [ThreadStatic] fields
            // and the blocks are located by a bounded pointer walk. A Unity
            // upgrade that reshuffles the native structs therefore either
            // re-derives the chain correctly or fails with
            // PlatformNotSupportedException - it can never return a guessed
            // address. HybridCLR hot-update assemblies implement FieldHandle
            // differently and are NOT covered by either path.
            // ------------------------------------------------------------------
            private class PlatDepIl2Cpp : IPlatDep
            {
                // The il2cpp C API is exported from GameAssembly.dll on Windows
                // IL2CPP players; iOS links everything statically into the app
                // binary, where the same functions are reachable through
                // __Internal (a link-time symbol reference - valid here because
                // the il2cpp_* symbols are defined in the very same binary on
                // both platforms; this is NOT true for libc symbols, which is
                // why the probe layer below never uses __Internal). The binding
                // is picked lazily; a miss on both throws.
                [DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_class_get_static_field_data")]
                private static extern IntPtr il2cpp_class_get_static_field_data_ga(IntPtr klass);
                [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_class_get_static_field_data")]
                private static extern IntPtr il2cpp_class_get_static_field_data_ia(IntPtr klass);
                [DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_thread_current")]
                private static extern IntPtr il2cpp_thread_current_ga();
                [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_thread_current")]
                private static extern IntPtr il2cpp_thread_current_ia();
                [DllImport("GameAssembly", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_field_static_set_value")]
                private static extern void il2cpp_field_static_set_value_ga(IntPtr field, IntPtr value);
                [DllImport("__Internal", CallingConvention = CallingConvention.Cdecl, EntryPoint = "il2cpp_field_static_set_value")]
                private static extern void il2cpp_field_static_set_value_ia(IntPtr field, IntPtr value);

                private const int ApiUnresolved = 0;
                private const int ApiGameAssembly = 1;
                private const int ApiInternal = 2;
                private const int ApiNone = -1;
                private static int _ApiBinding = ApiUnresolved;
                private static readonly object _ApiBindingLock = new object();

                private static void ResolveApiBinding()
                {
                    lock (_ApiBindingLock)
                    {
                        if (_ApiBinding != ApiUnresolved)
                        {
                            return;
                        }
                        try
                        {
                            il2cpp_thread_current_ga();
                            _ApiBinding = ApiGameAssembly;
                            return;
                        }
                        catch (Exception) { }
                        try
                        {
                            il2cpp_thread_current_ia();
                            _ApiBinding = ApiInternal;
                            return;
                        }
                        catch (Exception) { }
                        _ApiBinding = ApiNone;
                    }
                }

                private static IntPtr Il2CppCurrentThread()
                {
                    if (_ApiBinding == ApiUnresolved)
                    {
                        ResolveApiBinding();
                    }
                    switch (_ApiBinding)
                    {
                        case ApiGameAssembly: return il2cpp_thread_current_ga();
                        case ApiInternal: return il2cpp_thread_current_ia();
                        default:
                            throw new PlatformNotSupportedException("il2cpp_thread_current is unreachable (neither GameAssembly nor __Internal).");
                    }
                }

                private static IntPtr Il2CppClassStaticFieldData(IntPtr klass)
                {
                    if (_ApiBinding == ApiUnresolved)
                    {
                        ResolveApiBinding();
                    }
                    switch (_ApiBinding)
                    {
                        case ApiGameAssembly: return il2cpp_class_get_static_field_data_ga(klass);
                        case ApiInternal: return il2cpp_class_get_static_field_data_ia(klass);
                        default:
                            throw new PlatformNotSupportedException("il2cpp_class_get_static_field_data is unreachable (neither GameAssembly nor __Internal).");
                    }
                }

                private static void Il2CppFieldStaticSetValue(IntPtr field, IntPtr value)
                {
                    if (_ApiBinding == ApiUnresolved)
                    {
                        ResolveApiBinding();
                    }
                    switch (_ApiBinding)
                    {
                        case ApiGameAssembly: il2cpp_field_static_set_value_ga(field, value); return;
                        case ApiInternal: il2cpp_field_static_set_value_ia(field, value); return;
                        default:
                            throw new PlatformNotSupportedException("il2cpp_field_static_set_value is unreachable (neither GameAssembly nor __Internal).");
                    }
                }

                // Typed static/TLS write through the official C API
                // (Field::StaticSetValue routes TLS through the CURRENT thread's
                // block - the same block the calibration diffs). The reflection
                // FieldInfo.SetValue does NOT write TLS slots on IL2CPP (verified:
                // the write silently does not appear in the block). Value
                // semantics per Field::SetValueRaw with deref_pointer = false:
                //   reference fields -> value IS the object pointer (null = 0)
                //   IntPtr/UIntPtr   -> value IS the pointer value itself
                //   value types      -> value points at the raw bytes
                private static void SetTlsFieldValueRaw(FieldInfo field, object value)
                {
                    var t = field.FieldType;
                    if (!t.IsValueType)
                    {
                        IntPtr ptr = IntPtr.Zero;
                        if (value != null)
                        {
                            RawRef r = default;
                            r.SetRefObj(value);
                            ptr = r.Address;
                        }
                        Il2CppFieldStaticSetValue(field.FieldHandle.Value, ptr);
                        GC.KeepAlive(value);
                    }
                    else if (t == typeof(IntPtr))
                    {
                        Il2CppFieldStaticSetValue(field.FieldHandle.Value, (IntPtr)value);
                    }
                    else if (t == typeof(UIntPtr))
                    {
                        Il2CppFieldStaticSetValue(field.FieldHandle.Value, (IntPtr)(ulong)(UIntPtr)value);
                    }
                    else
                    {
                        if (value == null)
                        {
                            // Nullable<T> null: the API memsets the footprint.
                            Il2CppFieldStaticSetValue(field.FieldHandle.Value, IntPtr.Zero);
                            return;
                        }
                        RawRef r = default;
                        r.SetRefObj(value);
                        Il2CppFieldStaticSetValue(field.FieldHandle.Value, IntPtr.Add(r.Address, GetObjectHeaderSize()));
                        GC.KeepAlive(value);
                    }
                }

                private sealed class Entry
                {
                    public IntPtr Parent;
                    public int Offset;
                    public bool IsTls;
                }

                private static readonly Dictionary<FieldInfo, Entry> _Entries = new Dictionary<FieldInfo, Entry>();

                private static Entry GetEntry(FieldInfo field)
                {
                    lock (_Entries)
                    {
                        Entry cached;
                        if (_Entries.TryGetValue(field, out cached))
                        {
                            return cached;
                        }
                        EnsureClassConstructor(field);
                        var handle = field.FieldHandle.Value;
                        if (handle == IntPtr.Zero)
                        {
                            throw new PlatformNotSupportedException("IL2CPP FieldHandle does not expose the native FieldInfo pointer.");
                        }
                        var parent = Marshal.ReadIntPtr(handle, 16);
                        var offset = Marshal.ReadInt32(handle, 24);
                        if (parent == IntPtr.Zero)
                        {
                            throw new PlatformNotSupportedException("IL2CPP FieldInfo.parent is unavailable.");
                        }
                        var entry = new Entry
                        {
                            Parent = parent,
                            Offset = offset,
                            // offset == -1 is the internal THREAD_STATIC_FIELD_OFFSET
                            // sentinel. The attribute check is defense in depth: if a
                            // future Unity stops using that sentinel, this keeps TLS
                            // fields from silently resolving to a wrong address.
                            IsTls = offset == -1 || IsThreadStatic(field),
                        };
                        _Entries[field] = entry;
                        return entry;
                    }
                }

                private static bool IsThreadStatic(FieldInfo field)
                {
                    try
                    {
                        return field.IsDefined(typeof(ThreadStaticAttribute), false);
                    }
                    catch
                    {
                        // Attribute metadata can be stripped on IL2CPP; then this
                        // second leg simply stays silent and the offset sentinel
                        // carries the detection.
                        return false;
                    }
                }

                private static ref byte GetAddress(FieldInfo field)
                {
                    var entry = GetEntry(field);
                    if (entry.IsTls)
                    {
                        return ref GetTlsAddress(field, entry);
                    }
                    var staticData = Il2CppClassStaticFieldData(entry.Parent);
                    if (staticData == IntPtr.Zero)
                    {
                        throw new PlatformNotSupportedException("IL2CPP static data is not initialized for the declaring type.");
                    }
                    return ref UnsafeEx.ToRef<byte>(IntPtr.Add(staticData, entry.Offset));
                }

                // --------------------------------------------------------------
                // [ThreadStatic] support: runtime chain derivation.
                // --------------------------------------------------------------
                private static class TlsCalibA
                {
                    [ThreadStatic] internal static long X;
                }
                private static class TlsCalibB
                {
                    [ThreadStatic] internal static long X;
                }
                private const long TlsCalibCookieA = 0x5FA5C0DE5FA51111L;
                private const long TlsCalibCookieB = 0x5FA5C0DE5FA52222L;

                private sealed class TlsChain
                {
                    public int InternalThreadOffset; // inside Il2CppThread
                    public bool StaticDataOnThreadObject; // static_data root: InternalThread or Il2CppThread
                    public int StaticDataOffset;     // inside the root struct
                    public int ClassIndexOffset;     // inside Il2CppClass
                }
                private static TlsChain _TlsChain;
                private static readonly object _TlsChainLock = new object();
                private static readonly Dictionary<FieldInfo, int> _TlsFieldOffsets = new Dictionary<FieldInfo, int>();

                // ---- known-layout fast path (the "structure mapping 脳 runtime
                //      derivation" hybrid). Offsets verified against the
                //      libil2cpp source of 2022.3 (identical in the
                //      HybridCLR-integrated tree):
                //        Il2CppInternalThread.static_data          @ +0x60
                //        Il2CppClass.thread_static_fields_offset   @ +0x114
                //      They are HINTS: every hinted step is re-verified against
                //      the calibration cookies before use, and a mismatch falls
                //      through to the empirical BFS walk - a Unity upgrade that
                //      reshuffles these structs costs calibration speed, never
                //      correctness. ----
                private const int HintStaticDataOffset = 0x60;
                private const int HintClassIndexOffset = 0x114;

                internal static string TlsChainDescription { get; private set; }

                // Verifies the hinted chain end-to-end: static_data @+0x60 on the
                // InternalThread, combined indices @+0x114 on both classes, and
                // the blocks reached through those indices must contain the
                // calibration cookies. Returns null on any mismatch.
                private static TlsChain TryHintedTlsChain(IntPtr it, int itOff, IntPtr klassA, IntPtr klassB, byte[] needleA, byte[] needleB)
                {
                    IntPtr staticData = Marshal.ReadIntPtr(it, HintStaticDataOffset);
                    if (!LooksLikePointer(staticData) || !IsPrivateReadable(staticData, 8))
                    {
                        return null;
                    }
                    if (!IsPrivateReadable(klassA, HintClassIndexOffset + 4)
                        || !IsPrivateReadable(klassB, HintClassIndexOffset + 4))
                    {
                        return null;
                    }
                    int off = HintClassIndexOffset;
                    uint vA = unchecked((uint)Marshal.ReadInt32(klassA, off));
                    uint vB = unchecked((uint)Marshal.ReadInt32(klassB, off));
                    if (vA == vB)
                    {
                        return null; // two distinct classes cannot share one block
                    }
                    IntPtr blockA = TryHintedBlock(staticData, (int)(vA >> 16), (int)(vA & 0xFFFF), needleA);
                    if (blockA == IntPtr.Zero)
                    {
                        return null;
                    }
                    IntPtr blockB = TryHintedBlock(staticData, (int)(vB >> 16), (int)(vB & 0xFFFF), needleB);
                    if (blockB == IntPtr.Zero || blockB == blockA)
                    {
                        return null;
                    }
                    return new TlsChain
                    {
                        InternalThreadOffset = itOff,
                        StaticDataOnThreadObject = false,
                        StaticDataOffset = HintStaticDataOffset,
                        ClassIndexOffset = off,
                    };
                }

                // Follows staticData -> slots[slot] -> data[index] and requires
                // the reached block to contain the cookie. Guarded throughout.
                private static IntPtr TryHintedBlock(IntPtr staticData, int slot, int index, byte[] needle)
                {
                    if (slot >= 1024 || index >= 1024)
                    {
                        return IntPtr.Zero;
                    }
                    IntPtr slotAddr = IntPtr.Add(staticData, slot * IntPtr.Size);
                    if (!IsPrivateReadable(slotAddr, 8))
                    {
                        return IntPtr.Zero;
                    }
                    IntPtr slotPtr = Marshal.ReadIntPtr(slotAddr);
                    if (!LooksLikePointer(slotPtr))
                    {
                        return IntPtr.Zero;
                    }
                    IntPtr dataAddr = IntPtr.Add(slotPtr, index * IntPtr.Size);
                    if (!IsPrivateReadable(dataAddr, 8))
                    {
                        return IntPtr.Zero;
                    }
                    IntPtr block = Marshal.ReadIntPtr(dataAddr);
                    if (!LooksLikePointer(block))
                    {
                        return IntPtr.Zero;
                    }
                    var region = ReadRegion(block, 0x40);
                    if (region == null || IndexOfBytes(region, needle) < 0)
                    {
                        return IntPtr.Zero;
                    }
                    return block;
                }

                private static TlsChain GetTlsChain()
                {
                    lock (_TlsChainLock)
                    {
                        if (_TlsChain != null)
                        {
                            return _TlsChain;
                        }
                        _TlsChain = CalibrateTls();
                        return _TlsChain;
                    }
                }

                private static bool LooksLikePointer(IntPtr v)
                {
                    long x = v.ToInt64();
                    return x >= 0x10000 && x < 0x00007FFFFFFFFFFFL && (x & 7) == 0;
                }

                [StructLayout(LayoutKind.Sequential)]
                private struct MemoryBasicInformation
                {
                    public IntPtr BaseAddress;
                    public IntPtr AllocationBase;
                    public uint AllocationProtect;
                    public IntPtr RegionSize;
                    public uint State;
                    public uint Protect;
                    public uint Type;
                }

                [DllImport("kernel32", SetLastError = true)]
                private static extern int VirtualQuery(IntPtr address, out MemoryBasicInformation info, int length);

                // Apple (iOS/macOS): mach_vm_read_overwrite (libsystem_kernel,
                // re-exported by libSystem) is a PROTECTION-CHECKED read: it fails
                // for unmapped AND PROT_NONE pages. That distinction is fatal on
                // iOS - the whole low 4GB is __PAGEZERO, mapped but PROT_NONE, so
                // a mappedness-only probe (mincore) approves any small garbage
                // value (sizes, packed uint32 pairs read from runtime structs)
                // and the following copy faults inside it (device-verified:
                // EXC_BAD_ACCESS at 0x40000, twice, same address). Bound through
                // the DYNAMIC library name - never "__Internal" (under IL2CPP
                // that becomes a link-time symbol reference and the declaration
                // alone would break Windows IL2CPP builds). dlopen of an
                // already-loaded system library only hits the dyld shared cache -
                // permitted, public SDK API. Inert on non-Apple runtimes.
                [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mach_task_self")]
                private static extern IntPtr mach_task_self_apple();
                [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mach_vm_read_overwrite")]
                private static extern int mach_vm_read_overwrite_apple(IntPtr task, ulong address, ulong size, ulong data, out ulong dataSize);

                // Android/bionic: mincore lives in libc. Both spellings are bound
                // and tried lazily; inert elsewhere (Linux glibc ships neither
                // "libc" nor "libc.so" at runtime, so desktop-IL2CPP simply falls
                // through to the no-probe path).
                [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getpagesize")]
                private static extern int getpagesize_bionic();
                [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mincore")]
                private static extern int mincore_bionic(IntPtr addr, UIntPtr length, byte[] vec);
                [DllImport("libc.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "getpagesize")]
                private static extern int getpagesize_bionic_so();
                [DllImport("libc.so", CallingConvention = CallingConvention.Cdecl, EntryPoint = "mincore")]
                private static extern int mincore_bionic_so(IntPtr addr, UIntPtr length, byte[] vec);

                private const int ProbeUnresolved = 0;
                private const int ProbeWindowsVq = 1;
                private const int ProbeAppleVmRead = 2;
                private const int ProbeBionicLibc = 3;
                private const int ProbeBionicLibcSo = 4;
                private const int ProbeNone = -1;
                private static int _ProbeKind = ProbeUnresolved;
                private static int _PosixPageSize;
                private static readonly object _ProbeLock = new object();

                // Guards the calibration pointer walk: non-pointer integers inside
                // the runtime structs (sizes, ids, packed uint32 pairs) must never
                // be dereferenced - an uncatchable SIGSEGV is the only outcome.
                // Probes are resolved lazily; a platform with NO probe reports
                // everything as unreadable, which disables the TLS feature with
                // an exception instead of running an unguarded walk.
                // Windows: VirtualQuery (protection-aware). Apple: mach_vm_read_
                // overwrite (protection-aware). Bionic: mincore (mappedness only
                // - guard pages count as readable; the mostly-null region filter
                // compensates).
                private static int ResolveProbe()
                {
                    lock (_ProbeLock)
                    {
                        if (_ProbeKind != ProbeUnresolved)
                        {
                            return _ProbeKind;
                        }
                        try
                        {
                            var probe = new byte[16];
                            RawRef r = default;
                            r.SetRefObj(probe);
                            var info = new MemoryBasicInformation();
                            VirtualQuery(r.Address, out info, Marshal.SizeOf(typeof(MemoryBasicInformation)));
                            GC.KeepAlive(probe);
                            _ProbeKind = ProbeWindowsVq;
                            return _ProbeKind;
                        }
                        catch (Exception) { }
                        try
                        {
                            // Apple: verify the protection-checked read path with a
                            // self-read of a managed buffer.
                            IntPtr task = mach_task_self_apple();
                            if (task != IntPtr.Zero)
                            {
                                var probe = new byte[16];
                                var pin = GCHandle.Alloc(probe, GCHandleType.Pinned);
                                try
                                {
                                    ulong dataAddr = (ulong)pin.AddrOfPinnedObject().ToInt64();
                                    ulong read;
                                    int kr = mach_vm_read_overwrite_apple(task, dataAddr, 16, dataAddr, out read);
                                    if (kr == 0 && read == 16)
                                    {
                                        _ProbeKind = ProbeAppleVmRead;
                                        return _ProbeKind;
                                    }
                                }
                                finally
                                {
                                    pin.Free();
                                }
                                GC.KeepAlive(probe);
                            }
                        }
                        catch (Exception) { }
                        try
                        {
                            int ps = getpagesize_bionic();
                            if (ps >= 256 && ps <= 0x100000)
                            {
                                _PosixPageSize = ps;
                                _ProbeKind = ProbeBionicLibc;
                                return _ProbeKind;
                            }
                        }
                        catch (Exception) { }
                        try
                        {
                            int ps = getpagesize_bionic_so();
                            if (ps >= 256 && ps <= 0x100000)
                            {
                                _PosixPageSize = ps;
                                _ProbeKind = ProbeBionicLibcSo;
                                return _ProbeKind;
                            }
                        }
                        catch (Exception) { }
                        _ProbeKind = ProbeNone;
                        return _ProbeKind;
                    }
                }

                private static bool IsPrivateReadable(IntPtr address, int length)
                {
                    int kind = _ProbeKind != ProbeUnresolved ? _ProbeKind : ResolveProbe();
                    switch (kind)
                    {
                        case ProbeWindowsVq:
                            return IsReadableVirtualQuery(address, length);
                        case ProbeAppleVmRead:
                            // The protection-checked read IS the check: it fails
                            // for unmapped and PROT_NONE (e.g. __PAGEZERO) alike.
                            return ReadRegionAppleVm(address, length) != null;
                        case ProbeBionicLibc:
                            return IsReadableMincoreCore(mincore_bionic, address, length);
                        case ProbeBionicLibcSo:
                            return IsReadableMincoreCore(mincore_bionic_so, address, length);
                        default:
                            // No probe on this platform: treat as unreadable so
                            // the caller fails with an exception instead of an
                            // unguarded dereference.
                            return false;
                    }
                }

                // Protection-checked read via mach_vm_read_overwrite. Returns the
                // data on success, null when any byte of the range is unmapped or
                // inaccessible - never faults the process.
                private static byte[] ReadRegionAppleVm(IntPtr address, int length)
                {
                    try
                    {
                        var buffer = new byte[length];
                        var pin = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        try
                        {
                            ulong dataAddr = (ulong)pin.AddrOfPinnedObject().ToInt64();
                            ulong read;
                            int kr = mach_vm_read_overwrite_apple(
                                mach_task_self_apple(), (ulong)address.ToInt64(), (ulong)length, dataAddr, out read);
                            return kr == 0 && read == (ulong)length ? buffer : null;
                        }
                        finally
                        {
                            pin.Free();
                        }
                    }
                    catch (Exception)
                    {
                        return null;
                    }
                }

                private static bool IsReadableVirtualQuery(IntPtr address, int length)
                {
                    try
                    {
                        int offset = 0;
                        while (offset < length)
                        {
                            var info = new MemoryBasicInformation();
                            int size = VirtualQuery(IntPtr.Add(address, offset), out info, Marshal.SizeOf(typeof(MemoryBasicInformation)));
                            if (size == 0)
                            {
                                return false;
                            }
                            if (info.State != 0x1000 // MEM_COMMIT
                                || info.Protect == 0x01 // PAGE_NOACCESS
                                || (info.Protect & 0x100) != 0 // PAGE_GUARD modifier
                                || info.Type != 0x20000) // MEM_PRIVATE
                            {
                                return false;
                            }
                            long region = info.RegionSize.ToInt64();
                            if (region <= 0)
                            {
                                return false;
                            }
                            offset += (int)Math.Min(region, (long)length - offset);
                        }
                        return true;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                private static bool IsReadableMincoreCore(FuncMincore mincore, IntPtr address, int length)
                {
                    try
                    {
                        int pageSize = _PosixPageSize;
                        long start = address.ToInt64() & ~(long)(pageSize - 1);
                        long end = (address.ToInt64() + length + pageSize - 1) & ~(long)(pageSize - 1);
                        int pages = (int)((end - start) / pageSize);
                        if (pages <= 0 || pages > 256)
                        {
                            return false;
                        }
                        var vec = new byte[pages];
                        // mincore returns 0 only when every page in the range is
                        // mapped; ENOMEM (-1) means at least one is not.
                        return mincore(new IntPtr(start), (UIntPtr)(end - start), vec) == 0;
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                }

                private delegate int FuncMincore(IntPtr addr, UIntPtr length, byte[] vec);

                private static TlsChain CalibrateTls()
                {
                    // The pointer walk below is only safe with a readability probe
                    // (VirtualQuery / mincore). Without one, a single non-pointer
                    // value that passes the alignment filter would crash the
                    // process - so the feature is disabled instead.
                    if (ResolveProbe() == ProbeNone)
                    {
                        throw new PlatformNotSupportedException(
                            "No memory readability probe (VirtualQuery/mincore) is available; IL2CPP thread-static support stays disabled rather than walk pointers unguarded.");
                    }

                    // ---- anchor 1: Thread.internal_thread offset (managed metadata,
                    //      stable across Unity versions). ----
                    var fiIT = typeof(Thread).GetField("internal_thread", BindingFlags.NonPublic | BindingFlags.Instance);
                    if (fiIT == null)
                    {
                        throw new PlatformNotSupportedException("Thread.internal_thread is not visible on IL2CPP (stripped?).");
                    }
                    int itOff = Marshal.ReadInt32(fiIT.FieldHandle.Value, 24);
                    if (itOff < 8 || itOff > 0x400)
                    {
                        throw new PlatformNotSupportedException("Unexpected Thread.internal_thread offset: " + itOff);
                    }

                    IntPtr thread = Il2CppCurrentThread();
                    if (!LooksLikePointer(thread))
                    {
                        throw new PlatformNotSupportedException("il2cpp_thread_current returned an implausible pointer.");
                    }
                    IntPtr it = Marshal.ReadIntPtr(thread, itOff);
                    if (!LooksLikePointer(it))
                    {
                        throw new PlatformNotSupportedException("IL2CPP InternalThread pointer is implausible.");
                    }

                    // ---- anchor 2: locate this thread's TLS blocks via unique
                    //      cookies (writing them also forces the blocks to exist).
                    //      Storage shape (libil2cpp vm/Thread.cpp):
                    //        root[staticDataOff] -> ThreadStaticData
                    //        ThreadStaticData.slots[slot] -> ThreadStaticDataSlot
                    //        ThreadStaticDataSlot.data[index] -> block
                    //      where a class' Il2CppClass.thread_static_fields_offset
                    //      encodes (slot = v >> 16, index = v & 0xFFFF). The walk
                    //      below finds the chain empirically (depth 3, guarded),
                    //      and anchor 3 then verifies the encoding against the
                    //      classes' own index fields. ----
                    TlsCalibA.X = TlsCalibCookieA;
                    TlsCalibB.X = TlsCalibCookieB;
                    var needleA = BitConverter.GetBytes(TlsCalibCookieA);
                    var needleB = BitConverter.GetBytes(TlsCalibCookieB);

                    // The two probe classes' Il2CppClass records - used by the
                    // hinted fast path and by the BFS cross-verification alike.
                    var fA = typeof(TlsCalibA).GetField("X", BindingFlags.Static | BindingFlags.NonPublic);
                    var fB = typeof(TlsCalibB).GetField("X", BindingFlags.Static | BindingFlags.NonPublic);
                    IntPtr klassA = Marshal.ReadIntPtr(fA.FieldHandle.Value, 16);
                    IntPtr klassB = Marshal.ReadIntPtr(fB.FieldHandle.Value, 16);

                    // ---- fast path: the known 2022.3 layout, cookie-verified. ----
                    var hinted = TryHintedTlsChain(it, itOff, klassA, klassB, needleA, needleB);
                    if (hinted != null)
                    {
                        _tlsChainDescription = "hint: static_data@0x"
                            + HintStaticDataOffset.ToString("X") + ", classIndex@0x"
                            + HintClassIndexOffset.ToString("X");
                        return hinted;
                    }

                    // ---- fallback: empirical guarded BFS walk (layout drift). ----
                    const int RegionLen = 0x100;
                    const int MaxChains = 8;
                    var chainRootA = new List<IntPtr>();
                    var chainOffA = new List<int[]>();
                    var chainRootB = new List<IntPtr>();
                    var chainOffB = new List<int[]>();

                    foreach (var root in new[] { it, thread })
                    {
                        for (int off1 = 0; off1 + 8 <= RegionLen; off1 += 8)
                        {
                            IntPtr p1 = Marshal.ReadIntPtr(root, off1);
                            if (!LooksLikePointer(p1))
                            {
                                continue;
                            }
                            var r1 = ReadRegion(p1, RegionLen);
                            if (r1 == null)
                            {
                                continue;
                            }
                            // Only follow depth-1 targets that look like the
                            // calloc'd ThreadStaticData array (mostly NULL, a few
                            // pointers). This prunes the Il2CppClass / os::Thread /
                            // managed-object branches, whose packed uint32 pairs
                            // and large integers are exactly the values that pass
                            // the alignment filter and crash the walk - it also
                            // compensates for mincore not checking protection.
                            if (!IsMostlyNullRegion(r1))
                            {
                                continue;
                            }
                            for (int off2 = 0; off2 + 8 <= RegionLen; off2 += 8)
                            {
                                IntPtr p2 = (IntPtr)BitConverter.ToInt64(r1, off2);
                                if (!LooksLikePointer(p2))
                                {
                                    continue;
                                }
                                var r2 = ReadRegion(p2, RegionLen);
                                if (r2 == null)
                                {
                                    continue;
                                }
                                for (int off3 = 0; off3 + 8 <= RegionLen; off3 += 8)
                                {
                                    IntPtr p3 = (IntPtr)BitConverter.ToInt64(r2, off3);
                                    if (!LooksLikePointer(p3))
                                    {
                                        continue;
                                    }
                                    var r3 = ReadRegion(p3, RegionLen);
                                    if (r3 == null)
                                    {
                                        continue;
                                    }
                                    if (chainOffA.Count < MaxChains && IndexOfBytes(r3, needleA) >= 0)
                                    {
                                        chainRootA.Add(root);
                                        chainOffA.Add(new[] { off1, off2, off3 });
                                    }
                                    if (chainOffB.Count < MaxChains && IndexOfBytes(r3, needleB) >= 0)
                                    {
                                        chainRootB.Add(root);
                                        chainOffB.Add(new[] { off1, off2, off3 });
                                    }
                                }
                            }
                        }
                    }
                    if (chainOffA.Count == 0 || chainOffB.Count == 0)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static blocks were not found (layout changed?).");
                    }

                    // ---- anchor 3: Il2CppClass.thread_static_fields_offset. For
                    //      every chain pair sharing the same root and static-data
                    //      offset, the classes must hold the combined
                    //      (slot << 16 | index) value at one common position. ----

                    for (int iA = 0; iA < chainOffA.Count; iA++)
                    {
                        var cA = chainOffA[iA];
                        for (int iB = 0; iB < chainOffB.Count; iB++)
                        {
                            var cB = chainOffB[iB];
                            if (chainRootA[iA] != chainRootB[iB] || cA[0] != cB[0])
                            {
                                continue;
                            }
                            int combinedA = (cA[1] / 8 << 16) | (cA[2] / 8);
                            int combinedB = (cB[1] / 8 << 16) | (cB[2] / 8);
                            if (combinedA == combinedB)
                            {
                                continue;
                            }
                            var posA = FindUint32Positions(klassA, 0x200, combinedA);
                            if (posA.Count == 0)
                            {
                                continue;
                            }
                            var posB = FindUint32Positions(klassB, 0x200, combinedB);
                            int common = -1;
                            int matches = 0;
                            foreach (var pa in posA)
                            {
                                foreach (var pb in posB)
                                {
                                    if (pa != pb)
                                    {
                                        continue;
                                    }
                                    common = pa;
                                    matches++;
                                }
                            }
                            if (matches == 1)
                            {
                                _tlsChainDescription = "bfs: root="
                                    + (chainRootA[iA] == thread ? "Thread" : "InternalThread")
                                    + ", staticData@0x" + cA[0].ToString("X")
                                    + ", classIndex@0x" + common.ToString("X");
                                return new TlsChain
                                {
                                    InternalThreadOffset = itOff,
                                    StaticDataOnThreadObject = chainRootA[iA] == thread,
                                    StaticDataOffset = cA[0],
                                    ClassIndexOffset = common,
                                };
                            }
                        }
                    }
                    throw new PlatformNotSupportedException("IL2CPP thread-static class index position could not be verified (layout changed?).");
                }

                private static IntPtr GetTlsBlock(Entry entry, TlsChain chain)
                {
                    // Il2CppClass.thread_static_fields_offset encodes the block
                    // location: slot = v >> 16, index = v & 0xFFFF.
                    uint v = unchecked((uint)Marshal.ReadInt32(entry.Parent, chain.ClassIndexOffset));
                    int slot = (int)(v >> 16);
                    int index = (int)(v & 0xFFFF);
                    if (slot >= 1024 || index >= 1024)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static index is out of range: " + v);
                    }
                    IntPtr thread = Il2CppCurrentThread();
                    IntPtr root = chain.StaticDataOnThreadObject
                        ? thread
                        : Marshal.ReadIntPtr(thread, chain.InternalThreadOffset);
                    IntPtr staticData = Marshal.ReadIntPtr(root, chain.StaticDataOffset);
                    if (!LooksLikePointer(staticData)
                        || !IsPrivateReadable(staticData, (slot + 1) * IntPtr.Size))
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static data array is unavailable on this thread.");
                    }
                    IntPtr slotPtr = Marshal.ReadIntPtr(staticData, slot * IntPtr.Size);
                    if (!LooksLikePointer(slotPtr)
                        || !IsPrivateReadable(slotPtr, (index + 1) * IntPtr.Size))
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static slot is unavailable (slot=" + slot + ").");
                    }
                    IntPtr block = Marshal.ReadIntPtr(slotPtr, index * IntPtr.Size);
                    if (!LooksLikePointer(block) || !IsPrivateReadable(block, 8))
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static block is unavailable (slot=" + slot + ", index=" + index + ").");
                    }
                    return block;
                }

                private static ref byte GetTlsAddress(FieldInfo field, Entry entry)
                {
                    var chain = GetTlsChain();
                    // Reading the value first also forces this thread's block for
                    // the declaring class to exist (lazy allocation).
                    object current = field.GetValue(null);
                    GC.KeepAlive(current);
                    IntPtr block = GetTlsBlock(entry, chain);
                    int fieldOffset = GetTlsFieldOffset(field, block);
                    return ref UnsafeEx.ToRef<byte>(IntPtr.Add(block, fieldOffset));
                }

                private static int GetTlsFieldOffset(FieldInfo field, IntPtr block)
                {
                    lock (_TlsFieldOffsets)
                    {
                        int cached;
                        if (_TlsFieldOffsets.TryGetValue(field, out cached))
                        {
                            return cached;
                        }
                        int offset = CalibrateTlsFieldOffset(field, block);
                        _TlsFieldOffsets[field] = offset;
                        return offset;
                    }
                }

                // The per-field offset inside the block (normally served from the
                // internal s_ThreadLocalStaticOffsetMap) is derived WITHOUT any raw
                // writes to the block: the field is set to an alternate value through
                // reflection (the runtime's own typed TLS write path - GC-safe by
                // construction), the block is diffed to locate the changed bytes, and
                // the original value is restored the same way.
                //
                // An earlier design verified candidates by writing raw bytes at every
                // byte offset that matched the field's current value, snapshotting and
                // restoring a 0x80 window. Both were GC crash hazards: a 4-byte match
                // could land on half of a neighboring object-reference slot and
                // transiently corrupt it (a concurrent scanner then follows the
                // garbage reference into unmapped memory), and the window spanned
                // NEIGHBORING classes' blocks, restoring stale bytes over slots other
                // threads may have written. Raw writes are eliminated entirely.
                private static int CalibrateTlsFieldOffset(FieldInfo field, IntPtr block)
                {
                    bool isRef = !field.FieldType.IsValueType;
                    int valueSize;
                    try
                    {
                        valueSize = isRef ? IntPtr.Size : Marshal.SizeOf(field.FieldType);
                    }
                    catch (Exception)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static offset calibration does not support non-blittable struct fields (" + field.FieldType + ").");
                    }

                    // Scan extent: every [ThreadStatic] field of the declaring class
                    // consumes at most align8(size) of offset space, so the sum bounds
                    // any field's footprint. The extent stays within the class's OWN
                    // block - neighboring blocks belong to other classes and are
                    // written by other threads.
                    int extent = 0;
                    foreach (var other in field.DeclaringType.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                    {
                        if (!IsThreadStatic(other))
                        {
                            continue;
                        }
                        try
                        {
                            extent += other.FieldType.IsValueType ? Align8(Marshal.SizeOf(other.FieldType)) : IntPtr.Size;
                        }
                        catch (Exception)
                        {
                            extent += 64; // unmeasurable (non-blittable struct): slack
                        }
                    }
                    if (extent < 0x10)
                    {
                        extent = 0x10;
                    }
                    if (extent > 0x100)
                    {
                        extent = 0x100;
                    }
                    // Clamp to what is actually readable (the block sits in a pooled
                    // region; its tail could cross a protection boundary).
                    while (extent >= 8 && !IsPrivateReadable(block, extent))
                    {
                        extent -= 8;
                    }
                    if (extent < valueSize)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static block is not readable for " + field.Name + ".");
                    }

                    object current = field.GetValue(null);
                    object alternate = MakeAlternateValue(field, current);
                    // For struct fields MakeAlternateValue mutates the box it was
                    // given, so re-read the original AFTERWARDS for the restore.
                    object restore = field.GetValue(null);

                    var before = ReadRegion(block, extent);
                    if (before == null)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static block read failed for " + field.Name + ".");
                    }
                    SetTlsFieldValueRaw(field, alternate);
                    var after = ReadRegion(block, extent);
                    // Restore through the typed path before analyzing the diff, so
                    // the original value is back even if the analysis throws.
                    SetTlsFieldValueRaw(field, restore);
                    GC.KeepAlive(current);
                    GC.KeepAlive(alternate);
                    GC.KeepAlive(restore);
                    if (after == null)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static block read failed for " + field.Name + ".");
                    }

                    int first = -1, last = -1;
                    for (int i = 0; i < extent; i++)
                    {
                        if (before[i] != after[i])
                        {
                            if (first < 0)
                            {
                                first = i;
                            }
                            last = i;
                        }
                    }
                    if (first < 0)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static offset calibration failed for " + field.Name + " (diff empty).");
                    }
                    // The changed bytes are a SUBSET of the field footprint (a
                    // bitwise-not alternate flips every byte, but a reference
                    // write only changes the bytes where the two addresses
                    // differ - the top bytes of both are zero). When the whole
                    // footprint changed, first IS the offset; otherwise snap
                    // first down to the slot's natural alignment.
                    int offset;
                    if (last - first + 1 == valueSize)
                    {
                        offset = first;
                    }
                    else
                    {
                        int align = Math.Min(8, valueSize);
                        offset = first & ~(align - 1);
                    }
                    if (offset < 0 || offset + valueSize > extent || first < offset || last >= offset + valueSize)
                    {
                        throw new PlatformNotSupportedException("IL2CPP thread-static offset calibration failed for " + field.Name
                            + " (diff [" + first + "," + last + "] does not fit a " + valueSize + "-byte slot).");
                    }
                    return offset;
                }

                private static int Align8(int v)
                {
                    return (v + 7) & ~7;
                }

                // An alternate value for the diff, chosen so that as many bytes
                // of the field footprint as possible DIFFER (bitwise-not for
                // numerics flips every byte; references can only toggle between
                // a valid object and null - the top address bytes match, the
                // diff analysis snaps those back to the slot alignment).
                private static object MakeAlternateValue(FieldInfo field, object current)
                {
                    var t = field.FieldType;
                    if (!t.IsValueType)
                    {
                        // Reference fields: null is always type-correct; otherwise
                        // construct an instance of the exact field type.
                        if (current != null)
                        {
                            return null;
                        }
                        if (t == typeof(string))
                        {
                            return "sfa-alt";
                        }
                        if (t == typeof(object))
                        {
                            return new object();
                        }
                        try
                        {
                            return Activator.CreateInstance(t);
                        }
                        catch (Exception)
                        {
                            throw new PlatformNotSupportedException("IL2CPP thread-static offset calibration cannot construct an alternate value for " + t + ".");
                        }
                    }
                    if (t == typeof(bool)) return !(bool)current;
                    if (t == typeof(char)) return (char)~(char)current;
                    if (t == typeof(byte)) return (byte)~(byte)current;
                    if (t == typeof(sbyte)) return (sbyte)~(sbyte)current;
                    if (t == typeof(short)) return (short)~(short)current;
                    if (t == typeof(ushort)) return (ushort)~(ushort)current;
                    if (t == typeof(int)) return ~(int)current;
                    if (t == typeof(uint)) return ~(uint)current;
                    if (t == typeof(long)) return ~(long)current;
                    if (t == typeof(ulong)) return ~(ulong)current;
                    if (t == typeof(float)) return -(float)current - 1f;
                    if (t == typeof(double)) return -(double)current - 1d;
                    if (t.IsEnum) return Enum.ToObject(t, ~Convert.ToInt64(current));
                    if (t == typeof(IntPtr)) return (IntPtr)~((IntPtr)current).ToInt64();
                    if (t == typeof(UIntPtr)) return (UIntPtr)~((UIntPtr)current).ToUInt64();
                    var underlying = Nullable.GetUnderlyingType(t);
                    if (underlying != null)
                    {
                        // Nullable<T>: toggle between null and a value.
                        return current == null ? (object)1 : null;
                    }
                    // Blittable struct (non-blittable was rejected by Marshal.SizeOf
                    // above): flip EVERY byte inside the boxed copy - it contains
                    // no references, so this is GC-safe, and the full footprint
                    // shows up in the diff.
                    int size = Marshal.SizeOf(t);
                    RawRef r = default;
                    r.SetRefObj(current);
                    IntPtr data = IntPtr.Add(r.Address, GetObjectHeaderSize());
                    for (int i = 0; i < size; i++)
                    {
                        Marshal.WriteByte(IntPtr.Add(data, i), (byte)(Marshal.ReadByte(IntPtr.Add(data, i)) ^ 0xFF));
                    }
                    GC.KeepAlive(current);
                    return current;
                }

                private static byte[] ReadRegion(IntPtr address, int length)
                {
                    int kind = _ProbeKind != ProbeUnresolved ? _ProbeKind : ResolveProbe();
                    if (kind == ProbeAppleVmRead)
                    {
                        // Protection-checked read primitive (also rejects the
                        // PROT_NONE __PAGEZERO low 4GB on iOS).
                        return ReadRegionAppleVm(address, length);
                    }
                    if (!IsPrivateReadable(address, length))
                    {
                        return null;
                    }
                    var buffer = new byte[length];
                    Marshal.Copy(address, buffer, 0, length);
                    return buffer;
                }

                // The ThreadStaticData root is IL2CPP_CALLOC'd: within its first
                // 0x100 only slots[0] is set, the rest is NULL. Live C++ objects
                // (os::Thread, Il2CppClass, managed instances) have dozens of
                // non-null fields, so this filter matches exactly the calloc'd
                // array shape. Threshold allows up to 8 set slots for margin.
                private static bool IsMostlyNullRegion(byte[] region)
                {
                    int nonNull = 0;
                    for (int off = 0; off + 8 <= region.Length; off += 8)
                    {
                        if (BitConverter.ToInt64(region, off) != 0)
                        {
                            nonNull++;
                            if (nonNull > 8)
                            {
                                return false;
                            }
                        }
                    }
                    return true;
                }

                private static int IndexOfBytes(byte[] haystack, byte[] needle)
                {
                    for (int i = 0; i + needle.Length <= haystack.Length; i++)
                    {
                        if (BytesMatchAt(haystack, i, needle))
                        {
                            return i;
                        }
                    }
                    return -1;
                }

                private static bool BytesMatchAt(byte[] buffer, int offset, byte[] needle)
                {
                    for (int i = 0; i < needle.Length; i++)
                    {
                        if (buffer[offset + i] != needle[i])
                        {
                            return false;
                        }
                    }
                    return true;
                }

                private static List<int> FindUint32Positions(IntPtr region, int length, int value)
                {
                    var result = new List<int>();
                    if (!IsPrivateReadable(region, length))
                    {
                        return result;
                    }
                    for (int off = 0; off + 4 <= length; off += 4)
                    {
                        if (Marshal.ReadInt32(region, off) == value)
                        {
                            result.Add(off);
                        }
                    }
                    return result;
                }

                public ref byte GetStaticFieldAddress(FieldInfo field)
                {
                    return ref GetAddress(field);
                }
                public ref byte GetThreadStaticAddress(FieldInfo field)
                {
                    return ref GetAddress(field);
                }
            }

            private class PlatDepUnsupported : IPlatDep
            {
                public ref byte GetStaticFieldAddress(FieldInfo field)
                {
                    throw new PlatformNotSupportedException("Static field addresses are not implemented for " + RuntimeInfo.Runtime + " yet.");
                }
                public ref byte GetThreadStaticAddress(FieldInfo field)
                {
                    throw new PlatformNotSupportedException("Thread-static field addresses are not implemented for " + RuntimeInfo.Runtime + " yet.");
                }
            }
        }
    }
}
