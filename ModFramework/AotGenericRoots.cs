using System;
using System.Reflection;

// Replaces the <GenericArgument> directives from the removed Rd.xml.
// CreateGenericInvoker calls MakeGenericMethod with runtime-constructed Type[]
// arguments, so ILC cannot infer those instantiations from the call sites.
// Calling MakeGenericMethod with constant typeof arguments here lets ILC root
// the exact generic method instantiations the reflection path needs, and the
// typeof expressions additionally root the closed generic struct instantiations
// (TestFieldsStruct<long>) that Rd.xml used to keep as "Required All".
internal static class AotGenericRoots
{
    public static void Prepare()
    {
        _ = typeof(TestScript.TestFieldsStruct).GetMethod("TestGeneric")
            .MakeGenericMethod(typeof(IntPtr));
        _ = typeof(TestScript.TestFieldsStruct<long>).GetMethod("TestGeneric")
            .MakeGenericMethod(typeof(IntPtr));
        _ = typeof(TestScript_UnsafeInstanceDelegate.TestFieldsStruct).GetMethod("TestGeneric")
            .MakeGenericMethod(typeof(IntPtr));
        _ = typeof(TestScript_UnsafeInstanceDelegate.TestFieldsStruct<long>).GetMethod("TestGeneric")
            .MakeGenericMethod(typeof(IntPtr));
    }
}
