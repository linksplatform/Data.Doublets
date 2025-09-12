using System;
using System.Collections.Generic;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Decorators;
using Platform.Memory;

// Simple test to verify DeleteAll can be overridden
public class TestMemoryLinks : LinksDecoratorBase<ulong>
{
    public bool CustomDeleteAllCalled { get; private set; } = false;
    
    public TestMemoryLinks(ILinks<ulong> links) : base(links) { }
    
    public override void DeleteAll()
    {
        CustomDeleteAllCalled = true;
        Console.WriteLine("Custom DeleteAll implementation called!");
        
        // Call the default implementation for demonstration
        base.DeleteAll();
    }
}

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing DeleteAll override functionality...");
        
        // Create a basic memory links implementation
        var memory = new HeapResizableDirectMemory();
        var baseLinks = new UnitedMemoryLinks<ulong>(memory);
        
        // Wrap it with our test decorator
        var testLinks = new TestMemoryLinks(baseLinks);
        
        // Create some test links
        testLinks.Create(1, 2);
        testLinks.Create(2, 3);
        testLinks.Create(3, 4);
        
        Console.WriteLine($"Created {testLinks.Count(null)} links");
        
        // Call DeleteAll - should use our overridden method
        testLinks.DeleteAll();
        
        Console.WriteLine($"After DeleteAll: {testLinks.Count(null)} links remain");
        Console.WriteLine($"Custom implementation called: {testLinks.CustomDeleteAllCalled}");
        
        if (testLinks.CustomDeleteAllCalled)
        {
            Console.WriteLine("SUCCESS: DeleteAll can be overridden!");
        }
        else
        {
            Console.WriteLine("FAILURE: DeleteAll override was not called");
        }
    }
}