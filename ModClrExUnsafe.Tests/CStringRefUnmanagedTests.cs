using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// CStringRef over memory obtained from Marshal.AllocHGlobal. Non-managed blocks are
    /// neither moved nor reclaimed by the GC, so this covers the IntPtr entry points as
    /// well as the Span/ReadOnlySpan entry points built on top of a raw pointer.
    ///
    /// Every view here is disposed with <c>Dispose()</c> before the test returns, exactly like the
    /// managed-backed views in the other files: a native address needs no GC protection, but the
    /// RawTrackingRef slot is a lease either way and Dispose returns it - see CStringRefTestUtil.
    /// </summary>
    public class CStringRefUnmanagedTests
    {
        private const int Capacity = 32;

        /// <summary>
        /// Allocates a zero-filled unmanaged block holding the given ASCII text, runs the
        /// body and always frees the block. The caller owns the memory, not CStringRef.
        /// </summary>
        private static void WithUnmanaged(string text, Action<IntPtr> body, int capacity = Capacity)
        {
            Assert.True(text.Length <= capacity, "text must fit in the block");
            var ptr = Marshal.AllocHGlobal(capacity);
            try
            {
                // AllocHGlobal does not zero the block, so clear it: StrLen must stop on a real NUL.
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

        private static byte At(IntPtr ptr, int offset)
        {
            return Marshal.ReadByte(ptr, offset);
        }

        // ---------- IntPtr constructors ----------

        [Fact]
        public void IntPtrCtor_WithExplicitLength_ReadsContent()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(ptr, Capacity, 5);
                Assert.Equal(5, s.Length);
                Assert.Equal(Capacity, s.Size);
                Assert.False(s.Readonly);
                Assert.Equal("hello", s.ToString());
                Assert.Equal((byte)'o', s[4]);

                s.Dispose();
            });
        }

        [Fact]
        public void IntPtrCtor_WithSize_ScansForNul()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(ptr, Capacity);
                Assert.Equal(5, s.Length);
                Assert.Equal(Capacity, s.Size);
                Assert.Equal("hello", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void IntPtrCtor_WithoutSize_UsesStrLenPlusNul()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(ptr);
                Assert.Equal(5, s.Length);
                Assert.Equal(6, s.Size);
                Assert.Equal("hello", s.ToString());

                s.Dispose();
            });
        }

        // ---------- Span over a raw pointer ----------

        [Fact]
        public void SpanOverPointer_ReadsContent()
        {
            WithUnmanaged("hello", ptr =>
            {
                var span = UnsafeEx.AsSpan<byte>(ptr, Capacity);
                var s = new CStringRef(span);
                Assert.Equal(5, s.Length);
                Assert.Equal(Capacity, s.Size);
                Assert.False(s.Readonly);
                Assert.Equal("hello", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_ToSpan_KeepsPointerAddress()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                Assert.Equal(ptr, UnsafeEx.GetAddress(s.ToSpan()));
                Assert.Equal(CStr.Bytes("hello"), s.ToSpan().Slice(0, 5).ToArray());

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_IndexerWrite_ReachesUnmanagedMemory()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s[0] = (byte)'H';
                Assert.Equal((byte)'H', At(ptr, 0));
                Assert.Equal("Hello", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_RefIndexerWrite_ReachesUnmanagedMemory()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s.Ref[1] = (byte)'E';
                Assert.Equal((byte)'E', At(ptr, 1));
                Assert.Equal("hEllo", s.ToString());

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_LengthShrink_ClearsTailInMemory()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s.Length = 2;
                Assert.Equal("he", s.ToString());
                Assert.Equal((byte)0, At(ptr, 2));
                Assert.Equal((byte)0, At(ptr, 3));
                Assert.Equal((byte)0, At(ptr, 4));

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_AppendWithinCapacity_WritesInPlace()
        {
            WithUnmanaged("hi", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                var result = s.Append((byte)'!');
                Assert.Equal(Capacity, result.Size);        // still the same unmanaged block
                Assert.Equal("hi!", result.ToString());
                Assert.Equal((byte)'!', At(ptr, 2));
                Assert.Equal((byte)0, At(ptr, 3));

                result.Dispose();  // result and s share one slot (in-place append): dispose exactly one handle
            });
        }

        [Fact]
        public void SpanOverPointer_AppendOverCapacity_ReturnsManagedInstance()
        {
            WithUnmanaged("abcd", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, 4));  // full: Length == Size
                Assert.Equal(4, s.Length);
                var grown = s.Append((byte)'e');
                Assert.Equal("abcde", grown.ToString());
                Assert.True(grown.Size > 4);
                // The unmanaged block is not owned by CStringRef and stays untouched.
                Assert.Equal((byte)'a', At(ptr, 0));
                Assert.Equal((byte)'c', At(ptr, 2));
                Assert.Equal((byte)'d', At(ptr, 3));

                // `grown` left the unmanaged block behind: the overflow re-bound the slot to a managed buffer.
                grown.Dispose();  // grown and s share one slot: dispose exactly one handle
            }, capacity: 4);
        }

        // ---------- overlapping in-place mutation ----------

        [Fact]
        public void SpanOverPointer_TrimStart_ShiftsInPlace()
        {
            WithUnmanaged("..hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s.TrimStart(CStr.Bytes("."));
                Assert.Equal("hello", s.ToString());
                Assert.Equal((byte)'h', At(ptr, 0));
                Assert.Equal((byte)0, At(ptr, 5));

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_Trim_BothEnds()
        {
            WithUnmanaged("..hello..", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s.Trim(CStr.Bytes("."));
                Assert.Equal("hello", s.ToString());
                Assert.Equal((byte)'h', At(ptr, 0));
                Assert.Equal((byte)0, At(ptr, 5));

                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_ReplaceInPlace_ReachesMemory()
        {
            WithUnmanaged("abcabc", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                s.Replace((byte)'a', (byte)'z');
                Assert.Equal("zbczbc", s.ToString());
                Assert.Equal((byte)'z', At(ptr, 0));
                Assert.Equal((byte)'z', At(ptr, 3));

                s.Dispose();
            });
        }

        // ---------- read-only span over a raw pointer ----------

        [Fact]
        public void ReadOnlySpanOverPointer_IsReadonlyAndRejectsWrites()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsReadOnlySpan<byte>(ptr, Capacity));
                Assert.True(s.Readonly);
                Assert.Equal(5, s.Length);
                Assert.Equal("hello", s.ToString());

                var copy = s;
                Assert.Throws<InvalidOperationException>(() => { copy[0] = (byte)'X'; });
                Assert.Throws<InvalidOperationException>(() => { copy.Append((byte)'!'); });
                Assert.Equal((byte)'h', At(ptr, 0));

                s.Dispose();       // copy is a struct copy sharing s's slot: dispose once
            });
        }

        // ---------- consistency with a managed instance ----------

        [Fact]
        public void SpanOverPointer_SearchAndCompare_MatchManagedBehaviour()
        {
            WithUnmanaged("hello world", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));
                var managed = new CStringRef("hello world");

                Assert.Equal(6, s.IndexOf((byte)'w'));
                Assert.Equal(managed.IndexOf((byte)'w'), s.IndexOf((byte)'w'));
                Assert.Equal(9, s.LastIndexOf((byte)'l'));
                Assert.Equal(managed.LastIndexOf((byte)'l'), s.LastIndexOf((byte)'l'));
                Assert.True(s.Contains(new CStringSpan("lo wo").Slice()));
                Assert.True(s.StartsWith(new CStringSpan("hello").Slice()));
                Assert.True(s.EndsWith(new CStringSpan("world").Slice()));
                Assert.Equal(0, s.CompareTo(managed));
                Assert.True(s.Equals(managed));

                // `managed` is the only managed-backed view in this file: only that one strictly
                // needs the reset, the native ones simply follow the same convention.
                managed.Dispose();
                s.Dispose();
            });
        }

        [Fact]
        public void SpanOverPointer_AfterGarbageAllocation_StillReadsCorrectly()
        {
            WithUnmanaged("hello", ptr =>
            {
                var s = new CStringRef(UnsafeEx.AsSpan<byte>(ptr, Capacity));

                // Allocate garbage so the GC has work to do, but never call GC.Collect:
                // this test process runs other unsafe tests in parallel and a forced
                // collection would relocate their managed buffers behind their backs.
                for (int i = 0; i < 64; ++i)
                {
                    _ = new byte[1024];
                }

                Assert.Equal("hello", s.ToString());
                Assert.Equal(0, s.IndexOf((byte)'h'));
                Assert.Equal((byte)'h', At(ptr, 0));

                s.Dispose();
            });
        }
    }
}
