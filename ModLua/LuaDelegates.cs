using System;
using System.Runtime.InteropServices;

// AOT.MonoPInvokeCallbackAttribute: empty stand-in for the netstandard2.0 build.
// The il2cpp translator matches this attribute by its full name
// (AOT.MonoPInvokeCallbackAttribute) and generates a ReversePInvokeWrapper for
// each annotated static method - that wrapper table is what
// Marshal.GetFunctionPointerForDelegate consults under IL2CPP.
// When the same source is compiled inside a Unity project
// (UNITY_EDITOR / UNITY_STANDALONE defined), "using AOT" resolves to the real
// UnityEngine attribute instead. Kept internal on purpose: it is not exported
// to consumer projects, avoiding ambiguity (CS0104) with UnityEngine's
// same-named attribute; consumers (e.g. ModLua.Tests) carry their own internal
// copy - the PointerDelegateTestSuite precedent.
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

namespace Mod.Lua
{
    public static partial class LuaLib
    {
        /// <summary>
        /// The 7 function-pointer types of the Lua 5.5 C API (C# delegate shapes,
        /// all Cdecl).
        /// Inbound direction (C# -> Lua, e.g. lua_pushcclosure / lua_atpanic /
        /// lua_sethook): static method + [AOT.MonoPInvokeCallback(typeof(X))] +
        /// Marshal.GetFunctionPointerForDelegate. Keep the delegate instance alive
        /// in a static readonly field (on CoreCLR/Mono the thunk is bound to the
        /// delegate instance - losing the last root leaves it dangling), and cache
        /// the thunk address once (it is per-delegate; identity is stable).
        /// Under IL2CPP only static methods work (the ReversePInvokeWrapper is
        /// generated at compile time - there is no runtime closure; instance
        /// context must travel through Lua's upvalue/ud/ctx channels, e.g.
        /// GCHandle.ToIntPtr).
        /// </summary>

        /// <summary>lua_CFunction: int (*)(lua_State *L) - a C function registered with Lua.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int CFunction(IntPtr L);

        /// <summary>lua_KFunction: int (*)(lua_State *L, int status, lua_KContext ctx) - continuation.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int KFunction(IntPtr L, int status, IntPtr ctx);

        /// <summary>lua_Reader: const char* (*)(lua_State *L, void *ud, size_t *sz) - lua_load reader.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Reader(IntPtr L, IntPtr ud, IntPtr sz);

        /// <summary>lua_Writer: int (*)(lua_State *L, const void *p, size_t sz, void *ud) - lua_dump writer.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate int Writer(IntPtr L, IntPtr p, UIntPtr sz, IntPtr ud);

        /// <summary>lua_Alloc: void* (*)(void *ud, void *ptr, size_t osize, size_t nsize) - memory allocator.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Alloc(IntPtr ud, IntPtr ptr, UIntPtr osize, UIntPtr nsize);

        /// <summary>lua_WarnFunction: void (*)(void *ud, const char *msg, int tocont) - warning handler.</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WarnFunction(IntPtr ud, IntPtr msg, int tocont);

        /// <summary>lua_Hook: void (*)(lua_State *L, lua_Debug *ar) - debug hook (ar is a lua_Debug*).</summary>
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Hook(IntPtr L, IntPtr ar);
    }
}
