using Mod.LowLevel;
using System;
using System.Reflection;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for ObjectEx.cs: uninitialized object creation (constructor skipped),
    /// MemberwiseClone (bound to the native Object.MemberwiseClone), AllocatedObject,
    /// and the Constructor/ConstructorToken pipeline (allocate uninitialized, then run
    /// the real constructor through a delegate that takes the token as its "this").
    /// </summary>
    public class ObjectExTests
    {
        // =====================================================================
        // GetUninitializedObject family
        // =====================================================================

        private class Initialized
        {
            public int A = 111;
            public string S = "set";
            public Initialized() { A = 222; }
        }

        [Fact]
        public void GetUninitializedObject_SkipsConstructorAndFieldInitializers()
        {
            var obj = (Initialized)typeof(Initialized).GetUninitializedObject();

            Assert.NotNull(obj);
            Assert.Equal(0, obj.A);                    // neither the initializer nor the ctor ran
            Assert.Null(obj.S);
        }

        [Fact]
        public void GetUninitialized_Generic_Class()
        {
            var obj = UnsafeEx.GetUninitialized<Initialized>();
            Assert.Equal(0, obj.A);
            Assert.Null(obj.S);
        }

        [Fact]
        public void GetUninitialized_Generic_Struct_ReturnsDefault()
        {
            var s = UnsafeEx.GetUninitialized<Initialized>();      // class path covered above
            Assert.Null(s.S);

            int i = UnsafeEx.GetUninitialized<int>();              // value type: default
            Assert.Equal(0, i);
        }

        [Fact]
        public void GetUninitializedStruct_ReturnsDefault()
        {
            var s = UnsafeEx.GetUninitializedStruct<DateTime>();
            Assert.Equal(default(DateTime), s);
        }

        [Fact]
        public void GetUninitializedObject_TypedClass()
        {
            var obj = UnsafeEx.GetUninitializedObject<Initialized>();
            Assert.IsType<Initialized>(obj);
            Assert.Equal(0, obj.A);
        }

        [Fact]
        public void GetUninitializedObject_DelegateType_ProducesEmptyDelegate()
        {
            // The delegate branch clones a template and rewrites the method table; the
            // result is a zero-field delegate. Only its shape is asserted here - invoking
            // a zeroed delegate is undefined, so it is NOT called.
            var del = typeof(Action).GetUninitializedObject();

            Assert.IsType<Action>(del);
            Assert.Null(((Delegate)del).Target);
        }

        // =====================================================================
        // MemberwiseClone
        // =====================================================================

        private class Cloneable
        {
            public int Value = 5;
            public object Ref = new object();
            public object NativeClone() => base.MemberwiseClone();   // expose the protected method
        }

        [Fact]
        public void MemberwiseClone_MatchesNativeSemantics()
        {
            var src = new Cloneable();

            var ours = (Cloneable)src.MemberwiseClone();
            var native = (Cloneable)src.NativeClone();

            Assert.NotSame(src, ours);
            Assert.Equal(src.Value, ours.Value);
            Assert.Same(src.Ref, ours.Ref);                 // shallow: references shared

            Assert.Equal(native.Value, ours.Value);         // identical to the native clone
            Assert.Same(native.Ref, ours.Ref);
        }

        // =====================================================================
        // AllocatedObject<T>
        // =====================================================================

        [Fact]
        public void AllocatedObject_ImplicitConversions_RoundTrip()
        {
            var payload = "hello";
            UnsafeEx.AllocatedObject<string> box = payload;          // T -> AllocatedObject<T>
            string back = box;                                      // AllocatedObject<T> -> T
            Assert.Same(payload, back);
            Assert.Same(payload, box.Value);
        }

        // =====================================================================
        // Constructor / ConstructorToken pipeline
        // =====================================================================

        private class Point
        {
            public int X, Y;
            public Point(int x, int y) { X = x; Y = y; }
            public Point() { X = 1; Y = 2; }
        }

        private struct PointStruct
        {
            public int X, Y;
            public PointStruct(int x, int y) { X = x; Y = y; }
        }

        public delegate ref UnsafeEx.ConstructorToken PointCtorDel(ref UnsafeEx.ConstructorToken token, int x, int y);
        public delegate ref UnsafeEx.ConstructorToken PointDefaultCtorDel(ref UnsafeEx.ConstructorToken token);
        public delegate int BadCtorDel(int x);

        [Fact]
        public void Constructor_Find_ByParamTypes()
        {
            var con = UnsafeEx.Constructor.Find(typeof(Point), new[] { typeof(int), typeof(int) });
            Assert.NotNull(con);
            Assert.Equal(2, con.GetParameters().Length);
        }

        [Fact]
        public void Constructor_Find_ByDelegateType_StripsTokenParameter()
        {
            // The delegate's leading 'ref ConstructorToken' is the "this" slot; the rest
            // of the signature is matched against the constructor parameters.
            var con = UnsafeEx.Constructor.Find(typeof(Point), typeof(PointCtorDel));
            Assert.NotNull(con);
            Assert.Equal(2, con.GetParameters().Length);

            var conDefault = UnsafeEx.Constructor.Find(typeof(Point), typeof(PointDefaultCtorDel));
            Assert.NotNull(conDefault);
            Assert.Equal(0, conDefault.GetParameters().Length);
        }

        [Fact]
        public void Constructor_CheckArguments_RejectsNonTokenDelegate()
        {
            var con = UnsafeEx.Constructor.Find(typeof(Point), new[] { typeof(int), typeof(int) });
            Assert.NotNull(con);

            Assert.Throws<ArgumentException>(() => new UnsafeEx.Constructor(con, typeof(BadCtorDel)));
        }

        [Fact]
        public void Constructor_AllocInit_ReferenceType()
        {
            var ctor = new UnsafeEx.Constructor(
                UnsafeEx.Constructor.Find(typeof(Point), typeof(PointCtorDel)), typeof(PointCtorDel));

            ref var token = ref ctor.Alloc(out object obj);

            Assert.IsType<Point>(obj);
            Assert.Equal(0, ((Point)obj).X);               // allocated uninitialized
            Assert.Equal(0, ((Point)obj).Y);

            ctor.Init<PointCtorDel>()(ref token, 3, 4);    // run the real constructor

            Assert.Equal(3, ((Point)obj).X);
            Assert.Equal(4, ((Point)obj).Y);
        }

        [Fact]
        public void Constructor_AllocInit_ValueType()
        {
            var ctor = new UnsafeEx.Constructor(
                UnsafeEx.Constructor.Find(typeof(PointStruct), typeof(PointCtorDel)), typeof(PointCtorDel));

            ref var token = ref ctor.Alloc(out PointStruct val);

            Assert.Equal(0, val.X);                        // uninitialized on the stack

            ctor.Init<PointCtorDel>()(ref token, 7, 8);

            Assert.Equal(7, val.X);                        // constructor wrote through the token
            Assert.Equal(8, val.Y);
        }

        [Fact]
        public void Constructor_AllocWrapped_ReferenceType()
        {
            var ctor = new UnsafeEx.Constructor(
                UnsafeEx.Constructor.Find(typeof(Point), typeof(PointCtorDel)), typeof(PointCtorDel));

            ref var token = ref ctor.AllocWrapped(out UnsafeEx.AllocatedObject<Point> box);

            Assert.NotNull(box.Value);
            Assert.Equal(0, box.Value.X);

            ctor.Init<PointCtorDel>()(ref token, 10, 20);

            Assert.Equal(10, box.Value.X);
            Assert.Equal(20, box.Value.Y);
        }

        [Fact]
        public void Constructor_TypedGeneric_Pipeline()
        {
            var ctor = new UnsafeEx.Constructor<Point, PointCtorDel>();

            ref var token = ref ctor.Alloc(out Point p);

            Assert.Equal(0, p.X);
            ctor.Init(ref token, 100, 200);
            Assert.Equal(100, p.X);
            Assert.Equal(200, p.Y);
        }
    }
}
