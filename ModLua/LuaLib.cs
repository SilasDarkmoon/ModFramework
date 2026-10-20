using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Mod.Lua
{
    /// <summary>
    /// ModLua convenience facade: forwards to the LuaImport platform bindings
    /// (ModLua.PlatDep) with C#-friendly shapes:
    ///  - lua_State* -> State (blittable one-field wrapper; default = NULL)
    ///  - struct pointer params (lua_Debug*, luaL_Buffer*, ...) -> ref
    ///    (address taken via RawRef; must be stack locals, see notes below)
    ///  - function-pointer params -> delegates from LuaDelegates.cs
    ///    (converted + kept alive via Keep); function-pointer returns ->
    ///    delegates via Marshal.GetDelegateForFunctionPointer (ToDel)
    ///  - names without the lua_/luaL_ prefixes
    ///  - gc() overloads per command shape
    ///  - the C-API convenience macros (lua_call, lua_pop, luaL_dostring, ...)
    ///    implemented as functions
    ///  - the LUA_* constants (pseudo-indices, type tags, status/op/GC/hook
    ///    codes) as LuaLib constants without the LUA_ prefix
    /// Name collisions after prefix stripping are resolved by overloading when
    /// the signatures differ (checkstack/error/getmetatable/setmetatable/newstate)
    /// and by renaming otherwise: lua_len -> pushlen (pushes the length),
    /// luaL_len -> len (returns it), luaL_tolstring -> tolstringL,
    /// luaL_typename -> typenameL.
    /// </summary>
    public static partial class LuaLib
    {
        /// <summary>
        /// lua_State* wrapper. Opaque, blittable, one internal IntPtr field;
        /// default(this State) is the NULL state. Obtained from newstate/newthread/
        /// tothread/resume-related calls and passed by value as the first
        /// parameter of every facade function.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct State
        {
            internal IntPtr _L;
        }

        /// <summary>State -> lua_State* (direct field access).</summary>
        private static IntPtr Ptr(this State L) => L._L;

        /// <summary>
        /// ref T -> raw address (lua_Debug*/luaL_Buffer* style params).
        /// The referent must be a stack local for the duration of the call.
        /// </summary>
        private static IntPtr Ptr<T>(ref T v) => RawRef.Of(ref v).Address;

        /// <summary>
        /// Delegate -> native thunk, with keep-alive. Lua holds the raw pointer
        /// beyond the call (pushcclosure/atpanic/sethook/setallocf/...), so the
        /// delegate instance must stay GC-rooted for the lifetime of the state:
        /// on CoreCLR/Mono the thunk is bound to the delegate instance and
        /// would dangle after collection. Under IL2CPP the ReversePInvokeWrapper
        /// is a compile-time static function (keep-alive is redundant but
        /// harmless); the delegate must still be bound to a static method
        /// annotated with [AOT.MonoPInvokeCallback] (see LuaDelegates.cs).
        /// </summary>
        private static readonly List<Delegate> s_kept = new List<Delegate>();
        private static IntPtr Keep(Delegate d)
        {
            if (d is null) return IntPtr.Zero;
            lock (s_kept) { s_kept.Add(d); }
            return Marshal.GetFunctionPointerForDelegate(d);
        }

        /// <summary>
        /// Native function pointer -> delegate (Marshal built-in). CoreCLR/Mono
        /// generate the marshaling thunk at runtime. Under IL2CPP
        /// GetDelegateForFunctionPointer has limited support (unverified; may
        /// throw NotSupportedException) - the fallback route there is wrapping
        /// a PointerFunc(fn, true) invoker instead.
        /// </summary>
        private static T ToDel<T>(IntPtr fn) where T : class
        {
            if (fn == IntPtr.Zero) return null;
            return (T)(object)Marshal.GetDelegateForFunctionPointer(fn, typeof(T));
        }

        #region constants (lua.h / lauxlib.h)

        public const int MULTRET = -1;

        // 5.5 pseudo-index formula: -(INT_MAX/2 + 1000).
        public const int REGISTRYINDEX = -(int.MaxValue / 2 + 1000);

        public const int OK = 0;
        public const int YIELD = 1;
        public const int ERRRUN = 2;
        public const int ERRSYNTAX = 3;
        public const int ERRMEM = 4;
        public const int ERRERR = 5;
        public const int ERRFILE = 6; // lauxlib.h: LUA_ERRERR + 1

        public const int TNONE = -1;
        public const int TNIL = 0;
        public const int TBOOLEAN = 1;
        public const int TLIGHTUSERDATA = 2;
        public const int TNUMBER = 3;
        public const int TSTRING = 4;
        public const int TTABLE = 5;
        public const int TFUNCTION = 6;
        public const int TUSERDATA = 7;
        public const int TTHREAD = 8;
        public const int NUMTYPES = 9;

        public const int MINSTACK = 20;

        public const int RIDX_GLOBALS = 2;
        public const int RIDX_MAINTHREAD = 3;
        public const int RIDX_LAST = 3;

        // Arithmetic ops (lua_arith).
        public const int OPADD = 0;
        public const int OPSUB = 1;
        public const int OPMUL = 2;
        public const int OPMOD = 3;
        public const int OPPOW = 4;
        public const int OPDIV = 5;
        public const int OPIDIV = 6;
        public const int OPBAND = 7;
        public const int OPBOR = 8;
        public const int OPBXOR = 9;
        public const int OPSHL = 10;
        public const int OPSHR = 11;
        public const int OPUNM = 12;
        public const int OPBNOT = 13;

        // Comparison ops (lua_compare).
        public const int OPEQ = 0;
        public const int OPLT = 1;
        public const int OPLE = 2;

        // gc() commands.
        public const int GCSTOP = 0;
        public const int GCRESTART = 1;
        public const int GCCOLLECT = 2;
        public const int GCCOUNT = 3;
        public const int GCCOUNTB = 4;
        public const int GCSTEP = 5;
        public const int GCISRUNNING = 6;
        public const int GCGEN = 7;
        public const int GCINC = 8;
        public const int GCPARAM = 9;

        // gc(L, GCPARAM, param, value) selectors.
        public const int GCPMINORMUL = 0;
        public const int GCPMAJORMINOR = 1;
        public const int GCPMINORMAJOR = 2;
        public const int GCPPAUSE = 3;
        public const int GCPSTEPMUL = 4;
        public const int GCPSTEPSIZE = 5;
        public const int GCPN = 6;

        // lua_numbertocstring buffer size.
        public const int N2SBUFFSZ = 64;

        // Hook events (Debug.event) and sethook masks.
        public const int HOOKCALL = 0;
        public const int HOOKRET = 1;
        public const int HOOKLINE = 2;
        public const int HOOKCOUNT = 3;
        public const int HOOKTAILCALL = 4;
        public const int MASKCALL = 1 << HOOKCALL;
        public const int MASKRET = 1 << HOOKRET;
        public const int MASKLINE = 1 << HOOKLINE;
        public const int MASKCOUNT = 1 << HOOKCOUNT;

        // luaL_ref / luaL_unref results (lauxlib.h).
        public const int NOREF = -2;
        public const int REFNIL = -1;

        // Numeric version (lua.h): 5*100+5, 5*10000+5*100+0.
        public const int VERSION_NUM = 505;
        public const int VERSION_RELEASE_NUM = 50500;

        // LUA_EXTRASPACE = sizeof(void*); runtime value (pointer size).
        public static readonly int EXTRASPACE = IntPtr.Size;

        // LUAL_BUFFERSIZE = 16 * sizeof(void*) * sizeof(lua_Number):
        // 1024 on 64-bit, 512 on 32-bit (lua_Number is double).
        public static readonly UIntPtr BUFFERSIZE = (UIntPtr)(16 * IntPtr.Size * sizeof(double));

        // openselectedlibs load/preload bit masks (lualib.h LUA_*LIBK).
        // luaL_openlibs(L) == openselectedlibs(L, ~0, 0).
        public const int GLIBK = 1;      // base
        public const int LOADLIBK = 2;   // package
        public const int COLIBK = 4;     // coroutine
        public const int DBLIBK = 8;     // debug
        public const int IOLIBK = 16;    // io
        public const int MATHLIBK = 32;  // math
        public const int OSLIBK = 64;    // os
        public const int STRLIBK = 128;  // string
        public const int TABLIBK = 256;  // table
        public const int UTF8LIBK = 512; // utf8

        // Version numbers (lua.h).
        public const int MAJOR_N = 5;
        public const int MINOR_N = 5;
        public const int RELEASE_N = 0;

        // LUAL_NUMSIZES = sizeof(lua_Integer)*16 + sizeof(lua_Number): packs
        // the two numeric-type sizes into one scalar (136 under the default
        // LONGLONG+DOUBLE config). checkversion_ compares it against the
        // core's own value - an ABI self-check that catches a native Lua
        // built with a non-default numeric configuration (e.g. LUA_32BITS).
        public static readonly int NUMSIZES = sizeof(long) * 16 + sizeof(double);

        // Signature of binary chunks (lua.h LUA_SIGNATURE): ESC "Lua". The
        // first 4 bytes of anything dump() produces / load() accepts.
        public const string SIGNATURE = "\x1bLua";

        // Well-known names (lua.h / lauxlib.h / lualib.h). Plain 'string' for
        // now; the marshaling mechanism for these is to be reworked later.
        public const string VERSION = "Lua 5.5";
        public const string RELEASE = "Lua 5.5.0";
        public const string COPYRIGHT = "Lua 5.5.0  Copyright (C) 1994-2025 Lua.org, PUC-Rio";
        public const string AUTHORS = "R. Ierusalimschy, L. H. de Figueiredo, W. Celes";
        public const string VERSUFFIX = "_5_5"; // global-name suffix: luaopen_xxx_5_5
        public const string GNAME = "_G";
        public const string LOADED_TABLE = "_LOADED";
        public const string PRELOAD_TABLE = "_PRELOAD";
        public const string FILEHANDLE = "FILE*";
        public const string LOADLIBNAME = "package";
        public const string COLIBNAME = "coroutine";
        public const string DBLIBNAME = "debug";
        public const string IOLIBNAME = "io";
        public const string MATHLIBNAME = "math";
        public const string OSLIBNAME = "os";
        public const string STRLIBNAME = "string";
        public const string TABLIBNAME = "table";
        public const string UTF8LIBNAME = "utf8";

        #endregion

        private const int _StackAllocMaxElements = 256;

        #region state manipulation

        public static State newstate(Alloc f, IntPtr ud, uint seed) => new State { _L = LuaImport.lua_newstate(Keep(f), ud, seed) };
        public static State newstate() => new State { _L = LuaImport.luaL_newstate() };
        public static void close(this State L) => LuaImport.lua_close(Ptr(L));
        public static State newthread(this State L) => new State { _L = LuaImport.lua_newthread(Ptr(L)) };
        public static int closethread(this State L, State from) => LuaImport.lua_closethread(Ptr(L), Ptr(from)); // from may be default (NULL)
        public static CFunction atpanic(this State L, CFunction panicf) => ToDel<CFunction>(LuaImport.lua_atpanic(Ptr(L), Keep(panicf))); // returns the previous panic fn
        public static double version(this State L) => LuaImport.lua_version(Ptr(L)); // L may be default (NULL)

        #endregion

        #region basic stack manipulation

        public static int absindex(this State L, int idx) => LuaImport.lua_absindex(Ptr(L), idx);
        public static int gettop(this State L) => LuaImport.lua_gettop(Ptr(L));
        public static void settop(this State L, int idx) => LuaImport.lua_settop(Ptr(L), idx);
        public static void pushvalue(this State L, int idx) => LuaImport.lua_pushvalue(Ptr(L), idx);
        public static void rotate(this State L, int idx, int n) => LuaImport.lua_rotate(Ptr(L), idx, n);
        public static void copy(this State L, int fromidx, int toidx) => LuaImport.lua_copy(Ptr(L), fromidx, toidx);
        public static int checkstack(this State L, int n) => LuaImport.lua_checkstack(Ptr(L), n);
        public static void checkstack(this State L, int sz, CStringSpan msg)
        {
            using (NativeBuffer bmsg = default)
            {
                IntPtr pmsg;
                if (!msg.IsNull() && (!RuntimeInfo.IsUnity || !msg.IsNullTerminated()))
                {
                    Span<byte> newspan = msg.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmsg) = new NativeBuffer(msg.Length + 1)) : stackalloc byte[msg.Length + 1];
                    msg.Slice().CopyTo(newspan);
                    newspan[msg.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmsg = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmsg = RawRef.Of(ref msg.ToSpan().GetRef()).Address;
                }
                LuaImport.luaL_checkstack(Ptr(L), sz, pmsg);
            }
        } // luaL_checkstack; WARNING: longjmps on failure
        public static void xmove(this State from, State to, int n) => LuaImport.lua_xmove(Ptr(from), Ptr(to), n);

        #endregion

        #region access functions (stack -> C)

        public static int isnumber(this State L, int idx) => LuaImport.lua_isnumber(Ptr(L), idx);
        public static int isstring(this State L, int idx) => LuaImport.lua_isstring(Ptr(L), idx);
        public static int iscfunction(this State L, int idx) => LuaImport.lua_iscfunction(Ptr(L), idx);
        public static int isinteger(this State L, int idx) => LuaImport.lua_isinteger(Ptr(L), idx);
        public static int isuserdata(this State L, int idx) => LuaImport.lua_isuserdata(Ptr(L), idx);
        public static int type(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx);
        public static CStringRef typename(this State L, int tp) => new CStringRef(LuaImport.lua_typename(Ptr(L), tp));
        public static double tonumberx(this State L, int idx, out bool isnum)
        {
            int bIsNum = 0;
            var result = LuaImport.lua_tonumberx(Ptr(L), idx, RawRef.Of(ref bIsNum));
            isnum = bIsNum != 0;
            return result;
        }
        public static long tointegerx(this State L, int idx, out bool isnum)
        {
            int bIsNum = 0;
            var result = LuaImport.lua_tointegerx(Ptr(L), idx, RawRef.Of(ref bIsNum));
            isnum = bIsNum != 0;
            return result;
        }
        public static int toboolean(this State L, int idx) => LuaImport.lua_toboolean(Ptr(L), idx);
        // WARNING (manual): if the value is a number, lua_tolstring CHANGES the
        // actual stack slot to a string (in-place conversion) - this also
        // confuses lua_next when applied to keys during a table traversal.
        public static CStringRef tolstring(this State L, int idx)
        {
            IntPtr len = default;
            var pstr = LuaImport.lua_tolstring(Ptr(L), idx, RawRef.Of(ref len));
            return new CStringRef(pstr, (int)len + 1, (int)len);
        }
        public static CStringRef tolstringL(this State L, int idx)
        {
            IntPtr len = default;
            var pstr = LuaImport.luaL_tolstring(Ptr(L), idx, RawRef.Of(ref len));
            return new CStringRef(pstr, (int)len + 1, (int)len);

        } // luaL_tolstring (applies __tostring);
        public static ulong rawlen(this State L, int idx) => LuaImport.lua_rawlen(Ptr(L), idx);
        public static CFunction tocfunction(this State L, int idx) => ToDel<CFunction>(LuaImport.lua_tocfunction(Ptr(L), idx));
        public static IntPtr touserdata(this State L, int idx) => LuaImport.lua_touserdata(Ptr(L), idx);
        public static State tothread(this State L, int idx) => new State { _L = LuaImport.lua_tothread(Ptr(L), idx) };
        public static IntPtr topointer(this State L, int idx) => LuaImport.lua_topointer(Ptr(L), idx);

        #endregion

        #region comparison and arithmetic functions

        public static void arith(this State L, int op) => LuaImport.lua_arith(Ptr(L), op);
        public static int rawequal(this State L, int idx1, int idx2) => LuaImport.lua_rawequal(Ptr(L), idx1, idx2);
        public static int compare(this State L, int idx1, int idx2, int op) => LuaImport.lua_compare(Ptr(L), idx1, idx2, op);

        #endregion

        #region push functions (C -> stack)

        public static void pushnil(this State L) => LuaImport.lua_pushnil(Ptr(L));
        public static void pushnumber(this State L, double n) => LuaImport.lua_pushnumber(Ptr(L), n);
        public static void pushinteger(this State L, long n) => LuaImport.lua_pushinteger(Ptr(L), n);
        public static CStringRef pushlstring(this State L, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if (!s.IsNull() && !RuntimeInfo.IsUnity)
                {
                    Span<byte> newspan = s.Length > _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length)) : stackalloc byte[s.Length];
                    s.Slice().CopyTo(newspan);
                    ps = RawRef.Of(ref newspan.GetRef()).Address;   // empty span -> NULL (lua_pushlstring(L, NULL, 0) is legal)
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address; // empty span -> NULL
                }
                var cloned = LuaImport.lua_pushlstring(Ptr(L), ps, (UIntPtr)s.Length);
                return new CStringRef(cloned, s.Length + 1, s.Length);
            }
        }
        public static CStringRef pushexternalstring(this State L, CStringRef s, Alloc falloc, IntPtr ud)
        {
            // Empty span -> NULL, but an external string REQUIRES s[len] == '\0'
            // (native api_check dereferences it) - an empty CStringRef is an
            // invalid argument either way, same as in C.
            IntPtr ps = RawRef.Of(ref s.Slice().GetRef()).Address;
            LuaImport.lua_pushexternalstring(Ptr(L), ps, (UIntPtr)s.Length, Keep(falloc), ud);
            return s;
        }
        public static CStringRef pushstring(this State L, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                var cloned = LuaImport.lua_pushstring(Ptr(L), ps);
                return new CStringRef(cloned);
            }
        }
        public static CStringRef pushvfstring(this State L, CStringSpan fmt, IntPtr argp)
        {
            using (NativeBuffer bfmt = default)
            {
                IntPtr pfmt;
                if (!fmt.IsNull() && (!RuntimeInfo.IsUnity || !fmt.IsNullTerminated()))
                {
                    Span<byte> newspan = fmt.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfmt) = new NativeBuffer(fmt.Length + 1)) : stackalloc byte[fmt.Length + 1];
                    fmt.Slice().CopyTo(newspan);
                    newspan[fmt.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfmt = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfmt = RawRef.Of(ref fmt.ToSpan().GetRef()).Address;
                }
    
                var cloned = LuaImport.lua_pushvfstring(Ptr(L), pfmt, argp);
                return new CStringRef(cloned);
            }
        }// TODO: argp: va_list (platform-specific)
        public static CStringRef pushfstring(this State L, CStringSpan fmt)
        {
            using (NativeBuffer bfmt = default)
            {
                IntPtr pfmt;
                if (!fmt.IsNull() && (!RuntimeInfo.IsUnity || !fmt.IsNullTerminated()))
                {
                    Span<byte> newspan = fmt.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfmt) = new NativeBuffer(fmt.Length + 1)) : stackalloc byte[fmt.Length + 1];
                    fmt.Slice().CopyTo(newspan);
                    newspan[fmt.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfmt = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfmt = RawRef.Of(ref fmt.ToSpan().GetRef()).Address;
                }
                var cloned = LuaImport.lua_pushfstring(Ptr(L), pfmt);
                return new CStringRef(cloned);
            }
        } // fmt must contain no conversion specifiers
        public static CStringRef pushfstring(this State L, CStringSpan fmt, IntPtr arg1)
        {
            using (NativeBuffer bfmt = default)
            {
                IntPtr pfmt;
                if (!fmt.IsNull() && (!RuntimeInfo.IsUnity || !fmt.IsNullTerminated()))
                {
                    Span<byte> newspan = fmt.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfmt) = new NativeBuffer(fmt.Length + 1)) : stackalloc byte[fmt.Length + 1];
                    fmt.Slice().CopyTo(newspan);
                    newspan[fmt.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfmt = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfmt = RawRef.Of(ref fmt.ToSpan().GetRef()).Address;
                }
                var cloned = LuaImport.lua_pushfstring(Ptr(L), pfmt, arg1);
                return new CStringRef(cloned);
            }
        } // single-conversion form
        public static void pushcclosure(this State L, CFunction fn, int n) => LuaImport.lua_pushcclosure(Ptr(L), Keep(fn), n);
        public static void pushboolean(this State L, int b) => LuaImport.lua_pushboolean(Ptr(L), b);
        public static void pushlightuserdata(this State L, IntPtr p) => LuaImport.lua_pushlightuserdata(Ptr(L), p);
        public static int pushthread(this State L) => LuaImport.lua_pushthread(Ptr(L));

        #endregion

        #region get functions (Lua -> stack)

        public static int getglobal(this State L, CStringSpan name)
        {
            using (NativeBuffer bname = default)
            {
                IntPtr pname;
                if (!name.IsNull() && (!RuntimeInfo.IsUnity || !name.IsNullTerminated()))
                {
                    Span<byte> newspan = name.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bname) = new NativeBuffer(name.Length + 1)) : stackalloc byte[name.Length + 1];
                    name.Slice().CopyTo(newspan);
                    newspan[name.Length] = 0;   // terminator: stackalloc is not zeroed
                    pname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pname = RawRef.Of(ref name.ToSpan().GetRef()).Address;
                }
                return LuaImport.lua_getglobal(Ptr(L), pname);
            }
        }
        public static int gettable(this State L, int idx) => LuaImport.lua_gettable(Ptr(L), idx);
        public static int getfield(this State L, int idx, CStringSpan k)
        {
            using (NativeBuffer bk = default)
            {
                IntPtr pk;
                if (!k.IsNull() && (!RuntimeInfo.IsUnity || !k.IsNullTerminated()))
                {
                    Span<byte> newspan = k.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bk) = new NativeBuffer(k.Length + 1)) : stackalloc byte[k.Length + 1];
                    k.Slice().CopyTo(newspan);
                    newspan[k.Length] = 0;   // terminator: stackalloc is not zeroed
                    pk = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pk = RawRef.Of(ref k.ToSpan().GetRef()).Address;
                }
                return LuaImport.lua_getfield(Ptr(L), idx, pk);
            }
        }
        public static int geti(this State L, int idx, long n) => LuaImport.lua_geti(Ptr(L), idx, n);
        public static int rawget(this State L, int idx) => LuaImport.lua_rawget(Ptr(L), idx);
        public static int rawgeti(this State L, int idx, long n) => LuaImport.lua_rawgeti(Ptr(L), idx, n);
        public static int rawgetp(this State L, int idx, IntPtr p) => LuaImport.lua_rawgetp(Ptr(L), idx, p);
        public static void createtable(this State L, int narr, int nrec) => LuaImport.lua_createtable(Ptr(L), narr, nrec);
        public static IntPtr newuserdatauv(this State L, UIntPtr sz, int nuvalue) => LuaImport.lua_newuserdatauv(Ptr(L), sz, nuvalue);
        public static int getmetatable(this State L, int objindex) => LuaImport.lua_getmetatable(Ptr(L), objindex);
        public static int getiuservalue(this State L, int idx, int n) => LuaImport.lua_getiuservalue(Ptr(L), idx, n);

        #endregion

        #region set functions (stack -> Lua)

        public static void setglobal(this State L, CStringSpan name)
        {
            using (NativeBuffer bname = default)
            {
                IntPtr pname;
                if (!name.IsNull() && (!RuntimeInfo.IsUnity || !name.IsNullTerminated()))
                {
                    Span<byte> newspan = name.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bname) = new NativeBuffer(name.Length + 1)) : stackalloc byte[name.Length + 1];
                    name.Slice().CopyTo(newspan);
                    newspan[name.Length] = 0;   // terminator: stackalloc is not zeroed
                    pname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pname = RawRef.Of(ref name.ToSpan().GetRef()).Address;
                }
                LuaImport.lua_setglobal(Ptr(L), pname);
            }
        }
        public static void settable(this State L, int idx) => LuaImport.lua_settable(Ptr(L), idx);
        public static void setfield(this State L, int idx, CStringSpan k)
        {
            using (NativeBuffer bk = default)
            {
                IntPtr pk;
                if (!k.IsNull() && (!RuntimeInfo.IsUnity || !k.IsNullTerminated()))
                {
                    Span<byte> newspan = k.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bk) = new NativeBuffer(k.Length + 1)) : stackalloc byte[k.Length + 1];
                    k.Slice().CopyTo(newspan);
                    newspan[k.Length] = 0;   // terminator: stackalloc is not zeroed
                    pk = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pk = RawRef.Of(ref k.ToSpan().GetRef()).Address;
                }
                LuaImport.lua_setfield(Ptr(L), idx, pk);
            }
        }
        public static void seti(this State L, int idx, long n) => LuaImport.lua_seti(Ptr(L), idx, n);
        public static void rawset(this State L, int idx) => LuaImport.lua_rawset(Ptr(L), idx);
        public static void rawseti(this State L, int idx, long n) => LuaImport.lua_rawseti(Ptr(L), idx, n);
        public static void rawsetp(this State L, int idx, IntPtr p) => LuaImport.lua_rawsetp(Ptr(L), idx, p);
        public static int setmetatable(this State L, int objindex) => LuaImport.lua_setmetatable(Ptr(L), objindex);
        public static void setmetatable(this State L, CStringSpan tname)
        {
            using (NativeBuffer bname = default)
            {
                IntPtr pname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    pname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                LuaImport.luaL_setmetatable(Ptr(L), pname);
            }
        } // luaL_setmetatable;
        public static int setiuservalue(this State L, int idx, int n) => LuaImport.lua_setiuservalue(Ptr(L), idx, n);

        #endregion

        #region 'load' and 'call' functions

        // WARNING: callk longjmps out on error (never returns normally);
        // do not rely on finally/using semantics on this call path.
        public static void callk(this State L, int nargs, int nresults, IntPtr ctx, KFunction k) => LuaImport.lua_callk(Ptr(L), nargs, nresults, ctx, Keep(k));
        public static int pcallk(this State L, int nargs, int nresults, int errfunc, IntPtr ctx, KFunction k) => LuaImport.lua_pcallk(Ptr(L), nargs, nresults, errfunc, ctx, Keep(k)); // catches errors as a return code
        public static int load(this State L, Reader reader, IntPtr dt, CStringSpan chunkname, CStringSpan mode)
        {
            using (NativeBuffer bchunkname = default, bmode = default)
            {
                IntPtr pchunkname;
                if (!chunkname.IsNull() && (!RuntimeInfo.IsUnity || !chunkname.IsNullTerminated()))
                {
                    Span<byte> newspan = chunkname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bchunkname) = new NativeBuffer(chunkname.Length + 1)) : stackalloc byte[chunkname.Length + 1];
                    chunkname.Slice().CopyTo(newspan);
                    newspan[chunkname.Length] = 0;   // terminator: stackalloc is not zeroed
                    pchunkname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pchunkname = RawRef.Of(ref chunkname.ToSpan().GetRef()).Address;
                }
                IntPtr pmode;
                if (!mode.IsNull() && (!RuntimeInfo.IsUnity || !mode.IsNullTerminated()))
                {
                    Span<byte> newspan = mode.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmode) = new NativeBuffer(mode.Length + 1)) : stackalloc byte[mode.Length + 1];
                    mode.Slice().CopyTo(newspan);
                    newspan[mode.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmode = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmode = RawRef.Of(ref mode.ToSpan().GetRef()).Address;
                }
                return LuaImport.lua_load(Ptr(L), Keep(reader), dt, pchunkname, pmode);
            }
        }
        public static int dump(this State L, Writer writer, IntPtr data, int strip) => LuaImport.lua_dump(Ptr(L), Keep(writer), data, strip);

        #endregion

        #region coroutine functions

        public static int yieldk(this State L, int nresults, IntPtr ctx, KFunction k) => LuaImport.lua_yieldk(Ptr(L), nresults, ctx, Keep(k)); // WARNING: longjmps if not yieldable
        public static int resume(this State L, State from, int narg, out int nres)
        {
            int lnres = 0;
            var result = LuaImport.lua_resume(Ptr(L), Ptr(from), narg, RawRef.Of(ref lnres));
            nres = lnres;
            return result;
        }
        public static int status(this State L) => LuaImport.lua_status(Ptr(L));
        public static int isyieldable(this State L) => LuaImport.lua_isyieldable(Ptr(L));

        #endregion

        #region warning-related functions

        public static void setwarnf(this State L, WarnFunction f, IntPtr ud) => LuaImport.lua_setwarnf(Ptr(L), Keep(f), ud);
        public static void warning(this State L, CStringSpan msg, int tocont)
        {
            using (NativeBuffer bmsg = default)
            {
                IntPtr pmsg;
                if (!msg.IsNull() && (!RuntimeInfo.IsUnity || !msg.IsNullTerminated()))
                {
                    Span<byte> newspan = msg.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmsg) = new NativeBuffer(msg.Length + 1)) : stackalloc byte[msg.Length + 1];
                    msg.Slice().CopyTo(newspan);
                    newspan[msg.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmsg = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmsg = RawRef.Of(ref msg.ToSpan().GetRef()).Address;
                }
                LuaImport.lua_warning(Ptr(L), pmsg, tocont);
            }
        }

        #endregion

        #region garbage-collection function

        // Commands that read no extra arguments: GCSTOP/GCRESTART/GCCOLLECT/
        // GCCOUNT/GCCOUNTB/GCISRUNNING/GCGEN/GCINC.
        public static int gc(this State L, int what) => LuaImport.lua_gc(Ptr(L), what, UIntPtr.Zero, 0);

        // GCSTEP: data = step size in bytes (size_t); n <= 0 forces one basic
        // step. Returns 1 when the cycle finished (back to pause state).
        public static int gc(this State L, int what, UIntPtr data) => LuaImport.lua_gc(Ptr(L), what, data, 0);

        // GCPARAM: param 0..6 (GCPMINORMUL/GCPMAJORMINOR/GCPMINORMAJOR/
        // GCPPAUSE/GCPSTEPMUL/GCPSTEPSIZE/GCPN); value >= 0 sets, value < 0
        // (convention: -1) queries. Returns the previous value
        // (percent-scaled; log-encoded internally, so round-trips are approximate).
        public static int gc(this State L, int what, int param, int value) => LuaImport.lua_gc(Ptr(L), what, (UIntPtr)(uint)param, value);

        #endregion

        #region miscellaneous functions

        // WARNING: lua_error longjmps out (never returns normally); do not rely
        // on finally/using semantics on this call path.
        public static int error(this State L) => LuaImport.lua_error(Ptr(L));
        // luaL_error; WARNING: longjmps out (never returns normally).
        public static int error(this State L, CStringSpan fmt)
        {
            using (NativeBuffer bfmt = default)
            {
                IntPtr pfmt;
                if (!fmt.IsNull() && (!RuntimeInfo.IsUnity || !fmt.IsNullTerminated()))
                {
                    Span<byte> newspan = fmt.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfmt) = new NativeBuffer(fmt.Length + 1)) : stackalloc byte[fmt.Length + 1];
                    fmt.Slice().CopyTo(newspan);
                    newspan[fmt.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfmt = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfmt = RawRef.Of(ref fmt.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_error(Ptr(L), pfmt);
            }
        } // fmt must contain no conversion specifiers
        // luaL_error; WARNING: longjmps out (never returns normally).
        public static int error(this State L, CStringSpan fmt, IntPtr arg1)
        {
            using (NativeBuffer bfmt = default)
            {
                IntPtr pfmt;
                if (!fmt.IsNull() && (!RuntimeInfo.IsUnity || !fmt.IsNullTerminated()))
                {
                    Span<byte> newspan = fmt.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfmt) = new NativeBuffer(fmt.Length + 1)) : stackalloc byte[fmt.Length + 1];
                    fmt.Slice().CopyTo(newspan);
                    newspan[fmt.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfmt = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfmt = RawRef.Of(ref fmt.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_error(Ptr(L), pfmt, arg1);
            }
        } // single-conversion form
        public static int next(this State L, int idx) => LuaImport.lua_next(Ptr(L), idx);
        public static void concat(this State L, int n) => LuaImport.lua_concat(Ptr(L), n);
        public static void pushlen(this State L, int idx) => LuaImport.lua_len(Ptr(L), idx); // lua_len: pushes the object's length onto the stack
        public static long len(this State L, int idx) => LuaImport.luaL_len(Ptr(L), idx); // luaL_len: returns the length; WARNING: longjmps on error
        // buff must hold N2SBUFFSZ (64) bytes - Buffer64 matches exactly.
        // NOTE: same stack-local bounce as getstack on compacting-GC hosts
        // (single-shot call, no self-referencing fields - the bounce is safe).
        public static uint numbertocstring(this State L, int idx, ref Buffer64 buff)
        {
            if (RuntimeInfo.IsUnity)
                return LuaImport.lua_numbertocstring(Ptr(L), idx, Ptr(ref buff));
            Buffer64 local = buff;
            uint r = LuaImport.lua_numbertocstring(Ptr(L), idx, Ptr(ref local));
            buff = local;
            return r;
        }
        public static UIntPtr stringtonumber(this State L, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                return LuaImport.lua_stringtonumber(Ptr(L), ps);
            }
        }
        public static Alloc getallocf(this State L, out IntPtr ud)
        {
            IntPtr lud = default;
            Alloc result = ToDel<Alloc>(LuaImport.lua_getallocf(Ptr(L), RawRef.Of(ref lud))); // ud: void** out
            ud = lud;
            return result;
        }
        public static void setallocf(this State L, Alloc f, IntPtr ud) => LuaImport.lua_setallocf(Ptr(L), Keep(f), ud);
        public static void toclose(this State L, int idx) => LuaImport.lua_toclose(Ptr(L), idx);
        public static void closeslot(this State L, int idx) => LuaImport.lua_closeslot(Ptr(L), idx);

        #endregion

        #region debug API

        // NOTE: on Unity the referent's address never moves (Boehm GC does not
        // compact), so the ref address is passed directly. On compacting-GC
        // hosts (CoreCLR / standalone Mono) a heap-field referent could move
        // during the call, so the struct is bounced through a stack local and
        // copied back afterwards (the managed byref is GC-tracked and already
        // points at the new location when the copy-back runs).
        public static int getstack(this State L, int level, ref Debug ar)
        {
            if (RuntimeInfo.IsUnity)
                return LuaImport.lua_getstack(Ptr(L), level, Ptr(ref ar));
            Debug local = ar;
            int r = LuaImport.lua_getstack(Ptr(L), level, Ptr(ref local));
            ar = local;
            return r;
        }
        // NOTE: same stack-local bounce as getstack on compacting-GC hosts.
        public static int getinfo(this State L, CStringSpan what, ref Debug ar)
        {
            using (NativeBuffer bwhat = default)
            {
                IntPtr pwhat;
                if (!what.IsNull() && (!RuntimeInfo.IsUnity || !what.IsNullTerminated()))
                {
                    Span<byte> newspan = what.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bwhat) = new NativeBuffer(what.Length + 1)) : stackalloc byte[what.Length + 1];
                    what.Slice().CopyTo(newspan);
                    newspan[what.Length] = 0;   // terminator: stackalloc is not zeroed
                    pwhat = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pwhat = RawRef.Of(ref what.ToSpan().GetRef()).Address;
                }
                if (RuntimeInfo.IsUnity)
                    return LuaImport.lua_getinfo(Ptr(L), pwhat, Ptr(ref ar));
                Debug local = ar;
                int r = LuaImport.lua_getinfo(Ptr(L), pwhat, Ptr(ref local));
                ar = local;
                return r;
            }
        }
        // NOTE: same stack-local bounce as getstack on compacting-GC hosts.
        public static CStringRef getlocal(this State L, in Debug ar, int n)
        {
            if (RuntimeInfo.IsUnity)
                return new CStringRef(LuaImport.lua_getlocal(Ptr(L), Ptr(ref Ref.Unprotect(in ar)), n));
            Debug local = ar;
            IntPtr r = LuaImport.lua_getlocal(Ptr(L), Ptr(ref local), n);
            return new CStringRef(r);
        } // (returns the variable name)
        // NOTE: same stack-local bounce as getstack on compacting-GC hosts.
        public static CStringRef setlocal(this State L, in Debug ar, int n)
        {
            if (RuntimeInfo.IsUnity)
                return new CStringRef(LuaImport.lua_setlocal(Ptr(L), Ptr(ref Ref.Unprotect(in ar)), n));
            Debug local = ar;
            IntPtr r = LuaImport.lua_setlocal(Ptr(L), Ptr(ref local), n);
            return new CStringRef(r);
        } // (returns the variable name)
        public static CStringRef getupvalue(this State L, int funcindex, int n) => new CStringRef(LuaImport.lua_getupvalue(Ptr(L), funcindex, n)); // (returns the upvalue name)
        public static CStringRef setupvalue(this State L, int funcindex, int n) => new CStringRef(LuaImport.lua_setupvalue(Ptr(L), funcindex, n)); // (returns the upvalue name)
        public static IntPtr upvalueid(this State L, int fidx, int n) => LuaImport.lua_upvalueid(Ptr(L), fidx, n);
        public static void upvaluejoin(this State L, int fidx1, int n1, int fidx2, int n2) => LuaImport.lua_upvaluejoin(Ptr(L), fidx1, n1, fidx2, n2);
        public static void sethook(this State L, Hook func, int mask, int count) => LuaImport.lua_sethook(Ptr(L), Keep(func), mask, count);
        public static Hook gethook(this State L) => ToDel<Hook>(LuaImport.lua_gethook(Ptr(L)));
        public static int gethookmask(this State L) => LuaImport.lua_gethookmask(Ptr(L));
        public static int gethookcount(this State L) => LuaImport.lua_gethookcount(Ptr(L));

        #endregion

        #region auxiliary functions

        public static void checkversion_(this State L, double ver, UIntPtr sz) => LuaImport.luaL_checkversion_(Ptr(L), ver, sz); // WARNING: longjmps on version mismatch
        public static int getmetafield(this State L, int obj, CStringSpan e)
        {
            using (NativeBuffer be = default)
            {
                IntPtr pe;
                if (!e.IsNull() && (!RuntimeInfo.IsUnity || !e.IsNullTerminated()))
                {
                    Span<byte> newspan = e.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in be) = new NativeBuffer(e.Length + 1)) : stackalloc byte[e.Length + 1];
                    e.Slice().CopyTo(newspan);
                    newspan[e.Length] = 0;   // terminator: stackalloc is not zeroed
                    pe = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pe = RawRef.Of(ref e.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_getmetafield(Ptr(L), obj, pe);
            }
        }
        public static int callmeta(this State L, int obj, CStringSpan e)
        {
            using (NativeBuffer be = default)
            {
                IntPtr pe;
                if (!e.IsNull() && (!RuntimeInfo.IsUnity || !e.IsNullTerminated()))
                {
                    Span<byte> newspan = e.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in be) = new NativeBuffer(e.Length + 1)) : stackalloc byte[e.Length + 1];
                    e.Slice().CopyTo(newspan);
                    newspan[e.Length] = 0;   // terminator: stackalloc is not zeroed
                    pe = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pe = RawRef.Of(ref e.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_callmeta(Ptr(L), obj, pe);
            }
        }
        public static int argerror(this State L, int arg, CStringSpan extramsg)
        {
            using (NativeBuffer bextramsg = default)
            {
                IntPtr pextramsg;
                if (!extramsg.IsNull() && (!RuntimeInfo.IsUnity || !extramsg.IsNullTerminated()))
                {
                    Span<byte> newspan = extramsg.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bextramsg) = new NativeBuffer(extramsg.Length + 1)) : stackalloc byte[extramsg.Length + 1];
                    extramsg.Slice().CopyTo(newspan);
                    newspan[extramsg.Length] = 0;   // terminator: stackalloc is not zeroed
                    pextramsg = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pextramsg = RawRef.Of(ref extramsg.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_argerror(Ptr(L), arg, pextramsg);
            }
        } // WARNING: longjmps (never returns normally)
        public static int typeerror(this State L, int arg, CStringSpan tname)
        {
            using (NativeBuffer btname = default)
            {
                IntPtr ptname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in btname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    ptname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ptname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_typeerror(Ptr(L), arg, ptname);
            }
        } // WARNING: longjmps (never returns normally)
        public static CStringRef checklstring(this State L, int arg)
        {
            IntPtr l = default;
            var result = LuaImport.luaL_checklstring(Ptr(L), arg, RawRef.Of(ref l));
            return new CStringRef(result, (int)l + 1, (int)l);
        } // WARNING: longjmps on bad argument
        public static CStringSpan optlstring(this State L, int arg, CStringSpan def)
        {
            if (isnoneornil(L, arg))
            {
                return def;
            }
            else
            {
                return checklstring(L, arg);
            }
        }
        public static double checknumber(this State L, int arg) => LuaImport.luaL_checknumber(Ptr(L), arg); // WARNING: longjmps on bad argument
        public static double optnumber(this State L, int arg, double def) => LuaImport.luaL_optnumber(Ptr(L), arg, def);
        public static long checkinteger(this State L, int arg) => LuaImport.luaL_checkinteger(Ptr(L), arg); // WARNING: longjmps on bad argument
        public static long optinteger(this State L, int arg, long def) => LuaImport.luaL_optinteger(Ptr(L), arg, def);
        public static void checktype(this State L, int arg, int t) => LuaImport.luaL_checktype(Ptr(L), arg, t); // WARNING: longjmps on bad argument
        public static void checkany(this State L, int arg) => LuaImport.luaL_checkany(Ptr(L), arg); // WARNING: longjmps on bad argument
        public static bool newmetatable(this State L, CStringSpan tname)
        {
            using (NativeBuffer btname = default)
            {
                IntPtr ptname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in btname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    ptname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ptname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                var result = LuaImport.luaL_newmetatable(Ptr(L), ptname);
                return result != 0;   // true: created and registered; false: already exists (previous value left on top)
            }
        }
        public static IntPtr testudata(this State L, int ud, CStringSpan tname)
        {
            using (NativeBuffer btname = default)
            {
                IntPtr ptname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in btname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    ptname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ptname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_testudata(Ptr(L), ud, ptname);
            }
        }
        public static IntPtr checkudata(this State L, int ud, CStringSpan tname)
        {
            using (NativeBuffer btname = default)
            {
                IntPtr ptname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in btname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    ptname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ptname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_checkudata(Ptr(L), ud, ptname);
            }
        } // WARNING: longjmps on bad argument
        public static void where(this State L, int lvl) => LuaImport.luaL_where(Ptr(L), lvl);
        public static int checkoption(this State L, int arg, CStringSpan def, Span<CStringRef> lst)
        {
            using (NativeBuffer bdef = default, blst = default, bitemlist = default)
            {
                IntPtr pdef;
                if (!def.IsNull() && (!RuntimeInfo.IsUnity || !def.IsNullTerminated()))
                {
                    Span<byte> newspan = def.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bdef) = new NativeBuffer(def.Length + 1)) : stackalloc byte[def.Length + 1];
                    def.Slice().CopyTo(newspan);
                    newspan[def.Length] = 0;   // terminator: stackalloc is not zeroed
                    pdef = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pdef = RawRef.Of(ref def.ToSpan().GetRef()).Address;
                }
                var len = lst.Length;
                if (lst.Length > 0 && lst[lst.Length - 1].IsEmpty())
                {
                    --len;
                }
                IntPtr plst;
                Span<NativeBuffer> itemdisposerlist = len > _StackAllocMaxElements ? (Ref.Unprotect(in bitemlist) = new NativeBuffer(len * Marshal.SizeOf<NativeBuffer>())).ToSpan<NativeBuffer>() : stackalloc NativeBuffer[len];
                try
                {
                    if (lst.IsNull())
                    {
                        plst = IntPtr.Zero;
                    }
                    else
                    {
                        Span<IntPtr> llst = len >= _StackAllocMaxElements ? (Ref.Unprotect(in blst) = new NativeBuffer((len + 1) * IntPtr.Size)).ToSpan<IntPtr>() : stackalloc IntPtr[len + 1];
                        llst[len] = default;   // terminator: stackalloc is not zeroed
                        int totalstrbytes = 0;
                        for (int i = 0; i < len; ++i)
                        {
                            itemdisposerlist[i] = default;
                            var item = lst[i];
                            IntPtr pitem;
                            if (!item.IsNull() && (!RuntimeInfo.IsUnity || !item.IsNullTerminated()))
                            {
                                totalstrbytes += item.Length + 1;
                                Span<byte> newspan = totalstrbytes > _StackAllocMaxElements ? (itemdisposerlist[i] = new NativeBuffer(item.Length + 1)) : stackalloc byte[item.Length + 1];
                                item.Slice().CopyTo(newspan);
                                newspan[item.Length] = 0;   // terminator: stackalloc is not zeroed
                                pitem = RawRef.Of(ref newspan[0]).Address;
                            }
                            else
                            {
                                pitem = RawRef.Of(ref item.ToSpan().GetRef()).Address;
                            }
                            llst[i] = pitem;
                        }
                        plst = RawRef.Of(ref llst[0]).Address;
                    }
                    return LuaImport.luaL_checkoption(Ptr(L), arg, pdef, plst);
                }
                finally
                {
                    for (int i = 0; i < itemdisposerlist.Length; ++i)
                    {
                        itemdisposerlist[i].Dispose();
                    }
                }
            }
        } // WARNING: longjmps on bad argument
        public static int fileresult(this State L, int stat, CStringSpan fname)
        {
            using (NativeBuffer bfname = default)
            {
                IntPtr pfname;
                if (!fname.IsNull() && (!RuntimeInfo.IsUnity || !fname.IsNullTerminated()))
                {
                    Span<byte> newspan = fname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfname) = new NativeBuffer(fname.Length + 1)) : stackalloc byte[fname.Length + 1];
                    fname.Slice().CopyTo(newspan);
                    newspan[fname.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfname = RawRef.Of(ref fname.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_fileresult(Ptr(L), stat, pfname);
            }
        }
        public static int execresult(this State L, int stat) => LuaImport.luaL_execresult(Ptr(L), stat);
        public static IntPtr alloc(IntPtr ud, IntPtr ptr, UIntPtr osize, UIntPtr nsize) => LuaImport.luaL_alloc(ud, ptr, osize, nsize); // Lua 5.5: the default allocator export (no state involved)
        public static int refer(this State L, int t) => LuaImport.luaL_ref(Ptr(L), t); // 'ref' is a C# keyword
        public static void unref(this State L, int t, int reference) => LuaImport.luaL_unref(Ptr(L), t, reference);
        public static int loadfilex(this State L, CStringSpan filename, CStringSpan mode)
        {
            using (NativeBuffer bfilename = default, bmode = default)
            {
                IntPtr pfilename;
                if (!filename.IsNull() && (!RuntimeInfo.IsUnity || !filename.IsNullTerminated()))
                {
                    Span<byte> newspan = filename.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfilename) = new NativeBuffer(filename.Length + 1)) : stackalloc byte[filename.Length + 1];
                    filename.Slice().CopyTo(newspan);
                    newspan[filename.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfilename = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfilename = RawRef.Of(ref filename.ToSpan().GetRef()).Address;
                }
                IntPtr pmode;
                if (!mode.IsNull() && (!RuntimeInfo.IsUnity || !mode.IsNullTerminated()))
                {
                    Span<byte> newspan = mode.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmode) = new NativeBuffer(mode.Length + 1)) : stackalloc byte[mode.Length + 1];
                    mode.Slice().CopyTo(newspan);
                    newspan[mode.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmode = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmode = RawRef.Of(ref mode.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_loadfilex(Ptr(L), pfilename, pmode);
            }
        }
        public static int loadbufferx(this State L, CStringSpan buff, CStringSpan name, CStringSpan mode)
        {
            using (NativeBuffer bbuff = default, bname = default(NativeBuffer), bmode = default)
            {
                IntPtr pbuff;
                if (!buff.IsNull() && (!RuntimeInfo.IsUnity))
                {
                    Span<byte> newspan = buff.Length > _StackAllocMaxElements ? (Ref.Unprotect(in bbuff) = new NativeBuffer(buff.Length)) : stackalloc byte[buff.Length];
                    buff.Slice().CopyTo(newspan);
                    pbuff = RawRef.Of(ref newspan.GetRef()).Address;   // empty span -> NULL (load returns ERRSYNTAX, no crash)
                }
                else
                {
                    pbuff = RawRef.Of(ref buff.Slice().GetRef()).Address; // empty span -> NULL
                }
                IntPtr pname;
                if (!name.IsNull() && (!RuntimeInfo.IsUnity || !name.IsNullTerminated()))
                {
                    Span<byte> newspan = name.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bname) = new NativeBuffer(name.Length + 1)) : stackalloc byte[name.Length + 1];
                    name.Slice().CopyTo(newspan);
                    newspan[name.Length] = 0;   // terminator: stackalloc is not zeroed
                    pname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pname = RawRef.Of(ref name.ToSpan().GetRef()).Address;
                }
                IntPtr pmode;
                if (!mode.IsNull() && (!RuntimeInfo.IsUnity || !mode.IsNullTerminated()))
                {
                    Span<byte> newspan = mode.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmode) = new NativeBuffer(mode.Length + 1)) : stackalloc byte[mode.Length + 1];
                    mode.Slice().CopyTo(newspan);
                    newspan[mode.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmode = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmode = RawRef.Of(ref mode.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_loadbufferx(Ptr(L), pbuff, (UIntPtr)buff.Length, pname, pmode);
            }
        }
        public static int loadstring(this State L, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if  (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_loadstring(Ptr(L), ps);
            }
        }
        public static uint makeseed(this State L) => LuaImport.luaL_makeseed(Ptr(L)); // Lua 5.5
        public static CStringRef gsub(this State L, CStringSpan s, CStringSpan p, CStringSpan r)
        {
            using (NativeBuffer bs = default, bp = default(NativeBuffer), br = default)
            {
                IntPtr ps;
                if  (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                IntPtr pp;
                if (!p.IsNull() && (!RuntimeInfo.IsUnity || !p.IsNullTerminated()))
                {
                    Span<byte> newspan = p.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bp) = new NativeBuffer(p.Length + 1)) : stackalloc byte[p.Length + 1];
                    p.Slice().CopyTo(newspan);
                    newspan[p.Length] = 0;   // terminator: stackalloc is not zeroed
                    pp = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pp = RawRef.Of(ref p.ToSpan().GetRef()).Address;
                }
                IntPtr pr;
                if (!r.IsNull() && (!RuntimeInfo.IsUnity || !r.IsNullTerminated()))
                {
                    Span<byte> newspan = r.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in br) = new NativeBuffer(r.Length + 1)) : stackalloc byte[r.Length + 1];
                    r.Slice().CopyTo(newspan);
                    newspan[r.Length] = 0;   // terminator: stackalloc is not zeroed
                    pr = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pr = RawRef.Of(ref r.ToSpan().GetRef()).Address;
                }
    
                return new CStringRef(LuaImport.luaL_gsub(Ptr(L), ps, pp, pr));
            }
        }
        // l must include the {NULL, NULL} terminator entry (default(Reg) is
        // all-zero, so 'new Reg[n + 1]' with the last element untouched works).
        // Unity: the array's address never moves (Boehm GC does not compact) -
        // the first element's address is taken directly. Other hosts: the
        // array is pinned via GCHandle (no unsafe) for the duration of the
        // call, and AddrOfPinnedObject() gives the first element's address.
        public static void setfuncs(this State L, Span<Reg> l, int nup)
        {
            if (l.Length == 0)
            {
                //LuaImport.luaL_setfuncs(Ptr(L), IntPtr.Zero, nup); // Crash. Not reasonable to call this with NULL.
                return;
            }
            using (NativeBuffer bp = default)
            {
                IntPtr p;
                if (RuntimeInfo.IsUnity && l[l.Length - 1].name == IntPtr.Zero && l[l.Length - 1].func == IntPtr.Zero)
                    p = RawRef.Of(ref l[0]).Address;
                else
                {
                    var len = l.Length;
                    if (l[l.Length - 1].name == IntPtr.Zero && l[l.Length - 1].func == IntPtr.Zero)
                    {
                        --len;
                    }
                    Span<Reg> l2 = len >= _StackAllocMaxElements ? (Ref.Unprotect(in bp) = new NativeBuffer((len + 1) * Marshal.SizeOf<Reg>())).ToSpan<Reg>() : stackalloc Reg[len + 1];
                    l.CopyTo(l2);
                    l2[len] = default;   // terminator: stackalloc is not zeroed
                    p = RawRef.Of(ref l2[0]).Address;
                }
                LuaImport.luaL_setfuncs(Ptr(L), p, nup);
            }
        }
        public static int getsubtable(this State L, int idx, CStringSpan fname)
        {
            using (NativeBuffer bfname = default)
            {
                IntPtr pfname;
                if (!fname.IsNull() && (!RuntimeInfo.IsUnity || !fname.IsNullTerminated()))
                {
                    Span<byte> newspan = fname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfname) = new NativeBuffer(fname.Length + 1)) : stackalloc byte[fname.Length + 1];
                    fname.Slice().CopyTo(newspan);
                    newspan[fname.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfname = RawRef.Of(ref fname.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_getsubtable(Ptr(L), idx, pfname);
            }
        }
        public static void traceback(this State L, State L1, CStringSpan msg, int level)
        {
            using (NativeBuffer bmsg = default)
            {
                IntPtr pmsg;
                if (!msg.IsNull() && (!RuntimeInfo.IsUnity || !msg.IsNullTerminated()))
                {
                    Span<byte> newspan = msg.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmsg) = new NativeBuffer(msg.Length + 1)) : stackalloc byte[msg.Length + 1];
                    msg.Slice().CopyTo(newspan);
                    newspan[msg.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmsg = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmsg = RawRef.Of(ref msg.ToSpan().GetRef()).Address;
                }
                LuaImport.luaL_traceback(Ptr(L), Ptr(L1), pmsg, level);
            }
        }
        // WARNING: requiref calls openf unprotected - an error inside openf
        // longjmps out.
        public static void requiref(this State L, CStringSpan modname, CFunction openf, int glb)
        {
            using (NativeBuffer bmodname = default)
            {
                IntPtr pmodname;
                if (!modname.IsNull() && (!RuntimeInfo.IsUnity || !modname.IsNullTerminated()))
                {
                    Span<byte> newspan = modname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bmodname) = new NativeBuffer(modname.Length + 1)) : stackalloc byte[modname.Length + 1];
                    modname.Slice().CopyTo(newspan);
                    newspan[modname.Length] = 0;   // terminator: stackalloc is not zeroed
                    pmodname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pmodname = RawRef.Of(ref modname.ToSpan().GetRef()).Address;
                }
                LuaImport.luaL_requiref(Ptr(L), pmodname, Keep(openf), glb);
            }
        }

        #endregion

        #region generic buffer manipulation

        // NOTE for every 'ref Buffer' parameter below: B must be a stack local,
        // not a field of a heap object - the raw address is handed to Lua for
        // the whole lifetime of the buffer operation sequence (buffinit through
        // pushresult), and a GC-compacted heap location would dangle.
        // Unlike the lua_Debug functions, these do NOT bounce through a stack
        // local on compacting-GC hosts: luaL_Buffer is self-referencing (b
        // points into its own init area) and the operation sequence spans
        // multiple calls, so a per-call copy would leave b pointing at a dead
        // stack frame. The stack-local contract is the only safe usage on
        // every host.
        //
        // Lua 5.5 growth mechanism (lauxlib.c): buffinit pushes a PLACEHOLDER
        // slot (a light userdata holding B's own address - one more reason the
        // address must stay stable). On the first growth the placeholder is
        // replaced by a to-be-closed userdata "box" that owns a raw block
        // (resized through the state's lua_Alloc; new size = old * 1.5, at
        // least n + sz + 1); the current content is memcpy'd over and B.b is
        // repointed to it, so the returned writable region and buffaddr()
        // move - do not cache them across buffer calls. pushresult hands the
        // box memory to Lua as an external string with zero copies
        // (lua_pushexternalstring) and removes the slot.
        //
        // Stack discipline while the buffer is open (manual, luaL_Buffer):
        // keep every push/pop balanced between buffer calls - the
        // placeholder/box must stay on top (gettop stays at buffinit's level
        // + 1). addvalue is the only exception: the value to add sits ABOVE
        // the slot and is popped by the call. After pushresult the stack is
        // back to its pre-buffinit level plus the final string on top.

        internal static void buffinit(this State L, ref BufferL B) => LuaImport.luaL_buffinit(Ptr(L), Ptr(ref B)); // NOTE: B must be a stack local
        public static void buffinit(this State L, ref Buffer B) => buffinit(L, ref B.Inner); // NOTE: B must be a stack local
        public static void buffinit(this State L, BufferBox B) => buffinit(L, ref B.Inner);
        internal static CStringRef prepbuffsize(ref BufferL B, UIntPtr sz)
        {
            var addr = LuaImport.luaL_prepbuffsize(Ptr(ref B), sz);
            return new CStringRef(addr, (int)sz, 0);   // len 0: writable region, content not yet written
        } // NOTE: B must be a stack local; returns a writable char* region
        public static CStringRef prepbuffsize(ref Buffer B, UIntPtr sz) => prepbuffsize(ref B.Inner, sz); // NOTE: B must be a stack local; returns a writable char* region
        public static CStringRef prepbuffsize(BufferBox B, UIntPtr sz) => prepbuffsize(ref B.Inner, sz); // returns a writable char* region
        internal static void addlstring(ref BufferL B, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if (!s.IsNull() && !RuntimeInfo.IsUnity)
                {
                    Span<byte> newspan = s.Length > _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length)) : stackalloc byte[s.Length];
                    s.Slice().CopyTo(newspan);
                    ps = RawRef.Of(ref newspan.GetRef()).Address;   // empty span -> NULL (addlstring(B, NULL, 0) is a no-op)
                }
                else
                {
                    ps = RawRef.Of(ref s.Slice().GetRef()).Address; // empty span -> NULL
                }
                LuaImport.luaL_addlstring(Ptr(ref B), ps, (UIntPtr)s.Length);
            }
        } // NOTE: B must be a stack local;
        public static void addlstring(ref Buffer B, CStringSpan s) => addlstring(ref B.Inner, s); // NOTE: B must be a stack local;
        public static void addlstring(BufferBox B, CStringSpan s) => addlstring(ref B.Inner, s);
        internal static void addstring(ref BufferL B, CStringSpan s) => addlstring(ref B, s); // NOTE: B must be a stack local;
        public static void addstring(ref Buffer B, CStringSpan s) => addstring(ref B.Inner, s); // NOTE: B must be a stack local;
        public static void addstring(BufferBox B, CStringSpan s) => addstring(ref B.Inner, s);
        internal static void addvalue(ref BufferL B) => LuaImport.luaL_addvalue(Ptr(ref B)); // NOTE: B must be a stack local
        public static void addvalue(ref Buffer B) => addvalue(ref B.Inner); // NOTE: B must be a stack local
        public static void addvalue(BufferBox B) => addvalue(ref B.Inner);
        internal static void pushresult(ref BufferL B) => LuaImport.luaL_pushresult(Ptr(ref B)); // NOTE: B must be a stack local
        public static void pushresult(ref Buffer B) => pushresult(ref B.Inner); // NOTE: B must be a stack local
        public static void pushresult(BufferBox B) => pushresult(ref B.Inner);
        internal static void pushresultsize(ref BufferL B, UIntPtr sz) => LuaImport.luaL_pushresultsize(Ptr(ref B), sz); // NOTE: B must be a stack local
        public static void pushresultsize(ref Buffer B, UIntPtr sz) => pushresultsize(ref B.Inner, sz); // NOTE: B must be a stack local
        public static void pushresultsize(BufferBox B, UIntPtr sz) => pushresultsize(ref B.Inner, sz);
        internal static CStringRef buffinitsize(this State L, ref BufferL B, UIntPtr sz)
        {
            var addr = LuaImport.luaL_buffinitsize(Ptr(L), Ptr(ref B), sz);
            return new CStringRef(addr, (int)sz, 0);   // len 0: writable region, content not yet written
        } // NOTE: B must be a stack local; returns a writable char* region
        public static CStringRef buffinitsize(this State L, ref Buffer B, UIntPtr sz) => buffinitsize(L, ref B.Inner, sz);  // NOTE: B must be a stack local; returns a writable char* region
        public static CStringRef buffinitsize(this State L, BufferBox B, UIntPtr sz) => buffinitsize(L, ref B.Inner, sz);  // returns a writable char* region
        internal static void addgsub(ref BufferL B, CStringSpan s, CStringSpan p, CStringSpan r)
        {
            using (NativeBuffer bs = default, bp = default(NativeBuffer), br = default)
            {
                IntPtr ps;
                if (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                IntPtr pp;
                if (!p.IsNull() && (!RuntimeInfo.IsUnity || !p.IsNullTerminated()))
                {
                    Span<byte> newspan = p.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bp) = new NativeBuffer(p.Length + 1)) : stackalloc byte[p.Length + 1];
                    p.Slice().CopyTo(newspan);
                    newspan[p.Length] = 0;   // terminator: stackalloc is not zeroed
                    pp = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pp = RawRef.Of(ref p.ToSpan().GetRef()).Address;
                }
                IntPtr pr;
                if (!r.IsNull() && (!RuntimeInfo.IsUnity || !r.IsNullTerminated()))
                {
                    Span<byte> newspan = r.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in br) = new NativeBuffer(r.Length + 1)) : stackalloc byte[r.Length + 1];
                    r.Slice().CopyTo(newspan);
                    newspan[r.Length] = 0;   // terminator: stackalloc is not zeroed
                    pr = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pr = RawRef.Of(ref r.ToSpan().GetRef()).Address;
                }
                LuaImport.luaL_addgsub(Ptr(ref B), ps, pp, pr);
            }
        } // NOTE: B must be a stack local;
        public static void addgsub(ref Buffer B, CStringSpan s, CStringSpan p, CStringSpan r) => addgsub(ref B.Inner, s, p, r); // NOTE: B must be a stack local;
        public static void addgsub(BufferBox B, CStringSpan s, CStringSpan p, CStringSpan r) => addgsub(ref B.Inner, s, p, r);

        #endregion

        #region standard libraries

        public static int open_base(this State L) => LuaImport.luaopen_base(Ptr(L));
        public static int open_package(this State L) => LuaImport.luaopen_package(Ptr(L));
        public static int open_coroutine(this State L) => LuaImport.luaopen_coroutine(Ptr(L));
        public static int open_debug(this State L) => LuaImport.luaopen_debug(Ptr(L));
        public static int open_io(this State L) => LuaImport.luaopen_io(Ptr(L));
        public static int open_math(this State L) => LuaImport.luaopen_math(Ptr(L));
        public static int open_os(this State L) => LuaImport.luaopen_os(Ptr(L));
        public static int open_string(this State L) => LuaImport.luaopen_string(Ptr(L));
        public static int open_table(this State L) => LuaImport.luaopen_table(Ptr(L));
        public static int open_utf8(this State L) => LuaImport.luaopen_utf8(Ptr(L));
        // Lua 5.5: luaL_openlibs(L) is openselectedlibs(L, ~0, 0).
        public static void openselectedlibs(this State L, int load, int preload) => LuaImport.luaL_openselectedlibs(Ptr(L), load, preload);

        #endregion

        #region convenience macros (lua.h)

        // lua_call(L,n,r) -> lua_callk(L, n, r, 0, NULL).
        // WARNING: longjmps out on error (never returns normally).
        public static void call(this State L, int nargs, int nresults) => LuaImport.lua_callk(Ptr(L), nargs, nresults, IntPtr.Zero, IntPtr.Zero);

        // lua_pcall(L,n,r,f) -> lua_pcallk(L, n, r, f, 0, NULL); catches errors
        // as a return code.
        public static int pcall(this State L, int nargs, int nresults, int errfunc) => LuaImport.lua_pcallk(Ptr(L), nargs, nresults, errfunc, IntPtr.Zero, IntPtr.Zero);

        // lua_yield(L,n) -> lua_yieldk(L, n, 0, NULL).
        public static int yield(this State L, int nresults) => LuaImport.lua_yieldk(Ptr(L), nresults, IntPtr.Zero, IntPtr.Zero);

        // lua_tonumber / lua_tointeger -> *x with NULL isnum.
        public static double tonumber(this State L, int idx) => LuaImport.lua_tonumberx(Ptr(L), idx, IntPtr.Zero);
        public static long tointeger(this State L, int idx) => LuaImport.lua_tointegerx(Ptr(L), idx, IntPtr.Zero);

        // lua_pop(L,n) -> lua_settop(L, -(n)-1).
        public static void pop(this State L, int n) => LuaImport.lua_settop(Ptr(L), -n - 1);

        // lua_newtable(L) -> lua_createtable(L, 0, 0).
        public static void newtable(this State L) => LuaImport.lua_createtable(Ptr(L), 0, 0);

        // lua_pushcfunction(L,f) -> lua_pushcclosure(L, f, 0).
        public static void pushcfunction(this State L, CFunction fn) => LuaImport.lua_pushcclosure(Ptr(L), Keep(fn), 0);

        // lua_register(L,n,f) -> pushcfunction + setglobal.
        public static void register(this State L, CStringSpan name, CFunction f)
        {
            using (NativeBuffer bname = default)
            {
                IntPtr pname;
                if (!name.IsNull() && (!RuntimeInfo.IsUnity || !name.IsNullTerminated()))
                {
                    Span<byte> newspan = name.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bname) = new NativeBuffer(name.Length + 1)) : stackalloc byte[name.Length + 1];
                    name.Slice().CopyTo(newspan);
                    newspan[name.Length] = 0;   // terminator: stackalloc is not zeroed
                    pname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pname = RawRef.Of(ref name.ToSpan().GetRef()).Address;
                }
                LuaImport.lua_pushcclosure(Ptr(L), Keep(f), 0);
                LuaImport.lua_setglobal(Ptr(L), pname);
            }
        }

        // lua_is* family -> lua_type comparisons.
        public static bool isfunction(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TFUNCTION;
        public static bool istable(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TTABLE;
        public static bool islightuserdata(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TLIGHTUSERDATA;
        public static bool isnil(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TNIL;
        public static bool isboolean(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TBOOLEAN;
        public static bool isthread(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TTHREAD;
        public static bool isnone(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TNONE;
        public static bool isnoneornil(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) <= 0;

        // lua_pushglobaltable(L) -> rawgeti(L, REGISTRYINDEX, RIDX_GLOBALS).
        public static void pushglobaltable(this State L) => LuaImport.lua_rawgeti(Ptr(L), REGISTRYINDEX, RIDX_GLOBALS);

        // lua_tostring(L,i) -> lua_tolstring(L, i, NULL).
        public static CStringRef tostring(this State L, int idx)
        {
            return new CStringRef(LuaImport.lua_tolstring(Ptr(L), idx, IntPtr.Zero));
        }

        // lua_insert(L,idx) -> lua_rotate(L, idx, 1).
        public static void insert(this State L, int idx) => LuaImport.lua_rotate(Ptr(L), idx, 1);

        // lua_remove(L,idx) -> rotate(L, idx, -1) + pop(L, 1).
        public static void remove(this State L, int idx)
        {
            LuaImport.lua_rotate(Ptr(L), idx, -1);
            pop(L, 1);
        }

        // lua_replace(L,idx) -> copy(L, -1, idx) + pop(L, 1).
        public static void replace(this State L, int idx)
        {
            LuaImport.lua_copy(Ptr(L), -1, idx);
            pop(L, 1);
        }

        // lua_newuserdata(L,s) -> lua_newuserdatauv(L, s, 1).
        public static IntPtr newuserdata(this State L, UIntPtr sz) => LuaImport.lua_newuserdatauv(Ptr(L), sz, 1);

        // lua_getuservalue / lua_setuservalue (n == 1).
        public static int getuservalue(this State L, int idx) => LuaImport.lua_getiuservalue(Ptr(L), idx, 1);
        public static int setuservalue(this State L, int idx) => LuaImport.lua_setiuservalue(Ptr(L), idx, 1);

        // lua_resetthread(L) -> lua_closethread(L, NULL).
        public static int resetthread(this State L) => LuaImport.lua_closethread(Ptr(L), IntPtr.Zero);

        // lua_upvalueindex(i) -> LUA_REGISTRYINDEX - i.
        public static int upvalueindex(int i) => REGISTRYINDEX - i;

        // lua_getextraspace(L) -> (char*)L - LUA_EXTRASPACE.
        public static IntPtr getextraspace(this State L) => (IntPtr)(Ptr(L).ToInt64() - EXTRASPACE);

        #endregion

        #region convenience macros (lauxlib.h)

        // luaL_checkversion(L) -> checkversion_(L, LUA_VERSION_NUM, LUAL_NUMSIZES).
        public static void checkversion(this State L) => LuaImport.luaL_checkversion_(Ptr(L), VERSION_NUM, (UIntPtr)NUMSIZES); // WARNING: longjmps on mismatch

        // luaL_loadfile(L,f) -> luaL_loadfilex(L, f, NULL).
        public static int loadfile(this State L, CStringSpan filename)
        {
            using (NativeBuffer bfilename = default)
            {
                IntPtr pfilename;
                if (!filename.IsNull() && (!RuntimeInfo.IsUnity || !filename.IsNullTerminated()))
                {
                    Span<byte> newspan = filename.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfilename) = new NativeBuffer(filename.Length + 1)) : stackalloc byte[filename.Length + 1];
                    filename.Slice().CopyTo(newspan);
                    newspan[filename.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfilename = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfilename = RawRef.Of(ref filename.ToSpan().GetRef()).Address;
                }
                return LuaImport.luaL_loadfilex(Ptr(L), pfilename, IntPtr.Zero);
            }
        }

        // luaL_checkstring / luaL_optstring -> *lstring with NULL len.
        public static CStringRef checkstring(this State L, int n)
        {
            return new CStringRef(LuaImport.luaL_checklstring(Ptr(L), n, IntPtr.Zero));
        } // WARNING: longjmps on bad argument
        public static CStringSpan optstring(this State L, int n, CStringSpan def) => optlstring(L, n, def);

        // luaL_typename(L,i) -> lua_typename(L, lua_type(L,i)).
        public static CStringRef typenameL(this State L, int idx) => new CStringRef(LuaImport.lua_typename(Ptr(L), LuaImport.lua_type(Ptr(L), idx)));

        // luaL_dofile(L, fn) -> loadfile || pcall(0, MULTRET, 0).
        public static int dofile(this State L, CStringSpan filename)
        {
            using (NativeBuffer bfilename = default)
            {
                IntPtr pfilename;
                if (!filename.IsNull() && (!RuntimeInfo.IsUnity || !filename.IsNullTerminated()))
                {
                    Span<byte> newspan = filename.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bfilename) = new NativeBuffer(filename.Length + 1)) : stackalloc byte[filename.Length + 1];
                    filename.Slice().CopyTo(newspan);
                    newspan[filename.Length] = 0;   // terminator: stackalloc is not zeroed
                    pfilename = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    pfilename = RawRef.Of(ref filename.ToSpan().GetRef()).Address;
                }
                int r = LuaImport.luaL_loadfilex(Ptr(L), pfilename, IntPtr.Zero);
                return r != OK ? r : pcall(L, 0, MULTRET, 0);
            }
        }

        // luaL_dostring(L, s) -> loadstring || pcall(0, MULTRET, 0).
        public static int dostring(this State L, CStringSpan s)
        {
            using (NativeBuffer bs = default)
            {
                IntPtr ps;
                if (!s.IsNull() && (!RuntimeInfo.IsUnity || !s.IsNullTerminated()))
                {
                    Span<byte> newspan = s.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in bs) = new NativeBuffer(s.Length + 1)) : stackalloc byte[s.Length + 1];
                    s.Slice().CopyTo(newspan);
                    newspan[s.Length] = 0;   // terminator: stackalloc is not zeroed
                    ps = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ps = RawRef.Of(ref s.ToSpan().GetRef()).Address;
                }
                int r = LuaImport.luaL_loadstring(Ptr(L), ps);
                return r != OK ? r : pcall(L, 0, MULTRET, 0);
            }
        }

        // luaL_getmetatable(L,n) -> lua_getfield(L, LUA_REGISTRYINDEX, n).
        public static int getmetatable(this State L, CStringSpan tname)
        {
            using (NativeBuffer btname = default)
            {
                IntPtr ptname;
                if (!tname.IsNull() && (!RuntimeInfo.IsUnity || !tname.IsNullTerminated()))
                {
                    Span<byte> newspan = tname.Length >= _StackAllocMaxElements ? (Ref.Unprotect(in btname) = new NativeBuffer(tname.Length + 1)) : stackalloc byte[tname.Length + 1];
                    tname.Slice().CopyTo(newspan);
                    newspan[tname.Length] = 0;   // terminator: stackalloc is not zeroed
                    ptname = RawRef.Of(ref newspan[0]).Address;
                }
                else
                {
                    ptname = RawRef.Of(ref tname.ToSpan().GetRef()).Address;
                }
                return LuaImport.lua_getfield(Ptr(L), REGISTRYINDEX, ptname);
            }
        }// macro form;

        // luaL_loadbuffer(L,s,sz,n) -> loadbufferx(L, s, sz, n, NULL).
        public static int loadbuffer(this State L, CStringSpan s, CStringSpan name) => loadbufferx(L, s, name, default);

        // luaL_pushfail(L): default configuration (no LUA_FAILISFALSE) ->
        // lua_pushnil(L).
        public static void pushfail(this State L) => LuaImport.lua_pushnil(Ptr(L));

        // luaL_bufflen(bf) / luaL_buffaddr(bf) -> field reads.
        internal static UIntPtr bufflen(ref BufferL B) => B.n;
        public static UIntPtr bufflen(ref Buffer B) => B.n;
        public static UIntPtr bufflen(BufferBox B) => B.n;
        internal static IntPtr buffaddr(ref BufferL B) => B.b;
        public static IntPtr buffaddr(ref Buffer B) => B.b;
        public static IntPtr buffaddr(BufferBox B) => B.b;

        // luaL_addchar(B,c): grow by 1 if full, write the byte, advance n.
        // NOTE: B must be a stack local (see the buffer region above).
        internal static void addchar(ref BufferL B, byte c)
        {
            if (B.n.ToUInt64() >= B.size.ToUInt64()) LuaImport.luaL_prepbuffsize(Ptr(ref B), (UIntPtr)1);
            Marshal.WriteByte(B.b, (int)B.n, c);
            B.n = (UIntPtr)(B.n.ToUInt64() + 1);
        }
        public static void addchar(ref Buffer B, byte c) => addchar(ref B.Inner, c); // luaL_addchar(B,c): grow by 1 if full, write the byte, advance n. // NOTE: B must be a stack local (see the buffer region above).
        public static void addchar(BufferBox B, byte c) => addchar(ref B.Inner, c); // luaL_addchar(B,c): grow by 1 if full, write the byte, advance n.

        // luaL_addsize(B,s) / luaL_buffsub(B,s) -> n += s / n -= s.
        internal static void addsize(ref BufferL B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        public static void addsize(ref Buffer B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        public static void addsize(BufferBox B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        internal static void buffsub(ref BufferL B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());
        public static void buffsub(ref Buffer B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());
        public static void buffsub(BufferBox B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());

        // luaL_prepbuffer(B) -> prepbuffsize(B, LUAL_BUFFERSIZE).
        internal static CStringRef prepbuffer(ref BufferL B) => prepbuffsize(ref B, BUFFERSIZE); // NOTE: B must be a stack local
        public static CStringRef prepbuffer(ref Buffer B) => prepbuffer(ref B.Inner); // NOTE: B must be a stack local
        public static CStringRef prepbuffer(BufferBox B) => prepbuffer(ref B.Inner); // NOTE: B must be a stack local

        // luaL_argcheck / luaL_argexpected: cond || argerror/typeerror.
        public static void argcheck(this State L, bool cond, int arg, CStringSpan extramsg)
        {
            if (!cond) argerror(L, arg, extramsg);
        } // WARNING: argerror longjmps (never returns)
        public static void argexpected(this State L, bool cond, int arg, CStringSpan tname)
        {
            if (!cond) typeerror(L, arg, tname);
        } // WARNING: typeerror longjmps (never returns)

        // Narrowing conveniences over checkinteger/optinteger.
        public static int checkint(this State L, int n) => (int)LuaImport.luaL_checkinteger(Ptr(L), n); // WARNING: longjmps on bad argument
        public static int optint(this State L, int n, int def) => (int)LuaImport.luaL_optinteger(Ptr(L), n, def);
        public static ulong checkunsigned(this State L, int n) => (ulong)LuaImport.luaL_checkinteger(Ptr(L), n); // WARNING: longjmps on bad argument
        public static ulong optunsigned(this State L, int n, ulong def) => (ulong)LuaImport.luaL_optinteger(Ptr(L), n, (long)def);

        // luaL_newlibtable(L, l) -> lua_createtable(L, 0, sizeof(l)/sizeof(l[0]) - 1):
        // preallocates the library table for the number of real entries in l.
        // The C macro counts the array elements minus the {NULL,NULL} terminator;
        // this facade applies the same convention as setfuncs - an explicit
        // all-zero last entry is not counted, anything else is (the terminator
        // is then appended automatically by setfuncs).
        public static void newlibtable(this State L, Span<Reg> l)
        {
            int count = l.Length;
            if (count > 0 && l[count - 1].name == IntPtr.Zero && l[count - 1].func == IntPtr.Zero)
                --count;
            LuaImport.lua_createtable(Ptr(L), 0, count);
        }

        // luaL_newlib(L, l) -> (luaL_checkversion(L), luaL_newlibtable(L, l),
        // luaL_setfuncs(L, l, 0)): registers all functions in l into a fresh
        // table left on the stack. WARNING: checkversion longjmps on mismatch.
        public static void newlib(this State L, Span<Reg> l)
        {
            checkversion(L);
            newlibtable(L, l);
            setfuncs(L, l, 0);
        }

        // luaL_checklong(L, n) -> (long)luaL_checkinteger(L, n).
        // NOTE: C 'long' is 32-bit on Windows (LLP64), where the C macro
        // truncates values outside int32 range; this facade returns the full
        // 64-bit lua_Integer (no truncation).
        public static long checklong(this State L, int n) => LuaImport.luaL_checkinteger(Ptr(L), n); // WARNING: longjmps on bad argument

        // luaL_optlong(L, n, d) -> (long)luaL_optinteger(L, n, d). Same LLP64
        // truncation note as checklong.
        public static long optlong(this State L, int n, long d) => LuaImport.luaL_optinteger(Ptr(L), n, d);

        // luaL_opt(L, f, n, d) -> (lua_isnoneornil(L, n) ? d : f(L, n)).
        // Generic macro shape: f is any argument checker (checkinteger,
        // checknumber, checklstring, ...). The Func parameter allocates on
        // non-cached lambdas - use a static lambda (no captures) to get the
        // compiler-cached delegate.
        public static T opt<T>(this State L, Func<State, int, T> f, int n, T d)
            => isnoneornil(L, n) ? d : f(L, n);

        // luaL_intop(op, v1, v2) -> (lua_Integer)((lua_Unsigned)(v1) op (lua_Unsigned)(v2)):
        // wrap-around arithmetic on lua_Integer, the semantics the Lua core
        // relies on. Shift counts follow the practical C behavior (the shift
        // amount is masked by the hardware, e.g. to 6 bits on x64).
        public static long intopAdd(long v1, long v2) => (long)((ulong)v1 + (ulong)v2);
        public static long intopSub(long v1, long v2) => (long)((ulong)v1 - (ulong)v2);
        public static long intopMul(long v1, long v2) => (long)((ulong)v1 * (ulong)v2);
        public static long intopAnd(long v1, long v2) => (long)((ulong)v1 & (ulong)v2);
        public static long intopOr(long v1, long v2) => (long)((ulong)v1 | (ulong)v2);
        public static long intopXor(long v1, long v2) => (long)((ulong)v1 ^ (ulong)v2);
        public static long intopShl(long v1, long v2) => (long)((ulong)v1 << (int)v2);
        public static long intopShr(long v1, long v2) => (long)((ulong)v1 >> (int)v2);

        #endregion
    }
}


