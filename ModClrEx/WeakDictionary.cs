using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Mod
{
    // ConditionalWeakTable only compares Key's address. This collection will avoid duplicated keys like normal Dictionary.
    public class WeakDictionary<TKey, TValue> where TKey : class
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
            public Key Key;
            public LinkedListNode<WeakReference> StubNode;
            public int HashCode;

            public KeyStub(Key key, LinkedListNode<WeakReference> node)
            {
                Key = key;
                StubNode = node;
                HashCode = key.HashCode;
            }

            private void DisposeImp()
            {
                if (Key != null)
                {
                    HashSlot slot = null;
                    lock (Key)
                    {
                        Key.Stubs.Remove(StubNode);
                        if (Key.Stubs.Count == 0)
                        {
                            slot = Key.Slot;
                        }
                    }
                    if (slot != null)
                    { // should cleanup the Key
                        lock (slot.Lock)
                        {
                            if (Key.InUsing <= 0)
                            {
                                lock (Key)
                                {
                                    if (Key.Stubs.Count == 0)
                                    {
                                        for (int i = 0; i < slot.Bucket.Count; ++i)
                                        {
                                            if (object.ReferenceEquals(slot.Bucket[i], Key))
                                            {
                                                slot.Bucket.RemoveAt(i);
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    Key = null;
                    StubNode = null;
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
        public TValue Get(TKey key)
        {
            if (_KeyMap.TryGetValue(key, out var stub))
            {
                lock (stub.Key)
                {
                    return stub.Key.Value;
                }
            }
            else
            {
                var finder = new KeyFinder(key);
                var mapkey = TryFindInSlots(finder) as Key;
                if (mapkey != null)
                {
                    lock (mapkey)
                    {
                        return mapkey.Value;
                    }
                }
            }
            return default;
        }
    }
}
