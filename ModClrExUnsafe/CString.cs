using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod.LowLevel
{
    public class Box<T> where T : struct
    {
        public T Value;
        public Box()
        {
        }
        public Box(T value)
        {
            Value = value;
        }

        public static implicit operator T(Box<T> box)
        {
            return box.Value;
        }
        public static implicit operator Box<T>(T value)
        {
            return new Box<T>(value);
        }
        public ref T GetRef()
        {
            return ref Value;
        }
    }

    public class DisposableBox<T> : Box<T>
        where T : struct, IDisposable
    {
        public DisposableBox() : base() { }
        public DisposableBox(T value) : base(value) { }

        private bool _DisposedValue;
        protected void Dispose(bool disposing)
        {
            if (!_DisposedValue)
            {
                if (disposing)
                {
                }

                DisposeImp();
                _DisposedValue = true;
            }
        }
        protected virtual void DisposeImp()
        {
            Value.Dispose();
            Value = default;
        }

        ~DisposableBox()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public static implicit operator T(DisposableBox<T> box)
        {
            return box.Value;
        }
        public static explicit operator DisposableBox<T>(T value)
        {
            return new DisposableBox<T>(value);
        }
    }

    /// <summary>
    /// string-like immutable wrapper over a <see cref="CStringRef"/>: every editing
    /// operation clones the underlying view and returns a NEW CString (the receiver
    /// stays unchanged), mirroring System.String semantics. Reads are transparently
    /// forwarded to <see cref="DisposableBox{T}.Value"/>.
    ///
    /// <para>
    /// The wrapped view is normalized on construction: it is either self-managed
    /// native (Dispose frees the AllocHGlobal block) or GC-tracked (Tracking mode).
    /// A Raw view that owns nothing gets Track()ed so the box keeps its target alive;
    /// IL2CPP has no Tracking mode, so Raw stays Raw there.
    /// </para>
    /// </summary>
    public sealed class CString : DisposableBox<CStringRef>,
        IEnumerable<byte>, IEnumerable, ICloneable, IDisposable,
        IComparable, IComparable<CString>, IComparable<string>,
        IEquatable<CString>, IEquatable<string>
    {
        public CString() : base()
        {
        }
        public CString(CStringRef value) : base(Normalize(value))
        {
        }
        public CString(string value) : base(Normalize(new CStringRef(value)))
        {
        }

        private static CStringRef Normalize(CStringRef value)
        {
            if (!value.IsSelfManagedNative && value.IsFixed
                && !RuntimeInfo.IsIl2Cpp)
            {
                value.Track();   // a Raw view that owns nothing: track it so the box keeps the target alive
            }
            return value;
        }

        // ---------- properties (forwarded) ----------

        public int Length => Value.Length;
        public int Size => Value.Size;
        public bool Readonly => Value.Readonly;
        public IntPtr Address => Value.Address;
        public bool IsFixed => Value.IsFixed;
        public CStringRef.RefIndexer Ref => Value.Ref;

        /// <summary>Read-only indexer: content edits produce new instances (string semantics).</summary>
        public byte this[int index] => Value[index];

        // ---------- state switches (in place: they change flags, not content) ----------

        public CString Freeze()
        {
            Value.Freeze();
            return this;
        }
        public CString Unfreeze()
        {
            Value.Unfreeze();
            return this;
        }
        public void ClearCache()
        {
            Value.ClearCache();
        }
        public void RecalculateLength()
        {
            Value.RecalculateLength();
        }

        // ---------- conversions ----------

        public static explicit operator string(CString thiz)
        {
            return thiz is null ? null : thiz.Value.ToString();
        }
        public static implicit operator CString(string s)
        {
            return s is null ? null : new CString(s);
        }
        public static implicit operator CStringRef(CString thiz)
        {
            return thiz is null ? default : thiz.Value;
        }
        public static implicit operator CString(CStringRef cstr)
        {
            return new CString(cstr);
        }
        public static implicit operator CStringSpan(CString thiz)
        {
            return thiz is null ? default : thiz.Value;
        }

        // ---------- ToString / equality / ordering ----------

        override public string ToString()
        {
            return Value.ToString();
        }
        public string ToString(Encoding encoding, bool ignoreCache = false)
        {
            return Value.ToString(encoding, ignoreCache);
        }
        public bool Equals(CString other)
        {
            return !(other is null) && Value.Equals(other.Value);
        }
        public bool Equals(string other)
        {
            return Value.Equals(other);
        }
        public override bool Equals(object obj)
        {
            if (obj is CString other)
            {
                return Equals(other);
            }
            else if (obj is string str)
            {
                return Equals(str);
            }
            return false;
        }
        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
        public static bool operator ==(CString left, CString right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }
            if (left is null || right is null)
            {
                return false;
            }
            return left.Value.Equals(right.Value);
        }
        public static bool operator !=(CString left, CString right)
        {
            return !(left == right);
        }
        public int CompareTo(CString other)
        {
            return other is null ? 1 : Value.CompareTo(other.Value);
        }
        public int CompareTo(string other)
        {
            return Value.CompareTo(other);
        }
        int IComparable.CompareTo(object obj)
        {
            if (obj is CString other)
            {
                return CompareTo(other);
            }
            else if (obj is string str)
            {
                return CompareTo(str);
            }
            throw new ArgumentException("Object is not a CString or string", nameof(obj));
        }

        // ---------- reads (forwarded) ----------

        public int IndexOf(byte value, int startIndex = 0) => Value.IndexOf(value, startIndex);
        public int IndexOf(CStringRef value, int startIndex = 0) => Value.IndexOf(value, startIndex);
        public int IndexOf(ReadOnlySpan<byte> value, int startIndex = 0) => Value.IndexOf(value, startIndex);
        public int IndexOf(Span<byte> value, int startIndex = 0) => Value.IndexOf(value, startIndex);
        public bool Contains(byte value) => Value.Contains(value);
        public bool Contains(CStringRef value) => Value.Contains(value);
        public bool Contains(ReadOnlySpan<byte> value) => Value.Contains(value);
        public bool Contains(Span<byte> value) => Value.Contains(value);
        public int LastIndexOf(byte value) => Value.LastIndexOf(value);
        public int LastIndexOf(CStringRef value) => Value.LastIndexOf(value);
        public int LastIndexOf(ReadOnlySpan<byte> value) => Value.LastIndexOf(value);
        public int LastIndexOf(Span<byte> value) => Value.LastIndexOf(value);
        public int IndexOfAny(ReadOnlySpan<byte> anyOf) => Value.IndexOfAny(anyOf);
        public int IndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex) => Value.IndexOfAny(anyOf, startIndex);
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf) => Value.LastIndexOfAny(anyOf);
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex) => Value.LastIndexOfAny(anyOf, startIndex);
        public bool StartsWith(byte value) => Value.StartsWith(value);
        public bool StartsWith(CStringRef value) => Value.StartsWith(value);
        public bool StartsWith(ReadOnlySpan<byte> value) => Value.StartsWith(value);
        public bool StartsWith(Span<byte> value) => Value.StartsWith(value);
        public bool EndsWith(byte value) => Value.EndsWith(value);
        public bool EndsWith(CStringRef value) => Value.EndsWith(value);
        public bool EndsWith(ReadOnlySpan<byte> value) => Value.EndsWith(value);
        public bool EndsWith(Span<byte> value) => Value.EndsWith(value);

        // ---------- search: string (UTF-8) and CString overloads ----------

        public int IndexOf(string value, int startIndex = 0) => IndexOfUtf8(value, startIndex);
        public int IndexOf(CString value, int startIndex = 0)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return IndexOf(value.Value, startIndex);
        }
        public int LastIndexOf(string value) => LastIndexOfUtf8(value);
        public int LastIndexOf(CString value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return LastIndexOf(value.Value);
        }
        public bool Contains(string value) => IndexOfUtf8(value, 0) >= 0;
        public bool Contains(CString value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return Contains(value.Value);
        }
        public bool StartsWith(string value) => StartsWithUtf8(value);
        public bool StartsWith(CString value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return StartsWith(value.Value);
        }
        public bool EndsWith(string value) => EndsWithUtf8(value);
        public bool EndsWith(CString value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            return EndsWith(value.Value);
        }

        /// <summary>Needles up to this many bytes are encoded on the stack; larger ones fall back to a heap array.</summary>
        private const int Utf8NeedleStackallocLimit = 256;

        // The stackalloc needle must stay inside the helper (a Span cannot escape the
        // method that allocated it), so each search shape gets its own encode-then-dispatch.
        private int IndexOfUtf8(string value, int startIndex)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return IndexOf(default(ReadOnlySpan<byte>), startIndex);
            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (byteCount <= Utf8NeedleStackallocLimit)
            {
                Span<byte> needle = stackalloc byte[byteCount];
                CStringRef.EncodeInto(value, Encoding.UTF8, needle, 0, byteCount);
                return IndexOf((ReadOnlySpan<byte>)needle, startIndex);
            }
            return IndexOf((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(value), startIndex);
        }
        private int LastIndexOfUtf8(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return LastIndexOf(default(ReadOnlySpan<byte>));
            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (byteCount <= Utf8NeedleStackallocLimit)
            {
                Span<byte> needle = stackalloc byte[byteCount];
                CStringRef.EncodeInto(value, Encoding.UTF8, needle, 0, byteCount);
                return LastIndexOf((ReadOnlySpan<byte>)needle);
            }
            return LastIndexOf((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(value));
        }
        private bool StartsWithUtf8(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return true;
            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (byteCount <= Utf8NeedleStackallocLimit)
            {
                Span<byte> needle = stackalloc byte[byteCount];
                CStringRef.EncodeInto(value, Encoding.UTF8, needle, 0, byteCount);
                return StartsWith((ReadOnlySpan<byte>)needle);
            }
            return StartsWith((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(value));
        }
        private bool EndsWithUtf8(string value)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            if (value.Length == 0) return true;
            int byteCount = Encoding.UTF8.GetByteCount(value);
            if (byteCount <= Utf8NeedleStackallocLimit)
            {
                Span<byte> needle = stackalloc byte[byteCount];
                CStringRef.EncodeInto(value, Encoding.UTF8, needle, 0, byteCount);
                return EndsWith((ReadOnlySpan<byte>)needle);
            }
            return EndsWith((ReadOnlySpan<byte>)Encoding.UTF8.GetBytes(value));
        }

        public bool IsNullTerminated() => Value.IsNullTerminated();
        public bool IsNull() => Value.IsNull();
        public bool IsEmpty() => Value.IsEmpty();
        public bool IsWhiteSpace() => Value.IsWhiteSpace();
        public CStringSpan Substring(int startIndex) => Value.Substring(startIndex);
        public CStringSpan Substring(int startIndex, int length) => Value.Substring(startIndex, length);
        public CStringSpanSplitEnumerator Split(ReadOnlySpan<byte> splitters) => Value.Split(splitters);
        public CStringSpanSplitEnumerator Split(byte splitter) => Value.Split(splitter);
        public CStringSpanSplitEnumerator Split() => Value.Split();
        public Span<byte> ToSpan() => Value.ToSpan();
        public Span<byte> Slice() => Value.Slice();
        public Span<byte> Slice(int start) => Value.Slice(start);
        public Span<byte> Slice(int start, int length) => Value.Slice(start, length);
        public void CopyTo(ref CStringRef to) => Value.CopyTo(ref to);
        public void CopyTo(Span<byte> to) => Value.CopyTo(to);
        public CStringRef.Enumerator GetEnumerator() => Value.GetEnumerator();
        IEnumerator<byte> IEnumerable<byte>.GetEnumerator() => Value.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => Value.GetEnumerator();

        // ---------- clone / concat ----------

        public CString Clone()
        {
            return new CString(Value.Clone());
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public static CString operator +(CString a, CString b)
        {
            return a.Append(b.Value);
        }
        public static CString operator +(CString a, CStringRef b)
        {
            return a.Append(b);
        }

        // ---------- edits: clone the view, mutate the clone, return a NEW CString ----------

        public CString Append(byte value)
        {
            return new CString(Value.Clone().Append(value));
        }
        public CString Append(CStringRef other)
        {
            return new CString(Value.Clone().Append(other));
        }
        public CString Append(ReadOnlySpan<byte> value)
        {
            return new CString(Value.Clone().Append(value));
        }
        public CString Append(string str)
        {
            return new CString(Value.Clone().Append(str));
        }
        public CString Append(string str, Encoding enc)
        {
            return new CString(Value.Clone().Append(str, enc));
        }
        public CString Insert(int index, byte ch)
        {
            return new CString(Value.Clone().Insert(index, ch));
        }
        public CString Insert(int index, byte ch, int repeat)
        {
            return new CString(Value.Clone().Insert(index, ch, repeat));
        }
        public CString Insert(int index, CStringRef sub)
        {
            return new CString(Value.Clone().Insert(index, sub));
        }
        public CString Insert(int index, CStringRef sub, int repeat)
        {
            return new CString(Value.Clone().Insert(index, sub, repeat));
        }
        public CString Insert(int index, ReadOnlySpan<byte> sub)
        {
            return new CString(Value.Clone().Insert(index, sub));
        }
        public CString Insert(int index, ReadOnlySpan<byte> sub, int repeat)
        {
            return new CString(Value.Clone().Insert(index, sub, repeat));
        }
        public CString Insert(int index, string str)
        {
            return new CString(Value.Clone().Insert(index, str));
        }
        public CString Insert(int index, string str, Encoding enc)
        {
            return new CString(Value.Clone().Insert(index, str, enc));
        }
        public CString Replace(byte oldch, byte newch)
        {
            return new CString(Value.Clone().Replace(oldch, newch));
        }
        public CString Replace(byte oldch, byte newch, int startIndex, int count)
        {
            return new CString(Value.Clone().Replace(oldch, newch, startIndex, count));
        }
        public CString Replace(CStringRef oldstr, CStringRef newstr)
        {
            return new CString(Value.Clone().Replace(oldstr, newstr));
        }
        public CString Replace(CStringRef oldstr, CStringRef newstr, int startIndex, int count)
        {
            return new CString(Value.Clone().Replace(oldstr, newstr, startIndex, count));
        }
        public CString Replace(ReadOnlySpan<byte> oldstr, ReadOnlySpan<byte> newstr)
        {
            return new CString(Value.Clone().Replace(oldstr, newstr));
        }
        public CString Replace(ReadOnlySpan<byte> oldstr, ReadOnlySpan<byte> newstr, int startIndex, int count)
        {
            return new CString(Value.Clone().Replace(oldstr, newstr, startIndex, count));
        }
        public CString Trim(ReadOnlySpan<byte> chars = default)
        {
            return new CString(Value.Clone().Trim(chars));
        }
        public CString TrimEnd(ReadOnlySpan<byte> chars = default)
        {
            return new CString(Value.Clone().TrimEnd(chars));
        }
        public CString TrimStart(ReadOnlySpan<byte> chars = default)
        {
            return new CString(Value.Clone().TrimStart(chars));
        }
        public CString Remove(int startIndex, int length)
        {
            return new CString(Value.Clone().Remove(startIndex, length));
        }
        public CString Clear()
        {
            return new CString(Value.Clone().Clear());
        }
        public CString Resize(int newsize)
        {
            return new CString(Value.Clone().Resize(newsize));
        }
        public CString Expand(int newsize)
        {
            return new CString(Value.Clone().Expand(newsize));
        }

        // ---------- null helpers ----------

        public static bool IsNullOrEmpty(CString thiz)
        {
            return thiz == null || thiz.Value.IsEmpty();
        }
        public static bool IsNullOrWhiteSpace(CString thiz)
        {
            return thiz == null || thiz.Value.IsWhiteSpace();
        }
    }
    public static class BoxExtensions
    {
        public static ref T Unbox<T>(this object box)
        {
            return ref UnsafeEx.GetFieldRef<T>(box, 0);
        }
    }
}
