using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Mod;
using Xunit;

namespace ModClrEx.Tests
{
    /// <summary>
    /// Concurrency-correctness tests for StringPoolConcurrent / StringPoolLocked
    /// (the existing StringPoolTests only cover single-threaded behavior and
    /// throughput). The invariants under concurrent Push/Get:
    ///   1. same string -> same slot index on every thread (no duplicate slots);
    ///   2. pool.Count == number of DISTINCT strings ever pushed (a string is
    ///      pooled exactly once, no matter how many threads race on it);
    ///   3. every index handed out by Push keeps resolving back to its original
    ///      string ("no holes" - indices never shift or get overwritten).
    /// </summary>
    public class StringPoolConcurrencyTests
    {
        private const int ThreadCount = 8;
        private const int DistinctStrings = 200;

        private static List<string> MakeStrings(int count) =>
            Enumerable.Range(0, count).Select(i => $"pooled_{i}").ToList();

        [Fact]
        public void Concurrent_PushSameStrings_PooledOnce_ConcurrentPool()
        {
            var pool = new StringPoolConcurrent();
            var strings = MakeStrings(DistinctStrings);
            var firstIndex = new ConcurrentDictionary<string, int>();
            var mismatch = 0;

            var barrier = new Barrier(ThreadCount);
            var threads = Enumerable.Range(0, ThreadCount).Select(_ => new Thread(() =>
            {
                barrier.SignalAndWait();   // maximize the race window overlap
                foreach (var s in strings)
                {
                    var idx = pool.Push(s);
                    var known = firstIndex.GetOrAdd(s, idx);
                    if (known != idx) Interlocked.Increment(ref mismatch);
                }
            })).ToArray();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Equal(0, mismatch);                          // invariant 1
            Assert.Equal(DistinctStrings, pool.Count);          // invariant 2
            foreach (var s in strings) Assert.Equal(s, pool.Get(firstIndex[s]));   // invariant 3
        }

        [Fact]
        public void Concurrent_PushSameStrings_PooledOnce_LockedPool()
        {
            var pool = new StringPoolLocked();
            var strings = MakeStrings(DistinctStrings);
            var firstIndex = new ConcurrentDictionary<string, int>();
            var mismatch = 0;

            var barrier = new Barrier(ThreadCount);
            var threads = Enumerable.Range(0, ThreadCount).Select(_ => new Thread(() =>
            {
                barrier.SignalAndWait();
                foreach (var s in strings)
                {
                    var idx = pool.Push(s);
                    var known = firstIndex.GetOrAdd(s, idx);
                    if (known != idx) Interlocked.Increment(ref mismatch);
                }
            })).ToArray();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Equal(0, mismatch);
            Assert.Equal(DistinctStrings, pool.Count);
            foreach (var s in strings) Assert.Equal(s, pool.Get(firstIndex[s]));
        }

        [Fact]
        public void Concurrent_PushGetInterleaved_IndicesStable()
        {
            // interleave Push of new strings with re-Get of old indices: indices
            // handed out earlier must keep resolving to their original strings
            // while other threads keep adding (list growth must not shift slots)
            var pool = new StringPoolConcurrent();
            var indices = new ConcurrentQueue<(string s, int idx)>();
            var errors = new ConcurrentQueue<string>();

            var threads = Enumerable.Range(0, ThreadCount).Select(tid => new Thread(() =>
            {
                var rng = new Random(tid);
                for (int i = 0; i < 300; ++i)
                {
                    var s = $"inter_{tid}_{i}";
                    var idx = pool.Push(s);
                    indices.Enqueue((s, idx));

                    if (pool.Get(idx) != s) errors.Enqueue($"push/get mismatch: {s}");

                    // re-check a random earlier index from this thread
                    if (rng.Next(3) == 0 && indices.TryDequeue(out var old))
                    {
                        var back = pool.Get(old.idx);
                        if (back != old.s) errors.Enqueue($"index shifted: {old.s} -> {back}");
                    }
                }
            })).ToArray();
            foreach (var t in threads) t.Start();
            foreach (var t in threads) t.Join();

            Assert.Empty(errors);
        }
    }
}
