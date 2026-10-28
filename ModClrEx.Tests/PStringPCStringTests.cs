using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Mod;
using Mod.LowLevel;
using Xunit;

namespace ModClrEx.Tests
{
    /// <summary>
    /// PString / PCString: pooled-string wrappers over the process-wide static
    /// StringPoolLocked. PString interns a string and assigns a stable slot index;
    /// PCString additionally caches a self-managed-native CString per pooled slot
    /// and hands out ONE instance per slot (GetOrCreate identity).
    ///
    /// <para>
    /// The pools are static and shared across the whole test process (xunit runs
    /// test classes in parallel), so every test uses Guid-based strings and only
    /// asserts RELATIVE facts (same/different slot, same/different instance) -
    /// never absolute slot numbers.
    /// </para>
    /// </summary>
    public class PStringPCStringTests
    {
        private static string Unique() => Guid.NewGuid().ToString();

        // ------------------------------------------------------------------
        // PString
        // ------------------------------------------------------------------

        [Fact]
        public void PString_Value_ToString_RoundTrip()
        {
            var str = Unique();
            var p = new PString(str);
            Assert.Equal(str, p.Value);
            Assert.Equal(str, p.ToString());
            string implicitStr = p;                     // implicit PString -> string
            Assert.Equal(str, implicitStr);
            Assert.True(p.Index >= 0);
        }

        [Fact]
        public void PString_SameContent_SameSlot_EvenForDistinctInstances()
        {
            // two equal-but-distinct string instances (not reference-equal, not
            // compile-time literals) must land on the SAME slot
            var content = Unique();
            string a = content;                         // Guid string: fresh runtime instance
            string b = new string(content.ToCharArray());
            Assert.False(ReferenceEquals(a, b));

            var pa = new PString(a);
            var pb = new PString(b);
            Assert.Equal(pa.Index, pb.Index);           // pooled exactly once
            Assert.True(pa.Equals(pb));                 // index-based equality
            Assert.Equal(pa.GetHashCode(), pb.GetHashCode());

            var other = new PString(Unique());
            Assert.NotEqual(pa.Index, other.Index);
            Assert.False(pa.Equals(other));
        }

        [Fact]
        public void PString_Equals_String_And_UnrelatedTypes()
        {
            var str = Unique();
            var p = new PString(str);
            Assert.True(p.Equals(str));                 // by value
            Assert.False(p.Equals(Unique()));
            Assert.False(p.Equals(null));
            Assert.False(p.Equals(42));
            Assert.Equal(str.GetHashCode(), p.GetHashCode());
        }

        [Fact]
        public void PString_ImplicitFromString_CreatesPooled()
        {
            string str = Unique();
            PString p = str;                            // implicit string -> PString
            Assert.Equal(str, p.Value);
            PString p2 = str;
            Assert.Equal(p.Index, p2.Index);            // both wrap the same slot
        }

        [Fact]
        public void PString_AsHashSetKey_DedupsByContent()
        {
            var content = Unique();
            var set = new HashSet<PString>
            {
                new PString(content),
                new PString(new string(content.ToCharArray())),
            };
            Assert.Single(set);                         // index-based Equals + hash
        }

        [Fact]
        public void PString_NullCtor_Pitfall_IndexIsDefaultZero()
        {
            // documented pitfall: the ctor skips every field for null, so Index is
            // the default 0 - NOT -1 - and two "null" PStrings compare equal by
            // index. A null PString also compares equal to whatever string sits on
            // slot 0 (not asserted: slot assignment is process-global).
            var p = new PString(null!);
            Assert.Null(p.Value);
            Assert.Null(p.ToString());
            Assert.Equal(0, p.Index);
            Assert.True(p.Equals(new PString(null!)));  // 0 == 0
            string implicitStr = p;                     // implicit conversion of a null wrapper
            Assert.Null(implicitStr);
        }

        // ------------------------------------------------------------------
        // PCString
        // ------------------------------------------------------------------

        [Fact]
        public void PCString_GetOrCreate_String_SameStringSameInstance()
        {
            var str = Unique();
            var a = PCString.GetOrCreate(str);
            var b = PCString.GetOrCreate(new string(str.ToCharArray()));   // distinct instance, same content
            Assert.Same(a, b);                          // one PCString per pooled slot

            var c = PCString.GetOrCreate(Unique());
            Assert.NotSame(a, c);

            Assert.Equal(str, a.String);
            Assert.Equal(str, a.ToString());
            Assert.True(a.Index >= 0);
            Assert.NotNull(a.PString);                  // the stored wrapper
            Assert.Equal(a.Index, a.PString.Index);
            Assert.NotNull(a.Key);
            string keyStr = a.Key;                      // WeakAttachment.Key -> string
            Assert.Equal(str, keyStr);
        }

        [Fact]
        public void PCString_GetOrCreate_PString_MatchesStringOverload()
        {
            var str = Unique();
            PString p = new PString(str);               // pooled first, no PCString yet
            var viaPString = PCString.GetOrCreate(p);
            var viaString = PCString.GetOrCreate(str);
            Assert.Same(viaPString, viaString);         // both resolve to the slot's instance
            Assert.Equal(p.Index, viaPString.Index);
        }

        [Fact]
        public void PCString_GetOrCreate_Null_ReturnsNull()
        {
            Assert.Null(PCString.GetOrCreate((string)null!));
            Assert.Null(PCString.GetOrCreate((PString)null!));
        }

        [Fact]
        public void PCString_CString_IsSelfManagedNative_WithUtf8Content()
        {
            // the private ctor builds the CString as new CStringRef(true).Append(str):
            // the empty self-managed start (Zero address) grows via ReAllocHGlobal
            var pc = PCString.GetOrCreate(Unique());
            CString cstr = pc.CString;
            Assert.Equal(pc.String, cstr.ToString());
            Assert.Equal(Encoding.UTF8.GetByteCount(pc.String), cstr.Length);
            Assert.Equal(cstr.Length + 1, cstr.Size);   // tight growth: content + NUL
            Assert.True(cstr.IsFixed);                  // self-managed native stays Raw
            Assert.True(cstr.IsNullTerminated());       // the growth path wrote the NUL
        }

        [Fact]
        public void PCString_CString_Multibyte()
        {
            var pc = PCString.GetOrCreate("你好-PString-PCString-你好");
            Assert.Equal("你好-PString-PCString-你好", pc.CString.ToString());
            Assert.Equal(Encoding.UTF8.GetByteCount("你好-PString-PCString-你好"), pc.CString.Length);
        }

        [Fact]
        public void PCString_CString_EmptyString_NoGrowth()
        {
            // Append("") is a no-op: the CString stays the empty self-managed
            // start view (Zero address, Size 0) - no terminator room
            var pc = PCString.GetOrCreate("");
            Assert.Equal("", pc.String);
            Assert.Equal(0, pc.CString.Length);
            Assert.Equal(0, pc.CString.Size);
            Assert.False(pc.CString.IsNullTerminated());
        }

        [Fact]
        public void PCString_Equals_And_Conversions()
        {
            var str = Unique();
            var pc = PCString.GetOrCreate(str);

            Assert.True(pc.Equals(str));                // by string value
            Assert.False(pc.Equals(Unique()));
            Assert.True(pc.Equals(PCString.GetOrCreate(str)));    // same instance
            Assert.False(pc.Equals(PCString.GetOrCreate(Unique())));
            Assert.False(pc.Equals(null));
            Assert.Equal(new PString(str).GetHashCode(), pc.GetHashCode());

            string asStr = pc;                          // PCString -> string
            Assert.Equal(str, asStr);
            PString asP = pc;                           // PCString -> PString (the stored one)
            Assert.Equal(pc.Index, asP.Index);
            PCString fromStr = str;                     // string -> PCString (GetOrCreate)
            Assert.Same(pc, fromStr);
            PCString fromP = new PString(str);          // PString -> PCString (GetOrCreate)
            Assert.Same(pc, fromP);
        }

        [Fact]
        public void PCString_EqualsPString_IsReferenceBased_Pitfall()
        {
            // documented pitfall: PCString.Equals(PString) compares the STORED
            // PString wrapper by reference, not by slot index. It is true only for
            // the exact PString instance the PCString was created from; a fresh
            // PString with the same content compares false. The string overloads
            // stay value-based.
            var str = Unique();
            PString p = new PString(str);               // fresh slot: the creation path stores p itself
            var pc = PCString.GetOrCreate(p);
            Assert.True(pc.Equals(p));                  // p IS the stored wrapper

            Assert.False(pc.Equals(new PString(str)));  // same content, different wrapper
            Assert.True(pc.Equals(str));                // string overload: value-based
        }

        [Fact]
        public void PCString_Concurrent_GetOrCreate_SameContent()
        {
            // GetOrCreate has a benign create race: several threads may all see
            // the slot empty and each build a PCString; the last Put wins. So
            // instance identity is NOT guaranteed mid-race - only content/index
            // agreement - but AFTER the race GetOrCreate hands out one stable
            // instance.
            var str = Unique();
            var results = new ConcurrentQueue<PCString>();
            var barrier = new Barrier(8);
            var threads = Enumerable.Range(0, 8).Select(_ => new Thread(() =>
            {
                barrier.SignalAndWait();                // maximize the race window overlap
                results.Enqueue(PCString.GetOrCreate(str));
            })).ToArray();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Equal(8, results.Count);
            foreach (var pc in results)
            {
                Assert.Equal(str, pc.String);           // every racer sees the right content
                Assert.Equal(Encoding.UTF8.GetByteCount(str), pc.CString.Length);
                Assert.Equal(str, pc.CString.ToString());
            }

            var final = PCString.GetOrCreate(str);
            var again = PCString.GetOrCreate(str);
            Assert.Same(final, again);                  // stable identity after the race
        }
    }
}
