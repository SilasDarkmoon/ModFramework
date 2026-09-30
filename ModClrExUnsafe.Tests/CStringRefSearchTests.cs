using Mod;
using System;
using System.Collections.Generic;
using Mod.LowLevel;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Convention for this file: every local that holds a constructed CStringRef is disposed with
    /// <c>Dispose()</c> before the test returns (see CStringRefBasicsTests for the full rationale:
    /// the RawTrackingRef slot is a lease, Dispose returns it). Purely temporary arguments are
    /// built as <c>CStringSpan</c> where a ReadOnlySpan overload exists — a span takes no slot at
    /// all. Please keep both conventions.
    /// </summary>
    public class CStringRefSearchTests
    {
        // ---------- IndexOf / LastIndexOf / Contains ----------

        [Fact]
        public void IndexOf_Byte_FromStart()
        {
            var s = new CStringRef("abcabc");
            Assert.Equal(1, s.IndexOf((byte)'b'));
            Assert.Equal(4, s.IndexOf((byte)'b', 2));
            Assert.Equal(4, s.IndexOf((byte)'b', 4));
            Assert.Equal(-1, s.IndexOf((byte)'b', 5));
            Assert.Equal(-1, s.IndexOf((byte)'z'));

            s.Dispose();
        }

        [Fact]
        public void IndexOf_Byte_StartIndexBounds()
        {
            var s = new CStringRef("abc");
            Assert.Equal(-1, s.IndexOf((byte)'x', 3));        // == Length allowed
            Assert.Throws<ArgumentOutOfRangeException>(() => s.IndexOf((byte)'a', 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.IndexOf((byte)'a', -1));

            s.Dispose();
        }

        [Fact]
        public void IndexOf_CStringRef_MatchAndStartIndex()
        {
            var s = new CStringRef("ababab");
            var needle = new CStringRef("ab");          // explicit local: exercises the CStringRef overload
            Assert.Equal(2, s.IndexOf(needle, 1));
            Assert.Equal(4, s.IndexOf(new CStringSpan("ab").Slice(), 3));   // temporaries: span, no slot taken
            Assert.Equal(-1, s.IndexOf(new CStringSpan("ab").Slice(), 5));
            Assert.Equal(1, s.IndexOf(new CStringSpan("bab").Slice()));

            s.Dispose();
            needle.Dispose();
        }

        [Fact]
        public void IndexOf_EmptyValue_ReturnsStartIndex()
        {
            var s = new CStringRef("abc");
            Assert.Equal(0, s.IndexOf(CStringRef.Empty));
            Assert.Equal(3, s.IndexOf(CStringRef.Empty, s.Length));
            Assert.True(s.Contains(CStringRef.Empty));

            s.Dispose();
        }

        [Fact]
        public void IndexOf_Span_Matches()
        {
            var s = new CStringRef("xxfooyy");
            Assert.Equal(2, s.IndexOf(CStr.Bytes("foo")));
            Assert.Equal(-1, s.IndexOf(CStr.Bytes("Foo")));

            s.Dispose();
        }

        [Fact]
        public void Contains_ByteAndCStringRef()
        {
            var s = new CStringRef("hello");
            Assert.True(s.Contains((byte)'l'));
            Assert.False(s.Contains((byte)'L'));
            var needle = new CStringRef("ell");         // explicit local: exercises the CStringRef overload
            Assert.True(s.Contains(needle));
            Assert.False(s.Contains(new CStringSpan("all").Slice()));

            s.Dispose();
            needle.Dispose();
        }

        [Fact]
        public void LastIndexOf_Byte_FromEnd()
        {
            var s = new CStringRef("abcabc");
            Assert.Equal(4, s.LastIndexOf((byte)'b'));
            Assert.Equal(-1, s.LastIndexOf((byte)'z'));
            Assert.Equal(-1, CStringRef.Empty.LastIndexOf((byte)'x'));

            s.Dispose();
        }

        [Fact]
        public void LastIndexOf_CStringRef_MatchAndEmpty()
        {
            var s = new CStringRef("ababab");
            var needle = new CStringRef("ab");          // explicit local: exercises the CStringRef overload
            Assert.Equal(4, s.LastIndexOf(needle));
            Assert.Equal(6, s.LastIndexOf(CStringRef.Empty));     // empty -> Length

            var shorter = new CStringRef("ab");
            Assert.Equal(-1, shorter.LastIndexOf(new CStringSpan("abc").Slice()));

            s.Dispose();
            needle.Dispose();
            shorter.Dispose();
        }

        [Fact]
        public void LastIndexOf_ReadOnlySpan()
        {
            var s = new CStringRef("xxfoofoo");
            Assert.Equal(5, s.LastIndexOf(CStr.Bytes("foo")));

            s.Dispose();
        }

        // ---------- IndexOfAny / LastIndexOfAny ----------

        [Fact]
        public void IndexOfAny_MultipleCandidates()
        {
            var s = new CStringRef("a.b,c");
            Assert.Equal(1, s.IndexOfAny(CStr.Bytes(".,;")));
            Assert.Equal(3, s.IndexOfAny(CStr.Bytes(".,;"), 2));
            Assert.Equal(-1, s.IndexOfAny(CStr.Bytes("xy")));

            s.Dispose();
        }

        [Fact]
        public void IndexOfAny_EmptyAnyOf_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentException>(() => s.IndexOfAny(CStr.Bytes("")));

            s.Dispose();
        }

        [Fact]
        public void LastIndexOfAny_FromEnd()
        {
            var s = new CStringRef("a.b,c");
            Assert.Equal(3, s.LastIndexOfAny(CStr.Bytes(".,")));
            Assert.Equal(-1, CStringRef.Empty.LastIndexOfAny(CStr.Bytes(".")));
            Assert.Throws<ArgumentException>(() => s.LastIndexOfAny(CStr.Bytes("")));

            s.Dispose();
        }

        // ---------- StartsWith / EndsWith ----------

        [Fact]
        public void StartsWith_Variants()
        {
            var s = new CStringRef("hello");
            Assert.True(s.StartsWith((byte)'h'));
            Assert.False(s.StartsWith((byte)'H'));
            var needle = new CStringRef("he");          // explicit local: exercises the CStringRef overload
            Assert.True(s.StartsWith(needle));
            Assert.False(s.StartsWith(new CStringSpan("hellx").Slice()));
            Assert.True(s.StartsWith(CStr.Bytes("hel")));
            Assert.True(s.StartsWith(CStr.Bytes("")));
            Assert.False(CStringRef.Empty.StartsWith((byte)'x'));

            s.Dispose();
            needle.Dispose();
        }

        [Fact]
        public void EndsWith_Variants()
        {
            var s = new CStringRef("hello");
            Assert.True(s.EndsWith((byte)'o'));
            Assert.False(s.EndsWith((byte)'O'));
            var needle = new CStringRef("llo");         // explicit local: exercises the CStringRef overload
            Assert.True(s.EndsWith(needle));
            Assert.True(s.EndsWith(CStr.Bytes("lo")));
            Assert.True(s.EndsWith(CStr.Bytes("")));
            Assert.False(CStringRef.Empty.EndsWith((byte)'x'));

            s.Dispose();
            needle.Dispose();
        }

        // ---------- IsWhiteSpace ----------

        [Fact]
        public void IsWhiteSpace_Static_ForSpaceAndControlRange()
        {
            Assert.True(CStringRef.IsWhiteSpace((byte)' '));
            Assert.True(CStringRef.IsWhiteSpace((byte)'\t'));
            Assert.True(CStringRef.IsWhiteSpace((byte)'\n'));
            Assert.False(CStringRef.IsWhiteSpace((byte)'a'));
        }

        [Fact]
        public void IsWhiteSpace_Content()
        {
            var spaces = new CStringRef("   ");
            Assert.True(spaces.IsWhiteSpace());

            var mixed = new CStringRef(" \t\r\n ");
            Assert.True(mixed.IsWhiteSpace());

            var letter = new CStringRef(" a ");
            Assert.False(letter.IsWhiteSpace());

            Assert.True(CStringRef.Empty.IsWhiteSpace());

            spaces.Dispose();
            mixed.Dispose();
            letter.Dispose();
        }

        // ---------- Split ----------

        private static List<string> SplitAll(CStringSpanSplitEnumerator e)
        {
            var parts = new List<string>();
            foreach (var part in e)
            {
                parts.Add(part.ToString());
            }
            return parts;
        }

        [Fact]
        public void Split_SingleSeparator_Basic()
        {
            var source = new CStringRef("a,b,c");
            var parts = SplitAll(source.Split((byte)','));
            Assert.Equal(new[] { "a", "b", "c" }, parts);

            source.Dispose();
        }

        [Fact]
        public void Split_SingleSeparator_EmptyFieldsPreserved()
        {
            var source = new CStringRef("a,,b,");
            var parts = SplitAll(source.Split((byte)','));
            Assert.Equal(new[] { "a", "", "b", "" }, parts);

            source.Dispose();
        }

        [Fact]
        public void Split_MultipleSeparators()
        {
            var source = new CStringRef("a,b;c");
            var parts = SplitAll(source.Split(CStr.Bytes(",;")));
            Assert.Equal(new[] { "a", "b", "c" }, parts);

            source.Dispose();
        }

        [Fact]
        public void Split_ByWhitespace_SkipsLeadingTrailingAndRuns()
        {
            var source = new CStringRef("  one \t two\r\nthree  ");
            var parts = SplitAll(source.Split());
            Assert.Equal(new[] { "one", "two", "three" }, parts);

            source.Dispose();
        }

        [Fact]
        public void Split_ByWhitespace_EmptySource_YieldsNothing()
        {
            Assert.Empty(SplitAll(CStringRef.Empty.Split()));
        }

        [Fact]
        public void Split_NoSeparatorFound_YieldsWholeString()
        {
            var source = new CStringRef("hello");
            var parts = SplitAll(source.Split((byte)','));
            Assert.Equal(new[] { "hello" }, parts);

            source.Dispose();
        }
    }
}
