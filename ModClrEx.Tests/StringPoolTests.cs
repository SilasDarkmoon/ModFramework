using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace ModClrEx.Tests
{
    public class StringPoolTests
    {
        private static readonly string[] _TestStrings = new string[1024 * 1024 * 2];
        static StringPoolTests()
        {
            for (int i = 0; i < 1024 * 1024; ++i)
            {
                _TestStrings[i * 2] = _TestStrings[i * 2 + 1] = Guid.NewGuid().ToString();
            }
        }

        [Fact]
        public void TestPushAndGet()
        {
            // 测试基本的 Push 和 Get 功能
            var str1 = "test1";
            var str2 = "test2";

            var pool = new Mod.StringPoolConcurrent();
            var index1 = pool.Push(str1);
            var index2 = pool.Push(str2);

            Assert.NotEqual(-1, index1);
            Assert.NotEqual(-1, index2);
            Assert.NotEqual(index1, index2);

            var retrievedStr1 = pool.Get(index1);
            var retrievedStr2 = pool.Get(index2);

            Assert.Equal(str1, retrievedStr1);
            Assert.Equal(str2, retrievedStr2);
        }

        [Fact]
        public void TestNullString()
        {
            var pool = new Mod.StringPoolConcurrent();
            // 测试 null 字符串的处理
            var index = pool.Push(null);
            Assert.Equal(-1, index);

            var retrieved = pool.Get(-1);
            Assert.Null(retrieved);
        }

        [Fact]
        public void TestOutOfRangeIndex()
        {
            var pool = new Mod.StringPoolConcurrent();
            // 测试超出范围的索引
            var retrieved = pool.Get(int.MaxValue);
            Assert.Null(retrieved);
        }

        [Fact]
        public void TestDuplicateStrings()
        {
            var pool = new Mod.StringPoolConcurrent();
            // 测试重复字符串的处理
            var str = "duplicate";
            var index1 = pool.Push(str);
            var index2 = pool.Push(str);

            Assert.Equal(index1, index2);

            var retrievedStr = pool.Get(index1);
            Assert.Equal(str, retrievedStr);
        }

        [Fact]
        public async Task TestThreadSafety()
        {
            var pool = new Mod.StringPoolConcurrent();
            // 测试线程安全性
            const int threadCount = 10;
            const int stringsPerThread = 100;
            var tasks = new Task[threadCount];
            var stringIndices = new Dictionary<string, int>();
            var stringValues = new List<string>();

            // 生成测试字符串
            for (int i = 0; i < stringsPerThread; i++)
            {
                stringValues.Add($"test_{i}");
            }

            // 启动多个线程并发访问
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    foreach (var str in stringValues)
                    {
                        var index = pool.Push(str);
                        lock (stringIndices)
                        {
                            if (!stringIndices.ContainsKey(str))
                            {
                                stringIndices[str] = index;
                            }
                            else
                            {
                                // 验证重复字符串返回相同的索引
                                Assert.Equal(stringIndices[str], index);
                            }
                        }
                    }
                });
            }

            // 等待所有线程完成
            await Task.WhenAll(tasks);

            // 验证所有字符串都能正确检索
            foreach (var kvp in stringIndices)
            {
                var retrievedStr = pool.Get(kvp.Value);
                Assert.Equal(kvp.Key, retrievedStr);
            }
        }

        [Fact]
        public async Task TestHighConcurrency()
        {
            var pool = new Mod.StringPoolConcurrent();
            // 测试高并发场景
            const int threadCount = 50;
            const int operationsPerThread = 1000;
            var tasks = new Task[threadCount];

            // 启动大量线程并发操作
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var str = $"concurrency_test_{threadId}_{j % 100}"; // 确保有重复字符串
                        var index = pool.Push(str);
                        var retrievedStr = pool.Get(index);
                        Assert.Equal(str, retrievedStr);
                    }
                });
            }

            // 等待所有线程完成
            await Task.WhenAll(tasks);
        }

        [Fact]
        public void TestPerformanceConcurrentPool()
        {
            var pool = new Mod.StringPoolConcurrent();
            foreach (var str in _TestStrings)
            {
                pool.Push(str);
            }
        }
        [Fact]
        public void TestPerformanceLockedPool()
        {
            var pool = new Mod.StringPoolLocked();
            foreach (var str in _TestStrings)
            {
                pool.Push(str);
            }
        }
        [Fact]
        public void TestPerformanceConcurrentPoolWhenConcurrent()
        {
            var pool = new Mod.StringPoolConcurrent();
            Parallel.ForEach(_TestStrings, str =>
            {
                pool.Push(str);
            });
        }
        [Fact]
        public void TestPerformanceLockedPoolWhenConcurrent()
        {
            var pool = new Mod.StringPoolLocked();
            Parallel.ForEach(_TestStrings, str =>
            {
                pool.Push(str);
            });
        }
    }
}
