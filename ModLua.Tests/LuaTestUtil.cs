using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.Lua;

namespace Mod.Lua.Tests
{
    /// <summary>
    /// 冒烟/互传测试共用 helper 与 LUA_* 常量（宏不属 DllImport 层，测试本地定义）。
    /// </summary>
    internal static class LuaTestUtil
    {
        public const int LUA_OK = 0;
        public const int LUA_ERRRUN = 2;
        public const int LUA_MULTRET = -1;
        public const int LUA_GCCOLLECT = 2;
        public const int LUA_GCCOUNT = 3;
        public const int LUA_GCISRUNNING = 6;
        public const double LUA_VERSION_NUM = 505;
        public const int LUAL_NUMSIZES = 8 * 16 + 8; // sizeof(lua_Integer)*16 + sizeof(lua_Number)

        // LUA_REGISTRYINDEX = -(INT_MAX/2 + 1000)；lua_upvalueindex(i) = LUA_REGISTRYINDEX - i
        public const int LUA_REGISTRYINDEX = -(int.MaxValue / 2 + 1000);
        public static int LuaUpvalueIndex(int i) => LUA_REGISTRYINDEX - i;

        public static IntPtr AllocUtf8(string s)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            IntPtr p = Marshal.AllocHGlobal(bytes.Length + 1);
            Marshal.Copy(bytes, 0, p, bytes.Length);
            Marshal.WriteByte(p, bytes.Length, 0);
            return p;
        }

        public static void FreeUtf8(IntPtr p) => Marshal.FreeHGlobal(p);

        /// <summary>luaL_dostring 等价物：luaL_loadbufferx + lua_pcallk（返回 pcall 状态码）。</summary>
        public static int DoString(IntPtr L, string chunk)
        {
            IntPtr p = AllocUtf8(chunk);
            try
            {
                int r = LuaImport.luaL_loadbufferx(L, p, (UIntPtr)Encoding.UTF8.GetByteCount(chunk), p, IntPtr.Zero);
                if (r != LUA_OK) return r;
                return LuaImport.lua_pcallk(L, 0, LUA_MULTRET, 0, IntPtr.Zero, IntPtr.Zero);
            }
            finally { FreeUtf8(p); }
        }

        /// <summary>取栈上字符串（idx 处必须是字符串/数字，否则返回 null）。</summary>
        public static string? ToClrString(IntPtr L, int idx)
        {
            IntPtr len = Marshal.AllocHGlobal(8);
            try
            {
                IntPtr p = LuaImport.lua_tolstring(L, idx, len);
                if (p == IntPtr.Zero) return null;
                long n = Marshal.ReadInt64(len); // size_t（x64）
                if (n <= 0) return string.Empty;
                byte[] bytes = new byte[n];
                Marshal.Copy(p, bytes, 0, (int)n);
                return Encoding.UTF8.GetString(bytes);
            }
            finally { Marshal.FreeHGlobal(len); }
        }

        public static IntPtr NewOpenState()
        {
            IntPtr L = LuaImport.luaL_newstate();
            Assert.NotEqual(IntPtr.Zero, L);
            LuaImport.luaL_openselectedlibs(L, -1, 0); // luaL_openlibs(L)
            return L;
        }
    }
}
