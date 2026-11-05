using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class SplitGrowthTests
{
    [Theory]
    [InlineData(4096, 4096)]
    [InlineData(8192, 4096)]
    [InlineData(4096, 8192)]
    public void GrowthRespectsTheCapacityOfBothBuffers(long dataCapacity, long indexCapacity)
    {
        using var data = new HeapResizableDirectMemory(dataCapacity);
        using var index = new HeapResizableDirectMemory(indexCapacity);
        using var links = new SplitMemoryLinks<ulong>(data, index, 4);
        for (ulong address = 1; address <= 128; address++)
        {
            Assert.Equal(address, links.Create());
            Assert.InRange(data.UsedCapacity, 0, data.ReservedCapacity);
            Assert.InRange(index.UsedCapacity, 0, index.ReservedCapacity);
        }
        Assert.Equal(128UL, links.Count());
    }
}
