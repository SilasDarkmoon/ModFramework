using System;
using Mod;
using Xunit;

namespace ModClrEx.Tests
{
    /// <summary>
    /// GC-side semantics of WeakAttachment: the backing store is a
    /// ConditionalWeakTable, so when a host object dies its whole
    /// WeakAttachmentStorage (and every attachment reachable only through it)
    /// must die with it. The existing WeakAttachmentTests all keep the host
    /// strongly referenced, so this half of the contract was never exercised.
    ///
    /// GC discipline (the WeakDelegate lessons): the doomed objects are created
    /// inside a helper and never escape it - only WeakReferences come back.
    /// Setting the local to null inside the test method itself does NOT work
    /// (Debug JIT keeps locals alive to the end of the method).
    /// </summary>
    public class WeakAttachmentGcTests
    {
        private static (WeakReference host, WeakReference attach) MakeDoomedHostWithAttachment()
        {
            var host = new object();
            var attach = new object();
            host.SetAttachment("gc-death-key", attach);
            return (new WeakReference(host), new WeakReference(attach));
        }

        private static void FullGc()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        [Fact]
        public void HostDeath_StorageAndAttachmentCollected()
        {
            var (hostWr, attachWr) = MakeDoomedHostWithAttachment();

            Assert.True(hostWr.IsAlive);        // sanity: alive before GC
            Assert.True(attachWr.IsAlive);

            FullGc();

            Assert.False(hostWr.IsAlive);       // host died...
            Assert.False(attachWr.IsAlive);     // ...and the attachment (only
                                                // reachable via the CWT storage)
                                                // died with it - no leak
        }

        [Fact]
        public void HostAlive_AttachmentStaysAlive()
        {
            // control group: same setup, but the host stays strongly referenced
            // (held by this method) - the attachment must survive GC
            var host = new object();
            var attach = new object();
            host.SetAttachment("gc-alive-key", attach);
            var attachWr = new WeakReference(attach);
            attach = null;                      // drop the local, host keeps the CWT entry alive

            FullGc();

            Assert.True(attachWr.IsAlive);      // storage is alive because host is
            Assert.Same(host.GetAttachment("gc-alive-key"), attachWr.Target);
        }

        [Fact]
        public void HostDeath_KeyedAttachmentAlsoCollected()
        {
            // same via the Key overload (known-key fast path)
            var (hostWr, attachWr) = MakeDoomedKeyedHost();
            FullGc();
            Assert.False(hostWr.IsAlive);
            Assert.False(attachWr.IsAlive);
        }

        private static (WeakReference host, WeakReference attach) MakeDoomedKeyedHost()
        {
            var host = new object();
            var attach = new object();
            host.SetAttachment(new WeakAttachment.Key("gc-death-keyed"), attach);
            return (new WeakReference(host), new WeakReference(attach));
        }
    }
}
