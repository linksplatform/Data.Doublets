using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkSwitcher.FromTypes(new[] { typeof(CountBenchmarks), typeof(LinkStructBenchmarks) }).Run(args.Length == 0 ? new[] { "--filter", "*" } : args);
        }
    }
}
