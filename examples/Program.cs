using System;
using Platform.Data.Doublets.Examples;

namespace Platform.Data.Doublets.Examples
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Bitstring Deleted Links Map Performance Test");
            Console.WriteLine("Issue #398: Try to use bitstring for a map of deleted links to test if it is faster");
            Console.WriteLine();

            try
            {
                DeletedLinksTrackingBenchmark.RunComparison();
                
                Console.WriteLine();
                Console.WriteLine("Test completed successfully!");
                Console.WriteLine();
                Console.WriteLine("Recommendations:");
                Console.WriteLine("- Custom Bitstring approach should show the best performance for most operations");
                Console.WriteLine("- BitArray is good for general use but has more overhead");
                Console.WriteLine("- Linked List approach (current) has overhead of HashSet + LinkedList");
                Console.WriteLine("- Bitstring approaches use significantly less memory for sparse deletion patterns");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error running benchmark: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}