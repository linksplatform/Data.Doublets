using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Experiments
{
    /// <summary>
    /// Experiment to test the improved disposal error messaging for GitHub issue #174
    /// </summary>
    public static class DisposalTest
    {
        public static void TestImprovedErrorMessagesAfterDisposal()
        {
            var links = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory());
            
            // Use the links normally
            var link = links.Create();
            Console.WriteLine($"Created link: {link}");
            
            // Dispose the links
            links.Dispose();
            Console.WriteLine("Links have been disposed.");
            
            // Test improved error messages for various operations
            TestOperation("Count()", () => links.Count());
            TestOperation("Create()", () => links.Create());
            TestOperation("Delete(link)", () => links.Delete(link));
            TestOperation("Each() with callback", () => links.Each(handler: link => link.Index));
            TestOperation("Update()", () => links.Update(new[] { link }, new[] { link }));
        }
        
        private static void TestOperation(string operationName, Action operation)
        {
            try
            {
                operation();
                Console.WriteLine($"ERROR: {operationName} should have thrown ObjectDisposedException!");
            }
            catch (ObjectDisposedException ex)
            {
                Console.WriteLine($"✓ {operationName}: Correctly threw ObjectDisposedException");
                Console.WriteLine($"  Object: {ex.ObjectName}");
                Console.WriteLine($"  Message: {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ {operationName}: Threw unexpected exception - {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}