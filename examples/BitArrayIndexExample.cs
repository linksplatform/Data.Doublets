using System;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Examples
{
    public class BitArrayIndexExample
    {
        public static void RunExample()
        {
            // Create memory links storage
            var memoryManager = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<ulong>(memoryManager);
            
            // Create BitArray index decorator
            var indexedLinks = new BitArrayLinksIndex<ulong>(links, useIndexForReads: true, maintainIndexOnWrites: true);
            
            Console.WriteLine("BitArray Index Example");
            Console.WriteLine("======================");
            
            // Create some test links
            var link1 = indexedLinks.Create(new ulong[] { 1, 2 });
            var link2 = indexedLinks.Create(new ulong[] { 1, 3 });
            var link3 = indexedLinks.Create(new ulong[] { 2, 3 });
            
            Console.WriteLine($"Created links: {link1}, {link2}, {link3}");
            
            // Test counting with source filter
            var constants = links.Constants;
            var countWithSource1 = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
            Console.WriteLine($"Links with source 1: {countWithSource1}");
            
            // Test counting with target filter
            var countWithTarget3 = indexedLinks.Count(new ulong[] { constants.Any, constants.Any, 3 });
            Console.WriteLine($"Links with target 3: {countWithTarget3}");
            
            // Test counting with both source and target
            var countWithSourceAndTarget = indexedLinks.Count(new ulong[] { constants.Any, 1, 3 });
            Console.WriteLine($"Links with source 1 and target 3: {countWithSourceAndTarget}");
            
            // Test update
            indexedLinks.Update(new ulong[] { link1, constants.Any, constants.Any }, new ulong[] { link1, 4, 5 });
            Console.WriteLine("Updated link1 to have source 4 and target 5");
            
            // Verify updated counts
            var countAfterUpdate = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
            Console.WriteLine($"Links with source 1 after update: {countAfterUpdate}");
            
            var countNewSource = indexedLinks.Count(new ulong[] { constants.Any, 4, constants.Any });
            Console.WriteLine($"Links with source 4 after update: {countNewSource}");
            
            // Test delete
            indexedLinks.Delete(new ulong[] { link2, constants.Any, constants.Any });
            Console.WriteLine("Deleted link2");
            
            var countAfterDelete = indexedLinks.Count(new ulong[] { constants.Any, 1, constants.Any });
            Console.WriteLine($"Links with source 1 after delete: {countAfterDelete}");
            
            Console.WriteLine("Example completed successfully!");
        }
    }
}