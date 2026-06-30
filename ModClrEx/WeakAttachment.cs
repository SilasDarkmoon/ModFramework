using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Mod
{
    /// <remarks>Cache or reuse attachment whenever possible. Donot "GetAttachment" every time. WeakAttachments using Key is 1.6 times slower than raw fields, and using string is 7 times slower than raw fields.</remarks>
    public static class WeakAttachment
    {
        private static readonly IStringPool _KnownKeys = new StringPoolLocked();
        public class Key
        {
            private readonly string _Value;
            private readonly int _Index;
            private readonly int _HashCode;

            public Key(string str)
            {
                if (str != null)
                {
                    _Value = string.Intern(str);
                    _Index = _KnownKeys.Push(_Value);
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
                if (obj is Key other)
                {
                    return _Index == other._Index;
                }
                else if (obj is string otherstr)
                {
                    return _Value == otherstr;
                }
                return false;
            }
            public static implicit operator string(Key pooled) => pooled._Value;
            public static implicit operator Key(string str) => new Key(str);
        }

        public static Key WeakAttachmentKey(this string str)
        {
            return new Key(str);
        }

        private class WeakAttachmentStorage
        {
            private readonly object[] _KnownAttachments;
            private readonly Dictionary<string, object> _DynamicAttachments = new Dictionary<string, object>();

            public WeakAttachmentStorage()
            {
                _KnownAttachments = new object[_KnownKeys.Count];
            }
            public void Set(Key key, object value)
            {
                if (key.Index is var index && index >= 0 && index < _KnownAttachments.Length)
                {
                    _KnownAttachments[index] = value;
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        if (value == null)
                        {
                            _DynamicAttachments.Remove(key.Value);
                        }
                        else
                        {
                            _DynamicAttachments[key.Value] = value;
                        }
                    }
                }
            }
            public void Set(string key, object value)
            {
                var index = _KnownKeys.IsPooled(key);
                if (index >= 0 && index < _KnownAttachments.Length)
                {
                    _KnownAttachments[index] = value;
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        if (value == null)
                        {
                            _DynamicAttachments.Remove(key);
                        }
                        else
                        {
                            _DynamicAttachments[key] = value;
                        }
                    }
                }
            }
            public object Get(Key key)
            {
                if (key.Index is var index && index >= 0 && index < _KnownAttachments.Length)
                {
                    return _KnownAttachments[index];
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        _DynamicAttachments.TryGetValue(key.Value, out var value);
                        return value;
                    }
                }
            }
            public object Get(string key)
            {
                var index = _KnownKeys.IsPooled(key);
                if (index >= 0 && index < _KnownAttachments.Length)
                {
                    return _KnownAttachments[index];
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        _DynamicAttachments.TryGetValue(key, out var value);
                        return value;
                    }
                }
            }
            public object GetOrCreate(Key key, Func<object> valueFactory)
            {
                if (key.Index is var index && index >= 0 && index < _KnownAttachments.Length)
                {
                    var value = _KnownAttachments[index];
                    if (value != null)
                    {
                        return value;
                    }
                    var newValue = valueFactory();
                    if (Interlocked.CompareExchange(ref _KnownAttachments[index], newValue, null) is var oldValue && oldValue == null)
                    {
                        return newValue;
                    }
                    else
                    {
                        return oldValue;
                    }
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        if (_DynamicAttachments.TryGetValue(key.Value, out var value))
                        {
                            return value;
                        }
                    }
                    var newValue = valueFactory();
                    lock (_DynamicAttachments)
                    {
                        if (_DynamicAttachments.TryGetValue(key.Value, out var value))
                        {
                            return value;
                        }
                        _DynamicAttachments[key.Value] = newValue;
                        return newValue;
                    }
                }
            }
            public object GetOrCreate(string key, Func<object> valueFactory)
            {
                var index = _KnownKeys.IsPooled(key);
                if (index >= 0 && index < _KnownAttachments.Length)
                {
                    var value = _KnownAttachments[index];
                    if (value != null)
                    {
                        return value;
                    }
                    var newValue = valueFactory();
                    if (Interlocked.CompareExchange(ref _KnownAttachments[index], newValue, null) is var oldValue && oldValue == null)
                    {
                        return newValue;
                    }
                    else
                    {
                        return oldValue;
                    }
                }
                else
                {
                    lock (_DynamicAttachments)
                    {
                        if (_DynamicAttachments.TryGetValue(key, out var value))
                        {
                            return value;
                        }
                    }
                    var newValue = valueFactory();
                    lock (_DynamicAttachments)
                    {
                        if (_DynamicAttachments.TryGetValue(key, out var value))
                        {
                            return value;
                        }
                        _DynamicAttachments[key] = newValue;
                        return newValue;
                    }
                }
            }
        }

        private static readonly ConditionalWeakTable<object, WeakAttachmentStorage> _Map = new ConditionalWeakTable<object, WeakAttachmentStorage>();
        private static WeakAttachmentStorage GetStorage(object obj)
        {
            _Map.TryGetValue(obj, out var storage);
            return storage;
        }
        private static WeakAttachmentStorage GetOrCreateStorage(object obj)
        {
            return _Map.GetOrCreateValue(obj);
        }

        public static void SetAttachment(this object parent, string name, object attach)
        {
            if (attach == null)
            {
                GetStorage(parent)?.Set(name, null);
            }
            else
            {
                GetOrCreateStorage(parent).Set(name, attach);
            }
        }
        public static object GetAttachment(this object parent, string name)
        {
            return GetStorage(parent)?.Get(name);
        }
        public static object GetOrCreateAttachment(this object parent, string name, Func<object> attachFactory)
        {
            return GetOrCreateStorage(parent).GetOrCreate(name, attachFactory);
        }
        public static object GetOrCreateAttachment(this object parent, string name, object defaultValue)
        {
            return GetOrCreateStorage(parent).GetOrCreate(name, () => defaultValue);
        }
        public static void SetAttachment(this object parent, Key name, object attach)
        {
            if (attach == null)
            {
                GetStorage(parent)?.Set(name, null);
            }
            else
            {
                GetOrCreateStorage(parent).Set(name, attach);
            }
        }
        public static object GetAttachment(this object parent, Key name)
        {
            return GetStorage(parent)?.Get(name);
        }
        public static object GetOrCreateAttachment(this object parent, Key name, Func<object> attachFactory)
        {
            return GetOrCreateStorage(parent).GetOrCreate(name, attachFactory);
        }
        public static object GetOrCreateAttachment(this object parent, Key name, object defaultValue)
        {
            return GetOrCreateStorage(parent).GetOrCreate(name, () => defaultValue);
        }

        public static void SetAttachment<T>(this object parent, string name, T attach) where T : struct
        {
            GetOrCreateStorage(parent).Set(name, attach);
        }
        public static T GetAttachment<T>(this object parent, string name)
        {
            if (GetStorage(parent)?.Get(name) is T value)
            {
                return value;
            }
            return default;
        }
        public static T GetOrCreateAttachment<T>(this object parent, string name, Func<T> attachFactory)
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => attachFactory());
        }
        public static T GetOrCreateAttachment<T>(this object parent, string name, T defaultValue)
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => defaultValue);
        }
        public static T GetOrCreateAttachment<T>(this object parent, string name) where T : new()
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => new T());
        }
        public static void SetAttachment<T>(this object parent, Key name, T attach) where T : struct
        {
            GetOrCreateStorage(parent).Set(name, attach);
        }
        public static T GetAttachment<T>(this object parent, Key name)
        {
            if (GetStorage(parent)?.Get(name) is T value)
            {
                return value;
            }
            return default;
        }
        public static T GetOrCreateAttachment<T>(this object parent, Key name, Func<T> attachFactory)
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => attachFactory());
        }
        public static T GetOrCreateAttachment<T>(this object parent, Key name, T defaultValue)
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => defaultValue);
        }
        public static T GetOrCreateAttachment<T>(this object parent, Key name) where T : new()
        {
            return (T)GetOrCreateStorage(parent).GetOrCreate(name, () => new T());
        }

        /// <remarks>It is suggested to call the overload which take "Key" as parameter, because it is more efficient.</remarks>
        public static void SetAttachment<T>(this object parent, T attach)
        {
            SetAttachment(parent, typeof(T).FullName, attach);
        }
        /// <remarks>It is suggested to call the overload which take "Key" as parameter, because it is more efficient.</remarks>
        public static T GetAttachment<T>(this object parent)
        {
            return GetAttachment<T>(parent, typeof(T).FullName);
        }
        /// <remarks>It is suggested to call the overload which take "Key" as parameter, because it is more efficient.</remarks>
        public static T GetOrCreateAttachment<T>(this object parent, Func<T> attachFactory)
        {
            return GetOrCreateAttachment(parent, typeof(T).FullName, attachFactory);
        }
        /// <remarks>It is suggested to call the overload which take "Key" as parameter, because it is more efficient.</remarks>
        public static T GetOrCreateAttachment<T>(this object parent, T defaultValue)
        {
            return GetOrCreateAttachment(parent, typeof(T).FullName, defaultValue);
        }
        /// <remarks>It is suggested to call the overload which take "Key" as parameter, because it is more efficient.</remarks>
        public static T GetOrCreateAttachment<T>(this object parent) where T : new()
        {
            return GetOrCreateAttachment<T>(parent, typeof(T).FullName);
        }
    }
}
