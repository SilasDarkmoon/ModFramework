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
#endif
        public static void Prepare()
        {
            // 这个方法的存在是为了让 NativeAOT 保留对某些成员的访问权限，防止它们被错误地裁剪掉。
            // 这里可以调用一些反射或者其他方式来确保这些成员被保留。
        }
    }
}
