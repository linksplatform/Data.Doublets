using System;
using System.IO;
using Platform.Data.Doublets.Sequences.Frequencies.Cache;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Example demonstrating the usage of LinkFrequenciesCache with serialization/deserialization
    /// and dumping to Links storage functionality.
    /// </summary>
    public class LinkFrequenciesCacheExample
    {
        public static void RunExample()
        {
            Console.WriteLine("=== LinkFrequenciesCache Example ===\n");

            // Mock implementation for demonstration purposes
            var mockLinks = new MockLinks();
            var mockCounter = new DefaultCounter<ulong, ulong>();

            // Create the cache
            var cache = new LinkFrequenciesCache<ulong>(mockLinks, mockCounter);

            Console.WriteLine("1. Building frequency cache from sequences...");
            
            // Simulate processing some sequences
            var sequence1 = new ulong[] { 1, 2, 3, 4 };
            var sequence2 = new ulong[] { 2, 3, 5 };
            var sequence3 = new ulong[] { 1, 2, 6, 7 };

            cache.IncrementFrequencies(sequence1);
            cache.IncrementFrequencies(sequence2);
            cache.IncrementFrequencies(sequence3);

            Console.WriteLine($"Cache contains {cache.Count} frequency entries.");
            
            // Print some frequencies
            Console.WriteLine("\n2. Current frequencies:");
            cache.PrintFrequency(1, 2);  // Should appear twice
            cache.PrintFrequency(2, 3);  // Should appear twice  
            cache.PrintFrequency(3, 4);  // Should appear once
            cache.PrintFrequency(5, 6);  // Should not exist

            Console.WriteLine("\n3. Serializing cache to JSON...");
            var json = cache.SerializeToJson();
            Console.WriteLine($"JSON length: {json.Length} characters");

            Console.WriteLine("\n4. Saving to file...");
            var filePath = Path.Combine(Path.GetTempPath(), "frequencies_cache.json");
            cache.SerializeToFile(filePath);
            Console.WriteLine($"Saved to: {filePath}");

            Console.WriteLine("\n5. Clearing cache and deserializing...");
            cache.Clear();
            Console.WriteLine($"Cache cleared, count: {cache.Count}");
            
            cache.DeserializeFromFile(filePath);
            Console.WriteLine($"Cache restored, count: {cache.Count}");

            Console.WriteLine("\n6. Verifying restored frequencies:");
            cache.PrintFrequency(1, 2);
            cache.PrintFrequency(2, 3);

            Console.WriteLine("\n7. Dumping cache to Links storage...");
            var createdLinks = cache.DumpToLinksStorage();
            Console.WriteLine($"Created {createdLinks} links in storage");

            Console.WriteLine("\n8. All entries in cache:");
            foreach (var entry in cache.GetAllEntries())
            {
                var doublet = entry.Key;
                var freq = entry.Value;
                Console.WriteLine($"  ({doublet.Source},{doublet.Target}) -> Frequency: {freq.Frequency}, Link: {freq.Link}");
            }

            // Cleanup
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Console.WriteLine($"\nCleaned up temporary file: {filePath}");
            }

            Console.WriteLine("\n=== Example Complete ===");
        }

        /// <summary>
        /// Simple mock implementation of ILinks for demonstration
        /// </summary>
        private class MockLinks : ILinks<ulong>
        {
            private ulong _nextId = 100;

            public LinksConstants<ulong> Constants { get; } = new LinksConstants<ulong>(true, 1, 2, 3);

            public ulong Count(IList<ulong> restriction)
            {
                return 0;
            }

            public ulong Each(Func<IList<ulong>, ulong> handler, IList<ulong> restriction)
            {
                return Constants.Continue;
            }

            public ulong Update(IList<ulong> restriction, IList<ulong> substitution, WriteHandler<ulong> handler)
            {
                return _nextId++;
            }

            public ulong SearchOrDefault(ulong source, ulong target)
            {
                // Return 0 to indicate link doesn't exist yet
                return 0;
            }

            public ulong CreateAndUpdate(ulong source, ulong target)
            {
                return _nextId++;
            }
        }
    }
}