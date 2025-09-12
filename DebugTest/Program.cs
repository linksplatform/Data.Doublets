using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Platform.Data.Doublets;

namespace DebugTest
{
    class Program
    {
        static void Main()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            Console.WriteLine($"Initial count: {links.Count(null)}");

            var source = links.CreatePoint();
            Console.WriteLine($"After creating source: {links.Count(null)}");
            
            var target = links.CreatePoint();
            Console.WriteLine($"After creating target: {links.Count(null)}");

            // Test original CreateAndUpdate
            var result1 = links.CreateAndUpdate(source, target);
            Console.WriteLine($"After CreateAndUpdate(source, target): {links.Count(null)}, result: {result1}");

            // Test with IList version
            var restriction = new uint[] { source, target };
            try 
            {
                var result2 = links.CreateAndUpdate(restriction);
                Console.WriteLine($"After CreateAndUpdate(restriction): {links.Count(null)}, result: {result2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
}
