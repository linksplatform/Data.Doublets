using System;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Stacks;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class DeletionIndexTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeletingByAddressKeepsSurvivingLinksIndexed(bool split)
        => CheckIssueLinks(split, (links, victim) =>
        {
            Assert.Equal(victim, links.DeleteIfExists(victim));
            Assert.Equal(0UL, links.DeleteIfExists(victim));
        });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeleteIfExistsSkipsAddressesWithoutLiveSlots(bool split)
    {
        using var store = CreateStore(split, externalReferences: true);
        var links = (ILinks<ulong>)store;
        var point = links.CreatePoint();
        var constants = links.Constants;

        Assert.Equal(constants.Null, links.DeleteIfExists(constants.Null));
        Assert.Equal(constants.Null, links.DeleteIfExists(constants.Any));
        Assert.Equal(constants.Null, links.DeleteIfExists(10UL));
        Assert.Equal(constants.Null, links.DeleteIfExists(constants.ExternalReferencesRange!.Value.Minimum));

        Assert.Equal(1UL, links.Count());
        Assert.Equal(point, links.SearchOrDefault(point, point));
        Assert.Equal(2UL, links.CreatePoint());
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeletingByPairKeepsSurvivingLinksIndexed(bool split)
        => CheckIssueLinks(split, (links, victim) =>
        {
            Assert.Equal(victim, links.DeleteIfExists(2UL, 1UL));
            Assert.Equal(0UL, links.DeleteIfExists(2UL, 1UL));
        });

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeletingManyKeepsSurvivingLinksIndexed(bool split)
        => CheckIssueLinks(split, (links, victim) => links.DeleteMany(new[] { victim }));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeletingByQueryKeepsSurvivingLinksIndexed(bool split)
        => CheckIssueLinks(split, (links, _) => links.DeleteByQuery(new Link<ulong>(links.Constants.Any, 2, 1)));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergingAndDeletingKeepsSurvivingLinksIndexed(bool split)
        => CheckIssueLinks(split, (links, victim) => Assert.Equal(3UL, links.MergeAndDelete(victim, 3UL)));

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void PoppingStackDetachesDeletedNode(bool split)
    {
        using var store = CreateStore(split);
        var links = (ILinks<ulong>)store;
        var marker = links.CreatePoint();
        var value = links.CreatePoint();
        var address = links.CreateStack(marker);
        var stack = new Stack<ulong>(links, address);
        stack.Push(value);

        Assert.Equal(value, stack.Pop());
        Assert.True(stack.IsEmpty);
        Assert.Equal(0UL, links.SearchOrDefault(address, value));
        Assert.Equal(1UL, links.Count(new Link<ulong>(links.Constants.Any, links.Constants.Any, value)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void CallerCanResetThenUseRawAddressDelete(bool split)
        => CheckIssueLinks(split, (links, victim) =>
        {
            links.Update(victim, 0UL, 0UL);
            Assert.Equal(victim, links.Delete(victim));
        });

    private static void CheckIssueLinks(bool split, Action<ILinks<ulong>, ulong> delete)
    {
        using var store = CreateStore(split);
        var links = (ILinks<ulong>)store;
        foreach (var (source, target) in new (ulong, ulong)[] { (1, 1), (2, 2), (1, 2), (2, 1), (2, 3) })
        {
            links.CreateAndUpdate(source, target);
        }

        delete(links, 4UL);

        Assert.Equal(4UL, links.Count());
        Assert.False(links.Exists(4UL));
        Assert.Equal(1UL, links.SearchOrDefault(1UL, 1UL));
        Assert.Equal(2UL, links.SearchOrDefault(2UL, 2UL));
        Assert.Equal(3UL, links.SearchOrDefault(1UL, 2UL));
        Assert.Equal(5UL, links.SearchOrDefault(2UL, 3UL));
        Assert.Equal(0UL, links.SearchOrDefault(2UL, 1UL));
        Assert.Equal(2UL, links.Count(new Link<ulong>(links.Constants.Any, 2, links.Constants.Any)));
        Assert.Equal(1UL, links.Count(new Link<ulong>(links.Constants.Any, links.Constants.Any, 1)));
        Assert.Equal(4UL, links.CreateAndUpdate(3UL, 2UL));
        Assert.Equal(4UL, links.SearchOrDefault(3UL, 2UL));
    }

    private static IDisposable CreateStore(bool split, bool externalReferences = false)
    {
        var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: externalReferences);
        return split
            ? new SplitMemoryLinks<ulong>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096, constants)
            : new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory(), 4096, constants, IndexTreeType.Default);
    }
}
