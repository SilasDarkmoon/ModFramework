using Mod.LowLevel;
using System;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Tests for ConvertEx.cs: the reference reinterpretation family.
    ///
    /// <para>
    /// All of these APIs reinterpret memory without any type check - a wrong layout
    /// silently corrupts. The tests therefore use deliberately layout-matched fixture
    /// types and assert both values and (where applicable) the write-through aliasing.
    /// </para>
    /// </summary>
    public class ConvertExTests
    {
        // layout-matched fixtures
        private struct StructA { public int X; public int Y; }
        private struct StructB { public int X; public int Y; }
        private class LayoutA { public int X = 11; public int Y = 22; public LayoutA() { } }
        private class LayoutB { public int X; public int Y; }
        private class BigLayout { public int X; public int Y; public int Z; }
        private class SmallLayout { public int X; }

        private class Box { public int X = 5; public int Y = 6; }

        // =====================================================================
        // ConvertRef / ConvertStructRef: same-address reinterpretation
        // =====================================================================

        [Fact]
        public void ConvertRefCommon_ReinterpretsAndAliases()
        {
            var a = new StructA { X = 1, Y = 2 };

            ref StructB b = ref UnsafeEx.ConvertRef<StructA, StructB>(ref a);

            Assert.Equal(1, b.X);
            Assert.Equal(2, b.Y);

            b.Y = 99;                                   // writes through to the same storage
            Assert.Equal(99, a.Y);
        }

        [Fact]
        public void ConvertStructRef_MatchesCommon()
        {
            var a = new StructA { X = 3, Y = 4 };

            ref StructB b = ref a.ConvertStructRef<StructA, StructB>();

            Assert.Equal(3, b.X);
            Assert.Equal(4, b.Y);
        }

        // =====================================================================
        // ConvertObjectTypeForcibly: read the object slot as another reference type
        // =====================================================================

        [Fact]
        public void ConvertObjectRef_ReinterpretsReference()
        {
            var list = new System.Collections.Generic.List<int> { 1, 2, 3 };
            object o = list;                            // the slot now holds the List reference

            var back = o.ConvertObjectTypeForcibly<System.Collections.Generic.List<int>>();

            Assert.Same(list, back);                    // same reference, re-typed
        }

        // =====================================================================
        // ConvertPretend pair - NOT tested here:
        //
        // ConvertPretend<F, T>(ref F) with T = object crashes the process on .NET 10:
        // RawRef.GetRefObj() lowers to CastHelpers.ChkCastClassSpecial, which dereferences
        // the fake reference (a stack struct address) as a MethodTable -> AccessViolation.
        // The fake-reference trick does not survive the runtime's cast checks; the
        // reverse direction (object -> ref T) yields a header-including view that has no
        // stable assertion. Both directions are left to their intended runtimes.
        // =====================================================================

        // =====================================================================
        // ConvertToInnerRef / ConvertToOuterObject: field <-> container (inverse pair)
        // =====================================================================

        [Fact]
        public void ConvertToInnerRef_PointsAtFirstField()
        {
            var box = new Box();

            ref int x = ref box.ConvertToInnerRef<int>();

            Assert.Equal(5, x);
            x = 55;                                     // writes the first field through the header
            Assert.Equal(55, box.X);
            Assert.Equal(6, box.Y);                     // untouched
        }

        [Fact]
        public void ConvertToOuterObject_FromFieldRefRecoversContainer()
        {
            var box = new Box();
            ref int fieldRef = ref box.X;               // a ref pointing at the first field

            var container = UnsafeEx.ConvertToOuterObject<int, Box>(ref fieldRef);

            Assert.Same(box, container);                // header subtracted, container recovered
        }

        [Fact]
        public void ConvertToOuterObject_GcCompactionStress()
        {
            // Reproduces the intermittent failure of FromFieldRefRecoversContainer:
            // the double-read retry loop in ConvertToOuterObject must survive
            // objects being MOVED by compacting GCs while the interior pointer
            // is being converted. Churn + forced compacting collections run in
            // parallel with a tight conversion loop; any mismatch dumps the
            // iteration number and both addresses.
            int stop = 0;
            var failures = new System.Collections.Concurrent.ConcurrentQueue<string>();

            var churn = System.Threading.Tasks.Task.Run(() =>
            {
                while (System.Threading.Volatile.Read(ref stop) == 0)
                {
                    var arr = new byte[64 * 1024];      // SOH storm -> frequent gen0/1 GCs
                    arr[0] = 1;
                }
            });
            var compact = System.Threading.Tasks.Task.Run(() =>
            {
                while (System.Threading.Volatile.Read(ref stop) == 0)
                {
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    System.Threading.Thread.Sleep(5);
                }
            });

            try
            {
                for (int i = 0; i < 200_000; ++i)
                {
                    var box = new Box();
                    ref int fieldRef = ref box.X;
                    var container = UnsafeEx.ConvertToOuterObject<int, Box>(ref fieldRef);
                    if (!ReferenceEquals(box, container))
                    {
                        var boxAddr = RawRef.Of(box).Address;
                        var containerAddr = RawRef.Of(container).Address;
                        failures.Enqueue(
                            $"iter {i}: mismatch - box@0x{boxAddr:X} (gen {GC.GetGeneration(box)}), " +
                            $"container@0x{containerAddr:X} (type {container.GetType().Name}, " +
                            $"gen {GC.GetGeneration(container)})");
                    }
                }
            }
            finally
            {
                System.Threading.Volatile.Write(ref stop, 1);
                System.Threading.Tasks.Task.WaitAll(churn, compact);
            }

            Assert.Empty(failures);
        }

        [Fact]
        public void ConvertPretend_ObjectToRefByte_BackToObject_GcCompactionStress()
        {
            // Round-trip under compaction pressure: object -> ref byte (the
            // byref-returning ConvertPretend, GC-tracked so it survives moves),
            // then ref byte -> object again (the ref-taking ConvertPretend with
            // its double-read loop). The recovered "object" is the field address
            // reinterpreted as a Box reference - its X must still read the live
            // value, proving the byref (not a stale snapshot) was used.
            int stop = 0;
            var failures = new System.Collections.Concurrent.ConcurrentQueue<string>();

            var churn = System.Threading.Tasks.Task.Run(() =>
            {
                while (System.Threading.Volatile.Read(ref stop) == 0)
                {
                    var arr = new byte[64 * 1024];
                    arr[0] = 1;
                }
            });
            var compact = System.Threading.Tasks.Task.Run(() =>
            {
                while (System.Threading.Volatile.Read(ref stop) == 0)
                {
                    GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
                    System.Threading.Thread.Sleep(5);
                }
            });

            try
            {
                for (int i = 0; i < 200_000; ++i)
                {
                    var box = new Box { X = 123, Y = 45 };
                    // 1) object -> ref byte: a GC-tracked byref into the first field
                    ref byte rb = ref UnsafeEx.ConvertPretend<byte>(box);
                    // 2) ref byte -> object: the double-read ConvertPretend
                    var recovered = UnsafeEx.ConvertPretend<byte, Box>(ref rb);
                    // the recovered reference points AT the field slot, so reading
                    // X through it must observe the live (possibly moved) box
                    if (recovered == null || recovered.X != 123 || recovered.Y != 45)
                    {
                        failures.Enqueue($"iter {i}: recovered.X={recovered?.X ?? -1}, recovered.Y={recovered?.Y ?? -1}");
                    }
                }
            }
            finally
            {
                System.Threading.Volatile.Write(ref stop, 1);
                System.Threading.Tasks.Task.WaitAll(churn, compact);
            }

            Assert.Empty(failures);
        }

        // =====================================================================
        // ConvertedClone family: cross-type memory clones
        // =====================================================================

        [Fact]
        public void ConvertedClone_ObjectToType_CopiesLayout()
        {
            var src = new LayoutA();

            var clone = (LayoutB)src.ConvertedClone(typeof(LayoutB));

            Assert.IsType<LayoutB>(clone);
            Assert.Equal(11, clone.X);
            Assert.Equal(22, clone.Y);
            Assert.NotSame(src, clone);
        }

        [Fact]
        public void ConvertedClone_TypedObjectOverload()
        {
            var src = new LayoutA();

            LayoutB clone = src.ConvertedClone<LayoutA, LayoutB>();

            Assert.Equal(11, clone.X);
            Assert.Equal(22, clone.Y);
        }

        [Fact]
        public void ConvertedClone_ObjectGenericOverload()
        {
            object src = new LayoutA();

            LayoutB clone = src.ConvertedClone<LayoutB>();

            Assert.IsType<LayoutB>(clone);
            Assert.Equal(11, clone.X);
        }

        [Fact]
        public void ConvertedClone_StructByValue_CopiesValue()
        {
            var src = new StructA { X = 31, Y = 41 };

            // plain static call: the receiver syntax is ambiguous between the
            // by-value and by-ref overloads for struct receivers.
            StructB clone = UnsafeEx.ConvertedClone<StructA, StructB>(src);

            Assert.Equal(31, clone.X);
            Assert.Equal(41, clone.Y);
            Assert.Equal(31, src.X);                    // source untouched
        }

        [Fact]
        public void ConvertedClone_TypeIndicatorOverload()
        {
            var src = new StructA { X = 5, Y = 6 };

            StructB clone = UnsafeEx.ConvertedClone<StructA, StructB>(src, TypeIndicator.Of<StructB>());

            Assert.Equal(5, clone.X);
            Assert.Equal(6, clone.Y);
        }

        [Fact]
        public void ConvertedClone_SmallSourceIntoBigTarget_PadsWithZero()
        {
            // Size(BigLayout) > Size(LayoutA): only Size(src) bytes are copied; the rest of
            // the uninitialized target stays zero.
            var src = new LayoutA();

            BigLayout clone = src.ConvertedClone<LayoutA, BigLayout>();

            Assert.Equal(11, clone.X);
            Assert.Equal(22, clone.Y);
            Assert.Equal(0, clone.Z);                  // beyond the copied prefix: still zero
        }

        [Fact]
        public void ConvertedClone_BigSourceIntoSmallTarget_Truncates()
        {
            var src = new BigLayout { X = 1, Y = 2, Z = 3 };

            SmallLayout clone = src.ConvertedClone<BigLayout, SmallLayout>();

            Assert.Equal(1, clone.X);                  // prefix copied
        }
    }
}
