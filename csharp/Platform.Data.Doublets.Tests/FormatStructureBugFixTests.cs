using System;
using Platform.Data.Doublets.Memory.United.Generic;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// Tests for the FormatStructure bug fix (issue #242)
    /// </summary>
    public class FormatStructureBugFixTests
    {
        [Fact]
        public void FormatStructure_WithSelfReferencingLink_ShouldNotCauseInfiniteLoop()
        {
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            
            // Create a self-referencing link
            var selfRef = links.Create();
            links.Update(selfRef, selfRef, selfRef);
            
            // This should not cause infinite recursion or stack overflow
            var result = links.FormatStructure(selfRef, _ => false, true, true);
            
            // Should contain the link index and debug markers
            Assert.Contains(selfRef.ToString(), result);
            Assert.Contains("*", result); // Debug marker for visited nodes
        }
        
        [Fact]
        public void FormatStructure_WithCircularReference_ShouldHandleGracefully()
        {
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            
            // Create circular reference between two links
            var linkA = links.Create();
            var linkB = links.Create();
            links.Update(linkA, linkB, linkB);
            links.Update(linkB, linkA, linkA);
            
            // This should not cause infinite recursion
            var result = links.FormatStructure(linkA, _ => false, true, true);
            
            // Should be finite and contain both link indices
            Assert.Contains(linkA.ToString(), result);
            Assert.Contains(linkB.ToString(), result);
            Assert.True(result.Length < 1000, "Result should be finite, not infinitely long");
        }
        
        [Fact]
        public void FormatStructure_WithoutDebugRendering_ShouldStillWork()
        {
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            
            // Create a self-referencing link
            var selfRef = links.Create();
            links.Update(selfRef, selfRef, selfRef);
            
            // This should work even without debug rendering
            var result = links.FormatStructure(selfRef, _ => false, true, false);
            
            // Should contain the link index but no debug markers
            Assert.Contains(selfRef.ToString(), result);
            Assert.DoesNotContain("*", result); // No debug markers when renderDebug is false
        }
        
        [Fact]
        public void FormatStructure_ComplexNestedStructure_ShouldTerminate()
        {
            using var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<ulong>(memory);
            
            // Create a more complex structure similar to what might be created by BalancedVariantConverter
            var link1 = links.Create();
            var link2 = links.Create();
            var link3 = links.Create();
            var root = links.Create();
            
            // Create nested structure
            links.Update(link1, link2, link3);
            links.Update(link2, link3, link1); // Circular reference
            links.Update(root, link1, link2);
            
            // This should terminate and not cause infinite loops
            var result = links.FormatStructure(root, _ => false, true, true);
            
            Assert.True(result.Length > 0, "Should produce some output");
            Assert.True(result.Length < 10000, "Should not be infinitely long");
        }
    }
}