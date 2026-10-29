using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Memory;
using Platform.Data.Doublets.Decorators;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class NullableContractTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void OptionalRestrictionsAndReadHandlersAreSupported(bool split)
    {
        using var first = new HeapResizableDirectMemory();
        using var second = new HeapResizableDirectMemory();
        using var storage = split
            ? (System.IDisposable)new SplitMemoryLinks<ulong>(first, second)
            : new UnitedMemoryLinks<ulong>(first);
        var links = (ILinks<ulong>)storage;
        links.Create();
        Assert.Equal(1UL, links.Count(null));
        Assert.Equal(links.Constants.Continue, links.Each(null, null));
        var visited = 0;
        links.Each(null, _ => { visited++; return links.Constants.Continue; });
        Assert.Equal(1, visited);
        links.Create();
        visited = 0;
        Assert.Equal(links.Constants.Break, links.Each(null, _ => { visited++; return links.Constants.Break; }));
        Assert.Equal(1, visited);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void UniversalCreationAndDeletionAllowIndependentOptionalHandlers(bool split)
    {
        using var first = new HeapResizableDirectMemory();
        using var second = new HeapResizableDirectMemory();
        using var storage = split
            ? (System.IDisposable)new SplitMemoryLinks<ulong>(first, second)
            : new UnitedMemoryLinks<ulong>(first);
        var links = (ILinks<ulong>)storage;
        var universal = new UniLinks<ulong>(links);
        var calls = 0;
        var created = 0UL;
        universal.Trigger(null, (Platform.Delegates.ReadHandler<ulong>?)null, new ulong[] { 0, 0, 0 }, (before, after) =>
        {
            System.ArgumentNullException.ThrowIfNull(after);
            created = after[0];
            calls++;
            return links.Constants.Continue;
        });
        Assert.Equal(1, calls);
        Assert.NotEqual(0UL, created);
        // A match handler must not cause invocation of an absent write handler.
        universal.Trigger(new[] { created }, _ => links.Constants.Continue, null, null);
        Assert.Equal(0UL, links.Count());
    }

    [Fact]
    public void UniversalUpdateUsesTheExistingLinkInsteadOfCreatingAnother()
    {
        using var memory = new HeapResizableDirectMemory();
        using var links = new UnitedMemoryLinks<ulong>(memory);
        var original = links.Create();
        var universal = new UniLinks<ulong>(links);
        universal.Trigger(new[] { original }, (Platform.Delegates.ReadHandler<ulong>?)null,
            new ulong[] { 0, original, original }, null);
        Assert.Equal(1UL, links.Count(null));
        Assert.Equal(original, links.GetSource(original));
    }
}
