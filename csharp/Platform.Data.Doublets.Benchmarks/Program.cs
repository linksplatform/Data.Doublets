using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using BenchmarkDotNet.Running;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

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
            // BenchmarkRunner.Run<MemoryBenchmarks>();

            File.Delete("db.links");

            IResizableDirectMemory mem = new FileMappedResizableDirectMemory("db.links");
            //mem = new HeapResizableDirectMemory();
            
            ILinks<uint> links = new UnitedMemoryLinks<uint>(mem);
            links = new LinksUniquenessResolver<uint>(links);

            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            links.CreateAndUpdate((uint)1, (uint)1);
            
            links.Each((link) =>
            {
                Console.WriteLine(links.Format(link));
                return links.Constants.Any;
            });
        }
    }
}
