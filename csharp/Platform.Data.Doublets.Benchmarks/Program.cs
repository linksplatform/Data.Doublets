using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<MemoryBenchmarks>();
            BenchmarkRunner.Run<LinkAddressBenchmarks>();
        }
    }
}
