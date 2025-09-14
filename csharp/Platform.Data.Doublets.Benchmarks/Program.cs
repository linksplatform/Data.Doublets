using BenchmarkDotNet.Running;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Columns;

namespace Platform.Data.Doublets.Benchmarks
{
    class Program
    {
        static void Main()
        {
            // Create a custom configuration for better benchmark comparison
            var config = DefaultConfig.Instance
                .AddColumn(StatisticColumn.Mean)
                .AddColumn(StatisticColumn.StdDev)
                .AddColumn(StatisticColumn.Median);

            // Original benchmarks
            BenchmarkRunner.Run<CountBenchmarks>(config);
            BenchmarkRunner.Run<LinkStructBenchmarks>(config);
            
            // New comparison benchmarks for experimental code
            BenchmarkRunner.Run<TreeImplementationsBenchmarks>(config);
            BenchmarkRunner.Run<MemoryImplementationsBenchmarks>(config);
            BenchmarkRunner.Run<ExperimentalAlgorithmsBenchmarks>(config);
            
            // BenchmarkRunner.Run<MemoryBenchmarks>(config);
        }
    }
}
