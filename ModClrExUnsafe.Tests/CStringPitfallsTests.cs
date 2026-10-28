using Mod.LowLevel;
using System;
using System.Text;
using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Documented pitfalls around CString/CStringSpan/CStringRef that the
    /// compiler will NOT catch:
    ///
    ///  1. CopyTo missing `ref`: `src.CopyTo(dst)` where dst is a
    ///     CStringSpan/CStringRef does not call `CopyTo(ref dst)` - the
    ///     implicit conversion to Span&lt;byte&gt; makes the compiler silently
    ///     pick the `CopyTo(Span&lt;byte&gt;)` overload instead, which has NO
    ///     readonly guard and different length semantics. Always spell out
    ///     `src.CopyTo(ref dst)`.
    ///
    ///  2. Boxed CString reference semantics: CString is a class wrapping the
    ///     CStringRef struct; swapping `cstr.Value` swaps the whole buffer for
    ///     everyone reading through the box, but CStringRef snapshots taken
    ///     earlier (struct copies) keep pointing at the old buffer.
    ///
    /// NOTE: CStringSpan is a ref struct, so Assert.Throws lambdas cannot
    /// capture it (CS8175) - plain try/catch is used instead.
    /// </summary>
    public class CStringPitfallsTests
    {
        // ------------------------------------------------------------------
        // pitfall 1: CopyTo without `ref` bypasses the readonly guard
        // ------------------------------------------------------------------

        [Fact]
        public void CopyTo_WithRef_ReadonlyTarget_Throws()
        {
            // the guarded path: `ref` selects CopyTo(ref CStringSpan), which
            // refuses to write a readonly target
            var src = new CStringSpan("src");
            ReadOnlySpan<byte> ro = Encoding.UTF8.GetBytes("readonly!");
            var dst = new CStringSpan(ro);

            InvalidOperationException thrown = null;
            try { src.CopyTo(ref dst); }
            catch (InvalidOperationException ex) { thrown = ex; }
            Assert.NotNull(thrown);
        }

        [Fact]
        public void CopyTo_MissingRef_CompilesViaSpanOverload_AndWritesReadonlyTarget()
        {
            // THE PITFALL, locked in as a test: `dst` without `ref` converts
            // implicitly to Span<byte>, so the compiler picks
            // CopyTo(Span<byte>) - which has no readonly guard AND only
            // overwrites src's length (the tail of the old content survives,
            // unlike a full CopyTo(ref) replace). The write goes straight into
            // the "readonly" target's memory. If a future change removes the
            // implicit Span<byte> conversion or guards the Span overload, this
            // test failing is the reminder to update the docs.
            var src = new CStringSpan("src");
            ReadOnlySpan<byte> ro = Encoding.UTF8.GetBytes("readonly!");
            var dst = new CStringSpan(ro);

            src.CopyTo(dst);   // no `ref` - silent Span overload

            // the readonly buffer WAS overwritten: "readonly!" -> "src" + the
            // surviving tail "donly!" (partial overwrite semantics)
            Assert.Equal("srcdonly!", dst.ToString());
        }

        [Fact]
        public void CopyTo_MissingRef_LengthSemanticsDiffer()
        {
            // the Span overload also skips the ref-overload's length check:
            // CopyTo(ref) throws InvalidOperationException when the target is
            // too small, while the Span overload throws Span.CopyTo's own
            // ArgumentException instead (different exception type) - callers
            // that catch InvalidOperationException will miss it.
            var src = new CStringSpan("0123456789");
            var small = new byte[4];
            var dst = new CStringSpan(small);

            ArgumentException thrown = null;
            try { src.CopyTo(dst); }
            catch (ArgumentException ex) { thrown = ex; }
            Assert.NotNull(thrown);
        }

        // ------------------------------------------------------------------
        // pitfall 2: boxed CString - whole-buffer swap vs struct snapshots
        // ------------------------------------------------------------------

        [Fact]
        public void BoxedCString_ValueSwap_ReferenceSemantics()
        {
            var cstr = new CString("first");
            var snapshot = cstr.Value;          // struct copy: old buffer

            cstr.Value = new CStringRef("second");   // whole-buffer swap

            // readers going through the box see the new buffer...
            Assert.Equal("second", cstr.Value.ToString());
            // ...while the earlier struct snapshot still sees the old one
            Assert.Equal("first", snapshot.ToString());
        }

        [Fact]
        public void BoxedCString_GetRef_TracksValueSwap()
        {
            // ref access into the box tracks the swap (live view)
            var cstr = new CString("first");
            ref var live = ref cstr.GetRef();

            cstr.Value = new CStringRef("second");

            Assert.Equal("second", live.ToString());
        }
    }
}
