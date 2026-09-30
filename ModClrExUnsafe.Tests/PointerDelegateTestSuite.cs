// PointerDelegateTestSuite.cs
// 自包含测试套件：Unity 与 dotnet 间直接拷贝同步（C# 9 / netstandard2.0 兼容，依赖 PointerDelegate.dll；
// byref/位模式操作用 ModClrExUnsafe 全家桶的 RawRef / Ref.RefEquals，不依赖 System.Runtime.CompilerServices.Unsafe）。
// Unity 用法：PointerDelegateTestSuite.RunAll();  （可选：gcStressMs 时长，parallelGC 并行压测）
// dotnet 侧：由 PointerDelegateTests.cs 以 xUnit 包装运行。
//
// API 版本注意：本套件按 2026/9/22 换代后的 PointerDelegate.dll API 编写
//   （ref R Invoke<P...>(out R, in P...) 主形态 + 非泛型 R Invoke(U...) + 便利包装 R Invoke<P>(in P...)；
//    FreeFunc 构造收 Func<U...,R>/Action<U...>；ByRefParam 为 struct {IntPtr _Address}；
//    U=ByRefParam → 参数传调用方变量地址；R=ByRefParam → ref 返回直接是目标 byref，
//    还原用 RawRef 一层链路：RawRef.Of(ref ret).GetRef<int>()（Unity 侧）；
//    out 槽 _Address = 目标 byref 地址（备用还原路径）。
//    ⚠ 便利包装 Invoke<P>(in P) 对 R=ByRefParam 是 ldobj 值语义（_Address=目标内存头8字节），勿用。
//    dll 为运行时 Reflection.Emit 架构（AssemblyRefs 含 Emit 三件套）——IL2CPP 上 Emit 不可用，
//    泛型 Invoke 走 fallback → 非泛型织入 Invoke（双约定分支），链路仍然完整。）
//
// 平台预期（2026-09-23 dll 修复后更新）：
//  - CoreCLR / Mono JIT：fnptr 路径（PointerFunc）走 MethodHandle.GetFunctionPointer（真代码入口），
//    managed calli（默认构造）+ unmanaged calli（(fn, true) + GetFunctionPointerForDelegate wrapper）双路径可测。
//  - Unity IL2CPP：**dll 侧 unmanaged calli 修复已落地**（2026-09-23）——
//    Generator 织入的 Invoke 按 _IsUnmanaged 运行时分支：false → managed calli（0x00），
//    true → unmanaged calli（0x09，delegate* unmanaged 同款编码）→ 转译器生成纯间接调用，
//    不再把 _Pfn 当 RuntimeMethod* 解引用。wrapper 获取链已验证可行
//    （MonoPInvokeCallback + GetFunctionPointerForDelegate → ReversePInvokeWrapper = unmanaged thunk），
//    因此 IL2CPP 下所有 PointerFunc 经 MakePF 工厂以 (fn, true) 构造，走 unmanaged calli。
//    s_il2cppCalliFixed 已置 true（wrapper 路径恢复）。
//  - ref 返回（RefReturnOfRef）：经 ref 返回 delegate（C# 7.0+ 合法，签名与目标完全一致）
//    拿 wrapper——纯 blittable 透传，calli 的 ByRefParam 形态 ABI 完全匹配。
//  - object/string 参数（TypeNameOf）：非 blittable，wrapper 会做 COM/char* marshaling →
//    经 IntPtr 转发函数（TypeNameOfCompatImpl，全 blittable 透传 + RawRef 还原引用）。
//
// 已知风险（不在默认套件内验证，Unity 侧可手动实验）：
//  - ref 返回（ref ByRefParam ret）是 GC 可追踪的栈上 byref（CoreCLR 精确 GC 会随对象搬移平移，
//    保守 GC 不搬移）——但不要把 ret 装箱/存入堆字段后跨 GC 使用（ByRefParam 含裸 IntPtr，
//    装箱后 GC 不修正 _Address）。正确用法是 Invoke 后立即还原使用，不长期持有。

using AOT;
using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Reflection.Emit;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Unity 侧（UNITY_EDITOR / UNITY_STANDALONE）：using AOT 解析到 UnityEngine 的真实
// MonoPInvokeCallbackAttribute（IL2CPP ReversePInvokeWrapper 生成开关）。
// dotnet 侧（本 Tests 工程）：下方条件编译提供同名空替身——纯标记、无运行时效果，
// 使本文件可在两个环境间直接拷贝。其他 Unity 平台（iOS/Android 等）需扩展条件列表。
#if !UNITY_EDITOR && !UNITY_STANDALONE
namespace AOT
{
    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class MonoPInvokeCallbackAttribute : System.Attribute
    {
        public MonoPInvokeCallbackAttribute(System.Type delegateType) { }
    }
}
#endif

namespace ModClrExUnsafe.Tests
{
    public static unsafe class PointerDelegateTestSuite
    {
        // ---------------- 目标函数群（public static 防 IL2CPP strip） ----------------
        // IL2CPP：ReversePInvokeWrapper 是 AOT 编译期静态生成的（PlatformInvoke.cpp:447——
        //   未标注方法查 wrapper 表为 NULL → NotSupportedException）。
        //   必须标 [MonoPInvokeCallback(typeof(对应Del))] 才会生成 wrapper 条目，
        //   GetFunctionPointerForDelegate 才能拿到真入口。Editor Mono 下此 attribute 无害（纯标记）。
        // RefReturnOfRef 不标：delegate 无 ref 返回形态，IL2CPP 上 SKIP（硬限制）。

        [MonoPInvokeCallback(typeof(AddIntsDel))]
        public static int AddInts(int a, int b) => a + b;
        [MonoPInvokeCallback(typeof(AddFourDel))]
        public static int AddFour(int a, int b, int c, int d) => a + b + c + d;
        [MonoPInvokeCallback(typeof(EchoIntPtrDel))]
        public static IntPtr EchoIntPtr(IntPtr p) => p;
        [MonoPInvokeCallback(typeof(IncrByRefIntDel))]
        public static int IncrByRefInt(ref int p) => ++p;
        [MonoPInvokeCallback(typeof(ReadRefCookieDel))]
        public static IntPtr ReadRefCookie(ref IntPtr p) => p;
        [MonoPInvokeCallback(typeof(RefReturnDel))]
        public static ref int RefReturnOfRef(ref int p) => ref p;
        [MonoPInvokeCallback(typeof(EnumToIntDel))]
        public static int EnumToInt(E8 e) => (int)e;
        public static string TypeNameOf(object o) => o.GetType().Name;
        // G1 专用（2026-09-24 修正轮）：managed fnptr 的 string 返回目标——
        // G1 原版用 GetFunctionPointerForDelegate(string 返回) 是非法组合（见 G 段注释）
        public static string ReturnG1() => "G1";
        // ---- 2026-09-23 同步新增（对应 PlatformCompatTest 的 unmanaged/void/mixed 用例） ----
        [MonoPInvokeCallback(typeof(IncIntDel))]
        public static int IncInt(int x) => x + 1;
        [MonoPInvokeCallback(typeof(NoParamVoidDel))]
        public static void NoParamVoid() { }
        [MonoPInvokeCallback(typeof(WriteIntPtrDel))]
        public static unsafe void WriteIntPtr(IntPtr p)
        {
            *(IntPtr*)p = (IntPtr)0x1234;
        }
        [MonoPInvokeCallback(typeof(RefPlusValDel))]
        public static unsafe int RefPlusVal(IntPtr p1, int p2)
        {
            return checked((int)(*(IntPtr*)p1).ToInt64()) + p2;
        }

        // TypeNameOf 的 IntPtr 转发（IL2CPP 专用）：object/string 非 blittable，直接按
        // string(object) 签名生成 wrapper 会做 COM/char* marshaling，与 managed calli 不匹配。
        // 转发函数全 IntPtr 签名（blittable 透传），内部经 RawRef 还原引用：
        //   IntPtr(=对象指针) → ref object → TypeNameOf → string → 对象指针返回。
        // E2 的 PointerFunc<string,object> calli 侧：传 Dog 引用(=指针) ✓ / 读返回 8 字节为 string 引用 ✓。
        [MonoPInvokeCallback(typeof(TypeNameOfCompatDel))]
        static unsafe IntPtr TypeNameOfCompatImpl(IntPtr o)
        {
            ref object robj = ref RawRef.Of(o).GetRef<object>();
            string name = TypeNameOf(robj);
            return RawRef.Of(ref name).GetValue<IntPtr>();
        }

        // 目标方法体内强制 GC：验证 cooperative calli 下安全点协议完整覆盖被调方。
        [MonoPInvokeCallback(typeof(AllocAndGcDel))]
        public static int AllocAndGc(int n)
        {
            long acc = 0;
            for (int i = 0; i < n; i++)
            {
                var junk = new byte[16];
                junk[0] = (byte)i;
                acc += junk[0];
            }
            GC.Collect(0, GCCollectionMode.Forced, true, true);
            GC.Collect(1, GCCollectionMode.Forced, true, true);
            return (int)(acc & 0x7FFFFFFF) | 1;
        }

        public enum E8 { A = 1, B = 7 }
        public sealed class Slot { public IntPtr Addr; }
        public class Animal { }
        public sealed class Dog : Animal { }

        // ---------------- IL2CPP 探测 + wrapper 路径 ----------------
        // 行为探测（类型存在性不可靠：AOT.MonoPInvokeCallbackAttribute 在 Editor Mono 下也有）：
        // IL2CPP 上对泛型实例化类型的方法拿 function pointer 会抛 NotSupportedException
        // （共享代码 + MethodInfo 尾参数，无单地址可给）；Mono/.NET JIT 上成功返回真代码入口。
        static readonly bool s_isIL2CPP = CheckIsIl2Cpp();

// 2026-09-24 HybridCLR detection (user's semantics): Assembly.Load(empty bytes)
        // throwing NotSupportedException = pure IL2CPP (Load not implemented); any other
        // exception (BadImageFormat etc.) or success = Load IS implemented - the empty
        // bytes are merely invalid assembly content, and dynamic-load capability existing
        // means HybridCLR. (The previous version inverted this - treating "other exceptions"
        // as false - which is why the guard never fired on HybridCLR.)
        // Combined with s_isIL2CPP: HybridCLR only exists on top of IL2CPP - Mono also
        // supports Load(byte[]) and would false-positive, excluded by the IL2CPP precondition.
        static readonly bool s_isHybridClr = s_isIL2CPP && ProbeIsHybridClr();
        static bool ProbeIsHybridClr()
        {
            try { System.Reflection.Assembly.Load(Array.Empty<byte>()); }
            catch (NotSupportedException) { return false; }   // pure IL2CPP: Load not implemented
            catch { }                                          // Load implemented - empty bytes are just invalid content
            return true;
        }

        // 2026-09-24 IL2CPP 实锤（Player.log 崩溃堆栈 + il2cppOutput Generics__57.cpp）：
        // IL2CPP 把泛型方法里的 calli 编译成 il2cpp_codegen_get_direct_method_pointer(RuntimeMethod*)
        // ——_Pfn 被当成元数据指针解引用 → native 崩。PointerFunc 的织入 calli 路径
        // （InvokePlain，含 unmanaged callsite）在 IL2CPP 全部不可用；dll 侧 delegate 路径待修。
        // 2026-09-28 R19 更新：根因 = 织入 callsite 用了 0x09（保留值）——Roslyn 对
        // delegate* unmanaged[Cdecl] 实际写 0x01 (C)。conv 修复后织入 unmanaged calli
        // 在 IL2CPP = 真 CDECL 直调（O 段全 PASS 实锤）；managed calli 仍走 delegate 路径。
        const string Il2CppCalliSkipReason = "IL2CPP: calli in generic code compiled as RuntimeMethod* dereference — fnptr crashes natively (pre-R19 dll; conv=1/Cdecl fix landed 2026-09-28)";

        static bool CheckIsIl2Cpp()
        {
            try
            {
                var pfunc = typeof(List<int>).GetMethod("Clear").MethodHandle.GetFunctionPointer();
                return false;
            }
            catch (NotSupportedException)
            {
                return true;
            }
        }

        // 静态 delegate 字段：① 方法组转换产生静态引用，防 IL2CPP 裁剪；
        // ② delegate 实例保活（GetFunctionPointerForDelegate 契约：指针活则 delegate 必须活）。
        public delegate int AddIntsDel(int a, int b);
        public delegate int AddFourDel(int a, int b, int c, int d);
        public delegate IntPtr EchoIntPtrDel(IntPtr p);
        public delegate int IncrByRefIntDel(ref int p);
        public delegate IntPtr ReadRefCookieDel(ref IntPtr p);
        public delegate int AllocAndGcDel(int n);
        public delegate int EnumToIntDel(E8 e);
        // ref 返回 delegate（C# 7.0+ 合法）：签名与目标完全一致 → wrapper 纯 blittable 透传。
        // ⚠ IntPtr 兼容 delegate（方案a）无法绑定 ref 返回方法（CS0407 方法组转换类型检查），
        //   方案b（ref 返回 delegate）是唯一正解。
        public delegate ref int RefReturnDel(ref int p);
        // TypeNameOf 的 IntPtr 转发 delegate（全 blittable，wrapper 透传）
        public delegate IntPtr TypeNameOfCompatDel(IntPtr o);
        // ---- 2026-09-23 同步新增 ----
        public delegate int IncIntDel(int x);
        public delegate void NoParamVoidDel();
        public delegate void WriteIntPtrDel(IntPtr p);
        public delegate int RefPlusValDel(IntPtr p1, int p2);

        static readonly AddIntsDel s_dAddInts = AddInts;
        static readonly AddFourDel s_dAddFour = AddFour;
        static readonly EchoIntPtrDel s_dEchoIntPtr = EchoIntPtr;
        static readonly IncrByRefIntDel s_dIncrByRefInt = IncrByRefInt;
        static readonly ReadRefCookieDel s_dReadRefCookie = ReadRefCookie;
        static readonly AllocAndGcDel s_dAllocAndGc = AllocAndGc;
        static readonly EnumToIntDel s_dEnumToInt = EnumToInt;
        static readonly RefReturnDel s_dRefReturn = RefReturnOfRef;
        static readonly TypeNameOfCompatDel s_dTypeNameOfCompat = TypeNameOfCompatImpl;
        // ---- 2026-09-23 同步新增 ----
        static readonly IncIntDel s_dIncInt = IncInt;
        static readonly NoParamVoidDel s_dNoParamVoid = NoParamVoid;
        static readonly WriteIntPtrDel s_dWriteIntPtr = WriteIntPtr;
        static readonly RefPlusValDel s_dRefPlusVal = RefPlusVal;

        // ---------------- MakePF 工厂（2026-09-23 同步新增） ----------------
        // IL2CPP 下 TryGetFnPtr 返回的是 ReversePInvokeWrapper（unmanaged thunk），
        // 必须以 unmanaged=true 构造 → 织入 Invoke 走 unmanaged calli（0x09）分支；
        // JIT 下 FnOf 返回托管代码入口，默认构造走 managed calli（0x00）。
        // 泛型 Invoke 在 IL2CPP 上 Emit 不可用 → fallback → 非泛型织入 Invoke →
        // 同样按 _IsUnmanaged 分支，链路一致。
        static PointerFunc<R> MakePF<R>(IntPtr fn)
        {
            return new PointerFunc<R>(fn);
        }
        static PointerFunc<R, U1> MakePF<R, U1>(IntPtr fn)
        {
            return new PointerFunc<R, U1>(fn);
        }
        static PointerFunc<R, U1, U2> MakePF<R, U1, U2>(IntPtr fn)
        {
            return new PointerFunc<R, U1, U2>(fn);
        }
        static PointerFunc<R, U1, U2, U3, U4> MakePF<R, U1, U2, U3, U4>(IntPtr fn)
        {
            return new PointerFunc<R, U1, U2, U3, U4>(fn);
        }

        // ---------------- 结果收集 ----------------

        public sealed class Report
        {
            public int Pass, Fail, Skip;
            public readonly List<string> Failures = new List<string>();
            public bool AllPassed => Fail == 0;
        }

        static Report s_rep;
        static Action<string> s_log;

        static void Check(string name, bool cond, string detail = null)
        {
            if (cond) { s_rep.Pass++; Log($"  [PASS] {name}"); }
            else
            {
                s_rep.Fail++;
                string msg = $"  [FAIL] {name}" + (detail != null ? $" ({detail})" : "");
                s_rep.Failures.Add(msg);
                Log(msg);
            }
        }

        static void Skip(string name, string reason)
        {
            s_rep.Skip++;
            Log($"  [SKIP] {name} — {reason}");
        }

        // 双通道 Log（2026-09-23）：Debug.Log 异步——native 崩溃时未 flush 的输出全丢
        // （多轮实锤：崩点后 2-3 段的 PASS 输出丢失）。同步落盘保证崩了也拿到已跑结果。
        static void Log(string s)
        {
            try { System.IO.File.AppendAllText("pointerdiag.txt", s + "\r\n"); } catch { }
            (s_log ?? Console.WriteLine).Invoke(s);
        }

        // AppDomain.DefineDynamicAssembly 在 net5+（CoreCLR）被移除——静态
        // AssemblyBuilder.DefineDynamicAssembly 等价替换（Run/RunAndCollect 支持；
        // RunAndSave 在 Core 抛 PNSE，由各探针自身的 try/catch 记录）。
        // Unity（netstandard2.1 profile）与 net472 走 AppDomain 老路。
        static System.Reflection.Emit.AssemblyBuilder DefineDynamicAssemblyCompat(
            string name, System.Reflection.Emit.AssemblyBuilderAccess access)
        {
#if NET5_0_OR_GREATER
            return System.Reflection.Emit.AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(name), access);
#else
            return AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(name), access);
#endif
        }

        // SKIP 原因增强：附带最近一次 fnptr 获取异常（IL2CPP wrapper 缺失时直接可见
        // "NotSupportedException: To marshal a managed method, please add MonoPInvokeCallback..."）
        static string FnPtrSkipReason(string what) =>
            s_lastFnPtrError != null
                ? $"{what} — {s_lastFnPtrError.GetType().Name}: {s_lastFnPtrError.Message}"
                : what;

        // ---------------- fnptr 获取（平台自适应） ----------------
        // CoreCLR / Mono JIT：MethodHandle.GetFunctionPointer() 返回真代码入口（managed）。
        // IL2CPP：同 API 返回 MethodInfo*（数据地址，calli 必崩）——改走
        //   Marshal.GetFunctionPointerForDelegate(静态 delegate) 拿 ReversePInvokeWrapper
        //   （native 可调用真入口 = unmanaged thunk → MakePF 以 (fn, true) 构造）。
        // RefReturnOfRef（ref 返回）经 ref 返回 delegate 拿 wrapper（签名一致，blittable 透传）。

        // 最近一次 fnptr 获取失败的异常（SKIP 消息引用，避免空 catch 吞错难排查）
        static Exception s_lastFnPtrError;

        // 2026-09-23：dll 侧 unmanaged calli 修复已落地（Generator 织入 _IsUnmanaged
        // 运行时分支 managed(0x00)/unmanaged(0x09) calli；Emit 路径 Cdecl）→ 置 true
        // 恢复 IL2CPP wrapper 路径（PointerFunc 经 MakePF 全部走 unmanaged calli）。
        const bool s_il2cppCalliFixed = true;

// PF-0: IL2CPP MethodHandle chain probe (2026-09-24). TryGetFnPtr now returns
        // MethodHandle.GetFunctionPointer (a RuntimeMethod* on IL2CPP) which matches the
        // calli translation model. Minimal verdict: PASS -> full PointerFunc sections run;
        // native crash -> process dies with pointerdiag already holding the FreeFunc results
        // (D section runs before this probe).
        static bool s_il2cppPfWorks;

        static void Section_PF0_IL2CppMethodHandleProbe()
        {
            if (!s_isIL2CPP) return;
            Log("-- PF-0. IL2CPP MethodHandle.GetFunctionPointer probe --");
            try
            {
                if (!TryGetFnPtr(nameof(IncInt), out var fn))
                {
                    Skip("PF-0 IL2CPP managed calli", FnPtrSkipReason("GetFunctionPointer failed"));
                    return;
                }
                var inv = new PointerFunc<int, int>(fn);   // managed calli (MakePF unified)
                int r = inv.Invoke(41);
                Check("PF-0 IL2CPP managed calli via MethodHandle: r == 42", r == 42);
                s_il2cppPfWorks = true;
                Log("  PF-0 PASS - IL2CPP PointerFunc chain live, running full sections");
            }
            catch (Exception ex)
            {
                Log($"  PF-0: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // P-1 (2026-09-29 user hypothesis): on IL2CPP, MethodHandle.GetFunctionPointer()
        // throws NotSupportedException (PF-0 verdict), BUT MethodHandle.Value returns the
        // raw RuntimeMethod* — which is exactly what IL2CPP's managed-calli translation
        // expects (it dereferences pfn as RuntimeMethod* and passes it back as the
        // trailing method arg). If this works, the managed-calli path comes alive on
        // IL2CPP without any thunk. Target = IncInt (an AOT-side method of this suite's
        // host type when built via HybridCLR; hot-update when pure-interpreted).
        static void Section_P1_MethodHandleValueProbe()
        {
            if (!s_isIL2CPP) return;
            Log("-- P-1. IL2CPP MethodHandle.Value as managed-calli fnptr --");
            try
            {
                var mi = typeof(PointerDelegateTestSuite).GetMethod(nameof(IncInt),
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                IntPtr fn = mi.MethodHandle.Value;   // RuntimeMethod* on IL2CPP
                Log($"  P-1 MethodHandle.Value = 0x{fn.ToString("X")}");
                var inv = new PointerFunc<int, int>(fn);   // managed calli (unmanaged=false)
                int r = inv.Invoke(41);
                Check("P-1 IL2CPP managed calli via MethodHandle.Value: r == 42", r == 42);
                Log("  P-1 PASS - managed-calli path live on IL2CPP via RuntimeMethod*");
            }
            catch (Exception ex)
            {
                Log($"  P-1: {ex.GetType().Name}: {ex.Message}");
            }
        }
// ---------------- N. IL2CPP native call final verdict (user's two questions) ----------------
        // Q1: can GetDelegateForFunctionPointer bind generic Func<> (even AOT-instantiated)?
        //   Build-output evidence (Il2CppInteropDataTable.cpp): 140 DelegatePInvokeWrapper
        //   entries in the 480-entry interopData table are ALL non-generic; Func_2/Action_1
        //   forms: 0 entries - il2cpp.exe hard-skips wrapper generation for generic delegates.
        //   N-2 is the runtime final check (reflection icall bypassing the managed layer).
        // Q2: does NativePointerFunc<T> (generic class + delegate* unmanaged) support generic
        //   parameters? Prior evidence (generic METHOD form): concrete instantiation = real
        //   native CDECL indirect call; fully-shared fallback = NotSupportedException +
        //   translator dead-code 'return L_4' bug -> C2065 build failure. N-1 is the generic
        //   CLASS build verdict: pure-IL2CPP build failure (C2065) = generic class with fnptr
        //   is a build dead-end; build success = only concrete instantiations are usable.
        private unsafe static class NativePointerFunc<T> where T : unmanaged
        {
            public static T Call(IntPtr fn, T arg) => ((delegate* unmanaged[Cdecl]<T, T>)fn)(arg);
        }

        static unsafe void Section_N_NativeCallVerdict()
        {
            Log("-- N. IL2CPP native-call final verdict (user's two questions) --");

            // N-1: generic-class fnptr - concrete instantiation runtime check
            // (only reachable if the pure-IL2CPP build succeeded)
            try
            {
                if (Type.GetType("Mono.Runtime") != null)
                {
                    Log("  N-1: SKIPPED on Mono - generic-class fnptr calli = A5a-family JIT crash risk (IL2CPP build verdict is the goal)");
                }
                else
                {
                    var thunk = Marshal.GetFunctionPointerForDelegate(s_dIncInt);
                    int r = NativePointerFunc<int>.Call(thunk, 41);
                    Check("N-1 NativePointerFunc<int> generic-class fnptr: r == 42", r == 42);
                }
            }
            catch (Exception ex)
            {
                Log($"  N-1: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
            }

            // N-2: generic Func binding final check - reflection icall
            // (bypasses the managed-layer IsGenericType check; native layer only looks at
            //  interopData wrapper - build output already proves Func_2 has none)
            try
            {
                if (Type.GetType("Mono.Runtime") != null)
                {
                    Log("  N-2: SKIPPED on Mono - same icall + generic Func SIGSEGV as M-B4b (2026-09-24)");
                }
                else
                {
                    var thunk = Marshal.GetFunctionPointerForDelegate(s_dIncInt);
                    var icall = typeof(Marshal).GetMethod("GetDelegateForFunctionPointerInternal",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (icall == null)
                    {
                        Log("  N-2: icall not found (managed layer differs on this runtime)");
                    }
                    else
                    {
                        var del = (Func<int, int>)icall.Invoke(null, new object[] { thunk, typeof(Func<int, int>) });
                        int r = del(41);
                        Log($"  N-2 icall-bypass + generic Func<int,int>: {r} " +
                            $"{(r == 42 ? "(SUPPORTED - wrapper exists after all)" : "(WRONG VALUE)")}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"  N-2: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
            }

            // N-3: non-generic delegate baseline (known PASS on pure IL2CPP)
            try
            {
                var thunk = Marshal.GetFunctionPointerForDelegate(s_dIncInt);
                var d2 = (MIntIntDelegate)Marshal.GetDelegateForFunctionPointer(thunk, typeof(MIntIntDelegate));
                int r = d2(41);
                Check("N-3 non-generic delegate baseline: r == 42", r == 42);
            }
            catch (Exception ex) { Log($"  N-3: {ex.GetType().Name}: {ex.Message}"); }
        }
// ---------------- O. AOT-bridge fnptr via PointerFunc unmanaged calli (2026-09-28) ----------------
        // Step 2 of the IL2CPP native-call plan: the fp comes from the AOT bridge
        // (AotNativeBridge in Assembly-CSharp — its DllImport declarations are AOT,
        // so the factory works on HybridCLR too), and the call goes through
        // PointerFunc's UNMANAGED mode ((pfn, true) constructor -> Cdecl calli).
        // End-to-end chain: hot-update code -> AOT DllImport factory -> native
        // fnptr -> PointerFunc unmanaged calli -> real C function (PointerNativeLib).
        static void Section_O_AotBridgeCalli()
        {
            Log("-- O. AOT-bridge fnptr via PointerFunc unmanaged calli --");

            // O-0: bridge reachable from hot-update code? (AOT assembly, reflection)
            var bridge = Type.GetType("AotNativeBridge, Assembly-CSharp");
            if (bridge == null)
            {
                Skip("O.*", "AotNativeBridge (Assembly-CSharp, AOT) not found - Assets/AotNativeBridge.cs missing?");
                return;
            }

            // O-1: baseline - direct DllImport call through the bridge (verifies the
            //     dll loads + the AOT pinvoke chain works, before any calli).
            try
            {
                int r = (int)bridge.GetMethod("AddInt").Invoke(null, new object[] { 20, 22 });
                Check("O-1 bridge baseline AddInt(20,22): r == 42", r == 42);
            }
            catch (Exception ex) { Log($"  O-1: {ex.GetType().Name}: {ex.Message}"); }

            // O-2: THE core test - factory fp (nf_add_int) -> PointerFunc unmanaged
            //     calli, non-generic Invoke (Emit-first trampoline path).
            try
            {
                IntPtr fp = (IntPtr)bridge.GetMethod("GetAddIntFn").Invoke(null, null);
                Log($"  O-2 GetAddIntFn fp = 0x{fp.ToString("X")}");
                var inv = new PointerFunc<int, int, int>(fp, true);
                int r = inv.Invoke(20, 22);
                Check("O-2 PF<int,int,int> unmanaged calli: 20+22 == 42", r == 42);
            }
            catch (Exception ex) { Log($"  O-2: {ex.GetType().Name}: {ex.Message}"); }

            // O-3: same fp, generic Invoke (Emit-first path).
            try
            {
                IntPtr fp = (IntPtr)bridge.GetMethod("GetAddIntFn").Invoke(null, null);
                var inv = new PointerFunc<int, int, int>(fp, true);
                int r;
                ref var ret = ref inv.Invoke(out r, 20, 22);
                Check("O-3 PF<int,int,int> generic Invoke: r == 42", r == 42);
                Check("O-3 PF<int,int,int> generic Invoke: ret == 42", ret == 42);
            }
            catch (Exception ex) { Log($"  O-3: {ex.GetType().Name}: {ex.Message}"); }

            // O-4: second factory export - nf_inc_int via GetIncIntFn.
            try
            {
                IntPtr fp = (IntPtr)bridge.GetMethod("GetIncIntFn").Invoke(null, null);
                var inv = new PointerFunc<int, int>(fp, true);
                int r = inv.Invoke(41);
                Check("O-4 PF<int,int> unmanaged calli: inc(41) == 42", r == 42);
            }
            catch (Exception ex) { Log($"  O-4: {ex.GetType().Name}: {ex.Message}"); }

            // O-5..O-10: table path - one export, N addresses (NfFn enum via reflection).
            System.Reflection.MethodInfo mTable = null;
            Type nfEnum = null;
            try
            {
                mTable = bridge.GetMethod("GetTableEntry");
                var mCount = bridge.GetMethod("GetTableCount");
                nfEnum = bridge.GetNestedType("NfFn");
                int n = (int)mCount.Invoke(null, null);
                Log($"  O-5 table count = {n}");
                Check("O-5 bridge table count == 9", n == 9);
            }
            catch (Exception ex) { Log($"  O-5: {ex.GetType().Name}: {ex.Message}"); }

            if (mTable != null && nfEnum != null)
            {
                // O-6: double - PointerFunc<double,double,double>.
                try
                {
                    IntPtr fp = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "AddDouble") });
                    var inv = new PointerFunc<double, double, double>(fp, true);
                    double r = inv.Invoke(1.5, 2.25);
                    Check("O-6 PF<double,double,double> table fp: 1.5+2.25 == 3.75", r == 3.75);
                }
                catch (Exception ex) { Log($"  O-6: {ex.GetType().Name}: {ex.Message}"); }

                // O-7: float - PointerFunc<float,float,float>.
                try
                {
                    IntPtr fp = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "AddFloat") });
                    var inv = new PointerFunc<float, float, float>(fp, true);
                    float r = inv.Invoke(1.5f, 2.25f);
                    Check("O-7 PF<float,float,float> table fp: 1.5f+2.25f == 3.75f", r == 3.75f);
                }
                catch (Exception ex) { Log($"  O-7: {ex.GetType().Name}: {ex.Message}"); }

                // O-8: int64 - PointerFunc<long,long,long>.
                try
                {
                    IntPtr fp = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "AddI64") });
                    var inv = new PointerFunc<long, long, long>(fp, true);
                    long r = inv.Invoke(20L, 22L);
                    Check("O-8 PF<long,long,long> table fp: 20+22 == 42", r == 42);
                }
                catch (Exception ex) { Log($"  O-8: {ex.GetType().Name}: {ex.Message}"); }

                // O-9: cross-size - nf_pass_i64(int64) -> int32: PointerFunc<int,long>.
                try
                {
                    IntPtr fp = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "PassI64") });
                    var inv = new PointerFunc<int, long>(fp, true);
                    int r = inv.Invoke(0x1234);
                    Check("O-9 PF<int,long> cross-size: pass_i64(0x1234) == 0x1234", r == 0x1234);
                }
                catch (Exception ex) { Log($"  O-9: {ex.GetType().Name}: {ex.Message}"); }

                // O-10: void path - nf_set_last(int) via PointerFunc<VoidReturn,int>,
                //       then nf_get_last() via 0-param PointerFunc<int>.
                try
                {
                    IntPtr fpSet = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "SetLast") });
                    var invSet = new PointerFunc<VoidReturn, int>(fpSet, true);
                    invSet.Invoke(0x2A);

                    IntPtr fpGet = (IntPtr)mTable.Invoke(null, new object[] { System.Enum.Parse(nfEnum, "GetLast") });
                    var invGet = new PointerFunc<int>(fpGet, true);
                    int r = invGet.Invoke();
                    Check("O-10 PF<VoidReturn,int> void path + PF<int> 0-param: get_last == 0x2A", r == 0x2A);
                }
                catch (Exception ex) { Log($"  O-10: {ex.GetType().Name}: {ex.Message}"); }
            }
        }
        static bool TryGetFnPtr(string methodName, out IntPtr fn)
        {
            fn = IntPtr.Zero;
            s_lastFnPtrError = null;
            try
            {
                if (s_isIL2CPP)
                {
                    // ===== 2026-09-23 修复说明 =====
                    // 旧况（Generics__57.cpp:10579）：IL2CPP 转译器把织入的 managed calli 改写为
                    //   il2cpp_codegen_get_direct_method_pointer((const RuntimeMethod*)_Pfn)(args, method)
                    // —— wrapper 代码地址被当 RuntimeMethod* 元数据解引用 → 必崩。
                    // 修复（已落地）：Generator 织入的 Invoke 按 _IsUnmanaged 运行时分支——
                    //   true → calli unmanaged（callsite 约定 0x09，与 delegate* unmanaged 同款）
                    //   → 转译器生成纯间接调用，不再解引用元数据。
                    // MakePF 在 IL2CPP 下以 (fn, true) 构造所有 PointerFunc → 走 unmanaged 分支。
// ===== 2026-09-24 v2 (user analysis): MethodHandle.Value =====
                    // IL2CPP compiles calli into il2cpp_codegen_get_direct_method_pointer(
                    // (const RuntimeMethod*)ptr)(args, ptr) - the stack top must be a
                    // RuntimeMethod* (MethodInfo*). GetFunctionPointerForDelegate thunks
                    // are native code addresses -> dereferenced as metadata -> crash.
                    // GetFunctionPointer() is not implemented on IL2CPP (PF-0) - but
                    // MethodHandle.Value returns the raw MethodInfo* the reflection
                    // system itself uses, which is exactly what the calli translation
                    // expects: get_direct_method_pointer(MethodInfo*) -> method pointer,
                    // and the same value flows as the trailing MethodInfo* arg.
                    var mi2 = typeof(PointerDelegateTestSuite).GetMethod(methodName,
                        BindingFlags.Public | BindingFlags.Static);
                    if (mi2 == null) { s_lastFnPtrError = new MissingMethodException(methodName); return false; }
                    fn = mi2.MethodHandle.Value;   // raw MethodInfo* on IL2CPP
                }

                var mi = typeof(PointerDelegateTestSuite).GetMethod(methodName,
                    BindingFlags.Public | BindingFlags.Static);
                if (mi == null) return false;
                RuntimeHelpers.PrepareMethod(mi.MethodHandle);
                fn = mi.MethodHandle.GetFunctionPointer();
            }
            catch (Exception ex) { s_lastFnPtrError = ex; }
            return fn != IntPtr.Zero;
        }

        // C0d: dll Emit 产物判决探测（2026-09-23 P0-2 验证）——E2 被证明不是合格判决
        // 用例（ConvertRef<Dog,object> 在 Emit/fallback 两分支都 JIT，引用共享必踩
        // 织入基元断言）。本探测直接反射调 dll 的 FreeFuncEmit.EmitDynamicInvoker：
        //   C0d-1: dump 产物参数的 modreq（SetSignature 补的 IsReadOnly 有没有写上）
        //   C0d-2: CreateDelegate(DynamicInvoker, 产物, throwOnBindFailure:true)——
        //          BOUND = P0-2 修复生效（Emit 复活）；异常详情 = 真因指路
        //   C0d-3: PointerFuncEmit 产物同测（对照，第一参 IntPtr）
        static void Section_C0d_EmitProductProbe()
        {
            Log("-- C0d. dll Emit product bind probe --");
            try
            {
                var asm = typeof(Mod.LowLevel.FreeFunc<int, int>).Assembly;

                // ---- C0d-1/2: FreeFuncEmit 产物 ----
                var emitFF = asm.GetType("Mod.LowLevel.FreeFuncEmit")
                    .GetMethod("EmitDynamicInvoker");
                var miFF = (System.Reflection.MethodInfo)emitFF.Invoke(null,
                    new object[] { typeof(Func<int, int>), typeof(int), new[] { typeof(int) }, 1, 0u });
                Log($"  C0d-1 FreeFuncEmit product: {miFF.DeclaringType.FullName}.{miFF.Name} (static={miFF.IsStatic})");
                var ps = miFF.GetParameters();
                for (int i = 0; i < ps.Length; i++)
                {
                    var req = ps[i].GetRequiredCustomModifiers();
                    Log($"    param[{i}] {ps[i].ParameterType.Name} modreq={(req.Length == 0 ? "NONE" : req[0].Name)}");
                }
                var delTypeFF = typeof(Mod.LowLevel.FreeFunc<int, int>)
                    .GetNestedType("DynamicInvoker", System.Reflection.BindingFlags.NonPublic);
                try
                {
                    var d = Delegate.CreateDelegate(delTypeFF, miFF, true);
                    Log($"  C0d-2 FreeFunc bind(throw:true): {(d != null ? "BOUND ✓ P0-2 fix works" : "NULL")}");
                }
                catch (Exception bex)
                {
                    Log($"  C0d-2 FreeFunc bind(throw:true): {bex.GetType().Name}: {bex.Message}");
                }

                // ---- C0d-3: PointerFuncEmit 产物（对照）----
                var emitPF = asm.GetType("Mod.LowLevel.PointerFuncEmit")
                    .GetMethod("EmitDynamicInvoker");
                var miPF = (System.Reflection.MethodInfo)emitPF.Invoke(null,
                    new object[] { false, typeof(int), new[] { typeof(int) }, 1, 0u });
                // ⚠ 2026-09-24：dll 已回滚（DynamicInvoker 回到泛型类嵌套）。Unity Mono 的
                //   GetNestedType 对构造泛型嵌套类型返回未实例化形态（Invoke returns R&）——
                //   本探测的绑定失败是已知假象，不是 dll 真问题；Emit 的权威判据是 C0g。
                var delTypePF = typeof(Mod.LowLevel.PointerFunc<int, int>)
                    .GetNestedType("DynamicInvoker", System.Reflection.BindingFlags.NonPublic);
                try
                {
                    var d = Delegate.CreateDelegate(delTypePF, miPF, true);
                    Log($"  C0d-3 PointerFunc bind(throw:true): {(d != null ? "BOUND ✓" : "NULL")}");
                }
                catch (Exception bex)
                {
                    Log($"  C0d-3 PointerFunc bind(throw:true): {bex.GetType().Name}: {bex.Message}");
                }
            }
            catch (Exception ex)
            {
                // 打印整条 InnerException 链——TIE 外层无信息，真因在内层
                Log($"  C0d: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
                var st = ex.InnerException?.StackTrace;
                if (st != null) Log($"    inner stack (first 3): {string.Join(" | ", st.Split('\n').Take(3))}");
            }
        }

        // C0e: 绑定失败变量二分（2026-09-23）——C0d 实锤 dll 产物绑定失败
        // （"method return type is incompatible"），但 C0c 测试侧同形态 BOUND。
        // 三个差异变量一次二分：modreq / 基类 FreeInvokableBase / delegate 来源。
        //   C0e-1: 裸 byref + 无基类 + 测试侧 delegate（对照，预期 BOUND）
        //   C0e-2: 裸 byref + 继承 FreeInvokableBase + 测试侧 delegate（测基类）
        //   C0e-3: 裸 byref + 无基类 + dll DynamicInvoker（测 delegate 来源）
        //   C0e-4: SetSignature modreq + 无基类 + 测试侧 delegate（测 modreq）
        //   C0e-5: 全复刻 dll 形态（modreq + 基类 + dll delegate）
        static void Section_C0e_BindVariableBisect()
        {
            Log("-- C0e. bind-failure variable bisect --");
            try
            {
                var ab = DefineDynamicAssemblyCompat("C0eProbe", AssemblyBuilderAccess.Run);
                var mod = ab.DefineDynamicModule("C0eMod");
                var modreqType = Type.GetType("System.Runtime.CompilerServices.IsReadOnly");

                // 测试侧 delegate：ref int(Delegate, out int, in int)——与 dll DynamicInvoker 同签名
                var testDel = typeof(GenDel<int, int>);
                // dll 的 delegate：回滚后回到 FreeFunc<int,int> 嵌套（Unity Mono 的
                //   GetNestedType 返回未实例化形态——C0e-3/5 的绑定失败是已知假象）
                var dllDel = typeof(Mod.LowLevel.FreeFunc<int, int>)
                    .GetNestedType("DynamicInvoker", System.Reflection.BindingFlags.NonPublic);

                System.Reflection.MethodInfo MakeMethod(string name, bool inheritBase, bool withModreq)
                {
                    var tb = mod.DefineType(name,
                        System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Sealed,
                        inheritBase ? typeof(Mod.LowLevel.FreeInvokableBase) : null);
                    // static ref int M(Delegate, int&, int&)
                    var mb = tb.DefineMethod("M",
                        System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                        typeof(int).MakeByRefType(),
                        new[] { typeof(Delegate), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() });
                    if (withModreq && modreqType != null)
                    {
                        var req = new Type[3][];
                        req[2] = new[] { modreqType };   // 'in int' param
                        mb.SetSignature(typeof(int).MakeByRefType(), null, null,
                            new[] { typeof(Delegate), typeof(int).MakeByRefType(), typeof(int).MakeByRefType() },
                            req, null);
                    }
                    var il = mb.GetILGenerator();
                    il.Emit(OpCodes.Ldarg_1);   // return ref r
                    il.Emit(OpCodes.Ret);
                    var created = tb.CreateType();   // netstandard2.0 lacks TypeBuilder.CreatedType — use the return value
                    return created.GetMethod("M");
                }

                void TryBind(string tag, System.Reflection.MethodInfo m, Type del)
                {
                    try
                    {
                        var d = Delegate.CreateDelegate(del, m, true);
                        Log($"  {tag}: {(d != null ? "BOUND" : "NULL")}");
                    }
                    catch (Exception bex)
                    {
                        Log($"  {tag}: {bex.GetType().Name}: {bex.Message}");
                    }
                }

                TryBind("C0e-1 bare+noBase+testDel (control)", MakeMethod("T1", false, false), testDel);
                TryBind("C0e-2 bare+FreeInvokableBase+testDel", MakeMethod("T2", true, false), testDel);
                TryBind("C0e-3 bare+noBase+dll DynamicInvoker", MakeMethod("T3", false, false), dllDel);
                TryBind("C0e-4 modreq+noBase+testDel", MakeMethod("T4", false, true), testDel);
                TryBind("C0e-5 modreq+base+dllDel (dll replica)", MakeMethod("T5", true, true), dllDel);

                // ---- C0f: dll delegate 签名 dump + dll 产物方法×测试 delegate 交叉 ----
                // C0e-3 锁定 delegate 来源是坏变量，但"dll 产物方法"从未与"好 delegate"交叉过
                // （C0d-1 的 GetParameters() dump 一行没打——dll 产物反射信息可能缺失）。
                var inv = dllDel.GetMethod("Invoke");
                Log($"  C0f-1 dllDel: {dllDel.FullName} generic={dllDel.IsGenericType}");
                Log($"  C0f-2 dllDel.Invoke returns: {inv.ReturnType.Name} (expect Int32&)");
                foreach (var p in inv.GetParameters())
                    Log($"    param: {p.ParameterType.Name}");
                var asmDll = typeof(Mod.LowLevel.FreeFunc<int, int>).Assembly;
                var emitFF = asmDll.GetType("Mod.LowLevel.FreeFuncEmit").GetMethod("EmitDynamicInvoker");
                var miFF = (System.Reflection.MethodInfo)emitFF.Invoke(null,
                    new object[] { typeof(Func<int, int>), typeof(int), new[] { typeof(int) }, 1, 0u });
                Log($"  C0f-3 dll product params: {miFF.GetParameters().Length} (0 = Mono reflection info missing)");
                TryBind("C0e-6 dllProduct+testDel (cross)", miFF, testDel);
            }
            catch (Exception ex)
            {
                Log($"  C0e: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
            }
        }

        // C0g: 正常调用链 Emit 终审（2026-09-23）——C0f 实锤 Mono 的 GetNestedType 对
        // 构造泛型类型的嵌套 delegate 返回未实例化形态（Invoke returns R&）——
        // C0d-2/C0e-3/C0e-5 的绑定失败是**探测假象**（反射拿的 delegate 类型是坏的）。
        // 正常调用链的 typeof(DynamicInvoker) 是编译期 ldtoken（正确实例化）——
        // R5 dll（catch Exception + modreq）可能已经修好 Emit。终审方法：
        //   正常链触发一次泛型 Invoke → 读 _EmitCache 缓存数
        //   count > 0 = Emit LIVE（CreateDelegate 成功且已缓存）——P0-2 修复生效实锤
        //   count == 0 = Emit dead（Invoke catch 置 disabled，无缓存）
        static void Section_C0g_NormalChainEmitProbe()
        {
            Log("-- C0g. normal-chain Emit final verdict --");
            try
            {
                // FreeFunc<int,int>：正常链触发（编译期 token 的 typeof）
                var ff = new Mod.LowLevel.FreeFunc<int, int>(x => x);
                int rr;
                ff.Invoke(out rr, 5);
                var cacheFF = typeof(Mod.LowLevel.FreeFunc<int, int>)
                    .GetField("_EmitCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .GetValue(null);
                int countFF = ((System.Collections.ICollection)cacheFF).Count;
                Log($"  C0g-1 FreeFunc<int,int> EmitCache count = {countFF} " +
                    (countFF > 0 ? "→ Emit LIVE ✓ (P0-2 fix works on the normal chain)" : "→ Emit dead (fallback)"));

                // PointerFunc<int,int>：正常链触发（IL2CPP 下跳过——thunk 配泛型 Invoke
                // 的 fallback 会踩织入 calli 的 RuntimeMethod* 解引用崩；PF-0 已另行探测）
                if (s_isIL2CPP)
                {
                    Log("  C0g-2 PointerFunc probe: SKIPPED on IL2CPP (woven calli path unusable; see PF-0)");
                }
                else
                {
                var fn = Marshal.GetFunctionPointerForDelegate((Func<int, int>)(x => x + 1));
                var pf = new Mod.LowLevel.PointerFunc<int, int>(fn);
                int rp;
                pf.Invoke(out rp, 5);
                var cachePF = typeof(Mod.LowLevel.PointerFunc<int, int>)
                    .GetField("_EmitCache", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                    .GetValue(null);
                int countPF = ((System.Collections.ICollection)cachePF).Count;
Log($"  C0g-2 PointerFunc<int,int> EmitCache count = {countPF} " +
                    (countPF > 0 ? "LIVE" : "dead (fallback)"));
                }
            }
            catch (Exception ex)
            {
                Log($"  C0g: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
            }
        }

        // G: Generator 织入 IL vs Roslyn IL 对照（2026-09-23）——A5b 证明运行时 Emit 产物
        // 在共享实例化下安全；A5a 崩在织入 calli。dotnet 侧已 dump 两边 IL：
        //   ldfld 形态一致（TypeSpec+MemberRef）；MaxStack 一致（都是 2）；
        //   剩余结构差异：①织入 pfn 先压栈跨分支持有，Roslyn 压栈后立即 calli；
        //   ②long vs short jump；③InitLocals False vs True。
        // G1: Roslyn 形态（delegate* 生成 calli）+ 全引用实例化（GProbe<string>）——
        //     在 Mono 共享代码下跑。PASS = Roslyn 形态安全 → 差异在织入的栈模式等细节；
        //     崩 = 泛型类+calli 在 Mono 共享代码根本不行（与 ILC 结论相反）。
        private unsafe class GProbe<T>
        {
            public IntPtr Fn;
            public bool IsUnmanaged;
            public int ReturnCategory;

            public T Call()
            {
                if (IsUnmanaged)
                {
                    if (ReturnCategory != 0)
                        return ((delegate*<T>)Fn)();
                    ((delegate*<void>)Fn)();
                    return default;
                }
                if (ReturnCategory != 0)
                    return ((delegate*<T>)Fn)();
                ((delegate*<void>)Fn)();
                return default;
            }
        }

        static void Section_G_WovenVsRoslyn()
        {
            Log("-- G. woven vs Roslyn calli in shared-generic code --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("G1/G1b", Il2CppCalliSkipReason); return; }
            try
            {
                // 2026-09-24 修正（用户定位 + G1Repro 双平台实锤）：原版 fn 用
                //   Marshal.GetFunctionPointerForDelegate(Func<string>) —— 产生的是
                //   native marshal thunk（native 签名 char*()，string 按 LPStr 转换，
                //   UnmanagedCallersOnly 标记），配 managed calli（期望托管 string 引用）
                //   是非法组合：CoreCLR 直接 Fatal error 拦截（"attempted to call a
                //   UnmanagedCallersOnly method"）、net472 状态损坏（Exception.ToString
                //   都崩）、Unity Mono 垃圾返回（r != "G1"，即此前的 G1 FAIL）。
                //   **G1 的 FAIL 不是 Mono 共享 JIT 的 bug，是测试自身的签名不匹配。**
                //   改用 managed fnptr（TryGetFnPtr = MethodHandle.GetFunctionPointer）——
                //   managed calli 的语义正确目标（E2 同款组合，Unity PASS 实锤）。
                if (!TryGetFnPtr(nameof(ReturnG1), out var fnG1))
                {
                    Skip("G1/G1b Roslyn-shape calli", FnPtrSkipReason("fnptr unavailable"));
                    return;
                }

                // G1: Roslyn 形态，全引用实例化（string → 共享桶）
                var probe = new GProbe<string> { Fn = fnG1, ReturnCategory = 1 };
                string r = probe.Call();
                Check("G1 Roslyn-shape calli, all-ref instantiation: returns \"G1\"", r == "G1");

                // G1b: void 路径（ReturnCategory=0）
                // 2026-09-24 修正（HybridCLR 实锤）：原版用 fnG1（string 返回方法）配
                //   delegate*<void> callsite——签名不匹配。Mono 下返回值被忽略碰巧无害；
                //   HybridCLR 解释器按 void() 签名调用 string() 方法 → native 崩
                //   （Interpreter_Execute.cpp:4871）。改用真 void 方法（NoParamVoid）
                //   配 void callsite——签名匹配，语义正确的测试。
                if (!TryGetFnPtr(nameof(NoParamVoid), out var fnVoid))
                {
                    Skip("G1b Roslyn-shape void-calli path", FnPtrSkipReason("fnptr unavailable"));
                }
                else
                {
                    var probe2 = new GProbe<string> { Fn = fnVoid, ReturnCategory = 0 };
                    string r2 = probe2.Call();
                    Check("G1b Roslyn-shape void-calli path: default", r2 == null);
                }
            }
            catch (Exception ex)
            {
                Log($"  G: {ex.GetType().Name}: {ex.Message}");
            }
        }

        // ---------------- 入口 ----------------

        public static Report RunAll(int gcStressMs = 1500, bool parallelGC = false, Action<string> log = null)
        {
            s_rep = new Report();
            s_log = log;
            var sw = Stopwatch.StartNew();
            Log("== PointerDelegateTestSuite ==");
            Log($"platform: IL2CPP={s_isIL2CPP}, HybridCLR={s_isHybridClr} (fnptr 路径: {(s_isIL2CPP ? "GetFunctionPointerForDelegate→ReversePInvokeWrapper(unmanaged)" : "MethodHandle.GetFunctionPointer(managed)")})");

            Section_P_PtrWrite();
            Section_C_EmitCapability();
// 2026-09-24 reorder (IL2CPP): FreeFunc (D) + PF-0 probe run BEFORE the PointerFunc
            // sections - if PF-0 crashes natively, the FreeFunc results are already on disk.
            Section_FreeFuncWrapper();
            Section_PF0_IL2CppMethodHandleProbe();
            // P-1: MethodHandle.Value (RuntimeMethod*) as managed-calli fnptr — user
            // hypothesis 2026-09-29. Runs right after PF-0; on non-IL2CPP it no-ops.
            Section_P1_MethodHandleValueProbe();
            Section_BasicCalls();
            Section_ByRefSemantics();
            Section_MultiParam();
            Section_F_UnmanagedCalli();
            Section_GenericInvokeTypeMismatch();
            Section_Gc_FnPtrStability();
            Section_Gc_InsideTarget();
            Section_Gc_RefReturnSemantics();
            Section_Gc_FreeFuncSurvival();
            Section_Gc_FreeFuncByRefRetSurvival();
            Section_Gc_ByRefRelocationStress(gcStressMs, parallelGC);

            // C0c 探测放最后（2026-09-23 实锤：其泛型 delegate 绑定疑似污染 Mono JIT 状态——
            // 放前面时后续 marshal.c:3687 断言崩（B3 或 D 段前）；放尾部则崩也不丢结果）。
            Section_C0c_GenericDelegateBindProbe();
            Section_C0d_EmitProductProbe();
            Section_C0e_BindVariableBisect();
            Section_C0g_NormalChainEmitProbe();
            Section_G_WovenVsRoslyn();
            Section_A5a_WovenDirectProbe();

            // M. Marshal 行为探针（2026-09-24）——IL2CPP native 调用能力实验（用户三问）。
            //   放最尾部：delegate* unmanaged 在 IL2CPP 的转译产物未知——若 native 崩，
            //   崩在收尾不丢其他结果。
            Section_M_MarshalProbe();

            // N. IL2CPP native-call final verdict (user's two questions).
            Section_N_NativeCallVerdict();

            // O. AOT-bridge fnptr via PointerFunc unmanaged calli (2026-09-28 step 2).
            Section_O_AotBridgeCalli();

            Log($"== Total: {s_rep.Pass} passed, {s_rep.Fail} failed, {s_rep.Skip} skipped " +
                $"({sw.ElapsedMilliseconds} ms) ==");
            return s_rep;
        }

        // A5a (tail probe, 2026-09-24 R14): the woven non-generic Invoke() DIRECT call on
        // an all-reference instantiation (PointerFunc<VoidReturn> → shared-generic code).
        // User experiments in this dll: exp 1 (fresh callsite type refs — byte-identical,
        // no-op), exp 2 (DynamicInvoker delegates moved out of the generic classes into
        // unified non-generic holders), exp 3 (branch-first + per-arm param pushing — in
        // place since R10). This probe is the verdict for the woven calli under shared
        // JIT. Placed at the tail so a native crash does not block the rest of the results.
        static void Section_A5a_WovenDirectProbe()
        {
            Log("-- A5a. woven non-generic Invoke() direct (shared instantiation) --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("A5a", Il2CppCalliSkipReason); return; }
            if (!TryGetFnPtr(nameof(NoParamVoid), out var fnVoid))
            {
                Skip("A5a", FnPtrSkipReason("fnptr unavailable"));
                return;
            }
            var inv = MakePF<VoidReturn>(fnVoid);
            var r5a = inv.Invoke();
            Check("A5a PF non-generic Invoke() direct: r == default", r5a == null);
        }

        // ---------------- P. PtrWrite 基础形态（2026-09-23 同步新增） ----------------
        // 隔离验证：纯 unsafe 指针写/读回（不涉及库）——排除"库注入 IL 非法"前，
        // 先证明平台本身能处理指针写回调用方变量。

        static void Section_P_PtrWrite()
        {
            Log("-- P. PtrWrite direct --");

            int slot = 0;
            IntPtr ptr = (IntPtr)0x12345678;
            WriteIntLocal<int>(ref slot, ptr);
            Check("P1 PtrWrite shape Write<int>: slot == 0x12345678", slot == 0x12345678);

            // 2026-09-24 修正（IL2CPP FAIL 根因）：原版 ReturnRefIntLocal(IntPtr value) 的
            // value 是【值传递形参】——返回指向自己形参的 ref = 悬垂指针（ECMA 层面 UB）。
            // Mono/CoreCLR 靠 JIT 内联碰巧 PASS（形参提升到调用方栈）；IL2CPP 不内联
            // （独立 gshared 函数，il2cppOutput 实证）→ ref 指向已死栈帧 → 读到垃圾。
            // 修法：ref 传参——ref 目标是调用方变量，跨调用存活。
            ref var r = ref ReturnRefIntLocal<int>(ref ptr);
            Check("P2 PtrWrite shape ReturnRef<int>: *r == 0x12345678", r == 0x12345678);
        }

        // RawRef 实现（Unity 侧指针操作替代——泛型 T* 指针在 Unity 编译器报 CS0208）：
        // RawRef.Of(ref value) 包变量本身的引用 → GetRef<T>() 同地址 T 视图 → 读位模式
        // （与 *(T*)&v 语义一致：读 v 槽的位模式，不是解引用 v 的值）。
        static void WriteIntLocal<T>(ref T slot, IntPtr value)
        {
            slot = RawRef.Of(ref value).GetRef<T>();
        }
        static ref T ReturnRefIntLocal<T>(ref IntPtr value)
        {
            return ref RawRef.Of(ref value).GetRef<T>();
        }

        // ---------------- C0. Emit 能力探测（2026-09-23 新增，全链路版） ----------------
        // 泛型 Invoke 的 Emit 链路（PointerFuncEmit.EmitDynamicInvoker）两级探测：
        //   C0a: DynamicMethod + ByRef 返回——.NET Framework 全系 / Unity Mono 拒绝
        //        （NotSupportedException）；CoreCLR 3+ 支持。
        //   C0b: AssemblyBuilder 全链路（dll 的 fallback 路径）：DefineDynamicAssembly
        //        (RunAndCollect→Run) → DefineDynamicModule → DefineType → DefineMethod
        //        (ref int 返回) → IL → CreateType → CreateDelegate → 真调用。
        //        每步打点——一次运行即可定位 Mono 卡在哪一步。
        // ⚠ 2026-09-23 Unity 实测背景：Mono 上 Emit fallback 失败 → 泛型 Invoke 退到
        //   织入 Invoke → 织入的 unmanaged calli（0x09）在【引用类型实例化】下 JIT
        //   编译必崩（mono_marshal_get_native_func_wrapper_indirect 断言，E2 实锤）。
        //   Emit 活了（C0b PASS）才能让引用类型 PointerFunc 避开织入路径。
        static void Section_C_EmitCapability()
        {
            Log("-- C0. Emit capability probe --");

            // C0a: DynamicMethod + ByRef return（CoreCLR 快路径）
            bool dmByRefOk = false;
            try
            {
                var dm = new DynamicMethod("probe", typeof(int).MakeByRefType(),
                    new[] { typeof(int).MakeByRefType() }, typeof(PointerDelegateTestSuite), true);
                var il = dm.GetILGenerator();
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Ret);
                dmByRefOk = true;
                Check("C0a DynamicMethod + ByRef return: SUPPORTED (fast path)", true);
            }
            catch (Exception ex)
            {
                Skip("C0a DynamicMethod + ByRef return",
                    $"NOT SUPPORTED — {ex.GetType().Name}: {ex.Message}");
            }

            // C0b: AssemblyBuilder 全链路（Framework/Mono 的 fallback 路径）
            if (dmByRefOk) return;   // 快路径可用，fallback 无需探测
            try
            {
                System.Reflection.Emit.AssemblyBuilder ab;
                try
                {
                    ab = DefineDynamicAssemblyCompat("C0Probe_RAC", System.Reflection.Emit.AssemblyBuilderAccess.RunAndCollect);
                }
                catch (Exception)
                {
                    ab = DefineDynamicAssemblyCompat("C0Probe_Run", System.Reflection.Emit.AssemblyBuilderAccess.Run);
                }
                var mod = ab.DefineDynamicModule("C0ProbeMod");
                var tb = mod.DefineType("C0ProbeType",
                    System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Sealed);
                // ref int Probe(ref int) —— 与 dll 的 DynamicInvoke 同形态（ByRef 返回）
                var mb = tb.DefineMethod("Probe",
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                    typeof(int).MakeByRefType(), new[] { typeof(int).MakeByRefType() });
                var il2 = mb.GetILGenerator();
                il2.Emit(OpCodes.Ldarg_0);
                il2.Emit(OpCodes.Ret);
                var t = tb.CreateType();
                var emitted = t.GetMethod("Probe");
                var del = (ProbeDel)Delegate.CreateDelegate(typeof(ProbeDel), emitted);
                int x = 42;
                ref int r = ref del(ref x);
                s_emitFallbackOk = true;
                Check("C0b AssemblyBuilder full chain: SUPPORTED (fallback path live)", r == 42);
            }
            catch (Exception ex)
            {
                Skip("C0b AssemblyBuilder full chain",
                    $"FAILED — {ex.GetType().Name}: {ex.Message} " +
                    "(generic Invoke falls back to the woven non-generic calli; " +
                    "reference-type PointerFunc instances CRASH the Mono JIT there " +
                    "when the dll contains unmanaged calli — see E2 notes)");
            }
        }

        private delegate ref int ProbeDel(ref int x);

        // C0c 探测用 delegate（与 FreeFunc.DynamicInvoker 同构）
        private delegate ref ByRefParam ConcreteDel(Delegate d, out ByRefParam r, in ByRefParam u);
        private delegate ref TR GenDel<TR, TU>(Delegate d, out TR r, in TU u);

        // ---------------- M. Marshal 行为探针（2026-09-24，IL2CPP native 调用能力实验） ----------------
        // 用户三问：①GetDelegateForFunctionPointer 对 object/string 的处理（blit 还是转换）
        // ②泛型 delegate 支持 ③delegate* unmanaged + where T:unmanaged 的泛型上下文。
        // dotnet 侧已验证（MarshalProbe 项目）：CoreCLR object/string 都是引用透传（same-ref=True，
        // 非 DllImport 式 LPStr 拷贝）；泛型 delegate 双平台拒绝（"must not be a generic type"）；
        // C1 where T:unmanaged + delegate* unmanaged 编译+运行 OK（net472 运行时 BadImageFormat）。
        // 本段在 IL2CPP 下复测——IL2CPP 的实现可能不同。
        //
        // ⚠ 2026-09-24 IL2CPP 构建产物实锤（GenericMethods__14.cpp）：
        //   ①delegate* unmanaged 在 IL2CPP = 真 native CDECL 间接调用（typedef int32_t (CDECL*)(int32_t)）
        //     ——不是 RuntimeMethod* 模式！方案 C 核心可行性已证。
        //   ②泛型 fnptr 的限制：具体实例化（T=int）生成正确 native 调用；完全共享泛型代码
        //     （Il2CppFullySharedGenericStruct 兜底版）抛 NotSupportedException
        //     （"Cannot call native function pointer with generic arguments from fully shared generic code"）。
        //   ③IL2CPP 转译器 bug：兜底版 raise 后的死代码 return L_4 引用未声明变量 → C++ 编译错误
        //     → 整个 Player 构建失败。因此泛型版 MCallUnmanaged<T> 已删——只要它存在，
        //     IL2CPP 必生成兜底版 → 必失败。泛型 fnptr 在 IL2CPP 的结论以构建产物为准。
        private static object MIdentityObj(object o) => o;
        private static string MIdentityStr(string s) => s;
        private static int MInc(int x) => x + 1;
        private static string MStrInfo(string s) => $"len={s.Length}|first={s[0]}|tail={s[s.Length - 1]}";
        private static string MObjToStr(object o) => $"got:{o}";
        private delegate object MObjObjDelegate(object o);
        private delegate string MStrStrDelegate(string s);
        private delegate int MIntIntDelegate(int x);

        static unsafe int MCallUnmanagedNoArg(IntPtr fn)
        {
            return ((delegate* unmanaged[Cdecl]<int>)fn)();
        }

        [DllImport("kernel32", EntryPoint = "LoadLibraryW", CharSet = CharSet.Unicode)]
        static extern IntPtr MLoadLibrary(string name);
        [DllImport("kernel32", EntryPoint = "GetProcAddress")]
        static extern IntPtr MGetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string name);

        static unsafe void Section_M_MarshalProbe()
        {
            Log("-- M. Marshal behavior probe (native-call capability) --");

            // 2026-09-24 HybridCLR 守卫：M 段是为纯 IL2CPP 的方案 A（GetDelegateForFunctionPointer
            // 签名表）设计的 Marshal 探针。HybridCLR 下不可用，根因（用户定位）：
            //   ①【热更新程序集不能有 DllImport】——M-A5 实锤：LoadLibraryW 静默返回 0
            //     （纯 IL2CPP 下同代码 PASS——AOT 的 pinvoke wrapper 预生成；HybridCLR 解释执行
            //     的热更新代码 DllImport 声明不生效）→ GetProcAddress 拿空指针 → M-C2 空指针
            //     fnptr 调用 native 崩（MethodBridge __M2NF_Bi4）。
            //   ②reverse wrapper 有数量配额（M-A2 "exceed max wrapper num"）。
            //   ③native 调用主路径（calli）已被 PF-0/A5a/G1 全部验证——M 段在 HybridCLR 无判决价值。
            //   若需在 HybridCLR 测真 native：DllImport 声明须挪到 AOT 程序集（热更新代码调
            //   AOT 的 wrapper 方法）。
            if (s_isHybridClr)
            {
                Skip("M.*", "HybridCLR: DllImport in hot-update assemblies silently fails " +
                    "(M-A5 LoadLibraryW returned 0) + reverse-wrapper quota; native-call " +
                    "main path (calli) already verified by PF-0/A5a/G1");
                return;
            }

            // M-A1: object roundtrip（地址 blit 还是 COM marshal？）
            try
            {
                var d = (MObjObjDelegate)MIdentityObj;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (MObjObjDelegate)Marshal.GetDelegateForFunctionPointer(thunk, typeof(MObjObjDelegate));
                var obj = new object();
                var back = d2(obj);
                Log($"  M-A1 object roundtrip: same-ref={ReferenceEquals(back, obj)}");
            }
            catch (Exception ex) { Log($"  M-A1: {ex.GetType().Name}: {ex.Message}"); }

            // M-A2: string roundtrip（转换发生吗？）
            try
            {
                var d = (MStrStrDelegate)MIdentityStr;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (MStrStrDelegate)Marshal.GetDelegateForFunctionPointer(thunk, typeof(MStrStrDelegate));
                string s = "Hello_Marshal_Probe_0123456789";
                var back = d2(s);
                Log($"  M-A2 string roundtrip: same-ref={ReferenceEquals(back, s)}, value-eq={back == s}");
            }
            catch (Exception ex) { Log($"  M-A2: {ex.GetType().Name}: {ex.Message}"); }

            // M-A4: 泛型 delegate（Func<int,int>）
            try
            {
                var d = (Func<int, int>)MInc;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<int, int>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<int, int>));
                Log($"  M-A4 generic-delegate: {d2(41)} (expect 42 if supported)");
            }
            catch (Exception ex) { Log($"  M-A4: {ex.GetType().Name}: {ex.Message}"); }

            // M-A5: 真 native 地址（kernel32!GetCurrentProcessId）
            try
            {
                var h = MLoadLibrary("kernel32.dll");
                var fp = MGetProcAddress(h, "GetCurrentProcessId");
                var d2 = (MIntIntDelegate)Marshal.GetDelegateForFunctionPointer(fp, typeof(MIntIntDelegate));
                int pid = d2(0);
                Log($"  M-A5 real-native via GetDelegateForFunctionPointer: pid={pid}, " +
                    $"matches={pid == System.Diagnostics.Process.GetCurrentProcess().Id}");
            }
            catch (Exception ex) { Log($"  M-A5: {ex.GetType().Name}: {ex.Message}"); }

            // M-C1: delegate* unmanaged + where T : unmanaged（泛型上下文 fnptr）
            // ⚠ 泛型版已删（IL2CPP 转译器对完全共享兜底版生成死代码 return L_4 → C++ 编译
            //   错误 → Player 构建失败）。结论以构建产物为准：
            //   具体实例化（T=int）= 真 native CDECL 间接调用 ✓；完全共享泛型代码 =
            //   NotSupportedException（IL2CPP 官方限制）。
            Log("  M-C1 delegate* unmanaged[where T:unmanaged]: verdict from build output - " +
                "concrete instantiation = real native CDECL indirect call; fully-shared generic " +
                "code = NotSupportedException (generic method removed: il2cpp translator emits " +
                "dead-code 'return L_4' -> C2065 build error)");

            // M-C2: 非泛型 delegate* unmanaged 直接调
            try
            {
                var h = MLoadLibrary("kernel32.dll");
                var fp = MGetProcAddress(h, "GetCurrentProcessId");
                int r = MCallUnmanagedNoArg(fp);
                Log($"  M-C2 delegate* unmanaged direct: pid={r}, " +
                    $"matches={r == System.Diagnostics.Process.GetCurrentProcess().Id}");
            }
            catch (Exception ex) { Log($"  M-C2: {ex.GetType().Name}: {ex.Message}"); }

            // ================= M-B 系列（2026-09-24 用户方案实验）=================
            // 最终方案候选：GetDelegateForFunctionPointer 绑定到【泛型 Func<>】——
            // IL2CPP 源码（PlatformInvoke.cpp:455）不检查 IsGenericType，只看 AOT 预生成
            // 的 interopData->delegatePInvokeWrapperFunction——泛型封闭类型是独立
            // Il2CppClass，wrapper 若被 AOT 生成则支持（与 CoreCLR 的入口拒绝不同）。
            // 着重测试 object/string 参数与返回值的【透传】问题。

            // M-B1: 泛型 Func<int,int> roundtrip（基础支持判决）
            try
            {
                Func<int, int> d = MInc;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<int, int>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<int, int>));
                Log($"  M-B1 generic Func<int,int> roundtrip: {d2(41)} (expect 42)");
            }
            catch (Exception ex) { Log($"  M-B1: {ex.GetType().Name}: {ex.Message}"); }

            // M-B2: 泛型 Func<string,string>——string 参数+返回值透传（重点）
            try
            {
                Func<string, string> d = MIdentityStr;   // 原样返回——same-ref 判定透传
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<string, string>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<string, string>));
                string s = "Hello_Transparent_Probe_0123456789ABCDEF";
                var back = d2(s);
                Log($"  M-B2 Func<string,string>: same-ref={ReferenceEquals(back, s)}, value-eq={back == s}" +
                    $" (same-ref=True = 引用透传; False+value-eq=True = marshal 拷贝转换)");
            }
            catch (Exception ex) { Log($"  M-B2: {ex.GetType().Name}: {ex.Message}"); }

            // M-B3: 泛型 Func<object,object>——object 参数+返回值透传（重点）
            try
            {
                Func<object, object> d = MIdentityObj;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<object, object>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<object, object>));
                var obj = new object();
                var back = d2(obj);
                Log($"  M-B3 Func<object,object>: same-ref={ReferenceEquals(back, obj)}" +
                    $" (True = 引用透传; False = marshal 包装)");
            }
            catch (Exception ex) { Log($"  M-B3: {ex.GetType().Name}: {ex.Message}"); }

            // M-B4: 真 native 地址 + 泛型 delegate（kernel32!GetCurrentProcessId → Func<int>）
            try
            {
                var h = MLoadLibrary("kernel32.dll");
                var fp = MGetProcAddress(h, "GetCurrentProcessId");
                var d2 = (Func<int>)Marshal.GetDelegateForFunctionPointer(fp, typeof(Func<int>));
                int pid = d2();
                Log($"  M-B4 real-native + generic Func<int>: pid={pid}, " +
                    $"matches={pid == System.Diagnostics.Process.GetCurrentProcess().Id}");
            }
            catch (Exception ex) { Log($"  M-B4: {ex.GetType().Name}: {ex.Message}"); }

            // M-B4b: 反射直调 GetDelegateForFunctionPointerInternal（绕过托管层 IsGenericType
            // 检查——native 层 MarshalFunctionPointerToDelegate 无泛型检查，只看 AOT wrapper）
            // 2026-09-24 Editor verdict: on Mono this icall + generic Func<int> SIGSEGVs
            // natively (ves_icall_..._GetDelegateForFunctionPointerInternal_raw, uncatchable).
            // Mono: skip. IL2CPP: keep going (there it throws MarshalDirectiveException).
            try
            {
                if (Type.GetType("Mono.Runtime") != null)
                {
                    Log("  M-B4b: SKIPPED on Mono - icall + generic Func<int> = native SIGSEGV (2026-09-24 Editor crash)");
                }
                else
                {
                    var h = MLoadLibrary("kernel32.dll");
                    var fp = MGetProcAddress(h, "GetCurrentProcessId");
                    var icall = typeof(Marshal).GetMethod("GetDelegateForFunctionPointerInternal",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
                    if (icall == null) { Log("  M-B4b: icall not found (managed layer differs)"); }
                    else
                    {
                        var del = (Func<int>)icall.Invoke(null, new object[] { fp, typeof(Func<int>) });
                        int pid = del();
                        Log($"  M-B4b icall-bypass + generic Func<int>: pid={pid}, " +
                            $"matches={pid == System.Diagnostics.Process.GetCurrentProcess().Id}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"  M-B4b: {ex.GetType().Name}: {ex.Message}");
                for (var ie = ex.InnerException; ie != null; ie = ie.InnerException)
                    Log($"    INNER: {ie.GetType().Name}: {ie.Message}");
            }

            // M-B5: string 参数内容完整性（经 native 侧观察——若 marshal 是 ANSI 拷贝，
            // 非 ASCII 字符会损坏；若透传则与直接调用完全一致）
            try
            {
                Func<string, string> d = MStrInfo;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<string, string>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<string, string>));
                string s = "中文内容完整性测试ABC";
                var viaNative = d2(s);
                var direct = d(s);
                Log($"  M-B5 string content integrity: viaNative=\"{viaNative}\" vs direct=\"{direct}\" match={viaNative == direct}");
            }
            catch (Exception ex) { Log($"  M-B5: {ex.GetType().Name}: {ex.Message}"); }

            // M-B6: string 返回值 + object 参数混合（双向透传综合）
            try
            {
                Func<object, string> d = MObjToStr;
                var thunk = Marshal.GetFunctionPointerForDelegate(d);
                var d2 = (Func<object, string>)Marshal.GetDelegateForFunctionPointer(thunk, typeof(Func<object, string>));
                string back = d2("mixed");
                Log($"  M-B6 Func<object,string> mixed: back=\"{back}\" (expect got:mixed)");
            }
            catch (Exception ex) { Log($"  M-B6: {ex.GetType().Name}: {ex.Message}"); }
        }

        // C0c-4 探测用：泛型类嵌套 delegate——精确模拟 FreeFunc<R, U1>.DynamicInvoker 形态
        // （GenDel 嵌套在非泛型类里 BOUND 了；FreeFunc 的嵌套在泛型类里——这个差异是
        //  CreateDelegate 返回 null 的头号嫌疑）
        private class GenHost<TR, TU>
        {
            public delegate ref TR NestedDel(Delegate d, out TR r, in TU u);
        }

        // C0c: 泛型 delegate 绑定探测（2026-09-23）——D0 发现 FreeFunc 的 CreateDelegate
        // 对 AssemblyBuilder 产物返回 null（两参重载绑定失败静默返回 null）。C0b 用
        // 非泛型 ProbeDel 绑定成功——本探测隔离"泛型 delegate 实例化"这个变量：
        // 同一动态方法，只换 delegate 类型。
        //   C0c-1: 非泛型 delegate（对照，预期 BOUND）
        //   C0c-2: 泛型 delegate 实例化（关键——NULL 则实锤 Mono 泛型 delegate 绑定限制）
        //   C0c-3: 泛型 + throwOnBindFailure:true（拿异常详情：哪个参数不匹配）
        static void Section_C0c_GenericDelegateBindProbe()
        {
            Log("-- C0c. generic-delegate bind probe --");
            try
            {
                // ⚠ RunAndCollect 在 Unity Mono 上疑似污染后续 JIT（2026-09-23 实锤：C0c 探测后
                //   B3 的 PointerFunc<IntPtr,ByRefParam> 泛型 Invoke JIT 踩 marshal.c:3687 断言崩；
                //   上轮无 C0c 时 B3 PASS）。探测程序集改用 Run（非 collectible，无回收副作用）。
                var ab = DefineDynamicAssemblyCompat("C0cProbe", AssemblyBuilderAccess.Run);
                var mod = ab.DefineDynamicModule("C0cMod");
                var tb = mod.DefineType("C0cType",
                    System.Reflection.TypeAttributes.Abstract | System.Reflection.TypeAttributes.Sealed);
                // static ref ByRefParam M(Delegate d, out ByRefParam r, in ByRefParam u)
                // —— 签名与 ConcreteDel / GenDel<ByRefParam, ByRefParam> 实例化都匹配
                var mb = tb.DefineMethod("M",
                    System.Reflection.MethodAttributes.Public | System.Reflection.MethodAttributes.Static,
                    typeof(ByRefParam).MakeByRefType(),
                    new[] { typeof(Delegate), typeof(ByRefParam).MakeByRefType(), typeof(ByRefParam).MakeByRefType() });
                var il = mb.GetILGenerator();
                il.Emit(OpCodes.Ldarg_1);   // return ref r（out 槽地址——签名匹配的最小 body）
                il.Emit(OpCodes.Ret);
                var t = tb.CreateType();
                var m = t.GetMethod("M");

                var d1 = Delegate.CreateDelegate(typeof(ConcreteDel), m);
                Log($"  C0c-1 non-generic delegate: {(d1 != null ? "BOUND" : "NULL")}");

                var d2 = Delegate.CreateDelegate(typeof(GenDel<ByRefParam, ByRefParam>), m);
                Log($"  C0c-2 generic delegate: {(d2 != null ? "BOUND" : "NULL")}");

                try
                {
                    var d3 = Delegate.CreateDelegate(typeof(GenDel<ByRefParam, ByRefParam>), m, true);
                    Log("  C0c-3 generic+throwOnBindFailure: BOUND");
                }
                catch (ArgumentException aex)
                {
                    Log($"  C0c-3 generic+throwOnBindFailure: {aex.Message}");
                }

                // C0c-4: 泛型类嵌套 delegate（FreeFunc<R,U1>.DynamicInvoker 的精确形态）——
                // GenDel 嵌套在非泛型类里 BOUND；FreeFunc 的嵌套在泛型类里 null——隔离这个差异。
                var d4 = Delegate.CreateDelegate(
                    typeof(GenHost<ByRefParam, ByRefParam>.NestedDel), m);
                Log($"  C0c-4 generic-class nested delegate: {(d4 != null ? "BOUND" : "NULL")}");

                Check("C0c-1 non-generic delegate binds", d1 != null);
                // C0c-2/C0c-4 不做 PASS/FAIL 判定——纯诊断（结果决定 dll 侧修复方向）
            }
            catch (Exception ex)
            {
                Skip("C0c generic-delegate bind probe", $"{ex.GetType().Name}: {ex.Message}");
            }
        }

        // C0b 探测结果（E 段引用：Mono 下 Emit fallback 不活时，引用类型 PointerFunc
        // 的织入 Invoke（含 unmanaged calli）JIT 编译必崩——E2 必须避开）
        static bool s_emitFallbackOk;

        // ---------------- A. 基本调用形态（PointerFunc fnptr 路径） ----------------

        static void Section_BasicCalls()
        {
            Log("-- A. PointerFunc basic calls --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("A.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(AddInts), out var fnAdd))
            {
                Skip("A.*", FnPtrSkipReason("GetFunctionPointer unavailable on this platform"));
                return;
            }

            // A1: 非泛型 Invoke，严格类型匹配
            var add = MakePF<int, int, int>(fnAdd);
            Check("A1 non-generic Invoke(int,int):int", add.Invoke(20, 22) == 42);

            // A2: 泛型 Invoke(in P1, in P2)
            int a = 20, b = 22;
            Check("A2 generic Invoke(in, in)", add.Invoke(in a, in b) == 42);

            // A3/A4: IntPtr 按值传递（旧版 API：仅 ByRefParam 才传地址）
            if (TryGetFnPtr(nameof(EchoIntPtr), out var fnEcho))
            {
                var echo = MakePF<IntPtr, IntPtr>(fnEcho);
                Check("A3 IntPtr by value (non-generic)",
                    echo.Invoke((IntPtr)0x1234) == (IntPtr)0x1234);
                IntPtr v = (IntPtr)0x4321;
                Check("A4 IntPtr by value (generic)", echo.Invoke(in v) == (IntPtr)0x4321);
            }

            // A5: void 返回（R=VoidReturn），0 参数（2026-09-23 同步）
            // ⚠ 崩点归因（pointerdiag 实锤）：崩在**静态织入的 calli**（Generator 注入的
            //   非泛型 Invoke() 方法体）——A5a 直调非泛型 Invoke() 当场崩，跳转地址 ≠
            //   fnPtr。运行时 Emit 的产物是发射时具体签名的普通方法（不在共享桶）——
            //   不受共享 bug 影响。Emit 复活（R9）后泛型 Invoke(out r) 走 Emit 产物，
            //   理论上已不碰织入 calli。本用例解除 SKIP 验证该推断：
            //   A5b PASS = 泛型路径已被 Emit 修复；A5b 崩 = Emit 产物在共享实例化下也有问题。
            if (TryGetFnPtr(nameof(NoParamVoid), out var fnVoid))
            {
                var inv = MakePF<VoidReturn>(fnVoid);
                // A5b: 泛型 Invoke(out r)——Emit 产物路径（具体签名 calli，不在共享桶）
                VoidReturn r;
                ref var ret = ref inv.Invoke(out r);
                Check("A5b PF generic Invoke (Emit path): r == default", r == null);
                Check("A5b PF generic Invoke (Emit path): ref r == r", Ref.RefEquals(ref ret, ref r));
                // A5a（非泛型 Invoke() 直调）挪到 RunAll 尾部探针（R14）——native 崩不丢其他结果。
            }

            // A6: void 返回（R=VoidReturn），ByRef 参数（2026-09-23 同步）
            if (TryGetFnPtr(nameof(WriteIntPtr), out var fnWrite))
            {
                var inv = MakePF<VoidReturn, ByRefParam>(fnWrite);
                IntPtr box = IntPtr.Zero;
                inv.Invoke(out _, in box);   // U1=ByRefParam: native receives &box, writes 0x1234
                Check("A6 PF void ret, ByRef param: *box == 0x1234", box == (IntPtr)0x1234);
            }
        }

        // ---------------- B. ByRef 语义 ----------------

        static void Section_ByRefSemantics()
        {
            Log("-- B. ByRef semantics --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("B.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(IncrByRefInt), out var fnIncr))
            {
                Skip("B.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            // B1: U1=ByRefParam：native 收到 &local（ref int 参数），函数内自增
            var incr = MakePF<int, ByRefParam>(fnIncr);
            int local = 41;
            int r1 = incr.Invoke(in local);
            Check("B1 ByRef param: callee increments caller variable",
                local == 42 && r1 == 42, $"local={local}, ret={r1}");

            // B2: R=ByRefParam：ref 返回。新版 dll（2026/9/22 换代 API）的正确形态：
            //   ref R Invoke<P>(out R r, in P p) —— ref 返回直接是目标 byref（按 ByRefParam 视图）。
            //   RawRef 还原（一层）：Of(ref ret) → _Ref = 目标变量地址；GetRef<int>() → 还原 byref。
            // ⚠ 便利包装 Invoke<P>(in P) 对 R=ByRefParam 是 ldobj 值语义（_Address=目标内存头8字节），
            //   地址丢失，不可用于 ref 返回场景。
            if (TryGetFnPtr(nameof(RefReturnOfRef), out var fnRefRet))
            {
                try
                {
                    var rr = MakePF<ByRefParam, ByRefParam>(fnRefRet);
                    int local2 = 7;
                    ByRefParam r;
                    ref ByRefParam ret = ref rr.Invoke(out r, in local2);
                    ref int asInt = ref RawRef.Of(ref ret).GetRef<int>();
                    Check("B2 ref return: byref points to caller var",
                        Ref.RefEquals(ref asInt, ref local2) && asInt == 7);
                }
                catch (Exception ex)
                {
                    Skip("B2 ref return", $"R=ByRefParam Invoke threw {ex.GetType().Name} " +
                        $"(Emit unavailable / weaving path unimplemented on this dll/platform?)");
                }
            }

            // B3: ref IntPtr 参数（堆字段地址透传）
            if (TryGetFnPtr(nameof(ReadRefCookie), out var fnReadRef))
            {
                var rd = MakePF<IntPtr, ByRefParam>(fnReadRef);
                var slot = new Slot { Addr = (IntPtr)0xABCD };
                IntPtr r3 = rd.Invoke(in slot.Addr);
                Check("B3 ref IntPtr param reads heap field", r3 == (IntPtr)0xABCD);
            }

            // B4: ByRefParam 参数强制 ByValue（SetParamFlag）——native 收到值本身（2026-09-23 同步）
            if (TryGetFnPtr(nameof(EchoIntPtr), out var fnEcho2))
            {
                var inv = MakePF<IntPtr, ByRefParam>(fnEcho2);
                inv.SetParamFlag(0, ParamFlag.ByValue);   // force param by-value despite ByRefParam
                inv.SetParamFlag(-1, ParamFlag.ByValue);   // force return by-value too
                IntPtr arg = (IntPtr)0xABCD;
                IntPtr r;
                ref var ret = ref inv.Invoke(out r, in arg);
                // ByValue: native receives the value 0xABCD, echoes it back.
                Check("B4 PF ByRefParam forced ByValue: r == 0xABCD", r == (IntPtr)0xABCD);
                Check("B4 PF ByRefParam forced ByValue: ref r == r", Ref.RefEquals(ref ret, ref r));
            }
        }

        // ---------------- C. 多参数 ----------------

        static void Section_MultiParam()
        {
            Log("-- C. Multi-param --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("C.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(AddFour), out var fnAdd4))
            {
                Skip("C.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            var f4 = MakePF<int, int, int, int, int>(fnAdd4);
            Check("C1 four params, non-generic", f4.Invoke(1, 2, 3, 4) == 10);

            int a = 1, b = 2, c = 3, d = 4;
            Check("C2 four params, generic", f4.Invoke(in a, in b, in c, in d) == 10);

            // C3: 混合参数——U1=ByRefParam（native 收 &box），U2=int（收值）（2026-09-23 同步）
            if (TryGetFnPtr(nameof(RefPlusVal), out var fnMix))
            {
                var inv = MakePF<int, ByRefParam, int>(fnMix);
                IntPtr box = (IntPtr)0x1111;
                int arg2 = 100;
                int r;
                ref var ret = ref inv.Invoke(out r, in box, in arg2);
                Check("C3 mixed params (ByRef + ByValue): r == 0x1111 + 100", r == 0x1111 + 100);
                Check("C3 mixed params: ref r == r", Ref.RefEquals(ref ret, ref r));
            }
        }

        // ---------------- D. FreeFunc（delegate 包装，IL2CPP 主路径） ----------------

        static unsafe void Section_FreeFuncWrapper()
        {
            Log("-- D. FreeFunc delegate wrapper --");

            // D0: 路径探针——泛型 Invoke 走 Emit 还是 Fallback（2026-09-23）
            // 2026-09-24 R17 解除 Mono 跳过：探针曾"污染"后续共享泛型 JIT 的根因是
            //   fallback 链不稳定（织入基元 native wrapper 断言）——R17 后 Emit 全活、
            //   fallback 不再被走，污染源消失。恢复双判据探针，预期 stack=emit +
            //   disabled=false（与 C0g 双 LIVE 互补的运行时判定）。
            {
                string stackPath = "unknown";
                Func<int, int> probe = (int x) =>
                {
                    var st = Environment.StackTrace;
                    if (st.Contains("InvokeFallback")) stackPath = "fallback";
                    else if (st.Contains("DynamicInvoke")) stackPath = "emit";
                    return x;
                };
                var inv0 = new FreeFunc<int, int>(probe);
                int r0;
                inv0.Invoke(out r0, 5);
                var fld = typeof(Mod.LowLevel.FreeInvokableBase).GetField("_IsDynamicCodeDisabled",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                bool disabled = fld != null && (bool)fld.GetValue(inv0);
                Log($"  D0 path probe: stack={stackPath}, _IsDynamicCodeDisabled={disabled}" +
                    (disabled ? " (Emit failed → fallback)" : " (Emit live)"));
                if (stackPath == "fallback" && !disabled)
                {
                    Log("  D0 ⚠ dll bug found: CreateDelegate returned null silently — Emit dead, generic Invoke runs on fallback");
                }
                Check("D0 path probe: invoke works", r0 == 5);
            }

            // D1: Func<U,R> 包装，非泛型 Invoke
            Func<IntPtr, IntPtr> echo = EchoIntPtr;
            var f1 = new FreeFunc<IntPtr, IntPtr>(echo);
            Check("D1 Func wrapper, non-generic", f1.Invoke((IntPtr)0x4321) == (IntPtr)0x4321);

            // D2: Func 包装，泛型 Invoke（in 传参）
            IntPtr a2 = (IntPtr)0x8888;
            Check("D2 Func wrapper, generic", f1.Invoke(in a2) == (IntPtr)0x8888);

            // D3: Action 包装（R=VoidReturn）
            int observed = 0;
            Action<int> act = x => { observed = x; };
            var f3 = new FreeFunc<VoidReturn, int>(act);
            f3.Invoke(77);
            Check("D3 Action wrapper side effect", observed == 77);

            // D4: 泛型 Invoke + U=IntPtr：注入 IL 对堆字段 ldobj（byref 槽活跃于注入帧），传值给 delegate
            var slot = new Slot { Addr = (IntPtr)0x5678 };
            Check("D4 generic Invoke derefs heap field (ldobj path)",
                f1.Invoke(in slot.Addr) == (IntPtr)0x5678);

            // D5: 多参 Func
            Func<int, int, int> addDel = AddInts;
            var f5 = new FreeFunc<int, int, int>(addDel);
            Check("D5 multi-param Func wrapper", f5.Invoke(20, 22) == 42);

            // D6: IntPtr 参数强制 ByValue（SetParamFlag）——delegate 收值本身（2026-09-23 同步）
            {
                var inv = new FreeFunc<IntPtr, IntPtr>(EchoIntPtr);
                inv.SetParamFlag(0, ParamFlag.ByValue);
                inv.SetParamFlag(-1, ParamFlag.ByValue);   // IntPtr 返回默认 ByRef，强制 ByValue
                IntPtr arg = (IntPtr)0xDEAD;
                IntPtr r;
                ref var ret = ref inv.Invoke(out r, in arg);
                Check("D6 FF IntPtr param forced ByValue: r == 0xDEAD", r == (IntPtr)0xDEAD);
                Check("D6 FF IntPtr param forced ByValue: ref r == r", Ref.RefEquals(ref ret, ref r));
            }

            // D7: void 返回（R=VoidReturn）+ ByRef 参数——delegate 收 &slot 写 0xBEEF（2026-09-23 同步）
            {
                IntPtr slot7 = IntPtr.Zero;
                IntPtr slotAddr;
                slotAddr = (IntPtr)(long)&slot7;   // 栈变量取地址（unsafe，无需 fixed）
                Func<IntPtr, VoidReturn> del = static (IntPtr p) =>
                {
                    *(IntPtr*)p = (IntPtr)0xBEEF;
                    return null;
                };
                var inv = new FreeFunc<VoidReturn, IntPtr>(del);
                VoidReturn r;
                ref var ret = ref inv.Invoke(out r, in slotAddr);
                Check("D7 FF void ret: r == default", r == null);
                Check("D7 FF void ret: *slot == 0xBEEF", slot7 == (IntPtr)0xBEEF);
                Check("D7 FF void ret: ref r == r", Ref.RefEquals(ref ret, ref r));
            }

            // D8: byref 返回（R=ByRefParam）——委托返回 ByRefParam（位模式=&slot），
            //     ByRef 分支：*r = &slot，ret refs slot（GC 安全的有效地址）（2026-09-23 同步）
            {
                IntPtr slot8 = (IntPtr)0x9999;
                IntPtr slotAddr;
                slotAddr = (IntPtr)(long)&slot8;
                Func<IntPtr, ByRefParam> del = static (IntPtr p) =>
                {
                    // IntPtr → ByRefParam 位模式转换（Of(ref p) 包 p 槽 → 同地址视图 → 读位模式；
                    // 与 dotnet 侧 Unsafe.As<IntPtr, ByRefParam>(ref p) 一致——不是解引用 p）
                    return RawRef.Of(ref p).GetRef<ByRefParam>();
                };
                var inv = new FreeFunc<ByRefParam, IntPtr>(del);
                ByRefParam r;
                ref var ret = ref inv.Invoke(out r, in slotAddr);
                // u1 槽持有 &slot（ByValue：delegate 收槽内容）——委托把 p=&slot 按 ByRefParam 回传。
                // ByRef ret 分支：*r = &slot；ret refs slot（有效地址，GC 安全）。
                var rAddr = RawRef.Of(ref r).GetValue<IntPtr>();
                Check("D8 FF byref ret: r == &slot", rAddr == slotAddr);
                var retVal = RawRef.Of(ref ret).GetRef<IntPtr>();
                Check("D8 FF byref ret: *ret == 0x9999 (ret refs slot)", retVal == (IntPtr)0x9999);
            }

            // D9: Action + ByRef 参数——action 收 &slot 写 0xCAFE（FreeAction 已并入 FreeFunc）（2026-09-23 同步）
            {
                IntPtr slot9 = IntPtr.Zero;
                IntPtr slotAddr;
                slotAddr = (IntPtr)(long)&slot9;
                Action<IntPtr> act9 = static (IntPtr p) =>
                {
                    *(IntPtr*)p = (IntPtr)0xCAFE;
                };
                var inv = new FreeFunc<VoidReturn, IntPtr>(act9);
                VoidReturn r;
                ref var ret = ref inv.Invoke(out r, in slotAddr);
                Check("D9 FF Action ByRef param: *slot == 0xCAFE", slot9 == (IntPtr)0xCAFE);
                Check("D9 FF Action ByRef param: r == default", r == null);
                Check("D9 FF Action ByRef param: ref r == r", Ref.RefEquals(ref ret, ref r));
            }

            // D10: FF byval ret + ByRef 语义参数（delegate 收 &target 解引用）（2026-09-23 同步）
            //     U1=IntPtr（ByValue 传值）：Px 槽持有 &target，delegate 收到地址值后手动解引用。
            {
                long target = 0x3333;
                IntPtr targetAddr;
                targetAddr = (IntPtr)(long)&target;
                Func<IntPtr, int> del = static (IntPtr p) =>
                {
                    return (int)(*(IntPtr*)p).ToInt64();
                };
                var inv = new FreeFunc<int, IntPtr>(del);
                int r;
                inv.Invoke(out r, in targetAddr);   // delegate receives &target, reads 0x3333
                Check("D10 FF byval ret, ByRef param: r == 0x3333", r == 0x3333);
            }
        }

        // ---------------- F. unmanaged calli 双约定（2026-09-23 同步新增） ----------------
        // dll 织入/Emit 的 Invoke 按 _IsUnmanaged 分支：false → managed calli（0x00），
        // true → unmanaged calli（0x09）。GetFunctionPointerForDelegate 产生 unmanaged
        // thunk——unmanaged 分支的语义正确目标。JIT 与 IL2CPP 都可测。

        static void Section_F_UnmanagedCalli()
        {
            Log("-- F. managed/unmanaged calli --");
            // 2026-09-28 R19 解除 IL2CPP 硬守卫：原死因（织入 calli 被转译为 RuntimeMethod*
            // dereference）已被 conv=1 修复根治（Generator callsite 0x09 → 0x01 Cdecl——
            // Roslyn delegate* unmanaged[Cdecl] 同款字节）。O 段 11 用例（同一条织入
            // unmanaged calli 路）已在纯 IL2CPP 与 HybridCLR 双双全 PASS 实锤。
            // F1/F2 的 fp 是 GetFunctionPointerForDelegate thunk（ReversePInvokeWrapper），
            // 与 O 段的真 C 函数互补——x64 单一调用约定，无 ABI 风险。

            // F1/F2: unmanaged calli（(pfn, true) + wrapper）。
            // 2026-09-24 R17 解除 SKIP：非泛型 Invoke 已 Emit 化（trampoline：运行时
            //   Emit 优先 → InvokePlain 织入 fallback）——Emit 产物的 unmanaged calli
            //   是 Cdecl 动态方法（非织入 0x09 callsite），不再触发 Mono trampoline
            //   断言。2026-09-23 的 native 崩点是织入 unmanaged calli 直调；R17 后该
            //   路径仅在 Emit 死亡时作为兜底存在。
            IntPtr pfnInc = Marshal.GetFunctionPointerForDelegate(s_dIncInt);
            {
                var inv = new PointerFunc<int, int>(pfnInc, true);
                int r = inv.Invoke(41);
                Check("F1 PF non-generic unmanaged calli: r == 42", r == 42);
            }

            // F2: 泛型 Invoke，unmanaged calli——JIT 上走运行时 Emit（PointerFuncEmit Cdecl），
            //     IL2CPP 上 Emit 不可用 → fallback → 非泛型织入 Invoke → 同样 unmanaged 分支。
            {
                var inv = new PointerFunc<int, int>(pfnInc, true);
                int r;
                ref var ret = ref inv.Invoke<int>(out r, 41);
                Check("F2 PF generic unmanaged calli: r == 42", r == 42);
                Check("F2 PF generic unmanaged calli: ret == 42", ret == 42);
            }

            // F3: managed 对照（仅 JIT——IL2CPP 无托管代码入口指针可取）：
            //     FnOf 返回托管入口 + 默认构造（IsUnmanaged=false）→ managed calli。
            if (!s_isIL2CPP && TryGetFnPtr(nameof(IncInt), out var fnManaged))
            {
                var inv = MakePF<int, int>(fnManaged);
                int r = inv.Invoke(41);
                Check("F3 PF non-generic managed calli (JIT): r == 42", r == 42);
            }
            else
            {
                Skip("F3 PF managed calli", "managed fnptr unavailable (IL2CPP)");
            }
        }

        // ---------------- E. 泛型 Invoke 类型不匹配（ABI 兼容透传） ----------------
        // ⚠ Mono 运行时（Unity Editor）实锤崩溃（2026-09-23 Editor.log）：
        //   E1 的 P1=E8（enum）触发泛型 Invoke 实例化链（Invoke<E8> → ConvertRef<E8,int>
        //   → 织入的 ConvertRefToAddress<T>(in T&)），Mono 在 marshal.c:3687 断言
        //   `!sig->has_type_parameters' not met —— 为含泛型变量的签名生成 native
        //   marshal wrapper → Editor 直接 Crash（native 断言，无法 catch）。
        //   A-D 段 19 用例全 PASS 证明核心链路 OK；仅 enum 泛型参数实例化踩中 Mono bug。
        //   CoreCLR/net472 无此问题（dotnet 侧 PlatformCompatTest 有对照用例）。
        //   → Mono 运行时下整段 SKIP；IL2CPP/CoreCLR 正常执行。

        static void Section_GenericInvokeTypeMismatch()
        {
            Log("-- E. Generic Invoke type mismatch --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("E.*", Il2CppCalliSkipReason); return; }

            // E1: U1=int，调用时 P1=E8（enum 底层 int，ABI 兼容）。
            // ⚠ Mono 专属崩溃（2026-09-23 Editor.log 实锤）：enum 作泛型 Invoke 类型
            //   参数触发实例化链 Invoke<E8> → ConvertRef<E8,int> → 织入的
            //   ConvertRefToAddress<T>(in T&)，Mono 在 marshal.c:3687 断言
            //   `!sig->has_type_parameters' not met → native 断言无法 catch →
            //   Editor 直接 Crash。与 calli 约定无关（managed/unmanaged 都会实例化
            //   同一条链）——Mono 下只能 SKIP。CoreCLR/net472 实测 PASS（对照用例
            //   在 dotnet 侧 PlatformCompatTest，33/33）。
            // ⚠ 2026-09-24 R12 解除 SKIP（P2-A 验证用例）：基元已纯 C# 化
            //   （ConvertRefToAddress/ConvertAddressToRef 不再是 Cecil 织入的恒等直通 IL，
            //   改为 Roslyn 原生编译的 Unsafe 实现）——Cecil 织入产物 + gsharedvt 共享 +
            //   含泛型变量签名 → marshal.c:3687 断言的链条已断。实跑验证：
            //   PASS = P2-A 生效；崩 = 断言链还有别的触发点。
            if (!TryGetFnPtr(nameof(EnumToInt), out var fnEnum))
            {
                Skip("E1", FnPtrSkipReason("fnptr unavailable"));
            }
            else
            {
                var f = MakePF<int, int>(fnEnum);
                E8 e = E8.B;
                Check("E1 enum→int mismatch", f.Invoke(in e) == 7);
            }

            if (!TryGetFnPtr(nameof(TypeNameOf), out var fnName))
            {
                Skip("E2", FnPtrSkipReason("fnptr unavailable"));
            }
            // ⚠ 2026-09-24 R12 解除 SKIP（P2-A 第二验证用例）：基元已纯 C# 化
            //   （ConvertRefToAddress/ConvertAddressToRef 不再是 Cecil 织入 IL）——
            //   ConvertRef<Dog,object> 的 gsharedvt 共享链条不再踩织入基元断言
            //   （E1 PASS 实锤同族链条修复）。实跑验证。
            else
            {
                // E2: U1=object，调用时 P1=Dog（引用兼容）
                var f = MakePF<string, object>(fnName);
                Dog dog = new Dog();
                Check("E2 Dog→object mismatch", f.Invoke(in dog) == nameof(Dog));
            }
        }

        // ---------------- GC-1. fnptr 跨 GC 稳定性 ----------------

        static void Section_Gc_FnPtrStability()
        {
            Log("-- GC-1. fnptr stability across GC --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("GC-1.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(AddInts), out var fnAdd))
            {
                Skip("GC-1.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            var add = MakePF<int, int, int>(fnAdd);
            Check("GC-1a before GC", add.Invoke(20, 22) == 42);

            // 强制多代压缩 GC + 垃圾分配
            for (int i = 0; i < 3; i++)
            {
                var junk = new byte[1024];
                junk[0] = 1;
                GC.Collect(0, GCCollectionMode.Forced, true, true);
                GC.Collect(1, GCCollectionMode.Forced, true, true);
                GC.Collect(2, GCCollectionMode.Forced, true, true);
            }

            Check("GC-1b after compacting GC, same fnptr still valid",
                add.Invoke(20, 22) == 42);
            TryGetFnPtr(nameof(AddInts), out var fnAdd2);
            Check("GC-1c entry point unchanged", fnAdd == fnAdd2,
                $"old=0x{(long)fnAdd:X}, new=0x{(long)fnAdd2:X}");
        }

        // ---------------- GC-2. 目标方法体内触发 GC（安全点覆盖） ----------------

        static void Section_Gc_InsideTarget()
        {
            Log("-- GC-2. GC inside target method --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("GC-2.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(AllocAndGc), out var fnAlloc))
            {
                Skip("GC-2.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            var f = MakePF<int, int>(fnAlloc);
            int r1 = f.Invoke(2000);
            int r2 = f.Invoke(2000);
            Check("GC-2 deterministic result across GC-ing target",
                r1 == r2 && r1 != 0, $"r1={r1}, r2={r2}");
        }

        // ---------------- GC-3. ref 返回语义（GC 前后各验证，不持有伪装引用跨 GC） ----------------

        static void Section_Gc_RefReturnSemantics()
        {
            Log("-- GC-3. ref return semantics across GC --");
            if (s_isIL2CPP && !s_il2cppPfWorks) { Skip("GC-3.*", Il2CppCalliSkipReason); return; }

            if (!TryGetFnPtr(nameof(RefReturnOfRef), out var fnRefRet))
            {
                Skip("GC-3.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            var rr = MakePF<ByRefParam, ByRefParam>(fnRefRet);

            // GC-3a: 返回 byref 正确指向调用方变量（out+ref 形态 + RawRef 一层还原，见 B2 注释）
            try
            {
                int local = 5;
                ByRefParam r;
                ref ByRefParam ret = ref rr.Invoke(out r, in local);
                ref int asInt = ref RawRef.Of(ref ret).GetRef<int>();
                Check("GC-3a ref return points to caller var",
                    Ref.RefEquals(ref asInt, ref local) && asInt == 5);
            }
            catch (Exception ex)
            {
                Skip("GC-3a ref return", $"threw {ex.GetType().Name}");
                return;
            }

            // GC-3b: 压缩 GC 后再次 Invoke 仍正确（ref 返回不跨 GC 持有——每次 Invoke 重新获取）
            for (int i = 0; i < 2; i++)
            {
                var junk = new byte[512];
                junk[0] = 1;
                GC.Collect(0, GCCollectionMode.Forced, true, true);
                GC.Collect(1, GCCollectionMode.Forced, true, true);
                GC.Collect(2, GCCollectionMode.Forced, true, true);
            }
            int local99 = 99;
            {
                ByRefParam r;
                ref ByRefParam ret2 = ref rr.Invoke(out r, in local99);
                ref int asInt2 = ref RawRef.Of(ref ret2).GetRef<int>();
                Check("GC-3b ref return correct after compacting GC",
                    Ref.RefEquals(ref asInt2, ref local99) && asInt2 == 99);
            }
        }

        // ---------------- GC-4. FreeFunc 跨 GC 存活 ----------------

        static void Section_Gc_FreeFuncSurvival()
        {
            Log("-- GC-4. FreeFunc survival across GC --");

            Func<IntPtr, IntPtr> echo = EchoIntPtr;
            var f = new FreeFunc<IntPtr, IntPtr>(echo);
            Check("GC-4a before GC", f.Invoke((IntPtr)0x777) == (IntPtr)0x777);

            GC.Collect(2, GCCollectionMode.Forced, true, true);
            var junk = new byte[256];
            GC.Collect(0, GCCollectionMode.Forced, true, true);

            Check("GC-4b after GC, wrapper + delegate still callable",
                f.Invoke((IntPtr)0x777) == (IntPtr)0x777);
        }

        // ---------------- GC-4c. FF byref ret 跨 GC 存活（2026-09-23 同步新增） ----------------
        // 委托返回 ByRefParam（位模式 = &slot，有效地址 GC 安全）——ByRef ret 分支：
        // *r = &slot，ret refs slot。循环内分配垃圾 + 周期 GC.Collect，验证每次
        // Invoke 后 byref 仍指向正确槽、槽值不变（对应 dotnet 侧 GC FF byref ret 用例）。

        static unsafe void Section_Gc_FreeFuncByRefRetSurvival()
        {
            Log("-- GC-4c. FF byref ret survives GC --");

            IntPtr slot = (IntPtr)0xBEEF;
            Func<IntPtr, ByRefParam> del = static (IntPtr p) =>
            {
                // IntPtr → ByRefParam 位模式转换（Of(ref p) 包 p 槽读位模式，非解引用）
                return RawRef.Of(ref p).GetRef<ByRefParam>();
            };
            var inv = new FreeFunc<ByRefParam, IntPtr>(del);
            for (int i = 0; i < 200_000; i++)
            {
                var garbage = new byte[64];
                garbage[0] = (byte)i;
                if ((i & 0x3FFF) == 0)
                {
                    GC.Collect(0, GCCollectionMode.Forced, true, true);
                    GC.Collect(1, GCCollectionMode.Forced, true, true);
                }
                IntPtr slotAddr;
                slotAddr = (IntPtr)(long)&slot;
                ByRefParam r;
                ref var ret = ref inv.Invoke(out r, in slotAddr);
                var rAddr = RawRef.Of(ref r).GetValue<IntPtr>();
                var retVal = RawRef.Of(ref ret).GetValue<IntPtr>();   // ret refs slot → reads slot
                if (rAddr != slotAddr || retVal != (IntPtr)0xBEEF)
                {
                    Check("GC-4c FF byref ret survives GC", false,
                        $"iter={i}: rAddr=0x{(long)rAddr:X} slotAddr=0x{(long)slotAddr:X} retVal=0x{(long)retVal:X}");
                    return;
                }
            }
            Check("GC-4c FF byref ret survives GC", true);
        }

        // ---------------- GC-5. ByRef 压缩平移 stress ----------------
        // 堆字段地址（&slot.Addr / &arr[0]）作为参数传入注入 IL；
        // 并发压缩 GC 搬移对象时，注入帧的 interior byref 槽必须被 GC 改写，
        // 被调方才能读到正确 cookie。三条路径：
        //   A: PointerFunc + U=ByRefParam（byref 透传给 native）
        //   B: PointerFunc + U=IntPtr（注入 ldobj 解引用——byref 槽同样活跃于注入帧）
        //   C: FreeFunc  + U=IntPtr（同 B，经 delegate）

        static void Section_Gc_ByRefRelocationStress(int durationMs, bool parallel)
        {
            Log($"-- GC-5. ByRef relocation stress ({(parallel ? "parallel" : "serial")}, {durationMs} ms) --");

            if (!TryGetFnPtr(nameof(ReadRefCookie), out var fnReadRef) ||
                !TryGetFnPtr(nameof(EchoIntPtr), out var fnEcho))
            {
                Skip("GC-5.*", FnPtrSkipReason("fnptr unavailable"));
                return;
            }

            int gc0Before = GC.CollectionCount(0), gc1Before = GC.CollectionCount(1),
                gc2Before = GC.CollectionCount(2);

            if (parallel)
                GcStressParallel(fnReadRef, fnEcho, durationMs);
            else
                GcStressSerial(fnReadRef, fnEcho, durationMs);

            int g0 = GC.CollectionCount(0) - gc0Before, g1 = GC.CollectionCount(1) - gc1Before,
                g2 = GC.CollectionCount(2) - gc2Before;
            Check($"GC-5 stress completed with 0 errors (gen0={g0}, gen1={g1}, gen2={g2})",
                s_stressErrors == 0 && s_stressCalls > 0,
                $"{s_stressErrors} errors / {s_stressCalls} calls");
        }

        static long s_stressCalls, s_stressErrors;

        static void StressBodyOnce(ref Slot slot, ref IntPtr[] arr, long cookie)
        {
            slot.Addr = (IntPtr)cookie;
            arr[0] = (IntPtr)cookie;

            // Path A: byref 透传（native: ReadRefCookie(ref IntPtr p) => p）
            IntPtr rA = s_invA.Value.Invoke(in slot.Addr);
            if (rA.ToInt64() != cookie) Interlocked.Increment(ref s_stressErrors);

            // Path B: ldobj 解引用（native: EchoIntPtr(IntPtr p) => p，收到字段值）
            IntPtr rB = s_invB.Value.Invoke(in slot.Addr);
            if (rB.ToInt64() != cookie) Interlocked.Increment(ref s_stressErrors);

            // Path C: FreeFunc 注入解引用后传 delegate
            IntPtr rC = s_invC.Value.Invoke(in arr[0]);
            if (rC.ToInt64() != cookie) Interlocked.Increment(ref s_stressErrors);

            Interlocked.Add(ref s_stressCalls, 3);
        }

        // invoker 缓存（每线程一份；Invoke 无状态，跨轮复用安全）
        static readonly ThreadLocal<PointerFunc<IntPtr, ByRefParam>> s_invA =
            new ThreadLocal<PointerFunc<IntPtr, ByRefParam>>(() => null);
        static readonly ThreadLocal<PointerFunc<IntPtr, IntPtr>> s_invB =
            new ThreadLocal<PointerFunc<IntPtr, IntPtr>>(() => null);
        static readonly ThreadLocal<FreeFunc<IntPtr, IntPtr>> s_invC =
            new ThreadLocal<FreeFunc<IntPtr, IntPtr>>(() => null);

        static void InitInvokers(IntPtr fnReadRef, IntPtr fnEcho)
        {
            // IL2CPP 下 wrapper 为 unmanaged thunk → MakePF 以 (fn, true) 构造（走 unmanaged calli）
            s_invA.Value = MakePF<IntPtr, ByRefParam>(fnReadRef);
            s_invB.Value = MakePF<IntPtr, IntPtr>(fnEcho);
            s_invC.Value = new FreeFunc<IntPtr, IntPtr>(EchoIntPtr);
        }

        static void GcStressSerial(IntPtr fnReadRef, IntPtr fnEcho, int durationMs)
        {
            InitInvokers(fnReadRef, fnEcho);
            var sw = Stopwatch.StartNew();
            long cookie = 0;
            var slot = new Slot();
            var arr = new IntPtr[4];

            while (sw.ElapsedMilliseconds < durationMs)
            {
                cookie++;
                if ((cookie & 511) == 0) { slot = new Slot(); arr = new IntPtr[4]; }
                StressBodyOnce(ref slot, ref arr, cookie);

                for (int g = 0; g < 16; g++)
                {
                    var junk = new byte[(int)(cookie + g) % 128 + 1];
                    junk[0] = (byte)cookie;
                }

                GC.Collect(0, GCCollectionMode.Forced, true, true);
                GC.Collect(1, GCCollectionMode.Forced, true, true);
                if ((cookie & 255) == 0)
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
            }
        }

        static void GcStressParallel(IntPtr fnReadRef, IntPtr fnEcho, int durationMs)
        {
            int workerCount = Environment.ProcessorCount * 2;
            var cts = new CancellationTokenSource();

            for (int w = 0; w < workerCount; w++)
            {
                Task.Run(() =>
                {
                    InitInvokers(fnReadRef, fnEcho);
                    long cookie = 0;
                    var slot = new Slot();
                    var arr = new IntPtr[4];
                    while (!cts.IsCancellationRequested)
                    {
                        cookie++;
                        if ((cookie & 511) == 0) { slot = new Slot(); arr = new IntPtr[4]; }
                        StressBodyOnce(ref slot, ref arr, cookie);

                        for (int g = 0; g < 32; g++)
                        {
                            var junk = new byte[(int)(cookie + g) % 128 + 1];
                            junk[0] = (byte)cookie;
                        }
                    }
                });
            }

            var control = Task.Run(() =>
            {
                int tick = 0;
                while (!cts.IsCancellationRequested)
                {
                    GC.Collect(0, GCCollectionMode.Forced, true, true);
                    GC.Collect(1, GCCollectionMode.Forced, true, true);
                    if ((tick++ & 7) == 0)
                        GC.Collect(2, GCCollectionMode.Forced, true, true);
                    Thread.Sleep(1);
                }
            });

            Thread.Sleep(durationMs);
            cts.Cancel();
            control.Wait();
        }
    }
}
