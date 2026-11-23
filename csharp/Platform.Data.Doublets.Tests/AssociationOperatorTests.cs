using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// Tests for the AssociationOperator class that implements the () operator 
    /// for explicit association indication as described in issue #383.
    /// </summary>
    public static class AssociationOperatorTests
    {
        [Fact]
        public static void BasicAssociationFormatTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var association = links.Association();
            var formatted = association.Format(1u, 2u);
            
            Assert.Equal("1(2)", formatted);
        }

        [Fact]
        public static void AssociationExtensionFormatTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var formatted = links.FormatAsAssociation(1u, 2u);
            
            Assert.Equal("1(2)", formatted);
        }

        [Fact]
        public static void CreateAssociationTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var associationLink = links.CreateAssociation(1u, 2u);
            
            Assert.True(links.Exists(associationLink));
            Assert.Equal(1u, links.GetSource(associationLink));
            Assert.Equal(2u, links.GetTarget(associationLink));
        }

        [Fact]
        public static void NestedAssociationFormatTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            // Create links: 1, 2, 3
            var link1 = links.CreatePoint();
            var link2 = links.CreatePoint(); 
            var link3 = links.CreatePoint();
            
            // Create 2(3)
            var inner = links.CreateAssociation(link2, link3);
            // Create 1(2(3))
            var outer = links.CreateAssociation(link1, inner);
            
            var formatted = links.FormatAsNestedAssociation(outer, 5);
            
            // Should show nested structure like 1(2(3))
            Assert.Contains($"{link1}", formatted);
            Assert.Contains($"{link2}", formatted);
            Assert.Contains($"{link3}", formatted);
            Assert.Contains("(", formatted);
        }

        [Fact]
        public static void AssociationStructuralDifferenceTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var link1 = links.CreatePoint();
            var link2 = links.CreatePoint();
            var link3 = links.CreatePoint();
            
            // Create 1(2(3))
            var inner1 = links.CreateAssociation(link2, link3);
            var nested = links.CreateAssociation(link1, inner1);
            
            // Create 1(2) and 1(3) separately - this represents 1(2)(3) structure
            var flat1 = links.CreateAssociation(link1, link2);
            var flat2 = links.CreateAssociation(link1, link3);
            
            // These should be different structures
            Assert.NotEqual(nested, flat1);
            Assert.NotEqual(nested, flat2);
            Assert.False(links.AreAssociationsEquivalent(nested, flat1));
            Assert.False(links.AreAssociationsEquivalent(nested, flat2));
        }

        [Fact]
        public static void ParseSimpleAssociationTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var parsed = links.ParseAssociation("1(2)");
            
            Assert.NotEqual(0u, parsed);
            Assert.True(links.Exists(parsed));
            Assert.Equal(1u, links.GetSource(parsed));
            Assert.Equal(2u, links.GetTarget(parsed));
        }

        [Fact]
        public static void ParseFailsOnInvalidSyntaxTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var parsed1 = links.ParseAssociation("invalid");
            var parsed2 = links.ParseAssociation("1(");
            var parsed3 = links.ParseAssociation(")2");
            var parsed4 = links.ParseAssociation("");
            
            Assert.Equal(0u, parsed1);
            Assert.Equal(0u, parsed2);
            Assert.Equal(0u, parsed3);
            Assert.Equal(0u, parsed4);
        }

        [Fact]
        public static void AssociationEquivalenceTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var assoc1 = links.CreateAssociation(1u, 2u);
            var assoc2 = links.CreateAssociation(1u, 2u); // Should be the same due to GetOrCreate
            var assoc3 = links.CreateAssociation(2u, 1u); // Different - order matters
            
            Assert.True(links.AreAssociationsEquivalent(assoc1, assoc2));
            Assert.False(links.AreAssociationsEquivalent(assoc1, assoc3));
        }

        [Fact]
        public static void FormatExistingLinkTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var source = links.CreatePoint();
            var target = links.CreatePoint(); 
            var association = links.CreateAndUpdate(source, target);
            
            var formatted = links.FormatAsAssociation(association);
            
            Assert.Equal($"{source}({target})", formatted);
        }

        [Fact]
        public static void QuotedStringParsingTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            // Test with quoted strings as mentioned in the issue: "11" = "1"("1")
            var parsed = links.ParseAssociation("\"1\"(\"1\")");
            
            Assert.NotEqual(0u, parsed);
            Assert.True(links.Exists(parsed));
            Assert.Equal(1u, links.GetSource(parsed));
            Assert.Equal(1u, links.GetTarget(parsed));
        }

        [Fact]
        public static void MaxDepthPreventionTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            // Create a self-referencing structure that could cause infinite recursion
            var selfRef = links.CreatePoint();
            links.Update(selfRef, selfRef, selfRef);
            
            // Should not throw due to max depth limitation
            var formatted = links.FormatAsNestedAssociation(selfRef, 3);
            
            Assert.NotNull(formatted);
            Assert.NotEmpty(formatted);
        }

        [Fact]
        public static void EmptyLinksFormattingTest()
        {
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            // Test formatting non-existent links
            var formatted = links.FormatAsAssociation(999u);
            
            Assert.Equal("999", formatted);
        }
    }
}