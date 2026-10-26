using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.Lua;
using Mod.LowLevel;
using lua = Mod.Lua.LuaLib;

// MonoPInvokeCallback 空替身：ModLua 的 internal 版经 InternalsVisibleTo 可见，
// 测试侧不再自带（自带会与 IVT 导入的同名类型 CS0436 冲突）。
// il2cpp 按全名 AOT.MonoPInvokeCallbackAttribute 识别——PointerDelegateTestSuite 先例。

namespace Mod.Lua.Tests
{
    /// <summary>
    /// 函数互传测试：
    ///   C# → Lua：静态方法 + [MonoPInvokeCallback] + GetFunctionPointerForDelegate
    ///             → lua_pushcclosure 注册（upvalue 传上下文）→ Lua 脚本调用；
    ///   Lua → C#：lua_getglobal + lua_pcallk 调 Lua 函数 / luaL_ref 持久引用；
    ///   嵌套链路：C# 调 Lua 函数 → Lua 内调 C# 回调 → 结果回 C#；
    ///   identity：lua_tocfunction == 注册时缓存的 thunk 地址；
    ///   保活：static readonly delegate 字段 + 只取一次的 thunk 地址，GC 压力下不悬垂。
    /// </summary>
    public class LuaInteropTests
    {
        // ---------------- C# 回调群（静态方法 + 保活字段 + 缓存 thunk 地址） ----------------

        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        private static int CAdd(IntPtr L)
        {
            long a = LuaImport.lua_tointegerx(L, 1, IntPtr.Zero);
            long b = LuaImport.lua_tointegerx(L, 2, IntPtr.Zero);
            LuaImport.lua_pushinteger(L, a + b);
            return 1;
        }
        private static readonly lua.CFunction s_CAdd = CAdd;
        private static readonly IntPtr s_CAddFn = Marshal.GetFunctionPointerForDelegate(s_CAdd);

        // upvalue 1 = lightuserdata（乘数因子）——实例上下文走 Lua 通道的标准模式
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        private static int CMulByUpvalue(IntPtr L)
        {
            long factor = LuaImport.lua_touserdata(L, LuaTestUtil.LuaUpvalueIndex(1)).ToInt64();
            long x = LuaImport.lua_tointegerx(L, 1, IntPtr.Zero);
            LuaImport.lua_pushinteger(L, x * factor);
            return 1;
        }
        private static readonly lua.CFunction s_CMulByUpvalue = CMulByUpvalue;
        private static readonly IntPtr s_CMulByUpvalueFn = Marshal.GetFunctionPointerForDelegate(s_CMulByUpvalue);

        // 字符串双向：读 Lua 字符串参数 → 拼接 → pushlstring 回
        [AOT.MonoPInvokeCallback(typeof(lua.CFunction))]
        private static int CGreet(IntPtr L)
        {
            string? name = LuaTestUtil.ToClrString(L, 1);
            string result = "Hello, " + (name ?? "nil") + "!";
            byte[] bytes = Encoding.UTF8.GetBytes(result);
            IntPtr p = Marshal.AllocHGlobal(bytes.Length);
            try
            {
                Marshal.Copy(bytes, 0, p, bytes.Length);
                LuaImport.lua_pushlstring(L, p, (UIntPtr)bytes.Length);
            }
            finally { Marshal.FreeHGlobal(p); }
            return 1;
        }
        private static readonly lua.CFunction s_CGreet = CGreet;
        private static readonly IntPtr s_CGreetFn = Marshal.GetFunctionPointerForDelegate(s_CGreet);

        // ---------------- helpers ----------------

        /// <summary>压 upvalue（如有）→ pushcclosure → setglobal。调用前先把 nup 个 upvalue 压栈。</summary>
        private static void RegisterGlobal(IntPtr L, IntPtr fn, int nup, string name)
        {
            LuaImport.lua_pushcclosure(L, fn, nup);
            IntPtr p = LuaTestUtil.AllocUtf8(name);
            try { LuaImport.lua_setglobal(L, p); }
            finally { LuaTestUtil.FreeUtf8(p); }
        }

        // ---------------- tests ----------------

        [Fact]
        public void CFunction_Registered_LuaCallsIt()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                RegisterGlobal(L, s_CAddFn, 0, "cadd");
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return cadd(3, 4)"));
                Assert.Equal(7L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void CFunction_Upvalue_AsContext()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                // lightuserdata(6) 作为 upvalue → CMulByUpvalue 读它做乘数
                LuaImport.lua_pushlightuserdata(L, (IntPtr)6);
                RegisterGlobal(L, s_CMulByUpvalueFn, 1, "cmul");
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return cmul(7)"));
                Assert.Equal(42L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void CFunction_StringRoundTrip()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                RegisterGlobal(L, s_CGreetFn, 0, "cgreet");
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return cgreet('Lua')"));
                Assert.Equal("Hello, Lua!", LuaTestUtil.ToClrString(L, -1));
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void TocFunction_Identity()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                RegisterGlobal(L, s_CAddFn, 0, "cadd");
                IntPtr name = LuaTestUtil.AllocUtf8("cadd");
                try
                {
                    LuaImport.lua_getglobal(L, name);
                    // 注册的 C 函数：tocfunction 返回注册时缓存的 thunk 地址（identity）
                    Assert.Equal(s_CAddFn, LuaImport.lua_tocfunction(L, -1));

                    // 纯 Lua 函数：tocfunction 返回 NULL
                    Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "function pure() end"));
                    IntPtr name2 = LuaTestUtil.AllocUtf8("pure");
                    try
                    {
                        LuaImport.lua_getglobal(L, name2);
                        Assert.Equal(IntPtr.Zero, LuaImport.lua_tocfunction(L, -1));
                    }
                    finally { LuaTestUtil.FreeUtf8(name2); }
                }
                finally { LuaTestUtil.FreeUtf8(name); }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void LuaFunction_CalledFromC()
        {
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "function lua_double(x) return x * 2 end"));
                IntPtr name = LuaTestUtil.AllocUtf8("lua_double");
                try
                {
                    LuaImport.lua_getglobal(L, name);
                    LuaImport.lua_pushinteger(L, 21);
                    int r = LuaImport.lua_pcallk(L, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);
                    Assert.Equal(LuaTestUtil.LUA_OK, r);
                    Assert.Equal(42L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                }
                finally { LuaTestUtil.FreeUtf8(name); }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void Nested_CSharpCallsLuaCallsCSharp()
        {
            // 深度互传：C# 调 Lua 函数 → Lua 内部调 C# 回调 → 结果回 C#
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                RegisterGlobal(L, s_CAddFn, 0, "cadd");
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "function calc(x) return cadd(x, 10) end"));
                IntPtr name = LuaTestUtil.AllocUtf8("calc");
                try
                {
                    LuaImport.lua_getglobal(L, name);
                    LuaImport.lua_pushinteger(L, 5);
                    int r = LuaImport.lua_pcallk(L, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);
                    Assert.Equal(LuaTestUtil.LUA_OK, r);
                    Assert.Equal(15L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                }
                finally { LuaTestUtil.FreeUtf8(name); }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void LuaLRef_PersistentFunctionReference()
        {
            // luaL_ref：把 Lua 函数存进 registry（持久引用），跨调用取出执行，最后 unref
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "function secret(x) return x + 1000 end"));
                IntPtr name = LuaTestUtil.AllocUtf8("secret");
                try { LuaImport.lua_getglobal(L, name); }
                finally { LuaTestUtil.FreeUtf8(name); }
                int refid = LuaImport.luaL_ref(L, LuaTestUtil.LUA_REGISTRYINDEX);
                Assert.True(refid > 0);

                // 中间在栈上折腾别的，再从 registry 取回函数执行
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "local junk = {} for i = 1, 10 do junk[i] = i end"));

                LuaImport.lua_rawgeti(L, LuaTestUtil.LUA_REGISTRYINDEX, refid);
                LuaImport.lua_pushinteger(L, 42);
                int r = LuaImport.lua_pcallk(L, 1, 1, 0, IntPtr.Zero, IntPtr.Zero);
                Assert.Equal(LuaTestUtil.LUA_OK, r);
                Assert.Equal(1042L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));

                LuaImport.luaL_unref(L, LuaTestUtil.LUA_REGISTRYINDEX, refid);
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void Callback_ThunkSurvivesGcPressure()
        {
            // 保活验证：static readonly delegate 字段 + 只取一次的 thunk 地址，
            // 在 GC 压力（含压缩）后依然可调用（thunk 不悬垂）。
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                RegisterGlobal(L, s_CAddFn, 0, "cadd");
                for (int round = 0; round < 3; round++)
                {
                    byte[][] junk = new byte[64][];
                    for (int i = 0; i < junk.Length; i++) junk[i] = new byte[1024];
                    GC.Collect(2, GCCollectionMode.Forced, true, true);
                    Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L, "return cadd(1, 2)"));
                    Assert.Equal(3L, LuaImport.lua_tointegerx(L, -1, IntPtr.Zero));
                }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void StructLayout_SizesMatchNativeX64()
        {
            // Expected sizes computed from the C layouts (x64):
            // lua_Debug:  4+pad4 + 4*8 + 8 + 3*4 + 5*1 + pad3 + 2*4 + 60 + 8 = 144
            // luaL_Reg:   2*8 = 16
            // luaL_Buffer: 4*8 + 1024 = 1056
            // luaL_Stream: 2*8 = 16
            Assert.Equal(144, Marshal.SizeOf<lua.Debug>());
            Assert.Equal(16, Marshal.SizeOf<lua.Reg>());
            // Buffer is a ref struct (compile-time stack-local enforcement);
            // the layout lives in the internal blittable BufferL.
            Assert.Equal(1056, Marshal.SizeOf<lua.BufferL>());
            Assert.Equal(16, Marshal.SizeOf<lua.Stream>());
        }

        [Fact]
        public void DebugStruct_NativeFill_CSharpRead()
        {
            // lua_getinfo(">S") fills our blittable lua.Debug - reading back the
            // fields the native side wrote verifies the layout end to end.
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                Assert.Equal(LuaTestUtil.LUA_OK, LuaTestUtil.DoString(L,
                    "function add(a, b)\n  return a + b\nend"));
                IntPtr name = LuaTestUtil.AllocUtf8("add");
                try { LuaImport.lua_getglobal(L, name); }
                finally { LuaTestUtil.FreeUtf8(name); }

                IntPtr arPtr = Marshal.AllocHGlobal(Marshal.SizeOf<lua.Debug>());
                try
                {
                    lua.Debug ar = default;
                    Marshal.StructureToPtr(ar, arPtr, false);
                    IntPtr what = LuaTestUtil.AllocUtf8(">S");
                    try { Assert.Equal(1, LuaImport.lua_getinfo(L, what, arPtr)); }
                    finally { LuaTestUtil.FreeUtf8(what); }
                    ar = Marshal.PtrToStructure<lua.Debug>(arPtr);

                    Assert.Equal("Lua", Marshal.PtrToStringAnsi(ar.what));
                    Assert.Equal(1, ar.linedefined);
                    Assert.Equal(3, ar.lastlinedefined);
                }
                finally { Marshal.FreeHGlobal(arPtr); }
            }
            finally { LuaImport.lua_close(L); }
        }

        [Fact]
        public void BufferStruct_NativeWrite_CSharpRead()
        {
            // luaL_buffinit / luaL_addlstring / luaL_pushresult write into our
            // blittable lua.Buffer - reading back the header fields the native
            // side wrote verifies the layout end to end.
            IntPtr L = LuaTestUtil.NewOpenState();
            try
            {
                IntPtr bufPtr = Marshal.AllocHGlobal(Marshal.SizeOf<lua.BufferL>());
                try
                {
                    lua.BufferL buf = default;
                    Marshal.StructureToPtr(buf, bufPtr, false);
                    LuaImport.luaL_buffinit(L, bufPtr);

                    IntPtr s = LuaTestUtil.AllocUtf8("hello");
                    try { LuaImport.luaL_addlstring(bufPtr, s, (UIntPtr)5); }
                    finally { LuaTestUtil.FreeUtf8(s); }
                    LuaImport.luaL_pushresult(bufPtr);

                    buf = Marshal.PtrToStructure<lua.BufferL>(bufPtr);
                    Assert.Equal(L, buf.L._L);              // lua_State* round-trip
                    Assert.Equal((UIntPtr)5, buf.n);        // 5 chars buffered
                    Assert.True(buf.size.ToUInt64() >= 5);  // capacity
                    Assert.Equal("hello", LuaTestUtil.ToClrString(L, -1));
                }
                finally { Marshal.FreeHGlobal(bufPtr); }
            }
            finally { LuaImport.lua_close(L); }
        }

        // ---------------- LuaLib facade（转发层）冒烟 ----------------

        [Fact]
        public void Facade_StateAndStack_RoundTrip()
        {
            // 走 LuaLib 转发层：State（RawRef 位模式强转 lua_State*）+ 栈操作 +
            // gc 重载 + ref Buffer（RawRef 地址）三条新链路。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0); // luaL_openlibs 宏形态
                Assert.Equal(0, lua.gettop(L));

                lua.pushinteger(L, 42);
                Assert.Equal(1, lua.gettop(L));
                Assert.Equal(42, lua.tointegerx(L, -1, out bool isnum));
                Assert.True(isnum);

                // gc 重载：GCCOUNT（无参形态）与 GCPARAM 查询（param/value 形态）
                Assert.True(lua.gc(L, 3) > 0);            // LUA_GCCOUNT: KB in use
                int pause = lua.gc(L, 9, 3, -1);          // LUA_GCPARAM query PAUSE
                Assert.InRange(pause, 1, 10000);          // percent-scaled old value

                // ref Buffer 链路（栈局部变量 + RawRef 地址）
                lua.settop(L, 0); // 清掉前面的 42
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                IntPtr hello = LuaTestUtil.AllocUtf8("hello");
                try { lua.addlstring(ref b, new Mod.LowLevel.CStringSpan(hello, 6, 5)); }
                finally { LuaTestUtil.FreeUtf8(hello); }
                lua.pushresult(ref b);
                Assert.Equal(1, lua.gettop(L));
                Assert.Equal(5, (long)lua.rawlen(L, -1));
                Assert.Equal("hello", LuaTestUtil.ToClrString(L._L, -1));
            }
            finally
            {
                lua.close(L);
            }
        }

        [Fact]
        public void Facade_MacrosAndDelegates_RoundTrip()
        {
            // 宏函数（dostring/pcall/pop/newtable/is*/tonumber）+ 常量 + delegate
            // 参数（pushcfunction 收 CFunction，Keep 保活 + thunk 转换）链路。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // dostring 宏 -> pcall 宏 -> tointeger 宏 -> pop 宏
                IntPtr code = LuaTestUtil.AllocUtf8("return 1 + 1");
                try { Assert.Equal(lua.OK, lua.dostring(L, new Mod.LowLevel.CStringSpan(code))); }
                finally { LuaTestUtil.FreeUtf8(code); }
                Assert.Equal(2, lua.tointeger(L, -1));
                lua.pop(L, 1);
                Assert.Equal(0, lua.gettop(L));

                // 常量 + is 宏 + newtable 宏 + isnoneornil 边界
                lua.newtable(L);
                Assert.True(lua.istable(L, -1));
                Assert.False(lua.isfunction(L, -1));
                Assert.False(lua.isnoneornil(L, -1));
                lua.pop(L, 1);
                Assert.True(lua.isnone(L, lua.gettop(L) + 1)); // 越界 = TNONE

                // delegate 参数：pushcfunction(CFunction) + pcall 调用（Keep 链路）
                lua.pushcfunction(L, s_CAdd);
                lua.pushinteger(L, 3);
                lua.pushinteger(L, 4);
                Assert.Equal(lua.OK, lua.pcall(L, 2, 1, 0));
                Assert.Equal(7, lua.tointeger(L, -1));
                lua.pop(L, 1);

                // register 宏（delegate + name）+ getglobal + pcall
                IntPtr name = LuaTestUtil.AllocUtf8("cadd");
                try { lua.register(L, new Mod.LowLevel.CStringSpan(name), s_CAdd); }
                finally { LuaTestUtil.FreeUtf8(name); }
                IntPtr gname = LuaTestUtil.AllocUtf8("cadd");
                try { Assert.Equal(lua.TFUNCTION, lua.getglobal(L, new Mod.LowLevel.CStringSpan(gname))); }
                finally { LuaTestUtil.FreeUtf8(gname); }
                lua.pushinteger(L, 10);
                lua.pushinteger(L, 20);
                Assert.Equal(lua.OK, lua.pcall(L, 2, 1, 0));
                Assert.Equal(30, lua.tointeger(L, -1));
                lua.pop(L, 1);

                // upvalueindex 宏 + 常量一致性
                Assert.Equal(lua.REGISTRYINDEX - 1, lua.upvalueindex(1));
                Assert.Equal(9, lua.NUMTYPES);

                // GC 压力下 delegate 保活（Keep 注册表兜底）
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
                IntPtr gname2 = LuaTestUtil.AllocUtf8("cadd");
                try { lua.getglobal(L, new Mod.LowLevel.CStringSpan(gname2)); }
                finally { LuaTestUtil.FreeUtf8(gname2); }
                lua.pushinteger(L, 5);
                lua.pushinteger(L, 6);
                Assert.Equal(lua.OK, lua.pcall(L, 2, 1, 0));
                Assert.Equal(11, lua.tointeger(L, -1));
                lua.pop(L, 1);
            }
            finally
            {
                lua.close(L);
            }
        }

        [Fact]
        public void Facade_Setfuncs_RegArray_And_ReturnDelegates()
        {
            // setfuncs(Reg[])：CoreCLR 测试主机走 GCHandle pin 分支；数组含
            // default(Reg) 终止项；注册后调用验证。atpanic 返回旧 panic 的
            // delegate（GetDelegateForFunctionPointer 链路）。getstack 走
            // 栈局部拷贝兜底分支（非 Unity 主机）。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // setfuncs(Reg[])：表在栈顶，注册 add，nup=0
                lua.newtable(L);
                IntPtr n1 = LuaTestUtil.AllocUtf8("add");
                try
                {
                    lua.Reg[] regs = new lua.Reg[2]; // 末项 default = {NULL, NULL} 终止
                    regs[0] = new lua.Reg { name = n1, func = s_CAddFn };
                    lua.setfuncs(L, regs, 0);
                }
                finally { LuaTestUtil.FreeUtf8(n1); }

                // 从表里取 add 并调用（setfuncs 后表在栈顶）
                IntPtr k = LuaTestUtil.AllocUtf8("add");
                try { Assert.Equal(lua.TFUNCTION, lua.getfield(L, -1, new Mod.LowLevel.CStringSpan(k))); }
                finally { LuaTestUtil.FreeUtf8(k); }
                lua.pushinteger(L, 20);
                lua.pushinteger(L, 22);
                Assert.Equal(lua.OK, lua.pcall(L, 2, 1, 0));
                Assert.Equal(42, lua.tointeger(L, -1));
                lua.pop(L, 2); // 结果 + 表

                // atpanic 返回旧 panic 的 delegate。luaL_newstate 装了默认
                // panic（lauxlib.c:1187 lua_atpanic(L, &panic)）——卸载时
                // 返回它的 native 地址 → GetDelegateForFunctionPointer 链路。
                lua.CFunction defPanic = lua.atpanic(L, null); // 卸载默认 panic
                Assert.NotNull(defPanic); // fnptr -> delegate 成功
                Assert.Null(lua.atpanic(L, s_CAdd)); // 装自定义，旧值是 NULL（刚卸载）
                lua.CFunction installed = lua.atpanic(L, null); // 卸载，返回刚装的 thunk
                Assert.NotNull(installed); // Keep 的 thunk 地址往返
                lua.atpanic(L, defPanic); // 恢复默认 panic（delegate -> fnptr -> Lua）

                // getstack：非 Unity 主机走栈局部拷贝兜底分支。dostring 已返回、
                // 栈上无 Lua 帧——level 0 返回 0（无帧），验证拷贝往返链路本身
                // 不崩（值语义由 LuaImport 层的 DebugStruct_NativeFill 背书）。
                lua.Debug ar = default;
                Assert.Equal(0, lua.getstack(L, 0, ref ar));
            }
            finally
            {
                lua.close(L);
            }
        }
        // ---------------- 探针：循环内 stackalloc 的地址行为（A2 争议实测） ----------------

        // CA2014 (stackalloc in loop) is deliberately exercised here: the probe
        // exists to verify that each iteration allocates NEW space (addresses
        // strictly descending, blocks independently alive until method return).
#pragma warning disable CA2014
        [Fact]
        public void Probe_StackallocInLoop_NewSpacePerIteration()
        {
            // 假设 1（复用）：每次迭代的 localloc 落在同一地址 → addrs 全相同、
            //               标记互相覆盖（只剩最后一项的内容）。
            // 假设 2（新空间）：localloc 从当前 RSP 向下分配，循环回边不恢复
            //               RSP → 地址单调下降且两两不同，各块独立存活到方法返回。
            const int N = 8;
            Span<IntPtr> addrs = stackalloc IntPtr[N];
            for (int i = 0; i < N; i++)
            {
                Span<byte> block = stackalloc byte[32];
                block.Fill((byte)(i + 1));                    // 每块写标记 i+1
                addrs[i] = Mod.LowLevel.RawRef.Of(ref block[0]).Address;   // 记录首地址
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < N; i++)
                sb.AppendLine($"[{i}] addr=0x{addrs[i]:X16} firstByte={Marshal.ReadByte(addrs[i])}");
            Console.WriteLine(sb.ToString());

            // 断言 1：地址两两不同（复用 → 相同；新空间 → 不同）
            for (int i = 0; i < N; i++)
                for (int j = i + 1; j < N; j++)
                    Assert.NotEqual(addrs[i], addrs[j]);

            // 断言 2：每块标记保留（独立存活的证据；复用场景下只剩最后一项的标记）
            for (int i = 0; i < N; i++)
                Assert.Equal((byte)(i + 1), Marshal.ReadByte(addrs[i]));
        }
#pragma warning restore CA2014

        // ---------------- 探针：空 Span 安全取首元素地址（A5 方案验证） ----------------

        [Fact]
        public void Probe_EmptySpan_GetRef_SafeAddress()
        {
            // span.GetRef()（SpanStrEx 扩展）对空 Span：fixed 得 null 指针 →
            // RawRef 地址应为 IntPtr.Zero（不越界、不崩）——即"空 Span → NULL"原语。
            Span<byte> empty = default;
            IntPtr pEmpty = Mod.LowLevel.RawRef.Of(ref empty.GetRef()).Address;
            Assert.Equal(IntPtr.Zero, pEmpty);

            // 非空 Span：首元素地址正确可读。
            Span<byte> four = stackalloc byte[4];
            four[0] = 0xAB;
            IntPtr pFour = Mod.LowLevel.RawRef.Of(ref four.GetRef()).Address;
            Assert.NotEqual(IntPtr.Zero, pFour);
            Assert.Equal((byte)0xAB, Marshal.ReadByte(pFour));

            // Slice() 后的空内容（Size>0 但 Length==0）同样安全。
            Span<byte> buf = stackalloc byte[8];
            Span<byte> content = buf.Slice(0, 0);
            IntPtr pContent = Mod.LowLevel.RawRef.Of(ref content.GetRef()).Address;
            Assert.Equal(IntPtr.Zero, pContent);
        }

        // ---------------- A5 修复验证：空 CStringSpan 不再越界 ----------------

        [Fact]
        public void Facade_EmptySpan_NoIndexOutOfRange()
        {
            // GetRef 原语：空 Span -> IntPtr.Zero（C 的 NULL 语义）。
            // 之前 pushlstring/addlstring/loadbufferx 对空 Span 抛 IndexOutOfRange。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // pushlstring 空 -> push 空串（lua_pushlstring(L, NULL, 0) 合法）
                lua.pushlstring(L, default);
                Assert.Equal(1, lua.gettop(L));
                Assert.NotEqual(0, lua.isstring(L, -1)); // isstring is a real function returning int
                Assert.Equal(0UL, lua.rawlen(L, -1));
                lua.pop(L, 1);

                // addlstring 空 -> no-op（不崩）
                lua.settop(L, 0);
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                lua.addlstring(ref b, default);
                lua.pushresult(ref b);
                Assert.Equal(1, lua.gettop(L));
                Assert.Equal(0UL, lua.rawlen(L, -1));
                lua.pop(L, 1);

                // loadbufferx 空 buff + 空(default) mode -> NULL 语义（IsNull 方案）：
                // mode=NULL -> f_parser 替换 "bt" -> 空 chunk 是合法程序（空 block）-> OK
                // （native 对照实锤：mode=NULL → OK；mode="" → ERRSYNTAX "attempt to load a
                //   text chunk (mode is '')" —— B6 的毒药值）
                lua.settop(L, 0);
                int r = lua.loadbufferx(L, default, default, default);
                Assert.Equal(lua.OK, r);
                Assert.Equal(1, lua.gettop(L)); // 空 chunk 编译成空函数入栈
                lua.settop(L, 0);
            }
            finally
            {
                lua.close(L);
            }
        }
        // ---------------- C8 样例安全性探针：using 变量 + Unprotect(in) 写本体还是拷贝？ ----------------

        [Fact]
        public void Probe_UsingLocal_InUnprotect_WritesOriginal()
        {
            // using 资源变量是 readonly local。Ref.Unprotect(in d) = ...
            // 若 Roslyn 对 readonly local 传 in 做防御性拷贝，则写的是临时拷贝——
            // d 保持 default → using 退出 Dispose 释放 null → NativeBuffer 静默泄漏。
            using (Mod.NativeBuffer d = default)
            {
                Mod.LowLevel.Ref.Unprotect(in d) = new Mod.NativeBuffer(16);
                Assert.True(d.Ptr != IntPtr.Zero,
                    "Unprotect(in usingLocal) wrote a DEFENSIVE COPY - Dispose would leak the NativeBuffer!");
            }
            // 对照：普通局部变量（可写）传 in——必须直传本体。
            Mod.NativeBuffer d2 = default;
            Mod.LowLevel.Ref.Unprotect(in d2) = new Mod.NativeBuffer(16);
            Assert.True(d2.Ptr != IntPtr.Zero);
            d2.Dispose();
        }

        // ---------------- C8 大路径冒烟：>256 元素走 AllocHGlobal 分支 ----------------

        [Fact]
        public void Facade_Checkoption_Paths()
        {
            // checkoption 三条路径：常规小表 / 大表（llst 走 AllocHGlobal）/
            // 大 item 累积（totalstrbytes 超 256 后 item 走 AllocHGlobal）+ def 缺省。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // 1) 常规：栈上 "b"，lst = {a, b, c}（无显式终止项，自动补）→ 索引 1
                lua.settop(L, 0);
                lua.pushstring(L, new Mod.LowLevel.CStringSpan("b"));
                Mod.CStringRef[] small = { new Mod.CStringRef("a"), new Mod.CStringRef("b"), new Mod.CStringRef("c") };
                Assert.Equal(1, lua.checkoption(L, 1, default, small));

                // 2) def 缺省：空栈（arg=1 为 TNONE）→ def="c" 匹配 → 索引 2
                lua.settop(L, 0);
                Assert.Equal(2, lua.checkoption(L, 1, new Mod.LowLevel.CStringSpan("c"), small));

                // 3) 大选项表（300 项 > 256）：llst 走 AllocHGlobal
                lua.settop(L, 0);
                lua.pushstring(L, new Mod.LowLevel.CStringSpan("opt150"));
                var big = new Mod.CStringRef[300];
                for (int i = 0; i < big.Length; i++) big[i] = new Mod.CStringRef("opt" + i);
                Assert.Equal(150, lua.checkoption(L, 1, default, big));

                // 4) 大 item 累积：每项 100 字节 × 5 项 = 500 > 256，
                //    第 3 项起走 AllocHGlobal（itemdisposerlist 路径）
                lua.settop(L, 0);
                lua.pushstring(L, new Mod.LowLevel.CStringSpan(new string('x', 100) + "2"));
                var bigItems = new Mod.CStringRef[5];
                for (int i = 0; i < bigItems.Length; i++) bigItems[i] = new Mod.CStringRef(new string('x', 100) + i);
                Assert.Equal(2, lua.checkoption(L, 1, default, bigItems));
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Facade_DeferredMacros_RoundTrip()
        {
            // 6 个补齐宏：newlibtable/newlib/checklong/optlong/opt/intop 族。
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // newlib：注册 add（含显式终止项）→ 表在栈顶 → 取出调用
                lua.settop(L, 0);
                IntPtr n1 = LuaTestUtil.AllocUtf8("add");
                try
                {
                    lua.Reg[] regs = new lua.Reg[2]; // 末项 default = {NULL, NULL} 终止
                    regs[0] = new lua.Reg { name = n1, func = s_CAddFn };
                    lua.newlib(L, regs);
                }
                finally { LuaTestUtil.FreeUtf8(n1); }
                Assert.Equal(1, lua.gettop(L)); // newlib 留下库表
                Assert.True(lua.istable(L, -1));
                IntPtr k = LuaTestUtil.AllocUtf8("add");
                try { Assert.Equal(lua.TFUNCTION, lua.getfield(L, -1, new Mod.LowLevel.CStringSpan(k))); }
                finally { LuaTestUtil.FreeUtf8(k); }
                lua.pushinteger(L, 3);
                lua.pushinteger(L, 4);
                Assert.Equal(lua.OK, lua.pcall(L, 2, 1, 0));
                Assert.Equal(7, lua.tointeger(L, -1));
                lua.pop(L, 2); // 结果 + 库表

                // newlibtable：仅预分配（无注册）——空表
                lua.settop(L, 0);
                lua.newlibtable(L, new lua.Reg[1]); // 仅终止项 = 0 个函数
                Assert.True(lua.istable(L, -1));
                lua.pop(L, 1);

                // checklong / optlong
                lua.pushinteger(L, 42);
                Assert.Equal(42, lua.checklong(L, 1));
                Assert.Equal(7, lua.optlong(L, 2, 7)); // arg 2 缺省 → def

                // opt 泛型（静态 lambda，编译器缓存委托）
                Assert.Equal(7L, lua.opt(L, static (st, n) => lua.checkinteger(st, n), 2, 7L));
                Assert.Equal(42L, lua.opt(L, static (st, n) => lua.checkinteger(st, n), 1, 7L));
                lua.pop(L, 1);

                // intop wrap-around 算术
                Assert.Equal(unchecked(long.MaxValue + 1), lua.intopAdd(long.MaxValue, 1));   // 回绕到 MinValue
                Assert.Equal(unchecked(long.MinValue - 1), lua.intopSub(long.MinValue, 1));   // 回绕到 MaxValue
                Assert.Equal(unchecked(long.MaxValue * 2), lua.intopMul(long.MaxValue, 2));
                Assert.Equal(0x0F0FL, lua.intopAnd(0xFF0FL, 0x0FFFL));
                Assert.Equal(0xFF0FL | 0x0FFFL, lua.intopOr(0xFF0FL, 0x0FFFL));
                Assert.Equal(0xFF0FL ^ 0x0FFFL, lua.intopXor(0xFF0FL, 0x0FFFL));
                Assert.Equal(1L << 62, lua.intopShl(1, 62));
                Assert.Equal(1L << 62 >> 62, lua.intopShr(1L << 62, 62));
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Facade_LargeString_AllocHGlobal_Path()
        {
            lua.State L = lua.newstate();
            try
            {
                lua.openselectedlibs(L, -1, 0);

                // 300 字节字符串（> 256 阈值）→ pushstring 拷贝走 AllocHGlobal
                string big = new string('x', 300);
                lua.pushstring(L, new Mod.LowLevel.CStringSpan(big));
                Assert.Equal(300UL, lua.rawlen(L, -1));
                Assert.Equal(300, lua.tostring(L, -1).Length);
                lua.pop(L, 1);

                // 大 key 的 getglobal（NUL 拷贝走 AllocHGlobal）—— 不存在的 key → nil
                lua.settop(L, 0);
                string bigKey = new string('k', 300);
                Assert.Equal(lua.TNIL, lua.getglobal(L, new Mod.LowLevel.CStringSpan(bigKey)));
                lua.pop(L, 1);

                // >256 字节的 dostring（loadstring 拷贝走 AllocHGlobal）
                string code = "local t = {} for i = 1, 50 do t[i] = string.rep('a', 5) end return #t -- "
                    + new string(' ', 250);
                Assert.Equal(lua.OK, lua.dostring(L, new Mod.LowLevel.CStringSpan(code)));
                Assert.Equal(50, lua.tointeger(L, -1));
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        // -------- luaL_Buffer 变长机制（5.5：placeholder → to-be-closed UBox → external string 零拷贝） --------

        [Fact]
        public void Buffer_Growth_BeyondInitial()
        {
            // > LUAL_BUFFERSIZE (1024 on x64) forces the placeholder to become
            // a to-be-closed box; b is repointed, content preserved, the stack
            // stays at buffinit level + 1, pushresult nets +1 (result string).
            lua.State L = lua.newstate();
            try
            {
                int top0 = lua.gettop(L);
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                Assert.Equal(top0 + 1, lua.gettop(L)); // placeholder slot pushed

                IntPtr initAddr = RawRef.Of(ref b._Inner.init).Address;
                Assert.Equal(initAddr, lua.buffaddr(b)); // still the embedded init area

                // 5000 bytes -> several growths (1024 -> 1537 -> 2306 -> 3460 -> 5191)
                const int N = 5000;
                IntPtr src = Marshal.AllocHGlobal(N);
                try
                {
                    for (int i = 0; i < N; i++) Marshal.WriteByte(src, i, (byte)(i % 127 + 1));
                    lua.addlstring(ref b, new CStringSpan(src, N, N));
                }
                finally { Marshal.FreeHGlobal(src); }

                Assert.Equal((UIntPtr)N, lua.bufflen(b));
                Assert.NotEqual(initAddr, lua.buffaddr(b)); // box owns the memory now
                Assert.True(b.size.ToUInt64() > 1024);          // grew past the initial size
                Assert.Equal(top0 + 1, lua.gettop(L));          // box replaced the placeholder slot

                lua.pushresult(ref b);
                Assert.Equal(top0 + 1, lua.gettop(L));          // pre-buffinit level + result string
                Assert.Equal((long)N, (long)lua.rawlen(L, -1));
                string s = LuaTestUtil.ToClrString(L._L, -1)!;
                Assert.Equal(N, s.Length);
                Assert.Equal((char)(0 % 127 + 1), s[0]);
                Assert.Equal((char)(2499 % 127 + 1), s[2499]);
                Assert.Equal((char)(4999 % 127 + 1), s[4999]);
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Buffer_Addchar_Growth()
        {
            // addchar (macro facade) crosses the 1024 boundary byte by byte.
            lua.State L = lua.newstate();
            try
            {
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                const int N = 1500;
                for (int i = 0; i < N; i++) lua.addchar(ref b, (byte)(i % 127 + 1));
                Assert.Equal((UIntPtr)N, lua.bufflen(b));
                Assert.NotEqual(RawRef.Of(ref b._Inner.init).Address, lua.buffaddr(b)); // grew
                lua.pushresult(ref b);
                string s = LuaTestUtil.ToClrString(L._L, -1)!;
                Assert.Equal(N, s.Length);
                for (int i = 0; i < N; i += 137) Assert.Equal((char)(i % 127 + 1), s[i]);
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Buffer_Buffinitsize_Pushresultsize()
        {
            // Known-size path: preallocate 2000 (> 1024: immediate box), write
            // through the returned region, finish with the actual size (<= sz).
            lua.State L = lua.newstate();
            try
            {
                int top0 = lua.gettop(L);
                lua.Buffer b = default;
                CStringRef r = lua.buffinitsize(L, ref b, 2000);
                Assert.Equal(top0 + 1, lua.gettop(L)); // placeholder/box slot
                Assert.NotEqual(RawRef.Of(ref b._Inner.init).Address, lua.buffaddr(b)); // box right away

                const int W = 1500; // actual written size, less than the preallocation
                for (int i = 0; i < W; i++) r[i] = (byte)(i % 127 + 1);
                lua.pushresultsize(ref b, (UIntPtr)W);
                Assert.Equal(top0 + 1, lua.gettop(L));
                Assert.Equal((long)W, (long)lua.rawlen(L, -1));
                string s = LuaTestUtil.ToClrString(L._L, -1)!;
                Assert.Equal(W, s.Length);
                Assert.Equal((char)(749 % 127 + 1), s[749]);
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Buffer_AddValue_StackDiscipline()
        {
            // addvalue: the only buffer call allowed with an extra element on
            // the stack (the value, above the placeholder/box); it pops it.
            lua.State L = lua.newstate();
            try
            {
                int top0 = lua.gettop(L);
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                lua.addstring(ref b, new CStringSpan("a="));
                lua.pushinteger(L, 42);                 // value above the placeholder
                Assert.Equal(top0 + 2, lua.gettop(L));
                lua.addvalue(ref b);                   // converts + adds + pops
                Assert.Equal(top0 + 1, lua.gettop(L));  // value gone
                lua.addstring(ref b, new CStringSpan("!"));
                lua.pushresult(ref b);
                Assert.Equal(top0 + 1, lua.gettop(L));
                Assert.Equal("a=42!", LuaTestUtil.ToClrString(L._L, -1));
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Buffer_Addgsub_IntoBuffer()
        {
            lua.State L = lua.newstate();
            try
            {
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                lua.addstring(ref b, new CStringSpan("prefix:"));
                lua.addgsub(ref b, new CStringSpan("hello world hello"),
                            new CStringSpan("hello"), new CStringSpan("hi"));
                lua.pushresult(ref b);
                Assert.Equal("prefix:hi world hi", LuaTestUtil.ToClrString(L._L, -1));
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void Buffer_Prepbuffer_ManualWrite()
        {
            // prepbuffer: LUAL_BUFFERSIZE writable region + addsize commit.
            lua.State L = lua.newstate();
            try
            {
                lua.Buffer b = default;
                lua.buffinit(L, ref b);
                lua.addstring(ref b, new CStringSpan("ab"));
                CStringRef r = lua.prepbuffer(ref b);
                r[0] = (byte)'c';
                r[1] = (byte)'d';
                lua.addsize(ref b, (UIntPtr)2);
                lua.pushresult(ref b);
                Assert.Equal("abcd", LuaTestUtil.ToClrString(L._L, -1));
                lua.pop(L, 1);
            }
            finally { lua.close(L); }
        }

        [Fact]
        public void BufferBox_HeapHolder_And_DisposeSemantics()
        {
            // BufferBox: the struct lives in native memory (stable address on
            // every host, including compacting-GC runtimes), so the whole
            // sequence works with the holder on the heap. After Dispose every
            // accessor throws ObjectDisposedException (uniform semantics).
            lua.State L = lua.newstate();
            try
            {
                int top0 = lua.gettop(L);
                lua.BufferBox box = new lua.BufferBox();
                lua.buffinit(L, box);
                Assert.Equal(top0 + 1, lua.gettop(L)); // placeholder slot

                // growth path through the heap holder: > 1024 forces the box
                const int N = 3000;
                IntPtr src = Marshal.AllocHGlobal(N);
                try
                {
                    for (int i = 0; i < N; i++) Marshal.WriteByte(src, i, (byte)(i % 127 + 1));
                    lua.addlstring(box, new CStringSpan(src, N, N));
                }
                finally { Marshal.FreeHGlobal(src); }
                Assert.Equal((UIntPtr)N, lua.bufflen(box));
                Assert.NotEqual(RawRef.Of(ref box.init).Address, lua.buffaddr(box)); // grew

                lua.pushresult(box);
                Assert.Equal(top0 + 1, lua.gettop(L));
                string s = LuaTestUtil.ToClrString(L._L, -1)!;
                Assert.Equal(N, s.Length);
                Assert.Equal((char)(2999 % 127 + 1), s[2999]);
                lua.pop(L, 1);

                // sequence finished -> safe to Dispose
                box.Dispose();
                Assert.Throws<ObjectDisposedException>(() => lua.addstring(box, new CStringSpan("x")));
                Assert.Throws<ObjectDisposedException>(() => _ = box.b);
                Assert.Throws<ObjectDisposedException>(() => _ = box.n);
                Assert.Throws<ObjectDisposedException>(() => _ = box.Value);
                box.Dispose(); // double-Dispose stays a no-op (idempotent)
            }
            finally { lua.close(L); }
        }
    }
}
