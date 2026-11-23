using System;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Experiments
{
    class TestPrintContents
    {
        static void Main()
        {
            Console.WriteLine("Testing PrintContents extension method...");
            
            // Create a simple in-memory links storage
            using var links = new UnitedMemoryLinks<ulong>("test.db");
            
            var messages = new List<string>();
            void CaptureMessage(string message) => messages.Add(message);
            
            // Test with empty database
            Console.WriteLine("1. Testing with empty database:");
            links.PrintContents(CaptureMessage);
            foreach (var message in messages)
            {
                Console.WriteLine($"Output: {message}");
            }
            Console.WriteLine();
            
            // Clear messages and add some links
            messages.Clear();
            
            // Create some test links
            var link1 = links.Create();
            var link2 = links.Create();
            var link3 = links.CreateAndUpdate(link1, link2);
            
            Console.WriteLine("2. Testing with some links:");
            Console.WriteLine($"Created links: {link1}, {link2}, {link3}");
            
            links.PrintContents(CaptureMessage);
            foreach (var message in messages)
            {
                Console.WriteLine($"Output: {message}");
            }
            
            Console.WriteLine("\nTest completed successfully!");
        }
    }
}