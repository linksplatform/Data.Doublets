using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents a BitArray-based index decorator for links storage that provides fast search capabilities.
///     </para>
///     <para>
///         This decorator maintains BitArray indexes for source and target link relationships to enable efficient querying.
///     </para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class BitArrayLinksIndex<TLinkAddress> : LinksDecoratorBase<TLinkAddress> 
    where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
{
    private readonly Dictionary<TLinkAddress, BitArray> _sourceIndexes;
    private readonly Dictionary<TLinkAddress, BitArray> _targetIndexes;
    private readonly bool _useIndexForReads;
    private readonly bool _maintainIndexOnWrites;
    private readonly object _lockObject;
    private TLinkAddress _maxLinkAddress;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="BitArrayLinksIndex{TLinkAddress}" /> instance.
    ///     </para>
    /// </summary>
    /// <param name="links">The underlying links storage.</param>
    /// <param name="useIndexForReads">Whether to use the index for read operations.</param>
    /// <param name="maintainIndexOnWrites">Whether to maintain the index on write operations.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public BitArrayLinksIndex(ILinks<TLinkAddress> links, bool useIndexForReads = true, bool maintainIndexOnWrites = true)
        : base(links)
    {
        _sourceIndexes = new Dictionary<TLinkAddress, BitArray>();
        _targetIndexes = new Dictionary<TLinkAddress, BitArray>();
        _useIndexForReads = useIndexForReads;
        _maintainIndexOnWrites = maintainIndexOnWrites;
        _lockObject = new object();
        _maxLinkAddress = TLinkAddress.Zero;
        
        if (_maintainIndexOnWrites)
        {
            InitializeIndexes();
        }
    }

    /// <summary>
    ///     Gets a value indicating whether the index is used for read operations.
    /// </summary>
    public bool UseIndexForReads => _useIndexForReads;

    /// <summary>
    ///     Gets a value indicating whether the index is maintained on write operations.
    /// </summary>
    public bool MaintainIndexOnWrites => _maintainIndexOnWrites;

    /// <summary>
    ///     Initializes the indexes by scanning all existing links.
    /// </summary>
    private void InitializeIndexes()
    {
        lock (_lockObject)
        {
            // Find the maximum link address to size our BitArrays appropriately
            _maxLinkAddress = TLinkAddress.Zero;
            _links.Each(null, link =>
            {
                var linkAddress = link[_constants.IndexPart];
                if (linkAddress > _maxLinkAddress)
                {
                    _maxLinkAddress = linkAddress;
                }
                return _constants.Continue;
            });

            if (_maxLinkAddress == TLinkAddress.Zero)
            {
                return; // No links exist yet
            }

            var maxIndex = Convert.ToInt32(_maxLinkAddress) + 1;

            // Build source and target indexes
            _links.Each(null, link =>
            {
                var linkAddress = link[_constants.IndexPart];
                var source = link[_constants.SourcePart];
                var target = link[_constants.TargetPart];

                AddToIndex(_sourceIndexes, source, linkAddress, maxIndex);
                AddToIndex(_targetIndexes, target, linkAddress, maxIndex);

                return _constants.Continue;
            });
        }
    }

    /// <summary>
    ///     Adds a link to the specified index.
    /// </summary>
    private void AddToIndex(Dictionary<TLinkAddress, BitArray> index, TLinkAddress key, TLinkAddress linkAddress, int maxIndex)
    {
        if (!index.TryGetValue(key, out var bitArray))
        {
            bitArray = new BitArray(maxIndex);
            index[key] = bitArray;
        }
        else if (bitArray.Length < maxIndex)
        {
            // Resize the BitArray if needed
            var newBitArray = new BitArray(maxIndex);
            for (int i = 0; i < bitArray.Length; i++)
            {
                newBitArray[i] = bitArray[i];
            }
            bitArray = newBitArray;
            index[key] = bitArray;
        }

        var linkIndex = Convert.ToInt32(linkAddress);
        if (linkIndex < bitArray.Length)
        {
            bitArray[linkIndex] = true;
        }
    }

    /// <summary>
    ///     Removes a link from the specified index.
    /// </summary>
    private void RemoveFromIndex(Dictionary<TLinkAddress, BitArray> index, TLinkAddress key, TLinkAddress linkAddress)
    {
        if (index.TryGetValue(key, out var bitArray))
        {
            var linkIndex = Convert.ToInt32(linkAddress);
            if (linkIndex < bitArray.Length)
            {
                bitArray[linkIndex] = false;
            }
        }
    }

    /// <summary>
    ///     Counts links using the index if enabled, otherwise delegates to the underlying storage.
    /// </summary>
    public override TLinkAddress Count(IList<TLinkAddress>? restriction)
    {
        if (!_useIndexForReads || restriction == null || restriction.Count == 0)
        {
            return base.Count(restriction);
        }

        // Try to use index for specific patterns
        if (restriction.Count >= 3)
        {
            var source = restriction[_constants.SourcePart];
            var target = restriction[_constants.TargetPart];

            if (source != _constants.Any && target != _constants.Any)
            {
                // Both source and target specified - intersect the indexes
                return CountWithSourceAndTarget(source, target);
            }
            else if (source != _constants.Any)
            {
                // Only source specified
                return CountWithSource(source);
            }
            else if (target != _constants.Any)
            {
                // Only target specified  
                return CountWithTarget(target);
            }
        }

        return base.Count(restriction);
    }

    private TLinkAddress CountWithSource(TLinkAddress source)
    {
        lock (_lockObject)
        {
            if (_sourceIndexes.TryGetValue(source, out var bitArray))
            {
                var count = 0;
                for (int i = 0; i < bitArray.Length; i++)
                {
                    if (bitArray[i]) count++;
                }
                return TLinkAddress.CreateTruncating(count);
            }
        }
        return TLinkAddress.Zero;
    }

    private TLinkAddress CountWithTarget(TLinkAddress target)
    {
        lock (_lockObject)
        {
            if (_targetIndexes.TryGetValue(target, out var bitArray))
            {
                var count = 0;
                for (int i = 0; i < bitArray.Length; i++)
                {
                    if (bitArray[i]) count++;
                }
                return TLinkAddress.CreateTruncating(count);
            }
        }
        return TLinkAddress.Zero;
    }

    private TLinkAddress CountWithSourceAndTarget(TLinkAddress source, TLinkAddress target)
    {
        lock (_lockObject)
        {
            if (_sourceIndexes.TryGetValue(source, out var sourceBitArray) && 
                _targetIndexes.TryGetValue(target, out var targetBitArray))
            {
                var count = 0;
                var minLength = Math.Min(sourceBitArray.Length, targetBitArray.Length);
                for (int i = 0; i < minLength; i++)
                {
                    if (sourceBitArray[i] && targetBitArray[i]) count++;
                }
                return TLinkAddress.CreateTruncating(count);
            }
        }
        return TLinkAddress.Zero;
    }

    /// <summary>
    ///     Iterates over links using the index if enabled, otherwise delegates to the underlying storage.
    /// </summary>
    public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        if (!_useIndexForReads || restriction == null || restriction.Count == 0 || handler == null)
        {
            return base.Each(restriction, handler);
        }

        // Try to use index for specific patterns
        if (restriction.Count >= 3)
        {
            var source = restriction[_constants.SourcePart];
            var target = restriction[_constants.TargetPart];

            if (source != _constants.Any && target != _constants.Any)
            {
                // Both source and target specified - intersect the indexes
                return EachWithSourceAndTarget(source, target, handler);
            }
            else if (source != _constants.Any)
            {
                // Only source specified
                return EachWithSource(source, handler);
            }
            else if (target != _constants.Any)
            {
                // Only target specified
                return EachWithTarget(target, handler);
            }
        }

        return base.Each(restriction, handler);
    }

    private TLinkAddress EachWithSource(TLinkAddress source, ReadHandler<TLinkAddress> handler)
    {
        lock (_lockObject)
        {
            if (_sourceIndexes.TryGetValue(source, out var bitArray))
            {
                for (int i = 0; i < bitArray.Length; i++)
                {
                    if (bitArray[i])
                    {
                        var linkAddress = TLinkAddress.CreateTruncating(i);
                        // Get the actual link data
                        var linkData = new TLinkAddress[3];
                        
                        _links.Each(new[] { linkAddress, _constants.Any, _constants.Any }, link =>
                        {
                            linkData[_constants.IndexPart] = link[_constants.IndexPart];
                            linkData[_constants.SourcePart] = link[_constants.SourcePart];
                            linkData[_constants.TargetPart] = link[_constants.TargetPart];
                            return _constants.Break;
                        });

                        var result = handler(linkData);
                        if (result == _constants.Break)
                        {
                            return _constants.Break;
                        }
                    }
                }
            }
        }
        return _constants.Continue;
    }

    private TLinkAddress EachWithTarget(TLinkAddress target, ReadHandler<TLinkAddress> handler)
    {
        lock (_lockObject)
        {
            if (_targetIndexes.TryGetValue(target, out var bitArray))
            {
                for (int i = 0; i < bitArray.Length; i++)
                {
                    if (bitArray[i])
                    {
                        var linkAddress = TLinkAddress.CreateTruncating(i);
                        // Get the actual link data
                        var linkData = new TLinkAddress[3];
                        
                        _links.Each(new[] { linkAddress, _constants.Any, _constants.Any }, link =>
                        {
                            linkData[_constants.IndexPart] = link[_constants.IndexPart];
                            linkData[_constants.SourcePart] = link[_constants.SourcePart];
                            linkData[_constants.TargetPart] = link[_constants.TargetPart];
                            return _constants.Break;
                        });

                        var result = handler(linkData);
                        if (result == _constants.Break)
                        {
                            return _constants.Break;
                        }
                    }
                }
            }
        }
        return _constants.Continue;
    }

    private TLinkAddress EachWithSourceAndTarget(TLinkAddress source, TLinkAddress target, ReadHandler<TLinkAddress> handler)
    {
        lock (_lockObject)
        {
            if (_sourceIndexes.TryGetValue(source, out var sourceBitArray) && 
                _targetIndexes.TryGetValue(target, out var targetBitArray))
            {
                var minLength = Math.Min(sourceBitArray.Length, targetBitArray.Length);
                for (int i = 0; i < minLength; i++)
                {
                    if (sourceBitArray[i] && targetBitArray[i])
                    {
                        var linkAddress = TLinkAddress.CreateTruncating(i);
                        // Get the actual link data
                        var linkData = new TLinkAddress[3];
                        
                        _links.Each(new[] { linkAddress, _constants.Any, _constants.Any }, link =>
                        {
                            linkData[_constants.IndexPart] = link[_constants.IndexPart];
                            linkData[_constants.SourcePart] = link[_constants.SourcePart];
                            linkData[_constants.TargetPart] = link[_constants.TargetPart];
                            return _constants.Break;
                        });

                        var result = handler(linkData);
                        if (result == _constants.Break)
                        {
                            return _constants.Break;
                        }
                    }
                }
            }
        }
        return _constants.Continue;
    }

    /// <summary>
    ///     Creates a new link and updates the index if enabled.
    /// </summary>
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        var result = base.Create(substitution, handler);
        
        if (_maintainIndexOnWrites && substitution != null && substitution.Count >= 2)
        {
            var source = substitution[_constants.SourcePart];
            var target = substitution[_constants.TargetPart];
            
            lock (_lockObject)
            {
                // Update max address if needed
                if (result > _maxLinkAddress)
                {
                    _maxLinkAddress = result;
                    ResizeIndexesIfNeeded(Convert.ToInt32(result) + 1);
                }
                
                AddToIndex(_sourceIndexes, source, result, Convert.ToInt32(_maxLinkAddress) + 1);
                AddToIndex(_targetIndexes, target, result, Convert.ToInt32(_maxLinkAddress) + 1);
            }
        }
        
        return result;
    }

    /// <summary>
    ///     Updates an existing link and updates the index if enabled.
    /// </summary>
    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        TLinkAddress[]? oldLink = null;
        
        if (_maintainIndexOnWrites && restriction != null && restriction.Count > 0)
        {
            // Get the old link data before update
            var linkAddress = restriction[_constants.IndexPart];
            if (linkAddress != _constants.Any)
            {
                oldLink = new TLinkAddress[3];
                _links.Each(restriction, link =>
                {
                    oldLink[_constants.IndexPart] = link[_constants.IndexPart];
                    oldLink[_constants.SourcePart] = link[_constants.SourcePart];
                    oldLink[_constants.TargetPart] = link[_constants.TargetPart];
                    return _constants.Break;
                });
            }
        }
        
        var result = base.Update(restriction, substitution, handler);
        
        if (_maintainIndexOnWrites && oldLink != null && substitution != null && substitution.Count >= 2)
        {
            var linkAddress = oldLink[_constants.IndexPart];
            var oldSource = oldLink[_constants.SourcePart];
            var oldTarget = oldLink[_constants.TargetPart];
            var newSource = substitution[_constants.SourcePart];
            var newTarget = substitution[_constants.TargetPart];
            
            lock (_lockObject)
            {
                // Remove old indexes
                RemoveFromIndex(_sourceIndexes, oldSource, linkAddress);
                RemoveFromIndex(_targetIndexes, oldTarget, linkAddress);
                
                // Add new indexes
                AddToIndex(_sourceIndexes, newSource, linkAddress, Convert.ToInt32(_maxLinkAddress) + 1);
                AddToIndex(_targetIndexes, newTarget, linkAddress, Convert.ToInt32(_maxLinkAddress) + 1);
            }
        }
        
        return result;
    }

    /// <summary>
    ///     Deletes a link and updates the index if enabled.
    /// </summary>
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        TLinkAddress[]? linkToDelete = null;
        
        if (_maintainIndexOnWrites && restriction != null && restriction.Count > 0)
        {
            // Get the link data before deletion
            linkToDelete = new TLinkAddress[3];
            _links.Each(restriction, link =>
            {
                linkToDelete[_constants.IndexPart] = link[_constants.IndexPart];
                linkToDelete[_constants.SourcePart] = link[_constants.SourcePart];
                linkToDelete[_constants.TargetPart] = link[_constants.TargetPart];
                return _constants.Break;
            });
        }
        
        var result = base.Delete(restriction, handler);
        
        if (_maintainIndexOnWrites && linkToDelete != null)
        {
            var linkAddress = linkToDelete[_constants.IndexPart];
            var source = linkToDelete[_constants.SourcePart];
            var target = linkToDelete[_constants.TargetPart];
            
            lock (_lockObject)
            {
                RemoveFromIndex(_sourceIndexes, source, linkAddress);
                RemoveFromIndex(_targetIndexes, target, linkAddress);
            }
        }
        
        return result;
    }

    /// <summary>
    ///     Resizes all BitArrays in the indexes to the specified size.
    /// </summary>
    private void ResizeIndexesIfNeeded(int newSize)
    {
        ResizeIndexDictionary(_sourceIndexes, newSize);
        ResizeIndexDictionary(_targetIndexes, newSize);
    }

    private void ResizeIndexDictionary(Dictionary<TLinkAddress, BitArray> indexDict, int newSize)
    {
        var keysToUpdate = new List<TLinkAddress>();
        foreach (var kvp in indexDict)
        {
            if (kvp.Value.Length < newSize)
            {
                keysToUpdate.Add(kvp.Key);
            }
        }

        foreach (var key in keysToUpdate)
        {
            var oldBitArray = indexDict[key];
            var newBitArray = new BitArray(newSize);
            for (int i = 0; i < oldBitArray.Length; i++)
            {
                newBitArray[i] = oldBitArray[i];
            }
            indexDict[key] = newBitArray;
        }
    }

    /// <summary>
    ///     Clears all indexes.
    /// </summary>
    public void ClearIndexes()
    {
        lock (_lockObject)
        {
            _sourceIndexes.Clear();
            _targetIndexes.Clear();
        }
    }

    /// <summary>
    ///     Rebuilds all indexes from the current links storage.
    /// </summary>
    public void RebuildIndexes()
    {
        lock (_lockObject)
        {
            ClearIndexes();
            InitializeIndexes();
        }
    }
}