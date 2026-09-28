using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators;

/// <summary>Chooses when supported endpoint queries use the additional index.</summary>
public enum BitArrayIndexReadMode
{
    /// <summary>Use the index for every supported endpoint query.</summary>
    Always,
    /// <summary>Maintain the index, but send all reads to the decorated storage.</summary>
    Never,
    /// <summary>Use the index only when at most half of the stored links match.</summary>
    WhenSelective
}

/// <summary>
/// Adds sparse, paged BitArray source and target indexes to a links storage.
/// </summary>
/// <remarks>
/// Indexes restrictions (Any, source, Any) and (Any, Any, target), with a
/// non-null, non-Any endpoint. Other restriction shapes retain the underlying
/// storage's semantics. All writes must pass through this decorator. Call
/// RebuildIndex after writing through another reference. The underlying storage
/// remains owned by its caller. Indexed enumeration order is unspecified.
/// </remarks>
public sealed class BitArrayLinksIndex<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
    where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly object _gate = new();
    private readonly Dictionary<TLinkAddress, int> _slots = new();
    private readonly List<Link<TLinkAddress>?> _rows = new();
    private readonly Stack<int> _freeSlots = new();
    private readonly Dictionary<TLinkAddress, Bitmap> _sources = new();
    private readonly Dictionary<TLinkAddress, Bitmap> _targets = new();
    private BitArrayIndexReadMode _readMode;
    private bool _valid;
    private int _writeDepth;
    private int _readDepth;

    /// <summary>Builds the additional index, including links already in storage.</summary>
    public BitArrayLinksIndex(ILinks<TLinkAddress> links,
        BitArrayIndexReadMode readMode = BitArrayIndexReadMode.Always)
        : base(links ?? throw new ArgumentNullException(nameof(links)))
    {
        ReadMode = readMode;
        RebuildIndex();
    }

    /// <summary>Changes read routing without disabling index maintenance.</summary>
    public BitArrayIndexReadMode ReadMode
    {
        get { lock (_gate) return _readMode; }
        set
        {
            if (!Enum.IsDefined(value)) throw new ArgumentOutOfRangeException(nameof(value));
            lock (_gate) _readMode = value;
        }
    }

    /// <summary>Number of links in the current index (zero after ClearIndex).</summary>
    public int IndexedLinkCount { get { lock (_gate) return _slots.Count; } }

    /// <summary>Allocated 256-bit pages across both endpoint indexes.</summary>
    public int BitmapPageCount
    {
        get
        {
            lock (_gate)
            {
                var count = 0;
                foreach (var bitmap in _sources.Values) count += bitmap.PageCount;
                foreach (var bitmap in _targets.Values) count += bitmap.PageCount;
                return count;
            }
        }
    }

    /// <summary>Releases the index. The next eligible read rebuilds it from storage.</summary>
    public void ClearIndex()
    {
        lock (_gate)
        {
            if (_readDepth != 0)
                throw new InvalidOperationException("Cannot clear the index during indexed enumeration.");
            _valid = false;
            _slots.Clear();
            _rows.Clear();
            _freeSlots.Clear();
            _sources.Clear();
            _targets.Clear();
        }
    }

    /// <summary>Rebuilds the index from the decorated storage.</summary>
    public void RebuildIndex()
    {
        lock (_gate)
        {
            if (_writeDepth != 0)
                throw new InvalidOperationException("Cannot rebuild the index during a write callback.");
            ClearIndex();
            _links.Each(Array.Empty<TLinkAddress>(), row =>
            {
                Add(row);
                return _constants.Continue;
            });
            _valid = true;
        }
    }

    /// <inheritdoc />
    public override TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        lock (_gate)
        {
            return TryGetBitmap(restriction, out var bitmap)
                ? TLinkAddress.CreateChecked(bitmap?.Count ?? 0)
                : _links.Count(restriction);
        }
    }

    /// <inheritdoc />
    public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        lock (_gate)
        {
            // Preserve the decorated storage's handling of a null handler.
            if (handler == null || !TryGetBitmap(restriction, out var bitmap))
                return _links.Each(restriction, handler);
            _readDepth++;
            try
            {
                if (bitmap != null)
                {
                    foreach (var slot in bitmap.Slots())
                    {
                        if (handler(_rows[slot]!.Value) == _constants.Break)
                            return _constants.Break;
                    }
                }
                return _constants.Continue;
            }
            finally { _readDepth--; }
        }
    }

    /// <inheritdoc />
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        => Write(() => _links.Create(substitution, handler));

    /// <inheritdoc />
    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution,
        WriteHandler<TLinkAddress>? handler)
        => Write(() => _links.Update(restriction, substitution, handler));

    /// <inheritdoc />
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        => Write(() => _links.Delete(restriction, handler));

    private TLinkAddress Write(Func<TLinkAddress> operation)
    {
        lock (_gate)
        {
            // Native tree traversals also cannot safely mutate their own tree.
            // Fail before a write rather than enumerating invalidated bitmaps.
            if (_readDepth != 0)
                throw new InvalidOperationException("Cannot write during indexed enumeration. Collect link IDs first.");
            _writeDepth++;
            _valid = false;
            var completed = false;
            try
            {
                var result = operation();
                completed = true;
                return result;
            }
            finally
            {
                _writeDepth--;
                // Existing decorators can change rows without reporting those
                // changes to the supplied handler. Callbacks and cardinality
                // therefore cannot establish which index memberships changed.
                // Rebuild from the actual final store after an outer write.
                // On failure retain the original exception and rebuild lazily.
                if (_writeDepth == 0 && completed) RebuildIndex();
            }
        }
    }

    private bool TryGetBitmap(IList<TLinkAddress>? restriction, out Bitmap? bitmap)
    {
        bitmap = null;
        if (_writeDepth != 0 || _readMode == BitArrayIndexReadMode.Never || restriction?.Count != 3
            || restriction[_constants.IndexPart] != _constants.Any) return false;
        var source = restriction[_constants.SourcePart];
        var target = restriction[_constants.TargetPart];
        var bySource = source != _constants.Any && source != _constants.Null && target == _constants.Any;
        var byTarget = target != _constants.Any && target != _constants.Null && source == _constants.Any;
        if (!bySource && !byTarget) return false;
        if (!_valid) RebuildIndex();
        (bySource ? _sources : _targets).TryGetValue(bySource ? source : target, out bitmap);
        return _readMode != BitArrayIndexReadMode.WhenSelective || (bitmap?.Count ?? 0) <= _slots.Count / 2;
    }

    private void Add(IList<TLinkAddress>? row)
    {
        if (row == null || row.Count != 3)
            throw new InvalidOperationException("The decorated storage returned an invalid link.");
        var id = row[_constants.IndexPart];
        Remove(id);
        var slot = _freeSlots.Count != 0 ? _freeSlots.Pop() : _rows.Count;
        var value = new Link<TLinkAddress>(id, row[_constants.SourcePart], row[_constants.TargetPart]);
        if (slot == _rows.Count) _rows.Add(value); else _rows[slot] = value;
        _slots.Add(id, slot);
        AddTo(_sources, value.Source, slot);
        AddTo(_targets, value.Target, slot);
    }

    private static void AddTo(Dictionary<TLinkAddress, Bitmap> index, TLinkAddress endpoint, int slot)
    {
        if (!index.TryGetValue(endpoint, out var bitmap)) index.Add(endpoint, bitmap = new Bitmap());
        bitmap.Set(slot);
    }

    private void Remove(TLinkAddress id)
    {
        if (!_slots.Remove(id, out var slot)) return;
        var value = _rows[slot]!.Value;
        RemoveFrom(_sources, value.Source, slot);
        RemoveFrom(_targets, value.Target, slot);
        _rows[slot] = null;
        _freeSlots.Push(slot);
    }

    private static void RemoveFrom(Dictionary<TLinkAddress, Bitmap> index, TLinkAddress endpoint, int slot)
    {
        var bitmap = index[endpoint];
        bitmap.Clear(slot);
        if (bitmap.Count == 0) index.Remove(endpoint);
    }

    // Slots are dense, recycled ordinals, never converted link addresses. Sparse
    // pages avoid allocating an N-bit array for each of N distinct endpoints.
    private sealed class Bitmap
    {
        private const int PageSize = 256;
        private readonly Dictionary<int, (BitArray Bits, int Count)> _pages = new();
        public int Count { get; private set; }
        public int PageCount => _pages.Count;

        public void Set(int slot)
        {
            var pageId = slot / PageSize;
            if (!_pages.TryGetValue(pageId, out var page)) page = (new BitArray(PageSize), 0);
            if (page.Bits[slot % PageSize]) return;
            page.Bits[slot % PageSize] = true;
            _pages[pageId] = (page.Bits, page.Count + 1);
            Count++;
        }

        public void Clear(int slot)
        {
            var pageId = slot / PageSize;
            var page = _pages[pageId];
            page.Bits[slot % PageSize] = false;
            if (page.Count == 1) _pages.Remove(pageId);
            else _pages[pageId] = (page.Bits, page.Count - 1);
            Count--;
        }

        public IEnumerable<int> Slots()
        {
            foreach (var page in _pages)
                for (var bit = 0; bit < PageSize; bit++)
                    if (page.Value.Bits[bit]) yield return page.Key * PageSize + bit;
        }
    }
}
