using System;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class LinksExternalReferenceValidatorTests
    {
        [Fact]
        public static void InternalReferenceValidationTest()
        {
            // This test verifies that internal references must exist in the store
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validator = new LinksExternalReferenceValidator<uint>(links);

            // Create a valid link
            var link1 = validator.Create();
            var link2 = validator.Create();

            // Update with valid internal references should succeed
            validator.Update(link1, link1, link2);

            // Try to update with non-existent internal reference should throw
            var nonExistentInternalRef = (uint)999;
            Assert.True(links.Constants.IsInternalReference(nonExistentInternalRef));
            Assert.False(links.Exists(nonExistentInternalRef));

            Assert.Throws<ArgumentLinkDoesNotExistsException<uint>>(() =>
            {
                validator.Update(link1, nonExistentInternalRef, link2);
            });

            memory.Dispose();
        }

        [Fact]
        public static void ExternalReferenceAllowedTest()
        {
            // This test verifies that external references (binary data) are allowed without existence validation
            var memory = new HeapResizableDirectMemory();

            // Configure with explicit internal and external ranges
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);
            var validator = new LinksExternalReferenceValidator<ulong>(links);

            // Create a valid link
            var link1 = validator.Create();

            // Use external reference (binary data) - should NOT throw even if it doesn't exist
            // External references represent raw binary data, not links in the store
            // Use values within internal range but marked as external via Hybrid (like LinksConstantsTests)
            // Use a large value that's unlikely to exist as a link ID
            var externalRef = new Hybrid<ulong>(long.MaxValue - 1000, isExternal: true);

            Assert.True(links.Constants.IsExternalReference(externalRef));
            // Note: We don't assert !Exists() because the value might coincidentally exist
            // The key test is that Update succeeds without throwing for external references

            // This should succeed because external references are allowed
            var updatedLink = validator.Update(link1, externalRef, externalRef);

            Assert.True(updatedLink > default(ulong));

            memory.Dispose();
        }

        [Fact]
        public static void MixedInternalAndExternalReferencesTest()
        {
            // This test verifies mixed usage of internal and external references
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);
            var validator = new LinksExternalReferenceValidator<ulong>(links);

            // Create valid internal links
            var internalLink1 = validator.Create();
            var internalLink2 = validator.Create();

            // Create external references (binary data)
            // Use values within internal range but marked as external via Hybrid
            var externalRef1 = new Hybrid<ulong>(100, isExternal: true);
            var externalRef2 = new Hybrid<ulong>(200, isExternal: true);

            // Verify classifications
            Assert.True(links.Constants.IsInternalReference(internalLink1));
            Assert.True(links.Constants.IsExternalReference(externalRef1));

            // Update with internal → external should succeed
            var link3 = validator.Update(internalLink1, internalLink2, externalRef1);
            Assert.True(link3 > default(ulong));

            // Update with external → external should succeed
            var link4 = validator.Create();
            var link5 = validator.Update(link4, externalRef1, externalRef2);
            Assert.True(link5 > default(ulong));

            // Update with internal (non-existent) should still throw
            var nonExistentInternal = (ulong)999;
            Assert.True(links.Constants.IsInternalReference(nonExistentInternal));

            Assert.Throws<ArgumentLinkDoesNotExistsException<ulong>>(() =>
            {
                validator.Update(internalLink1, nonExistentInternal, externalRef1);
            });

            memory.Dispose();
        }

        [Fact]
        public static void EachOperationWithExternalReferencesTest()
        {
            // This test verifies the Each operation with external references
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);
            var validator = new LinksExternalReferenceValidator<ulong>(links);

            // Create links with external references
            var externalRef = new Hybrid<ulong>(500, isExternal: true);
            var internalLink = validator.Create();

            var linkWithExternal = validator.Update(internalLink, externalRef, externalRef);

            // Query using Each with external reference in restriction should work
            int count = 0;
            validator.Each(new[] { linkWithExternal }, link =>
            {
                count++;
                return links.Constants.Continue;
            });

            Assert.Equal(1, count);

            memory.Dispose();
        }

        [Fact]
        public static void DeleteOperationValidationTest()
        {
            // This test verifies delete operation still validates link existence
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);
            var validator = new LinksExternalReferenceValidator<ulong>(links);

            // Create a link
            var link1 = validator.Create();

            // Delete existing link should succeed
            validator.Delete(link1);

            // Try to delete non-existent link should throw
            Assert.Throws<ArgumentLinkDoesNotExistsException<ulong>>(() =>
            {
                validator.Delete(link1);
            });

            memory.Dispose();
        }

        [Fact]
        public static void ComparisonWithInnerReferenceValidatorTest()
        {
            // This test demonstrates the difference between LinksInnerReferenceExistenceValidator
            // and LinksExternalReferenceValidator
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);

            var innerValidator = new LinksInnerReferenceExistenceValidator<ulong>(links);
            var externalValidator = new LinksExternalReferenceValidator<ulong>(links);

            var internalLink = links.Create();
            var externalRef = new Hybrid<ulong>(999, isExternal: true);

            // Inner validator only accepts internal references
            // External reference should be treated as valid by EnsureInnerReferenceExists
            // because it checks IsInternalReference first
            var link2 = innerValidator.Update(internalLink, externalRef, externalRef);
            Assert.True(link2 > default(ulong));

            // External validator also accepts external references explicitly
            var link3 = links.Create();
            var link4 = externalValidator.Update(link3, externalRef, externalRef);
            Assert.True(link4 > default(ulong));

            memory.Dispose();
        }

        [Fact]
        public static void BinaryDataStorageScenarioTest()
        {
            // This test demonstrates the real-world use case from issue #508:
            // Using the doublets store as a file system or binary data storage
            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(
                (1, long.MaxValue),
                (long.MaxValue + 1UL, ulong.MaxValue)
            );
            var links = new UnitedMemoryLinks<ulong>(memory, UnitedMemoryLinks<ulong>.DefaultLinksSizeStep, constants, IndexTreeType.Default);
            var validator = new LinksExternalReferenceValidator<ulong>(links);

            // Scenario: Store file-like data where references can be binary data
            // Create a "file type" marker link
            var fileTypeMarker = validator.Create();

            // Create a "file" link where source is the type and target is binary metadata
            var binaryMetadata = new Hybrid<ulong>(0xDEADBEEF, isExternal: true);
            Assert.True(links.Constants.IsExternalReference(binaryMetadata));

            var fileLink = validator.Create();
            var updatedFileLink = validator.Update(fileLink, fileTypeMarker, binaryMetadata);

            Assert.True(updatedFileLink > default(ulong));

            // Create another link with binary data for file content pointer
            var contentPointer = new Hybrid<ulong>(0x12345678, isExternal: true);
            var contentLink = validator.Create();
            var updatedContentLink = validator.Update(contentLink, updatedFileLink, contentPointer);

            Assert.True(updatedContentLink > default(ulong));

            // Verify we can query these links
            int filesFound = 0;
            validator.Each(link =>
            {
                if (links.GetSource(link) == fileTypeMarker || links.GetSource(link) == updatedFileLink)
                {
                    filesFound++;
                }
                return links.Constants.Continue;
            });

            Assert.True(filesFound >= 2);

            memory.Dispose();
        }
    }
}
