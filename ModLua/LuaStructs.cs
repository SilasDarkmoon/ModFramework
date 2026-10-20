using Mod.LowLevel;
using System;
using System.Runtime.InteropServices;

namespace Mod.Lua
{
    public static partial class LuaLib
    {
        /// <summary>
        /// Blittable C# shapes for the structs defined by the Lua 5.5 C API
        /// (lua.h / lauxlib.h). Field order and types mirror the C layouts;
        /// LayoutKind.Sequential reproduces the native offsets on every
        /// pointer size (IntPtr/UIntPtr adapt automatically).
        /// lua_State and CallInfo are opaque and only ever travel as IntPtr.
        /// Fixed-size byte areas (C char[N]) are built from the POT block
        /// structs below instead of unsafe fixed buffers, so the project
        /// compiles with AllowUnsafeBlocks off.
        /// </summary>

        /// <summary>
        /// Power-of-two blittable byte blocks. Pack=1 is essential: a block
        /// used as a field must align like a C char array (1 byte), otherwise
        /// the layout would drift (e.g. lua_Debug.short_src starts at offset
        /// 76, which is not 8-aligned). Inner layout is unaffected by Pack=1
        /// (fields are laid out contiguously anyway).
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer4
        {
            public short F0;
            public short F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer8
        {
            public int F0;
            public int F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer16
        {
            public long F0;
            public long F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer32
        {
            public Buffer16 F0;
            public Buffer16 F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer64
        {
            public Buffer32 F0;
            public Buffer32 F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer128
        {
            public Buffer64 F0;
            public Buffer64 F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer256
        {
            public Buffer128 F0;
            public Buffer128 F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer512
        {
            public Buffer256 F0;
            public Buffer256 F1;
        }

        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        public struct Buffer1024
        {
            public Buffer512 F0;
            public Buffer512 F1;
        }

        /// <summary>
        /// lua_Debug (lua.h): debug information filled by lua_getstack /
        /// lua_getinfo and passed to lua_Hook callbacks (as lua_Debug*).
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Debug
        {
            public int @event;           // event code (LUA_HOOKCALL/RET/LINE/COUNT/TAILCALL)
            public IntPtr name;          // (n) variable name; NULL if not found
            public IntPtr namewhat;      // (n) 'global', 'local', 'field', 'method'
            public IntPtr what;          // (S) 'Lua', 'C', 'main', 'tail'
            public IntPtr source;        // (S) source name
            public UIntPtr srclen;       // (S) length of source
            public int currentline;      // (l) current line
            public int linedefined;      // (S) first line where the function is defined
            public int lastlinedefined;  // (S) last line where the function is defined
            public byte nups;            // (u) number of upvalues
            public byte nparams;         // (u) number of parameters
            public sbyte isvararg;       // (u) whether the function is vararg
            public byte extraargs;       // (t) number of extra arguments
            public sbyte istailcall;     // (t) whether it is a tail call
            public int ftransfer;        // (r) index of first value transferred
            public int ntransfer;        // (r) number of transferred values
            // short_src[LUA_IDSIZE = 60] = 32 + 16 + 8 + 4
            public Buffer32 short_src_32;
            public Buffer16 short_src_16;
            public Buffer8 short_src_8;
            public Buffer4 short_src_4;
            public IntPtr i_ci;          // private: struct CallInfo* (opaque)
        }

        /// <summary>
        /// luaL_Reg (lauxlib.h): registration entry for luaL_setfuncs arrays.
        /// An array is terminated by a { NULL, NULL } entry.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Reg
        {
            public IntPtr name;  // const char* name (NUL-terminated; NULL terminates the array)
            public IntPtr func;  // lua_CFunction func
        }

        /// <summary>
        /// luaL_Buffer (lauxlib.h): generic buffer for the lauxlib buffer API
        /// (luaL_buffinit / luaL_prepbuffsize / luaL_addlstring / luaL_addvalue /
        /// luaL_pushresult / ...). Access the data through the pointer returned
        /// by luaL_prepbuffsize, not through 'init'.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct BufferL
        {
            public IntPtr b;        // char* b - buffer address
            public UIntPtr size;    // size_t size - buffer size
            public UIntPtr n;       // size_t n - number of characters in buffer
            public State L;        // lua_State* L
            // C: union { LUAI_MAXALIGN; char b[LUAL_BUFFERSIZE]; } init;
            // LUAL_BUFFERSIZE = 16 * sizeof(void*) * sizeof(lua_Number):
            // 1024 bytes on 64-bit, 512 on 32-bit. We reserve the maximum;
            // on 32-bit Lua only writes the first 512 bytes, which stays in bounds.
            public Buffer1024 init;
        }
        /// <summary>
        /// Heap-resident holder for a luaL_Buffer. The struct itself lives
        /// in native memory (NativeMapper), so its address never moves on
        /// any host - including compacting-GC runtimes - and the buffer's
        /// placeholder/box contract holds even when this holder is a heap
        /// object or a field of one.
        ///
        /// Lifetime: finish the buffer sequence (buffinit ... pushresult),
        /// or abandon it by letting a Lua error unwind the stack, BEFORE
        /// Dispose (or finalization). Disposing mid-sequence frees the
        /// native struct while the Lua stack still holds a placeholder
        /// pointing at it (a light userdata): Lua never dereferences the
        /// placeholder by itself, so the consequence is one leaked stack
        /// slot until lua_close - not a crash - but the disposed buffer
        /// must not be used again: every accessor throws
        /// ObjectDisposedException after Dispose.
        /// </summary>
        public class BufferBox : IDisposable
        {
            private NativeMapper<BufferL> _Inner;
            internal ref BufferL Inner
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner;
                }
            }
            internal BufferL Value
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return _Inner.Value;
                }
                set
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    _Inner.Value = value;
                }
            }

            public BufferBox()
            {
                _Inner = new NativeMapper<BufferL>();
                _Inner.Init();
            }
            public void Dispose()
            {
                _Inner.Dispose();
                GC.SuppressFinalize(this);
            }
            ~BufferBox()
            {
                _Inner.Dispose();
            }

            // Uniform "disposed means disposed" semantics: every accessor
            // throws ObjectDisposedException after Dispose (no silent
            // empty-ref fallbacks, no silent drops) - the facades all go
            // through Inner, so a use-after-Dispose surfaces as a managed
            // exception with a full stack trace instead of a NULL pointer
            // handed to native code.
            public ref IntPtr b
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner.b;
                }
            }
            public ref UIntPtr size
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner.size;
                }
            }
            public ref UIntPtr n
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner.n;
                }
            }
            public ref State L
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner.L;
                }
            }
            public ref Buffer1024 init
            {
                get
                {
                    if (!_Inner.IsValid) throw new ObjectDisposedException(nameof(BufferBox));
                    return ref _Inner.Inner.init;
                }
            }
        }
        public ref struct Buffer
        {
            private BufferL _Inner;
            internal ref BufferL Inner => ref RawRef.Of(ref _Inner).GetRef<BufferL>();

            public ref IntPtr b => ref RawRef.Of(ref _Inner.b).GetRef<IntPtr>();
            public ref UIntPtr size => ref RawRef.Of(ref _Inner.size).GetRef<UIntPtr>();
            public ref UIntPtr n => ref RawRef.Of(ref _Inner.n).GetRef<UIntPtr>();
            public ref State L => ref RawRef.Of(ref _Inner.L).GetRef<State>();
            public ref Buffer1024 init => ref RawRef.Of(ref _Inner.init).GetRef<Buffer1024>();
        }


        /// <summary>
        /// luaL_Stream (lauxlib.h): initial structure of a file-handle userdata
        /// (metatable LUA_FILEHANDLE = "FILE*"). Derived implementations may
        /// append fields after it.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct Stream
        {
            public IntPtr f;       // FILE* f (NULL for incompletely created streams)
            public IntPtr closef;  // lua_CFunction closef (NULL for closed streams)
        }
    }
}
