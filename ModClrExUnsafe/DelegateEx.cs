using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Mod.LowLevel
{
    public static class DelegateEx
    {
        private struct DelegateStub
        {
            public object Target;
            public object MethodBase;
            public IntPtr MethodPtr;
            public IntPtr MethodPtrAux;
            // Multicast below:
            public Delegate[] InvocationList;
            public int InvocationCount;
        }
        public static readonly FieldIndicator<object> FieldTarget;
        public static readonly FieldIndicator<object> FieldMethodBase;
        public static readonly FieldIndicator<IntPtr> FieldMethodPtr;
        public static readonly FieldIndicator<IntPtr> FieldMethodPtrAux;
        // Multicast below:
        public static readonly FieldIndicator<Delegate[]> FieldInvocationList;
        public static readonly FieldIndicator<int> FieldInvocationCount;
        static DelegateEx()
        {
            Console.WriteLine(typeof(DelegateStub).StructLayoutAttribute.Value);
            foreach (var field in typeof(DelegateStub).GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance))
            {
                Console.WriteLine(field.Name);
                Console.WriteLine(field.FieldHandle.Value);
            }

            DelegateStub stub = new DelegateStub();
            FieldTarget = stub.GetFieldOffset(ref stub.Target);
            FieldMethodBase = stub.GetFieldOffset(ref stub.MethodBase);
            FieldMethodPtr = stub.GetFieldOffset(ref stub.MethodPtr);
            FieldMethodPtrAux = stub.GetFieldOffset(ref stub.MethodPtrAux);
            // Multicast below:
            FieldInvocationList = stub.GetFieldOffset(ref stub.InvocationList);
            FieldInvocationCount = stub.GetFieldOffset(ref stub.InvocationCount);

            Console.WriteLine(FieldTarget);
            Console.WriteLine(FieldMethodBase);
            Console.WriteLine(FieldMethodPtr);
            Console.WriteLine(FieldMethodPtrAux);
            Console.WriteLine(FieldInvocationList);
            Console.WriteLine(FieldInvocationCount);
        }

        //public static T ConvertDelegate<T>(Delegate source) where T : Delegate
        //{
        //    if (source == null)
        //    {
        //        return null;
        //    }
        //    if (source.GetInvocationList().Length > 1)
        //    {
        //        Delegate.CreateDelegate(
        //    }
        //}
    }
}
