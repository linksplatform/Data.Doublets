using System;
using System.Collections.Generic;
using System.Linq;
using Platform.Data.Doublets.Decorators;
using Platform.Data.Doublets.Memory.United.Generic;
using Platform.Delegates;
using Platform.Memory;
using Xunit;

namespace Platform.Data.Doublets.Tests;

public class BitArrayLinksIndexTests
{
    private sealed class Fixture : IDisposable
    {
        public readonly UnitedMemoryLinks<ulong> Store = new(new HeapResizableDirectMemory());
        public readonly BitArrayLinksIndex<ulong> Index;
        public Fixture() => Index = new(Store);
        public void Dispose() => Store.Dispose();
    }

    private static ulong[] Query(ILinks<ulong> links, ulong source, ulong target)
        => new[] { links.Constants.Any, source, target };

    private static string[] Rows(ILinks<ulong> links, IList<ulong> query)
    {
        var rows = new List<string>();
        links.Each(query, row =>
        {
            rows.Add(string.Join(",", row!));
            return links.Constants.Continue;
        });
        return rows.OrderBy(row => row, StringComparer.Ordinal).ToArray();
    }

    private static void AssertSame(ILinks<ulong> store, BitArrayLinksIndex<ulong> index, IList<ulong> query)
    {
        Assert.True(store.Count(query) == index.Count(query),
            $"Query {string.Join(',', query)}: store count {store.Count(query)}, index count {index.Count(query)}; " +
            $"store matches {string.Join(';', Rows(store, query))}; index matches {string.Join(';', Rows(index, query))}; " +
            $"all stored {string.Join(';', Rows(store, Array.Empty<ulong>()))}");
        Assert.Equal(Rows(store, query), Rows(index, query));
        var expected = store.Each(query, _ => store.Constants.Continue);
        Assert.Equal(expected, index.Each(query, _ => store.Constants.Continue));
    }

    [Fact]
    public void BuildsFromExistingStorageAndPreservesAllRestrictionShapes()
    {
        using var store = new UnitedMemoryLinks<ulong>(new HeapResizableDirectMemory());
        var one = store.CreateAndUpdate<ulong>(11, 21);
        var two = store.CreateAndUpdate<ulong>(11, 22);
        store.CreateAndUpdate<ulong>(21, 11);
        store.CreateAndUpdate<ulong>(11, 11);
        store.Create();
        var index = new BitArrayLinksIndex<ulong>(store);
        var any = store.Constants.Any;
        var queries = new[]
        {
            Array.Empty<ulong>(), new[] { any }, new[] { one }, new[] { 999UL },
            new[] { any, 11UL }, new[] { one, 11UL }, new[] { any, any },
            Query(store, 11, any), Query(store, any, 11), Query(store, 99, any),
            Query(store, any, 99), Query(store, 0, any), Query(store, any, 0),
            Query(store, 11, 21), Query(store, any, any),
            new[] { two, 11UL, 21UL }, new[] { one, any, 11UL },
            new[] { one, 21UL, any }, new[] { 999UL, 11UL, any }
        };
        foreach (var query in queries) AssertSame(store, index, query);
        Assert.Equal(5, index.IndexedLinkCount);
    }

    [Theory]
    [InlineData(BitArrayIndexReadMode.Always)]
    [InlineData(BitArrayIndexReadMode.Never)]
    [InlineData(BitArrayIndexReadMode.WhenSelective)]
    public void MaintainsCrudInEveryReadMode(BitArrayIndexReadMode mode)
    {
        using var f = new Fixture();
        f.Index.ReadMode = mode;
        var any = f.Store.Constants.Any;
        var ids = new List<ulong>();
        for (var i = 0; i < 20; i++) ids.Add(f.Index.CreateAndUpdate<ulong>((ulong)(i % 4 + 1), (ulong)(i + 100)));
        foreach (var id in ids.Where(id => id % 2 == 0)) f.Index.Update<ulong>(id, 7, id + 1000);
        foreach (var id in ids.Where(id => id % 3 == 0)) Platform.Data.Doublets.ILinksExtensions.Delete(f.Index, id, null);
        f.Index.CreateAndUpdate<ulong>(7, 999);
        foreach (var endpoint in new[] { 1UL, 2UL, 3UL, 4UL, 7UL, 999UL, 101UL })
        {
            AssertSame(f.Store, f.Index, Query(f.Store, endpoint, any));
            AssertSame(f.Store, f.Index, Query(f.Store, any, endpoint));
        }
        Assert.Equal((int)f.Store.Count(Array.Empty<ulong>()), f.Index.IndexedLinkCount);
        f.Index.ReadMode = BitArrayIndexReadMode.Always;
        AssertSame(f.Store, f.Index, Query(f.Store, 7, any));
    }

    private sealed class ReadCounter : LinksDecoratorBase<ulong>
    {
        public int Reads;
        public ReadCounter(ILinks<ulong> links) : base(links) { }
        public override ulong Count(IList<ulong>? restriction) { Reads++; return base.Count(restriction); }
        public override ulong Each(IList<ulong>? restriction, ReadHandler<ulong>? handler)
        { Reads++; return base.Each(restriction, handler); }
    }

    [Fact]
    public void ReadModesActuallyRouteQueriesAndKeepMaintenanceEnabled()
    {
        using var f = new Fixture();
        for (ulong i = 1; i <= 5; i++) f.Index.CreateAndUpdate<ulong>(1, i + 10);
        var counter = new ReadCounter(f.Store);
        var index = new BitArrayLinksIndex<ulong>(counter);
        var query = Query(index, 1, index.Constants.Any);
        counter.Reads = 0;
        Assert.Equal(5UL, index.Count(query));
        Assert.Equal(5, Rows(index, query).Length);
        Assert.Equal(0, counter.Reads);
        index.ReadMode = BitArrayIndexReadMode.Never;
        index.Count(query);
        Assert.Equal(1, counter.Reads);
        index.ReadMode = BitArrayIndexReadMode.WhenSelective;
        index.Count(query);
        Assert.Equal(2, counter.Reads);
        Assert.Equal(0UL, index.Count(Query(index, 999, index.Constants.Any)));
        Assert.Equal(2, counter.Reads);
    }

    [Fact]
    public void ClearAndRebuildNeverHideStoredData()
    {
        using var f = new Fixture();
        f.Index.CreateAndUpdate<ulong>(11, 22);
        f.Index.ClearIndex();
        Assert.Equal(0, f.Index.IndexedLinkCount);
        Assert.Equal(0, f.Index.BitmapPageCount);
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
        f.Store.CreateAndUpdate<ulong>(11, 33);
        f.Index.RebuildIndex();
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
        Assert.Equal(2, f.Index.IndexedLinkCount);
    }

    [Fact]
    public void CreateUsesActualStoredValuesAndPreservesCallbackFlow()
    {
        using var f = new Fixture();
        ulong created = 0;
        var result = f.Index.Create(new ulong[] { 0, 11, 22 }, (before, after) =>
        {
            Assert.Null(before);
            created = after![0];
            Assert.Equal(new[] { created, 0UL, 0UL }, after);
            return f.Index.Constants.Break;
        });
        Assert.Equal(f.Store.Constants.Break, result);
        Assert.NotEqual(0UL, created);
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
        Assert.Equal(1, f.Index.IndexedLinkCount);
        Assert.Equal(f.Store.Constants.Continue, f.Index.Update(new[] { created }, new[] { created, 11UL, 22UL }, null));
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
    }

    [Fact]
    public void UpdateAndDeleteForwardUnchangedImagesAndBreak()
    {
        using var f = new Fixture();
        var id = f.Index.CreateAndUpdate<ulong>(11, 22);
        Assert.Equal(f.Store.Constants.Break, f.Index.Update(new[] { id }, new[] { id, 33UL, 44UL }, (before, after) =>
        {
            Assert.Equal(new[] { id, 11UL, 22UL }, before);
            Assert.Equal(new[] { id, 33UL, 44UL }, after);
            return f.Store.Constants.Break;
        }));
        AssertSame(f.Store, f.Index, Query(f.Store, 33, f.Store.Constants.Any));
        f.Index.Update<ulong>(id, 0, 0);
        Assert.Equal(f.Store.Constants.Break, f.Index.Delete(new[] { id }, (before, after) =>
        {
            Assert.Equal(new[] { id, 0UL, 0UL }, before);
            Assert.Null(after);
            return f.Store.Constants.Break;
        }));
        Assert.Equal(0, f.Index.IndexedLinkCount);
        Assert.Equal(0, f.Index.BitmapPageCount);
    }

    [Fact]
    public void CallbackExceptionLeavesCommittedWriteVisible()
    {
        using var f = new Fixture();
        var id = f.Index.CreateAndUpdate<ulong>(11, 22);
        var error = new InvalidOperationException("user callback");
        Assert.Same(error, Assert.Throws<InvalidOperationException>(() =>
            f.Index.Update(new[] { id }, new[] { id, 33UL, 44UL }, (_, _) => throw error)));
        AssertSame(f.Store, f.Index, Query(f.Store, 33, f.Store.Constants.Any));
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
    }

    [Fact]
    public void WriteCallbacksCanReadAndPerformNestedWrites()
    {
        using var f = new Fixture();
        var id = f.Index.CreateAndUpdate<ulong>(11, 22);
        var other = f.Index.CreateAndUpdate<ulong>(11, 23);
        f.Index.Update(new[] { id }, new[] { id, 33UL, 44UL }, (_, _) =>
        {
            AssertSame(f.Store, f.Index, Query(f.Store, 33, f.Store.Constants.Any));
            f.Index.Update<ulong>(other, 33, 55);
            return f.Store.Constants.Continue;
        });
        AssertSame(f.Store, f.Index, Query(f.Store, 33, f.Store.Constants.Any));
        Assert.Equal(2UL, f.Index.Count(Query(f.Store, 33, f.Store.Constants.Any)));
    }

    [Fact]
    public void IndexedEachStopsOnBreakAndRejectsMutationBeforeChangingStorage()
    {
        using var f = new Fixture();
        var id = f.Index.CreateAndUpdate<ulong>(11, 22);
        f.Index.CreateAndUpdate<ulong>(11, 33);
        var visited = 0;
        var query = Query(f.Store, 11, f.Store.Constants.Any);
        Assert.Equal(f.Store.Constants.Break, f.Index.Each(query, _ =>
        {
            visited++;
            return f.Store.Constants.Break;
        }));
        Assert.Equal(1, visited);
        var before = Rows(f.Store, Array.Empty<ulong>());
        f.Index.Each(query, _ =>
        {
            Assert.Throws<InvalidOperationException>(() => f.Index.Update<ulong>(id, 77, 88));
            Assert.Throws<InvalidOperationException>(() => f.Index.ClearIndex());
            Assert.Throws<InvalidOperationException>(() => f.Index.RebuildIndex());
            return f.Store.Constants.Continue;
        });
        Assert.Equal(before, Rows(f.Store, Array.Empty<ulong>()));
    }

    private sealed class BatchUpdater : LinksDecoratorBase<ulong>
    {
        public bool OmitCallbacks;
        public bool ThrowAfterWrite;
        public bool ReverseImages;
        public BatchUpdater(ILinks<ulong> links) : base(links) { }
        public override ulong Update(IList<ulong>? restriction, IList<ulong>? substitution, WriteHandler<ulong>? handler)
        {
            var ids = new List<ulong>();
            Links.Each(restriction, row => { ids.Add(row![0]); return Constants.Continue; });
            foreach (var id in ids)
            {
                // Keep each pair unique: the native tree requires this invariant.
                var result = Links.Update(new[] { id }, new[] { id, substitution![1], substitution[2] + id }, (before, after) =>
                    OmitCallbacks || handler == null ? Constants.Continue :
                    ReverseImages ? handler(after, before) : handler(before, after));
                if (ThrowAfterWrite) throw new InvalidOperationException("storage failed after commit");
                if (result == Constants.Break) return result;
            }
            return Constants.Continue;
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void BatchWritesReconcileAllActualRows(bool omitCallbacks, bool reverseImages)
    {
        using var f = new Fixture();
        for (ulong i = 1; i < 5; i++) f.Index.CreateAndUpdate<ulong>(11, i);
        var store = new BatchUpdater(f.Store) { OmitCallbacks = omitCallbacks, ReverseImages = reverseImages };
        var index = new BitArrayLinksIndex<ulong>(store);
        index.Update(Query(store, 11, store.Constants.Any), new ulong[] { 0, 33, 44 }, (_, _) => store.Constants.Continue);
        AssertSame(f.Store, index, Query(store, 11, store.Constants.Any));
        AssertSame(f.Store, index, Query(store, 33, store.Constants.Any));
        Assert.Equal(4UL, index.Count(Query(store, 33, store.Constants.Any)));
    }

    [Fact]
    public void PartialWriteFailureIsNotHiddenByStaleIndex()
    {
        using var f = new Fixture();
        f.Index.CreateAndUpdate<ulong>(11, 22);
        f.Index.CreateAndUpdate<ulong>(11, 23);
        var store = new BatchUpdater(f.Store) { ThrowAfterWrite = true };
        var index = new BitArrayLinksIndex<ulong>(store);
        Assert.Throws<InvalidOperationException>(() => index.Update(Query(store, 11, store.Constants.Any),
            new ulong[] { 0, 33, 44 }, (_, _) => store.Constants.Continue));
        AssertSame(f.Store, index, Query(store, 11, store.Constants.Any));
        AssertSame(f.Store, index, Query(store, 33, store.Constants.Any));
    }

    [Fact]
    public void PagedBitmapsHandleSeveralPagesAndReuseFreedSlots()
    {
        using var f = new Fixture();
        var ids = Enumerable.Range(1, 600).Select(i => f.Index.CreateAndUpdate<ulong>(11, (ulong)i + 1000)).ToArray();
        Assert.Equal(603, f.Index.BitmapPageCount); // 3 shared-source pages + 600 targets.
        foreach (var id in ids.Where((_, i) => i % 2 == 0)) Platform.Data.Doublets.ILinksExtensions.Delete(f.Index, id, null);
        for (var i = 0; i < 300; i++) f.Index.CreateAndUpdate<ulong>(11, (ulong)i + 5000);
        Assert.Equal(600, f.Index.IndexedLinkCount);
        Assert.Equal(603, f.Index.BitmapPageCount);
        AssertSame(f.Store, f.Index, Query(f.Store, 11, f.Store.Constants.Any));
    }

    [Fact]
    public void LargeExternalAddressesNeverDetermineBitmapSize()
    {
        using var f = new Fixture();
        f.Index.CreateAndUpdate<ulong>(ulong.MaxValue, ulong.MaxValue - 1);
        AssertSame(f.Store, f.Index, Query(f.Store, ulong.MaxValue, f.Store.Constants.Any));
        AssertSame(f.Store, f.Index, Query(f.Store, f.Store.Constants.Any, ulong.MaxValue - 1));
        Assert.Equal(2, f.Index.BitmapPageCount);
    }

    [Fact]
    public void SupportsSmallAddressTypesWithoutConvertingPageSizeToAnAddress()
    {
        using var store = new UnitedMemoryLinks<byte>(new HeapResizableDirectMemory());
        var index = new BitArrayLinksIndex<byte>(store);
        index.CreateAndUpdate<byte>((byte)11, (byte)12);
        Assert.Equal((byte)1, index.Count(new byte[] { store.Constants.Any, 11, store.Constants.Any }));
    }

    [Fact]
    public void InvalidConfigurationFailsExplicitly()
    {
        using var f = new Fixture();
        Assert.Throws<ArgumentNullException>(() => new BitArrayLinksIndex<ulong>(null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => f.Index.ReadMode = (BitArrayIndexReadMode)99);
    }

    [Fact]
    public void BatchBreakKeepsOnlyCommittedRowsInIndex()
    {
        using var f = new Fixture();
        for (ulong i = 1; i < 5; i++) f.Index.CreateAndUpdate<ulong>(11, i);
        var store = new BatchUpdater(f.Store);
        var index = new BitArrayLinksIndex<ulong>(store);
        Assert.Equal(store.Constants.Break, index.Update(Query(store, 11, store.Constants.Any),
            new ulong[] { 0, 33, 44 }, (_, _) => store.Constants.Break));
        Assert.Equal(1UL, index.Count(Query(store, 33, store.Constants.Any)));
        AssertSame(f.Store, index, Query(store, 11, store.Constants.Any));
        AssertSame(f.Store, index, Query(store, 33, store.Constants.Any));
    }

    [Fact]
    public void SeededCrudMatchesTreeIndexesAfterEveryOperation()
    {
        using var f = new Fixture();
        var random = new System.Random(240);
        var ids = new List<ulong>();
        for (var i = 0; i < 150; i++)
        {
            var operation = random.Next(3);
            if (ids.Count == 0 || operation == 0)
                ids.Add(f.Index.CreateAndUpdate<ulong>((ulong)random.Next(1, 9), (ulong)i + 1000));
            else
            {
                var at = random.Next(ids.Count);
                if (operation == 1)
                    f.Index.Update<ulong>(ids[at], (ulong)random.Next(1, 9), (ulong)i + 1000);
                else
                {
                    Platform.Data.Doublets.ILinksExtensions.Delete(f.Index, ids[at], null);
                    ids.RemoveAt(at);
                }
            }
            foreach (var source in Enumerable.Range(1, 8))
                AssertSame(f.Store, f.Index, Query(f.Store, (ulong)source, f.Store.Constants.Any));
            Assert.Equal(ids.Count, f.Index.IndexedLinkCount);
        }
    }

    // A read-only adapter with sparse 64-bit IDs exercises address-to-slot
    // mapping independently of a native store's contiguous allocation policy.
    private sealed class SparseRows : LinksDecoratorBase<ulong>
    {
        private readonly ulong[][] _values =
        {
            new[] { 1UL << 40, 11UL, 22UL },
            new[] { 1UL << 60, 11UL, 33UL }
        };
        public SparseRows(ILinks<ulong> constantsProvider) : base(constantsProvider) { }
        public override ulong Each(IList<ulong>? restriction, ReadHandler<ulong>? handler)
        {
            foreach (var row in _values)
                if (restriction!.Count == 0 || (restriction.Count == 1 && restriction[0] == row[0]))
                    if (handler!(row) == Constants.Break) return Constants.Break;
            return Constants.Continue;
        }
    }

    [Fact]
    public void SparseLargeLinkIdsUseThreeSmallPages()
    {
        using var f = new Fixture();
        var index = new BitArrayLinksIndex<ulong>(new SparseRows(f.Store));
        var query = Query(index, 11, index.Constants.Any);
        Assert.Equal(2UL, index.Count(query));
        Assert.Equal(new[] { $"{1UL << 40},11,22", $"{1UL << 60},11,33" }, Rows(index, query));
        Assert.Equal(3, index.BitmapPageCount); // one source page, two target pages
    }

    private sealed class NullHandlerSensitive : LinksDecoratorBase<ulong>
    {
        public NullHandlerSensitive(ILinks<ulong> links) : base(links) { }
        public override ulong Create(IList<ulong>? substitution, WriteHandler<ulong>? handler)
        {
            base.Create(substitution, handler);
            return handler == null ? 123UL : 456UL;
        }
    }

    [Fact]
    public void NullWriteHandlerAndStorageReturnValueArePreserved()
    {
        using var f = new Fixture();
        var index = new BitArrayLinksIndex<ulong>(new NullHandlerSensitive(f.Store));
        Assert.Equal(123UL, index.Create(null, null));
        Assert.Equal(1, index.IndexedLinkCount);
        Assert.Equal(456UL, index.Create(null, (_, _) => index.Constants.Continue));
        Assert.Equal(2, index.IndexedLinkCount);
    }

    [Fact]
    public void RealDependencyResolverSilentCreatesRemainIndexed()
    {
        using var f = new Fixture();
        var selfReferences = new LinksNullConstantToSelfReferenceResolver<ulong>(f.Store);
        var dependencies = new LinksNonExistentDependenciesCreator<ulong>(selfReferences);
        var index = new BitArrayLinksIndex<ulong>(dependencies);
        var one = index.Create();
        Assert.Equal(1UL, one);
        index.Update(new[] { one }, new[] { one, 2UL, one }, (_, _) => index.Constants.Continue);
        // The dependency decorator emits no outer callback for newly-created
        // point 2. Both point 2 and updated point 1 must appear in source 2.
        Assert.Equal(new[] { "1,2,1", "2,2,2" }, Rows(f.Store, Array.Empty<ulong>()));
        AssertSame(f.Store, index, Query(index, 2, index.Constants.Any));
        Assert.Equal(2UL, index.Count(Query(index, 2, index.Constants.Any)));
        Assert.Equal(2, index.IndexedLinkCount);
    }
}
