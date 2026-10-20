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
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "System.Delegate", "System.Private.CoreLib")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.LowLevel.ByRefParam", "PointerDelegate")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`2", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`3", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`4", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`5", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`6", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`7", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`8", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`9", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`10", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`11", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`12", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`13", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`14", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`15", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`16", "ModClrEx")]
        [DynamicDependency(DynamicallyAccessedMemberTypes.All, "Mod.DelegateCurry.CurryWrapper`17", "ModClrEx")]
#endif
        public static void Prepare()
        {
            // 这个方法的存在是为了让 NativeAOT 保留对某些成员的访问权限，防止它们被错误地裁剪掉。
            // 这里可以调用一些反射或者其他方式来确保这些成员被保留。
        }
    }
}
