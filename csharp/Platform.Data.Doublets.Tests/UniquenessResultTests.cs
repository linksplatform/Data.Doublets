using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Delegates;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class UniquenessResultTests
{
    public static IEnumerable<object[]> Stores()
    {
        foreach (var split in new[] { false, true })
        {
            foreach (var policy in new[] { "raw", "unique", "automatic" })
            {
                yield return new object[] { split, policy };
            }
        }
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void CreationReturnsLiveAddresses(bool split, string policy)
    {
        CheckCreation<byte>(split, policy);
        CheckCreation<ushort>(split, policy);
        CheckCreation<uint>(split, policy);
        CheckCreation<ulong>(split, policy);
    }

    private static void CheckCreation<T>(bool split, string policy)
        where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
            IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        ILinks<T> raw = split ? separated! : united!;
        var links = Decorate(raw, policy);
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        var created = links.CreateAndUpdate(first, first);
        Assert.Equal(policy == "raw" ? T.CreateTruncating(3) : first, created);
        Assert.True(links.Exists(created));
        Assert.Equal(new[] { created, first, first }, links.GetLink(created));

        // No (first, second) exists: this is the CreateAndUpdate fallback in #519.
        var updated = links.UpdateOrCreateOrGet(first, second, second, second);
        Assert.Equal(policy == "raw" ? T.CreateTruncating(4) : second, updated);
        Assert.True(links.Exists(updated));
        Assert.Equal(new[] { updated, second, second }, links.GetLink(updated));
        Assert.Equal(T.CreateTruncating(policy == "raw" ? 4 : 2), links.Count());

        // An absent target pair still creates a new link through every stack.
        var unique = links.CreateAndUpdate(first, second);
        Assert.True(links.Exists(unique));
        Assert.Equal(unique, links.SearchOrDefault(first, second));
        Assert.Equal(new[] { unique, first, second }, links.GetLink(unique));
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void UpdateOrCreateOrGetReturnsAddressForEveryBranch(bool split, string policy)
    {
        using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        var links = Decorate<uint>(split ? separated! : united!, policy);
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        Assert.Equal(first, links.UpdateOrCreateOrGet(first, first, first, first));
        Assert.Equal(first, links.UpdateOrCreateOrGet(first, first, first, second));
        var merged = links.UpdateOrCreateOrGet(first, second, second, second);
        Assert.Equal(policy == "raw" ? first : second, merged);
        Assert.True(links.Exists(merged));
        Assert.Equal(new[] { merged, second, second }, links.GetLink(merged));
    }

    [Theory]
    [InlineData(false, "unique")]
    [InlineData(false, "automatic")]
    [InlineData(true, "unique")]
    [InlineData(true, "automatic")]
    public void MergeReportsSurvivorAfterDeletionCallbacks(bool split, string policy)
    {
        using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        var links = Decorate<uint>(split ? separated! : united!, policy);
        var survivor = links.CreatePoint();
        var redundant = links.CreatePoint();
        var anchor = links.CreatePoint();
        var usage = policy == "automatic" ? links.CreateAndUpdate(redundant, anchor) : 0;
        var events = new List<(uint[]? Before, uint[]? After)>();
        Assert.Equal(links.Constants.Continue, links.Update(redundant, survivor, survivor, (before, after) =>
        {
            events.Add((before?.ToArray(), after?.ToArray()));
            return links.Constants.Continue;
        }));
        Assert.Contains(events, change => change.Before?[0] == redundant && change.After == null);
        Assert.Equal(new[] { survivor, survivor, survivor }, events[^1].After);
        Assert.False(links.Exists(redundant));
        Assert.True(links.Exists(survivor));
        if (policy == "automatic")
        {
            Assert.Equal(new[] { usage, survivor, anchor }, links.GetLink(usage));
        }
        // Updating the survivor to its current pair must also supply the result.
        Assert.Equal(survivor, links.Update(survivor, survivor, survivor));
        Assert.Equal(links.Constants.Continue, links.Update(survivor, survivor, survivor, null));
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void BreakSuppressesLaterCallbacksButCompletesCreation(bool split, string policy)
    {
        using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        var links = Decorate<uint>(split ? separated! : united!, policy);
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        foreach (var collide in new[] { false, true })
        {
            foreach (var stopAt in new[] { 1, 2 })
            {
                var calls = 0;
                Assert.Equal(links.Constants.Break, links.CreateAndUpdate(first, collide ? first : second, (_, _) =>
                    ++calls == stopAt ? links.Constants.Break : links.Constants.Continue));
                Assert.Equal(stopAt, calls);
                Assert.NotEqual(0u, links.SearchOrDefault(first, collide ? first : second));
            }
        }
        Assert.Equal(links.Constants.Continue, links.CreateAndUpdate(second, first, null));
        Assert.NotEqual(0u, links.SearchOrDefault(second, first));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergePreservesBothEndpointsOfEveryUsage(bool split)
    {
        using var united = split ? null : new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<uint>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        ILinks<uint> raw = split ? separated! : united!;
        var links = raw.DecorateWithAutomaticUniquenessAndUsagesResolution();
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        var survivor = links.CreateAndUpdate(first, second);
        var redundant = links.CreateAndUpdate(second, first);
        var sourceUsage = links.CreateAndUpdate(redundant, second);
        var targetUsage = links.CreateAndUpdate(first, redundant);
        var bothUsage = links.CreateAndUpdate(redundant, redundant);
        Assert.Equal(survivor, links.Update(redundant, first, second));
        Assert.False(links.Exists(redundant));
        Assert.Equal(new[] { sourceUsage, survivor, second }, links.GetLink(sourceUsage));
        Assert.Equal(new[] { targetUsage, first, survivor }, links.GetLink(targetUsage));
        Assert.Equal(new[] { bothUsage, survivor, survivor }, links.GetLink(bothUsage));
        Assert.Equal(6u, links.Count());
    }

    [Fact]
    public void CreateAndUpdateRetainsBreakEvenWhenInnerUpdateReturnsContinue()
    {
        using var raw = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        var links = new ContinueAfterUpdate(raw);
        var calls = 0;
        Assert.Equal(links.Constants.Break, links.CreateAndUpdate(0u, 0u, (_, _) =>
            ++calls == 2 ? links.Constants.Break : links.Constants.Continue));
        Assert.Equal(2, calls);
        Assert.Equal(1u, raw.Count());
    }

    private static ILinks<T> Decorate<T>(ILinks<T> raw, string policy) where T : IUnsignedNumber<T>
        => policy switch
        {
            "unique" => new LinksUniquenessResolver<T>(new NonNullContentsLinkDeletionResolver<T>(raw)),
            "automatic" => raw.DecorateWithAutomaticUniquenessAndUsagesResolution(),
            _ => raw
        };

    private sealed class ContinueAfterUpdate(ILinks<uint> links) : LinksDecoratorBase<uint>(links)
    {
        public override uint Update(IList<uint>? restriction, IList<uint>? substitution, WriteHandler<uint>? handler)
        {
            base.Update(restriction, substitution, handler);
            return Constants.Continue;
        }
    }
}
