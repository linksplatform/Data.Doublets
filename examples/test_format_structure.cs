using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

namespace TestFormatStructure
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a simple memory-based links storage
            var links = new UnitedMemoryLinks<ulong>();
            
            try
            {
                // Create some test links
                var link1 = links.CreatePoint();
                var link2 = links.CreatePoint();
                var link3 = links.Create(link1, link2);
                
                Console.WriteLine("Testing FormatStructure with optional isElement parameter:");
                
                // Test 1: Call FormatStructure without isElement parameter (should use default)
                var result1 = links.FormatStructure(link3);
                Console.WriteLine($"FormatStructure without isElement: {result1}");
                
                // Test 2: Call FormatStructure with isElement parameter (original behavior)
                var result2 = links.FormatStructure(link3, link => false);
                Console.WriteLine($"FormatStructure with isElement: {result2}");
                
                // Test 3: Call FormatStructure with both optional parameters
                var result3 = links.FormatStructure(link3, renderIndex: true);
                Console.WriteLine($"FormatStructure with renderIndex: {result3}");
                
                Console.WriteLine("All tests passed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}