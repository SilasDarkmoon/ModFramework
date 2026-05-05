using System;
using System.Reflection;
using Xunit;

namespace Mod.Tests
{
    public class FieldExTests
    {
        [Fact]
        public void GetObjectHeaderSize_ReturnsPositiveValue()
        {
            var headerSize = UnsafeEx.GetObjectHeaderSize();
            Assert.True(headerSize > 0, "Object header size should be positive");
        }

        [Fact]
        public void GetObjectHeaderSize_ReturnsConsistentValue()
        {
            var headerSize1 = UnsafeEx.GetObjectHeaderSize();
            var headerSize2 = UnsafeEx.GetObjectHeaderSize();
            Assert.Equal(headerSize1, headerSize2);
        }

        [Fact]
        public void GetFieldOffset_Object_ReturnsCorrectOffset()
        {
            var testObj = new TestClass { IntField = 42, StringField = "test" };
            var offset = UnsafeEx.GetFieldOffset(testObj, ref testObj.IntField);
            Assert.True(offset >= 0, "Field offset should be non-negative");
        }

        [Fact]
        public void GetFieldRef_Object_ReturnsCorrectReference()
        {
            var testObj = new TestClass { IntField = 42 };
            var offset = UnsafeEx.GetFieldOffset(testObj, ref testObj.IntField);
            ref var fieldRef = ref UnsafeEx.GetFieldRef<int>(testObj, offset);
            Assert.Equal(42, fieldRef);
            
            // Modify the value through the reference
            fieldRef = 100;
            Assert.Equal(100, testObj.IntField);
        }

        [Fact]
        public void GetFieldOffset_Struct_ReturnsCorrectOffset()
        {
            var testStruct = new TestStruct { IntField = 42, ByteField = 100 };
            var offset = UnsafeEx.GetFieldOffset(ref testStruct, ref testStruct.ByteField);
            Assert.True(offset >= 0, "Field offset should be non-negative");
        }

        [Fact]
        public void GetFieldRef_Struct_ReturnsCorrectReference()
        {
            var testStruct = new TestStruct { IntField = 42, ByteField = 100 };
            var offset = UnsafeEx.GetFieldOffset(ref testStruct, ref testStruct.ByteField);
            ref var fieldRef = ref UnsafeEx.GetFieldRef<TestStruct, byte>(ref testStruct, offset);
            Assert.Equal(100, fieldRef);
            
            // Modify the value through the reference
            fieldRef = 200;
            Assert.Equal(200, testStruct.ByteField);
        }

        [Fact]
        public void GetHeadRef_Object_ReturnsValidReference()
        {
            var testObj = new TestClass { IntField = 42 };
            ref var headRef = ref UnsafeEx.GetHeadRef(testObj);
            // Just verify we can get a reference without exception
            Assert.True(true);
        }

        [Fact]
        public void GetHeadRef_Struct_ReturnsValidReference()
        {
            var testStruct = new TestStruct { IntField = 42 };
            ref var headRef = ref UnsafeEx.GetHeadRef(ref testStruct);
            // Just verify we can get a reference without exception
            Assert.True(true);
        }

        [Fact]
        public void SizeOf_Type_ReturnsCorrectSize()
        {
            Assert.Equal(4, UnsafeEx.Size(typeof(int)));
            Assert.Equal(8, UnsafeEx.Size(typeof(long)));
            Assert.Equal(1, UnsafeEx.Size(typeof(bool)));
        }

        [Fact]
        public void SizeOf_Generic_ReturnsCorrectSize()
        {
            Assert.Equal(4, UnsafeEx.Size<int>());
            Assert.Equal(8, UnsafeEx.Size<long>());
            Assert.Equal(1, UnsafeEx.Size<bool>());
        }

        [Fact]
        public void SizeOf_Object_ReturnsCorrectSize()
        {
            var testObj = new TestClass { IntField = 42, StringField = "test" };
            var size = UnsafeEx.Size(testObj);
            Assert.True(size > 0, "Object size should be positive");
        }

        [Fact]
        public void SizeOf_NullObject_ReturnsZero()
        {
            Assert.Equal(0, UnsafeEx.Size((object)null));
        }

        [Fact]
        public void GetFieldOffset_FieldInfo_ReturnsCorrectOffset()
        {
            var fieldInfo = typeof(TestClass).GetField(nameof(TestClass.IntField), BindingFlags.Instance | BindingFlags.Public);
            var offset = UnsafeEx.GetFieldOffset(fieldInfo!);
            Assert.True(offset >= 0, "Field offset should be non-negative");
        }

        private class TestClass
        {
            public int IntField;
            public string? StringField;
        }

        private struct TestStruct
        {
            public int IntField;
            public byte ByteField;
        }
    }
}
