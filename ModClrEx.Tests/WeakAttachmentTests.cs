using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Mod;

namespace ModClrEx.Tests
{
    public class WeakAttachmentTests
    {
        [Fact]
        public void TestBasicAttachment()
        {
            // 测试基本的 SetAttachment 和 GetAttachment 功能
            var obj = new object();
            var key = "testKey";
            var value = new object();

            // 设置附件
            obj.SetAttachment(key, value);

            // 获取附件
            var retrievedValue = obj.GetAttachment(key);

            // 验证附件是否正确
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TestAttachmentWithKey()
        {
            // 测试使用 Key 对象作为键的情况
            var obj = new object();
            var key = new Mod.WeakAttachment.Key("testKey");
            var value = new object();

            // 设置附件
            obj.SetAttachment(key, value);

            // 获取附件
            var retrievedValue = obj.GetAttachment(key);

            // 验证附件是否正确
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TestGenericAttachment()
        {
            // 测试泛型版本的 SetAttachment 和 GetAttachment
            var obj = new object();
            var key = "testKey";
            var value = 42;

            // 设置附件
            obj.SetAttachment(key, value);

            // 获取附件
            var retrievedValue = obj.GetAttachment<int>(key);

            // 验证附件是否正确
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TestGetOrCreateAttachment()
        {
            // 测试 GetOrCreateAttachment 功能
            var obj = new object();
            var key = "testKey";
            var defaultValue = 42;

            // 获取或创建附件
            var retrievedValue = obj.GetOrCreateAttachment(key, defaultValue);

            // 验证附件是否正确创建
            Assert.Equal(defaultValue, retrievedValue);

            // 再次获取，应该返回相同的值
            var retrievedValue2 = obj.GetOrCreateAttachment(key, 100);
            Assert.Equal(defaultValue, retrievedValue2);
        }

        [Fact]
        public void TestGetOrCreateAttachmentWithFactory()
        {
            // 测试使用工厂函数的 GetOrCreateAttachment
            var obj = new object();
            var key = "testKey";
            var factoryCalled = false;

            // 获取或创建附件
            var retrievedValue = obj.GetOrCreateAttachment(key, () =>
            {
                factoryCalled = true;
                return "createdValue";
            });

            // 验证工厂函数被调用
            Assert.True(factoryCalled);
            Assert.Equal("createdValue", retrievedValue);

            // 再次获取，工厂函数不应该被调用
            factoryCalled = false;
            var retrievedValue2 = obj.GetOrCreateAttachment(key, () =>
            {
                factoryCalled = true;
                return "newValue";
            });
            Assert.False(factoryCalled);
            Assert.Equal("createdValue", retrievedValue2);
        }

        [Fact]
        public void TestGetOrCreateAttachmentWithNew()
        {
            // 测试使用 new() 约束的 GetOrCreateAttachment
            var obj = new object();
            var key = "testKey";

            // 获取或创建附件
            var retrievedValue = obj.GetOrCreateAttachment<TestClass>(key);

            // 验证附件是否正确创建
            Assert.NotNull(retrievedValue);
            Assert.IsType<TestClass>(retrievedValue);
        }

        [Fact]
        public void TestTypeBasedAttachment()
        {
            // 测试基于类型的附件
            var obj = new object();
            var value = new TestClass();

            // 设置附件
            obj.SetAttachment(value);

            // 获取附件
            var retrievedValue = obj.GetAttachment<TestClass>();

            // 验证附件是否正确
            Assert.Equal(value, retrievedValue);
        }

        [Fact]
        public void TestWeakReference()
        {
            // 测试弱引用特性
            var obj = new object();
            var key = "testKey";
            var value = new object();

            // 设置附件
            obj.SetAttachment(key, value);

            // 验证附件存在
            Assert.NotNull(obj.GetAttachment(key));

            // 清除引用并强制垃圾回收
            obj = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            // 创建新对象，应该没有附件
            var newObj = new object();
            Assert.Null(newObj.GetAttachment(key));
        }

        [Fact]
        public async Task TestThreadSafety()
        {
            // 测试线程安全性
            var obj = new object();
            const int threadCount = 10;
            const int operationsPerThread = 100;
            var tasks = new Task[threadCount];

            // 启动多个线程并发访问
            for (int i = 0; i < threadCount; i++)
            {
                int threadId = i;
                tasks[i] = Task.Run(() =>
                {
                    for (int j = 0; j < operationsPerThread; j++)
                    {
                        var key = $"key_{threadId}_{j}";
                        var value = $"value_{threadId}_{j}";

                        // 设置附件
                        obj.SetAttachment(key, value);

                        // 获取附件
                        var retrievedValue = obj.GetAttachment(key);

                        // 验证附件是否正确
                        Assert.Equal(value, retrievedValue);
                    }
                });
            }

            // 等待所有线程完成
            await Task.WhenAll(tasks);
        }

        // 测试类
        private class TestClass
        {
        }
    }
}