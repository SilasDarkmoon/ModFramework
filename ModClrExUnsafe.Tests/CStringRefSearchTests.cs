using System;
using System.Collections.Generic;
using Xunit;

namespace Mod.Tests
{
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
        }

        [Fact]
        public void IndexOf_Byte_StartIndexBounds()
        {
            var s = new CStringRef("abc");
            Assert.Equal(-1, s.IndexOf((byte)'x', 3));        // == Length allowed
            Assert.Throws<ArgumentOutOfRangeException>(() => s.IndexOf((byte)'a', 4));
            Assert.Throws<ArgumentOutOfRangeException>(() => s.IndexOf((byte)'a', -1));
        }

        [Fact]
        public void IndexOf_CStringRef_MatchAndStartIndex()
        {
            var s = new CStringRef("ababab");
            Assert.Equal(2, s.IndexOf(new CStringRef("ab"), 1));
            Assert.Equal(4, s.IndexOf(new CStringRef("ab"), 3));
            Assert.Equal(-1, s.IndexOf(new CStringRef("ab"), 5));
            Assert.Equal(1, s.IndexOf(new CStringRef("bab")));
        }

        [Fact]
        public void IndexOf_EmptyValue_ReturnsStartIndex()
        {
            var s = new CStringRef("abc");
            Assert.Equal(0, s.IndexOf(CStringRef.Empty));
            Assert.Equal(3, s.IndexOf(CStringRef.Empty, s.Length));
            Assert.True(s.Contains(CStringRef.Empty));
        }

        [Fact]
        public void IndexOf_Span_Matches()
        {
            var s = new CStringRef("xxfooyy");
            Assert.Equal(2, s.IndexOf(CStr.Bytes("foo")));
            Assert.Equal(-1, s.IndexOf(CStr.Bytes("Foo")));
        }

        [Fact]
        public void Contains_ByteAndCStringRef()
        {
            var s = new CStringRef("hello");
            Assert.True(s.Contains((byte)'l'));
            Assert.False(s.Contains((byte)'L'));
            Assert.True(s.Contains(new CStringRef("ell")));
            Assert.False(s.Contains(new CStringRef("all")));
        }

        [Fact]
        public void LastIndexOf_Byte_FromEnd()
        {
            var s = new CStringRef("abcabc");
            Assert.Equal(4, s.LastIndexOf((byte)'b'));
            Assert.Equal(-1, s.LastIndexOf((byte)'z'));
            Assert.Equal(-1, CStringRef.Empty.LastIndexOf((byte)'x'));
        }

        [Fact]
        public void LastIndexOf_CStringRef_MatchAndEmpty()
        {
            var s = new CStringRef("ababab");
            Assert.Equal(4, s.LastIndexOf(new CStringRef("ab")));
            Assert.Equal(6, s.LastIndexOf(CStringRef.Empty));     // empty -> Length
            Assert.Equal(-1, new CStringRef("ab").LastIndexOf(new CStringRef("abc")));
        }

        [Fact]
        public void LastIndexOf_ReadOnlySpan()
        {
            var s = new CStringRef("xxfoofoo");
            Assert.Equal(5, s.LastIndexOf(CStr.Bytes("foo")));
        }

        // ---------- IndexOfAny / LastIndexOfAny ----------

        [Fact]
        public void IndexOfAny_MultipleCandidates()
        {
            var s = new CStringRef("a.b,c");
            Assert.Equal(1, s.IndexOfAny(CStr.Bytes(".,;")));
            Assert.Equal(3, s.IndexOfAny(CStr.Bytes(".,;"), 2));
            Assert.Equal(-1, s.IndexOfAny(CStr.Bytes("xy")));
        }

        [Fact]
        public void IndexOfAny_EmptyAnyOf_Throws()
        {
            var s = new CStringRef("abc");
            Assert.Throws<ArgumentException>(() => s.IndexOfAny(CStr.Bytes("")));
        }

        [Fact]
        public void LastIndexOfAny_FromEnd()
        {
            var s = new CStringRef("a.b,c");
            Assert.Equal(3, s.LastIndexOfAny(CStr.Bytes(".,")));
            Assert.Equal(-1, CStringRef.Empty.LastIndexOfAny(CStr.Bytes(".")));
            Assert.Throws<ArgumentException>(() => s.LastIndexOfAny(CStr.Bytes("")));
        }

        // ---------- StartsWith / EndsWith ----------

        [Fact]
        public void StartsWith_Variants()
        {
            var s = new CStringRef("hello");
            Assert.True(s.StartsWith((byte)'h'));
            Assert.False(s.StartsWith((byte)'H'));
            Assert.True(s.StartsWith(new CStringRef("he")));
            Assert.False(s.StartsWith(new CStringRef("hellx")));
            Assert.True(s.StartsWith(CStr.Bytes("hel")));
            Assert.True(s.StartsWith(CStr.Bytes("")));
            Assert.False(CStringRef.Empty.StartsWith((byte)'x'));
        }

        [Fact]
        public void EndsWith_Variants()
        {
            var s = new CStringRef("hello");
            Assert.True(s.EndsWith((byte)'o'));
            Assert.False(s.EndsWith((byte)'O'));
            Assert.True(s.EndsWith(new CStringRef("llo")));
            Assert.True(s.EndsWith(CStr.Bytes("lo")));
            Assert.True(s.EndsWith(CStr.Bytes("")));
            Assert.False(CStringRef.Empty.EndsWith((byte)'x'));
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
            Assert.True(new CStringRef("   ").IsWhiteSpace());
            Assert.True(new CStringRef(" \t\r\n ").IsWhiteSpace());
            Assert.False(new CStringRef(" a ").IsWhiteSpace());
            Assert.True(CStringRef.Empty.IsWhiteSpace());
        }

        // ---------- Split ----------

        private static List<string> SplitAll(CStringRef.CStringRefSplitEnumerator e)
        {
            var parts = new List<string>();
            foreach (CStringRef part in e)
            {
                parts.Add(part.ToString());
            }
            return parts;
        }

        [Fact]
        public void Split_SingleSeparator_Basic()
        {
            var parts = SplitAll(new CStringRef("a,b,c").Split((byte)','));
            Assert.Equal(new[] { "a", "b", "c" }, parts);
        }

        [Fact]
        public void Split_SingleSeparator_EmptyFieldsPreserved()
        {
            var parts = SplitAll(new CStringRef("a,,b,").Split((byte)','));
            Assert.Equal(new[] { "a", "", "b", "" }, parts);
        }

        [Fact]
        public void Split_MultipleSeparators()
        {
            var parts = SplitAll(new CStringRef("a,b;c").Split(CStr.Bytes(",;")));
            Assert.Equal(new[] { "a", "b", "c" }, parts);
        }

        [Fact]
        public void Split_ByWhitespace_SkipsLeadingTrailingAndRuns()
        {
            var parts = SplitAll(new CStringRef("  one \t two\r\nthree  ").Split());
            Assert.Equal(new[] { "one", "two", "three" }, parts);
        }

        [Fact]
        public void Split_ByWhitespace_EmptySource_YieldsNothing()
        {
            Assert.Empty(SplitAll(CStringRef.Empty.Split()));
        }

        [Fact]
        public void Split_NoSeparatorFound_YieldsWholeString()
        {
            var parts = SplitAll(new CStringRef("hello").Split((byte)','));
            Assert.Equal(new[] { "hello" }, parts);
        }
    }
}
