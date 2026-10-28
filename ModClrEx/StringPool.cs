using Mod.LowLevel;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Mod
{
    public interface IStringPool
    {
        int Push(string str);
        string Get(int index);
        int Count { get; }
        int IsPooled(string str);
    }

    /// <remarks>Use this only when there are a lot of "IsPooled" checks concurrently.</remarks>
    public class StringPoolConcurrent : IStringPool
    {
        private readonly ConcurrentDictionary<string, object> _Locks = new ConcurrentDictionary<string, object>();
        private readonly ConcurrentDictionary<string, int> _String2Slot = new ConcurrentDictionary<string, int>();
        private readonly List<string> _StringSlots = new List<string>();

        public int Push(string str)
        {
            if (str == null)
            {
                return -1;
            }

            str = string.Intern(str);
            int RecordSlot(string key)
            {
                object mylock = null;
                var lockObj = _Locks.GetOrAdd(key, _ => mylock = new object());
                if (lockObj == mylock)
                { // this call should really do the record.
                    lock (_StringSlots)
                    {
                        var slot = _StringSlots.Count;
                        _StringSlots.Add(key);
                        //_String2Slot.TryAdd(key, slot); // No need. after this call, the ConcurrentDictionary will do this set.
                        return slot;
                    }
                }
                else
                { // just wait for the other thread to do the record.
                    int slot = -1;
                    SpinWait.SpinUntil(() => _String2Slot.TryGetValue(key, out slot));
                    return slot;
                }
            }

            return _String2Slot.GetOrAdd(str, RecordSlot);
        }

        public string Get(int index)
        {
            lock (_StringSlots)
            {
                if (index >= 0 && index < _StringSlots.Count)
                {
                    return _StringSlots[index];
                }
            }
            return null;
        }
        public int Count
        {
            get
            {
                return _String2Slot.Count;
            }
        }
        public int IsPooled(string str)
        {
            if (str == null)
            {
                return -1;
            }
            if (_String2Slot.TryGetValue(str, out var slot))
            {
                return slot;
            }
            return -1;
        }
    }
    public class StringPoolLocked : IStringPool
    {
        private readonly Dictionary<string, int> _String2Slot = new Dictionary<string, int>();
        private readonly List<string> _StringSlots = new List<string>();

        public int Push(string str)
        {
            if (str == null)
            {
                return -1;
            }

            str = string.Intern(str);
            lock (_StringSlots)
            {
                if (_String2Slot.TryGetValue(str, out var slot))
                {
                    return slot;
                }
                else
                {
                    slot = _StringSlots.Count;
                    _StringSlots.Add(str);
                    _String2Slot.Add(str, slot);
                    return slot;
                }
            }
        }

        public string Get(int index)
        {
            lock (_StringSlots)
            {
                if (index >= 0 && index < _StringSlots.Count)
                {
                    return _StringSlots[index];
                }
            }
            return null;
        }
        public int Count
        {
            get
            {
                lock (_StringSlots)
                {
                    return _StringSlots.Count;
                }
            }
        }
        public int IsPooled(string str)
        {
            if (str == null)
            {
                return -1;
            }
            lock (_StringSlots)
            {
                if (_String2Slot.TryGetValue(str, out var slot))
                {
                    return slot;
                }
                return -1;
            }
        }
    }

    public sealed class PString
    {
        private readonly string _Value;
        private readonly int _Index;
        private readonly int _HashCode;
        private static readonly IStringPool _StringPool = new StringPoolLocked();

        public PString(string str)
        {
            if (str != null)
            {
                _Value = string.Intern(str);
                _Index = _StringPool.Push(_Value);
                _HashCode = _Value.GetHashCode();
            }
        }

        public string Value => _Value;
        public int Index => _Index;

        public override string ToString() => _Value;
        public override int GetHashCode()
        {
            return _HashCode;
        }
        public override bool Equals(object obj)
        {
            if (obj is PString other)
            {
                return _Index == other._Index;
            }
            else if (obj is string otherstr)
            {
                return _Value == otherstr;
            }
            return false;
        }
        public static implicit operator string(PString pooled) => pooled._Value;
        public static implicit operator PString(string str) => new PString(str);
    }

    public sealed class PCString
    {
        private readonly static List<PCString> _Pool = new List<PCString>();
        private static void Put(PCString pcstr)
        {
            var index = pcstr.Index;
            if (index >= 0)
            {
                lock (_Pool)
                {
                    while (_Pool.Count <= index)
                    {
                        _Pool.Add(null);
                    }
                    _Pool[index] = pcstr;
                }
            }
        }
        private static PCString Get(int index)
        {
            if (index >= 0)
            {
                lock (_Pool)
                {
                    if (_Pool.Count > index)
                    {
                        return _Pool[index];
                    }
                }
            }
            return null;
        }

        public static PCString GetOrCreate(string str)
        {
            if (str == null)
            {
                return null;
            }
            PString pstr = new PString(str);
            var index = pstr.Index;
            return Get(index) ?? new PCString(str);
        }
        public static PCString GetOrCreate(PString pstr)
        {
            if (pstr == null)
            {
                return null;
            }
            var index = pstr.Index;
            return Get(index) ?? new PCString(pstr);
        }

        private PString _Str;
        private CString _CStr;
        private WeakAttachment.Key _Key;

        private PCString(string str)
        {
            if (str != null)
            {
                _Str = new PString(str);
                _Key = new WeakAttachment.Key(str);
                _CStr = new CString(new CStringRef(true).Append(str));
                Put(this);
            }
        }
        private PCString(PString pstr)
        {
            if (pstr != null)
            {
                _Str = pstr;
                _Key = new WeakAttachment.Key(pstr.Value);
                _CStr = new CString(new CStringRef(true).Append(pstr.Value));
                Put(this);
            }
        }

        public PString PString => _Str;
        public string String => _Str?.Value;
        public int Index => _Str?.Index ?? -1;
        public CString CString => _CStr;
        public WeakAttachment.Key Key => _Key;

        public override string ToString() => String;
        public override int GetHashCode()
        {
            return _Str?.GetHashCode() ?? 0;
        }
        public override bool Equals(object obj)
        {
            if (obj is PCString other)
            {
                return _Str == other._Str;
            }
            else if (obj is PString otherpstr)
            {
                return _Str == otherpstr;
            }
            else if (obj is string otherstr)
            {
                return String == otherstr;
            }
            return false;
        }
        public static implicit operator string(PCString pooled) => pooled.String;
        public static implicit operator PCString(string str) => GetOrCreate(str);
        public static implicit operator PString(PCString pooled) => pooled.PString;
        public static implicit operator PCString(PString pstr) => GetOrCreate(pstr);
    }
}
