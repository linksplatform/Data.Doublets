using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<CountBenchmarks>();
            BenchmarkRunner.Run<LinkStructBenchmarks>();
            // BenchmarkRunner.Run<MemoryBenchmarks>();
        }
    }
}
