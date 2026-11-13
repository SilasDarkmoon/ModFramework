using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Mod.LowLevel
{
#if NET7_0_OR_GREATER
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
#endif
    public static class NativeAOTHelper
    {
#if NET7_0_OR_GREATER
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.FieldInfos.NativeFormat.NativeFormatRuntimeFieldInfo", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.MethodInfos.RuntimeMethodInfo", "System.Private.CoreLib")]
        // 10.0.15 trimmer no longer keeps derived overrides through the base-class
        // DLA alone: UnsafeDelegateWrapperNativeAOT cctor reflects on the runtime
        // cdmi type RuntimeNamedMethodInfo`1 (typeof(minew) is the DERIVED type),
        // so its override chain must be rooted explicitly.
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Reflection.Runtime.MethodInfos.RuntimeNamedMethodInfo`1", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Delegate", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.ByRefParam", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.ByRefLocalSlot", "PointerDelegate")]

        // CreateDelegateUnsafe 的 NativeAOT wrapper（嵌套类 + 嵌套委托，ILC 反射数据保留）
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT+CreateDelegateWithoutSignatureValidationDel", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.DelegateExImp.UnsafeDelegateWrapperNativeAOT+UnsafeCreateDelegateDel", "ModClrExUnsafe")]

        // CurryWrapper 族（修正：实际嵌套于 UnsafeEx，此前指向不存在的 Mod.DelegateCurry）
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`2", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`3", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`4", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`5", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`6", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`7", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`8", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`9", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`10", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`11", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`12", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`13", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`14", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`15", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`16", "ModClrExUnsafe")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.UnsafeEx.CurryWrapper`17", "ModClrExUnsafe")]

        // RefFunc 族（byref 返回 delegate——RefFunc 链路核心）
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`1", "PointerDelegate")]
        // RefInvoker wrapper family (Generator-woven Invoke bodies reach these through
        // interface dispatch the trimmer cannot see; AOT strips them at publish time
        // and the runtime throws TypeLoadException from FuncRefInvoker`1.Invoke)
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FuncRefInvoker1`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.RefFunc`17", "PointerDelegate")]

        // FreeFunc 族
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.FreeFunc`17", "PointerDelegate")]

        // PointerFunc 族
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`1", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`2", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`3", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`4", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`5", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`6", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`7", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`8", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`9", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`10", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`11", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`12", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`13", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`14", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`15", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`16", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.PointerFunc`17", "PointerDelegate")]
#endif
        public static void Prepare()
        {
            // 这个方法的存在是为了让 NativeAOT 保留对某些成员的访问权限，防止它们被错误地裁剪掉。
            // 这里可以调用一些反射或者其他方式来确保这些成员被保留。
        }
    }
}
