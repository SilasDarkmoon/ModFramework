using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Mod;
using Xunit;

namespace ModClrEx.Tests
{
    public class WeakDictionaryTests
    {
        private static void Collect()
        {
            for (int i = 0; i < 8; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static bool IsAlive<T>(WeakReference<T> reference) where T : class
        {
            return reference.TryGetTarget(out _);
        }

        [Fact]
        public void DictionaryInterfaceMembersWork()
        {
            var dict = new WeakDictionary<string, object>();
            var key1 = "alpha";
            var key2 = "beta";
            var key3 = "gamma";
            var value1 = new object();
            var value2 = new object();
            var value3 = new object();

            dict.AddOrUpdate(key1, value1);
            dict[key2] = value2;
            IDictionary<string, object> generic = dict;
            generic.Add(key3, value3);

            Assert.Same(value1, dict[key1]);
            Assert.True(dict.TryGetValue(new string(key2.ToCharArray()), out var found));
            Assert.Same(value2, found);
            Assert.False(dict.TryGetValue("missing", out _));
            Assert.True(dict.ContainsKey(key1));
            Assert.Equal(3, dict.Count);
            Assert.Equal(3, dict.Keys.Count);
            Assert.Contains(key1, dict.Keys);
            Assert.Equal(3, dict.Values.Count);
            Assert.Contains(value1, dict.Values);
            Assert.Single(dict, pair => pair.Key == key2 && ReferenceEquals(pair.Value, value2));

            Assert.Throws<ArgumentException>(() => generic.Add(new string(key1.ToCharArray()), value1));
            Assert.Throws<KeyNotFoundException>(() => dict["missing"]);

            Assert.True(dict.Remove(key3));
            Assert.False(dict.Remove(key3));
            Assert.True(((ICollection<KeyValuePair<string, object>>)dict).Remove(
                new KeyValuePair<string, object>(key2, value2)));
            Assert.Single(dict);

            dict.Clear();
            Assert.Empty(dict);
            Assert.False(dict.ContainsKey(key1));
        }

        [Fact]
        public void ReadOnlyAndNonGenericInterfacesWork()
        {
            var dict = new WeakDictionary<string, object>();
            var key = "key";
            var value = new object();
            dict[key] = value;

            IReadOnlyDictionary<string, object> readOnly = dict;
            Assert.Single(readOnly);
            Assert.Same(value, readOnly[key]);
            Assert.Contains(key, readOnly.Keys);
            Assert.Contains(value, readOnly.Values);

            var collection = (ICollection<KeyValuePair<string, object>>)dict;
            var pair = new KeyValuePair<string, object>(key, value);
            Assert.True(collection.Contains(pair));
            Assert.False(collection.IsReadOnly);
            var array = new KeyValuePair<string, object>[2];
            collection.CopyTo(array, 1);
            Assert.Null(array[0].Key);
            Assert.Equal(new[] { key }, array.Skip(1).Select(item => item.Key).ToArray());

            IDictionary nonGeneric = dict;
            Assert.Same(value, nonGeneric[key]);
            Assert.True(nonGeneric.Contains(key));
            Assert.False(nonGeneric.Contains("missing"));
            Assert.False(nonGeneric.IsFixedSize);
            Assert.False(nonGeneric.IsSynchronized);
            Assert.Same(dict, nonGeneric.SyncRoot);

            nonGeneric.Add("extra", value);
            Assert.Equal(2, dict.Count);
            nonGeneric.Remove("extra");
            Assert.Single(dict);

            var entries = new DictionaryEntry[2];
            nonGeneric.CopyTo(entries, 0);
            Assert.Equal(key, entries[0].Key);

            var enumerator = nonGeneric.GetEnumerator();
            Assert.True(enumerator.MoveNext());
            Assert.IsType<string>(enumerator.Key);
            Assert.False(enumerator.MoveNext());
        }

        [Fact]
        public void KeyAndValueAreCollectedWhenKeyDies()
        {
            var dict = new WeakDictionary<string, object>();
            var refs = AddEphemeral(dict);
            Collect();

            Assert.False(IsAlive(refs.Key));
            Assert.False(IsAlive(refs.Value));
            Assert.Empty(dict);
            Assert.False(dict.ContainsKey(new string(EphemeralKey.ToCharArray())));
        }

        [Fact]
        public void EqualKeyInstancesShareEntryWhileOneIsAlive()
        {
            var dict = new WeakDictionary<string, object>();
            var refs = AddTwoEqualWithFirstAlive(dict);
            Collect();

            Assert.False(IsAlive(refs.Second));
            Assert.True(IsAlive(refs.Value));
            Assert.Single(dict);
            Assert.True(HasExpectedValue(dict, refs.Value));
            dict.Clear();
        }

        [Fact]
        public void ReAddAfterKeyDeathWorks()
        {
            var dict = new WeakDictionary<string, object>();
            var first = AddEphemeral(dict);
            Collect();
            Assert.Empty(dict);

            var value = new object();
            dict[EphemeralKey] = value;
            Assert.Same(value, dict[new string(EphemeralKey.ToCharArray())]);
            Assert.Single(dict);
            GC.KeepAlive(first);
        }

        private const string EphemeralKey = "ephemeral-key";
        private const string SharedKey = "shared-key";

        private readonly struct EphemeralRefs
        {
            public readonly WeakReference<string> Key;
            public readonly WeakReference<object> Value;

            public EphemeralRefs(WeakReference<string> key, WeakReference<object> value)
            {
                Key = key;
                Value = value;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static bool HasExpectedValue(WeakDictionary<string, object> dict, WeakReference<object> valueRef)
        {
            return valueRef.TryGetTarget(out var expected) &&
                   ReferenceEquals(expected, dict[new string(SharedKey.ToCharArray())]);
        }

        private sealed class TwoEqualRefs
        {
            public string[]? Keep;
            public WeakReference<string> Second = null!;
            public WeakReference<object> Value = null!;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static EphemeralRefs AddEphemeral(WeakDictionary<string, object> dict)
        {
            var key = new string(EphemeralKey.ToCharArray());
            var value = new object();
            dict.AddOrUpdate(key, value);
            return new EphemeralRefs(new WeakReference<string>(key), new WeakReference<object>(value));
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static TwoEqualRefs AddTwoEqualWithFirstAlive(WeakDictionary<string, object> dict)
        {
            var keep = new[] { new string(SharedKey.ToCharArray()) };
            var second = new string(SharedKey.ToCharArray());
            var value = new object();
            dict.AddOrUpdate(keep[0], value);
            dict.AddOrUpdate(second, value);
            return new TwoEqualRefs
            {
                Keep = keep,
                Second = new WeakReference<string>(second),
                Value = new WeakReference<object>(value)
            };
        }
    }
}
