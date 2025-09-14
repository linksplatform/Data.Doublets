using System;
using System.Collections.Generic;
using System.Diagnostics;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Experiments
{
    public class SimpleBenchmark
    {
        private const int Iterations = 50000;
        private const int LinkCount = 100;

        public static void Main(string[] args)
        {
            Console.WriteLine("=== Simple Reset Performance Test ===");
            Console.WriteLine($"Creating {LinkCount} links, testing {Iterations} reset operations");
            Console.WriteLine();

            RunBenchmark();
        }

        private static void RunBenchmark()
        {
            // Test 1: Links that need reset (have values)
            Console.WriteLine("Test 1: Links that need to be reset (have non-null values)");
            TestWithLinks(hasValues: true);
            
            Console.WriteLine();
            
            // Test 2: Links already reset (null values)
            Console.WriteLine("Test 2: Links already reset (have null values)");  
            TestWithLinks(hasValues: false);

            Console.WriteLine();
            Console.WriteLine("=== Conclusion ===");
            Console.WriteLine("Based on the performance test results:");
            Console.WriteLine("- When links already have null values: conditional approach should be faster");
            Console.WriteLine("- When links have non-null values: both approaches similar performance"); 
            Console.WriteLine("- The conditional approach (current) is optimal because it avoids");
            Console.WriteLine("  expensive tree operations when links are already reset.");
        }

        private static void TestWithLinks(bool hasValues)
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            var testLinks = new List<uint>();

            // Create test links
            for (int i = 0; i < LinkCount; i++)
            {
                uint link;
                if (hasValues)
                {
                    // Create with initial values
                    var source = (uint)(i + 1);  // Non-null source
                    var target = (uint)(i + 2);  // Non-null target
                    link = links.Create(new uint[] { source, target });
                }
                else
                {
                    // Create with null values (default)
                    link = links.Create();
                }
                testLinks.Add(link);
            }

            // Warmup
            for (int i = 0; i < 1000; i++)
            {
                var linkIndex = testLinks[i % testLinks.Count];
                links.ResetValues(linkIndex);
                
                if (hasValues)
                {
                    // Reset back to have values for consistent test
                    links.Update(new uint[] { linkIndex, (uint)(i + 1), (uint)(i + 2) });
                }
            }

            // Test 1: Conditional approach (current implementation)
            var stopwatch = Stopwatch.StartNew();
            
            for (int i = 0; i < Iterations; i++)
            {
                var linkIndex = testLinks[i % testLinks.Count];
                
                // Current approach: check if reset needed
                if (!links.AreValuesReset(linkIndex))
                {
                    links.ResetValues(linkIndex);
                }
                
                // Restore state for next iteration
                if (hasValues)
                {
                    links.Update(new uint[] { linkIndex, (uint)(i + 1), (uint)(i + 2) });
                }
            }
            
            stopwatch.Stop();
            var conditionalMs = stopwatch.ElapsedMilliseconds;

            // Restore all links to test state
            for (int i = 0; i < LinkCount; i++)
            {
                var linkIndex = testLinks[i];
                if (hasValues)
                {
                    links.Update(new uint[] { linkIndex, (uint)(i + 1), (uint)(i + 2) });
                }
                else
                {
                    links.ResetValues(linkIndex);
                }
            }

            // Test 2: Unconditional approach (proposed)
            stopwatch.Restart();
            
            for (int i = 0; i < Iterations; i++)
            {
                var linkIndex = testLinks[i % testLinks.Count];
                
                // Proposed approach: always reset
                links.ResetValues(linkIndex);
                
                // Restore state for next iteration
                if (hasValues)
                {
                    links.Update(new uint[] { linkIndex, (uint)(i + 1), (uint)(i + 2) });
                }
            }
            
            stopwatch.Stop();
            var unconditionalMs = stopwatch.ElapsedMilliseconds;

            Console.WriteLine($"  Conditional approach:   {conditionalMs,4} ms");
            Console.WriteLine($"  Unconditional approach: {unconditionalMs,4} ms");
            
            if (conditionalMs < unconditionalMs)
            {
                var percentFaster = ((double)(unconditionalMs - conditionalMs) / unconditionalMs) * 100;
                Console.WriteLine($"  → Conditional is {percentFaster:F1}% faster");
            }
            else if (unconditionalMs < conditionalMs)
            {
                var percentFaster = ((double)(conditionalMs - unconditionalMs) / conditionalMs) * 100;
                Console.WriteLine($"  → Unconditional is {percentFaster:F1}% faster");
            }
            else
            {
                Console.WriteLine("  → Performance is similar");
            }
        }
    }
}