using System;
using Platform.Data.Doublets.Memory;

namespace Platform.Data.Doublets.Examples
{
    /// <summary>
    /// Demonstrates the new MetadataAwareSizeType functionality that allows
    /// storing any number of links of any type by containing metadata in size_type.
    /// This addresses issue #356: "Maybe rework header logic to contain metadata in `size_type`"
    /// </summary>
    public class MetadataAwareSizeTypeDemo
    {
        public static void DemonstrateMetadataAwareSizeType()
        {
            Console.WriteLine("=== MetadataAwareSizeType Demo ===");
            Console.WriteLine("This demonstrates how the reworked header logic now supports metadata in size_type");
            Console.WriteLine();

            // Create a MetadataAwareSizeType with count and metadata
            var sizeType = new MetadataAwareSizeType<ulong>(count: 42, metadata: 100);
            
            Console.WriteLine($"Initial size type - Count: {sizeType.Count}, Metadata: {sizeType.GetMetadata()}");
            
            // Demonstrate implicit conversion from ulong
            MetadataAwareSizeType<ulong> fromValue = 1000;
            Console.WriteLine($"From implicit conversion - Count: {fromValue.Count}, Metadata: {fromValue.GetMetadata()}");
            
            // Demonstrate implicit conversion to ulong (for backward compatibility)
            ulong backToValue = fromValue;
            Console.WriteLine($"Back to ulong: {backToValue}");
            
            // Demonstrate metadata setting
            fromValue.SetMetadata(255);
            Console.WriteLine($"After setting metadata - Count: {fromValue.Count}, Metadata: {fromValue.GetMetadata()}");
            
            // Show how this enables flexible link types
            Console.WriteLine();
            Console.WriteLine("=== Flexible Link Types Example ===");
            
            // Example: Different link types can have different metadata
            var standardLink = new MetadataAwareSizeType<ulong>(count: 10, metadata: 0);  // Standard link
            var indexedLink = new MetadataAwareSizeType<ulong>(count: 15, metadata: 1);   // Indexed link  
            var typedLink = new MetadataAwareSizeType<ulong>(count: 8, metadata: 2);      // Typed link
            
            Console.WriteLine($"Standard Link - Count: {standardLink.Count}, Type: {GetLinkType(standardLink.GetMetadata())}");
            Console.WriteLine($"Indexed Link - Count: {indexedLink.Count}, Type: {GetLinkType(indexedLink.GetMetadata())}");
            Console.WriteLine($"Typed Link - Count: {typedLink.Count}, Type: {GetLinkType(typedLink.GetMetadata())}");
            
            Console.WriteLine();
            Console.WriteLine("The signature has changed from 'count -> T' to 'count -> size_type'");
            Console.WriteLine("This allows storing any number of links of any type with embedded metadata!");
        }
        
        private static string GetLinkType(ulong metadata)
        {
            return metadata switch
            {
                0 => "Standard",
                1 => "Indexed",
                2 => "Typed",
                _ => "Custom"
            };
        }
    }
}