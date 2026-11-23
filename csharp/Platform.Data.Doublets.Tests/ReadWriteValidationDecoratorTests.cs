using System;
using System.Collections.Generic;
using Xunit;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;

namespace Platform.Data.Doublets.Tests
{
    /// <summary>
    /// <para>
    /// Tests for the ReadWriteValidationDecorator to ensure that write operations
    /// are properly blocked during read operations.
    /// </para>
    /// </summary>
    public class ReadWriteValidationDecoratorTests
    {
        /// <summary>
        /// <para>
        /// Tests that write operations are allowed when not in a read operation.
        /// </para>
        /// </summary>
        [Fact]
        public void WriteOperationsAllowedWhenNotInReadOperation()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // These operations should work normally
            var link1 = validatedLinks.Create(null, null);
            var link2 = validatedLinks.Create(null, null);
            
            // No exception should be thrown for basic operations
            Assert.True(link1 > 0, "Created link should have valid address");
            Assert.True(link2 > 0, "Created link should have valid address");
        }

        /// <summary>
        /// <para>
        /// Tests that Create operations are blocked during Each (read) operations.
        /// </para>
        /// </summary>
        [Fact]
        public void CreateBlockedDuringReadOperation()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            // Try to create a link during Each operation
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
                {
                    validatedLinks.Create(null, null); // This should throw
                    return validatedLinks.Constants.Continue;
                });
            });

            Assert.Contains("Cannot perform write operation 'Create' during a read operation", exception.Message);
            Assert.Contains("unsafe", exception.Message);
        }

        /// <summary>
        /// <para>
        /// Tests that Update operations are blocked during Each (read) operations.
        /// </para>
        /// </summary>
        [Fact]
        public void UpdateBlockedDuringReadOperation()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            var link1 = validatedLinks.Create(null, null);
            var link2 = validatedLinks.Create(null, null);

            // Try to update a link during Each operation
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
                {
                    validatedLinks.Update(link1, link1, link2, null); // This should throw
                    return validatedLinks.Constants.Continue;
                });
            });

            Assert.Contains("Cannot perform write operation 'Update' during a read operation", exception.Message);
        }

        /// <summary>
        /// <para>
        /// Tests that Delete operations are blocked during Each (read) operations.
        /// </para>
        /// </summary>
        [Fact]
        public void DeleteBlockedDuringReadOperation()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            var link1 = validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            // Try to delete a link during Each operation
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
                {
                    validatedLinks.Delete(link1, null); // This should throw
                    return validatedLinks.Constants.Continue;
                });
            });

            Assert.Contains("Cannot perform write operation 'Delete' during a read operation", exception.Message);
        }

        /// <summary>
        /// <para>
        /// Tests that Count operations are allowed during Each (read) operations
        /// as they don't modify the data structure.
        /// </para>
        /// </summary>
        [Fact]
        public void CountAllowedDuringReadOperation()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            var countDuringRead = 0u;

            // Count should work during Each operation
            validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
            {
                countDuringRead = validatedLinks.Count(new Link<uint>(validatedLinks.Constants.Any));
                return validatedLinks.Constants.Continue;
            });

            Assert.True(countDuringRead > 0, "Count should work during read operation");
        }

        /// <summary>
        /// <para>
        /// Tests that nested Each operations work correctly and still block write operations.
        /// </para>
        /// </summary>
        [Fact]
        public void NestedReadOperationsStillBlockWrites()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            // Try nested Each operations with write operation in inner loop
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), outerLink =>
                {
                    validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), innerLink =>
                    {
                        validatedLinks.Create(null, null); // This should throw even in nested Each
                        return validatedLinks.Constants.Continue;
                    });
                    return validatedLinks.Constants.Continue;
                });
            });

            Assert.Contains("Cannot perform write operation 'Create' during a read operation", exception.Message);
        }

        /// <summary>
        /// <para>
        /// Tests that write operations are allowed again after Each operations complete.
        /// </para>
        /// </summary>
        [Fact]
        public void WriteOperationsAllowedAfterReadOperationCompletes()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            var linksProcessed = 0;

            // Do a read operation
            validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
            {
                linksProcessed++;
                return validatedLinks.Constants.Continue;
            });

            // Write operations should work after Each completes
            var newLink = validatedLinks.Create(null, null);
            validatedLinks.Update(newLink, newLink, newLink, null);
            validatedLinks.Delete(newLink, null);

            Assert.True(linksProcessed > 0, "Should have processed some links");
            // No exception should be thrown for write operations after read
        }

        /// <summary>
        /// <para>
        /// Tests that the IsInReadOperation property correctly reflects the current state.
        /// </para>
        /// </summary>
        [Fact]
        public void IsInReadOperationPropertyWorksCorrectly()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);

            // Should not be in read operation initially
            Assert.False(ReadWriteValidationDecorator<uint>.IsInReadOperation, 
                "Should not be in read operation initially");

            validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
            {
                // Should be in read operation during Each
                Assert.True(ReadWriteValidationDecorator<uint>.IsInReadOperation, 
                    "Should be in read operation during Each");
                return validatedLinks.Constants.Continue;
            });

            // Should not be in read operation after Each completes
            Assert.False(ReadWriteValidationDecorator<uint>.IsInReadOperation, 
                "Should not be in read operation after Each completes");
        }

        /// <summary>
        /// <para>
        /// Tests that the decorator works correctly when using extension methods that internally call write operations.
        /// </para>
        /// </summary>
        [Fact]
        public void BlocksWriteOperationsThroughExtensionMethods()
        {
            var memory = new HeapResizableDirectMemory();
            var links = new UnitedMemoryLinks<uint>(memory);
            var validatedLinks = new ReadWriteValidationDecorator<uint>(links);

            // Create some initial links
            validatedLinks.Create(null, null);
            validatedLinks.Create(null, null);

            // Try to use GetOrCreate (which calls Create internally) during Each operation
            var exception = Assert.Throws<InvalidOperationException>(() =>
            {
                validatedLinks.Each(new Link<uint>(validatedLinks.Constants.Any), link =>
                {
                    // GetOrCreate extension method should be blocked as it calls Create internally
                    validatedLinks.GetOrCreate(link[0], link[0]);
                    return validatedLinks.Constants.Continue;
                });
            });

            Assert.Contains("Cannot perform write operation", exception.Message);
        }
    }
}