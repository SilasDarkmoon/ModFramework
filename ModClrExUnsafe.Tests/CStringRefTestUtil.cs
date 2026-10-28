using System;
using System.Text;
using Mod.LowLevel;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// Test-side helpers for building CStringRef instances.
    ///
    /// <para>
    /// CONVENTION - dispose every CStringRef local with <c>Dispose()</c>.
    /// </para>
    /// <para>
    /// <c>CStringRef._Address</c> is a <c>RawTrackingRef&lt;byte&gt;</c>: a byref slot on the dedicated
    /// tracking thread's stack that the GC follows as an interior byref. The slot is leased by
    /// <c>RawTrackingRef.Create()</c> (every constructor of CStringRef goes through it) and is only
    /// returned by <c>Dispose()</c>. A view that is never disposed therefore leaks its slot and
    /// keeps its target buffer alive for the rest of the process.
    /// </para>
    /// <para>
    /// Every test disposes its CStringRef locals before returning, native-backed views included
    /// (a native target needs no GC protection, but the slot lease exists either way). Those
    /// Dispose calls are load-bearing - please do not remove them as "noise". Instances that are
    /// purely temporary and passed straight into a call should be built as <c>CStringSpan</c>
    /// where a ReadOnlySpan overload exists (e.g. <c>s.IndexOf(new CStringSpan("ab"))</c>): a span
    /// takes no slot at all. Methods without a ReadOnlySpan overload (Append / Insert / Replace)
    /// keep their <c>CStringRef</c> temporaries.
    /// </para>
    /// </summary>
    internal static class CStr
    {
        /// <summary>Builds a mutable CStringRef over an ASCII/UTF8 text buffer with extra zero-filled capacity.</summary>
        public static CStringRef New(string text, int extraCapacity = 0)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var buffer = new byte[bytes.Length + extraCapacity];
            Array.Copy(bytes, buffer, bytes.Length);
            return new CStringRef(new Span<byte>(buffer));
        }

        /// <summary>
        /// CStringSpan counterpart of the removed (string, capacity) constructor:
        /// new CStringSpan(max(capacity, byteCount) + 1).Append(text) - the
        /// size-ctor + Append replacement pattern, Size exactly as that ctor produced.
        /// </summary>
        public static Mod.LowLevel.CStringSpan NewSpan(string text, int capacity = 0)
        {
            if (text == null)
            {
                return default;
            }
            int size = Math.Max(capacity, Encoding.UTF8.GetByteCount(text)) + 1;
            return new Mod.LowLevel.CStringSpan(size).Append(text);
        }

        /// <summary>Reads the current logical content as a fresh byte array.</summary>
        public static byte[] Content(CStringRef s)
        {
            var result = new byte[s.Length];
            for (int i = 0; i < s.Length; ++i)
            {
                result[i] = s[i];
            }
            return result;
        }

        public static string Text(CStringRef s) => s.ToString();

        public static byte Ch(char c) => (byte)c;

        public static byte[] Bytes(string asciiText) => Encoding.ASCII.GetBytes(asciiText);
    }
}
