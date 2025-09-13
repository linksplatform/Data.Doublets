using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Testing FormatStructure fix for issue #242...");
        
        using var memory = new HeapResizableDirectMemory(UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
        using var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep);
        
        Console.WriteLine("Creating self-referencing link...");
        var selfRef = links.CreatePoint();
        links.Update(selfRef, selfRef, selfRef);
        
        Console.WriteLine($"Self-referencing link created: {selfRef}");
        
        Console.WriteLine("Testing FormatStructure with renderDebug=true...");
        try
        {
            var result = links.FormatStructure(selfRef, _ => false, true, true);
            Console.WriteLine($"Result: {result}");
            Console.WriteLine($"Result length: {result.Length}");
            
            if (result.Length < 1000)
            {
                Console.WriteLine("✓ SUCCESS: FormatStructure completed without infinite loop");
            }
            else
            {
                Console.WriteLine("❌ FAIL: Result too long, might indicate infinite loop");
            }
        }
        catch (StackOverflowException)
        {
            Console.WriteLine("❌ FAIL: StackOverflowException - infinite recursion detected");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAIL: Exception occurred: {ex.Message}");
        }
        
        Console.WriteLine("\nTesting circular reference...");
        var linkA = links.CreatePoint();
        var linkB = links.CreatePoint();
        links.Update(linkA, linkB, linkB);
        links.Update(linkB, linkA, linkA);
        
        try
        {
            var result2 = links.FormatStructure(linkA, _ => false, true, true);
            Console.WriteLine($"Circular reference result: {result2}");
            Console.WriteLine($"Circular result length: {result2.Length}");
            
            if (result2.Length < 1000)
            {
                Console.WriteLine("✓ SUCCESS: Circular reference handled correctly");
            }
            else
            {
                Console.WriteLine("❌ FAIL: Circular reference result too long");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ FAIL: Exception with circular reference: {ex.Message}");
        }
        
        Console.WriteLine("\nAll tests completed.");
    }
}