using System;
using System.Runtime.InteropServices;
using System.Text;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// CString: the string-like immutable wrapper over CStringRef. Every edit returns a
    /// NEW instance (the receiver stays unchanged); reads are forwarded to Value; the
    /// wrapped view is normalized to self-managed-native or Tracking on construction.
    /// </summary>
    public class CStringTests
    {
        [Fact]
        public void Construction_FromString_ReadsContent()
        {
            var s = new CString("hello");
            Assert.Equal(5, s.Length);
            Assert.Equal("hello", s.ToString());
            Assert.Equal((byte)'o', s[4]);
            Assert.False(s.IsFixed);            // net10: Tracking mode
        }

        [Fact]
        public void Construction_Normalizes_RawUnownedView_ToTracking()
        {
            // IntPtr ctor (caller owns the block): Raw and not self-managed -> the box tracks it
            var ptr = Marshal.AllocHGlobal(16);
            try
            {
                var bytes = Encoding.ASCII.GetBytes("hi");
                Marshal.Copy(bytes, 0, ptr, bytes.Length);
                Marshal.WriteByte(ptr, bytes.Length, 0);
                var raw = new CStringRef(ptr, 16, 2);        // Raw, not self-managed
                Assert.True(raw.IsFixed);
                var boxed = new CString(raw);
                Assert.False(boxed.IsFixed);                 // normalized: tracked now
                Assert.Equal("hi", boxed.ToString());

                boxed.Dispose();                             // returns the slot; the block is NOT freed
                Assert.Equal((byte)'h', Marshal.ReadByte(ptr, 0));   // the caller still owns it
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        [Fact]
        public void Construction_Keeps_SelfManagedNative_AsRaw()
        {
            var native = new CStringRef(16, true);
            native[0] = (byte)'h';
            native[1] = (byte)'i';
            var boxed = new CString(native);
            Assert.True(boxed.IsFixed);                      // self-managed stays Raw
            Assert.Equal("hi", boxed.ToString());

            boxed.Dispose();                                 // frees the native block
        }

        [Fact]
        public void Edits_ReturnNewInstances_ReceiverUnchanged()
        {
            var s = new CString("ab");
            var appended = s.Append((byte)'c');
            Assert.Equal("abc", appended.ToString());
            Assert.Equal("ab", s.ToString());                // receiver unchanged (string semantics)

            var inserted = s.Insert(1, "XY");
            Assert.Equal("aXYb", inserted.ToString());
            Assert.Equal("ab", s.ToString());

            var replaced = s.Replace((byte)'a', (byte)'z');
            Assert.Equal("zb", replaced.ToString());
            Assert.Equal("ab", s.ToString());

            var trimmed = new CString("  hi  ").Trim();
            Assert.Equal("hi", trimmed.ToString());

            var removed = new CString("abcdef").Remove(2, 2);
            Assert.Equal("abef", removed.ToString());

            var cleared = new CString("abc").Clear();
            Assert.Equal("", cleared.ToString());
            Assert.Equal(0, cleared.Length);

            var resized = new CString("hello").Resize(32);
            Assert.Equal("hello", resized.ToString());
            Assert.Equal(32, resized.Size);

            var expanded = new CString("hi").Expand(24);
            Assert.Equal(24, expanded.Size);
            Assert.Equal("hi", expanded.ToString());
        }

        [Fact]
        public void OperatorPlus_Concats()
        {
            var a = new CString("foo");
            var b = new CString("bar");
            var sum = a + b;
            Assert.Equal("foobar", sum.ToString());
            Assert.Equal("foo", a.ToString());               // operands unchanged

            var sum2 = a + new CStringRef("baz");
            Assert.Equal("foobaz", sum2.ToString());
        }

        [Fact]
        public void Conversions_String_CStringSpan_CStringRef()
        {
            var s = new CString("hello");
            string str = (string)s;                          // explicit (strong) cast
            Assert.Equal("hello", str);
            CStringSpan span = s;                            // implicit view conversion
            Assert.Equal("hello", span.ToString());
            CStringRef r = s;                                // implicit unwrap
            Assert.Equal("hello", r.ToString());
            CString fromStr = "abc";                         // implicit from string
            Assert.Equal("abc", fromStr.ToString());

            Assert.Null((string)(CString)null);              // null-safe casts
        }

        [Fact]
        public void Equality_And_Ordering()
        {
            var a = new CString("hello");
            var b = new CString("hello");
            var c = new CString("hellp");
            Assert.True(a == b);
            Assert.False(a != b);
            Assert.True(a != c);
            Assert.True(a.Equals(b));
            Assert.True(a.Equals((object)b));
            Assert.True(a.Equals("hello"));
            Assert.Equal(a.GetHashCode(), b.GetHashCode());
            Assert.Equal(0, a.CompareTo(b));
            Assert.True(a.CompareTo(c) < 0);
            Assert.Equal(0, a.CompareTo("hello"));
            Assert.Equal(0, ((IComparable)a).CompareTo(b));  // IComparable explicit impl
            Assert.True(a != null);
        }

        [Fact]
        public void Clone_IsIndependent()
        {
            var s = new CString("hello");
            var clone = s.Clone();
            Assert.Equal("hello", clone.ToString());
            Assert.Equal("hello", s.ToString());             // source unaffected
            // the clone's buffer is independent: mutating through Ref does not leak back
            clone.Ref[0] = (byte)'H';
            clone.ClearCache();                              // external writes need manual cache reset
            Assert.Equal("Hello", clone.ToString());
            Assert.Equal("hello", s.ToString());
        }

        [Fact]
        public void Search_Family_Forwarded()
        {
            var s = new CString("hello world");
            Assert.Equal(6, s.IndexOf((byte)'w'));
            Assert.Equal(4, s.IndexOf(new CStringRef("o w")));
            Assert.True(s.Contains((ReadOnlySpan<byte>)CStr.Bytes("lo wo")));
            Assert.True(s.StartsWith(new CStringRef("hello")));
            Assert.True(s.EndsWith(new CStringRef("world")));
            Assert.Equal(9, s.LastIndexOf((byte)'l'));
        }

        [Fact]
        public void Search_String_And_CString_Overloads()
        {
            var s = new CString("hello world");
            Assert.Equal(6, s.IndexOf("world"));
            Assert.Equal(6, s.IndexOf(new CString("world")));
            Assert.Equal(6, s.IndexOf("world", 2));               // startIndex
            Assert.Equal(-1, s.IndexOf("world", 7));              // no match past 7
            Assert.Equal(9, s.LastIndexOf("l"));
            Assert.Equal(9, s.LastIndexOf(new CString("l")));
            Assert.True(s.Contains("lo wo"));
            Assert.True(s.Contains(new CString("lo wo")));
            Assert.False(s.Contains("xyz"));
            Assert.True(s.StartsWith("hello"));
            Assert.True(s.StartsWith(new CString("hello")));
            Assert.True(s.EndsWith("world"));
            Assert.True(s.EndsWith(new CString("world")));
            Assert.False(s.StartsWith("world"));
        }

        [Fact]
        public void Search_StringOverloads_Utf8_And_EdgeCases()
        {
            var s = new CString("你好 world");                    // 你好 = 6 UTF-8 bytes
            Assert.Equal(0, s.IndexOf("你好"));                   // multibyte needle
            Assert.Equal(3, s.IndexOf("好"));                     // second char at byte 3
            Assert.True(s.StartsWith("你好"));
            Assert.True(s.EndsWith("world"));

            Assert.Equal(0, s.IndexOf(""));                       // empty: startIndex semantics
            Assert.Equal(2, s.IndexOf("", 2));
            Assert.True(s.Contains(""));
            Assert.True(s.StartsWith(""));
            Assert.True(s.EndsWith(""));
            Assert.Equal(s.Length, s.LastIndexOf(""));            // empty matches at the end

            Assert.Throws<ArgumentNullException>(() => s.IndexOf((string)null));
            Assert.Throws<ArgumentNullException>(() => s.Contains((string)null));
            Assert.Throws<ArgumentNullException>(() => s.IndexOf((CString)null));
        }

        [Fact]
        public void Search_StringOverloads_LongNeedle_HeapPath()
        {
            string needle = new string('x', 300);                  // > 256: heap fallback path
            var s = new CString("prefix-" + needle + "-suffix");
            Assert.Equal(7, s.IndexOf(needle));
            Assert.True(s.Contains(needle));
            Assert.True(s.EndsWith("-suffix"));
        }

        [Fact]
        public void Search_CStringSpan_Overloads()
        {
            var s = new CString("hello world");
            Assert.Equal(6, s.IndexOf(new CStringSpan("world")));
            Assert.True(s.Contains(new CStringSpan("lo wo")));
            Assert.True(s.StartsWith(new CStringSpan("hello")));
            Assert.True(s.EndsWith(new CStringSpan("world")));
            Assert.Equal(2, s.LastIndexOf(new CStringSpan("ll")));   // the only "ll" in "hello world"
        }

        [Fact]
        public void Edits_CStringSpan_And_CString_Overloads()
        {
            var s = new CString("ab");
            var a1 = s.Append(new CStringSpan("cd"));             // Append(CStringSpan)
            Assert.Equal("abcd", a1.ToString());
            Assert.Equal("ab", s.ToString());

            var a2 = s.Append(new CString("cd"));                 // Append(CString)
            Assert.Equal("abcd", a2.ToString());

            var i1 = s.Insert(1, new CStringSpan("XY"));          // Insert(CStringSpan)
            Assert.Equal("aXYb", i1.ToString());
            var i2 = s.Insert(1, new CStringSpan("-"), 3);        // Insert(CStringSpan, repeat)
            Assert.Equal("a---b", i2.ToString());
            var i3 = s.Insert(1, new CString("XY"));              // Insert(CString)
            Assert.Equal("aXYb", i3.ToString());
            var i4 = s.Insert(1, new CString("-"), 3);            // Insert(CString, repeat)
            Assert.Equal("a---b", i4.ToString());

            var r1 = s.Replace(new CStringSpan("a"), new CStringSpan("zz"));   // Replace(CStringSpan)
            Assert.Equal("zzb", r1.ToString());
            var r2 = s.Replace(new CString("a"), new CString("zz"));           // Replace(CString)
            Assert.Equal("zzb", r2.ToString());

            Assert.Equal("ab", s.ToString());                     // receiver unchanged throughout
        }

        [Fact]
        public void Replace_StringOverload_Utf8_Window_AndEdges()
        {
            var s = new CString("a.b.c");
            var r = s.Replace(".", "-");                          // Replace(string, string)
            Assert.Equal("a-b-c", r.ToString());
            Assert.Equal("a.b.c", s.ToString());

            var w = new CString("x-y-zz");
            var rw = w.Replace("-", "QQ", 2, 4);                  // window version
            Assert.Equal("x-yQQzz", rw.ToString());

            var u = new CString("你好.世界");
            var ru = u.Replace(".", "，");                        // UTF-8 multibyte replacement
            Assert.Equal("你好，世界", ru.ToString());

            var d = new CString("a1b1c1");
            var rd = d.Replace("1", "");                          // deletion via empty newstr
            Assert.Equal("abc", rd.ToString());

            Assert.Throws<ArgumentException>(() => s.Replace("", "x"));   // empty oldstr: string.Replace semantics
            Assert.Throws<ArgumentNullException>(() => s.Replace(null, "x"));
            Assert.Throws<ArgumentNullException>(() => s.Replace("a", null));

            string needle = new string('x', 300);                 // > 256: heap fallback for both needles
            var big = new CString("p-" + needle + "-q");
            Assert.Equal(2, big.IndexOf(needle));
            var bigReplaced = big.Replace(needle, "y");
            Assert.Equal("p-y-q", bigReplaced.ToString());
        }

        [Fact]
        public void Substring_Split_Forwarded()
        {
            var s = new CString("a,b,c");
            Assert.Equal("b", s.Substring(2, 1).ToString());
            var parts = new System.Collections.Generic.List<string>();
            foreach (var part in s.Split((byte)','))
            {
                parts.Add(part.ToString());
            }
            Assert.Equal(new[] { "a", "b", "c" }, parts);
        }

        [Fact]
        public void Enumeration_YieldsBytes()
        {
            var s = new CString("abc");
            int sum = 0;
            foreach (byte b in s)
            {
                sum += b;
            }
            Assert.Equal(97 + 98 + 99, sum);
        }

        [Fact]
        public void NullHelpers()
        {
            Assert.True(CString.IsNullOrEmpty(null));
            Assert.True(CString.IsNullOrEmpty(new CString("")));
            Assert.False(CString.IsNullOrEmpty(new CString("x")));
            Assert.True(CString.IsNullOrWhiteSpace(new CString("  ")));
            Assert.False(CString.IsNullOrWhiteSpace(new CString(" x ")));
        }

        [Fact]
        public void Dispose_ClearsValue()
        {
            var s = new CString("hello");
            s.Dispose();
            Assert.True(s.IsEmpty());                        // Value == default after Dispose
            Assert.True(CString.IsNullOrEmpty(s));
        }
    }
}
