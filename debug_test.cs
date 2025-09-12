using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace DebugTest
{
    class Program
    {
        static void Main()
        {
            var mem = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(mem);

            Console.WriteLine($"Initial count: {links.Count()}");

            var source = links.CreatePoint();
            Console.WriteLine($"After creating source: {links.Count()}");
            
            var target = links.CreatePoint();
            Console.WriteLine($"After creating target: {links.Count()}");

            // Test original CreateAndUpdate
            var result1 = links.CreateAndUpdate(source, target);
            Console.WriteLine($"After CreateAndUpdate(source, target): {links.Count()}, result: {result1}");

            // Test with IList version
            var restriction = new uint[] { source, target };
            try 
            {
                var result2 = links.CreateAndUpdate(restriction);
                Console.WriteLine($"After CreateAndUpdate(restriction): {links.Count()}, result: {result2}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
            }
        }
    }
}