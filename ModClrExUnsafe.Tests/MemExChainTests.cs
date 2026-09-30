using Mod.LowLevel;
using System;
using System.Reflection;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Regression tests for the handwritten MemCopy chains (MemCopy1024 forward chain /
    /// MemCopyRight1024 backward chain).
    ///
    /// <para>
    /// The public <see cref="UnsafeEx.MemCopy"/> entry routes to Span.CopyTo on .NET Core,
    /// so the handwritten chains (only consumed on .NET Framework) have zero coverage through
    /// the public API here. These tests fetch the private chain entry points via reflection
    /// and bind them to a signature-exact delegate, which preserves the ref-byte calling
    /// convention - no test hooks needed inside the library.
    /// </para>
    /// <para>
    /// Chain contracts being locked:
    /// <list type="bullet">
    /// <item>MemCopy1024 (forward): non-overlapping and left-shift-overlapping copies.</item>
    /// <item>MemCopyRight1024 (backward): right-shift-overlapping copies (the stack-swap fix
    /// for the in-block read-after-write hazard, and the (long)OffsetBetween fix in the LE7
    /// tail for cross-region distances).</item>
    /// </list>
    /// The entry-point direction routing itself (right-shift-only into the backward chain)
    /// lives in MemCopy and is only exercised on .NET Framework - not covered here.
    /// </para>
    /// <para>
    /// NOTE: xUnit runs these under tier0; the tier1 (optimized) check that the stack swap
    /// survives JIT copy-propagation was verified separately with a forced-tier1 probe and
    /// relies on the NoOptimization marker staying on MemCopyUsingSwap.
    /// </para>
    /// </summary>
    public class MemExChainTests
    {
        private delegate void MemCopyChainDel(ref byte dst, int dstStart, ref byte src, int srcStart, int cnt);

        private static readonly MemCopyChainDel Forward = Bind("MemCopy1024");
        private static readonly MemCopyChainDel Backward = Bind("MemCopyRight1024");

        private static MemCopyChainDel Bind(string name)
        {
            var mi = typeof(UnsafeEx).GetMethod(name, BindingFlags.NonPublic | BindingFlags.Static);
            Assert.NotNull(mi);
            return (MemCopyChainDel)Delegate.CreateDelegate(typeof(MemCopyChainDel), mi);
        }

        // =====================================================================
        // forward chain: non-overlapping + left-shift overlap
        // =====================================================================

        [Fact]
        public void ForwardChain_NonOverlapping_CopiesCorrectly()
        {
            var src = new byte[3000];
            var dst = new byte[3000];
            for (int i = 0; i < 3000; i++) src[i] = (byte)(i % 251);

            Forward(ref dst[0], 0, ref src[0], 0, 3000);

            for (int i = 0; i < 3000; i++) Assert.Equal(src[i], dst[i]);
        }

        [Fact]
        public void ForwardChain_LeftShiftOverlap_IsSafe()
        {
            // small: the classic Right16+Right8 shape, but on the forward chain left-shift
            // is contract-safe (forward blocks write low and read high).
            var small = new byte[26];
            for (int i = 0; i < 26; i++) small[i] = (byte)i;
            Forward(ref small[0], 0, ref small[0], 2, 24);
            for (int i = 0; i < 24; i++) Assert.Equal((byte)(i + 2), small[i]);

            // large: crosses every block boundary
            var large = new byte[3002];
            for (int i = 0; i < 3002; i++) large[i] = (byte)(i % 251);
            Forward(ref large[0], 0, ref large[0], 2, 3000);
            for (int i = 0; i < 3000; i++) Assert.Equal((byte)((i + 2) % 251), large[i]);
        }

        [Fact]
        public void ForwardChain_TinyTail_AllSizes()
        {
            // pure LE7 tail (cnt <= 7) and a big copy with a <=7 remainder
            for (int cnt = 1; cnt <= 7; cnt++)
            {
                var src = new byte[16];
                var dst = new byte[16];
                for (int i = 0; i < cnt; i++) src[i] = (byte)(0x30 + i);
                Forward(ref dst[0], 0, ref src[0], 0, cnt);
                for (int i = 0; i < cnt; i++) Assert.Equal((byte)(0x30 + i), dst[i]);
                Assert.Equal(0, dst[cnt]);
            }

            var srcBig = new byte[1024 + 7];
            var dstBig = new byte[1024 + 7];
            for (int i = 0; i < srcBig.Length; i++) srcBig[i] = (byte)(i % 251);
            Forward(ref dstBig[0], 0, ref srcBig[0], 0, 1024 + 7);
            for (int i = 0; i < srcBig.Length; i++) Assert.Equal(srcBig[i], dstBig[i]);
        }

        // =====================================================================
        // backward chain: right-shift overlap (the stack-swap fix)
        // =====================================================================

        [Fact]
        public void BackwardChain_RightShift_SmallBlocks()
        {
            // the minimal in-block hazard shape: single 64-byte block, shift 1 - the
            // second 32-byte granule used to read the byte the first granule just wrote.
            var buf = new byte[65];
            for (int i = 0; i < 65; i++) buf[i] = (byte)i;
            Backward(ref buf[0], 1, ref buf[0], 0, 64);
            for (int i = 1; i <= 64; i++) Assert.Equal((byte)(i - 1), buf[i]);
            Assert.Equal(0, buf[0]);
        }

        [Fact]
        public void BackwardChain_RightShift_LargeCrossesBlocks()
        {
            foreach (int shift in new[] { 1, 2, 3 })
            {
                var buf = new byte[3000 + shift];
                for (int i = 0; i < buf.Length; i++) buf[i] = (byte)(i % 251);

                Backward(ref buf[0], shift, ref buf[0], 0, 3000);

                for (int i = 0; i < shift; i++) Assert.Equal((byte)(i % 251), buf[i]);   // head untouched
                for (int i = shift; i < 3000 + shift; i++) Assert.Equal((byte)((i - shift) % 251), buf[i]);
            }
        }

        [Fact]
        public void BackwardChain_SizeShiftMatrix()
        {
            // the forced-tier1 probe matrix, now locked as a functional regression.
            foreach (int shift in new[] { 1, 2, 3, 7, 8, 33, 100 })
            {
                foreach (int size in new[] { 3, 15, 24, 64, 100, 300, 1024, 3000 })
                {
                    var buf = new byte[size + shift + 2];
                    for (int i = 0; i < buf.Length; i++) buf[i] = (byte)((i + shift) % 251);

                    Backward(ref buf[0], shift, ref buf[0], 0, size);

                    for (int i = 0; i < size; i++)
                    {
                        Assert.Equal((byte)((i + shift) % 251), buf[shift + i]);
                    }
                }
            }
        }

        // =====================================================================
        // backward chain LE7 tail: cross-region OffsetBetween must not overflow
        // =====================================================================

        [Fact]
        public void BackwardChain_CrossRegion_LE7TailDoesNotThrow()
        {
            // MemCopyRightLE7 computes (long)OffsetBetween unconditionally; with dst on the
            // stack and src on the heap the distance is a huge 64-bit value that the old
            // (int) cast converted with a checked OverflowException.
            var heap = new byte[64];
            for (int i = 0; i < 64; i++) heap[i] = (byte)(i + 1);
            Span<byte> stackBuf = stackalloc byte[64];

            // non-overlapping cross-region copy through the backward chain (the LE7 tail
            // still runs its OffsetBetween direction check on the huge distance)
            Backward(ref stackBuf[0], 0, ref heap[0], 0, 64);
            for (int i = 0; i < 64; i++) Assert.Equal((byte)(i + 1), stackBuf[i]);
        }
    }
}
