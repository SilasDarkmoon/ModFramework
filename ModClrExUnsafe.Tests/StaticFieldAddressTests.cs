using System;
using System.Reflection;
using System.Runtime;
using System.Runtime.InteropServices;
using System.Threading;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Regression coverage for UnsafeEx.GetStaticFieldAddress /
    /// GetThreadStaticAddress. The platform formulas were verified on
    /// net10 / net48 / Mono / NativeAOT with Probes/StaticFieldAddressProbe;
    /// these tests keep the public contract intact on the JIT test runner.
    /// </summary>
    public class StaticFieldAddressTests
    {
        private class Fixture
        {
            public static int IntValue = 0x11111111;
            public static object RefValue = new object();
            public static string StringValue = "static-value";
            public static readonly int ReadOnlyValue = 42;
            private static int PrivateValue = 7;

            public const int ConstValue = 77;
            public int InstanceValue;

            [ThreadStatic] public static int TlsInt;
            [ThreadStatic] public static string TlsString;

            public static int PeekPrivate()
            {
                return PrivateValue;
            }
        }

        private class GenericFixture<T>
        {
            public static int Value;
        }

        private class CctorFixture
        {
            public static int Value;

            static CctorFixture()
            {
                // The marker lives on the outer type so observing it cannot
                // itself trigger this class constructor.
                StaticFieldAddressTests.CctorMarker = true;
                Value = 4242;
            }
        }

        private static bool CctorMarker;

        private static FieldInfo Field(string name)
        {
            return typeof(Fixture).GetField(name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        }

        private static IntPtr AddressOf(ref byte r)
        {
            return RawRef.Of(ref r).Address;
        }

        [Fact]
        public void PlainStatic_WriteThrough()
        {
            ref byte r = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.IntValue)));
            var address = AddressOf(ref r);
            Assert.Equal(0x11111111, Marshal.ReadInt32(address));
            Marshal.WriteInt32(address, 0x12345678);
            try
            {
                Assert.Equal(0x12345678, Fixture.IntValue);
            }
            finally
            {
                Marshal.WriteInt32(address, 0x11111111);
            }
        }

        [Fact]
        public void ReferenceStatic_ResolvesTheLiveObject()
        {
            ref byte r = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.RefValue)));
            object resolved = RawRef.Of(ref r).GetRef<object>();
            Assert.Same(Fixture.RefValue, resolved);

            ref byte s = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.StringValue)));
            object resolvedString = RawRef.Of(ref s).GetRef<object>();
            Assert.Same(Fixture.StringValue, resolvedString);
        }

        [Fact]
        public void ReadOnlyAndPrivate_AreAddressable()
        {
            ref byte ro = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.ReadOnlyValue)));
            Assert.Equal(42, Marshal.ReadInt32(AddressOf(ref ro)));

            ref byte priv = ref UnsafeEx.GetStaticFieldAddress(Field("PrivateValue"));
            var privateAddress = AddressOf(ref priv);
            Marshal.WriteInt32(privateAddress, 0x0BADF00D);
            try
            {
                Assert.Equal(0x0BADF00D, Fixture.PeekPrivate());
            }
            finally
            {
                Marshal.WriteInt32(privateAddress, 7);
            }
        }

        [Fact]
        public void GenericInstantiations_HaveDistinctStorage()
        {
            var intField = typeof(GenericFixture<int>).GetField("Value", BindingFlags.Static | BindingFlags.Public);
            var stringField = typeof(GenericFixture<string>).GetField("Value", BindingFlags.Static | BindingFlags.Public);
            ref byte ri = ref UnsafeEx.GetStaticFieldAddress(intField);
            ref byte rs = ref UnsafeEx.GetStaticFieldAddress(stringField);
            var intAddress = AddressOf(ref ri);
            var stringAddress = AddressOf(ref rs);

            Assert.NotEqual(intAddress, stringAddress);
            GenericFixture<int>.Value = 111;
            GenericFixture<string>.Value = 222;
            Assert.Equal(111, Marshal.ReadInt32(intAddress));
            Assert.Equal(222, Marshal.ReadInt32(stringAddress));
        }

        [Fact]
        public void RepeatedCalls_ResolveTheSameAddress()
        {
            ref byte first = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.IntValue)));
            ref byte second = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.IntValue)));
            Assert.Equal(AddressOf(ref first), AddressOf(ref second));
        }

        [Fact]
        public void ClassConstructor_RunsBeforeTheAddressIsUsed()
        {
            var field = typeof(CctorFixture).GetField("Value", BindingFlags.Static | BindingFlags.Public);
            ref byte r = ref UnsafeEx.GetStaticFieldAddress(field);
            Assert.True(CctorMarker);
            Assert.Equal(4242, Marshal.ReadInt32(AddressOf(ref r)));
        }

        [Fact]
        public void ThreadStatic_IsPerThread()
        {
            Fixture.TlsInt = 0;
            ref byte r0 = ref UnsafeEx.GetThreadStaticAddress(Field(nameof(Fixture.TlsInt)));
            // Keep a tracked byref across Thread.Join: other tests running in
            // parallel may force a compacting GC and move the TLS block, so a
            // cached IntPtr would go stale (this is exactly why the API returns
            // ref byte rather than IntPtr).
            ref int mainValue = ref RawRef.Of(ref r0).GetRef<int>();
            mainValue = 111;

            IntPtr workerAddress = IntPtr.Zero;
            int workerInitial = -1;
            int workerAfter = -1;
            var worker = new Thread(() =>
            {
                ref byte r1 = ref UnsafeEx.GetThreadStaticAddress(Field(nameof(Fixture.TlsInt)));
                workerAddress = AddressOf(ref r1);
                // Write through a tracked byref, not through a cached IntPtr:
                // a parallel compacting GC can relocate the TLS block.
                ref int slot1 = ref RawRef.Of(ref r1).GetRef<int>();
                workerInitial = slot1;
                slot1 = 222;
                workerAfter = Fixture.TlsInt;
            });
            worker.Start();
            worker.Join();

            Assert.NotEqual(AddressOf(ref r0), workerAddress);
            Assert.Equal(111, mainValue);
            Assert.Equal(0, workerInitial);
            Assert.Equal(222, workerAfter);
            Assert.Equal(111, Fixture.TlsInt);
        }

        [Fact]
        public void ThreadStatic_Reference_ResolvesTheLiveObject()
        {
            Fixture.TlsString = "tls-value";
            ref byte r = ref UnsafeEx.GetThreadStaticAddress(Field(nameof(Fixture.TlsString)));
            object resolved = RawRef.Of(ref r).GetRef<object>();
            Assert.Same(Fixture.TlsString, resolved);
        }

        [Fact]
        public void ReturnedByRef_IsRelocatedAndWritableAfterCompactingCollection()
        {
            // Read-back alone is a weak criterion: a stale byref can still see
            // the old reference bits in a dead slot. Write a fresh marker
            // through the byref and check the language-visible field instead.
            // Both byrefs (GC statics and TLS) must survive the same GC.
            for (int iteration = 0; iteration < 20; iteration++)
            {
                var marker = new object();
                int cookie = unchecked(0x55AA0000 + iteration);

                ref byte r = ref UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.RefValue)));
                ref object slot = ref RawRef.Of(ref r).GetRef<object>();
                ref byte tls = ref UnsafeEx.GetThreadStaticAddress(Field(nameof(Fixture.TlsInt)));
                ref int tlsSlot = ref RawRef.Of(ref tls).GetRef<int>();

                for (int i = 0; i < 64; i++)
                {
                    var junk = new byte[20000];
                    junk[0] = (byte)i;
                    GC.KeepAlive(junk);
                }
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect(2, GCCollectionMode.Forced, true, true);
                GC.WaitForPendingFinalizers();
                GC.Collect(2, GCCollectionMode.Forced, true, true);

                slot = marker;
                tlsSlot = cookie;
                Assert.Same(marker, Fixture.RefValue);
                Assert.Equal(cookie, Fixture.TlsInt);
            }
        }

        [Fact]
        public void InvalidFields_AreRejected()
        {
            Assert.Throws<ArgumentNullException>(() =>
            {
                byte value = UnsafeEx.GetStaticFieldAddress(null);
                GC.KeepAlive(value);
            });
            var instanceField = typeof(Fixture).GetField(nameof(Fixture.InstanceValue));
            Assert.Throws<ArgumentException>(() =>
            {
                byte value = UnsafeEx.GetStaticFieldAddress(instanceField);
                GC.KeepAlive(value);
            });
            Assert.Throws<ArgumentException>(() =>
            {
                byte value = UnsafeEx.GetStaticFieldAddress(Field(nameof(Fixture.ConstValue)));
                GC.KeepAlive(value);
            });
        }

    }
}
