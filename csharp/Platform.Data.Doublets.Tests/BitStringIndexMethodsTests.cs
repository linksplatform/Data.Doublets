using System;
using System.Collections;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests
{
    public static class BitStringIndexMethodsTests
    {
        [Fact]
        public static void BitStringBasicOperationsTest()
        {
            const ulong firstLink = 1ul;
            const ulong secondLink = 2ul;
            const ulong thirdLink = 3ul;
            const int bitPosition1 = 0;
            const int bitPosition2 = 5;
            const int bitPosition3 = 10;

            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 3);

                var bitStringMethods = new LinksSourcesBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                // Test SetBit and GetBit
                bitStringMethods.SetBit(firstLink, bitPosition1, true);
                bitStringMethods.SetBit(firstLink, bitPosition2, true);
                bitStringMethods.SetBit(secondLink, bitPosition1, true);
                bitStringMethods.SetBit(secondLink, bitPosition3, true);

                Assert.True(bitStringMethods.GetBit(firstLink, bitPosition1));
                Assert.True(bitStringMethods.GetBit(firstLink, bitPosition2));
                Assert.False(bitStringMethods.GetBit(firstLink, bitPosition3));
                
                Assert.True(bitStringMethods.GetBit(secondLink, bitPosition1));
                Assert.False(bitStringMethods.GetBit(secondLink, bitPosition2));
                Assert.True(bitStringMethods.GetBit(secondLink, bitPosition3));

                // Test CountSetBits
                Assert.Equal(2, bitStringMethods.CountSetBits(firstLink));
                Assert.Equal(2, bitStringMethods.CountSetBits(secondLink));
                Assert.Equal(0, bitStringMethods.CountSetBits(thirdLink));

                // Test BitwiseAnd
                var andResult = bitStringMethods.BitwiseAnd(firstLink, secondLink);
                Assert.True(andResult[bitPosition1]); // Both have bit 0 set
                Assert.False(andResult[bitPosition2]); // Only firstLink has bit 5 set
                Assert.False(andResult[bitPosition3]); // Only secondLink has bit 10 set

                // Test BitwiseOr
                var orResult = bitStringMethods.BitwiseOr(firstLink, secondLink);
                Assert.True(orResult[bitPosition1]); // Both have bit 0 set
                Assert.True(orResult[bitPosition2]); // firstLink has bit 5 set
                Assert.True(orResult[bitPosition3]); // secondLink has bit 10 set

                // Test BitwiseXor
                var xorResult = bitStringMethods.BitwiseXor(firstLink, secondLink);
                Assert.False(xorResult[bitPosition1]); // Both have bit 0 set (XOR = false)
                Assert.True(xorResult[bitPosition2]); // Only firstLink has bit 5 set
                Assert.True(xorResult[bitPosition3]); // Only secondLink has bit 10 set

                memory.Free();
            }
        }

        [Fact]
        public static void BitStringAttachDetachTest()
        {
            const ulong rootLink = 1ul;
            const ulong childLink1 = 2ul;
            const ulong childLink2 = 3ul;
            const ulong childLink3 = 4ul;

            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 4);

                var bitStringMethods = new LinksTargetsBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                // Test Attach
                var root = rootLink;
                bitStringMethods.Attach(ref root, childLink1);
                bitStringMethods.Attach(ref root, childLink2);
                bitStringMethods.Attach(ref root, childLink3);

                // Test CountUsages
                Assert.Equal(3ul, bitStringMethods.CountUsages(rootLink));

                // Test GetBitString
                var bitString = bitStringMethods.GetBitString(rootLink);
                Assert.True(bitString[int.CreateTruncating(childLink1) - 1]);
                Assert.True(bitString[int.CreateTruncating(childLink2) - 1]);
                Assert.True(bitString[int.CreateTruncating(childLink3) - 1]);

                // Test Detach
                bitStringMethods.Detach(ref root, childLink2);
                Assert.Equal(2ul, bitStringMethods.CountUsages(rootLink));

                var updatedBitString = bitStringMethods.GetBitString(rootLink);
                Assert.True(updatedBitString[int.CreateTruncating(childLink1) - 1]);
                Assert.False(updatedBitString[int.CreateTruncating(childLink2) - 1]);
                Assert.True(updatedBitString[int.CreateTruncating(childLink3) - 1]);

                memory.Free();
            }
        }

        [Fact]
        public static void BitStringSearchTest()
        {
            const ulong sourceLink = 1ul;
            const ulong targetLink = 2ul;
            const ulong connectionLink = 3ul;

            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 3);

                var bitStringMethods = new LinksSourcesBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                // Set up a connection: both source and target should reference the same connection
                bitStringMethods.SetBit(sourceLink, int.CreateTruncating(connectionLink) - 1, true);
                bitStringMethods.SetBit(targetLink, int.CreateTruncating(connectionLink) - 1, true);

                // Test Search - should find the connection
                var searchResult = bitStringMethods.Search(sourceLink, targetLink);
                Assert.Equal(connectionLink, searchResult);

                // Test Search with no common connection
                const ulong otherLink = 4ul;
                bitStringMethods.SetBit(otherLink, 10, true); // Different bit position
                var noConnectionResult = bitStringMethods.Search(sourceLink, otherLink);
                Assert.Equal(0ul, noConnectionResult);

                memory.Free();
            }
        }

        [Fact]
        public static void BitStringEachUsageTest()
        {
            const ulong rootLink = 1ul;
            const ulong childLink1 = 2ul;
            const ulong childLink2 = 3ul;

            var memory = new HeapResizableDirectMemory();
            var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);

            unsafe
            {
                var header = memory.AllocateOrReserve(sizeof(LinksHeader<ulong>));
                var links = memory.AllocateOrReserve(sizeof(RawLink<ulong>) * 3);

                var bitStringMethods = new LinksTargetsBitStringIndexMethods<ulong>(constants, (byte*)links, (byte*)header);

                // Attach children
                var root = rootLink;
                bitStringMethods.Attach(ref root, childLink1);
                bitStringMethods.Attach(ref root, childLink2);

                // Test EachUsage
                var visitedLinks = new System.Collections.Generic.List<ulong>();
                bitStringMethods.EachUsage(rootLink, link =>
                {
                    visitedLinks.Add(link[0]);
                    return constants.Continue;
                });

                Assert.Equal(2, visitedLinks.Count);
                Assert.Contains(childLink1, visitedLinks);
                Assert.Contains(childLink2, visitedLinks);

                // Test EachUsage with early break
                var limitedVisits = new System.Collections.Generic.List<ulong>();
                bitStringMethods.EachUsage(rootLink, link =>
                {
                    limitedVisits.Add(link[0]);
                    return constants.Break; // Break after first visit
                });

                Assert.Equal(1, limitedVisits.Count);

                memory.Free();
            }
        }
    }
}