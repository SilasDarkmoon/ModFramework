using Mod.LowLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public struct CStringRef : IEnumerable<byte>, IEnumerable, ICloneable, IComparable, IComparable<CStringRef>, IComparable<string>, IEquatable<CStringRef>, IEquatable<string>, IDisposable
    {
        private UnionRef<byte> _Address;
        private int _Size;
        private int _Length;
        private bool _Readonly;
        private string _ConvertedString;
        private int _CachedHashCode;

        public int Length
        {
            get => _Length;
            set
            {
                if (value != _Length)
                {
                    if (_Readonly)
                    {
                        throw new InvalidOperationException("Cannot modify a readonly CStringRef");
                    }
                    if (value < 0 || value > _Size)
                    {
                        throw new ArgumentOutOfRangeException(nameof(value));
                    }
                    if (value < _Length)
                    {
                        UnsafeEx.MemClean(ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)value), 0, _Length - value);
                    }
                    _Length = value;
                    ClearCache();
                }
            }
        }
        public int Size => _Size;
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
            var newlen = UnsafeEx.StrLen(ref _Address.GetRef(), _Size);
            if (_Length != newlen)
            {
                ClearCache();
                _Length = newlen;
            }
        }

        public static readonly CStringRef Empty = default;

        private static UnionRef.RefType _RefType = RuntimeInfo.Runtime == RuntimeInfo.RuntimeCategory.NetFramework || RuntimeInfo.Runtime == RuntimeInfo.RuntimeCategory.NetCore ? UnionRef.RefType.Tracking : UnionRef.RefType.Raw;
        public CStringRef(IntPtr address, int size, int len)
        {
            _Address = new UnionRef<byte>(UnionRef.RefType.Raw); // Native address -> Raw
            _Address.Address = address;
            _Size = size;
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(IntPtr address, int size)
        {
            _Address = new UnionRef<byte>(UnionRef.RefType.Raw); // Native address -> Raw
            _Address.Address = address;
            _Size = size;
            _Length = UnsafeEx.StrLen(address, size);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(IntPtr address)
        {
            _Address = new UnionRef<byte>(UnionRef.RefType.Raw); // Native address -> Raw
            _Address.Address = address;
            _Length = UnsafeEx.StrLen(address);
            _Size = _Length + 1;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ref byte address, int size, int len)
        {
            _Address = new UnionRef<byte>(_RefType); // ref -> auto
            _Address.SetRef(ref address);
            _Size = size;
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ref byte address, int size)
        {
            _Address = new UnionRef<byte>(_RefType); // ref -> auto
            _Address.SetRef(ref address);
            _Size = size;
            _Length = UnsafeEx.StrLen(ref address, size);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ref byte address)
        {
            _Address = new UnionRef<byte>(_RefType); // ref -> auto
            _Address.SetRef(ref address);
            _Length = UnsafeEx.StrLen(ref address);
            _Size = _Length + 1;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(Span<byte> data)
        {
            _Address = new UnionRef<byte>(_RefType); // span -> auto
            _Address.SetRef(ref data.GetRef());
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ReadOnlySpan<byte> data)
        {
            _Address = new UnionRef<byte>(_RefType); // span -> auto
            _Address.SetRef(ref data.GetRef());
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = true;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(Span<byte> data, int length)
        {
            _Address = new UnionRef<byte>(_RefType); // span -> auto
            _Address.SetRef(ref data.GetRef());
            _Size = data.Length;
            _Length = Math.Min(length, _Size);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ReadOnlySpan<byte> data, int length)
        {
            _Address = new UnionRef<byte>(_RefType); // span -> auto
            _Address.SetRef(ref data.GetRef());
            _Size = data.Length;
            _Length = Math.Min(length, _Size);
            _Readonly = true;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(string str) : this(new Span<byte>(Encoding.UTF8.GetBytes(str)))
        {
        }
        public CStringRef(string str, Encoding encoding) : this(new Span<byte>((encoding ?? Encoding.Default).GetBytes(str)))
        {
        }
        public void Dispose()
        {
            _Address.Dispose();
        }

        public void Track()
        {
            _Address.Type = UnionRef.RefType.Tracking;
        }
        public void Untrack()
        {
            _Address.Type = UnionRef.RefType.Raw;
        }

        /// <summary>
        /// Growth path shared by all mutators: re-points the owned slot at a fresh
        /// (larger) managed buffer instead of returning a new CStringRef. Mutators
        /// always return this, so a whole mutation chain shares one slot and
        /// Dispose happens exactly once, on the final handle.
        /// </summary>
        private void RebindToNewBuffer(byte[] newbuffer, int newLength)
        {
            _Address.SetRef(ref newbuffer[0]);
            _Size = newbuffer.Length;
            _Length = newLength;
            ClearCache();
        }

        public byte this[int index]
        {
            get
            {
                if (index < 0 || index >= _Size)
                {
                    throw new IndexOutOfRangeException();
                }
                else
                {
                    return UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)index);
                }
            }
            set
            {
                if (index < 0 || index >= _Size)
                {
                    throw new IndexOutOfRangeException();
                }
                else if (_Readonly)
                {
                    throw new InvalidOperationException("Cannot modify a readonly CStringRef");
                }
                else
                {
                    UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)index) = value;
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

        public struct RefIndexer
        {
            private readonly UnionRef<byte> _Address;
            private readonly int _Size;

            public RefIndexer(CStringRef cstr)
            {
                _Address = cstr._Address;
                _Size = cstr._Size;
            }
            public ref byte this[int index]
            {
                get
                {
                    if (index < 0 || index >= _Size)
                    {
                        throw new IndexOutOfRangeException();
                    }
                    return ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)index);
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
                    fixed (byte* ptr = &_Address.GetRef())
                    {
                        _ConvertedString = Encoding.UTF8.GetString(ptr, _Length);
                    }
                }
            }
            return _ConvertedString;
        }

        public struct Enumerator : IEnumerator<byte>
        {
            private readonly UnionRef<byte> _Address;
            private int _Length;
            private int _Index;
            public Enumerator(CStringRef cstr)
            {
                _Address = cstr._Address;
                _Length = cstr._Length;
                _Index = -1;
            }
            public byte Current => UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)_Index);
            object IEnumerator.Current => Current;
            public bool MoveNext()
            {
                _Index++;
                return _Index < _Length;
            }
            public void Reset()
            {
                _Index = -1;
            }
            public void Dispose()
            {
                // No resources to dispose
            }
        }
        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        IEnumerator<byte> IEnumerable<byte>.GetEnumerator()
        {
            return GetEnumerator();
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void CopyTo(ref CStringRef to)
        {
            if (to._Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            UnsafeEx.MemCopy(ref to._Address.GetRef(), 0, ref _Address.GetRef(), 0, Math.Min(_Size, to._Size));
            if (to._Size > _Size)
            {
                UnsafeEx.MemClean(ref to._Address.GetRef(), _Size, to._Size - _Size);
            }
            if (to._Size >= _Length)
            {
                to._Length = _Length;
                to._ConvertedString = _ConvertedString;
                to._CachedHashCode = _CachedHashCode;
            }
            else
            {
                to._Length = to._Size;
                to.ClearCache();
            }
        }
        public void CopyFrom(CStringRef from)
        {
            from.CopyTo(ref this);
        }
        public void CopyTo(Span<byte> to)
        {
            UnsafeEx.MemCopy(ref to.GetRef(), 0, ref _Address.GetRef(), 0, Math.Min(_Size, to.Length));
            if (to.Length > _Size)
            {
                UnsafeEx.MemClean(ref to.GetRef(), _Size, to.Length - _Size);
            }
        }
        public CStringRef Clone()
        {
            var newCStr = new CStringRef(new Span<byte>(new byte[_Length + 1]));
            CopyTo(ref newCStr);
            return newCStr;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }

        public static int MemCmp(ref byte a, int aOffset, ref byte b, int bOffset, int count)
        {
            unsafe
            {
                fixed (byte* pa = &a)
                {
                    fixed (byte* pb = &b)
                    {
                        for (int i = 0; i < count; ++i)
                        {
                            var cmp = pa[i + aOffset] - pb[i + bOffset];
                            if (cmp < 0)
                            {
                                return -1;
                            }
                            else if (cmp > 0)
                            {
                                return 1;
                            }
                        }
                    }
                }
            }
            return 0;
        }

        public int CompareTo(CStringRef other)
        {
            var result = MemCmp(ref _Address.GetRef(), 0, ref other._Address.GetRef(), 0, Math.Min(_Length, other._Length));
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
        public static int Compare(CStringRef a, CStringRef b)
        {
            return a.CompareTo(b);
        }

        int IComparable.CompareTo(object obj)
        {
            if (obj is CStringRef other)
            {
                return CompareTo(other);
            }
            else if (obj is string str)
            {
                return CompareTo(str);
            }
            else
            {
                throw new ArgumentException("Object is not a CStringRef or string", nameof(obj));
            }
        }

        public bool Equals(CStringRef other)
        {
            return CompareTo(other) == 0;
        }
        public bool Equals(string other)
        {
            return ToString().Equals(other);
        }
        public override bool Equals(object obj)
        {
            if (obj is CStringRef other)
            {
                return Equals(other);
            }
            else if (obj is string str)
            {
                return Equals(str);
            }
            else
            {
                return false;
            }
        }
        public override int GetHashCode()
        {
            if (_CachedHashCode == 0)
            {
                int hash = 0;
                if (_Length > 0)
                {
                    var first = _Address.GetValue();
                    hash |= (first << 28);
                    if (_Length > 1)
                    {
                        var last = UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)(_Length - 1));
                        hash |= (last & 0x0F);
                    }
                    for (int i = 0; i < _Length; ++i)
                    {
                        var b = UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)i);
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

        public static bool operator ==(CStringRef left, CStringRef right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(CStringRef left, CStringRef right)
        {
            return !left.Equals(right);
        }

        public static implicit operator string(CStringRef thiz)
        {
            return thiz.ToString();
        }
        public static explicit operator CStringRef(string s)
        {
            return new CStringRef(s);
        }

        public Span<byte> ToSpan()
        {
            return UnsafeEx.AsSpan<byte>(ref _Address.GetRef(), _Size);
        }
        public static implicit operator Span<byte>(CStringRef thiz)
        {
            return thiz.ToSpan();
        }
        public static implicit operator ReadOnlySpan<byte>(CStringRef thiz)
        {
            return thiz.ToSpan();
        }
        public static implicit operator CStringRef(Span<byte> span)
        {
            return new CStringRef(span);
        }
        public static implicit operator CStringRef(ReadOnlySpan<byte> span)
        {
            return new CStringRef(span);
        }

        public static implicit operator CStringSpan(CStringRef thiz)
        {
            return new CStringSpan(ref thiz._Address.GetRef(), thiz._Size, thiz._Length)
            {
                _Readonly = thiz._Readonly,
                _ConvertedString = thiz._ConvertedString,
                _CachedHashCode = thiz._CachedHashCode,
            };
        }
        public static explicit operator CStringRef(CStringSpan thiz)
        {
            return new CStringRef(thiz.ToSpan(), thiz.Length)
            {
                _Readonly = thiz._Readonly,
                _ConvertedString = thiz._ConvertedString,
                _CachedHashCode = thiz._CachedHashCode,
            };
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
            if (startIndex >= _Size)
            {
                var empty = default(CStringSpan);
                empty._Readonly = _Readonly;
                return empty;
            }
            else
            {
                return new CStringSpan(ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)startIndex), _Size - startIndex, _Length - startIndex)
                {
                    _Readonly = _Readonly,
                };
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
            if (startIndex >= _Size)
            {
                var empty = default(CStringSpan);
                empty._Readonly = _Readonly;
                return empty;
            }
            if (startIndex + length == _Length)
            {
                return new CStringSpan(ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)startIndex), _Size - startIndex, length)
                {
                    _Readonly = _Readonly,
                };
            }
            else
            {
                return new CStringSpan(ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)startIndex), length, length)
                {
                    _Readonly = _Readonly,
                };
            }
        }

        public int IndexOf(byte value, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int i = startIndex; i < _Length; ++i)
                    {
                        if (p[i] == value)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int IndexOf(CStringRef value, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (value._Length == 0)
            {
                return startIndex;
            }
            if (value._Length > _Length - startIndex)
            {
                return -1;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                fixed (byte* v = &value._Address.GetRef())
                {
                    int lastStart = _Length - value._Length;
                    for (int i = startIndex; i <= lastStart; ++i)
                    {
                        bool found = true;
                        for (int j = 0; j < value._Length; ++j)
                        {
                            if (p[i + j] != v[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int IndexOf(ReadOnlySpan<byte> value, int startIndex = 0)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (value.Length == 0)
            {
                return startIndex;
            }
            if (value.Length > _Length - startIndex)
            {
                return -1;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    int lastStart = _Length - value.Length;
                    for (int i = startIndex; i <= lastStart; ++i)
                    {
                        bool found = true;
                        for (int j = 0; j < value.Length; ++j)
                        {
                            if (p[i + j] != value[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int IndexOf(Span<byte> value, int startIndex = 0)
        {
            return IndexOf((ReadOnlySpan<byte>)value, startIndex);
        }

        public bool Contains(byte value)
        {
            return IndexOf(value) >= 0;
        }
        public bool Contains(CStringRef value)
        {
            return IndexOf(value) >= 0;
        }
        public bool Contains(ReadOnlySpan<byte> value)
        {
            return IndexOf(value) >= 0;
        }
        public bool Contains(Span<byte> value)
        {
            return IndexOf(value) >= 0;
        }

        public int LastIndexOf(byte value)
        {
            if (_Length == 0)
            {
                return -1;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int i = _Length - 1; i >= 0; --i)
                    {
                        if (p[i] == value)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int LastIndexOf(CStringRef value)
        {
            if (value._Length == 0)
            {
                return _Length;
            }
            if (value._Length > _Length)
            {
                return -1;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                fixed (byte* v = &value._Address.GetRef())
                {
                    for (int i = _Length - value._Length; i >= 0; --i)
                    {
                        bool found = true;
                        for (int j = 0; j < value._Length; ++j)
                        {
                            if (p[i + j] != v[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int LastIndexOf(ReadOnlySpan<byte> value)
        {
            if (value.Length == 0)
            {
                return _Length;
            }
            if (value.Length > _Length)
            {
                return -1;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int i = _Length - value.Length; i >= 0; --i)
                    {
                        bool found = true;
                        for (int j = 0; j < value.Length; ++j)
                        {
                            if (p[i + j] != value[j])
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            return i;
                        }
                    }
                }
            }
            return -1;
        }
        public int LastIndexOf(Span<byte> value)
        {
            return LastIndexOf((ReadOnlySpan<byte>)value);
        }

        public int IndexOfAny(ReadOnlySpan<byte> anyOf)
        {
            return IndexOfAny(anyOf, 0);
        }
        public int IndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex)
        {
            if (startIndex < 0 || startIndex > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (anyOf.Length == 0)
            {
                throw new ArgumentException("anyOf must not be empty.", nameof(anyOf));
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int i = startIndex; i < _Length; ++i)
                    {
                        for (int j = 0; j < anyOf.Length; ++j)
                        {
                            if (p[i] == anyOf[j])
                            {
                                return i;
                            }
                        }
                    }
                }
            }
            return -1;
        }
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf)
        {
            if (anyOf.Length == 0)
            {
                throw new ArgumentException("anyOf must not be empty.", nameof(anyOf));
            }
            if (_Length == 0)
            {
                return -1;
            }
            return LastIndexOfAny(anyOf, _Length - 1);
        }
        public int LastIndexOfAny(ReadOnlySpan<byte> anyOf, int startIndex)
        {
            if (startIndex < 0 || startIndex >= _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (anyOf.Length == 0)
            {
                throw new ArgumentException("anyOf must not be empty.", nameof(anyOf));
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int i = startIndex; i >= 0; --i)
                    {
                        for (int j = 0; j < anyOf.Length; ++j)
                        {
                            if (p[i] == anyOf[j])
                            {
                                return i;
                            }
                        }
                    }
                }
            }
            return -1;
        }

        public bool StartsWith(byte value)
        {
            if (_Length == 0)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    return p[0] == value;
                }
            }
        }
        public bool StartsWith(CStringRef value)
        {
            if (value._Length == 0)
            {
                return true;
            }
            if (value._Length > _Length)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                fixed (byte* v = &value._Address.GetRef())
                {
                    for (int j = 0; j < value._Length; ++j)
                    {
                        if (p[j] != v[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool StartsWith(ReadOnlySpan<byte> value)
        {
            if (value.Length > _Length)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    for (int j = 0; j < value.Length; ++j)
                    {
                        if (p[j] != value[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool StartsWith(Span<byte> value)
        {
            return StartsWith((ReadOnlySpan<byte>)value);
        }

        public bool EndsWith(byte value)
        {
            if (_Length == 0)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    return p[_Length - 1] == value;
                }
            }
        }
        public bool EndsWith(CStringRef value)
        {
            if (value._Length == 0)
            {
                return true;
            }
            if (value._Length > _Length)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                fixed (byte* v = &value._Address.GetRef())
                {
                    int offset = _Length - value._Length;
                    for (int j = 0; j < value._Length; ++j)
                    {
                        if (p[offset + j] != v[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool EndsWith(ReadOnlySpan<byte> value)
        {
            if (value.Length > _Length)
            {
                return false;
            }
            unsafe
            {
                fixed (byte* p = &_Address.GetRef())
                {
                    int offset = _Length - value.Length;
                    for (int j = 0; j < value.Length; ++j)
                    {
                        if (p[offset + j] != value[j])
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        public bool EndsWith(Span<byte> value)
        {
            return EndsWith((ReadOnlySpan<byte>)value);
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
                if (!IsWhiteSpace(this[i]))
                {
                    return false;
                }
            }
            return true;
        }

        public CStringSpanSplitEnumerator Split(ReadOnlySpan<byte> splitters)
        {
            if (_Size == 0)
            {
                // default / Empty source: no valid slot behind it, nothing to enumerate
                return new CStringSpanSplitEnumerator(default(CStringSpan), splitters);
            }
            if (splitters.IsEmpty)
            {
                // An empty splitter set means whitespace mode (legacy behavior)
                return new CStringSpanSplitEnumerator(this, -1);
            }
            return new CStringSpanSplitEnumerator(this, splitters);
        }
        public CStringSpanSplitEnumerator Split(byte splitter)
        {
            if (_Size == 0)
            {
                return new CStringSpanSplitEnumerator(default(CStringSpan), splitter);
            }
            return new CStringSpanSplitEnumerator(this, splitter);
        }
        public CStringSpanSplitEnumerator Split()
        {
            if (_Size == 0)
            {
                return new CStringSpanSplitEnumerator(default(CStringSpan), -1);
            }
            return new CStringSpanSplitEnumerator(this, -1);
        }

        public CStringRef Append(byte value)
        {
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            if (_Length >= _Size)
            {
                var newbuffer = new byte[_Length + 2];
                UnsafeEx.MemCopy(ref newbuffer[0], 0, ref _Address.GetRef(), 0, _Length);
                newbuffer[_Length] = value;
                RebindToNewBuffer(newbuffer, _Length + 1);
                return this;
            }
            ClearCache();
            ToSpan().Append(value);
            ++_Length;
            return this;
        }
        public CStringRef Append(CStringRef other)
        {
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            if (other._Length == 0)
            {
                return this;
            }
            if (_Length + other._Length > _Size)
            {
                var newbuffer = new byte[_Length + other._Length + 1];
                UnsafeEx.MemCopy(ref newbuffer[0], 0, ref _Address.GetRef(), 0, _Length);
                UnsafeEx.MemCopy(ref newbuffer[0], _Length, ref other._Address.GetRef(), 0, other._Length);
                RebindToNewBuffer(newbuffer, _Length + other._Length);
                return this;
            }
            ClearCache();
            ToSpan().Append(other.ToSpan());
            _Length += other._Length;
            return this;
        }
        public static CStringRef operator +(CStringRef a, CStringRef b)
        {
            return a.Append(b);
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
        public CStringRef Trim(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int start = 0;
            while (start < _Length && ShouldTrim(this[start], chars))
            {
                ++start;
            }
            int end = _Length;
            while (end > start && ShouldTrim(this[end - 1], chars))
            {
                --end;
            }
            if (start == 0 && end == _Length)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            int newLength = end - start;
            if (newLength > 0)
            {
                UnsafeEx.MemCopy(ref _Address.GetRef(), 0, ref _Address.GetRef(), start, newLength);
            }
            Length = newLength;
            return this;
        }
        public CStringRef TrimEnd(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int end = _Length;
            while (end > 0 && ShouldTrim(this[end - 1], chars))
            {
                --end;
            }
            if (end == _Length)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            Length = end;
            return this;
        }
        public CStringRef TrimStart(ReadOnlySpan<byte> chars = default)
        {
            if (_Length == 0)
            {
                return this;
            }
            int start = 0;
            while (start < _Length && ShouldTrim(this[start], chars))
            {
                ++start;
            }
            if (start == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            int newLength = _Length - start;
            UnsafeEx.MemCopy(ref _Address.GetRef(), 0, ref _Address.GetRef(), start, newLength);
            Length = newLength;
            return this;
        }

        public CStringRef Clear()
        {
            if (_Length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            Length = 0;
            return this;
        }
        public CStringRef Remove(int startIndex, int length)
        {
            if (length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
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
            UnsafeEx.MemCopy(ref _Address.GetRef(), startIndex, ref _Address.GetRef(), startIndex + length, newLength - startIndex);
            Length = newLength;
            return this;
        }

        public CStringRef Insert(int index, byte ch)
        {
            return Insert(index, ch, 1);
        }
        public CStringRef Insert(int index, byte ch, int repeat)
        {
            if (repeat == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
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
            if (newLength > _Size)
            {
                var buffer = new byte[newLength + 1];
                UnsafeEx.MemCopy(ref buffer[0], 0, ref _Address.GetRef(), 0, index);
                UnsafeEx.MemSet(ref buffer[0], index, ch, repeat);
                UnsafeEx.MemCopy(ref buffer[0], index + repeat, ref _Address.GetRef(), index, _Length - index);
                RebindToNewBuffer(buffer, newLength);
                return this;
            }
            UnsafeEx.MemCopy(ref _Address.GetRef(), index + repeat, ref _Address.GetRef(), index, _Length - index);
            UnsafeEx.MemSet(ref _Address.GetRef(), index, ch, repeat);
            if (newLength < _Size)
            {
                UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)newLength) = 0;
            }
            _Length = newLength;
            ClearCache();
            return this;
        }
        public CStringRef Insert(int index, CStringRef sub)
        {
            return Insert(index, sub, 1);
        }
        public CStringRef Insert(int index, CStringRef sub, int repeat)
        {
            if (repeat == 0 || sub._Length == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            if (index < 0 || index > _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            if (repeat < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }
            long addLength64 = (long)sub._Length * repeat;
            if (addLength64 > int.MaxValue - _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(repeat));
            }
            int addLength = (int)addLength64;
            int newLength = _Length + addLength;
            if (newLength > _Size)
            {
                var buffer = new byte[newLength + 1];
                UnsafeEx.MemCopy(ref buffer[0], 0, ref _Address.GetRef(), 0, index);
                for (int i = 0; i < repeat; ++i)
                {
                    UnsafeEx.MemCopy(ref buffer[0], index + i * sub._Length, ref sub._Address.GetRef(), 0, sub._Length);
                }
                UnsafeEx.MemCopy(ref buffer[0], index + addLength, ref _Address.GetRef(), index, _Length - index);
                RebindToNewBuffer(buffer, newLength);
                return this;
            }
            // In-place insertion: snapshot sub first so it survives the shift when sub shares the same underlying buffer as this view
            Span<byte> snapshot = stackalloc byte[sub._Length];
            UnsafeEx.MemCopy(ref snapshot[0], 0, ref sub._Address.GetRef(), 0, sub._Length);
            UnsafeEx.MemCopy(ref _Address.GetRef(), index + addLength, ref _Address.GetRef(), index, _Length - index);
            for (int i = 0; i < repeat; ++i)
            {
                UnsafeEx.MemCopy(ref _Address.GetRef(), index + i * sub._Length, ref snapshot[0], 0, sub._Length);
            }
            if (newLength < _Size)
            {
                UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)newLength) = 0;
            }
            _Length = newLength;
            ClearCache();
            return this;
        }
        public CStringRef Replace(byte oldch, byte newch)
        {
            return Replace(oldch, newch, 0, _Length);
        }
        public CStringRef Replace(byte oldch, byte newch, int startIndex, int count)
        {
            if (count == 0)
            {
                return this;
            }
            if (startIndex < 0 || startIndex >= _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (count < 0 || count > _Length - startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            if (oldch == newch)
            {
                return this;
            }
            int windowEnd = startIndex + count;
            int first = -1;
            for (int i = startIndex; i < windowEnd; ++i)
            {
                if (UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)i) == oldch)
                {
                    first = i;
                    break;
                }
            }
            if (first < 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }
            // Pure byte replacement; _Length is unchanged (same when newch is 0: a NUL is merely written into the content, no truncation)
            for (int i = first; i < windowEnd; ++i)
            {
                ref var address = ref UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)i);
                if (address == oldch)
                {
                    address = newch;
                }
            }
            ClearCache();
            return this;
        }
        public CStringRef Replace(CStringRef oldstr, CStringRef newstr)
        {
            return ReplaceCore(oldstr, newstr, 0, _Length);
        }
        public CStringRef Replace(CStringRef oldstr, CStringRef newstr, int startIndex, int count)
        {
            if (count == 0)
            {
                return this;
            }
            if (startIndex < 0 || startIndex >= _Length)
            {
                throw new ArgumentOutOfRangeException(nameof(startIndex));
            }
            if (count < 0 || count > _Length - startIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }
            return ReplaceCore(oldstr, newstr, startIndex, startIndex + count);
        }

        // Replaces oldstr with newstr left-to-right and non-overlapping within [windowStart, windowEnd); content outside the window keeps its relative position and shifts naturally as the length changes
        private CStringRef ReplaceCore(CStringRef oldstr, CStringRef newstr, int windowStart, int windowEnd)
        {
            if (oldstr._Length == 0)
            {
                throw new ArgumentException("String cannot be of zero length", nameof(oldstr));
            }
            int oldLength = oldstr._Length;
            int newLength = newstr._Length;
            int maxStart = windowEnd - oldLength;

            // First pass: count matches, keeping the first/last positions so that tiny cases need no second pass
            int first = -1;
            int last = -1;
            int matchCount = 0;
            int scan = windowStart;
            while (scan <= maxStart)
            {
                if (MemCmp(ref _Address.GetRef(), scan, ref oldstr._Address.GetRef(), 0, oldLength) == 0)
                {
                    if (matchCount == 0)
                    {
                        first = scan;
                    }
                    last = scan;
                    ++matchCount;
                    scan += oldLength;
                }
                else
                {
                    ++scan;
                }
            }
            if (matchCount == 0)
            {
                return this;
            }
            if (_Readonly)
            {
                throw new InvalidOperationException("Cannot modify a readonly CStringRef");
            }

            Span<byte> snapshot = stackalloc byte[newLength];
            if (newLength > 0)
            {
                UnsafeEx.MemCopy(ref snapshot[0], 0, ref newstr._Address.GetRef(), 0, newLength);
            }
            int diff = newLength - oldLength;
            long newTotal64 = (long)_Length + (long)diff * matchCount;
            if (newTotal64 < 0 || newTotal64 > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(nameof(newstr));
            }
            int newTotal = (int)newTotal64;

            Span<int> positions = matchCount <= 64 ? stackalloc int[matchCount] : new int[matchCount];
            if (matchCount > 2)
            {
                int k = 0;
                scan = windowStart;
                while (scan <= maxStart)
                {
                    if (MemCmp(ref _Address.GetRef(), scan, ref oldstr._Address.GetRef(), 0, oldLength) == 0)
                    {
                        positions[k++] = scan;
                        scan += oldLength;
                    }
                    else
                    {
                        ++scan;
                    }
                }
            }
            else
            {
                positions[0] = first;
                if (matchCount == 2)
                {
                    positions[1] = last;
                }
            }

            if (newTotal > _Size)
            {
                var buffer = new byte[newTotal + 1];
                int source = 0;
                int target = 0;
                for (int i = 0; i < matchCount; ++i)
                {
                    int pos = positions[i];
                    if (source < pos)
                    {
                        int segmentLength = pos - source;
                        UnsafeEx.MemCopy(ref buffer[0], target, ref _Address.GetRef(), source, segmentLength);
                        source += segmentLength;
                        target += segmentLength;
                    }
                    if (newLength > 0)
                    {
                        UnsafeEx.MemCopy(ref buffer[0], target, ref snapshot[0], 0, newLength);
                        target += newLength;
                    }
                    source += oldLength;
                }
                if (source < _Length)
                {
                    UnsafeEx.MemCopy(ref buffer[0], target, ref _Address.GetRef(), source, _Length - source);
                }
                RebindToNewBuffer(buffer, newTotal);
                return this;
            }
            else
            {
                if (diff == 0)
                {
                    for (int i = 0; i < matchCount; ++i)
                    {
                        UnsafeEx.MemCopy(ref _Address.GetRef(), positions[i], ref snapshot[0], 0, oldLength);
                    }
                    ClearCache();
                    return this;
                }
                else if (diff > 0)
                {
                    int sourceRight = _Length;
                    int targetRight = newTotal;
                    for (int i = matchCount - 1; i >= 0; --i)
                    {
                        int start = positions[i];
                        int segmentEnd = start + oldLength;
                        int segmentLength = sourceRight - segmentEnd;
                        if (segmentLength > 0)
                        {
                            int targetSegmentStart = targetRight - segmentLength;
                            UnsafeEx.MemCopy(ref _Address.GetRef(), targetSegmentStart, ref _Address.GetRef(), segmentEnd, segmentLength);
                            targetRight = targetSegmentStart;
                        }
                        if (newLength > 0)
                        {
                            int targetNewStart = targetRight - newLength;
                            UnsafeEx.MemCopy(ref _Address.GetRef(), targetNewStart, ref snapshot[0], 0, newLength);
                            targetRight = targetNewStart;
                        }
                        sourceRight = start;
                    }
                    if (newTotal < _Size)
                    {
                        UnsafeEx.AddByteOffset(ref _Address.GetRef(), (IntPtr)newTotal) = 0;
                    }
                    _Length = newTotal;
                    ClearCache();
                    return this;
                }
                else //if (diff < 0)
                {
                    int write = 0;
                    int read = 0;
                    for (int i = 0; i < matchCount; ++i)
                    {
                        int pos = positions[i];
                        if (pos > read)
                        {
                            UnsafeEx.MemCopy(ref _Address.GetRef(), write, ref _Address.GetRef(), read, pos - read);
                            write += pos - read;
                        }
                        read = pos;
                        if (newLength > 0)
                        {
                            UnsafeEx.MemCopy(ref _Address.GetRef(), write, ref snapshot[0], 0, newLength);
                            write += newLength;
                        }
                        read += oldLength;
                    }
                    if (_Length > read)
                    {
                        UnsafeEx.MemCopy(ref _Address.GetRef(), write, ref _Address.GetRef(), read, _Length - read);
                        write += _Length - read;
                    }
                    Length = write;
                    return this;
                }
            }
        }
    }

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

    public class CString : DisposableBox<CStringRef>
    {
        public CString() : base()
        {
        }
        public CString(CStringRef value) : base(value)
        {
        }
        public CString(string value) : base(new CStringRef(value))
        {
        }
        public static implicit operator string(CString thiz)
        {
            return thiz.Value.ToString();
        }
        public static implicit operator CString(string s)
        {
            return new CString(s);
        }
        public static implicit operator CStringRef(CString thiz)
        {
            return thiz.Value;
        }
        public static implicit operator CString(CStringRef cstr)
        {
            return new CString(cstr);
        }
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
