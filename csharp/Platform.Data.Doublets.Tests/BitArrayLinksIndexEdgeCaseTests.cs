using System;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class BitArrayLinksIndexEdgeCaseTests
{
    [Fact]
    public void ReadCallbackFailureDoesNotKeepWriteGuardLocked()
    {
        using var store = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
        var index = new BitArrayLinksIndex<ulong>(store);
        var id = index.CreateAndUpdate<ulong>(11, 21);
        var query = new[] { store.Constants.Any, 11UL, store.Constants.Any };
        var failure = new InvalidOperationException("read callback");
        Assert.Same(failure, Assert.Throws<InvalidOperationException>(() =>
            index.Each(query, _ => throw failure)));
        index.Update<ulong>(id, 12, 22);
        Assert.Equal(0UL, index.Count(query));
        Assert.Equal(1UL, index.Count(new[] { store.Constants.Any, 12UL, store.Constants.Any }));
    }

    [Fact]
    public void WritesAfterClearRemainIndexedWhenReadsAreDisabled()
    {
        using var store = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
        var index = new BitArrayLinksIndex<ulong>(store, BitArrayIndexReadMode.Never);
        index.CreateAndUpdate<ulong>(11, 21);
        index.ClearIndex();
        index.CreateAndUpdate<ulong>(11, 22);
        Assert.Equal(2, index.IndexedLinkCount);
        index.ReadMode = BitArrayIndexReadMode.Always;
        var query = new[] { store.Constants.Any, 11UL, store.Constants.Any };
        Assert.Equal(store.Count(query), index.Count(query));
        Assert.Equal(2UL, index.Count(query));
    }
}
