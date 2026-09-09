using System;
using System.Text;

namespace Mod.Tests
{
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
