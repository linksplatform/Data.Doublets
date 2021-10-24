namespace Platform::Data::Doublets::Benchmarks
{
    private: Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<MemoryBenchmarks>();
        }
    }
}
