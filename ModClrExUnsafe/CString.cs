using Mod.LowLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    public struct CStringRef : IEnumerable<byte>, IEnumerable, ICloneable, IComparable, IComparable<CStringRef>, IComparable<string>, IEquatable<CStringRef>, IEquatable<string>
    {
        private readonly RefContainer<byte> _Address;
        private readonly int _Size;
        private int _Length;
        private bool _Readonly;
        private string _ConvertedString;
        private int _CachedHashCode;

        public int Length => _Length;
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

        public CStringRef(IntPtr address, int size, int len)
        {
            _Address = new RefContainer<byte>(address);
            _Size = size;
            _Length = len;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(IntPtr address, int size)
        {
            _Address = new RefContainer<byte>(address);
            _Size = size;
            _Length = UnsafeEx.StrLen(address, size);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(IntPtr address)
        {
            _Address = new RefContainer<byte>(address);
            _Length = UnsafeEx.StrLen(address);
            _Size = _Length + 1;
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(Span<byte> data)
        {
            _Address = new RefContainer<byte>(ref data[0]);
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = false;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(ReadOnlySpan<byte> data)
        {
            _Address = new RefContainer<byte>(ref LowLevel.Ref.Unprotect(in data[0]));
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = true;
            _ConvertedString = null;
            _CachedHashCode = 0;
        }
        public CStringRef(string str) : this(Encoding.UTF8.GetBytes(str))
        {
        }
        public CStringRef(string str, Encoding encoding) : this((encoding ?? Encoding.Default).GetBytes(str))
        {
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
                    return (_Address + index).GetValue();
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
                    (_Address + index).SetValue(value);
                    if (value == 0 && index < _Length)
                    {
                        _Length = index;
                        ClearCache();
                    }
                    else if (value != 0 && index >= _Length)
                    {
                        _Length = index + 1;
                        ClearCache();
                    }
                }
            }
        }

        public struct RefIndexer
        {
            private readonly RefContainer<byte> _Address;
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
                    return ref (_Address + index).GetRef();
                }
            }
        }
        public RefIndexer Ref => new RefIndexer(this);

        override public string ToString()
        {
            if (_ConvertedString == null)
            {
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
            private readonly RefContainer<byte> _Address;
            private int _Length;
            private int _Index;
            public Enumerator(CStringRef cstr)
            {
                _Address = cstr._Address;
                _Length = cstr._Length;
                _Index = -1;
            }
            public byte Current => (_Address + _Index).GetValue();
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

        public void CopyTo(CStringRef to)
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
        public void CopyTo(Span<byte> to)
        {
            UnsafeEx.MemCopy(ref to[0], 0, ref _Address.GetRef(), 0, Math.Min(_Size, to.Length));
            if (to.Length > _Size)
            {
                UnsafeEx.MemClean(ref to[0], _Size, to.Length - _Size);
            }
        }
        public CStringRef Clone()
        {
            var newCStr = new CStringRef(new byte[_Length + 1]);
            CopyTo(newCStr);
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
                        var last = (_Address + (_Length - 1)).GetValue();
                        hash |= (last & 0x0F);
                    }
                    for (int i = 0; i < _Length; ++i)
                    {
                        var b = (_Address + i).GetValue();
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

        public CStringRef Append(byte value)
        {
            if (_Length >= _Size)
            {
                var newbuffer = new byte[_Length + 2];
                ToSpan().CopyTo(newbuffer);
                newbuffer[_Length] = value;
                return new CStringRef(newbuffer);
            }
            ClearCache();
            ToSpan().Append(value);
            ++_Length;
            return this;
        }
        public CStringRef Append(CStringRef other)
        {
            if (other._Length == 0)
            {
                return this;
            }
            if (_Length + other._Length > _Size)
            {
                var newbuffer = new byte[_Length + other._Length + 1];
                ToSpan().CopyTo(newbuffer);
                other.ToSpan().CopyTo(new Span<byte>(newbuffer, _Length, newbuffer.Length - _Length));
                return new CStringRef(newbuffer);
            }
            ClearCache();
            ToSpan().Append(other.ToSpan());
            _Length += other._Length;
            return this;
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
    public class CString : Box<CStringRef>
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
