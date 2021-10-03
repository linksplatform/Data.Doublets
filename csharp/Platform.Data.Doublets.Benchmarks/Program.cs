using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    private Program
    {
        /// <summary>
        /// <para>
        /// Main.
        /// </para>
        /// <para></para>
        /// </summary>
        static void Main()
        {
            BenchmarkRunner.Run<MemoryBenchmarks>();
        }
    }
}
