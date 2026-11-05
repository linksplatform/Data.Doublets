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

public class PositionalQueryTests
{
    [Theory]
    [MemberData(nameof(EitherEndpointQueryTests.Stores), MemberType = typeof(EitherEndpointQueryTests))]
    public void CountMatchesEndpointPositions(bool split, IndexTreeType tree, bool linkedList, bool decorated)
        => CheckAllWidths(split, tree, linkedList, decorated, count: true);

    [Theory]
    [MemberData(nameof(EitherEndpointQueryTests.Stores), MemberType = typeof(EitherEndpointQueryTests))]
    public void EachMatchesEndpointPositions(bool split, IndexTreeType tree, bool linkedList, bool decorated)
        => CheckAllWidths(split, tree, linkedList, decorated, count: false);

    private static void CheckAllWidths(bool split, IndexTreeType tree, bool linkedList, bool decorated, bool count)
    {
        Check<byte>(split, tree, linkedList, decorated, count);
        Check<ushort>(split, tree, linkedList, decorated, count);
        Check<uint>(split, tree, linkedList, decorated, count);
        Check<ulong>(split, tree, linkedList, decorated, count);
    }

    private static void Check<T>(bool split, IndexTreeType tree, bool linkedList, bool decorated, bool count)
        where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
            IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        var constants = new LinksConstants<T>(enableExternalReferencesSupport: true);
        using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096, constants, tree);
        using var separated = split ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(),
            4096, constants, tree, linkedList) : null;
        ILinks<T> raw = split ? separated! : united!;
        var links = decorated ? raw.DecorateWithAutomaticUniquenessAndUsagesResolution() : raw;
        var a = links.CreatePoint();
        var b = links.CreatePoint();
        var asymmetric = links.CreateAndUpdate(a, b);
        links.CreateAndUpdate(b, a);
        var external = new Hybrid<T>(T.CreateChecked(20), isExternal: true).Value;
        links.CreateAndUpdate(external, a);
        links.CreateAndUpdate(b, external);
        links.CreateAndUpdate(external, external);
        links.CreateAndUpdate(T.Zero, a);
        links.CreateAndUpdate(b, T.Zero);
        links.Create(); // A live record with two null endpoints.
        var deleted = links.CreatePoint();
        var last = links.CreatePoint(); // Keep the deleted address inside the allocated range.
        links.Update(deleted, T.Zero, T.Zero);
        links.Delete(deleted);
        var values = new[] { constants.Any, T.Zero, a, b, external, T.CreateChecked(50) };
        AssertQueries(links, values, deleted, count);

        // Recheck after changing endpoint indexes and recycling a deleted slot.
        links.Update(asymmetric, b, asymmetric);
        Assert.Equal(deleted, links.CreateAndUpdate(last, a));
        AssertQueries(links, values, T.CreateChecked(50), count);
    }

    private static void AssertQueries<T>(ILinks<T> links, T[] values, T deleted, bool count)
        where T : IUnsignedNumber<T>
    {
        var rows = new List<T[]>();
        links.Each(Array.Empty<T>(), link =>
        {
            rows.Add(link!.ToArray());
            return links.Constants.Continue;
        });
        foreach (var index in rows.Select(row => row[0]).Concat(new[] { T.Zero, deleted, T.CreateChecked(50), values[4] }))
        {
            var row = rows.SingleOrDefault(row => row[0] == index);
            foreach (var source in values)
            {
                foreach (var target in values)
                {
                    var matches = row != null && (source == links.Constants.Any || row[1] == source)
                        && (target == links.Constants.Any || row[2] == target);
                    var query = new[] { index, source, target };
                    if (count)
                    {
                        Assert.True(links.Count(query) == (matches ? T.One : T.Zero),
                            $"Count [{index}, {source}, {target}] should match {matches}");
                    }
                    else
                    {
                        AssertEach(links, query, row, matches);
                    }
                }
                // Two-part queries intentionally match either endpoint.
                var either = row != null && (source == links.Constants.Any || row[1] == source || row[2] == source);
                var shortQuery = new[] { index, source };
                if (count)
                {
                    Assert.Equal(either ? T.One : T.Zero, links.Count(shortQuery));
                }
                else
                {
                    AssertEach(links, shortQuery, row, either);
                }
            }
        }
    }

    private static void AssertEach<T>(ILinks<T> links, T[] query, T[]? expected, bool matches)
        where T : IUnsignedNumber<T>
    {
        foreach (var flow in new[] { links.Constants.Continue, links.Constants.Break })
        {
            var calls = 0;
            var result = links.Each(query, link =>
            {
                calls++;
                Assert.Equal(expected, link!.ToArray());
                return flow;
            });
            Assert.True(calls == (matches ? 1 : 0), $"Each [{string.Join(", ", query)}] should match {matches}");
            Assert.Equal(matches ? flow : links.Constants.Continue, result);
        }
        Assert.Equal(links.Constants.Continue, links.Each(query, null));
    }
}
