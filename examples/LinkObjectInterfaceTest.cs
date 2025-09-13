using System;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// <para>
    /// Example demonstrating the object-like interface for Link struct.
    /// </para>
    /// <para></para>
    /// </summary>
    public class Program
    {
        public static void Main()
        {
            Console.WriteLine("Testing Link struct object-like interface...");
            
            // Create a memory-based links storage
            var memory = new HeapResizableDirectMemory();
            var memoryManager = new UnitedMemoryLinks<ulong>(memory);
            
            // Test 1: Create a link with traditional approach
            Console.WriteLine("\n1. Traditional approach:");
            var linkAddress = memoryManager.Create();
            var traditionalLink = new Link<ulong>(memoryManager.GetLink(linkAddress));
            Console.WriteLine($"Created link: {traditionalLink}");
            Console.WriteLine($"Has links container: {traditionalLink.HasLinksContainer()}");
            
            // Test 2: Create a link with object-like interface
            Console.WriteLine("\n2. Object-like interface approach:");
            var objectLink = new Link<ulong>(memoryManager, linkAddress, linkAddress, linkAddress);
            Console.WriteLine($"Created object link: {objectLink}");
            Console.WriteLine($"Has links container: {objectLink.HasLinksContainer()}");
            Console.WriteLine($"Links container available: {objectLink.Links != null}");
            
            // Test 3: Update using object-like interface
            Console.WriteLine("\n3. Update using object-like interface:");
            var linkAddress2 = memoryManager.Create();
            var linkAddress3 = memoryManager.Create();
            
            var updatedAddress = objectLink.Update(linkAddress2, linkAddress3);
            Console.WriteLine($"Updated link address: {updatedAddress}");
            
            // Refresh to get updated values
            var refreshedLink = objectLink.Refresh();
            Console.WriteLine($"Refreshed link: {refreshedLink}");
            
            // Test 4: Test with link created from links container and link data
            Console.WriteLine("\n4. Create link using container and link data:");
            var linkData = new List<ulong> { linkAddress3, linkAddress2, linkAddress };
            var dataLink = new Link<ulong>(memoryManager, linkData);
            Console.WriteLine($"Data link: {dataLink}");
            Console.WriteLine($"Has links container: {dataLink.HasLinksContainer()}");
            
            // Test 5: Test error handling when no links container is available
            Console.WriteLine("\n5. Error handling test:");
            var standaloneLink = new Link<ulong>(1, 2, 3);
            Console.WriteLine($"Standalone link: {standaloneLink}");
            Console.WriteLine($"Has links container: {standaloneLink.HasLinksContainer()}");
            
            try
            {
                standaloneLink.Update(4, 5);
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Expected error: {ex.Message}");
            }
            
            Console.WriteLine("\nAll tests completed successfully!");
            
            // Cleanup
            memoryManager.Dispose();
        }
    }
}