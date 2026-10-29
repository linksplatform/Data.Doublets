using System;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Exceptions;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class AddressLimitTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExhaustedAddressesAreRejectedButFreedLinksCanBeReused(bool split)
    {
        using var data = new HeapResizableDirectMemory();
        using var index = new HeapResizableDirectMemory();
        var constants = new LinksConstants<ulong>((1, 9), (0, 0));
        using var disposable = split
            ? (IDisposable)new SplitMemoryLinks<ulong>(data, index, 4096, constants)
            : new UnitedMemoryLinks<ulong>(data, 4096, constants, IndexTreeType.Default);
        var links = (ILinks<ulong>)disposable;
        var maximum = constants.InternalReferencesRange.Maximum;
        Assert.InRange(maximum, 1UL, 9UL);
        for (ulong address = 1; address <= maximum; address++)
            Assert.Equal(address, links.Create());
        Assert.Throws<LinksLimitReachedException<ulong>>(() => links.Create());
        Assert.Equal(maximum, links.Count());
        links.Delete(1UL);
        Assert.Equal(1UL, links.Create());
        Assert.Throws<LinksLimitReachedException<ulong>>(() => links.Create());
    }
}
