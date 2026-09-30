using Xunit;

namespace ModClrExUnsafe.Tests
{
    /// <summary>
    /// PointerDelegate 测试入口（xUnit 包装）。
    /// 实际测试逻辑在 <see cref="PointerDelegateTestSuite"/>（自包含，Unity 与 dotnet 间直接拷贝同步）。
    /// </summary>
    public class PointerDelegateTests
    {
        [Fact]
        public void PointerDelegateSuite_AllPass()
        {
            var rep = PointerDelegateTestSuite.RunAll(gcStressMs: 1500, parallelGC: false);
            Assert.True(rep.Fail == 0,
                $"{rep.Fail} failed:\n{string.Join("\n", rep.Failures)}");
        }

        [Fact]
        public void PointerDelegateSuite_ParallelGcStress()
        {
            // 并行 + 高频压缩 GC 压测（时长短，作为 CI 冒烟；Unity 侧可调大时长）
            var rep = PointerDelegateTestSuite.RunAll(gcStressMs: 2000, parallelGC: true);
            Assert.True(rep.Fail == 0,
                $"{rep.Fail} failed:\n{string.Join("\n", rep.Failures)}");
        }
    }
}
