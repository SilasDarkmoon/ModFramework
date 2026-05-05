using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Mod
{
    //public struct CStringRef : IEnumerable<char>, IEnumerable, IComparable, IComparable<CStringRef>, IEquatable<CStringRef>, IComparable<CString>, IEquatable<CString>
    //{
    //    private readonly IntPtr _Address;
    //    private readonly int _Length;
    //    private readonly string _ConvertedString;

    //    public static readonly CStringRef Empty = default;

    //    public CStringRef(IntPtr address, int len)
    //    {
    //        _Address = address;
    //        _Length = len;
    //        _ConvertedString = null;
    //    }
    //    public CStringRef(IntPtr address)
    //    {
    //        _Address = address;
    //        _Length = Unsafe.StrLen(address);
    //        _ConvertedString = null;
    //    }
    //    public CStringRef(Span<byte> data)
    //    {
    //        (_Address, _Length) = data.GetAddressAndLength();
    //        _ConvertedString = null;
    //    }
    //    public CStringRef(ReadOnlySpan<byte> data)
    //    {
    //        (_Address, _Length) = data.GetAddressAndLength();
    //        _ConvertedString = null;
    //    }

    //    public byte this[int index]
    //    {
    //        get
    //        {
    //            if (index < 0 || index >= _data.Length)
    //            {
    //                throw new IndexOutOfRangeException();
    //            }
    //            return _data[index];
    //        }
    //    }
    //    public override string ToString()
    //    {
    //        return Encoding.UTF8.GetString(_data);
    //    }
    //}

    //public sealed class CString : IEquatable<CString>, IComparable<CString>
    //{
    //    internal CStringRef _Internal;

    //    private readonly byte[] _data;
    //    private readonly int _length;

    //    public int Length => _length;

    //    public CString()
    //    {
    //        _data = Array.Empty<byte>();
    //        _length = 0;
    //    }

    //    public CString(byte[] data)
    //    {
    //        if (data == null)
    //        {
    //            _data = Array.Empty<byte>();
    //            _length = 0;
    //        }
    //        else
    //        {
    //            _data = new byte[data.Length];
    //            Array.Copy(data, _data, data.Length);
    //            _length = data.Length;
    //        }
    //    }

    //    public CString(string str)
    //    {
    //        if (str == null)
    //        {
    //            _data = Array.Empty<byte>();
    //            _length = 0;
    //        }
    //        else
    //        {
    //            _data = Encoding.UTF8.GetBytes(str);
    //            _length = _data.Length;
    //        }
    //    }

    //    public CString(byte[] data, int startIndex, int length)
    //    {
    //        if (data == null)
    //        {
    //            throw new ArgumentNullException(nameof(data));
    //        }

    //        if (startIndex < 0 || startIndex >= data.Length)
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(startIndex));
    //        }

    //        if (length < 0 || startIndex + length > data.Length)
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(length));
    //        }

    //        _data = new byte[length];
    //        Array.Copy(data, startIndex, _data, 0, length);
    //        _length = length;
    //    }

    //    public byte this[int index]
    //    {
    //        get
    //        {
    //            if (index < 0 || index >= _length)
    //            {
    //                throw new IndexOutOfRangeException();
    //            }
    //            return _data[index];
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        return Encoding.UTF8.GetString(_data);
    //    }

    //    public static implicit operator CString(string str)
    //    {
    //        return new CString(str);
    //    }

    //    public static implicit operator string(CString cstr)
    //    {
    //        return cstr?.ToString() ?? null;
    //    }

    //    public static CString operator +(CString left, CString right)
    //    {
    //        if (left == null)
    //        {
    //            return right ?? new CString();
    //        }

    //        if (right == null)
    //        {
    //            return left;
    //        }

    //        byte[] newData = new byte[left._length + right._length];
    //        Array.Copy(left._data, newData, left._length);
    //        Array.Copy(right._data, 0, newData, left._length, right._length);

    //        return new CString(newData);
    //    }

    //    public static CString operator +(CString left, string right)
    //    {
    //        return left + new CString(right);
    //    }

    //    public static CString operator +(string left, CString right)
    //    {
    //        return new CString(left) + right;
    //    }

    //    public static bool operator ==(CString left, CString right)
    //    {
    //        if (ReferenceEquals(left, right))
    //        {
    //            return true;
    //        }

    //        if (left is null || right is null)
    //        {
    //            return false;
    //        }

    //        if (left._length != right._length)
    //        {
    //            return false;
    //        }

    //        for (int i = 0; i < left._length; i++)
    //        {
    //            if (left._data[i] != right._data[i])
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }

    //    public static bool operator !=(CString left, CString right)
    //    {
    //        return !(left == right);
    //    }

    //    public override bool Equals(object obj)
    //    {
    //        return Equals(obj as CString);
    //    }

    //    public bool Equals(CString other)
    //    {
    //        return this == other;
    //    }

    //    public override int GetHashCode()
    //    {
    //        int hash = 17;
    //        for (int i = 0; i < _length; i++)
    //        {
    //            hash = hash * 31 + _data[i];
    //        }
    //        return hash;
    //    }

    //    public int CompareTo(CString other)
    //    {
    //        if (other == null)
    //        {
    //            return 1;
    //        }

    //        int minLength = Math.Min(_length, other._length);
    //        for (int i = 0; i < minLength; i++)
    //        {
    //            int comparison = _data[i].CompareTo(other._data[i]);
    //            if (comparison != 0)
    //            {
    //                return comparison;
    //            }
    //        }

    //        return _length.CompareTo(other._length);
    //    }

    //    public CString Substring(int startIndex)
    //    {
    //        if (startIndex < 0 || startIndex >= _length)
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(startIndex));
    //        }

    //        return new CString(_data, startIndex, _length - startIndex);
    //    }

    //    public CString Substring(int startIndex, int length)
    //    {
    //        if (startIndex < 0 || startIndex >= _length)
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(startIndex));
    //        }

    //        if (length < 0 || startIndex + length > _length)
    //        {
    //            throw new ArgumentOutOfRangeException(nameof(length));
    //        }

    //        return new CString(_data, startIndex, length);
    //    }

    //    public int IndexOf(byte value)
    //    {
    //        for (int i = 0; i < _length; i++)
    //        {
    //            if (_data[i] == value)
    //            {
    //                return i;
    //            }
    //        }
    //        return -1;
    //    }

    //    public int IndexOf(CString value)
    //    {
    //        if (value == null || value._length == 0 || value._length > _length)
    //        {
    //            return -1;
    //        }

    //        for (int i = 0; i <= _length - value._length; i++)
    //        {
    //            bool match = true;
    //            for (int j = 0; j < value._length; j++)
    //            {
    //                if (_data[i + j] != value._data[j])
    //                {
    //                    match = false;
    //                    break;
    //                }
    //            }
    //            if (match)
    //            {
    //                return i;
    //            }
    //        }
    //        return -1;
    //    }

    //    public int IndexOf(string value)
    //    {
    //        return IndexOf(new CString(value));
    //    }

    //    public bool StartsWith(CString value)
    //    {
    //        if (value == null || value._length == 0)
    //        {
    //            return true;
    //        }

    //        if (value._length > _length)
    //        {
    //            return false;
    //        }

    //        for (int i = 0; i < value._length; i++)
    //        {
    //            if (_data[i] != value._data[i])
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }

    //    public bool StartsWith(string value)
    //    {
    //        return StartsWith(new CString(value));
    //    }

    //    public bool EndsWith(CString value)
    //    {
    //        if (value == null || value._length == 0)
    //        {
    //            return true;
    //        }

    //        if (value._length > _length)
    //        {
    //            return false;
    //        }

    //        for (int i = 0; i < value._length; i++)
    //        {
    //            if (_data[_length - value._length + i] != value._data[i])
    //            {
    //                return false;
    //            }
    //        }

    //        return true;
    //    }

    //    public bool EndsWith(string value)
    //    {
    //        return EndsWith(new CString(value));
    //    }

    //    public CString ToLower()
    //    {
    //        byte[] lowerData = new byte[_length];
    //        for (int i = 0; i < _length; i++)
    //        {
    //            if (_data[i] >= 65 && _data[i] <= 90) // ASCII A-Z
    //            {
    //                lowerData[i] = (byte)(_data[i] + 32);
    //            }
    //            else
    //            {
    //                lowerData[i] = _data[i];
    //            }
    //        }
    //        return new CString(lowerData);
    //    }

    //    public CString ToUpper()
    //    {
    //        byte[] upperData = new byte[_length];
    //        for (int i = 0; i < _length; i++)
    //        {
    //            if (_data[i] >= 97 && _data[i] <= 122) // ASCII a-z
    //            {
    //                upperData[i] = (byte)(_data[i] - 32);
    //            }
    //            else
    //            {
    //                upperData[i] = _data[i];
    //            }
    //        }
    //        return new CString(upperData);
    //    }

    //    public static CString Empty => new CString();
    //}
}
