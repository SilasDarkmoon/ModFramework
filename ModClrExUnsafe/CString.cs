using Mod.LowLevel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
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
        public void RecalculateLength()
        {
            var newlen = UnsafeEx.StrLen(ref _Address.GetRef(), _Size);
            if (_Length != newlen)
            {
                _ConvertedString = null;
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
        }
        public CStringRef(IntPtr address, int size)
        {
            _Address = new RefContainer<byte>(address);
            _Size = size;
            _Length = UnsafeEx.StrLen(address, size);
            _Readonly = false;
            _ConvertedString = null;
        }
        public CStringRef(IntPtr address)
        {
            _Address = new RefContainer<byte>(address);
            _Length = UnsafeEx.StrLen(address);
            _Size = _Length + 1;
            _Readonly = false;
            _ConvertedString = null;
        }
        public CStringRef(Span<byte> data)
        {
            _Address = new RefContainer<byte>(ref data[0]);
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = false;
            _ConvertedString = null;
        }
        public CStringRef(ReadOnlySpan<byte> data)
        {
            _Address = new RefContainer<byte>(ref LowLevel.Ref.Unprotect(in data[0]));
            _Size = data.Length;
            _Length = UnsafeEx.StrLen(data);
            _Readonly = true;
            _ConvertedString = null;
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
                        _ConvertedString = null;
                    }
                    else if (value != 0 && index >= _Length)
                    {
                        _Length = index + 1;
                        _ConvertedString = null;
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

        public void CloneTo(CStringRef to)
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
            }
            else
            {
                to._Length = to._Size;
                to._ConvertedString = null;
            }
        }

        public CStringRef Clone()
        {
            var newCStr = new CStringRef(new byte[_Length + 1]);
            CloneTo(newCStr);
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
            return hash;
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

        public CStringRef Append(byte value)
        {
            if (_Length >= _Size)
            {
                var newbuffer = new byte[_Length + 2];
                ToSpan().CopyTo(newbuffer);
                newbuffer[_Length] = value;
                return new CStringRef(newbuffer);
            }
            return new CStringRef(ToSpan().Append(value));
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
            var newspan = ToSpan().Append(other.ToSpan());
            return new CStringRef(newspan);
        }
    }
}
