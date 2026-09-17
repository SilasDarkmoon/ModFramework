using System;
using System.Runtime.CompilerServices;
using System.Text;
using Mod;

namespace Mod.LowLevel
{
    /// <summary>
    /// Span-backed variant of <see cref="Mod.CStringRef"/> — the "safe by construction" candidate.
    ///
    /// Essential difference from CStringRef:
    ///  _Address is a Span&lt;byte&gt; (the official byref carrier) instead of a
    ///  RefContainer&lt;byte&gt; (an object slot holding a byref bit pattern — a lying GC slot).
    ///  The GC tracks it as an interior byref for its whole lifetime:
    ///  mark keeps the owner alive, relocate moves it with compaction — correct under
    ///  BGC / foreground GC / any GC plan. No "IntPtr intermediate → stale address"
    ///  minefield, no pinning required.
    ///
    /// Cost of being a ref struct (enforced by the language, not by convention):
    ///  no boxing, no fields in classes / ordinary structs, no arrays, no lambda or
    ///  iterator capture, no interface implementation (enumerators use the duck-typed
    ///  pattern instead), no static Empty field (use default), stack-only lifetime.
    ///
    /// Iron rules for mutating methods (same as CStringRef):
    ///  1. throw InvalidOperationException when _Readonly;
    ///  2. ClearCache() after every content/length change;
    ///  3. maintain _Length correctly; shrinking operations zero the new _Length
    ///     position to keep null termination — but _Length may equal Size, in which
    ///     case there is no room for the trailing '\0' and it is simply absent;
    ///  4. when growth exceeds capacity, allocate a new buffer and return a new
    ///     instance (callers must consume the return value).
    ///
    /// Read API mimics string; write API mimics StringBuilder.
    /// </summary>
    public ref struct CStringSpan
    {
        private Span<byte> _Address;
        private int _Length;
        internal bool _Readonly;
        internal string _ConvertedString;
        internal int _CachedHashCode;

        public int Length
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _Length;
            set
            {
                if (value != _Length)
                {
                    if (_Readonly)
                    {
                        throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
                    }
                    if (value < 0 || value > _Address.Length)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                    if (value < _Length)
                    {
                        _Address.Slice(value, _Length - value).Clear();
                    }
                    _Length = value;
                    ClearCache();
                }
            }
        }
        public int Size => _Address.Length;
        public bool Readonly => _Readonly;
        public void Freeze()
        {
            if (!_Readonly)
            {
                RecalculateLength();
                _Readonly = true;
            }
        }
        public void Unfreeze()
        {
            _Readonly = false;
        }
        public void ClearCache()
        {
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public void RecalculateLength()
        {
            var newlen = UnsafeEx.StrLen(_Address);
            if (_Length != newlen)
            {
                ClearCache();
                _Length = newlen;
            }
        }

        public CStringSpan(IntPtr address, int size, int len)
        {
            if (address == IntPtr.Zero)
            {
                this = default;
                return;
            }
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            unsafe
            {
                _Address = new Span<byte>(address.ToPointer(), size);
            }
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(IntPtr address, int size)
        {
            if (address == IntPtr.Zero)
            {
                this = default;
                return;
            }
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            unsafe
            {
                _Address = new Span<byte>(address.ToPointer(), size);
            }
            _Length = UnsafeEx.StrLen(_Address);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(IntPtr address)
        {
            if (address == IntPtr.Zero)
            {
                this = default;
                return;
            }
            _Length = UnsafeEx.StrLen(address);
            unsafe
            {
                _Address = new Span<byte>(address.ToPointer(), _Length + 1);
            }
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(ref byte address, int size, int len)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            // ref → Span: AsSpan's double-read loop guarantees the address did not
            // move inside the creation window.
            _Address = UnsafeEx.AsSpan<byte>(ref address, size);
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(ref byte address, int size)
        {
            if (size < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size));
            }
            _Address = UnsafeEx.AsSpan<byte>(ref address, size);
            _Length = UnsafeEx.StrLen(_Address);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(ref byte address)
        {
            _Length = UnsafeEx.StrLen(ref address);
            _Address = UnsafeEx.AsSpan<byte>(ref address, _Length + 1);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(Span<byte> data)
        {
            _Address = data;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(ReadOnlySpan<byte> data)
        {
            // ReadOnlySpan → Span reinterpretation (same semantics as
            // CStringRef(ReadOnlySpan)); writes are guarded by _Readonly.
            _Address = data.Unprotect();
            _Length = UnsafeEx.StrLen(data);
            _Readonly = true;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(Span<byte> data, int length)
        {
            _Address = data;
            _Length = Math.Min(length, _Address.Length);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(ReadOnlySpan<byte> data, int length)
        {
            _Address = data.Unprotect();
            _Length = Math.Min(length, _Address.Length);
            _Readonly = true;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(string value)
        {
            if (value == null)
            {
                this = default;
                return;
            }
            var buffer = new byte[Encoding.UTF8.GetByteCount(value) + 1];
            int len = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            _Address = buffer;
            _Length = len;
            _Readonly = false;
            // No pre-caching: external writes (RefIndexer / ToSpan) bypass the
            // indexer setter and would leave a stale _ConvertedString behind.
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(string value, int capacity)
        {
            if (value == null)
            {
                this = default;
                return;
            }
            int bytes = Encoding.UTF8.GetByteCount(value);
            var buffer = new byte[Math.Max(capacity, bytes) + 1];
            int len = Encoding.UTF8.GetBytes(value, 0, value.Length, buffer, 0);
            _Address = buffer;
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(string value, Encoding encoding)
        {
            if (value == null)
            {
                this = default;
                return;
            }
            if (encoding == null)
            {
                encoding = Encoding.Default;
            }
            var buffer = new byte[encoding.GetByteCount(value) + 1];
            int len = encoding.GetBytes(value, 0, value.Length, buffer, 0);
            _Address = buffer;
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(byte[] buffer)
        {
            _Address = buffer == null ? default : new Span<byte>(buffer);
            _Length = UnsafeEx.StrLen(_Address);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringSpan(byte[] buffer, int length)
        {
            _Address = buffer == null ? default : new Span<byte>(buffer);
            _Length = Math.Min(length, _Address.Length);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }

        public byte this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if (index < 0 || index >= _Address.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                return _Address[index];
            }
            set
            {
                if (index < 0 || index >= _Address.Length)
                {
                    throw new IndexOutOfRangeException();
                }
                else if (_Readonly)
                {
                    throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
                }
                else
                {
                    _Address[index] = value;
                    if (value == 0 && index < _Length)
                    {
                        _Length = index;
                    }
                    else if (value != 0 && index >= _Length)
                    {
                        _Length = index + 1;
                    }
                    ClearCache();   // overwriting an existing byte also invalidates cached string/hash
                }
            }
        }

        /// <summary>
        /// Live reference accessor struct (mirrors CStringRef.RefIndexer).
        /// Must be a ref struct itself: it holds a Span&lt;byte&gt; field, which an
        /// ordinary struct cannot contain (CS8345).
        /// </summary>
        public ref struct RefIndexer
        {
            private Span<byte> _Address;

            public RefIndexer(CStringSpan s)
            {
                _Address = s._Address;
            }
            public ref byte this[int index]
            {
                get
                {
                    if (index < 0 || index >= _Address.Length)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return ref _Address[index];
                }
            }
        }
        public RefIndexer Ref => new RefIndexer(this);

        override public string ToString()
        {
            if (_ConvertedString == null)
            {
                if (IsEmpty())
                {
                    return _ConvertedString = string.Empty;
                }
                unsafe
                {
                    fixed (byte* ptr = _Address)
                    {
                        _ConvertedString = Encoding.UTF8.GetString(ptr, _Length);
                    }
                }
            }
            return _ConvertedString;
        }

        public CStringSpanEnumerator GetEnumerator()
        {
            return new CStringSpanEnumerator(this);
        }

        public void CopyTo(ref CStringSpan to)
        {
            if (to._Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (_Length > to._Address.Length)
            {
                throw new InvalidOperationException("Destination too small");
            }
            _Address.Slice(0, _Length).CopyTo(to._Address);
            to._Length = _Length;
            to.ClearCache();
        }
        public void CopyFrom(CStringSpan from)
        {
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (from._Length > _Address.Length)
            {
                throw new InvalidOperationException("Destination too small");
            }
            from._Address.Slice(0, from._Length).CopyTo(_Address);
            _Length = from._Length;
            ClearCache();
        }
        public void CopyTo(Span<byte> to)
        {
            _Address.Slice(0, _Length).CopyTo(to);
        }
        public CStringSpan Clone()
        {
            if (_Length == 0)
            {
                return new CStringSpan { _Readonly = _Readonly };
            }
            var buffer = new byte[_Length + 1];
            _Address.Slice(0, _Length).CopyTo(buffer);
            return new CStringSpan(buffer, _Length);
        }

        public int CompareTo(CStringSpan other)
        {
            var result = CompareTo((ReadOnlySpan<byte>)other._Address.Slice(0, other._Length));
            if (result != 0)
            {
                return result;
            }
            return _Length.CompareTo(other._Length);
        }
        public int CompareTo(string other)
        {
            return ToString().CompareTo(other);
        }
        public int CompareTo(ReadOnlySpan<byte> other)
        {
            int count = Math.Min(_Length, other.Length);
            for (int i = 0; i < count; ++i)
            {
                var cmp = _Address[i] - other[i];
                if (cmp < 0) return -1;
                if (cmp > 0) return 1;
            }
            return _Length.CompareTo(other.Length);
        }
        public static int Compare(CStringSpan a, CStringSpan b)
        {
            return a.CompareTo(b);
        }

        public bool Equals(CStringSpan other)
        {
            return _Address.Slice(0, _Length).SequenceEqual(other._Address.Slice(0, other._Length));
        }
        public bool Equals(string other)
        {
            return ToString().Equals(other);
        }
        public bool Equals(ReadOnlySpan<byte> other)
        {
            return _Address.Slice(0, _Length).SequenceEqual(other);
        }
        /// <summary>Unreachable: a ref struct cannot be boxed (silences CS0660 only).</summary>
        public override bool Equals(object obj)
        {
            throw new NotSupportedException("CStringSpan is a ref struct and cannot be boxed.");
        }
        public override int GetHashCode()
        {
            if (_CachedHashCode == 0)
            {
                int hash = 0;
                if (_Length > 0)
                {
                    var first = _Address[0];
                    hash |= (first << 28);
                    if (_Length > 1)
                    {
                        var last = _Address[_Length - 1];
                        hash |= (last & 0x0F);
                    }
                    for (int i = 0; i < _Length; ++i)
                    {
                        var b = _Address[i];
                        var offset = i / 3;
                        var group = i % 3;
                        var shift = (3 - group) * 8;
                        var codepart = ((int)b) << shift;
                        codepart >>= (offset % 8);
                        codepart = ((codepart & 0xFF) << 16) | (codepart >> 8);
                        hash ^= codepart << 4;
                    }
                }
                _CachedHashCode = hash;
            }
            return _CachedHashCode;
        }

        public static bool operator ==(CStringSpan left, CStringSpan right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CStringSpan left, CStringSpan right)
        {
            return !left.Equals(right);
        }
        public static CStringSpan operator +(CStringSpan a, CStringSpan b)
        {
            return a.Append(b);
        }

        public static implicit operator string(CStringSpan thiz)
        {
            return thiz.ToString();
        }
        public static explicit operator CStringSpan(string s)
        {
            return new CStringSpan(s);
        }

        public Span<byte> ToSpan()
        {
            return _Address;
        }
        public static implicit operator Span<byte>(CStringSpan thiz)
        {
            return thiz.ToSpan();
        }
        public static implicit operator ReadOnlySpan<byte>(CStringSpan thiz)
        {
            return thiz.ToSpan();
        }
        public static implicit operator CStringSpan(Span<byte> span)
        {
            return new CStringSpan(span);
        }
        public static implicit operator CStringSpan(ReadOnlySpan<byte> span)
        {
            return new CStringSpan(span);
        }

        public Span<byte> Slice()
        {
            return ToSpan().Slice(0, _Length);
        }
        public Span<byte> Slice(int start)
        {
            return ToSpan().Slice(start, _Length - start);
        }
        public Span<byte> Slice(int start, int length)
        {
            return ToSpan().Slice(start, length);
        }

        public CStringSpan Substring(int startIndex)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (startIndex >= _Address.Length)
            {
                return new CStringSpan { _Readonly = _Readonly };
            }
            else
            {
                return new CStringSpan(_Address.Slice(startIndex), _Length - startIndex) { _Readonly = _Readonly };
            }
        }
        public CStringSpan Substring(int startIndex, int length)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (length < 0 || length > _Length - startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            if (startIndex >= _Address.Length)
            {
                return new CStringSpan { _Readonly = _Readonly };
            }
            if (startIndex + length == _Length)
            {
                return new CStringSpan(_Address.Slice(startIndex), length) { _Readonly = _Readonly };
            }
            else
            {
                return new CStringSpan(_Address.Slice(startIndex, length), length) { _Readonly = _Readonly };
            }
        }

        public int IndexOf(byte value, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            return _Address.Slice(startIndex, _Length - startIndex).IndexOf(value) + startIndex;
        }
        public int IndexOf(ReadOnlySpan<byte> value, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            return _Address.Slice(startIndex, _Length - startIndex).IndexOf(value) + startIndex;
        }
        public bool Contains(byte value)
        {
            return _Address.Slice(0, _Length).IndexOf(value) >= 0;
        }
        public bool Contains(ReadOnlySpan<byte> value)
        {
            return _Address.Slice(0, _Length).IndexOf(value) >= 0;
        }
        public int LastIndexOf(byte value)
        {
            return _Address.Slice(0, _Length).LastIndexOf(value);
        }
        public int LastIndexOf(ReadOnlySpan<byte> value)
        {
            return _Address.Slice(0, _Length).LastIndexOf(value);
        }
        public int IndexOfAny()
        {
            return IndexOfAny(new byte[] { (byte)' ', (byte)'\t', (byte)'\r', (byte)'\n' });
        }
        public int IndexOfAny(ReadOnlySpan<byte> anyOf)
        {
            var slice = _Address.Slice(0, _Length);
            for (int i = 0; i < slice.Length; ++i)
            {
                for (int j = 0; j < anyOf.Length; ++j)
                {
                    if (slice[i] == anyOf[j]) return i;
                }
            }
            return -1;
        }
        public int IndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            var slice = _Address.Slice(startIndex, _Length - startIndex);
            for (int i = 0; i < slice.Length; ++i)
            {
                for (int j = 0; j < anyOf.Length; ++j)
                {
                    if (slice[i] == anyOf[j]) return i + startIndex;
                }
            }
            return -1;
        }
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf)
        {
            var slice = _Address.Slice(0, _Length);
            for (int i = slice.Length - 1; i >= 0; --i)
            {
                for (int j = 0; j < anyOf.Length; ++j)
                {
                    if (slice[i] == anyOf[j]) return i;
                }
            }
            return -1;
        }
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex)
        {
            if (startIndex < 0 || startIndex >= _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            var slice = _Address.Slice(0, _Length);
            for (int i = startIndex; i >= 0; --i)
            {
                for (int j = 0; j < anyOf.Length; ++j)
                {
                    if (slice[i] == anyOf[j]) return i;
                }
            }
            return -1;
        }
        public bool StartsWith(byte value)
        {
            return _Length > 0 && _Address[0] == value;
        }
        public bool StartsWith(ReadOnlySpan<byte> value)
        {
            return _Length >= value.Length && _Address.Slice(0, _Length).StartsWith(value);
        }
        public bool EndsWith(byte value)
        {
            return _Length > 0 && _Address[_Length - 1] == value;
        }
        public bool EndsWith(ReadOnlySpan<byte> value)
        {
            return _Length >= value.Length && _Address.Slice(0, _Length).EndsWith(value);
        }

        public static bool IsWhiteSpace(byte value)
        {
            return value == 0x20 || (value >= 0x09 && value <= 0x0D);
        }
        public bool IsEmpty()
        {
            return _Length == 0;
        }
        public bool IsWhiteSpace()
        {
            for (int i = 0; i < _Length; ++i)
            {
                if (!IsWhiteSpace(_Address[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public CStringSpanSplitEnumerator Split(ReadOnlySpan<byte> splitters)
        {
            return new CStringSpanSplitEnumerator(this, splitters);
        }
        public CStringSpanSplitEnumerator Split(byte splitter)
        {
            return new CStringSpanSplitEnumerator(this, splitter);
        }
        public CStringSpanSplitEnumerator Split()
        {
            return new CStringSpanSplitEnumerator(this, -1);
        }

        public CStringSpan Append(byte value)
        {
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (_Length + 1 > _Address.Length)        // content itself doesn't fit
            {
                var buffer = new byte[_Length + 2];
                _Address.Slice(0, _Length).CopyTo(buffer);
                buffer[_Length] = value;
                return new CStringSpan(buffer, _Length + 1);
            }
            _Address[_Length] = value;
            if (_Length + 1 < _Address.Length)        // room for the terminator
            {
                _Address[_Length + 1] = 0;
            }
            _Length += 1;
            ClearCache();
            return this;
        }
        public CStringSpan Append(ReadOnlySpan<byte> value)
        {
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (_Length + value.Length > _Address.Length)   // content itself doesn't fit
            {
                var buffer = new byte[_Length + value.Length + 1];
                _Address.Slice(0, _Length).CopyTo(buffer);
                value.CopyTo(buffer.AsSpan(_Length));
                return new CStringSpan(buffer, _Length + value.Length);
            }
            value.CopyTo(_Address.Slice(_Length));
            _Length += value.Length;
            if (_Length < _Address.Length)            // room for the terminator
            {
                _Address[_Length] = 0;
            }
            ClearCache();
            return this;
        }
        public CStringSpan Append(CStringSpan other)
        {
            return Append((ReadOnlySpan<byte>)other._Address.Slice(0, other._Length));
        }

        public CStringSpan Insert(int index, byte value)
        {
            return Insert(index, value, 1);
        }
        public CStringSpan Insert(int index, byte value, int repeat)
        {
            if (repeat == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (index < 0 || index > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (repeat < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }
            if (repeat > int.MaxValue - _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }
            int newLength = _Length + repeat;
            if (newLength > _Address.Length)
            {
                var buffer = new byte[newLength + 1];
                _Address.Slice(0, index).CopyTo(buffer);
                buffer.AsSpan(index, repeat).Fill(value);
                _Address.Slice(index, _Length - index).CopyTo(buffer.AsSpan(index + repeat));
                return new CStringSpan(buffer, newLength);
            }
            // In-place right shift: Span.CopyTo has memmove semantics (CoreCLR
            // Buffer.Memmove handles overlap).
            _Address.Slice(index, _Length - index).CopyTo(_Address.Slice(index + repeat));
            _Address.Slice(index, repeat).Fill(value);
            _Length = newLength;
            if (_Length < _Address.Length)            // room for the terminator
            {
                _Address[_Length] = 0;
            }
            ClearCache();
            return this;
        }

        public CStringSpan Remove(int startIndex, int length)
        {
            if (length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            if (startIndex < 0 || startIndex >= _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (length < 0 || length > _Length - startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            int newLength = _Length - length;
            // Left shift: dst is always before src, forward copy is safe.
            _Address.Slice(startIndex + length, newLength - startIndex).CopyTo(_Address.Slice(startIndex));
            _Length = newLength;
            _Address[_Length] = 0;                    // shrinking: always in range
            ClearCache();
            return this;
        }

        public CStringSpan Replace(ReadOnlySpan<byte> find, ReadOnlySpan<byte> replace)
        {
            if (find.Length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            // Count non-overlapping matches (left to right).
            int matchCount = 0;
            int pos = 0;
            var content = _Address.Slice(0, _Length);
            while (pos <= content.Length - find.Length)
            {
                if (content.Slice(pos, find.Length).SequenceEqual(find))
                {
                    matchCount++;
                    pos += find.Length;
                }
                else
                {
                    pos++;
                }
            }
            if (matchCount == 0)
            {
                return this;
            }
            int delta = (replace.Length - find.Length) * matchCount;
            int newLength = _Length + delta;
            if (replace.Length > find.Length || newLength > _Address.Length)
            {
                // Growth path: new buffer (in-place growth would corrupt the read
                // window while writing).
                var buffer = new byte[newLength + 1];
                int srcPos = 0, dstPos = 0;
                while (srcPos < _Length)
                {
                    if (srcPos <= _Length - find.Length && content.Slice(srcPos, find.Length).SequenceEqual(find))
                    {
                        replace.CopyTo(buffer.AsSpan(dstPos));
                        dstPos += replace.Length;
                        srcPos += find.Length;
                    }
                    else
                    {
                        buffer[dstPos++] = _Address[srcPos++];
                    }
                }
                return new CStringSpan(buffer, newLength);
            }
            // Shrink / equal-length path: in place (dst <= src always holds, so
            // writes never pollute the unread region).
            int src = 0, dst = 0;
            while (src < _Length)
            {
                if (src <= _Length - find.Length && content.Slice(src, find.Length).SequenceEqual(find))
                {
                    replace.CopyTo(_Address.Slice(dst));
                    dst += replace.Length;
                    src += find.Length;
                }
                else
                {
                    _Address[dst++] = _Address[src++];
                }
            }
            _Length = newLength;
            if (_Length < _Address.Length)            // room for the terminator
            {
                _Address[_Length] = 0;
            }
            ClearCache();
            return this;
        }

        public CStringSpan Trim(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int start = 0;
            while (start < _Length && ShouldTrim(_Address[start], chars))
            {
                ++start;
            }
            int end = _Length;
            while (end > start && ShouldTrim(_Address[end - 1], chars))
            {
                --end;
            }
            if (start == 0 && end == _Length)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            int newLength = end - start;
            if (newLength > 0)
            {
                _Address.Slice(start, newLength).CopyTo(_Address);
            }
            Length = newLength;
            return this;
        }
        public CStringSpan TrimEnd(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int end = _Length;
            while (end > 0 && ShouldTrim(_Address[end - 1], chars))
            {
                --end;
            }
            if (end == _Length)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            Length = end;
            return this;
        }
        public CStringSpan TrimStart(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int start = 0;
            while (start < _Length && ShouldTrim(_Address[start], chars))
            {
                ++start;
            }
            if (start == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            int newLength = _Length - start;
            _Address.Slice(start, newLength).CopyTo(_Address);
            Length = newLength;
            return this;
        }

        public CStringSpan Clear()
        {
            if (_Length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringSpan");
            }
            _Address.Clear();
            _Length = 0;
            ClearCache();
            return this;
        }

        private static bool ShouldTrim(byte value, ReadOnlySpan<byte> chars)
        {
            if (chars.Length == 0)
            {
                return IsWhiteSpace(value);
            }
            for (int i = 0; i < chars.Length; ++i)
            {
                if (chars[i] == value)
                {
                    return true;
                }
            }
            return false;
        }
    }

    /// <summary>
    /// Byte enumerator (ref struct, duck-typed pattern: Current / MoveNext / GetEnumerator).
    /// </summary>
    public ref struct CStringSpanEnumerator
    {
        private Span<byte> _span;
        private int _index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal CStringSpanEnumerator(CStringSpan s)
        {
            _span = s.ToSpan().Slice(0, s.Length);
            _index = -1;
        }

        public byte Current
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => _span[_index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MoveNext()
        {
            return ++_index < _span.Length;
        }

        public CStringSpanEnumerator GetEnumerator()
        {
            return this;
        }
    }

    /// <summary>
    /// Split enumerator (ref struct, duck-typed pattern — a ref struct cannot return
    /// arrays, so foreach is used instead).
    /// Semantics match CStringRef.Split: empty fields are preserved
    /// ("a,,b," → a, empty, b, empty).
    /// </summary>
    public ref struct CStringSpanSplitEnumerator
    {
        private Span<byte> _remaining;
        private byte _singleSep;
        private ReadOnlySpan<byte> _multiSeps;
        private bool _whitespaceMode;
        private bool _multiMode;
        private bool _consumedSeparator;   // whether the previous yield consumed a separator (decides the trailing empty field)
        private bool _finished;
        private CStringSpan _current;

        internal CStringSpanSplitEnumerator(CStringSpan s, byte separator)
        {
            _remaining = s.ToSpan().Slice(0, s.Length);
            _singleSep = separator;
            _multiSeps = default;
            _whitespaceMode = false;
            _multiMode = false;
            _consumedSeparator = false;
            _finished = false;
            _current = default;
        }

        internal CStringSpanSplitEnumerator(CStringSpan s, int whitespaceMarker)
        {
            _remaining = s.ToSpan().Slice(0, s.Length);
            _singleSep = 0;
            _multiSeps = default;
            _whitespaceMode = true;
            _multiMode = false;
            _consumedSeparator = false;
            _finished = false;
            _current = default;
        }

        internal CStringSpanSplitEnumerator(CStringSpan s, ReadOnlySpan<byte> separators)
        {
            _remaining = s.ToSpan().Slice(0, s.Length);
            _singleSep = 0;
            _multiSeps = separators;
            _whitespaceMode = false;
            _multiMode = true;
            _consumedSeparator = false;
            _finished = false;
            _current = default;
        }

        // CStringRef entry points: the source converts to a CStringSpan view (no
        // extra slot), then the enumeration itself is slot-free.
        internal CStringSpanSplitEnumerator(CStringRef s, byte separator)
            : this((CStringSpan)s, separator)
        {
        }

        internal CStringSpanSplitEnumerator(CStringRef s, int whitespaceMarker)
            : this((CStringSpan)s, whitespaceMarker)
        {
        }

        internal CStringSpanSplitEnumerator(CStringRef s, ReadOnlySpan<byte> separators)
            : this((CStringSpan)s, separators)
        {
        }

        public CStringSpan Current
        {
            get => _current;
        }

        public bool MoveNext()
        {
            if (_whitespaceMode)
            {
                return MoveNextWhitespace();
            }
            if (_multiMode)
            {
                return MoveNextMulti();
            }
            return MoveNextSingle();
        }

        private bool MoveNextSingle()
        {
            if (_finished)
            {
                return false;
            }
            if (_remaining.IsEmpty)
            {
                // Right after a trailing separator → yield the final empty field
                // (the 4th part of "a,,b,").
                if (_consumedSeparator)
                {
                    _consumedSeparator = false;
                    _finished = true;
                    _current = default;
                    return true;
                }
                _finished = true;
                return false;
            }
            int sepIndex = _remaining.IndexOf(_singleSep);
            if (sepIndex < 0)
            {
                _current = new CStringSpan(_remaining, _remaining.Length);
                _remaining = default;
                _finished = true;
                return true;
            }
            _current = new CStringSpan(_remaining.Slice(0, sepIndex), sepIndex);
            _remaining = _remaining.Slice(sepIndex + 1);
            _consumedSeparator = true;
            return true;
        }

        private bool MoveNextWhitespace()
        {
            // Skip leading whitespace.
            while (!_remaining.IsEmpty && CStringSpan.IsWhiteSpace(_remaining[0]))
            {
                _remaining = _remaining.Slice(1);
            }
            if (_remaining.IsEmpty)
            {
                return false;
            }
            int end = 0;
            while (end < _remaining.Length && !CStringSpan.IsWhiteSpace(_remaining[end]))
            {
                ++end;
            }
            _current = new CStringSpan(_remaining.Slice(0, end), end);
            _remaining = _remaining.Slice(end);
            return true;
        }

        private bool MoveNextMulti()
        {
            if (_finished)
            {
                return false;
            }
            if (_remaining.IsEmpty)
            {
                if (_consumedSeparator)
                {
                    _consumedSeparator = false;
                    _finished = true;
                    _current = default;
                    return true;
                }
                _finished = true;
                return false;
            }
            int sepIndex = FindAny(_remaining);
            if (sepIndex < 0)
            {
                _current = new CStringSpan(_remaining, _remaining.Length);
                _remaining = default;
                _finished = true;
                return true;
            }
            _current = new CStringSpan(_remaining.Slice(0, sepIndex), sepIndex);
            _remaining = _remaining.Slice(sepIndex + 1);
            _consumedSeparator = true;
            return true;
        }

        private int FindAny(Span<byte> span)
        {
            for (int i = 0; i < span.Length; ++i)
            {
                for (int j = 0; j < _multiSeps.Length; ++j)
                {
                    if (span[i] == _multiSeps[j]) return i;
                }
            }
            return -1;
        }

        public CStringSpanSplitEnumerator GetEnumerator()
        {
            return this;
        }
    }
}
