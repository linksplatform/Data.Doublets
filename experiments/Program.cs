using System;
using System.IO;
using Platform.Data.Doublets.Memory.Split.Generic;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing SplitMemoryLinks string constructor...");
        
        // Create temporary file paths for testing
        string dataPath = Path.GetTempFileName();
        string indexPath = Path.GetTempFileName();
        
        try
        {
            // Test using string constructor (new implementation)
            using (var links = new SplitMemoryLinks<uint>(dataPath, indexPath))
            {
                Console.WriteLine("✓ String constructor works correctly!");
                Console.WriteLine($"  Data file: {dataPath}");
                Console.WriteLine($"  Index file: {indexPath}");
                
                // Verify we can perform basic operations
                var linkId = links.Create(new uint[] { links.Constants.Any, links.Constants.Any }, null);
                Console.WriteLine($"  Created link with ID: {linkId}");
                
                var count = links.Count(new uint[] { links.Constants.Any, links.Constants.Any, links.Constants.Any });
                Console.WriteLine($"  Total links count: {count}");
            }
            
            Console.WriteLine("✓ All tests passed! String constructor implementation is working correctly.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Test failed: {ex.Message}");
        }
        finally
        {
            // Cleanup
            if (File.Exists(dataPath)) File.Delete(dataPath);
            if (File.Exists(indexPath)) File.Delete(indexPath);
        }
    }
}