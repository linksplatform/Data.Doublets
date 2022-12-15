using System;
using System.IO;
using Platform.Data.Doublets.Decorators;
using Xunit;

using Platform.Memory;

using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class GenericLinksTests
    {
        [Fact]
        public static void CRUDTest()
        {
            Using<byte>(links => links.TestCRUDOperations());
            Using<ushort>(links => links.TestCRUDOperations());
            Using<uint>(links => links.TestCRUDOperations());
            Using<ulong>(links => links.TestCRUDOperations());
        }

        [Fact]
        public static void RawNumbersCRUDTest()
        {
            Using<byte>(links => links.TestRawNumbersCRUDOperations());
            Using<ushort>(links => links.TestRawNumbersCRUDOperations());
            Using<uint>(links => links.TestRawNumbersCRUDOperations());
            Using<ulong>(links => links.TestRawNumbersCRUDOperations());
        }

        [Fact]
        public static void MultipleRandomCreationsAndDeletionsTest()
        {
            Using<byte>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(16)); // Cannot use more because current implementation of tree cuts out 5 bits from the address space.
            Using<ushort>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<uint>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
            Using<ulong>(links => links.DecorateWithAutomaticUniquenessAndUsagesResolution().TestMultipleRandomCreationsAndDeletions(100));
        }
        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) 
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            using (var logFile = File.Open("linksLogger.txt", FileMode.Create, FileAccess.Write))
            {
                LoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks, logFile);
                action(decoratedStorage);
            }

            /*
            File.Delete("db.links");
            using var ffiLinks = new Ffi.Links<TLinkAddress>("db.links");
            action(ffiLinks);
        */
        }
    }
}
