using System;
using System.Runtime.InteropServices;

namespace Mod.LowLevel
{
    /// <summary>
    /// Resolves native function pointers from a dynamic library - or, on iOS,
    /// from the statically linked main executable (the __Internal case).
    /// <para>Platform behaviour (selected via <see cref="RuntimeInfo"/>):</para>
    /// <list type="bullet">
    /// <item><b>iOS</b>: <paramref name="dllName"/> is IGNORED - the symbol is
    /// looked up in the main executable through dlsym(RTLD_DEFAULT), which is
    /// exactly what a DllImport("__Internal") binding resolves to.</item>
    /// <item><b>Windows</b>: LoadLibrary + GetProcAddress. A short name
    /// ("lua55") gets the usual .dll suffix probing. NOTE: in Unity
    /// Editor/Player a short name does NOT resolve - see remarks.</item>
    /// <item><b>macOS / Linux / Android</b>: dlopen(RTLD_NOW|RTLD_GLOBAL) +
    /// dlsym on the library handle. A short name is completed to the full
    /// soname ("lua55" -&gt; "liblua55.dylib" / "liblua55.so"); a name that
    /// already contains a dot is passed through unchanged.</item>
    /// </list>
    /// Returns a <see cref="FunctionPointer"/> (IsUnmanaged = true); on any
    /// miss returns default (Pfn == <see cref="IntPtr.Zero"/>, no exception) -
    /// callers decide how to handle the miss.
    /// </summary>
    /// <remarks>
    /// <b>Unity Editor / Player limitation</b> (verified 2026-11-11 on
    /// Windows Editor Mono and a Windows IL2CPP player): a SHORT library
    /// name does NOT resolve there. The process "application directory" is
    /// the Unity editor / player shell, and Unity loads native plugins by
    /// FULL path without adding their folder to the process-wide loader
    /// search path (IL2CPP resolves its own DllImports against a build-time
    /// plugin table, which a raw LoadLibrary/dlopen call cannot see), so a
    /// short name misses plugins under &lt;player&gt;_Data/Plugins. iOS is
    /// the exception (dllName is ignored - RTLD_DEFAULT lookup). In Unity
    /// editors and players, pass the ABSOLUTE plugin path instead (e.g.
    /// "&lt;player&gt;_Data/Plugins/x86_64/lua55.dll", or the plugin's
    /// asset/package path in the editor). This library deliberately
    /// references no UnityEngine types, so it cannot discover Unity plugin
    /// paths itself - the caller resolves the path (e.g. from
    /// Application.dataPath) and passes it in.
    /// <para><b>Calling variadic natives through resolved pointers.</b>
    /// Neither DllImport nor the unmanaged calli can express a C "..." - a
    /// FIXED shape must be declared, and each platform's vararg ABI decides
    /// where va_arg actually reads. Matrix (probed 2026-11-03/05 on
    /// Windows/Linux/Android/Apple devices; preserved from the removed
    /// NativeVariadicFunc matrix, see Probes/VarargProbe*):</para>
    /// <list type="bullet">
    /// <item><b>Windows x64</b>: the variadic callee reads ONLY the GP
    /// (integer) slot stream - the single overflow area is spilled
    /// unconditionally, AL ignored. A double vararg MUST be blitted to its
    /// 64-bit bit pattern and passed in a GP slot: declare the vararg slot
    /// as long/IntPtr and pass BitConverter.DoubleToInt64Bits(d). A
    /// declared real double parameter routes to XMM and is invisible to
    /// the callee.</item>
    /// <item><b>Linux x64</b> (CoreCLR/Mono/Unity): the GP-slot blit works;
    /// a real double parameter also works (unmanaged calli leaves AL != 0,
    /// so the callee spills XMM0 into the va_list save area).</item>
    /// <item><b>Linux / Android ARM64</b>: the va_list has SEPARATE GP and
    /// FP save areas - va_arg(double) reads the FP area and a GP blit is
    /// INVISIBLE there. Declare a real double parameter (it rides v0).</item>
    /// <item><b>Apple ARM64 (iOS/Mac)</b>: varargs are passed ENTIRELY on
    /// the stack - the callee's va_list reads the caller SP, while a plain
    /// call keeps the argument in a register (x2) where va_arg never looks.
    /// PAD the declared shape so the real vararg lands in the FIRST STACK
    /// SLOT: declare 8 fixed parameters (L, fmt, pad x6, arg1) - parameters
    /// 9+ spill to the stack, putting arg1 exactly where va_arg reads it.</item>
    /// <item><b>Mono</b>: 1-in-16 AL hazard for real-double shapes (AL =
    /// pfn low byte) - bind-time check (pfn &amp; 0xFF) or a trampoline.
    /// <b>IL2CPP</b>: AL is caller residue - per-build probe required.</item>
    /// </list>
    /// <para>Vararg slot rules: the slot must be exactly 8 bytes
    /// (double/long/ulong/IntPtr) - float/int are reinterpreted out of
    /// bounds (float is NOT promoted; garbage high bytes) and reference
    /// types silently pass the managed pointer; promote manually
    /// ((double)f, (long)i) before the call. Never pass 128-bit structs,
    /// and a fixed-part struct that could be treated as a vector (same
    /// numeric elements) desynchronizes the GP stream. Under IL2CPP never
    /// route &gt;8B struct args through virtual / virtual-generic wrappers -
    /// the fully shared vtable truncates them to 8 bytes; call the
    /// non-generic Invoke directly.</para>
    /// <para>Working end-to-end example: ModLua's pushfstring/error facade
    /// - LuaLib.fstringConvArg dispatches funccate 1 (GP blit, Windows /
    /// Linux x64) / 2 (real double, Linux) / 3 (Apple padded 8-arg shape)
    /// into the lua_pushfstring overloads in ModLua.PlatDep/LuaImport.cs.</para>
    /// </remarks>
    public static class NativeFuncLoader
    {
        // ---- Windows ---------------------------------------------------------
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr LoadLibraryW(string name);
        [DllImport("kernel32", SetLastError = true, CharSet = CharSet.Ansi)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, string name);

        // ---- Unix ------------------------------------------------------------
        // Apple: dlopen/dlsym are libSystem exports - reference them through
        // the DYNAMIC library name, never through "__Internal": under IL2CPP a
        // __Internal DllImport becomes a LINK-TIME symbol reference, and
        // "dlopen"/"dlsym" do not exist in a Windows main executable (the
        // declaration alone would break Windows IL2CPP builds). A dynamic
        // library name is resolved lazily at runtime on every runtime
        // (CoreCLR/Mono/IL2CPP), so the declarations are inert where unused.
        // iOS note: dlopen("libSystem.dylib") on an already-loaded system
        // library is permitted (it only hits the dyld shared cache - no new
        // code is loaded; dlopen/dlsym are public libSystem APIs, not on the
        // private-API scan list), and dlsym(RTLD_DEFAULT) still searches the
        // main executable's symbols (RTLD_DEFAULT is an argument, not a
        // linking mode) - the static-linking lookup path is unaffected.
        // CAVEAT: the IL2CPP runtime-dlopen path on iOS is reasoned, not yet
        // device-verified; a miss degrades to a default return (no crash).
        // If it ever misses on device, the fallback plan is __Internal plus
        // a csproj-injected conditional (the ModLua.PlatDep #if MODLUA_IOS
        // pattern).
        // Linux glibc: dlopen/dlsym moved into libc in glibc 2.34, and newer
        // distros (Ubuntu 22.04+) no longer ship the "libdl.so" dev symlink -
        // a plain DllImport("libdl") throws DllNotFoundException there. The
        // runtime stub "libdl.so.2" however is shipped by EVERY glibc (the
        // real lib before 2.34, a forwarding stub after). Android/bionic has
        // the opposite layout: only "libdl" exists (no .so.2). musl has
        // neither - everything lives in libc. So bind ALL THREE and pick
        // lazily at first use: the probe call is dlopen(NULL) (yields the
        // main-program handle on success); a missing binding throws before
        // any real work happens, and the losing declarations stay inert on
        // every runtime (same pattern as the Apple declarations above).
        // NOTE: without an explicit EntryPoint the runtime looks up the MANAGED
        // method name as the symbol - these helpers are renamed per platform,
        // so every one of them pins EntryPoint to the real native name.
        [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlopen_mac(string file, int mode);
        [DllImport("libSystem.dylib", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlsym_mac(IntPtr handle, string symbol);
        [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlopen_linux_glibc(string file, int mode);
        [DllImport("libdl.so.2", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlsym_linux_glibc(IntPtr handle, string symbol);
        [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlopen_linux_bionic(string file, int mode);
        [DllImport("libdl", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlsym_linux_bionic(IntPtr handle, string symbol);
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlopen", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlopen_linux_libc(string file, int mode);
        [DllImport("libc", CallingConvention = CallingConvention.Cdecl, EntryPoint = "dlsym", CharSet = CharSet.Ansi)]
        private static extern IntPtr dlsym_linux_libc(IntPtr handle, string symbol);

        // 0 = undecided, 1 = libdl.so.2 (glibc), 2 = libdl (bionic/Android),
        // 3 = libc (musl, or a glibc without either stub - should not happen).
        private static int _linuxDlBinding;
        private static int LinuxDlBinding
        {
            get
            {
                if (_linuxDlBinding == 0)
                {
                    // dlopen(NULL, RTLD_NOW) returns the main-program handle -
                    // a side-effect-free probe that validates the binding.
                    try { dlopen_linux_glibc(null, RTLD_NOW); _linuxDlBinding = 1; }
                    catch (DllNotFoundException)
                    {
                        try { dlopen_linux_bionic(null, RTLD_NOW); _linuxDlBinding = 2; }
                        catch (DllNotFoundException)
                        {
                            dlopen_linux_libc(null, RTLD_NOW); // last resort: let it throw if truly absent
                            _linuxDlBinding = 3;
                        }
                        catch (EntryPointNotFoundException)
                        {
                            dlopen_linux_libc(null, RTLD_NOW);
                            _linuxDlBinding = 3;
                        }
                    }
                    catch (EntryPointNotFoundException)
                    {
                        try { dlopen_linux_bionic(null, RTLD_NOW); _linuxDlBinding = 2; }
                        catch (DllNotFoundException)
                        {
                            dlopen_linux_libc(null, RTLD_NOW);
                            _linuxDlBinding = 3;
                        }
                        catch (EntryPointNotFoundException)
                        {
                            dlopen_linux_libc(null, RTLD_NOW);
                            _linuxDlBinding = 3;
                        }
                    }
                }
                return _linuxDlBinding;
            }
        }

        private const int RTLD_NOW = 2;
        private const int RTLD_GLOBAL_LINUX = 0x100;
        private const int RTLD_GLOBAL_MAC = 0x8;

        // RTLD_DEFAULT: macOS/iOS = (void*)-2, Linux/Android = NULL.
        // iOS MUST be -2 as well: Apple's dlsym(NULL) searches ONLY the main
        // executable (the thin app shell), while the statically linked lua
        // symbols live in UnityFramework.framework - only RTLD_DEFAULT(-2)
        // reaches them (device-verified via the ScenePatch probe 2026-11-05:
        // dlsym(RTLD_DEFAULT) resolves UnityAppController.mm symbols).
        private static IntPtr RtldDefault =>
            (RuntimeInfo.Platform == RuntimeInfo.OSPlatform.Mac || RuntimeInfo.Platform == RuntimeInfo.OSPlatform.iOS)
                ? new IntPtr(-2) : IntPtr.Zero;

        // iOS shares macOS's libSystem (there is no libdl on iOS).
        private static bool IsApple =>
            RuntimeInfo.Platform == RuntimeInfo.OSPlatform.Mac || RuntimeInfo.Platform == RuntimeInfo.OSPlatform.iOS;

        private static IntPtr Dlopen(string file, int mode)
        {
            if (IsApple) return dlopen_mac(file, mode);
            switch (LinuxDlBinding)
            {
                case 2: return dlopen_linux_bionic(file, mode);
                case 3: return dlopen_linux_libc(file, mode);
                default: return dlopen_linux_glibc(file, mode);
            }
        }
        private static IntPtr Dlsym(IntPtr handle, string symbol)
        {
            if (IsApple) return dlsym_mac(handle, symbol);
            switch (LinuxDlBinding)
            {
                case 2: return dlsym_linux_bionic(handle, symbol);
                case 3: return dlsym_linux_libc(handle, symbol);
                default: return dlsym_linux_glibc(handle, symbol);
            }
        }

        /// <summary>
        /// Resolves a native function pointer. See the class remarks for the
        /// per-platform lookup strategy.
        /// </summary>
        /// <returns>A <see cref="FunctionPointer"/> with
        /// <see cref="FunctionPointer.IsUnmanaged"/> = true (the resolved
        /// symbol is native code - feed it straight into
        /// CreateFreeInvokable/PointerFunc). On any miss returns default
        /// (<see cref="FunctionPointer.Pfn"/> == <see cref="IntPtr.Zero"/>,
        /// no exception) - callers decide how to handle the miss.</returns>
        /// <param name="dllName">Library name ("lua55" or a full soname);
        /// ignored on iOS (static linking - always the main executable).
        /// In Unity Editor/Player pass the ABSOLUTE plugin path - short names
        /// do not resolve there (see the class remarks).</param>
        /// <param name="functionName">The native symbol name (C name).</param>
        public static FunctionPointer LoadNativeFunction(string dllName, string functionName)
        {
            if (string.IsNullOrEmpty(functionName)) return default;

            if (RuntimeInfo.Platform == RuntimeInfo.OSPlatform.iOS)
            {
                // Static linking: the symbol lives in the main executable;
                // dlsym(RTLD_DEFAULT) is what __Internal bindings resolve to.
                return Dlsym(RtldDefault, functionName).AsFunctionPointer(true);
            }

            if (RuntimeInfo.Platform == RuntimeInfo.OSPlatform.Windows)
            {
                if (string.IsNullOrEmpty(dllName)) return default;
                IntPtr h = LoadLibraryW(dllName);
                return h == IntPtr.Zero ? default : GetProcAddress(h, functionName).AsFunctionPointer(true);
            }

            // macOS / Linux / Android: open the library, resolve in its handle.
            if (string.IsNullOrEmpty(dllName)) return default;
            IntPtr hUnix = Dlopen(CompleteUnixLibName(dllName),
                RTLD_NOW | (IsApple ? RTLD_GLOBAL_MAC : RTLD_GLOBAL_LINUX));
            return hUnix == IntPtr.Zero ? default : Dlsym(hUnix, functionName).AsFunctionPointer(true);
        }

        // Unix dlopen does no lib-prefix/suffix probing (unlike DllImport):
        // complete "lua55" to the full soname unless one was already given.
        private static string CompleteUnixLibName(string dllName)
        {
            if (dllName.IndexOf('.') >= 0) return dllName;
            return RuntimeInfo.Platform == RuntimeInfo.OSPlatform.Mac
                ? "lib" + dllName + ".dylib"
                : "lib" + dllName + ".so";
        }
    }
}
