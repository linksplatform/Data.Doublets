using System;
using System.Collections;
using Platform.Data.Doublets;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Example demonstrating bitstring indexing functionality.
    /// </summary>
    public static class BitStringIndexingExample
    {
        /// <summary>
        /// Demonstrates basic bitstring index operations.
        /// </summary>
        public static void BasicBitStringIndexOperations()
        {
            Console.WriteLine("=== BitString Index Basic Operations ===");
            
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 10);

                var bitStringMethods = new LinksSourcesBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                // Set up some data relationships
                const ulong sourceLink = 1ul;
                const ulong targetLink = 2ul;
                const ulong relationshipLink = 3ul;

                Console.WriteLine("Setting up bitstring relationships...");
                
                // Mark that sourceLink has relationship at position 2 (relationshipLink - 1)
                bitStringMethods.SetBit(sourceLink, (int)relationshipLink - 1, true);
                bitStringMethods.SetBit(sourceLink, 5, true);
                bitStringMethods.SetBit(sourceLink, 10, true);
                
                // Mark that targetLink has relationship at position 2 (relationshipLink - 1)  
                bitStringMethods.SetBit(targetLink, (int)relationshipLink - 1, true);
                bitStringMethods.SetBit(targetLink, 7, true);

                Console.WriteLine($"Source link has {bitStringMethods.CountSetBits(sourceLink)} relationships");
                Console.WriteLine($"Target link has {bitStringMethods.CountSetBits(targetLink)} relationships");

                // Demonstrate bitwise operations
                Console.WriteLine("\nPerforming bitwise operations...");
                
                var andResult = bitStringMethods.BitwiseAnd(sourceLink, targetLink);
                Console.WriteLine("Bitwise AND result (common relationships):");
                for (int i = 0; i < Math.Min(15, andResult.Length); i++)
                {
                    if (andResult[i])
                    {
                        Console.WriteLine($"  Position {i}: true (relationship {i + 1})");
                    }
                }

                var orResult = bitStringMethods.BitwiseOr(sourceLink, targetLink);
                Console.WriteLine("Bitwise OR result (all relationships):");
                for (int i = 0; i < Math.Min(15, orResult.Length); i++)
                {
                    if (orResult[i])
                    {
                        Console.WriteLine($"  Position {i}: true (relationship {i + 1})");
                    }
                }

                // Search for common relationships
                var searchResult = bitStringMethods.Search(sourceLink, targetLink);
                Console.WriteLine($"\nSearch result: {searchResult} (first common relationship)");

                memory.Free();
            }
            
            Console.WriteLine("=== Example Complete ===\n");
        }

        /// <summary>
        /// Demonstrates attach/detach operations.
        /// </summary>
        public static void AttachDetachOperations()
        {
            Console.WriteLine("=== BitString Attach/Detach Operations ===");
            
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 10);

                var bitStringMethods = new LinksTargetsBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                const ulong rootLink = 1ul;
                const ulong childLink1 = 2ul;
                const ulong childLink2 = 3ul;
                const ulong childLink3 = 4ul;

                Console.WriteLine("Attaching child links to root...");
                
                // Attach children to root
                var root = rootLink;
                bitStringMethods.Attach(ref root, childLink1);
                bitStringMethods.Attach(ref root, childLink2);
                bitStringMethods.Attach(ref root, childLink3);

                Console.WriteLine($"Root link {rootLink} now has {bitStringMethods.CountUsages(rootLink)} usages");

                // List all usages
                Console.WriteLine("Enumerating all usages:");
                bitStringMethods.EachUsage(rootLink, link =>
                {
                    Console.WriteLine($"  Usage: {link[0]}");
                    return constants.Continue;
                });

                // Detach one child
                Console.WriteLine($"\nDetaching child link {childLink2}...");
                bitStringMethods.Detach(ref root, childLink2);
                
                Console.WriteLine($"Root link {rootLink} now has {bitStringMethods.CountUsages(rootLink)} usages");

                Console.WriteLine("Remaining usages:");
                bitStringMethods.EachUsage(rootLink, link =>
                {
                    Console.WriteLine($"  Usage: {link[0]}");
                    return constants.Continue;
                });

                memory.Free();
            }
            
            Console.WriteLine("=== Example Complete ===\n");
        }

        /// <summary>
        /// Runs all examples.
        /// </summary>
        public static void RunAll()
        {
            Console.WriteLine("BitString Indexing Examples\n");
            
            BasicBitStringIndexOperations();
            AttachDetachOperations();
            
            Console.WriteLine("All examples completed successfully!");
        }
    }
}