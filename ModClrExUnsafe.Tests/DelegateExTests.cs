using Mod.LowLevel;
using System;
using System.Collections.Generic;
using System.Reflection;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for <see cref="UnsafeEx"/> delegate utilities in DelegateEx.cs.
    ///
    /// <para>
    /// "Unsafe" in CreateDelegateUnsafe means the official Delegate.CreateDelegate signature
    /// validation is bypassed: any delegate type can bind any method as long as the call is
    /// ABI-compatible at runtime. These tests lock both the bypass behaviour and the
    /// reflection helpers (FindOverload / GuessGenericParameters / MakeGenericMethod) that
    /// usually feed into it.
    /// </para>
    /// </summary>
    public class DelegateExTests
    {
        // ---------- CreateDelegateUnsafe: basics ----------

        [Fact]
        public void CreateDelegateUnsafe_StaticMethod_InvokesCorrectly()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Square), BindingFlags.Public | BindingFlags.Static);
            var del = (Func<int, int>)UnsafeEx.CreateDelegateUnsafe(typeof(Func<int, int>), mi);
            Assert.Equal(36, del(6));
        }

        [Fact]
        public void CreateDelegateUnsafe_InstanceMethod_BindsTarget()
        {
            var target = new Helper { X = 7 };
            var mi = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del = (Func<int>)UnsafeEx.CreateDelegateUnsafe(typeof(Func<int>), target, mi);
            Assert.Equal(7, del());
        }

        [Fact]
        public void CreateDelegateUnsafe_GenericExtension_InvokesCorrectly()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Square), BindingFlags.Public | BindingFlags.Static);
            var del = mi.CreateDelegateUnsafe<Func<int, int>>();
            Assert.Equal(49, del(7));

            var target = new Helper { X = 3 };
            var mi2 = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del2 = target.CreateDelegateUnsafe<Func<int>>(mi2);
            Assert.Equal(3, del2());
        }

        // ---------- CreateDelegateUnsafe: the signature-validation bypass ----------

        [Fact]
        public void CreateDelegateUnsafe_BypassesSignatureValidation_ReferenceTypes()
        {
            // Func<object, object> over EchoString(string): the official API rejects the
            // parameter mismatch; the unsafe path binds it (x64: all references are pointers).
            var mi = typeof(Helper).GetMethod(nameof(Helper.EchoString), BindingFlags.Public | BindingFlags.Static);

            Assert.Throws<ArgumentException>(() => Delegate.CreateDelegate(typeof(Func<object, object>), mi));

            var del = (Func<object, object>)UnsafeEx.CreateDelegateUnsafe(typeof(Func<object, object>), mi);
            Assert.Equal("abc", del("abc"));
        }

        [Fact]
        public void CreateDelegateUnsafe_BypassesSignatureValidation_ExtraArguments()
        {
            // Func<int, int, int> over Square(int): binding succeeds (no signature check),
            // but the unsafe path installs no argument-shuffle thunk, so a delegate with a
            // different arity misaligns registers (Square reads the target slot, not arg1).
            // Lock only "binds and does not crash"; the returned value is undefined ABI noise.
            var mi = typeof(Helper).GetMethod(nameof(Helper.Square), BindingFlags.Public | BindingFlags.Static);

            Assert.Throws<ArgumentException>(() => Delegate.CreateDelegate(typeof(Func<int, int, int>), mi));

            var del = (Func<int, int, int>)UnsafeEx.CreateDelegateUnsafe(typeof(Func<int, int, int>), mi);
            var _ = del(3, 99);          // must not crash; value is undefined
        }

        // ---------- ChangeTarget ----------

        [Fact]
        public void ChangeTarget_RetargetsInstanceMethod()
        {
            var a = new Helper { X = 1 };
            var b = new Helper { X = 2 };
            var mi = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del = (Func<int>)UnsafeEx.CreateDelegateUnsafe(typeof(Func<int>), a, mi);
            Assert.Equal(1, del());

            var changed = (Func<int>)UnsafeEx.ChangeTarget(del, b);
            Assert.Equal(2, changed());
        }

        [Fact]
        public void ChangeTarget_GenericExtension_Retargets()
        {
            var a = new Helper { X = 10 };
            var b = new Helper { X = 20 };
            var mi = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del = a.CreateDelegateUnsafe<Func<int>>(mi);

            var changed = del.ChangeTarget<Func<int>>(b);
            Assert.Equal(20, changed());
        }

        // ---------- GetTargetRef ----------

        [Fact]
        public void GetTargetRef_ReadsAndWritesTarget()
        {
            var a = new Helper { X = 5 };
            var b = new Helper { X = 6 };
            var mi = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del = a.CreateDelegateUnsafe<Func<int>>(mi);

            ref object target = ref del.GetTargetRef();
            Assert.Same(a, target);

            target = b;                      // write through the ref swaps the live target
            Assert.Equal(6, del());
        }

        // ---------- ChangeDelegateType ----------

        [Fact]
        public void ChangeDelegateType_SameSignatureDifferentDelegateType()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Square), BindingFlags.Public | BindingFlags.Static);
            var del = mi.CreateDelegateUnsafe<Func<int, int>>();

            var converted = (MyIntToInt)UnsafeEx.ChangeDelegateType(del, typeof(MyIntToInt));
            Assert.Equal(64, converted(8));
        }

        [Fact]
        public void ChangeDelegateType_GenericExtension_Invokes()
        {
            var target = new Helper { X = 11 };
            var mi = typeof(Helper).GetMethod(nameof(Helper.GetX), BindingFlags.Public | BindingFlags.Instance);
            var del = target.CreateDelegateUnsafe<Func<int>>(mi);

            var converted = del.ChangeDelegateType<MyIntGetter>();
            Assert.Equal(11, converted());
        }

        // ---------- FindOverload ----------

        [Fact]
        public void FindOverload_StrictParameterMatch()
        {
            var overloads = typeof(Helper).GetMethods(nameof(Helper.Over));
            var found = overloads.FindOverload(new[] { typeof(int) }, typeof(int));
            Assert.NotNull(found);
            found.Invoke(null, new object[] { 5 });
            Assert.Equal(1, Helper.LastOverResult);       // Over(int)
        }

        [Fact]
        public void FindOverload_ReturnTypeFilters()
        {
            var overloads = typeof(Helper).GetMethods(nameof(Helper.Over));

            var found = overloads.FindOverload(new[] { typeof(long) }, typeof(long));
            Assert.NotNull(found);
            found.Invoke(null, new object[] { 5L });
            Assert.Equal(3, Helper.LastOverResult);      // Over(long) -> long

            var none = overloads.FindOverload(new[] { typeof(int) }, typeof(string));
            Assert.Null(none);                            // no (int) -> string overload
        }

        [Fact]
        public void FindOverload_FallsBackToRawSizeCompatible()
        {
            // float strict-matches nothing (no float overload, no reference compatibility),
            // but has the same 4-byte raw size as int/uint, so the public-compatible stage
            // picks the first size-matching overload.
            var overloads = typeof(Helper).GetMethods(nameof(Helper.Over));
            var found = overloads.FindOverload(new[] { typeof(float) });
            Assert.NotNull(found);
            Assert.Equal(typeof(int), found.GetParameters()[0].ParameterType);
        }

        [Fact]
        public void FindOverload_WithThis_PrependsInstanceType()
        {
            var overloads = typeof(Helper).GetMethods(nameof(Helper.Add));
            var found = overloads.FindOverload(new[] { typeof(Helper), typeof(int), typeof(int) }, typeof(int), withThis: true);
            Assert.NotNull(found);
            Assert.False(found.IsStatic);

            var instance = new Helper();
            Assert.Equal(9, (int)found.Invoke(instance, new object[] { 4, 5 }));
        }

        [Fact]
        public void FindOverload_GenericMethod_CheckGenericMatchesByConstraint()
        {
            var overloads = typeof(Helper).GetGenericMethods(nameof(Helper.Echo));
            var found = overloads.FindOverload(new[] { typeof(int) }, typeof(int), checkGeneric: true);
            Assert.NotNull(found);
            Assert.True(found.IsGenericMethodDefinition);

            var concrete = found.MakeGenericMethod(typeof(int));
            Assert.Equal(42, (int)concrete.Invoke(null, new object[] { 42 }));
        }

        [Fact]
        public void FindOverload_NoMatch_ReturnsNull()
        {
            // byte is 1 raw byte: no overload has that size (candidates are 4/4/8/8),
            // so both the strict and the size-compatible stages miss.
            var overloads = typeof(Helper).GetMethods(nameof(Helper.Over));
            Assert.Null(overloads.FindOverload(new[] { typeof(byte) }));
        }

        // ---------- FindOverload generic-parameter inference ----------
        // (replaces the removed GuessGenericParameters: one FindOverload call now
        //  does matching + inference via the genericParameters output)

        [Fact]
        public void FindOverload_Infers_FromParameter()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Echo));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(int) }, checkGeneric: true, genericParameters: inferred);
            Assert.Same(mi, found);
            Assert.Equal(new[] { typeof(int) }, inferred);
        }

        [Fact]
        public void FindOverload_Infers_FromNestedGenericParameter()
        {
            // First<T>(IList<T>) vs List<int>: the open-generic interface is
            // resolved through the implementing type (IList<int> on List<int>)
            var mi = typeof(Helper).GetMethod(nameof(Helper.First));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(List<int>) }, checkGeneric: true, genericParameters: inferred);
            Assert.Same(mi, found);
            Assert.Equal(new[] { typeof(int) }, inferred);
        }

        [Fact]
        public void FindOverload_Infers_MultipleParametersCrossReferenced()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.MakePair));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(int), typeof(string) }, checkGeneric: true, genericParameters: inferred);
            Assert.Same(mi, found);
            Assert.Equal(new[] { typeof(int), typeof(string) }, inferred);
        }

        [Fact]
        public void FindOverload_Infers_FromReturnType()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Create));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(Array.Empty<Type>(), returnType: typeof(string), checkGeneric: true, genericParameters: inferred);
            Assert.Same(mi, found);
            Assert.Equal(new[] { typeof(string) }, inferred);
        }

        [Fact]
        public void FindOverload_ContradictoryInference_SkipsOverload()
        {
            // Echo<T>(T a, T b) with (int, string): T cannot be both - the overload
            // is inapplicable instead of silently keeping the last inference
            var mi = typeof(Helper).GetMethod(nameof(Helper.Echo));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(int), typeof(string) }, checkGeneric: true, genericParameters: inferred);
            Assert.Null(found);
        }

        [Fact]
        public void FindOverload_ConstrainedGeneric_NoLongerHangs()
        {
            // regression for the `goto BeginStrictCheckMethod` infinite loop: a
            // constrained generic (where T : IComparable<T>) used to spin forever
            // when the constraint check failed on the open-generic constraint
            var mi = typeof(Helper).GetMethod(nameof(Helper.EchoComparable));
            var inferred = new List<Type>();
            var found = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(int) }, checkGeneric: true, genericParameters: inferred);
            Assert.Same(mi, found);                          // int satisfies IComparable<int>
            Assert.Equal(new[] { typeof(int) }, inferred);

            var rejected = new[] { (MethodBase)mi }.FindOverload(new[] { typeof(object) }, checkGeneric: true, genericParameters: new List<Type>());
            Assert.Null(rejected);                            // object : IComparable<object>? no
        }

        // ---------- MakeGenericMethod / GetMethods / GetGenericMethods ----------

        [Fact]
        public void MakeGenericMethod_InstantiatesAndInvokes()
        {
            var mi = typeof(Helper).GetMethod(nameof(Helper.Echo));
            // Call the extension explicitly: MethodInfo.MakeGenericMethod (the instance
            // method) shadows it for plain receiver syntax and throws on non-definitions.
            var concrete = UnsafeEx.MakeGenericMethod(mi, typeof(long));
            Assert.Equal(123L, (long)concrete.Invoke(null, new object[] { 123L }));

            // Non-generic methods pass through unchanged.
            var plain = typeof(Helper).GetMethod(nameof(Helper.Square));
            Assert.Same(plain, UnsafeEx.MakeGenericMethod(plain, typeof(int)));
        }

        [Fact]
        public void GetMethods_FindsAllOverloads()
        {
            var found = typeof(Helper).GetMethods(nameof(Helper.Over));
            Assert.Equal(5, found.Length);                 // Over(int)/Over(uint)/Over(long)/Over(string)/Over(int,int)
        }

        [Fact]
        public void GetGenericMethods_FiltersGenericOnly()
        {
            var byName = typeof(Helper).GetGenericMethods(nameof(Helper.Echo));
            Assert.Single(byName);
            Assert.True(byName[0].IsGenericMethod);

            var fromList = new MethodBase[] { typeof(Helper).GetMethod(nameof(Helper.Echo)), typeof(Helper).GetMethod(nameof(Helper.Square)) }
                .GetGenericMethods();
            Assert.Single(fromList);
        }

        // ---------- IsGenericParameterCompatiable ----------

        [Fact]
        public void IsGenericParameterCompatiable_ConstraintChecks()
        {
            var structT = typeof(Helper).GetMethod(nameof(Helper.EchoStruct)).GetGenericArguments()[0];
            Assert.True(UnsafeEx.IsGenericParameterCompatiable(structT, typeof(int)));
            Assert.False(UnsafeEx.IsGenericParameterCompatiable(structT, typeof(string)));

            var classT = typeof(Helper).GetMethod(nameof(Helper.EchoClass)).GetGenericArguments()[0];
            Assert.True(UnsafeEx.IsGenericParameterCompatiable(classT, typeof(string)));
            Assert.False(UnsafeEx.IsGenericParameterCompatiable(classT, typeof(int)));

            // Non-generic interface constraint resolves through IsAssignableFrom.
            var formattableT = typeof(Helper).GetMethod(nameof(Helper.EchoFormattable)).GetGenericArguments()[0];
            Assert.True(UnsafeEx.IsGenericParameterCompatiable(formattableT, typeof(int)));    // int : IFormattable
            Assert.False(UnsafeEx.IsGenericParameterCompatiable(formattableT, typeof(object)));

            // FIXED (was the DelegateEx.cs TODO): generic interface constraints
            // (where T : IComparable<T>) are now instantiated with the candidate
            // before the IsAssignableFrom check - int : IComparable<int> matches.
            var ifaceT = typeof(Helper).GetMethod(nameof(Helper.EchoComparable)).GetGenericArguments()[0];
            Assert.True(UnsafeEx.IsGenericParameterCompatiable(ifaceT, typeof(int)));
            Assert.False(UnsafeEx.IsGenericParameterCompatiable(ifaceT, typeof(object)));

            // open-generic interface pattern vs an implementing type: the closed
            // form is resolved through the interface list (IComparable<int> on int)
            var openIface = typeof(IComparable<>).MakeGenericType(ifaceT);   // IComparable<T> (still open)
            Assert.True(UnsafeEx.IsGenericParameterCompatiable(openIface, typeof(int)));
            Assert.False(UnsafeEx.IsGenericParameterCompatiable(openIface, typeof(object)));
        }

        // ---------- IsInInterpter ----------

        [Fact]
        public void IsInInterpter_OnNetCore_ReturnsTrue()
        {
            // Per implementation: JIT runtimes (NetCore/NetFramework/Mono) report true;
            // only NativeAOT reports false. Locks the dotnet-side behaviour.
            var mi = typeof(Helper).GetMethod(nameof(Helper.Square));
            Assert.True(UnsafeEx.IsInInterpter(mi));
        }

        // ---------- integration: the typical pipeline ----------

        [Fact]
        public void Integration_GenericOverloadDiscoveryToDelegateInvocation()
        {
            // The canonical pipeline this API family serves: find a generic overload
            // AND infer its parameters in one FindOverload call, instantiate, bind,
            // invoke (the old two-step FindOverload + GuessGenericParameters is gone).
            var overloads = typeof(Helper).GetGenericMethods(nameof(Helper.Echo));
            var inferred = new List<Type>();
            var found = overloads.FindOverload(new[] { typeof(int) }, typeof(int), checkGeneric: true, genericParameters: inferred);
            Assert.NotNull(found);
            Assert.Equal(new[] { typeof(int) }, inferred);

            var concrete = found.MakeGenericMethod(inferred.ToArray());

            var del = concrete.CreateDelegateUnsafe<Func<int, int>>();
            Assert.Equal(99, del(99));
        }

        // =====================================================================
        // test fixtures
        // =====================================================================

        private delegate int MyIntToInt(int x);
        private delegate int MyIntGetter();

        private class Helper
        {
            public int X;
            public static int LastOverResult;

            public static int Square(int x) => x * x;
            public static string EchoString(string s) => s;

            public int GetX() => X;
            public int Add(int a, int b) => a + b;

            // overload family for FindOverload
            public static int Over(int x) { LastOverResult = 1; return x; }
            public static int Over(uint x) { LastOverResult = 2; return (int)x; }
            public static long Over(long x) { LastOverResult = 3; return x; }
            public static string Over(string x) { LastOverResult = 4; return x; }
            public static int Over(int x, int y) { LastOverResult = 5; return x + y; }

            // generic family for Guess / MakeGenericMethod
            public static T Echo<T>(T x) => x;
            public static T First<T>(IList<T> list) => list[0];
            public static K MakePair<K, V>(K k, V v) => k;
            public static T Create<T>() => default;

            // constrained generics for IsGenericParameterCompatiable
            public static T EchoStruct<T>(T x) where T : struct => x;
            public static T EchoClass<T>(T x) where T : class => x;
            public static T EchoComparable<T>(T x) where T : IComparable<T> => x;
            public static T EchoFormattable<T>(T x) where T : IFormattable => x;
        }
    }
}
