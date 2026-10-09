using Mod.LowLevel;
using System;
using System.Numerics;
using System.Reflection;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for GenericInvokers.cs: CreateGenericInvoker (MethodBase and Type+name
    /// entry points) and CreateGenericInstanceInvoker. The pipeline is
    /// GuessGenericParameters -> MakeGenericMethod -> CreateDelegateUnified (the
    /// UnifiedInvokers layer, covered by UnifiedInvokersTests), so these tests focus
    /// on the generic layer: type inference from argument shapes, explicit
    /// genericTypes, instance-method `this` handling, the R=ByRefParam (ref-return)
    /// and R=VoidReturn marker shapes, and failure modes.
    ///
    /// R/U/P convention (library-wide): R is the return type as seen by the caller
    /// (ByRefParam marks a ref return, VoidReturn marks void); U are the target
    /// method's parameter types; P are the UnifiedRefFunc `in` parameter types.
    /// </summary>
    public class GenericInvokersTests
    {
        // =====================================================================
        // host methods the invokers are built against
        // =====================================================================

        private static class GHost
        {
            public static T Echo<T>(T x) => x;

            public static T Sum2<T>(T a, T b) where T : IAdditionOperators<T, T, T> => a + b;

            public static T Sum8<T>(T a, T b, T c, T d, T e, T f, T g, T h)
                where T : IAdditionOperators<T, T, T>
                => a + b + c + d + e + f + g + h;

            public static T Sum16<T>(T a, T b, T c, T d, T e, T f, T g, T h,
                                     T i, T j, T k, T l, T m, T n, T o, T p)
                where T : IAdditionOperators<T, T, T>
                => a + b + c + d + e + f + g + h + i + j + k + l + m + n + o + p;

            public static ref T FirstOf<T>(T[] arr) => ref arr[0];

            public static void Drop<T>(T x) { }
        }

        private sealed class GInst
        {
            public int Base;
            public GInst(int b) => Base = b;

            // NOTE: the constrained shape currently HANGS the Type+name entry
            // point in FindOverload (same root cause as TypeAndName_Sum2_Int);
            // kept faithfully - see the note there.
            public T AddBase<T>(T x) where T : IAdditionOperators<T, T, T>
                => x + (T)Convert.ChangeType(Base, typeof(T));
        }

        private static MethodInfo M(string name) => typeof(GHost).GetMethod(name, BindingFlags.Public | BindingFlags.Static);

        // =====================================================================
        // MethodBase entry, value return, inference from argument types
        // =====================================================================

        [Fact]
        public void MethodBase_Echo1_Int()
        {
            // genericTypes == null -> T inferred as int from U1
            var f = M("Echo").CreateGenericInvoker<int, int, int>();

            Assert.Equal(42, f.Invoke(42));
            Assert.Equal(-1, f.Invoke(-1));
        }

        [Fact]
        public void MethodBase_Echo1_String()
        {
            // reference-type instantiation through the calli pipeline
            var f = M("Echo").CreateGenericInvoker<string, string, string>();

            Assert.Equal("hello", f.Invoke("hello"));
        }

        [Fact]
        public void MethodBase_Sum2_Int()
        {
            var f = M("Sum2").CreateGenericInvoker<int, int, int, int, int>();

            Assert.Equal(42, f.Invoke(20, 22));
        }

        [Fact]
        public void MethodBase_Sum8_Int()
        {
            // 8-arg overload: R + U1..U8 + P1..P8 = 17 type arguments
            var f = M("Sum8").CreateGenericInvoker<int, int, int, int, int, int, int, int, int,
                                                          int, int, int, int, int, int, int, int>();

            Assert.Equal(36, f.Invoke(1, 2, 3, 4, 5, 6, 7, 8));
        }

        [Fact]
        public void MethodBase_Sum16_Int()
        {
            // 16-arg overload: R + U1..U16 + P1..P16 = 33 type arguments
            var f = M("Sum16").CreateGenericInvoker<int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int,
                                                              int, int, int, int, int, int, int, int, int, int, int, int, int, int, int, int>();

            Assert.Equal(136, f.Invoke(1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16));
        }

        // =====================================================================
        // Type + method-name entry point
        // =====================================================================

        [Fact]
        public void TypeAndName_Echo1_Int()
        {
            var f = typeof(GHost).CreateGenericInvoker<int, int, int>("Echo");

            Assert.Equal(42, f.Invoke(42));
        }

        [Fact]
        public void TypeAndName_Sum2_Int()
        {
            // NOTE: currently HANGS - FindOverload infinite-loops on open-generic
            // constraints via the Type+name entry point (IsGenericParameterCompatiable
            // rejects `where T : IAdditionOperators<T,T,T>`, then
            // `goto BeginStrictCheckMethod` retries the same method forever).
            // Kept in its faithful form on purpose: the session timeout caps the
            // damage and run-tests.ps1 lists this test in its MISS report, which
            // is exactly what should guide the FindOverload fix. Once fixed, this
            // test passes unchanged.
            var f = typeof(GHost).CreateGenericInvoker<int, int, int, int, int>("Sum2");

            Assert.Equal(30, f.Invoke(10, 20));
        }

        // =====================================================================
        // explicit genericTypes (inference skipped)
        // =====================================================================

        [Fact]
        public void ExplicitGenericTypes_Echo1_Long()
        {
            // explicit long instantiation; U1/P1 still describe the call shape
            var f = M("Echo").CreateGenericInvoker<long, long, long>(new[] { typeof(long) });

            Assert.Equal(123456789012345L, f.Invoke(123456789012345L));
        }

        // =====================================================================
        // R=ByRefParam: generic method with a ref return
        // =====================================================================

        [Fact]
        public void RefReturn_FirstOf_WriteThrough()
        {
            // FirstOf<T>(T[] arr) -> UnifiedRefFunc<ByRefParam, int[]>; the ref return
            // comes back as a ByRefParam view of arr[0], restored with ToRef<T>.
            var f = M("FirstOf").CreateGenericInvoker<ByRefParam, int[], int[]>();

            int[] arr = { 7, 8, 9 };
            ref ByRefParam view = ref f(out ByRefParam r, in arr);
            ref int first = ref view.ToRef<int>();

            Assert.Equal(7, first);

            first = 99;                                   // write through to arr[0]
            Assert.Equal(99, arr[0]);
        }

        // =====================================================================
        // R=VoidReturn: generic void method
        // =====================================================================

        [Fact]
        public void VoidReturn_Drop_CompletesWithoutError()
        {
            var f = M("Drop").CreateGenericInvoker<VoidReturn, int, int>();

            var r = f.Invoke(42);                         // must not throw
            Assert.Null(r);                               // VoidReturn slot stays default
        }

        // =====================================================================
        // instance generic methods (CreateGenericInstanceInvoker)
        // =====================================================================

        [Fact]
        public void Instance_AddBase_Int()
        {
            // U1/P1 pair is the target (receiver type is P1); U2/P2 onward are the
            // method's parameters. The returned UnifiedRefFunc has the target
            // baked in (the P1 slot is consumed by the wrapper).
            // NOTE: currently HANGS via the same FindOverload bug as
            // TypeAndName_Sum2_Int - see the note there.
            var target = new GInst(40);
            var f = target.CreateGenericInstanceInvoker<int, GInst, int, GInst, int>("AddBase");

            Assert.Equal(42, f.Invoke(2));
            Assert.Equal(50, f.Invoke(10));
        }

        // =====================================================================
        // failure modes
        // =====================================================================

        [Fact]
        public void Error_WrongGenericArgumentCount_Throws()
        {
            // Echo has one generic parameter; an empty explicit array must fail
            // inside MakeGenericMethod.
            Assert.ThrowsAny<ArgumentException>(
                () => M("Echo").CreateGenericInvoker<int, int, int>(Array.Empty<Type>()));
        }

        [Fact]
        public void Error_NoSuchMethod_Throws()
        {
            // Type+name entry: unknown name -> FindOverload yields null -> the
            // MethodBase entry dereferences it. Lock the actual exception shape.
            Assert.ThrowsAny<Exception>(
                () => typeof(GHost).CreateGenericInvoker<int, int, int>("NoSuchMethod"));
        }

        // =====================================================================
        // ByRefParam shapes (migrated from TestScript.cs "Generic Invokers"):
        // value-type `this` carried via U=ByRefParam, inference from the
        // method's own generic parameter
        // =====================================================================

        private struct ByRefHostStruct
        {
            public T TestGeneric<T>(T t) => t;
        }
        private struct ByRefHostStruct<T>
        {
            public T Field0;
            public ByRefHostStruct(T t) { Field0 = t; }
            public T TestGeneric<U>(U u)
            {
                // reads the U argument's address as a T
                return RawRef.Of(ref u).GetRef<T>();
            }
        }

        [Fact]
        public void ByRefParam_GenericInvoker_ValueTypeThis()
        {
            // U1=ByRefParam: the struct `this` is passed by address; T is inferred
            // from the method argument (IntPtr)
            var invoker = typeof(ByRefHostStruct).CreateGenericInvoker<IntPtr, ByRefParam, IntPtr, ByRefHostStruct, IntPtr>(nameof(ByRefHostStruct.TestGeneric));
            Assert.Equal((IntPtr)100, invoker.Invoke(default, (IntPtr)100));
        }

        [Fact]
        public void ByRefParam_GenericInvoker_GenericClassThis_SmallCarrier()
        {
            // P1=byte: the `this` carrier only needs to be an addressable slot -
            // the method never touches the receiver, so a 1-byte carrier is fine
            // (size-compatible through the lenient stage)
            var invoker = typeof(ByRefHostStruct<long>).CreateGenericInvoker<long, ByRefParam, IntPtr, byte, IntPtr>(nameof(ByRefHostStruct<long>.TestGeneric));
            Assert.Equal(111L, invoker.Invoke(default, (IntPtr)111));
        }
    }
}
