using System;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// <para>
    /// Example demonstrating the Association Operator () functionality
    /// as described in issue #383 for explicit association indication.
    /// </para>
    /// <para>
    /// This example shows how to:
    /// 1. Create associations using the () operator syntax
    /// 2. Parse association strings 
    /// 3. Demonstrate structural differences like 1(2(3)) vs 1(2)(3)
    /// 4. Format associations in nested form
    /// </para>
    /// </summary>
    public static class AssociationOperatorExample
    {
        public static void Run()
        {
            Console.WriteLine("=== Association Operator () Example ===");
            Console.WriteLine("Demonstrating explicit association indication as described in issue #383");
            Console.WriteLine();

            // Create a doublets links storage
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);

            Console.WriteLine("1. Basic Association Formatting:");
            Console.WriteLine("--------------------------------");
            
            // Basic formatting examples
            Console.WriteLine($"Format 1 associated with 2: {links.FormatAsAssociation(1u, 2u)}");
            Console.WriteLine($"Format 5 associated with 10: {links.FormatAsAssociation(5u, 10u)}");
            Console.WriteLine();

            Console.WriteLine("2. Creating Associations:");
            Console.WriteLine("-------------------------");
            
            // Create actual link associations
            var assoc1 = links.CreateAssociation(1u, 2u);
            var assoc2 = links.CreateAssociation(3u, 4u);
            
            Console.WriteLine($"Created association 1(2) as link: {assoc1}");
            Console.WriteLine($"Created association 3(4) as link: {assoc2}");
            Console.WriteLine($"Format existing link {assoc1}: {links.FormatAsAssociation(assoc1)}");
            Console.WriteLine();

            Console.WriteLine("3. Nested Associations - Demonstrating 1(2(3)) vs 1(2)(3):");
            Console.WriteLine("-----------------------------------------------------------");
            
            // Create nested structure: 1(2(3))
            var innerAssociation = links.CreateAssociation(2u, 3u);  // 2(3)
            var nestedAssociation = links.CreateAssociation(1u, innerAssociation);  // 1(2(3))
            
            Console.WriteLine($"Inner association 2(3) created as link: {innerAssociation}");
            Console.WriteLine($"Nested association 1(2(3)) created as link: {nestedAssociation}");
            Console.WriteLine($"Nested format: {links.FormatAsNestedAssociation(nestedAssociation)}");
            Console.WriteLine();
            
            // Create flat structure: 1(2) and 1(3) - represents 1(2)(3) conceptually
            var flatAssoc1 = links.CreateAssociation(1u, 2u);  // 1(2)
            var flatAssoc2 = links.CreateAssociation(1u, 3u);  // 1(3)
            
            Console.WriteLine($"Flat association 1(2) as link: {flatAssoc1}");
            Console.WriteLine($"Flat association 1(3) as link: {flatAssoc2}");
            Console.WriteLine($"These represent different structures than the nested 1(2(3))");
            Console.WriteLine();

            Console.WriteLine("4. Parsing Association Strings:");
            Console.WriteLine("-------------------------------");
            
            // Parse association strings as mentioned in the issue
            var parsed1 = links.ParseAssociation("11(1)");
            var parsed2 = links.ParseAssociation("\"1\"(\"1\")");  // Quoted version as in issue
            
            Console.WriteLine($"Parsed '11(1)' as link: {parsed1}");
            Console.WriteLine($"Parsed '\"1\"(\"1\")' as link: {parsed2}");
            
            if (parsed1 != 0u)
            {
                Console.WriteLine($"Formatted back: {links.FormatAsAssociation(parsed1)}");
            }
            Console.WriteLine();

            Console.WriteLine("5. Association Equivalence:");
            Console.WriteLine("---------------------------");
            
            var eq1 = links.CreateAssociation(5u, 6u);
            var eq2 = links.CreateAssociation(5u, 6u);  // Same association
            var eq3 = links.CreateAssociation(6u, 5u);  // Different order
            
            Console.WriteLine($"Association 5(6): {eq1}");
            Console.WriteLine($"Association 5(6) again: {eq2}"); 
            Console.WriteLine($"Association 6(5): {eq3}");
            Console.WriteLine($"Are 5(6) and 5(6) equivalent? {links.AreAssociationsEquivalent(eq1, eq2)}");
            Console.WriteLine($"Are 5(6) and 6(5) equivalent? {links.AreAssociationsEquivalent(eq1, eq3)}");
            Console.WriteLine();

            Console.WriteLine("6. Complex Nested Structure Example:");
            Console.WriteLine("------------------------------------");
            
            // Create a more complex nested structure
            var deepInner = links.CreateAssociation(4u, 5u);      // 4(5)
            var middleLayer = links.CreateAssociation(3u, deepInner); // 3(4(5))
            var outerLayer = links.CreateAssociation(2u, middleLayer); // 2(3(4(5)))
            var rootLayer = links.CreateAssociation(1u, outerLayer);   // 1(2(3(4(5))))
            
            Console.WriteLine($"Deep nested association: {links.FormatAsNestedAssociation(rootLayer)}");
            Console.WriteLine($"This demonstrates how () operator creates distinct nested structures");
            Console.WriteLine();

            Console.WriteLine("7. Using Link.ToAssociationString() method:");
            Console.WriteLine("-------------------------------------------");
            
            var link = new Link<uint>(assoc1, links.GetSource(links.GetLink(assoc1)), links.GetTarget(links.GetLink(assoc1)));
            Console.WriteLine($"Link structure: {link}");
            Console.WriteLine($"As association string: {link.ToAssociationString()}");
            Console.WriteLine();

            Console.WriteLine("=== Summary ===");
            Console.WriteLine("The () operator provides explicit association indication where:");
            Console.WriteLine("• source(target) represents a directed association");
            Console.WriteLine("• 1(2(3)) != 1(2)(3) - nesting structure matters");
            Console.WriteLine("• Supports both parsing from strings and formatting to strings");
            Console.WriteLine("• Integrates seamlessly with existing Links platform architecture");
        }
    }
}

// Usage example:
// AssociationOperatorExample.Run();