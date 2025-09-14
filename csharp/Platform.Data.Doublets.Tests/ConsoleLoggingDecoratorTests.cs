using System;
using System.IO;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Xunit;

using Platform.Memory;

using Platform.Data.Doublets.Memory.United.Generic;

namespace Platform.Data.Doublets.Tests
{
    public static class ConsoleLoggingDecoratorTests
    {
        [Fact]
        public static void BasicConsoleLoggingTest()
        {
            Using<ulong>(links => 
            {
                // Create a link
                var link = links.Create();
                
                // Update it
                links.Update(link, link, link);
                
                // Count links  
                var count = links.Count();
                
                // Delete the link
                links.Delete(link);
            });
        }

        [Fact]
        public static void ConsoleLoggingWithRestrictionsTest()
        {
            Using<ulong>(links => 
            {
                var link1 = links.Create();
                var link2 = links.Create();
                
                // Test with restrictions
                var count = links.Count(new[] { links.Constants.Any, link1, links.Constants.Any });
                
                links.Delete(link1);
                links.Delete(link2);
            });
        }

        private static void Using<TLinkAddress>(Action<ILinks<TLinkAddress>> action) where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>, IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var unitedMemoryLinks = new UnitedMemoryLinks<TLinkAddress>(new HeapResizableDirectMemory());
            ConsoleLoggingDecorator<TLinkAddress> decoratedStorage = new(unitedMemoryLinks);
            action(decoratedStorage);
        }
    }
}