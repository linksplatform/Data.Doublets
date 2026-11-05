using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.Data.Doublets.Memory.Split.Generic;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Data.Doublets.Memory;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class FormatStructureTests
{
    // Keep allocations small and exercise both in-memory implementations.
    private static IDisposable CreateStore(bool split, out ILinks<ulong> links, bool external = false)
    {
        var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: external);
        links = split
            ? new SplitMemoryLinks<ulong>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096, constants)
            : new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory(), 4096, constants, IndexTreeType.Default);
        return (IDisposable)links;
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FormatsReportedSuperNameSequence(bool split)
    {
        using var store = CreateStore(split, out var links);
        // The same adjacent-pair layers as BalancedVariantConverter, without a dependency on Sequences.
        var layer = "super_name".Select(c => (ulong)c).ToArray();
        while (layer.Length > 1)
        {
            var next = new List<ulong>();
            for (var i = 0; i < layer.Length; i += 2)
                next.Add(i + 1 < layer.Length ? links.GetOrCreate(layer[i], layer[i + 1]) : layer[i]);
            layer = next.ToArray();
        }
        Assert.Equal(9UL, layer[0]);
        Assert.Equal("(9:(8:(6:(1:~115 ~117) (2:~112 ~101)) (7:(3:~114 ~95) (4:~110 ~97))) (5:~109 ~101))",
            links.FormatStructure(layer[0], _ => false, true, true));
        Assert.Equal("((((115 117) (112 101)) ((114 95) (110 97))) (109 101))",
            links.FormatStructure(layer[0], _ => false));
        Assert.Equal(9UL, links.Count());
        Assert.False(links.Exists(115UL));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void MissingEndpointsAndRootsAreNumericLeaves(bool split)
    {
        using var store = CreateStore(split, out var links);
        var root = links.CreateAndUpdate(100UL, 101UL);
        Func<Link<ulong>, bool> unexpected = _ => throw new InvalidOperationException("Only stored elements reach the predicate.");
        Assert.Equal("(1:~100 ~101)", links.FormatStructure(root, unexpected, true, true));
        Assert.Equal("(100 101)", links.FormatStructure(root, unexpected));
        Assert.Equal("~100", links.FormatStructure(100UL, unexpected, true, true));
        Assert.Equal("100", links.FormatStructure(100UL, unexpected));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void DeletedEndpointsAreRenderedWithoutDereferencing(bool split)
    {
        using var store = CreateStore(split, out var links);
        var deleted = links.CreatePoint();
        var root = links.CreateAndUpdate(deleted, deleted);
        links.Delete(deleted);
        Assert.Equal($"({root}:~{deleted} ~{deleted})", links.FormatStructure(root, _ => false, true, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SelfReferencesRemainPlainNumericLeaves(bool split)
    {
        using var store = CreateStore(split, out var links);
        var point = links.CreatePoint();
        Assert.Equal("(1:1 1)", links.FormatStructure(point, _ => false, true, true));
        Assert.Equal("(1 1)", links.FormatStructure(point, _ => false));
    }

    [Theory]
    [InlineData(false, false, false, "((1 1) 2)")]
    [InlineData(false, false, true, "((*1 *1) *2)")]
    [InlineData(false, true, false, "(1:(2:1 1) 2)")]
    [InlineData(false, true, true, "(1:(2:*1 *1) *2)")]
    [InlineData(true, false, false, "((1 1) 2)")]
    [InlineData(true, false, true, "((*1 *1) *2)")]
    [InlineData(true, true, false, "(1:(2:1 1) 2)")]
    [InlineData(true, true, true, "(1:(2:*1 *1) *2)")]
    public void CyclesTerminateAndPreserveNestedDebugOptions(bool split, bool renderIndex, bool renderDebug, string expected)
    {
        using var store = CreateStore(split, out var links);
        var one = links.Create();
        var two = links.CreateAndUpdate(one, one);
        links.Update(one, two, two);
        Assert.Equal(expected, links.FormatStructure(one, _ => false, renderIndex, renderDebug));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void SharedSubtreesExpandOnceAndElementCallbacksStaySupported(bool split)
    {
        using var store = CreateStore(split, out var links);
        var point = links.CreatePoint();
        var root = links.CreateAndUpdate(point, point);
        Assert.Equal("(2:(1:1 1) *1)", links.FormatStructure(root, _ => false, true, true));
        Assert.Equal("(2:1 1)", links.FormatStructure(root, _ => true, true, true));
        var calls = 0;
        Assert.Equal("(2:leaf leaf)", links.FormatStructure(root, _ => true, (sb, element) =>
        {
            Assert.Equal(point, element.Index);
            calls++;
            sb.Append("leaf");
        }, true, true));
        Assert.Equal(2, calls);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ReservedRootsAndNullEndpointsAreSkipped(bool split)
    {
        using var store = CreateStore(split, out var links, external: true);
        var root = links.Create();
        foreach (var value in new[] { links.Constants.Null, links.Constants.Any, links.Constants.Itself })
        {
            Assert.Equal("", links.FormatStructure(value, _ => false, true, true));
        }
        Assert.Equal("(1: )", links.FormatStructure(root, _ => false, true, true));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ExternalReferencesKeepTheirVirtualPointBehavior(bool split)
    {
        var constants = new LinksConstants<ulong>(enableExternalReferencesSupport: true);
        using var united = split ? null : new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory(), 4096, constants, IndexTreeType.Default);
        using var separated = split ? new SplitMemoryLinks<ulong>(new HeapResizableDirectMemory(), new HeapResizableDirectMemory(), 4096, constants) : null;
        ILinks<ulong> links = split ? separated! : united!;
        var external = new Hybrid<ulong>(20, isExternal: true).Value;
        var root = links.CreateAndUpdate(external, external);
        Assert.Equal($"({root}:({external}:{external} {external}) *{external})", links.FormatStructure(root, _ => false, true, true));
        Assert.Equal($"({root}:{external} {external})", links.FormatStructure(root, _ => true, true, true));
    }

    [Fact]
    public void AppendStructureUsesTheProvidedBuilderAndVisitedSet()
    {
        using var store = CreateStore(false, out var links);
        var point = links.CreatePoint();
        var sb = new StringBuilder("prefix:");
        var visited = new HashSet<ulong>();
        links.AppendStructure(sb, visited, point, _ => false, (text, link) => text.Append(link.Index), true, true);
        links.AppendStructure(sb, visited, point, _ => false, (text, link) => text.Append(link.Index), true, true);
        Assert.Equal("prefix:(1:1 1)*1", sb.ToString());
        Assert.Equal(new[] { point }, visited);
        Assert.Throws<ArgumentNullException>(() => links.AppendStructure(null!, visited, point, _ => false, (_, _) => { }));
    }
}
