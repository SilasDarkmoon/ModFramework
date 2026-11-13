using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Mod.LowLevel
{
    public static partial class UnsafeEx
    {
        public static IntPtr GetStaticFieldAddress(FieldInfo field)
        {
            return StaticFieldPlatDep.GetStaticFieldAddress(field);
        }
        public static IntPtr GetThreadStaticAddress(FieldInfo field)
        {
            return StaticFieldPlatDep.GetThreadStaticAddress(field);
        }

        private static partial class StaticFieldPlatDep
        {
            private interface IPlatDep
            {
                IntPtr GetStaticFieldAddress(FieldInfo field); // NOTICE: cache the result in the implementation, when the address is fixed, or cache the getter wrapper.
                IntPtr GetThreadStaticAddress(FieldInfo field); // NOTICE: cache the result in the implementation, when the address is fixed, or cache the getter wrapper.
            }
            private static IPlatDep _PlatDep;

            static StaticFieldPlatDep()
            {
            }

            public static IntPtr GetStaticFieldAddress(FieldInfo field)
            {
                return _PlatDep.GetStaticFieldAddress(field);
            }
            public static IntPtr GetThreadStaticAddress(FieldInfo field)
            {
                return _PlatDep.GetThreadStaticAddress(field);
            }
        }
    }
}
