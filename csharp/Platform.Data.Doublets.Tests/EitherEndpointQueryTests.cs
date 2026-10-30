using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Memory;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class EitherEndpointQueryTests
{
    public static IEnumerable<object[]> Stores()
    {
        foreach (var decorated in new[] { false, true })
        {
            foreach (var tree in new[] { IndexTreeType.Default, IndexTreeType.SizedAndThreadedAVLBalancedTree })
            {
                yield return new object[] { false, tree, true, decorated };
            }
            foreach (var tree in new[] { IndexTreeType.Default, IndexTreeType.SizeBalancedTree, IndexTreeType.RecursionlessSizeBalancedTree })
            {
                foreach (var linkedList in new[] { false, true })
                {
                    yield return new object[] { true, tree, linkedList, decorated };
                }
            }
        }
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void CountMatchesOriginalReproduction(bool split, IndexTreeType tree, bool linkedList, bool decorated)
        => CheckOriginal(split, tree, linkedList, decorated, count: true);

    [Theory]
    [MemberData(nameof(Stores))]
    public void EachMatchesOriginalReproduction(bool split, IndexTreeType tree, bool linkedList, bool decorated)
        => CheckOriginal(split, tree, linkedList, decorated, count: false);

    private static void CheckOriginal(bool split, IndexTreeType tree, bool linkedList, bool decorated, bool count)
    {
        Using<uint>(split, tree, linkedList, decorated, links =>
        {
            links.CreatePoint();
            links.CreatePoint();
            links.Update(2u, 1u, 2u);
            var query = new[] { links.Constants.Any, 1u };
            if (count)
            {
                Assert.Equal(2u, links.Count(query));
            }
            else
            {
                Assert.Equal(new[] { 1u, 2u }, Find(links, query));
            }
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void MatchingSnapshotCanBeDeletedOnce(bool split, IndexTreeType tree, bool linkedList, bool decorated)
    {
        Using<uint>(split, tree, linkedList, decorated, links =>
        {
            var a = links.CreatePoint();
            var b = links.CreatePoint();
            links.Update(b, a, b);
            var matches = Find(links, new[] { links.Constants.Any, a });
            foreach (var index in Enumerable.Reverse(matches))
            {
                Assert.True(links.Exists(index));
                links.Update(index, 0u, 0u);
                links.Delete(index);
            }
            Assert.Equal(0u, links.Count());
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void UnionMatchesScanAcrossAddressTypes(bool split, IndexTreeType tree, bool linkedList, bool decorated)
    {
        CheckUnion<byte>(split, tree, linkedList, decorated);
        CheckUnion<ushort>(split, tree, linkedList, decorated);
        CheckUnion<uint>(split, tree, linkedList, decorated);
        CheckUnion<ulong>(split, tree, linkedList, decorated);
    }

    private static void CheckUnion<T>(bool split, IndexTreeType tree, bool linkedList, bool decorated)
        where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
            IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        Using<T>(split, tree, linkedList, decorated, links =>
        {
            var any = links.Constants.Any;
            AssertUnion(links, T.One);
            var a = links.CreatePoint();
            var b = links.CreatePoint();
            var sourceOnly = links.CreateAndUpdate(a, b);
            var targetOnly = links.CreateAndUpdate(b, a);
            var overlap = decorated ? links.CreateAndUpdate(a, a) : a;
            var values = new List<T> { a, b, T.Zero, T.CreateChecked(50) };
            var external = T.Zero;
            if (split)
            {
                external = new Hybrid<T>(T.CreateChecked(20), isExternal: true).Value;
                links.CreateAndUpdate(external, external);
                links.CreateAndUpdate(external, a);
                links.CreateAndUpdate(b, external);
                values.Add(external);
            }
            foreach (var value in values)
            {
                AssertUnion(links, value);
            }
            var empty = links.Create();
            AssertUnion(links, T.Zero);
            links.Delete(empty);
            Assert.Equal(Find(links, Array.Empty<T>()).OrderBy(x => x), Find(links, new[] { any, any }).OrderBy(x => x));
            Assert.Equal(links.Count(), links.Count(new[] { any, any }));
            Assert.Equal(new[] { a }, Find(links, new[] { a, a }));
            Assert.Equal(T.One, links.Count(new[] { a, a }));

            // Exercise index changes, removal and free-slot reuse.
            links.Update(sourceOnly, b, sourceOnly);
            links.Update(targetOnly, T.Zero, T.Zero);
            links.Delete(targetOnly);
            links.CreateAndUpdate(b, a);
            foreach (var value in values)
            {
                AssertUnion(links, value);
            }
            Assert.True(links.Exists(overlap));

            // AVL indexes reject duplicate keys even on raw stores.
            if (!decorated && tree != IndexTreeType.SizedAndThreadedAVLBalancedTree)
            {
                links.CreateAndUpdate(a, a);
                links.CreateAndUpdate(a, a);
                if (split)
                {
                    links.CreateAndUpdate(external, external);
                }
                foreach (var value in values)
                {
                    AssertUnion(links, value);
                }
            }
        });
    }

    [Theory]
    [MemberData(nameof(Stores))]
    public void BreakStopsInEitherPass(bool split, IndexTreeType tree, bool linkedList, bool decorated)
    {
        Using<uint>(split, tree, linkedList, decorated, links =>
        {
            var a = links.CreatePoint();
            var b = links.CreatePoint();
            links.CreateAndUpdate(a, b);
            links.CreateAndUpdate(b, a);
            var query = new[] { links.Constants.Any, a };
            var all = Find(links, query);
            for (var stopAt = 1; stopAt <= all.Length; stopAt++)
            {
                var visited = new List<uint>();
                var result = links.Each(query, link =>
                {
                    visited.Add(link![0]);
                    return visited.Count == stopAt ? links.Constants.Break : links.Constants.Continue;
                });
                Assert.Equal(links.Constants.Break, result);
                Assert.Equal(all.Take(stopAt), visited);
                Assert.Equal(visited.Count, visited.Distinct().Count());
            }
            Assert.Equal(links.Constants.Continue, links.Each(query, null));
        });
    }

    private static void AssertUnion<T>(ILinks<T> links, T value) where T : IUnsignedNumber<T>
    {
        var expected = new List<T>();
        links.Each(Array.Empty<T>(), link =>
        {
            if (link![1] == value || link[2] == value)
            {
                expected.Add(link[0]);
            }
            return links.Constants.Continue;
        });
        var query = new[] { links.Constants.Any, value };
        Assert.Equal(T.CreateChecked(expected.Count), links.Count(query));
        Assert.Equal(expected.OrderBy(x => x), Find(links, query).OrderBy(x => x));
    }

    private static T[] Find<T>(ILinks<T> links, T[] query) where T : IUnsignedNumber<T>
    {
        var found = new List<T>();
        Assert.Equal(links.Constants.Continue, links.Each(query, link =>
        {
            found.Add(link![0]);
            return links.Constants.Continue;
        }));
        return found.ToArray();
    }

    private static void Using<T>(bool split, IndexTreeType tree, bool linkedList, bool decorated, Action<ILinks<T>> action)
        where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
            IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        var constants = new LinksConstants<T>(enableExternalReferencesSupport: true);
        using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096, constants, tree);
        using var separated = split ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(),
            4096, constants, tree, linkedList) : null;
        ILinks<T> raw = split ? separated! : united!;
        action(decorated ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw);
    }
}
