using System;
using System.Collections.Generic;
using System.Numerics;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

// Issue #354: DeleteAll must delete every link and leave a store that works like a new one.
public class DeleteAllTests
{
    public static readonly TheoryData<bool> Stores = new() { false, true };

    [Theory]
    [MemberData(nameof(Stores))]
    public void DeletesContiguousPoints(bool split)
    {
        // The original loop deleted 3, skipped 2 and deleted 1.
        Check<byte>(split, links => CreatePoints(links, 3));
        Check<ushort>(split, links => CreatePoints(links, 3));
        Check<uint>(split, links => CreatePoints(links, 3));
        Check<ulong>(split, links => CreatePoints(links, 3));
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void DeletesLinksBetweenPoints(bool split)
    {
        // Deleting without resetting endpoints first leaves stale entries in the endpoint indexes.
        Check<uint>(split, links => links.CreateAndUpdate(links.CreatePoint(), links.CreatePoint()));
        Check<uint>(split, links =>
        {
            var (first, second) = (links.CreatePoint(), links.CreatePoint());
            links.CreateAndUpdate(first, second);
            links.CreateAndUpdate(second, first);
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void DeletesLinksAroundFreedAddresses(bool split)
    {
        // Count() is 3 while the highest address is 5: links 4 and 5 are above the count.
        Check<uint>(split, links =>
        {
            var points = CreatePoints(links, 5);
            links.Delete(points[1], null);
            links.Delete(points[2], null);
            links.Update(points[4], points[0], points[3]);
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void DeletesLinksThatUseHigherAddresses(bool split)
    {
        Check<uint>(split, links =>
        {
            var points = CreatePoints(links, 3);
            links.Update(points[0], points[1], points[2]);
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void EmptyStoreStaysEmpty(bool split) => Check<uint>(split, _ => { });

    [Theory]
    [MemberData(nameof(Stores))]
    public void WorksThroughCascadeDecorators(bool split)
    {
        Check<uint>(split, links =>
        {
            var points = CreatePoints(links, 4);
            links.Delete(points[1], null);
            links.GetOrCreate(points[0], points[3]);
            links.GetOrCreate(points[3], points[2]);
        }, links => links.DecorateWithAutomaticUniquenessAndUsagesResolution());
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void DeletesUsagesBeforeTheLinksTheyUse(bool split)
    {
        // The validator rejects deleting a link that is still used by another link.
        Check<uint>(split, links =>
        {
            var (first, second) = (links.CreatePoint(), links.CreatePoint());
            links.CreateAndUpdate(first, second);
        }, links => new LinksUsagesValidator<uint>(links));
    }

    private static void Check<TLinkAddress>(bool split, Action<ILinks<TLinkAddress>> fill, Func<ILinks<TLinkAddress>, ILinks<TLinkAddress>>? decorate = null)
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IShiftOperators<TLinkAddress, int, TLinkAddress>,
            IBitwiseOperators<TLinkAddress, TLinkAddress, TLinkAddress>, IMinMaxValue<TLinkAddress>,
            IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        using var sources = new HeapResizableDirectMemory();
        using var targets = new HeapResizableDirectMemory();
        if (split)
        {
            using var store = new SplitMemoryLinks<TLinkAddress>(sources, targets, 4096);
            DeleteAllAndReuse(store, fill, decorate);
        }
        else
        {
            using var store = new UnitedMemoryLinks<TLinkAddress>(sources, 4096);
            DeleteAllAndReuse(store, fill, decorate);
        }
    }

    private static void DeleteAllAndReuse<TLinkAddress>(ILinks<TLinkAddress> raw, Action<ILinks<TLinkAddress>> fill, Func<ILinks<TLinkAddress>, ILinks<TLinkAddress>>? decorate)
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        fill(raw);
        var links = decorate?.Invoke(raw) ?? raw;
        links.DeleteAll();
        Assert.Equal(TLinkAddress.Zero, links.Count());
        Assert.Empty(links.All());
        // Addresses start from 1 again, and the endpoint indexes contain only new links.
        var first = links.CreatePoint();
        var second = links.CreatePoint();
        // GetOrCreate searches the index first, so a stale entry would be returned instead of a new link.
        var link = links.GetOrCreate(first, second);
        Assert.Equal(TLinkAddress.One, first);
        Assert.Equal(TLinkAddress.CreateTruncating(3), link);
        Assert.Equal(TLinkAddress.CreateTruncating(3), links.Count());
        Assert.Equal(link, links.SearchOrDefault(first, second));
        var any = links.Constants.Any;
        Assert.Equal(TLinkAddress.CreateTruncating(2), links.Count(any, first, any));
        Assert.Equal(TLinkAddress.CreateTruncating(2), links.Count(any, any, second));
    }

    private static List<TLinkAddress> CreatePoints<TLinkAddress>(ILinks<TLinkAddress> links, int count)
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        var points = new List<TLinkAddress>();
        for (var i = 0; i < count; i++)
        {
            points.Add(links.CreatePoint());
        }
        return points;
    }
}
