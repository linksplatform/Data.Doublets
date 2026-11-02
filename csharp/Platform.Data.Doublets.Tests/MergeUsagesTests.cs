using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

// Issue #515: re-pointing a usage must replace only the endpoint naming the merged link.
public class MergeUsagesTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeUsagesRewritesOnlyTheMergedEndpoint(bool split)
    {
        Check<byte>(split, Merge.Usages);
        Check<ushort>(split, Merge.Usages);
        Check<uint>(split, Merge.Usages);
        Check<ulong>(split, Merge.Usages);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeUsagesWithHandlerRewritesOnlyTheMergedEndpoint(bool split)
    {
        Check<byte>(split, Merge.UsagesWithHandler);
        Check<ulong>(split, Merge.UsagesWithHandler);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MergeAndDeleteRewritesOnlyTheMergedEndpoint(bool split)
    {
        Check<uint>(split, Merge.AndDelete);
        Check<uint>(split, Merge.AndDeleteWithHandler);
    }

    [Fact]
    public void MergeUsagesReportsEveryRewrittenLink()
    {
        using var links = new UnitedMemoryLinks<uint>(new HeapResizableDirectMemory(), 4096);
        var (one, two, three, asSource, asTarget, asBoth) = CreateIssueLinks(links);
        var changes = new List<string>();
        var result = links.MergeUsages(one, two, (before, after) =>
        {
            changes.Add($"({string.Join(" ", before!)}) -> ({string.Join(" ", after!)})");
            return links.Constants.Continue;
        });
        Assert.Equal(links.Constants.Continue, result);
        // A link using the merged link on both ends is rewritten once per endpoint.
        Assert.Equal(new[]
        {
            $"({asSource} {one} {three}) -> ({asSource} {two} {three})",
            $"({asBoth} {one} {one}) -> ({asBoth} {two} {one})",
            $"({asTarget} {three} {one}) -> ({asTarget} {three} {two})",
            $"({asBoth} {two} {one}) -> ({asBoth} {two} {two})",
        }.Order(), changes.Order());
    }

    private enum Merge { Usages, UsagesWithHandler, AndDelete, AndDeleteWithHandler }

    private static void Check<T>(bool split, Merge merge)
        where T : IUnsignedNumber<T>, IShiftOperators<T, int, T>,
            IBitwiseOperators<T, T, T>, IMinMaxValue<T>, IComparisonOperators<T, T, bool>
    {
        using var united = split ? null : new UnitedMemoryLinks<T>(new HeapResizableDirectMemory(), 4096);
        using var separated = split ? new SplitMemoryLinks<T>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096) : null;
        ILinks<T> links = split ? separated! : united!;
        var (one, two, three, asSource, asTarget, asBoth) = CreateIssueLinks(links);
        var continueResult = links.Constants.Continue;
        switch (merge)
        {
            case Merge.Usages:
                links.MergeUsages(one, two);
                break;
            case Merge.UsagesWithHandler:
                Assert.Equal(continueResult, links.MergeUsages(one, two, (_, _) => continueResult));
                break;
            case Merge.AndDelete:
                Assert.Equal(two, links.MergeAndDelete(one, two));
                break;
            case Merge.AndDeleteWithHandler:
                Assert.Equal(continueResult, links.MergeAndDelete(one, two, (_, _) => continueResult));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(merge), merge, null);
        }
        var deleted = merge is Merge.AndDelete or Merge.AndDeleteWithHandler;
        Assert.Equal(!deleted, links.Exists(one));
        if (!deleted)
        {
            Assert.Equal(new[] { one, T.Zero, T.Zero }, links.GetLink(one));
        }
        Assert.Equal(new[] { two, T.Zero, T.Zero }, links.GetLink(two));
        Assert.Equal(new[] { three, T.Zero, T.Zero }, links.GetLink(three));
        // The issue expects (4: 1 3) -> (4: 2 3) and (5: 3 1) -> (5: 3 2), not (4: 3 0) and (5: 2 0).
        Assert.Equal(new[] { asSource, two, three }, links.GetLink(asSource));
        Assert.Equal(new[] { asTarget, three, two }, links.GetLink(asTarget));
        Assert.Equal(new[] { asBoth, two, two }, links.GetLink(asBoth));
        Assert.Equal(T.CreateTruncating(deleted ? 5 : 6), links.Count());
        Assert.Equal(T.Zero, links.Count(new Link<T>(links.Constants.Any, one, links.Constants.Any)));
        Assert.Equal(T.Zero, links.Count(new Link<T>(links.Constants.Any, links.Constants.Any, one)));
    }

    private static (T One, T Two, T Three, T AsSource, T AsTarget, T AsBoth) CreateIssueLinks<T>(ILinks<T> links)
        where T : IUnsignedNumber<T>
    {
        var nothing = links.Constants.Null;
        var one = links.CreateAndUpdate(nothing, nothing);
        var two = links.CreateAndUpdate(nothing, nothing);
        var three = links.CreateAndUpdate(nothing, nothing);
        var asSource = links.CreateAndUpdate(one, three);
        var asTarget = links.CreateAndUpdate(three, one);
        var asBoth = links.CreateAndUpdate(one, one);
        Assert.Equal(new[] { asSource, one, three }, links.GetLink(asSource));
        Assert.Equal(new[] { asTarget, three, one }, links.GetLink(asTarget));
        return (one, two, three, asSource, asTarget, asBoth);
    }
}
