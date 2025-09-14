using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    class Program
    {
        static void Main()
        {
            // Focus only on the performance comparison for issue #85
            BenchmarkRunner.Run<IsChildCheckBenchmarks>();
            // BenchmarkRunner.Run<CountBenchmarks>();
            // BenchmarkRunner.Run<LinkStructBenchmarks>();
            // BenchmarkRunner.Run<MemoryBenchmarks>();
        }
    }
}
