using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Experiments
{
    class Program
    {
        static void Main(string[] args)
        {
            using var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            
            Console.WriteLine("=== WITHOUT DEDUPLICATION ===");
            var character = links.CreatePoint();
            Console.WriteLine($"Character: {character}");
            
            var seq1 = links.CreateAndUpdate(character, character);
            var seq2 = links.CreateAndUpdate(character, character);
            
            Console.WriteLine($"Sequence 1: {seq1}");
            Console.WriteLine($"Sequence 2: {seq2}");
            Console.WriteLine($"Are they equal? {seq1 == seq2}");
            Console.WriteLine($"Links count: {links.Count()}");
            
            Console.WriteLine("\n=== WITH BASIC DEDUPLICATION ===");
            var linksWithDedup = new LinksUniquenessResolver<uint>(links);
            
            var dedupSeq1 = linksWithDedup.CreateAndUpdate(character, character);
            var dedupSeq2 = linksWithDedup.CreateAndUpdate(character, character);
            
            Console.WriteLine($"Dedup Sequence 1: {dedupSeq1}");
            Console.WriteLine($"Dedup Sequence 2: {dedupSeq2}");
            Console.WriteLine($"Are they equal? {dedupSeq1 == dedupSeq2}");
            Console.WriteLine($"Links count: {links.Count()}");
            
            // Test the advanced deduplication
            Console.WriteLine("\n=== WITH ADVANCED DEDUPLICATION ===");
            using var memory2 = new HeapResizableDirectMemory();
            var links2 = new UnitedMemoryLinks<uint>(memory2);
            var linksAdvanced = links2.DecorateWithAutomaticUniquenessAndUsagesResolution();
            
            var character2 = linksAdvanced.CreatePoint();
            var advSeq1 = linksAdvanced.CreateAndUpdate(character2, character2);
            var advSeq2 = linksAdvanced.CreateAndUpdate(character2, character2);
            
            Console.WriteLine($"Advanced Sequence 1: {advSeq1}");
            Console.WriteLine($"Advanced Sequence 2: {advSeq2}");
            Console.WriteLine($"Are they equal? {advSeq1 == advSeq2}");
            Console.WriteLine($"Links count: {links2.Count()}");
        }
    }
}