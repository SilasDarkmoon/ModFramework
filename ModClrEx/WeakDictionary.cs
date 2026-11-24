using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Mod
{
    // ConditionalWeakTable only compares Key's address. This collection will avoid duplicated keys like normal Dictionary.
    public class WeakDictionary<TKey, TValue> :
        IDictionary<TKey, TValue>,
        IReadOnlyDictionary<TKey, TValue>,
        ICollection<KeyValuePair<TKey, TValue>>,
        IDictionary,
        ICollection
        where TKey : class
    {
        private class HashSlot
        {
            public object Lock = new object();
            public List<object> Bucket = new List<object>();
        }
        private ConcurrentDictionary<int, HashSlot> _Slots = new ConcurrentDictionary<int, HashSlot>();
        private static HashSlot SlotCreator(int hash)
        {
            return new HashSlot();
        }
        private static Func<int, HashSlot> _SlotCreator = SlotCreator;
        private HashSlot FindSlot(int hash)
        {
            var slot = _Slots.GetOrAdd(hash, _SlotCreator);
            return slot;
        }
        private HashSlot TryFindSlot(int hash)
        {
            _Slots.TryGetValue(hash, out var slot);
            return slot;
        }
        private object FindInSlots(object target)
        {
            var slot = FindSlot(target.GetHashCode());
            lock (slot.Lock)
            {
                for (int i = 0; i < slot.Bucket.Count; ++i)
                {
                    object existing = slot.Bucket[i];
                    if (target.Equals(existing))
                    {
                        return existing;
                    }
                }
            }
            return null;
        }
        private object TryFindInSlots(object target)
        {
            var slot = TryFindSlot(target.GetHashCode());
            if (slot != null)
            {
                lock (slot.Lock)
                {
                    for (int i = 0; i < slot.Bucket.Count; ++i)
                    {
                        object existing = slot.Bucket[i];
                        if (target.Equals(existing))
                        {
                            return existing;
                        }
                    }
                }
            }
            return null;
        }
        private object FindInSlotsOrCreate(object target, Func<object, HashSlot, object> creator)
        {
            var slot = FindSlot(target.GetHashCode());
            lock (slot.Lock)
            {
                for (int i = 0; i < slot.Bucket.Count; ++i)
                {
                    object existing = slot.Bucket[i];
                    if (target.Equals(existing))
                    {
                        return existing;
                    }
                }
                var newobj = creator(target, slot);
                slot.Bucket.Add(newobj);
                return newobj;
            }
        }
        private object FindInSlotsAndUpdate(object target, Func<object, HashSlot, object> creator, Func<object, object> updater)
        {
            var slot = FindSlot(target.GetHashCode());
            lock (slot.Lock)
            {
                for (int i = 0; i < slot.Bucket.Count; ++i)
                {
                    object existing = slot.Bucket[i];
                    if (target.Equals(existing))
                    {
                        var updated = updater(existing);
                        slot.Bucket[i] = updated;
                        return updated;
                    }
                }
                var newobj = creator(target, slot);
                slot.Bucket.Add(newobj);
                return newobj;
            }
        }
        private static object KeyCreator(object finder, HashSlot slot)
        {
            if (finder is KeyFinder kfinder)
            {
                return new Key(kfinder.GetHashCode(), slot) { InUsing = 1 };
            }
            throw new ArgumentException($"{nameof(finder)} should be a KeyFinder", nameof(finder));
        }
        private static Func<object, HashSlot, object> _KeyCreator = KeyCreator;
        private static object KeyUpdater(object keyobj)
        {
            if (keyobj is Key key)
            {
                ++key.InUsing;
            }
            return keyobj;
        }
        private static Func<object, object> _KeyUpdater = KeyUpdater;

        private Key FindKey(KeyFinder finder)
        {
            return FindInSlotsAndUpdate(finder, _KeyCreator, _KeyUpdater) as Key;
        }

        private static void CleanupKeyIfEmpty(Key key)
        {
            HashSlot slot = key.Slot;
            lock (slot.Lock)
            {
                if (key.InUsing > 0) return;
                lock (key)
                {
                    if (key.Stubs.Count != 0) return;
                    for (int i = 0; i < slot.Bucket.Count; ++i)
                    {
                        if (object.ReferenceEquals(slot.Bucket[i], key))
                        {
                            slot.Bucket.RemoveAt(i);
                            break;
                        }
                    }
                }
            }
        }

        private class KeyFinder
        {
            public object Key;
            public int HashCode;
            public KeyFinder(object key)
            {
                Key = key;
                HashCode = key.GetHashCode();
            }
            public override int GetHashCode()
            {
                return HashCode;
            }
            public override bool Equals(object obj)
            {
                if (obj is KeyFinder other)
                {
                    return Key.Equals(other.Key);
                }
                else if (obj is Key key)
                {
                    return key.Find(Key) != null;
                }
                return false;
            }
        }
        private class Key
        {
            public LinkedList<WeakReference> Stubs = new LinkedList<WeakReference>();
            public int HashCode;
            public HashSlot Slot;
            public TValue Value;
            public int InUsing;

            public LinkedListNode<WeakReference> Find(object key)
            {
                lock (this)
                {
                    var node = Stubs.First;
                    while (node != null)
                    {
                        var next = node.Next;
                        if (node.Value.Target is object obj)
                        {
                            if (key.Equals(obj))
                            {
                                return node;
                            }
                        }
                        else
                        {
                            Stubs.Remove(node);
                        }
                        node = next;
                    }
                    return null;
                }
            }
            public override int GetHashCode()
            {
                return HashCode;
            }
            public override bool Equals(object obj)
            {
                if (obj is Key other)
                {
                    return object.ReferenceEquals(other, this);
                }
                else if (obj is KeyFinder finder)
                {
                    return Find(finder.Key) != null;
                }
                return false;
            }

            public Key(int hashcode, HashSlot slot)
            {
                HashCode = hashcode;
                Slot = slot;
            }
        }
        private class KeyStub : IDisposable
        {
            public readonly Key Key;
            public readonly LinkedListNode<WeakReference> StubNode;
            public readonly int HashCode;
            private int _Disposed;

            public KeyStub(Key key, LinkedListNode<WeakReference> node)
            {
                Key = key;
                StubNode = node;
                HashCode = key.HashCode;
            }

            private void DisposeImp()
            {
                if (Interlocked.Exchange(ref _Disposed, 1) == 0)
                {
                    lock (Key)
                    {
                        Key.Stubs.Remove(StubNode);
                    }
                    CleanupKeyIfEmpty(Key);
                }
            }
            public void Dispose()
            {
                DisposeImp();
                GC.SuppressFinalize(this);
            }
            ~KeyStub()
            {
                DisposeImp();
            }
        }

        private ConditionalWeakTable<object, KeyStub> _KeyMap = new ConditionalWeakTable<object, KeyStub>();

        public void AddOrUpdate(TKey key, TValue value)
        {
            if (_KeyMap.TryGetValue(key, out var stub))
            {
                lock (stub.Key)
                {
                    stub.Key.Value = value;
                }
            }
            else
            {
                var finder = new KeyFinder(key);
                var mapkey = FindKey(finder);
                lock (mapkey)
                {
                    mapkey.Value = value;
                    var node = mapkey.Stubs.AddLast(new WeakReference(key));
                    _KeyMap.Remove(key);
                    _KeyMap.Add(key, new KeyStub(mapkey, node));
                }
                lock (mapkey.Slot.Lock)
                {
                    --mapkey.InUsing;
                }
            }
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_KeyMap.TryGetValue(key, out var stub))
            {
                lock (stub.Key)
                {
                    value = stub.Key.Value;
                    return true;
                }
            }
            var finder = new KeyFinder(key);
            var mapkey = TryFindInSlots(finder) as Key;
            if (mapkey != null)
            {
                lock (mapkey)
                {
                    if (mapkey.Find(key) != null)
                    {
                        value = mapkey.Value;
                        return true;
                    }
                }
            }
            value = default;
            return false;
        }
        public TValue Get(TKey key) => TryGetValue(key, out var value) ? value : default;

        private static readonly Func<TKey, TValue, KeyValuePair<TKey, TValue>> s_PairSelector =
            (key, value) => new KeyValuePair<TKey, TValue>(key, value);
        private static readonly Func<TKey, TValue, TKey> s_KeySelector = (key, value) => key;
        private static readonly Func<TKey, TValue, TValue> s_ValueSelector = (key, value) => value;

        private List<T> SnapshotSelect<T>(Func<TKey, TValue, T> selector)
        {
            var result = new List<T>();
            foreach (var entry in _Slots)
            {
                var slot = entry.Value;
                lock (slot.Lock)
                {
                    foreach (var item in slot.Bucket)
                    {
                        var key = (Key)item;
                        lock (key)
                        {
                            var node = key.Stubs.First;
                            while (node != null)
                            {
                                if (node.Value.Target is TKey target)
                                {
                                    result.Add(selector(target, key.Value));
                                    break;
                                }
                                node = node.Next;
                            }
                        }
                    }
                }
            }
            return result;
        }
        private List<KeyValuePair<TKey, TValue>> Snapshot() => SnapshotSelect(s_PairSelector);

        private int CountSnapshot()
        {
            int count = 0;
            foreach (var entry in _Slots)
            {
                var slot = entry.Value;
                lock (slot.Lock)
                {
                    foreach (var item in slot.Bucket)
                    {
                        var key = (Key)item;
                        lock (key)
                        {
                            var node = key.Stubs.First;
                            while (node != null)
                            {
                                if (node.Value.Target is TKey)
                                {
                                    count++;
                                    break;
                                }
                                node = node.Next;
                            }
                        }
                    }
                }
            }
            return count;
        }

        public TValue this[TKey key]
        {
            get
            {
                if (TryGetValue(key, out var value)) return value;
                throw new KeyNotFoundException("The given key was not present in the dictionary.");
            }
            set => AddOrUpdate(key, value);
        }

        public ICollection<TKey> Keys => SnapshotSelect(s_KeySelector);
        public ICollection<TValue> Values => SnapshotSelect(s_ValueSelector);

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Keys;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Values;

        public int Count => CountSnapshot();
        public bool IsReadOnly => false;
        public bool IsFixedSize => false;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public void Add(TKey key, TValue value)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (ContainsKey(key)) throw new ArgumentException("An item with the same key has already been added.");
            AddOrUpdate(key, value);
        }
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public bool ContainsKey(TKey key) => TryGetValue(key, out _);
        public bool Contains(KeyValuePair<TKey, TValue> item) =>
            TryGetValue(item.Key, out var value) && EqualityComparer<TValue>.Default.Equals(value, item.Value);
        public bool Contains(object key) => key is TKey typed && ContainsKey(typed);

        public bool Remove(TKey key)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (_KeyMap.TryGetValue(key, out var stub))
            {
                _KeyMap.Remove(key);
                stub.Dispose();
                return true;
            }
            var finder = new KeyFinder(key);
            var mapkey = TryFindInSlots(finder) as Key;
            if (mapkey != null)
            {
                lock (mapkey)
                {
                    var node = mapkey.Find(key);
                    if (node == null) return false;
                    if (node.Value.Target is TKey target && _KeyMap.TryGetValue(target, out var targetStub))
                    {
                        _KeyMap.Remove(target);
                        targetStub.Dispose();
                    }
                    else
                    {
                        mapkey.Stubs.Remove(node);
                        CleanupKeyIfEmpty(mapkey);
                    }
                    return true;
                }
            }
            return false;
        }
        public bool Remove(KeyValuePair<TKey, TValue> item) => Contains(item) && Remove(item.Key);
        public void Remove(object key)
        {
            if (key is TKey typed) Remove(typed);
        }

        public void Clear()
        {
            _Slots.Clear();
            _KeyMap = new ConditionalWeakTable<object, KeyStub>();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            foreach (var pair in Snapshot()) array[arrayIndex++] = pair;
        }
        public void CopyTo(Array array, int index)
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            foreach (var pair in Snapshot()) array.SetValue(new DictionaryEntry(pair.Key, pair.Value), index++);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() => Snapshot().GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            var table = new Hashtable();
            foreach (var pair in Snapshot()) table[pair.Key] = pair.Value;
            return table.GetEnumerator();
        }
        object IDictionary.this[object key]
        {
            get => key is TKey typed ? this[typed] : throw new ArgumentException("Invalid key type.", nameof(key));
            set
            {
                if (key is TKey typed) this[typed] = (TValue)value;
                else throw new ArgumentException("Invalid key type.", nameof(key));
            }
        }
        void IDictionary.Add(object key, object value)
        {
            if (key is TKey typed) Add(typed, (TValue)value);
            else throw new ArgumentException("Invalid key type.", nameof(key));
        }
        ICollection IDictionary.Keys
        {
            get
            {
                var list = new ArrayList();
                foreach (var pair in Snapshot()) list.Add(pair.Key);
                return list;
            }
        }
        ICollection IDictionary.Values
        {
            get
            {
                var list = new ArrayList();
                foreach (var pair in Snapshot()) list.Add(pair.Value);
                return list;
            }
        }
    }
}
