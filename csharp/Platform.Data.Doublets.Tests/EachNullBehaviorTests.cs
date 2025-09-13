using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// Tests to document and verify the Each method behavior with null values.
    /// 
    /// This test class addresses Issue #173 by documenting the expected behavior
    /// when Constants.Null is passed to the Each method.
    /// </summary>
    public class EachNullBehaviorTests
    {
        /// <summary>
        /// Documents and tests the current behavior when Constants.Null is passed as a restriction.
        /// 
        /// Expected behavior: Constants.Null should be treated as literal link index 0,
        /// not as a wildcard. For wildcard behavior, Constants.Any should be used.
        /// </summary>
        [Fact]
        public void Each_WithConstantsNull_TreatsAsLiteralIndex0()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var constants = links.Constants;
            var createdLink = links.Create();  // This should create a link with index 1 (not 0)
            
            var foundLinks = new List<uint>();
            
            // Act - Search using Constants.Null (which is 0)
            links.Each(link => 
            {
                foundLinks.Add(link[constants.IndexPart]);
                return constants.Continue;
            }, constants.Null);
            
            // Assert
            // Constants.Null (0) should only find link at index 0 if it exists
            // Since we created a link starting from index 1, no links should be found
            Assert.Empty(foundLinks);
        }

        /// <summary>
        /// Tests the contrast between Constants.Null and Constants.Any behavior.
        /// </summary>
        [Fact]
        public void Each_ConstantsNull_vs_ConstantsAny_BehaviorDifference()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var constants = links.Constants;
            var createdLink1 = links.Create();
            var createdLink2 = links.Create();
            
            var nullResults = new List<uint>();
            var anyResults = new List<uint>();
            
            // Act - Search using Constants.Null (literal index 0)
            links.Each(link => 
            {
                nullResults.Add(link[constants.IndexPart]);
                return constants.Continue;
            }, constants.Null);
            
            // Act - Search using Constants.Any (wildcard)
            links.Each(link => 
            {
                anyResults.Add(link[constants.IndexPart]);
                return constants.Continue;
            }, constants.Any);
            
            // Assert
            // Constants.Null should find nothing (no link at index 0)
            Assert.Empty(nullResults);
            
            // Constants.Any should find all links
            Assert.Equal(2, anyResults.Count);
            Assert.Contains(createdLink1, anyResults);
            Assert.Contains(createdLink2, anyResults);
        }

        /// <summary>
        /// Tests the clarity extension methods added to address Issue #173.
        /// </summary>
        [Fact] 
        public void EachClarificationExtensions_ProvidesClearBehavior()
        {
            // Arrange
            var memory = new HeapResizableDirectMemory();
            using var links = new UnitedMemoryLinks<uint>(memory);
            
            var constants = links.Constants;
            var createdLink = links.Create();
            
            var allLinksResults = new List<uint>();
            var anyLinkResults = new List<uint>();
            var indexZeroResults = new List<uint>();
            
            // Act - Test extension methods
            links.EachAllLinks(link => 
            {
                allLinksResults.Add(link[constants.IndexPart]);
                return constants.Continue;
            });
            
            links.EachAnyLink(link => 
            {
                anyLinkResults.Add(link[constants.IndexPart]);
                return constants.Continue;
            });
            
            links.EachLinkAtIndexZero(link => 
            {
                indexZeroResults.Add(link[constants.IndexPart]);
                return constants.Continue;
            });
            
            // Assert
            // All methods should work as expected
            Assert.Single(allLinksResults);
            Assert.Single(anyLinkResults); 
            Assert.Empty(indexZeroResults); // No link at index 0
            
            Assert.Equal(createdLink, allLinksResults[0]);
            Assert.Equal(createdLink, anyLinkResults[0]);
        }
    }
}