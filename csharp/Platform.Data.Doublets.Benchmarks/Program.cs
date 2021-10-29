using BenchmarkDotNet.Running;

namespace Platform.Data.Doublets.Benchmarks
{
    /// <summary>
    /// <para>
    /// Represents the program.
    /// </para>
    /// <para></para>
    /// </summary>
    class Program
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
