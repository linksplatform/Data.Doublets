using System;
using System.IO;
using Platform.Data.Doublets;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Example demonstrating shared access to a links database from multiple processes.
    /// </summary>
    public class SharedAccessExample
    {
        public static void Main(string[] args)
        {
            var databasePath = Path.Combine(Path.GetTempPath(), "shared_links_example.db");
            
            Console.WriteLine($"Shared Access Links Database Example");
            Console.WriteLine($"Database path: {databasePath}");
            Console.WriteLine();

            try
            {
                // Example 1: Basic usage
                Console.WriteLine("Example 1: Basic shared access usage");
                BasicUsageExample(databasePath);
                Console.WriteLine();

                // Example 2: Multiple instances accessing same database
                Console.WriteLine("Example 2: Multiple instances accessing same database");
                MultipleInstancesExample(databasePath);
                Console.WriteLine();

                Console.WriteLine("All examples completed successfully!");
                Console.WriteLine("You can now run multiple instances of this program simultaneously");
                Console.WriteLine("to see shared access in action.");
            }
            finally
            {
                // Clean up
                if (File.Exists(databasePath))
                {
                    File.Delete(databasePath);
                    Console.WriteLine($"Cleaned up database file: {databasePath}");
                }
            }
        }

        private static void BasicUsageExample(string databasePath)
        {
            using (var links = new SharedAccessLinks<ulong>(databasePath))
            {
                Console.WriteLine("Creating some links...");
                
                // Create some links
                var link1 = links.Create();
                var link2 = links.Create();
                var link3 = links.Create(link1, link2);
                
                Console.WriteLine($"Created links: {link1}, {link2}, {link3}");
                Console.WriteLine($"Total links in database: {links.Count()}");
                
                // Update a link
                var updatedLink = links.Update(link3, link2, link1);
                Console.WriteLine($"Updated link {link3} to point from {link2} to {link1}");
                
                // Search for links
                var foundLinks = 0;
                links.Each(linkAddress =>
                {
                    foundLinks++;
                    return links.Constants.Continue;
                });
                
                Console.WriteLine($"Found {foundLinks} links by iterating through all links");
            }
        }

        private static void MultipleInstancesExample(string databasePath)
        {
            // First instance
            using (var links1 = new SharedAccessLinks<ulong>(databasePath))
            {
                Console.WriteLine("Instance 1: Creating initial links...");
                var link1 = links1.Create();
                var link2 = links1.Create();
                
                Console.WriteLine($"Instance 1 created links: {link1}, {link2}");
                Console.WriteLine($"Instance 1 sees {links1.Count()} total links");
                
                // Second instance accessing the same database
                using (var links2 = new SharedAccessLinks<ulong>(databasePath))
                {
                    Console.WriteLine("Instance 2: Accessing same database...");
                    Console.WriteLine($"Instance 2 sees {links2.Count()} total links");
                    
                    // Create a link from second instance
                    var link3 = links2.Create(link1, link2);
                    Console.WriteLine($"Instance 2 created link: {link3}");
                    
                    // Both instances should see the same total count
                    Console.WriteLine($"Instance 1 now sees {links1.Count()} total links");
                    Console.WriteLine($"Instance 2 now sees {links2.Count()} total links");
                }
                
                Console.WriteLine($"After Instance 2 closed, Instance 1 still sees {links1.Count()} total links");
            }
        }
    }
}