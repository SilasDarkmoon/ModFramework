using System;
using Mod.Lua;

namespace Mod.Lua.Tests
{
    /// <summary>
    /// ModLua 全链条冒烟测试：LuaImport（ModLua.PlatDep，默认动态库配置）
    /// → natives/win-x64/lua55.dll（MSVC 构建，LUA_BUILD_AS_DLL 导出）。
    /// 链路：luaL_newstate → luaL_openselectedlibs → luaL_loadbufferx + lua_pcallk
    /// → lua_tointegerx / lua_tolstring 取回 → lua_close。
    /// helper 与 LUA_* 常量见 <see cref="LuaTestUtil"/>；数值配置假设 luaconf.h 默认值
    /// （LUA_INT_LONGLONG + LUA_FLOAT_DOUBLE，LUAL_NUMSIZES = 8*16 + 8 = 136）。
    /// </summary>
    public class LuaSmokeTests
    {
        [Fact]
        public void Version_Matches_505()
        {
            Assert.Equal(LuaTestUtil.LUA_VERSION_NUM, LuaImport.lua_version(IntPtr.Zero));
        }

        [Fact]
        public void CheckVersion_AbiSizesMatch()
        {
            // 数值 ABI 校验：lua_Integer=8 字节、lua_Number=8 字节（luaconf.h 默认配置）。
            // 若 Lua 侧重编时改了数值配置，此调用内部走 luaL_error → longjmp（进程级崩溃
            // 而非断言失败）——这正是冒烟的意义：灾难性 ABI 漂移必须第一时间暴露。
            IntPtr L = LuaTestUtil.NewOpenState();
            try { LuaImport.luaL_checkversion_(L, LuaTestUtil.LUA_VERSION_NUM, (UIntPtr)LuaTestUtil.LUAL_NUMSIZES); }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void ExecuteScript_IntegerReturnValue()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return 1 + 1"));
                Assert.Equal(2L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                LuaImport.lua_settop(L, 0);
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void ExecuteScript_StringRoundTrip()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return string.upper('hello')"));
                Assert.Equal("HELLO", LuaTestUtil.ToClrString(L, -1));
                LuaImport.lua_settop(L, 0);
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void GlobalVariable_SetFromLua_GetFromC()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "answer = 42"));
                IntPtr name = LuaTestUtil.AllocUtf8("answer");
                try
                {
                    LuaImport.lua_getglobal(L, name);
                    Assert.Equal(42L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                }
                finally { LuaTestUtil.FreeUtf8(name); }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void RuntimeError_MessageOnStack()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_ERRRUN, LuaTestUtil.DoString(L, "error('boom')"));
                string? msg = LuaTestUtil.ToClrString(L, -1);
                Assert.NotNull(msg);
                Assert.Contains("boom", msg);
                LuaImport.lua_settop(L, 0);
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void PushFString_SingleArgOverload()
        {
            // 3 参变参重载（EntryPoint 同名 DllImport）：lua_pushfstring(L, "%s", "world")
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                IntPtr fmt = LuaTestUtil.AllocUtf8("%s");
                IntPtr arg = LuaTestUtil.AllocUtf8("world");
                try
                {
                    IntPtr pushed = LuaImport.lua_pushfstring(L, fmt, arg);
                    Assert.NotEqual(IntPtr.Zero, pushed);
                    Assert.Equal("world", LuaTestUtil.ToClrString(L, -1));
                }
                finally { LuaTestUtil.FreeUtf8(fmt); LuaTestUtil.FreeUtf8(arg); }
                LuaImport.lua_settop(L, 0);
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void Gc_CountAndCollect()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                int kb = LuaImport.lua_gc(L, LuaTestUtil.LUA_GCCOUNT, UIntPtr.Zero, 0);
                Assert.True(kb > 0, $"GCCOUNT should be positive, got {kb}");
                Assert.Equal(0, LuaImport.lua_gc(L, LuaTestUtil.LUA_GCCOLLECT, UIntPtr.Zero, 0));
                Assert.Equal(1, LuaImport.lua_gc(L, LuaTestUtil.LUA_GCISRUNNING, UIntPtr.Zero, 0));
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void StateLifecycle_Loop100()
        {
            for (int i = 0; i < 100; i++)
            {
                IntPtr L = LuaImport.luaL_newstate();
                Assert.NotEqual(IntPtr.Zero, L);
                LuaImport.luaL_openselectedlibs(L, -1, 0);
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return 6 * 7"));
                Assert.Equal(42L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                LuaImport.lua_close(L);
            }
        }
    }
}
