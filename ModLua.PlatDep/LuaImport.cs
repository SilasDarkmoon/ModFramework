using System;
using System.Runtime.InteropServices;

namespace Mod.Lua
{
    /// <summary>
    /// Direct bindings for the Lua 5.5.0 C API (lua.h / lauxlib.h / lualib.h;
    /// sources under 3rd/lua-5.5.0/src).
    /// Type mapping rules (most direct native-compatible types, no marshaling wrappers):
    ///   char* / void* / const void* / lua_State* / function pointers
    ///   (lua_CFunction, lua_Alloc, ...) -> IntPtr
    ///   size_t -> UIntPtr; unsigned -> uint
    ///   lua_Number -> double; lua_Integer -> long; lua_Unsigned -> ulong; lua_KContext -> IntPtr
    ///   pointer out-params (int* / size_t* / void**) -> IntPtr (IntPtr.Zero means C NULL)
    /// Numeric configuration matches the luaconf.h defaults:
    /// LUA_INT_LONGLONG + LUA_FLOAT_DOUBLE.
    ///
    /// Visibility: internal + InternalsVisibleTo(ModLua, ModLua.Tests) -
    /// LuaLib (ModLua) is the only public facade; consumers never touch
    /// this class directly.
    ///
    /// Platform flavors (selected by the Configuration-injected define in
    /// ModLua.PlatDep.csproj):
    ///   Default (Debug/Release/Release.Win64/Mac/Linux/Android) ->
    ///     DllImport("lua55"), dynamic-library mode.
    ///   MODLUA_IOS (Release.iOS) -> DllImport("__Internal"), static-linking mode
    ///     (the Lua static library must be linked into the main binary; under
    ///     IL2CPP, __Internal is a compile-time symbol reference that must be
    ///     resolvable at link time).
    /// </summary>
    internal static class LuaImport
    {
#if MODLUA_IOS
        // iOS/WebGL: Lua is statically linked into the executable;
        // "__Internal" refers to the current process' main module
        // (a compile-time symbol reference under IL2CPP).
        private const string LuaDllName = "__Internal";
#else
        // Dynamic-library mode: Windows -> lua55.dll;
        // Unix -> liblua55.so / liblua55.dylib (matched by DllImport probing).
        private const string LuaDllName = "lua55";
#endif

        #region lua.h - state manipulation

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_newstate(IntPtr f, IntPtr ud, uint seed);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_close(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_newthread(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_closethread(IntPtr L, IntPtr from);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_atpanic(IntPtr L, IntPtr panicf); // returns the previous panic function

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double lua_version(IntPtr L); // L may be IntPtr.Zero

        #endregion

        #region lua.h - basic stack manipulation

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_absindex(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_gettop(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_settop(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushvalue(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_rotate(IntPtr L, int idx, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_copy(IntPtr L, int fromidx, int toidx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_checkstack(IntPtr L, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_xmove(IntPtr from, IntPtr to, int n);

        #endregion

        #region lua.h - access functions (stack -> C)

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_isnumber(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_isstring(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_iscfunction(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_isinteger(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_isuserdata(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_type(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_typename(IntPtr L, int tp);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double lua_tonumberx(IntPtr L, int idx, IntPtr isnum); // int* isnum

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern long lua_tointegerx(IntPtr L, int idx, IntPtr isnum); // int* isnum

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_toboolean(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_tolstring(IntPtr L, int idx, IntPtr len); // size_t* len

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern ulong lua_rawlen(IntPtr L, int idx); // returns lua_Unsigned

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_tocfunction(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_touserdata(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_tothread(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_topointer(IntPtr L, int idx);

        #endregion

        #region lua.h - comparison and arithmetic functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_arith(IntPtr L, int op);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_rawequal(IntPtr L, int idx1, int idx2);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_compare(IntPtr L, int idx1, int idx2, int op);

        #endregion

        #region lua.h - push functions (C -> stack)

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushnil(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushnumber(IntPtr L, double n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushinteger(IntPtr L, long n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushlstring(IntPtr L, IntPtr s, UIntPtr len);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushexternalstring(IntPtr L, IntPtr s, UIntPtr len, IntPtr falloc, IntPtr ud); // new in Lua 5.5

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushstring(IntPtr L, IntPtr s);

        // 3rd parameter is a va_list (platform-specific type); on Windows x86/x64
        // va_list is just char*, so IntPtr is a valid representation.
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushvfstring(IntPtr L, IntPtr fmt, IntPtr argp);

        // Variadic function - DllImport cannot express the variable argument part:
        // this declaration is only usable when fmt contains no conversion specifiers.
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushfstring(IntPtr L, IntPtr fmt);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_pushfstring(IntPtr L, IntPtr fmt, IntPtr arg1);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushcclosure(IntPtr L, IntPtr fn, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushboolean(IntPtr L, int b);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_pushlightuserdata(IntPtr L, IntPtr p);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_pushthread(IntPtr L);

        #endregion

        #region lua.h - get functions (Lua -> stack)

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getglobal(IntPtr L, IntPtr name);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_gettable(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getfield(IntPtr L, int idx, IntPtr k);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_geti(IntPtr L, int idx, long n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_rawget(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_rawgeti(IntPtr L, int idx, long n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_rawgetp(IntPtr L, int idx, IntPtr p);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_createtable(IntPtr L, int narr, int nrec);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_newuserdatauv(IntPtr L, UIntPtr sz, int nuvalue);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getmetatable(IntPtr L, int objindex);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getiuservalue(IntPtr L, int idx, int n);

        #endregion

        #region lua.h - set functions (stack -> Lua)

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_setglobal(IntPtr L, IntPtr name);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_settable(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_setfield(IntPtr L, int idx, IntPtr k);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_seti(IntPtr L, int idx, long n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_rawset(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_rawseti(IntPtr L, int idx, long n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_rawsetp(IntPtr L, int idx, IntPtr p);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_setmetatable(IntPtr L, int objindex);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_setiuservalue(IntPtr L, int idx, int n);

        #endregion

        #region lua.h - 'load' and 'call' functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_callk(IntPtr L, int nargs, int nresults, IntPtr ctx, IntPtr k);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_pcallk(IntPtr L, int nargs, int nresults, int errfunc, IntPtr ctx, IntPtr k);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_load(IntPtr L, IntPtr reader, IntPtr dt, IntPtr chunkname, IntPtr mode);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_dump(IntPtr L, IntPtr writer, IntPtr data, int strip);

        #endregion

        #region lua.h - coroutine functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_yieldk(IntPtr L, int nresults, IntPtr ctx, IntPtr k);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_resume(IntPtr L, IntPtr from, int narg, IntPtr nres); // int* nres

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_status(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_isyieldable(IntPtr L);

        #endregion

        #region lua.h - warning-related functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_setwarnf(IntPtr L, IntPtr f, IntPtr ud);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_warning(IntPtr L, IntPtr msg, int tocont);

        #endregion

        #region lua.h - garbage-collection function

        // Variadic function: LUA_GCSTEP reads arg1 (size_t); LUA_GCPARAM reads
        // arg1 (param, int) and arg2 (value, int); all other commands read no
        // extra arguments (passing extras is harmless - cdecl cleanup is the
        // caller's job).
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_gc(IntPtr L, int what, UIntPtr arg1, int arg2);

        #endregion

        #region lua.h - miscellaneous functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_error(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_next(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_concat(IntPtr L, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_len(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint lua_numbertocstring(IntPtr L, int idx, IntPtr buff); // new in Lua 5.5; buff needs LUA_N2SBUFFSZ (64) bytes

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern UIntPtr lua_stringtonumber(IntPtr L, IntPtr s);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_getallocf(IntPtr L, IntPtr ud); // void** ud

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_setallocf(IntPtr L, IntPtr f, IntPtr ud);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_toclose(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_closeslot(IntPtr L, int idx);

        #endregion

        #region lua.h - debug API

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getstack(IntPtr L, int level, IntPtr ar); // lua_Debug* ar

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_getinfo(IntPtr L, IntPtr what, IntPtr ar); // lua_Debug* ar

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_getlocal(IntPtr L, IntPtr ar, int n); // const lua_Debug* ar

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_setlocal(IntPtr L, IntPtr ar, int n); // const lua_Debug* ar

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_getupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_setupvalue(IntPtr L, int funcindex, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_upvalueid(IntPtr L, int fidx, int n);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_upvaluejoin(IntPtr L, int fidx1, int n1, int fidx2, int n2);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void lua_sethook(IntPtr L, IntPtr func, int mask, int count);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr lua_gethook(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_gethookmask(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int lua_gethookcount(IntPtr L);

        #endregion

        #region lauxlib.h - auxiliary functions

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_checkversion_(IntPtr L, double ver, UIntPtr sz);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_getmetafield(IntPtr L, int obj, IntPtr e);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_callmeta(IntPtr L, int obj, IntPtr e);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_tolstring(IntPtr L, int idx, IntPtr len); // size_t* len

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_argerror(IntPtr L, int arg, IntPtr extramsg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_typeerror(IntPtr L, int arg, IntPtr tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_checklstring(IntPtr L, int arg, IntPtr l); // size_t* l

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_optlstring(IntPtr L, int arg, IntPtr def, IntPtr l); // size_t* l

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double luaL_checknumber(IntPtr L, int arg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern double luaL_optnumber(IntPtr L, int arg, double def);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern long luaL_checkinteger(IntPtr L, int arg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern long luaL_optinteger(IntPtr L, int arg, long def);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_checkstack(IntPtr L, int sz, IntPtr msg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_checktype(IntPtr L, int arg, int t);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_checkany(IntPtr L, int arg);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_newmetatable(IntPtr L, IntPtr tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_setmetatable(IntPtr L, IntPtr tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_testudata(IntPtr L, int ud, IntPtr tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_checkudata(IntPtr L, int ud, IntPtr tname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_where(IntPtr L, int lvl);

        // Variadic function - DllImport cannot express the variable argument part:
        // this declaration is only usable when fmt contains no conversion specifiers.
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_error(IntPtr L, IntPtr fmt);
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_error(IntPtr L, IntPtr fmt, IntPtr arg1);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_checkoption(IntPtr L, int arg, IntPtr def, IntPtr lst); // const char* const* lst

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_fileresult(IntPtr L, int stat, IntPtr fname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_execresult(IntPtr L, int stat);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_alloc(IntPtr ud, IntPtr ptr, UIntPtr osize, UIntPtr nsize); // new in Lua 5.5: the default allocator

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_ref(IntPtr L, int t);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_unref(IntPtr L, int t, int @ref); // C parameter name 'ref' is a C# keyword

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_loadfilex(IntPtr L, IntPtr filename, IntPtr mode);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_loadbufferx(IntPtr L, IntPtr buff, UIntPtr sz, IntPtr name, IntPtr mode);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_loadstring(IntPtr L, IntPtr s);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_newstate();

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern uint luaL_makeseed(IntPtr L); // new in Lua 5.5

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern long luaL_len(IntPtr L, int idx);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_addgsub(IntPtr B, IntPtr s, IntPtr p, IntPtr r); // luaL_Buffer* B

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_gsub(IntPtr L, IntPtr s, IntPtr p, IntPtr r);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_setfuncs(IntPtr L, IntPtr l, int nup); // const luaL_Reg* l

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaL_getsubtable(IntPtr L, int idx, IntPtr fname);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_traceback(IntPtr L, IntPtr L1, IntPtr msg, int level);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_requiref(IntPtr L, IntPtr modname, IntPtr openf, int glb);

        #endregion

        #region lauxlib.h - generic buffer manipulation

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_buffinit(IntPtr L, IntPtr B); // luaL_Buffer* B

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_prepbuffsize(IntPtr B, UIntPtr sz);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_addlstring(IntPtr B, IntPtr s, UIntPtr l);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_addstring(IntPtr B, IntPtr s);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_addvalue(IntPtr B);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_pushresult(IntPtr B);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_pushresultsize(IntPtr B, UIntPtr sz);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern IntPtr luaL_buffinitsize(IntPtr L, IntPtr B, UIntPtr sz);

        #endregion

        #region lualib.h - standard libraries

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_base(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_package(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_coroutine(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_debug(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_io(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_math(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_os(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_string(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_table(IntPtr L);

        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern int luaopen_utf8(IntPtr L);

        // New in Lua 5.5; luaL_openlibs(L) is luaL_openselectedlibs(L, ~0, 0).
        [DllImport(LuaDllName, CallingConvention = CallingConvention.Cdecl, ExactSpelling = true)]
        public static extern void luaL_openselectedlibs(IntPtr L, int load, int preload);

        #endregion
    }
}
