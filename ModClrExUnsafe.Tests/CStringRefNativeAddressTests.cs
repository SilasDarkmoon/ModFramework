using Mod;
using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Is a CStringRef safe when its RawTrackingRef carries a NATIVE address?
    ///
    /// <c>CStringRef._Address</c> is a <c>RawTrackingRef&lt;byte&gt;</c>: the byref lives in a slot
    /// on the dedicated tracking thread's stack, and the GC follows it as an interior byref (mark
    /// keeps the owner alive, relocate moves it with compaction). A native address
    /// (Marshal.AllocHGlobal) lies in no GC heap range, so the GC skips it: no crash, and the
    /// address stays valid for as long as the block is alive. These tests pin down that statement
    /// under forced full collections.
    ///
    /// Like every other test in this project they dispose the view before returning: the
    /// RawTrackingRef slot is a lease and Dispose returns it - see CStringRefTestUtil.
    /// </summary>
    public class CStringRefNativeAddressTests
    {
        private const int Capacity = 32;

        /// <summary>Allocates a zero-filled unmanaged block holding the given ASCII text.</summary>
        private static void WithUnmanaged(string text, Action<IntPtr> body, int capacity = Capacity)
        {
            Assert.True(text.Length <= capacity, "text must fit in the block");
            var ptr = Marshal.AllocHGlobal(capacity);
            try
            {
                // AllocHGlobal does not zero the block, so clear it before scanning for a NUL.
                var initial = new byte[capacity];
                Encoding.ASCII.GetBytes(text, 0, text.Length, initial, 0);
                Marshal.Copy(initial, 0, ptr, capacity);
                body(ptr);
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        private static void ForceFullCollection()
        {
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
            GC.WaitForPendingFinalizers();
            GC.Collect(2, GCCollectionMode.Forced, blocking: true, compacting: true);
        }

        // ---------- forced collections ----------

        [Fact]
        public void SpanCtor_SurvivesForcedFullCollection()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                ForceFullCollection();

                // A managed-backed CStringRef dies here; the native one must not.
                Assert.Equal("hello", s.ToString());
                Assert.Equal(5, s.Length);

                s.Dispose();
            });
        }

        [Fact]
        public void IntPtrCtor_SurvivesForcedFullCollection()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(ptr, Capacity, 5);

                ForceFullCollection();

                Assert.Equal("hello", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void ReadOnlySpanCtor_SurvivesForcedFullCollection()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsReadOnlySpan<byte>(ptr, Capacity));

                ForceFullCollection();


                Assert.True(s.Readonly);
                Assert.Equal("hello", s.ToString());
                s.Dispose();
            });
        }

        [Fact]
        public void SurvivesRepeatedCollections()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                for (int i = 0; i < 5; ++i)
                {
                    ForceFullCollection();
                    Assert.Equal("hello", s.ToString());
                    Assert.Equal(0, s.IndexOf((byte)'h'));
                    Assert.Equal(4, s.LastIndexOf((byte)'o'));
                }

                s.Dispose();
            });
        }

        [Fact]
        public void AddressIsStableAcrossCollections()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                var before = UnsafeEx.GetAddress(s.ToSpan());

                ForceFullCollection();

                var after = UnsafeEx.GetAddress(s.ToSpan());
                Assert.Equal(ptr, before);
                Assert.Equal(before, after);

                s.Dispose();
            });
        }

        [Fact]
        public void RemainsWritableAfterCollection()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                ForceFullCollection();

                // Mutations must still reach the native block after the GC ran.
                s[0] = (byte)'H';
                s.Ref[4] = (byte)'O';
                Assert.Equal((byte)'H', Marshal.ReadByte(ptr, 0));
                Assert.Equal((byte)'O', Marshal.ReadByte(ptr, 4));
                Assert.Equal("HellO", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void SurvivesBoxingAndCollection()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                // Boxing puts the struct on the GC heap, so the GC definitely scans the
                // RefContainer inside it. Safe only because the carried address is native.
                object boxed = s;

                ForceFullCollection();

                Assert.Equal("hello", ((CStringRef)boxed).ToString());

                boxed = null;
                s.Dispose();
            });
        }

        [Fact]
        public void SurvivesHeavyAllocationAndNaturalCollections()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                // Trigger natural collections without ever calling GC.Collect.
                for (int i = 0; i < 4000; ++i)
                {
                    var junk = new byte[64 * 1024];
                    junk[0] = (byte)i;
                    GC.KeepAlive(junk);
                }

                Assert.Equal("hello", s.ToString());
                Assert.Equal((byte)'h', Marshal.ReadByte(ptr, 0));

                s.Dispose();
            });
        }

        [Fact]
        public void LongLivedAcrossManyCollectionsAndMutations()
        {
            WithUnmanaged("abcdef", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                var expected = new StringBuilder("abcdef");

                for (int round = 0; round < 4; ++round)
                {
                    ForceFullCollection();

                    s.Replace((byte)('a' + round), (byte)('z' - round));
                    expected[round] = (char)('z' - round);

                    Assert.Equal(expected.ToString(), s.ToString());
                    Assert.Equal(ptr, UnsafeEx.GetAddress(s.ToSpan()));
                }

                s.Dispose();
            });
        }
    }
}
