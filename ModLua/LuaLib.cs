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
    /// <remarks>
    /// Stack-effect annotation (from the Lua 5.5 manual), shown at the head of
    /// every function's summary as <c>[-pops, +pushes, errors]</c>:
    ///  - first field: how many elements the function pops from the Lua stack
    ///    (negative numbers; ? = depends on the arguments);
    ///  - second field: how many elements it pushes (or alternatives like
    ///    +(0|1));
    ///  - third field: error behavior - '-' never throws; 'm' may raise only
    ///    out-of-memory errors; 'e' may raise any error (including errors
    ///    raised by called Lua code, e.g. metamethods); 'v' may raise an
    ///    error on purpose (argument checking).
    /// In this binding the third letter doubles as the longjmp-risk grade:
    /// anything other than '-' unwinds through native frames (setjmp/longjmp),
    /// so managed finally/using blocks on the call path may be skipped.
    /// </remarks>
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

        /// <summary>LUA_EXTRASPACE = sizeof(void*); runtime value (pointer size).</summary>
        public static readonly int EXTRASPACE = IntPtr.Size;

        /// <summary>LUAL_BUFFERSIZE = 16 * sizeof(void*) * sizeof(lua_Number): 1024 on 64-bit, 512 on 32-bit (lua_Number is double).</summary>
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

        /// <summary>
        /// LUAL_NUMSIZES = sizeof(lua_Integer)*16 + sizeof(lua_Number): packs the two
        /// numeric-type sizes into one scalar (136 under the default LONGLONG+DOUBLE
        /// config). checkversion_ compares it against the core's own value - an ABI
        /// self-check that catches a native Lua built with a non-default numeric
        /// configuration (e.g. LUA_32BITS).
        /// </summary>
        public static readonly int NUMSIZES = sizeof(long) * 16 + sizeof(double);

        /// <summary>Signature of binary chunks (lua.h LUA_SIGNATURE): ESC "Lua". The first 4 bytes of anything dump() produces / load() accepts.</summary>
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

        /// <summary><c>[-0, +0, -]</c> Creates a new state with a custom allocator and a random seed (lua_newstate).</summary>
        /// <remarks>Returns default(State) on allocation failure.</remarks>
        public static State newstate(Alloc f, IntPtr ud, uint seed) => new State { _L = LuaImport.lua_newstate(Keep(f), ud, seed) };
        /// <summary><c>[-0, +0, -]</c> Creates a new state with the default allocator (luaL_newstate).</summary>
        /// <remarks>Installs the default panic and warn functions (lauxlib).</remarks>
        public static State newstate() => new State { _L = LuaImport.luaL_newstate() };
        /// <summary><c>[-0, +0, -]</c> Destroys all objects in the state and releases all memory (lua_close).</summary>
        public static void close(this State L) => LuaImport.lua_close(Ptr(L));
        /// <summary><c>[-0, +1, m]</c> Creates a new coroutine thread sharing the global environment (lua_newthread).</summary>
        public static State newthread(this State L) => new State { _L = LuaImport.lua_newthread(Ptr(L)) };
        /// <summary><c>[-0, +?, -]</c> Resets a thread, closing its pending variables to be closed, and returns its status (lua_closethread).</summary>
        /// <remarks>from may be default(State) (NULL) to close only the given thread.</remarks>
        public static int closethread(this State L, State from) => LuaImport.lua_closethread(Ptr(L), Ptr(from));
        /// <summary><c>[-0, +0, -]</c> Sets a new panic function; returns the old one (lua_atpanic).</summary>
        /// <remarks>The returned delegate is the previous panic function wrapped via GetDelegateForFunctionPointer.</remarks>
        public static CFunction atpanic(this State L, CFunction panicf) => ToDel<CFunction>(LuaImport.lua_atpanic(Ptr(L), Keep(panicf)));
        /// <summary><c>[-0, +0, -]</c> Returns the version number of the core (lua_version).</summary>
        /// <remarks>L may be default(State) (NULL) to query the linked core without a state.</remarks>
        public static double version(this State L) => LuaImport.lua_version(Ptr(L));

        #endregion

        #region basic stack manipulation

        /// <summary><c>[-0, +0, -]</c> Converts an acceptable index into an equivalent absolute index (lua_absindex).</summary>
        public static int absindex(this State L, int idx) => LuaImport.lua_absindex(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns the index of the top element (lua_gettop).</summary>
        public static int gettop(this State L) => LuaImport.lua_gettop(Ptr(L));
        /// <summary><c>[-?, +?, e]</c> Sets the stack top to the given index (lua_settop).</summary>
        /// <remarks>Lowering the top may trigger __close metamethods of popped to-be-closed
        /// variables (hence 'e'); those are Lua code and can raise errors.</remarks>
        public static void settop(this State L, int idx) => LuaImport.lua_settop(Ptr(L), idx);
        /// <summary><c>[-0, +1, -]</c> Pushes a copy of the element at idx onto the stack (lua_pushvalue).</summary>
        public static void pushvalue(this State L, int idx) => LuaImport.lua_pushvalue(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Rotates the stack elements between idx and the top by n positions (lua_rotate).</summary>
        /// <remarks>Positive n rotates towards the top; n == 1 is equivalent to insert; n == -1 to
        /// remove (followed by pop). Zeroed by raising an error on bad input.</remarks>
        public static void rotate(this State L, int idx, int n) => LuaImport.lua_rotate(Ptr(L), idx, n);
        /// <summary><c>[-0, +0, -]</c> Copies the element at fromidx to toidx (lua_copy).</summary>
        public static void copy(this State L, int fromidx, int toidx) => LuaImport.lua_copy(Ptr(L), fromidx, toidx);
        /// <summary><c>[-0, +0, -]</c> Ensures the stack has n extra slots; returns 1 on success (lua_checkstack).</summary>
        public static int checkstack(this State L, int n) => LuaImport.lua_checkstack(Ptr(L), n);
        /// <summary><c>[-0, +0, v]</c> Grows the stack to sz elements or raises an error (luaL_checkstack).</summary>
        /// <remarks>WARNING: longjmps on failure - never returns normally on error, and managed
        /// finally/using blocks on the call path may be skipped.</remarks>
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
        }
        /// <summary><c>[-?, +?, -]</c> Exchanges n elements between two states (lua_xmove).</summary>
        public static void xmove(this State from, State to, int n) => LuaImport.lua_xmove(Ptr(from), Ptr(to), n);

        #endregion

        #region access functions (stack -> C)

        /// <summary><c>[-0, +0, -]</c> Returns 1 if the value at idx is convertible to a number (lua_isnumber).</summary>
        public static int isnumber(this State L, int idx) => LuaImport.lua_isnumber(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns 1 if the value at idx is a string or a number (lua_isstring).</summary>
        public static int isstring(this State L, int idx) => LuaImport.lua_isstring(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns 1 if the value at idx is a C function (lua_iscfunction).</summary>
        public static int iscfunction(this State L, int idx) => LuaImport.lua_iscfunction(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns 1 if the value at idx is an integer-valued number (lua_isinteger).</summary>
        public static int isinteger(this State L, int idx) => LuaImport.lua_isinteger(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns 1 if the value at idx is a (full or light) userdata (lua_isuserdata).</summary>
        public static int isuserdata(this State L, int idx) => LuaImport.lua_isuserdata(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns the type of the value at idx, or TNONE for a bad index (lua_type).</summary>
        public static int type(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns the name of the type tag tp (lua_typename).</summary>
        /// <remarks>The returned CStringRef is backed by a static literal - permanently valid.</remarks>
        public static CStringRef typename(this State L, int tp) => new CStringRef(LuaImport.lua_typename(Ptr(L), tp));
        /// <summary><c>[-0, +0, -]</c> Converts the value at idx to a double; isnum reports success (lua_tonumberx).</summary>
        public static double tonumberx(this State L, int idx, out bool isnum)
        {
            int bIsNum = 0;
            var result = LuaImport.lua_tonumberx(Ptr(L), idx, RawRef.Of(ref bIsNum));
            isnum = bIsNum != 0;
            return result;
        }
        /// <summary><c>[-0, +0, -]</c> Converts the value at idx to a long; isnum reports success (lua_tointegerx).</summary>
        public static long tointegerx(this State L, int idx, out bool isnum)
        {
            int bIsNum = 0;
            var result = LuaImport.lua_tointegerx(Ptr(L), idx, RawRef.Of(ref bIsNum));
            isnum = bIsNum != 0;
            return result;
        }
        /// <summary><c>[-0, +0, -]</c> Returns the boolean truth value at idx (lua_toboolean).</summary>
        public static int toboolean(this State L, int idx) => LuaImport.lua_toboolean(Ptr(L), idx);
        /// <summary><c>[-0, +0, m]</c> Converts the value at idx to a C string; returns null for non-convertible values (lua_tolstring).</summary>
        /// <remarks>WARNING (manual): if the value is a number, the actual stack slot is CHANGED
        /// to a string (in-place conversion) - this also confuses lua_next when applied to keys
        /// during a table traversal. The returned pointer is only valid while the value stays on
        /// the stack (see the manual, "Pointers to Strings").</remarks>
        public static CStringRef tolstring(this State L, int idx)
        {
            IntPtr len = default;
            var pstr = LuaImport.lua_tolstring(Ptr(L), idx, RawRef.Of(ref len));
            return new CStringRef(pstr, (int)len + 1, (int)len);
        }
        /// <summary><c>[-0, +1, e]</c> Converts any value to a string honouring __tostring; the result is pushed and returned (luaL_tolstring).</summary>
        /// <remarks>Raises an error when __tostring fails (hence 'e'). The pushed string's pointer is
        /// valid while the value stays on the stack.</remarks>
        public static CStringRef tolstringL(this State L, int idx)
        {
            IntPtr len = default;
            var pstr = LuaImport.luaL_tolstring(Ptr(L), idx, RawRef.Of(ref len));
            return new CStringRef(pstr, (int)len + 1, (int)len);

        }
        /// <summary><c>[-0, +0, -]</c> Returns the raw length of the value at idx (lua_rawlen).</summary>
        public static ulong rawlen(this State L, int idx) => LuaImport.lua_rawlen(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns the C function at idx as a delegate, or null (lua_tocfunction).</summary>
        public static CFunction tocfunction(this State L, int idx) => ToDel<CFunction>(LuaImport.lua_tocfunction(Ptr(L), idx));
        /// <summary><c>[-0, +0, -]</c> Returns the memory block of the userdata at idx (lua_touserdata).</summary>
        public static IntPtr touserdata(this State L, int idx) => LuaImport.lua_touserdata(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Returns the thread at idx (lua_tothread).</summary>
        public static State tothread(this State L, int idx) => new State { _L = LuaImport.lua_tothread(Ptr(L), idx) };
        /// <summary><c>[-0, +0, -]</c> Returns a light-internal pointer to the value at idx (lua_topointer).</summary>
        public static IntPtr topointer(this State L, int idx) => LuaImport.lua_topointer(Ptr(L), idx);

        #endregion

        #region comparison and arithmetic functions

        /// <summary><c>[-(2|1), +1, e]</c> Performs an arithmetic operation over the top values (lua_arith).</summary>
        /// <remarks>Pops two operands (unary ops use only the top one) and pushes the result;
        /// may trigger metamethods ('e'). op is one of OPADD..OPBNOT.</remarks>
        public static void arith(this State L, int op) => LuaImport.lua_arith(Ptr(L), op);
        /// <summary><c>[-0, +0, -]</c> Raw equality (no metamethods) of the two values (lua_rawequal).</summary>
        public static int rawequal(this State L, int idx1, int idx2) => LuaImport.lua_rawequal(Ptr(L), idx1, idx2);
        /// <summary><c>[-0, +0, e]</c> Compares two values with OPEQ/OPLT/OPLE, honouring metamethods (lua_compare).</summary>
        public static int compare(this State L, int idx1, int idx2, int op) => LuaImport.lua_compare(Ptr(L), idx1, idx2, op);

        #endregion

        #region push functions (C -> stack)

        /// <summary><c>[-0, +1, -]</c> Pushes a nil (lua_pushnil).</summary>
        public static void pushnil(this State L) => LuaImport.lua_pushnil(Ptr(L));
        /// <summary><c>[-0, +1, -]</c> Pushes a float number (lua_pushnumber).</summary>
        public static void pushnumber(this State L, double n) => LuaImport.lua_pushnumber(Ptr(L), n);
        /// <summary><c>[-0, +1, -]</c> Pushes an integer number (lua_pushinteger).</summary>
        public static void pushinteger(this State L, long n) => LuaImport.lua_pushinteger(Ptr(L), n);
        /// <summary><c>[-0, +1, v]</c> Pushes the string s (length-prefixed by the span) onto the stack (lua_pushlstring).</summary>
        /// <remarks>Embedded zeros are allowed. Returns the CStringRef of the pushed (interned) copy -
        /// valid while the value stays on the stack. An empty span pushes an empty string.</remarks>
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
        /// <summary><c>[-0, +1, m]</c> Pushes an external string backed by caller-owned memory (lua_pushexternalstring).</summary>
        /// <remarks>The buffer is NOT copied: Lua stores the pointer and calls falloc(ud, s, len+1, 0)
        /// when the string is collected. s[len] must be '\0' (the native api_check dereferences it) -
        /// an empty CStringRef is an invalid argument, same as in C. falloc = null marks the string
        /// as fixed (never deallocated). Returns the input reference (the string's identity).</remarks>
        public static CStringRef pushexternalstring(this State L, CStringRef s, Alloc falloc, IntPtr ud)
        {
            // Empty span -> NULL, but an external string REQUIRES s[len] == '\0'
            // (native api_check dereferences it) - an empty CStringRef is an
            // invalid argument either way, same as in C.
            IntPtr ps = RawRef.Of(ref s.Slice().GetRef()).Address;
            LuaImport.lua_pushexternalstring(Ptr(L), ps, (UIntPtr)s.Length, Keep(falloc), ud);
            return s;
        }
        /// <summary><c>[-0, +1, m]</c> Pushes the NUL-terminated string s onto the stack (lua_pushstring).</summary>
        /// <remarks>A null span pushes nil. Returns the CStringRef of the pushed copy - valid while
        /// the value stays on the stack.</remarks>
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
        /// <summary><c>[-0, +1, -]</c> Pushes a formatted string from a va_list argument pointer (lua_pushvfstring).</summary>
        /// <remarks>TODO: argp is a raw va_list - platform-specific, no portable C# shape.</remarks>
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
        }
        /// <summary><c>[-0, +1, v]</c> Pushes a formatted string, no conversion specifiers in fmt (lua_pushfstring).</summary>
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
        }
        /// <summary><c>[-0, +1, v]</c> Pushes a formatted string with a single conversion argument (lua_pushfstring).</summary>
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
        }
        /// <summary><c>[-n, +1, m]</c> Pushes a C function with n upvalues as a closure (lua_pushcclosure).</summary>
        /// <remarks>The n upvalues are popped from the stack. The delegate is kept alive for the
        /// lifetime of the process (Keep registry - Lua holds a raw thunk pointer).</remarks>
        public static void pushcclosure(this State L, CFunction fn, int n) => LuaImport.lua_pushcclosure(Ptr(L), Keep(fn), n);
        /// <summary><c>[-0, +1, -]</c> Pushes a boolean with the given truth value (lua_pushboolean).</summary>
        public static void pushboolean(this State L, int b) => LuaImport.lua_pushboolean(Ptr(L), b);
        /// <summary><c>[-0, +1, -]</c> Pushes a light userdata with the given pointer (lua_pushlightuserdata).</summary>
        public static void pushlightuserdata(this State L, IntPtr p) => LuaImport.lua_pushlightuserdata(Ptr(L), p);
        /// <summary><c>[-0, +1, -]</c> Pushes the running thread; returns 1 if it is the main thread (lua_pushthread).</summary>
        public static int pushthread(this State L) => LuaImport.lua_pushthread(Ptr(L));

        #endregion

        #region get functions (Lua -> stack)

        /// <summary><c>[-0, +1, e]</c> Pushes the value of the global name; returns its type (lua_getglobal).</summary>
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
        /// <summary><c>[-1, +1, e]</c> Pops a key and pushes the value from the table at idx (lua_gettable).</summary>
        public static int gettable(this State L, int idx) => LuaImport.lua_gettable(Ptr(L), idx);
        /// <summary><c>[-0, +1, e]</c> Pushes the value of field k from the table at idx (lua_getfield).</summary>
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
        /// <summary><c>[-0, +1, e]</c> Pushes the value t[n] from the table at idx (lua_geti).</summary>
        public static int geti(this State L, int idx, long n) => LuaImport.lua_geti(Ptr(L), idx, n);
        /// <summary><c>[-1, +1, -]</c> Raw get: pops a key, pushes t[key] without metamethods (lua_rawget).</summary>
        public static int rawget(this State L, int idx) => LuaImport.lua_rawget(Ptr(L), idx);
        /// <summary><c>[-0, +1, -]</c> Raw get of t[n] (lua_rawgeti).</summary>
        public static int rawgeti(this State L, int idx, long n) => LuaImport.lua_rawgeti(Ptr(L), idx, n);
        /// <summary><c>[-0, +1, -]</c> Raw get of t[p] with a light userdata key (lua_rawgetp).</summary>
        public static int rawgetp(this State L, int idx, IntPtr p) => LuaImport.lua_rawgetp(Ptr(L), idx, p);
        /// <summary><c>[-0, +1, m]</c> Creates a new empty table with array/hash hints (lua_createtable).</summary>
        public static void createtable(this State L, int narr, int nrec) => LuaImport.lua_createtable(Ptr(L), narr, nrec);
        /// <summary><c>[-0, +1, m]</c> Creates a full userdata block of sz bytes with nuvalue user values (lua_newuserdatauv).</summary>
        public static IntPtr newuserdatauv(this State L, UIntPtr sz, int nuvalue) => LuaImport.lua_newuserdatauv(Ptr(L), sz, nuvalue);
        /// <summary><c>[-0, +(0|1), -]</c> Pushes the metatable of the value at objindex if it has one (lua_getmetatable).</summary>
        public static int getmetatable(this State L, int objindex) => LuaImport.lua_getmetatable(Ptr(L), objindex);
        /// <summary><c>[-0, +1, -]</c> Pushes the n-th user value of the userdata at idx (lua_getiuservalue).</summary>
        public static int getiuservalue(this State L, int idx, int n) => LuaImport.lua_getiuservalue(Ptr(L), idx, n);

        #endregion

        #region set functions (stack -> Lua)

        /// <summary><c>[-1, +0, e]</c> Pops a value and assigns it to the global name (lua_setglobal).</summary>
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
        /// <summary><c>[-2, +0, e]</c> Pops a value and a key, then does t[key] = value (lua_settable).</summary>
        public static void settable(this State L, int idx) => LuaImport.lua_settable(Ptr(L), idx);
        /// <summary><c>[-1, +0, e]</c> Pops a value and does t[k] = value on the table at idx (lua_setfield).</summary>
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
        /// <summary><c>[-1, +0, e]</c> Pops a value and does t[n] = value on the table at idx (lua_seti).</summary>
        public static void seti(this State L, int idx, long n) => LuaImport.lua_seti(Ptr(L), idx, n);
        /// <summary><c>[-2, +0, m]</c> Raw set: pops a value and a key, t[key] = value without metamethods (lua_rawset).</summary>
        public static void rawset(this State L, int idx) => LuaImport.lua_rawset(Ptr(L), idx);
        /// <summary><c>[-1, +0, m]</c> Raw set of t[n] = value (pops the value) (lua_rawseti).</summary>
        public static void rawseti(this State L, int idx, long n) => LuaImport.lua_rawseti(Ptr(L), idx, n);
        /// <summary><c>[-1, +0, m]</c> Raw set of t[p] = value with a light userdata key (lua_rawsetp).</summary>
        public static void rawsetp(this State L, int idx, IntPtr p) => LuaImport.lua_rawsetp(Ptr(L), idx, p);
        /// <summary><c>[-1, +0, -]</c> Pops a table and sets it as the metatable of the value at objindex (lua_setmetatable).</summary>
        public static int setmetatable(this State L, int objindex) => LuaImport.lua_setmetatable(Ptr(L), objindex);
        /// <summary><c>[-0, +0, -]</c> Sets the registry metatable tname as the metatable of the value on top (luaL_setmetatable).</summary>
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
        }
        /// <summary><c>[-1, +0, -]</c> Pops a value and assigns it as the n-th user value of the userdata at idx (lua_setiuservalue).</summary>
        public static int setiuservalue(this State L, int idx, int n) => LuaImport.lua_setiuservalue(Ptr(L), idx, n);

        #endregion

        #region 'load' and 'call' functions

        /// <summary><c>[-(nargs+1), +nresults, e]</c> Calls a function with a continuation (lua_callk).</summary>
        /// <remarks>WARNING: longjmps out on error (never returns normally); do not rely on
        /// finally/using semantics on this call path.</remarks>
        public static void callk(this State L, int nargs, int nresults, IntPtr ctx, KFunction k) => LuaImport.lua_callk(Ptr(L), nargs, nresults, ctx, Keep(k));
        /// <summary><c>[-(nargs+1), +(nresults|1), -]</c> Protected call with a continuation; errors come back as return codes (lua_pcallk).</summary>
        /// <remarks>On error pushes the error object (hence +(nresults|1)). Does not longjmp for
        /// called-code errors; memory errors still longjmp.</remarks>
        public static int pcallk(this State L, int nargs, int nresults, int errfunc, IntPtr ctx, KFunction k) => LuaImport.lua_pcallk(Ptr(L), nargs, nresults, errfunc, ctx, Keep(k));
        /// <summary><c>[-0, +1, -]</c> Loads a chunk with a reader callback; errors come back as return codes (lua_load).</summary>
        /// <remarks>mode: "b"/"t"/"bt" (null span = "bt"); chunkname is the source name for
        /// error messages and debug info.</remarks>
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
        /// <summary><c>[-0, +0, -]</c> Dumps the function on top as a binary chunk via a writer callback (lua_dump).</summary>
        /// <remarks>strip removes debug information. The writer's return value controls abort.</remarks>
        public static int dump(this State L, Writer writer, IntPtr data, int strip) => LuaImport.lua_dump(Ptr(L), Keep(writer), data, strip);

        #endregion

        #region coroutine functions

        /// <summary><c>[-?, +?, v]</c> Yields from a C function with a continuation (lua_yieldk).</summary>
        /// <remarks>WARNING: longjmps if the thread is not yieldable; never returns normally
        /// in the successful case either (control goes back to the resumer).</remarks>
        public static int yieldk(this State L, int nresults, IntPtr ctx, KFunction k) => LuaImport.lua_yieldk(Ptr(L), nresults, ctx, Keep(k));
        /// <summary><c>[-?, +?, -]</c> Starts/resumes a coroutine; nres receives the number of transferred values (lua_resume).</summary>
        public static int resume(this State L, State from, int narg, out int nres)
        {
            int lnres = 0;
            var result = LuaImport.lua_resume(Ptr(L), Ptr(from), narg, RawRef.Of(ref lnres));
            nres = lnres;
            return result;
        }
        /// <summary><c>[-0, +0, -]</c> Returns the status of the thread (lua_status).</summary>
        public static int status(this State L) => LuaImport.lua_status(Ptr(L));
        /// <summary><c>[-0, +0, -]</c> Returns 1 if the thread can yield (lua_isyieldable).</summary>
        public static int isyieldable(this State L) => LuaImport.lua_isyieldable(Ptr(L));

        #endregion

        #region warning-related functions

        /// <summary><c>[-0, +0, -]</c> Sets the warning function (lua_setwarnf).</summary>
        public static void setwarnf(this State L, WarnFunction f, IntPtr ud) => LuaImport.lua_setwarnf(Ptr(L), Keep(f), ud);
        /// <summary><c>[-0, +0, -]</c> Emits a warning; tocont marks a continued warning sequence (lua_warning).</summary>
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

        /// <summary><c>[-0, +0, -]</c> Zero-extra-argument gc commands (lua_gc).</summary>
        /// <remarks>GCSTOP/GCRESTART/GCCOLLECT/GCCOUNT/GCCOUNTB/GCISRUNNING/GCGEN/GCINC.</remarks>
        public static int gc(this State L, int what) => LuaImport.lua_gc(Ptr(L), what, UIntPtr.Zero, 0);

        /// <summary><c>[-0, +0, -]</c> GCSTEP: performs an incremental GC step (lua_gc).</summary>
        /// <remarks>data = step size in bytes; 0 forces one basic step. Returns 1 when the cycle
        /// finished (back to pause state).</remarks>
        public static int gc(this State L, int what, UIntPtr data) => LuaImport.lua_gc(Ptr(L), what, data, 0);

        /// <summary><c>[-0, +0, -]</c> GCPARAM: gets/sets generational GC parameters (lua_gc).</summary>
        /// <remarks>param 0..6 (GCPMINORMUL/GCPMAJORMINOR/GCPMINORMAJOR/GCPPAUSE/GCPSTEPMUL/
        /// GCPSTEPSIZE/GCPN); value >= 0 sets, value < 0 (convention: -1) queries. Returns the
        /// previous value (percent-scaled; log-encoded internally, so round-trips are approximate).</remarks>
        public static int gc(this State L, int what, int param, int value) => LuaImport.lua_gc(Ptr(L), what, (UIntPtr)(uint)param, value);

        #endregion

        #region miscellaneous functions

        /// <summary><c>[-1, +0, v]</c> Raises the value on top as an error (lua_error).</summary>
        /// <remarks>WARNING: longjmps out (never returns normally); do not rely on
        /// finally/using semantics on this call path.</remarks>
        public static int error(this State L) => LuaImport.lua_error(Ptr(L));
        /// <summary><c>[-0, +0, v]</c> Raises a formatted error message (luaL_error).</summary>
        /// <remarks>WARNING: longjmps out (never returns normally). fmt must contain no
        /// conversion specifiers.</remarks>
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
        }
        /// <summary><c>[-0, +0, v]</c> Raises a formatted error message with a single conversion argument (luaL_error).</summary>
        /// <remarks>WARNING: longjmps out (never returns normally).</remarks>
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
        }
        /// <summary><c>[-1, +(2|0), v]</c> Pops a key and pushes the next key-value pair (lua_next).</summary>
        /// <remarks>Traverse order is unspecified. Returns 0 at the end of the table (pushes
        /// nothing). The key stays on the stack between calls - do not use tolstring on it
        /// (in-place number conversion corrupts the traversal).</remarks>
        public static int next(this State L, int idx) => LuaImport.lua_next(Ptr(L), idx);
        /// <summary><c>[-n, +1, e]</c> Concatenates the n top values and pushes the result (lua_concat).</summary>
        public static void concat(this State L, int n) => LuaImport.lua_concat(Ptr(L), n);
        /// <summary><c>[-0, +1, e]</c> Pushes the length of the value at idx, honouring __length (lua_len).</summary>
        public static void pushlen(this State L, int idx) => LuaImport.lua_len(Ptr(L), idx);
        /// <summary><c>[-0, +0, e]</c> Returns the length of the value at idx, honouring __length (luaL_len).</summary>
        /// <remarks>WARNING: longjmps on error (calls the __length metamethod).</remarks>
        public static long len(this State L, int idx) => LuaImport.luaL_len(Ptr(L), idx);
        /// <summary><c>[-0, +0, -]</c> Converts the number at idx to its canonical string form into buff (lua_numbertocstring).</summary>
        /// <remarks>buff must hold N2SBUFFSZ (64) bytes - Buffer64 matches exactly. Returns the
        /// number of flags used to represent the number (0 if not a number). The same
        /// stack-local bounce as getstack is applied on compacting-GC hosts (single-shot
        /// call, no self-referencing fields - the bounce is safe).</remarks>
        public static uint numbertocstring(this State L, int idx, ref Buffer64 buff)
        {
            if (RuntimeInfo.IsUnity)
                return LuaImport.lua_numbertocstring(Ptr(L), idx, Ptr(ref buff));
            Buffer64 local = buff;
            uint r = LuaImport.lua_numbertocstring(Ptr(L), idx, Ptr(ref local));
            buff = local;
            return r;
        }
        /// <summary><c>[-0, +1, -]</c> Converts the string s to a number and pushes it; returns its length or 0 (lua_stringtonumber).</summary>
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
        /// <summary><c>[-0, +0, -]</c> Returns the allocation function of the state; ud receives its user data (lua_getallocf).</summary>
        public static Alloc getallocf(this State L, out IntPtr ud)
        {
            IntPtr lud = default;
            Alloc result = ToDel<Alloc>(LuaImport.lua_getallocf(Ptr(L), RawRef.Of(ref lud)));
            ud = lud;
            return result;
        }
        /// <summary><c>[-0, +0, -]</c> Replaces the allocation function of the state (lua_setallocf).</summary>
        public static void setallocf(this State L, Alloc f, IntPtr ud) => LuaImport.lua_setallocf(Ptr(L), Keep(f), ud);
        /// <summary><c>[-0, +0, v]</c> Marks the value at idx to be closed when leaving the current block (lua_toclose).</summary>
        /// <remarks>WARNING: longjmps on a bad argument (index must be at most -2 or a special
        /// index, and already-marked slots raise an error).</remarks>
        public static void toclose(this State L, int idx) => LuaImport.lua_toclose(Ptr(L), idx);
        /// <summary><c>[-0, +0, e]</c> Closes the to-be-closed slot at idx (running its __close) (lua_closeslot).</summary>
        public static void closeslot(this State L, int idx) => LuaImport.lua_closeslot(Ptr(L), idx);

        #endregion

        #region debug API

        /// <summary><c>[-0, +0, -]</c> Fills ar with debug info for the function at the given level (lua_getstack).</summary>
        /// <remarks>Returns 0 when the level is too deep. The following NOTE applies to every
        /// 'ref Debug' function in this region: on Unity the referent's address never moves
        /// (Boehm GC does not compact), so the ref address is passed directly. On compacting-GC
        /// hosts (CoreCLR / standalone Mono) a heap-field referent could move during the call,
        /// so the struct is bounced through a stack local and copied back afterwards (the
        /// managed byref is GC-tracked and already points at the new location when the
        /// copy-back runs).</remarks>
        public static int getstack(this State L, int level, ref Debug ar)
        {
            if (RuntimeInfo.IsUnity)
                return LuaImport.lua_getstack(Ptr(L), level, Ptr(ref ar));
            Debug local = ar;
            int r = LuaImport.lua_getstack(Ptr(L), level, Ptr(ref local));
            ar = local;
            return r;
        }
        /// <summary><c>[-(0|1), +(0|1|2), m]</c> Fills the Debug struct and/or pushes results per the what string (lua_getinfo).</summary>
        /// <remarks>Same stack-local bounce as getstack on compacting-GC hosts.</remarks>
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
        /// <summary><c>[-0, +(0|1), -]</c> Pushes the n-th local variable of the frame and returns its name (lua_getlocal).</summary>
        /// <remarks>Returns an empty CStringRef when the index is out of range. The returned name
        /// lives in the function's debug info - valid while the function exists. Same
        /// stack-local bounce as getstack on compacting-GC hosts.</remarks>
        public static CStringRef getlocal(this State L, in Debug ar, int n)
        {
            if (RuntimeInfo.IsUnity)
                return new CStringRef(LuaImport.lua_getlocal(Ptr(L), Ptr(ref Ref.Unprotect(in ar)), n));
            Debug local = ar;
            IntPtr r = LuaImport.lua_getlocal(Ptr(L), Ptr(ref local), n);
            return new CStringRef(r);
        }
        /// <summary><c>[-(0|1), +0, -]</c> Sets the n-th local of the frame from the top value; returns its name (lua_setlocal).</summary>
        /// <remarks>Same stack-local bounce as getstack on compacting-GC hosts.</remarks>
        public static CStringRef setlocal(this State L, in Debug ar, int n)
        {
            if (RuntimeInfo.IsUnity)
                return new CStringRef(LuaImport.lua_setlocal(Ptr(L), Ptr(ref Ref.Unprotect(in ar)), n));
            Debug local = ar;
            IntPtr r = LuaImport.lua_setlocal(Ptr(L), Ptr(ref local), n);
            return new CStringRef(r);
        }
        /// <summary><c>[-0, +(0|1), -]</c> Pushes the n-th upvalue of the function at funcindex and returns its name (lua_getupvalue).</summary>
        public static CStringRef getupvalue(this State L, int funcindex, int n) => new CStringRef(LuaImport.lua_getupvalue(Ptr(L), funcindex, n));
        /// <summary><c>[-(0|1), +0, -]</c> Sets the n-th upvalue from the top value; returns its name (lua_setupvalue).</summary>
        public static CStringRef setupvalue(this State L, int funcindex, int n) => new CStringRef(LuaImport.lua_setupvalue(Ptr(L), funcindex, n));
        /// <summary><c>[-0, +0, -]</c> Returns a unique identifier for the n-th upvalue (lua_upvalueid).</summary>
        public static IntPtr upvalueid(this State L, int fidx, int n) => LuaImport.lua_upvalueid(Ptr(L), fidx, n);
        /// <summary><c>[-0, +0, -]</c> Makes the n1-th upvalue of fidx1 share its cell with the n2-th of fidx2 (lua_upvaluejoin).</summary>
        public static void upvaluejoin(this State L, int fidx1, int n1, int fidx2, int n2) => LuaImport.lua_upvaluejoin(Ptr(L), fidx1, n1, fidx2, n2);
        /// <summary><c>[-0, +0, -]</c> Sets the debug hook (lua_sethook).</summary>
        public static void sethook(this State L, Hook func, int mask, int count) => LuaImport.lua_sethook(Ptr(L), Keep(func), mask, count);
        /// <summary><c>[-0, +0, -]</c> Returns the current hook as a delegate (lua_gethook).</summary>
        public static Hook gethook(this State L) => ToDel<Hook>(LuaImport.lua_gethook(Ptr(L)));
        /// <summary><c>[-0, +0, -]</c> Returns the current hook mask (lua_gethookmask).</summary>
        public static int gethookmask(this State L) => LuaImport.lua_gethookmask(Ptr(L));
        /// <summary><c>[-0, +0, -]</c> Returns the current hook count (lua_gethookcount).</summary>
        public static int gethookcount(this State L) => LuaImport.lua_gethookcount(Ptr(L));

        #endregion

        #region auxiliary functions

        /// <summary><c>[-0, +0, v]</c> Checks that the core matches ver/sz; raises an error otherwise (luaL_checkversion_).</summary>
        /// <remarks>WARNING: longjmps on version mismatch. Prefer the checkversion() macro form.</remarks>
        public static void checkversion_(this State L, double ver, UIntPtr sz) => LuaImport.luaL_checkversion_(Ptr(L), ver, sz);
        /// <summary><c>[-0, +(0|1), m]</c> Pushes the field e of the metatable of obj if it exists (luaL_getmetafield).</summary>
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
        /// <summary><c>[-0, +(0|1), e]</c> Calls the metamethod e of the value at obj if present (luaL_callmeta).</summary>
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
        /// <summary><c>[-0, +0, v]</c> Raises a bad-argument error for argument arg with an extra message (luaL_argerror).</summary>
        /// <remarks>WARNING: longjmps (never returns normally).</remarks>
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
        }
        /// <summary><c>[-0, +0, v]</c> Raises a bad-argument error announcing the expected type tname (luaL_typeerror).</summary>
        /// <remarks>WARNING: longjmps (never returns normally).</remarks>
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
        }
        /// <summary><c>[-0, +0, v]</c> Checks the argument is a string/number and returns it as a C string (luaL_checklstring).</summary>
        /// <remarks>WARNING: longjmps on a bad argument. The returned pointer is valid while the
        /// value stays on the stack.</remarks>
        public static CStringRef checklstring(this State L, int arg)
        {
            IntPtr l = default;
            var result = LuaImport.luaL_checklstring(Ptr(L), arg, RawRef.Of(ref l));
            return new CStringRef(result, (int)l + 1, (int)l);
        }
        /// <summary><c>[-0, +0, v]</c> optlstring: def when the argument is absent/nil, otherwise checklstring (luaL_optlstring).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
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
        /// <summary><c>[-0, +0, v]</c> Checks the argument is a number and returns it as a double (luaL_checknumber).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static double checknumber(this State L, int arg) => LuaImport.luaL_checknumber(Ptr(L), arg);
        /// <summary><c>[-0, +0, v]</c> optnumber: def when the argument is absent/nil, otherwise checknumber (luaL_optnumber).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static double optnumber(this State L, int arg, double def) => LuaImport.luaL_optnumber(Ptr(L), arg, def);
        /// <summary><c>[-0, +0, v]</c> Checks the argument is an integer/number and returns it as a long (luaL_checkinteger).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static long checkinteger(this State L, int arg) => LuaImport.luaL_checkinteger(Ptr(L), arg);
        /// <summary><c>[-0, +0, v]</c> optinteger: def when the argument is absent/nil, otherwise checkinteger (luaL_optinteger).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static long optinteger(this State L, int arg, long def) => LuaImport.luaL_optinteger(Ptr(L), arg, def);
        /// <summary><c>[-0, +0, v]</c> Checks the argument has type t (luaL_checktype).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static void checktype(this State L, int arg, int t) => LuaImport.luaL_checktype(Ptr(L), arg, t);
        /// <summary><c>[-0, +0, v]</c> Checks the argument has any value (not absent, not nil) (luaL_checkany).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static void checkany(this State L, int arg) => LuaImport.luaL_checkany(Ptr(L), arg);
        /// <summary><c>[-0, +1, m]</c> Creates/looks up the metatable tname in the registry (luaL_newmetatable).</summary>
        /// <remarks>Returns true when created and registered, false when it already existed
        /// (the previous value is left on top in that case).</remarks>
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
        /// <summary><c>[-0, +0, m]</c> Returns the userdata at ud if it has metatable tname, otherwise null (luaL_testudata).</summary>
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
        /// <summary><c>[-0, +0, v]</c> Checks the userdata at ud has metatable tname and returns it (luaL_checkudata).</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
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
        /// <summary><c>[-0, +1, m]</c> Pushes a "where" string for the current call level lvl (luaL_where).</summary>
        public static void where(this State L, int lvl) => LuaImport.luaL_where(Ptr(L), lvl);
        /// <summary><c>[-0, +0, v]</c> Maps a string argument to its index in lst (with optional default) (luaL_checkoption).</summary>
        /// <remarks>WARNING: longjmps on a bad argument (invalid option).</remarks>
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
        }
        /// <summary><c>[-0, +(1|3), m]</c> Turns a C library call result into a Lua return tuple (nil+err / true / value) (luaL_fileresult).</summary>
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
        /// <summary><c>[-0, +3, m]</c> Turns a C system-call result into a Lua return tuple (luaL_execresult).</summary>
        public static int execresult(this State L, int stat) => LuaImport.luaL_execresult(Ptr(L), stat);
        /// <summary><c>[-0, +0, m]</c> The default allocator export (realloc-based, no state involved) (luaL_alloc).</summary>
        public static IntPtr alloc(IntPtr ud, IntPtr ptr, UIntPtr osize, UIntPtr nsize) => LuaImport.luaL_alloc(ud, ptr, osize, nsize);
        /// <summary><c>[-1, +0, m]</c> Stores the popped value in table t and returns its reference number (luaL_ref).</summary>
        /// <remarks>Named 'refer' because 'ref' is a C# keyword. Returns NOREF when storing nil.</remarks>
        public static int refer(this State L, int t) => LuaImport.luaL_ref(Ptr(L), t);
        /// <summary><c>[-0, +0, -]</c> Releases reference from table t (luaL_unref).</summary>
        public static void unref(this State L, int t, int reference) => LuaImport.luaL_unref(Ptr(L), t, reference);
        /// <summary><c>[-0, +1, m]</c> Loads a file as a chunk; errors come back as return codes (luaL_loadfilex).</summary>
        /// <remarks>mode: "b"/"t"/"bt" (null span = "bt").</remarks>
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
        /// <summary><c>[-0, +1, -]</c> Loads a memory buffer as a chunk; errors come back as return codes (luaL_loadbufferx).</summary>
        /// <remarks>mode: "b"/"t"/"bt" (null span = "bt"). name is the chunk name for debug info.</remarks>
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
        /// <summary><c>[-0, +1, -]</c> Loads a string as a chunk; errors come back as return codes (luaL_loadstring).</summary>
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
        /// <summary><c>[-0, +0, -]</c> Returns a seed for lua_newstate derived from the state's addresses (luaL_makeseed).</summary>
        public static uint makeseed(this State L) => LuaImport.luaL_makeseed(Ptr(L));
        /// <summary><c>[-0, +1, m]</c> Pushes a copy of s with every occurrence of p replaced by r (luaL_gsub).</summary>
        /// <remarks>The result is pushed AND returned - the pointer is valid while the value stays on the stack.</remarks>
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
        /// <summary><c>[-nup, +0, m]</c> Registers the Reg list into the table on top, popping nup upvalues (luaL_setfuncs).</summary>
        /// <remarks>The Span may omit the {NULL,NULL} terminator - it is appended automatically.
        /// The span is copied to a non-movable stack/native buffer (terminator included) before
        /// its address is handed to Lua; only under Unity a span that already ends with the
        /// all-zero terminator entry is passed directly (Boehm GC does not move it).</remarks>
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
        /// <summary><c>[-0, +1, e]</c> Pushes the field fname of the value at idx as a table, creating it if needed (luaL_getsubtable).</summary>
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
        /// <summary><c>[-0, +1, m]</c> Pushes a traceback of the thread L1 with the given message (luaL_traceback).</summary>
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
        /// <summary><c>[-0, +1, e]</c> Calls openf to open modname and registers the result per package rules (luaL_requiref).</summary>
        /// <remarks>glb != 0 also sets the module as a global. WARNING: openf is called
        /// unprotected - an error raised inside it longjmps out (hence 'e').</remarks>
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

        /// <summary><c>[-0, +?, -]</c> Initializes the buffer and pushes its placeholder slot (luaL_buffinit).</summary>
        /// <remarks>B must be a stack local (see the region note above).</remarks>
        internal static void buffinit(this State L, ref BufferL B) => LuaImport.luaL_buffinit(Ptr(L), Ptr(ref B));
        /// <summary><c>[-0, +?, -]</c> Initializes the buffer and pushes its placeholder slot (luaL_buffinit).</summary>
        public static void buffinit(this State L, ref Buffer B) => buffinit(L, ref B.Inner);
        /// <summary><c>[-0, +?, -]</c> Initializes the buffer and pushes its placeholder slot (luaL_buffinit).</summary>
        public static void buffinit(this State L, BufferBox B) => buffinit(L, ref B.Inner);
        /// <summary><c>[-?, +?, m]</c> Returns an address with at least sz writable bytes in the buffer (luaL_prepbuffsize).</summary>
        /// <remarks>B must be a stack local. Commit written bytes with addsize. The returned
        /// region moves on any later growth - do not cache it across buffer calls.</remarks>
        internal static CStringRef prepbuffsize(ref BufferL B, UIntPtr sz)
        {
            var addr = LuaImport.luaL_prepbuffsize(Ptr(ref B), sz);
            return new CStringRef(addr, (int)sz, 0);   // len 0: writable region, content not yet written
        }
        /// <summary><c>[-?, +?, m]</c> Returns an address with at least sz writable bytes in the buffer (luaL_prepbuffsize).</summary>
        public static CStringRef prepbuffsize(ref Buffer B, UIntPtr sz) => prepbuffsize(ref B.Inner, sz);
        /// <summary><c>[-?, +?, m]</c> Returns an address with at least sz writable bytes in the buffer (luaL_prepbuffsize).</summary>
        public static CStringRef prepbuffsize(BufferBox B, UIntPtr sz) => prepbuffsize(ref B.Inner, sz);
        /// <summary><c>[-?, +?, m]</c> Adds the string s (with embedded-zero-safe length) to the buffer (luaL_addlstring).</summary>
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
        }
        /// <summary><c>[-?, +?, m]</c> Adds the string s (with embedded-zero-safe length) to the buffer (luaL_addlstring).</summary>
        public static void addlstring(ref Buffer B, CStringSpan s) => addlstring(ref B.Inner, s);
        /// <summary><c>[-?, +?, m]</c> Adds the string s (with embedded-zero-safe length) to the buffer (luaL_addlstring).</summary>
        public static void addlstring(BufferBox B, CStringSpan s) => addlstring(ref B.Inner, s);
        /// <summary><c>[-?, +?, m]</c> Adds the NUL-terminated string s to the buffer (luaL_addstring).</summary>
        internal static void addstring(ref BufferL B, CStringSpan s) => addlstring(ref B, s);
        /// <summary><c>[-?, +?, m]</c> Adds the NUL-terminated string s to the buffer (luaL_addstring).</summary>
        public static void addstring(ref Buffer B, CStringSpan s) => addstring(ref B.Inner, s);
        /// <summary><c>[-?, +?, m]</c> Adds the NUL-terminated string s to the buffer (luaL_addstring).</summary>
        public static void addstring(BufferBox B, CStringSpan s) => addstring(ref B.Inner, s);
        /// <summary><c>[-?, +?, m]</c> Adds the value on top of the stack to the buffer and pops it (luaL_addvalue).</summary>
        /// <remarks>The only buffer call allowed with an extra element above the placeholder/box slot.</remarks>
        internal static void addvalue(ref BufferL B) => LuaImport.luaL_addvalue(Ptr(ref B));
        /// <summary><c>[-?, +?, m]</c> Adds the value on top of the stack to the buffer and pops it (luaL_addvalue).</summary>
        public static void addvalue(ref Buffer B) => addvalue(ref B.Inner);
        /// <summary><c>[-?, +?, m]</c> Adds the value on top of the stack to the buffer and pops it (luaL_addvalue).</summary>
        public static void addvalue(BufferBox B) => addvalue(ref B.Inner);
        /// <summary><c>[-?, +1, m]</c> Finishes the buffer, leaving the final string on the stack (luaL_pushresult).</summary>
        internal static void pushresult(ref BufferL B) => LuaImport.luaL_pushresult(Ptr(ref B));
        /// <summary><c>[-?, +1, m]</c> Finishes the buffer, leaving the final string on the stack (luaL_pushresult).</summary>
        public static void pushresult(ref Buffer B) => pushresult(ref B.Inner);
        /// <summary><c>[-?, +1, m]</c> Finishes the buffer, leaving the final string on the stack (luaL_pushresult).</summary>
        public static void pushresult(BufferBox B) => pushresult(ref B.Inner);
        /// <summary><c>[-?, +1, m]</c> Adds sz already-written bytes and finishes the buffer (luaL_pushresultsize).</summary>
        internal static void pushresultsize(ref BufferL B, UIntPtr sz) => LuaImport.luaL_pushresultsize(Ptr(ref B), sz);
        /// <summary><c>[-?, +1, m]</c> Adds sz already-written bytes and finishes the buffer (luaL_pushresultsize).</summary>
        public static void pushresultsize(ref Buffer B, UIntPtr sz) => pushresultsize(ref B.Inner, sz);
        /// <summary><c>[-?, +1, m]</c> Adds sz already-written bytes and finishes the buffer (luaL_pushresultsize).</summary>
        public static void pushresultsize(BufferBox B, UIntPtr sz) => pushresultsize(ref B.Inner, sz);
        /// <summary><c>[-?, +?, m]</c> buffinit + prepbuffsize: initializes and preallocates sz writable bytes (luaL_buffinitsize).</summary>
        internal static CStringRef buffinitsize(this State L, ref BufferL B, UIntPtr sz)
        {
            var addr = LuaImport.luaL_buffinitsize(Ptr(L), Ptr(ref B), sz);
            return new CStringRef(addr, (int)sz, 0);   // len 0: writable region, content not yet written
        }
        /// <summary><c>[-?, +?, m]</c> buffinit + prepbuffsize: initializes and preallocates sz writable bytes (luaL_buffinitsize).</summary>
        public static CStringRef buffinitsize(this State L, ref Buffer B, UIntPtr sz) => buffinitsize(L, ref B.Inner, sz);
        /// <summary><c>[-?, +?, m]</c> buffinit + prepbuffsize: initializes and preallocates sz writable bytes (luaL_buffinitsize).</summary>
        public static CStringRef buffinitsize(this State L, BufferBox B, UIntPtr sz) => buffinitsize(L, ref B.Inner, sz);
        /// <summary><c>[-?, +?, m]</c> Adds a copy of s with every occurrence of p replaced by r (luaL_addgsub).</summary>
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
        }
        /// <summary><c>[-?, +?, m]</c> Adds a copy of s with every occurrence of p replaced by r (luaL_addgsub).</summary>
        public static void addgsub(ref Buffer B, CStringSpan s, CStringSpan p, CStringSpan r) => addgsub(ref B.Inner, s, p, r);
        /// <summary><c>[-?, +?, m]</c> Adds a copy of s with every occurrence of p replaced by r (luaL_addgsub).</summary>
        public static void addgsub(BufferBox B, CStringSpan s, CStringSpan p, CStringSpan r) => addgsub(ref B.Inner, s, p, r);

        #endregion

        #region standard libraries

        /// <summary><c>[-0, +1, e]</c> Opens the base library, pushing it onto the stack (luaopen_base; stack effect inferred - the manual has no entry).</summary>
        public static int open_base(this State L) => LuaImport.luaopen_base(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the package library (luaopen_package; inferred).</summary>
        public static int open_package(this State L) => LuaImport.luaopen_package(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the coroutine library (luaopen_coroutine; inferred).</summary>
        public static int open_coroutine(this State L) => LuaImport.luaopen_coroutine(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the debug library (luaopen_debug; inferred).</summary>
        public static int open_debug(this State L) => LuaImport.luaopen_debug(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the io library (luaopen_io; inferred).</summary>
        public static int open_io(this State L) => LuaImport.luaopen_io(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the math library (luaopen_math; inferred).</summary>
        public static int open_math(this State L) => LuaImport.luaopen_math(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the os library (luaopen_os; inferred).</summary>
        public static int open_os(this State L) => LuaImport.luaopen_os(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the string library (luaopen_string; inferred).</summary>
        public static int open_string(this State L) => LuaImport.luaopen_string(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the table library (luaopen_table; inferred).</summary>
        public static int open_table(this State L) => LuaImport.luaopen_table(Ptr(L));
        /// <summary><c>[-0, +1, e]</c> Opens the utf8 library (luaopen_utf8; inferred).</summary>
        public static int open_utf8(this State L) => LuaImport.luaopen_utf8(Ptr(L));
        /// <summary><c>[-0, +0, e]</c> Opens the libraries selected by the load/preload bit masks (luaL_openselectedlibs).</summary>
        /// <remarks>Lua 5.5: luaL_openlibs(L) is openselectedlibs(L, ~0, 0). Masks: GLIBK..UTF8LIBK.</remarks>
        public static void openselectedlibs(this State L, int load, int preload) => LuaImport.luaL_openselectedlibs(Ptr(L), load, preload);

        #endregion

        #region convenience macros (lua.h)

        /// <summary><c>[-(nargs+1), +nresults, e]</c> lua_call: calls a function without continuation (lua_callk with k=NULL).</summary>
        /// <remarks>WARNING: longjmps out on error (never returns normally).</remarks>
        public static void call(this State L, int nargs, int nresults) => LuaImport.lua_callk(Ptr(L), nargs, nresults, IntPtr.Zero, IntPtr.Zero);

        /// <summary><c>[-(nargs+1), +(nresults|1), -]</c> lua_pcall: protected call; errors come back as return codes (lua_pcallk with k=NULL).</summary>
        public static int pcall(this State L, int nargs, int nresults, int errfunc) => LuaImport.lua_pcallk(Ptr(L), nargs, nresults, errfunc, IntPtr.Zero, IntPtr.Zero);

        /// <summary><c>[-?, +?, v]</c> lua_yield: yields from a C function without continuation (lua_yieldk with k=NULL).</summary>
        /// <remarks>WARNING: longjmps if the thread is not yieldable.</remarks>
        public static int yield(this State L, int nresults) => LuaImport.lua_yieldk(Ptr(L), nresults, IntPtr.Zero, IntPtr.Zero);

        /// <summary><c>[-0, +0, -]</c> lua_tonumber: converts without the isnum flag (lua_tonumberx with isnum=NULL).</summary>
        public static double tonumber(this State L, int idx) => LuaImport.lua_tonumberx(Ptr(L), idx, IntPtr.Zero);
        /// <summary><c>[-0, +0, -]</c> lua_tointeger: converts without the isnum flag (lua_tointegerx with isnum=NULL).</summary>
        public static long tointeger(this State L, int idx) => LuaImport.lua_tointegerx(Ptr(L), idx, IntPtr.Zero);

        /// <summary><c>[-n, +0, e]</c> lua_pop: removes n elements from the top (lua_settop with -(n)-1).</summary>
        /// <remarks>'e': lowering the top may trigger __close of popped to-be-closed variables.</remarks>
        public static void pop(this State L, int n) => LuaImport.lua_settop(Ptr(L), -n - 1);

        /// <summary><c>[-0, +1, m]</c> lua_newtable: creates an empty table (lua_createtable with 0,0).</summary>
        public static void newtable(this State L) => LuaImport.lua_createtable(Ptr(L), 0, 0);

        /// <summary><c>[-0, +1, -]</c> lua_pushcfunction: pushes a C function with no upvalues (lua_pushcclosure with n=0).</summary>
        public static void pushcfunction(this State L, CFunction fn) => LuaImport.lua_pushcclosure(Ptr(L), Keep(fn), 0);

        /// <summary><c>[-0, +0, e]</c> lua_register: pushcfunction + setglobal - registers f as global name.</summary>
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

        /// <summary><c>[-0, +0, -]</c> lua_is* family: type checks against the lua_type result (bool forms).</summary>
        public static bool isfunction(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TFUNCTION;
        /// <summary><c>[-0, +0, -]</c> is*: table check.</summary>
        public static bool istable(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TTABLE;
        /// <summary><c>[-0, +0, -]</c> is*: light userdata check.</summary>
        public static bool islightuserdata(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TLIGHTUSERDATA;
        /// <summary><c>[-0, +0, -]</c> is*: nil check.</summary>
        public static bool isnil(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TNIL;
        /// <summary><c>[-0, +0, -]</c> is*: boolean check.</summary>
        public static bool isboolean(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TBOOLEAN;
        /// <summary><c>[-0, +0, -]</c> is*: thread check.</summary>
        public static bool isthread(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TTHREAD;
        /// <summary><c>[-0, +0, -]</c> is*: absent-value check (bad index).</summary>
        public static bool isnone(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) == TNONE;
        /// <summary><c>[-0, +0, -]</c> is*: absent or nil check.</summary>
        public static bool isnoneornil(this State L, int idx) => LuaImport.lua_type(Ptr(L), idx) <= 0;

        /// <summary><c>[-0, +1, -]</c> lua_pushglobaltable: pushes the global environment table.</summary>
        public static void pushglobaltable(this State L) => LuaImport.lua_rawgeti(Ptr(L), REGISTRYINDEX, RIDX_GLOBALS);

        /// <summary><c>[-0, +0, m]</c> lua_tostring: tolstring without the length out-param.</summary>
        /// <remarks>Inherits tolstring's in-place number-conversion caveat.</remarks>
        public static CStringRef tostring(this State L, int idx)
        {
            return new CStringRef(LuaImport.lua_tolstring(Ptr(L), idx, IntPtr.Zero));
        }

        /// <summary><c>[-1, +1, -]</c> lua_insert: moves the top element into idx, shifting up (lua_rotate by 1).</summary>
        public static void insert(this State L, int idx) => LuaImport.lua_rotate(Ptr(L), idx, 1);

        /// <summary><c>[-1, +0, -]</c> lua_remove: removes the element at idx (rotate -1 + pop).</summary>
        public static void remove(this State L, int idx)
        {
            LuaImport.lua_rotate(Ptr(L), idx, -1);
            pop(L, 1);
        }

        /// <summary><c>[-1, +0, -]</c> lua_replace: pops the top and sets it at idx (copy + pop).</summary>
        public static void replace(this State L, int idx)
        {
            LuaImport.lua_copy(Ptr(L), -1, idx);
            pop(L, 1);
        }

        /// <summary><c>[-0, +1, m]</c> lua_newuserdata: allocates a userdata with one user value (newuserdatauv with nuvalue=1).</summary>
        public static IntPtr newuserdata(this State L, UIntPtr sz) => LuaImport.lua_newuserdatauv(Ptr(L), sz, 1);

        /// <summary><c>[-0, +1, -]</c> lua_getuservalue: pushes the (single) user value (getiuservalue with n=1).</summary>
        public static int getuservalue(this State L, int idx) => LuaImport.lua_getiuservalue(Ptr(L), idx, 1);
        /// <summary><c>[-1, +0, -]</c> lua_setuservalue: sets the (single) user value from the top (setiuservalue with n=1).</summary>
        public static int setuservalue(this State L, int idx) => LuaImport.lua_setiuservalue(Ptr(L), idx, 1);

        /// <summary><c>[-0, +?, -]</c> lua_resetthread: resets a thread (closethread with from=NULL).</summary>
        public static int resetthread(this State L) => LuaImport.lua_closethread(Ptr(L), IntPtr.Zero);

        /// <summary><c>[-0, +0, -]</c> lua_upvalueindex: pseudo-index of the i-th upvalue (REGISTRYINDEX - i).</summary>
        public static int upvalueindex(int i) => REGISTRYINDEX - i;

        /// <summary><c>[-0, +0, -]</c> lua_getextraspace: the EXTRASPACE-sized scratch area before the state pointer.</summary>
        public static IntPtr getextraspace(this State L) => (IntPtr)(Ptr(L).ToInt64() - EXTRASPACE);

        #endregion

        #region convenience macros (lauxlib.h)

        /// <summary><c>[-0, +0, v]</c> luaL_checkversion: checks the core matches this binding (VERSION_NUM + NUMSIZES).</summary>
        /// <remarks>WARNING: longjmps on mismatch.</remarks>
        public static void checkversion(this State L) => LuaImport.luaL_checkversion_(Ptr(L), VERSION_NUM, (UIntPtr)NUMSIZES);

        /// <summary><c>[-0, +1, m]</c> luaL_loadfile: loads a file in text mode (loadfilex with mode=NULL).</summary>
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

        /// <summary><c>[-0, +0, v]</c> luaL_checkstring: checklstring without the length out-param.</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static CStringRef checkstring(this State L, int n)
        {
            return new CStringRef(LuaImport.luaL_checklstring(Ptr(L), n, IntPtr.Zero));
        }
        /// <summary><c>[-0, +0, v]</c> luaL_optstring: optlstring without the length out-param.</summary>
        public static CStringSpan optstring(this State L, int n, CStringSpan def) => optlstring(L, n, def);

        /// <summary><c>[-0, +0, -]</c> luaL_typename: the type name of the value at idx (lua_typename of lua_type).</summary>
        public static CStringRef typenameL(this State L, int idx) => new CStringRef(LuaImport.lua_typename(Ptr(L), LuaImport.lua_type(Ptr(L), idx)));

        /// <summary><c>[-0, +?, m]</c> luaL_dofile: loads and runs a file (loadfile || pcall(0, MULTRET, 0)).</summary>
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

        /// <summary><c>[-0, +?, -]</c> luaL_dostring: loads and runs a string (loadstring || pcall(0, MULTRET, 0)).</summary>
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

        /// <summary><c>[-0, +1, m]</c> luaL_getmetatable: pushes the named metatable from the registry (getfield(REGISTRYINDEX, n)).</summary>
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
        }

        /// <summary><c>[-0, +1, -]</c> luaL_loadbuffer: loads a buffer in text mode (loadbufferx with mode=NULL).</summary>
        public static int loadbuffer(this State L, CStringSpan s, CStringSpan name) => loadbufferx(L, s, name, default);

        /// <summary><c>[-0, +1, -]</c> luaL_pushfail: pushes fail (nil under the default configuration - no LUA_FAILISFALSE).</summary>
        public static void pushfail(this State L) => LuaImport.lua_pushnil(Ptr(L));

        /// <summary><c>[-0, +0, -]</c> luaL_bufflen: the number of bytes currently in the buffer.</summary>
        internal static UIntPtr bufflen(ref BufferL B) => B.n;
        /// <summary><c>[-0, +0, -]</c> luaL_bufflen: the number of bytes currently in the buffer.</summary>
        public static UIntPtr bufflen(ref Buffer B) => B.n;
        /// <summary><c>[-0, +0, -]</c> luaL_bufflen: the number of bytes currently in the buffer.</summary>
        public static UIntPtr bufflen(BufferBox B) => B.n;
        /// <summary><c>[-0, +0, -]</c> luaL_buffaddr: the address of the current buffer content (moves on growth).</summary>
        internal static IntPtr buffaddr(ref BufferL B) => B.b;
        /// <summary><c>[-0, +0, -]</c> luaL_buffaddr: the address of the current buffer content (moves on growth).</summary>
        public static IntPtr buffaddr(ref Buffer B) => B.b;
        /// <summary><c>[-0, +0, -]</c> luaL_buffaddr: the address of the current buffer content (moves on growth).</summary>
        public static IntPtr buffaddr(BufferBox B) => B.b;

        /// <summary><c>[-?, +?, m]</c> luaL_addchar: grows the buffer by 1 if full, writes the byte, advances n.</summary>
        internal static void addchar(ref BufferL B, byte c)
        {
            if (B.n.ToUInt64() >= B.size.ToUInt64()) LuaImport.luaL_prepbuffsize(Ptr(ref B), (UIntPtr)1);
            Marshal.WriteByte(B.b, (int)B.n, c);
            B.n = (UIntPtr)(B.n.ToUInt64() + 1);
        }
        /// <summary><c>[-?, +?, m]</c> luaL_addchar: grows the buffer by 1 if full, writes the byte, advances n.</summary>
        public static void addchar(ref Buffer B, byte c) => addchar(ref B.Inner, c);
        /// <summary><c>[-?, +?, m]</c> luaL_addchar: grows the buffer by 1 if full, writes the byte, advances n.</summary>
        public static void addchar(BufferBox B, byte c) => addchar(ref B.Inner, c);

        /// <summary><c>[-?, +?, -]</c> luaL_addsize: advances n by s after bytes were written into the prep area.</summary>
        internal static void addsize(ref BufferL B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        /// <summary><c>[-?, +?, -]</c> luaL_addsize: advances n by s after bytes were written into the prep area.</summary>
        public static void addsize(ref Buffer B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        /// <summary><c>[-?, +?, -]</c> luaL_addsize: advances n by s after bytes were written into the prep area.</summary>
        public static void addsize(BufferBox B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() + s.ToUInt64());
        /// <summary><c>[-?, +?, -]</c> luaL_buffsub: rewinds n by s bytes (the buffer must have at least that many).</summary>
        internal static void buffsub(ref BufferL B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());
        /// <summary><c>[-?, +?, -]</c> luaL_buffsub: rewinds n by s bytes (the buffer must have at least that many).</summary>
        public static void buffsub(ref Buffer B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());
        /// <summary><c>[-?, +?, -]</c> luaL_buffsub: rewinds n by s bytes (the buffer must have at least that many).</summary>
        public static void buffsub(BufferBox B, UIntPtr s) => B.n = (UIntPtr)(B.n.ToUInt64() - s.ToUInt64());

        /// <summary><c>[-?, +?, m]</c> luaL_prepbuffer: prepbuffsize with LUAL_BUFFERSIZE.</summary>
        internal static CStringRef prepbuffer(ref BufferL B) => prepbuffsize(ref B, BUFFERSIZE);
        /// <summary><c>[-?, +?, m]</c> luaL_prepbuffer: prepbuffsize with LUAL_BUFFERSIZE.</summary>
        public static CStringRef prepbuffer(ref Buffer B) => prepbuffer(ref B.Inner);
        /// <summary><c>[-?, +?, m]</c> luaL_prepbuffer: prepbuffsize with LUAL_BUFFERSIZE.</summary>
        public static CStringRef prepbuffer(BufferBox B) => prepbuffer(ref B.Inner);

        /// <summary><c>[-0, +0, v]</c> luaL_argcheck: raises a bad-argument error for arg unless cond holds.</summary>
        /// <remarks>WARNING: longjmps (never returns) when cond is false.</remarks>
        public static void argcheck(this State L, bool cond, int arg, CStringSpan extramsg)
        {
            if (!cond) argerror(L, arg, extramsg);
        }
        /// <summary><c>[-0, +0, v]</c> luaL_argexpected: raises a type error for arg unless cond holds.</summary>
        /// <remarks>WARNING: longjmps (never returns) when cond is false.</remarks>
        public static void argexpected(this State L, bool cond, int arg, CStringSpan tname)
        {
            if (!cond) typeerror(L, arg, tname);
        }

        /// <summary><c>[-0, +0, v]</c> luaL_checkint: checkinteger narrowed to int.</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static int checkint(this State L, int n) => (int)LuaImport.luaL_checkinteger(Ptr(L), n);
        /// <summary><c>[-0, +0, v]</c> luaL_optint: optinteger narrowed to int.</summary>
        public static int optint(this State L, int n, int def) => (int)LuaImport.luaL_optinteger(Ptr(L), n, def);
        /// <summary><c>[-0, +0, v]</c> luaL_checkunsigned: checkinteger reinterpreted as unsigned.</summary>
        /// <remarks>WARNING: longjmps on a bad argument.</remarks>
        public static ulong checkunsigned(this State L, int n) => (ulong)LuaImport.luaL_checkinteger(Ptr(L), n);
        /// <summary><c>[-0, +0, v]</c> luaL_optunsigned: optinteger reinterpreted as unsigned.</summary>
        public static ulong optunsigned(this State L, int n, ulong def) => (ulong)LuaImport.luaL_optinteger(Ptr(L), n, (long)def);

        /// <summary><c>[-0, +1, m]</c> luaL_newlibtable: preallocates the library table for the real entries of l.</summary>
        /// <remarks>Same terminator convention as setfuncs: an explicit all-zero last entry is
        /// not counted, anything else is.</remarks>
        public static void newlibtable(this State L, Span<Reg> l)
        {
            int count = l.Length;
            if (count > 0 && l[count - 1].name == IntPtr.Zero && l[count - 1].func == IntPtr.Zero)
                --count;
            LuaImport.lua_createtable(Ptr(L), 0, count);
        }

        /// <summary><c>[-0, +1, m]</c> luaL_newlib: checkversion + newlibtable + setfuncs(l, 0) - registers l into a fresh table on the stack.</summary>
        /// <remarks>WARNING: checkversion longjmps on a version mismatch.</remarks>
        public static void newlib(this State L, Span<Reg> l)
        {
            checkversion(L);
            newlibtable(L, l);
            setfuncs(L, l, 0);
        }

        /// <summary><c>[-0, +0, v]</c> luaL_checklong: checkinteger as long.</summary>
        /// <remarks>WARNING: longjmps on a bad argument. C 'long' is 32-bit on Windows (LLP64),
        /// where the C macro truncates values outside int32 range; this facade returns the
        /// full 64-bit lua_Integer (no truncation).</remarks>
        public static long checklong(this State L, int n) => LuaImport.luaL_checkinteger(Ptr(L), n);

        /// <summary><c>[-0, +0, v]</c> luaL_optlong: optinteger as long. Same LLP64 note as checklong.</summary>
        public static long optlong(this State L, int n, long d) => LuaImport.luaL_optinteger(Ptr(L), n, d);

        /// <summary><c>[-0, +0, -]</c> luaL_opt: d when the argument is absent/nil, otherwise f(L, n).</summary>
        /// <remarks>Generic macro shape: f is any argument checker (checkinteger, checknumber,
        /// checklstring, ...). The Func parameter allocates on non-cached lambdas - use a
        /// static lambda (no captures) to get the compiler-cached delegate.</remarks>
        public static T opt<T>(this State L, Func<State, int, T> f, int n, T d)
            => isnoneornil(L, n) ? d : f(L, n);

        /// <summary><c>[-0, +0, -]</c> luaL_intop family: wrap-around arithmetic on lua_Integer ((lua_Unsigned) casts), the semantics the Lua core relies on.</summary>
        /// <remarks>Shift counts follow the practical C behavior (the shift amount is masked by
        /// the hardware, e.g. to 6 bits on x64).</remarks>
        public static long intopAdd(long v1, long v2) => (long)((ulong)v1 + (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: wrap-around subtraction.</summary>
        public static long intopSub(long v1, long v2) => (long)((ulong)v1 - (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: wrap-around multiplication.</summary>
        public static long intopMul(long v1, long v2) => (long)((ulong)v1 * (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: bitwise and.</summary>
        public static long intopAnd(long v1, long v2) => (long)((ulong)v1 & (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: bitwise or.</summary>
        public static long intopOr(long v1, long v2) => (long)((ulong)v1 | (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: bitwise xor.</summary>
        public static long intopXor(long v1, long v2) => (long)((ulong)v1 ^ (ulong)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: wrap-around shift left.</summary>
        public static long intopShl(long v1, long v2) => (long)((ulong)v1 << (int)v2);
        /// <summary><c>[-0, +0, -]</c> luaL_intop: wrap-around shift right.</summary>
        public static long intopShr(long v1, long v2) => (long)((ulong)v1 >> (int)v2);

        #endregion
    }
}


