using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;

/// <summary>
/// Demonstrates the new tree-like structure for links instead of IList<TLinkAddress>.
/// This shows how the tree structure provides a more hierarchical way to work with links.
/// </summary>
class TreeStructureDemo
{
    static void Main()
    {
        Console.WriteLine("=== Tree Structure Demo ===");

        // Create a simple memory-based links storage
        using var links = new UnitedMemoryLinks<ulong>(new UnitedMemoryLinksOptions<ulong>());

        Console.WriteLine("1. Traditional IList<TLinkAddress> approach:");
        
        // Create some links using the traditional array/list approach
        var link1 = links.Create();
        var link2 = links.Create();
        var link3 = links.GetOrCreate(link1, link2);
        
        Console.WriteLine($"   Created links: {link1}, {link2}, {link3}");
        
        // Get link as traditional list format
        var link3AsList = links.GetLink(link3);
        Console.WriteLine($"   Link 3 as list: [{string.Join(", ", link3AsList)}]");

        Console.WriteLine("\n2. New Tree-like structure approach:");
        
        // Convert to tree structure using new extension method
        var link3AsTree = links.AsTree(link3AsList);
        Console.WriteLine($"   Link 3 as tree:");
        Console.WriteLine($"     Index: {links.GetTreeIndex(link3AsTree)}");
        Console.WriteLine($"     Source: {links.GetTreeSource(link3AsTree)}");  
        Console.WriteLine($"     Target: {links.GetTreeTarget(link3AsTree)}");
        Console.WriteLine($"     Parent: {link3AsTree?.Parent}");
        Console.WriteLine($"     Children Count: {link3AsTree?.ChildrenCount}");
        
        // Demonstrate tree-specific functionality
        Console.WriteLine($"     Is Null: {link3AsTree?.IsNull()}");
        Console.WriteLine($"     ToString: {link3AsTree?.ToString()}");
        
        // Create a Link struct that implements both IList and ILinkTree
        var linkStruct = new Link<ulong>(link3, link1, link2);
        Console.WriteLine($"\n3. Link struct with tree interface:");
        Console.WriteLine($"   As ILinkTree - Index: {((ILinkTree<ulong>)linkStruct).Index}");
        Console.WriteLine($"   As ILinkTree - Source: {((ILinkTree<ulong>)linkStruct).Source}");
        Console.WriteLine($"   As ILinkTree - Target: {((ILinkTree<ulong>)linkStruct).Target}");
        
        // Show backward compatibility
        Console.WriteLine($"\n4. Backward compatibility:");
        var linkAsArray = linkStruct.ToArray();
        Console.WriteLine($"   Link as array: [{string.Join(", ", linkAsArray)}]");
        
        // Demonstrate format extension
        Console.WriteLine($"   Formatted tree: {links.FormatTree(link3AsTree)}");

        Console.WriteLine("\n=== Demo Complete ===");
    }
}