using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using Platform.Data.Doublets.Memory;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    /// <summary>
    /// <para>
    /// Represents an indexed links decorator that adds configurable indexing capabilities 
    /// to any ILinks implementation.
    /// </para>
    /// <para>
    /// This decorator wraps an existing ILinks implementation and provides efficient 
    /// indexed operations while maintaining compatibility with the base interface.
    /// </para>
    /// </summary>
    public class IndexedLinksDecorator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly Dictionary<TLinkAddress, HashSet<TLinkAddress>> _sourceIndex;
        private readonly Dictionary<TLinkAddress, HashSet<TLinkAddress>> _targetIndex;
        private readonly Dictionary<(TLinkAddress Source, TLinkAddress Target), TLinkAddress> _linkIndex;
        private readonly bool _enableSourceIndex;
        private readonly bool _enableTargetIndex;
        private readonly bool _enableLinkIndex;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="IndexedLinksDecorator"/> instance with default indexing.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>The links implementation to decorate.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexedLinksDecorator(ILinks<TLinkAddress> links) 
            : this(links, enableSourceIndex: true, enableTargetIndex: true, enableLinkIndex: true) { }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="IndexedLinksDecorator"/> instance with configurable indexing.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="links">
        /// <para>The links implementation to decorate.</para>
        /// <para></para>
        /// </param>
        /// <param name="enableSourceIndex">
        /// <para>Whether to enable source-based indexing for fast source lookups.</para>
        /// <para></para>
        /// </param>
        /// <param name="enableTargetIndex">
        /// <para>Whether to enable target-based indexing for fast target lookups.</para>
        /// <para></para>
        /// </param>
        /// <param name="enableLinkIndex">
        /// <para>Whether to enable link-based indexing for fast source-target pair lookups.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IndexedLinksDecorator(ILinks<TLinkAddress> links, bool enableSourceIndex, bool enableTargetIndex, bool enableLinkIndex) 
            : base(links)
        {
            _enableSourceIndex = enableSourceIndex;
            _enableTargetIndex = enableTargetIndex;
            _enableLinkIndex = enableLinkIndex;

            if (_enableSourceIndex)
            {
                _sourceIndex = new Dictionary<TLinkAddress, HashSet<TLinkAddress>>();
            }
            
            if (_enableTargetIndex)
            {
                _targetIndex = new Dictionary<TLinkAddress, HashSet<TLinkAddress>>();
            }
            
            if (_enableLinkIndex)
            {
                _linkIndex = new Dictionary<(TLinkAddress Source, TLinkAddress Target), TLinkAddress>();
            }

            // Build initial indexes from existing links
            BuildIndexes();
        }

        /// <summary>
        /// <para>
        /// Counts links with optional restriction using indexes when possible.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction criteria.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links matching the restriction.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Count(IList<TLinkAddress>? restriction)
        {
            if (restriction == null || restriction.Count == 0)
            {
                return base.Count(restriction);
            }

            var constants = Constants;
            var any = constants.Any;

            // Try to use indexes for efficient counting
            if (restriction.Count == 3)
            {
                var index = restriction[0];
                var source = restriction[1];
                var target = restriction[2];

                if (index == any && source != any && target == any && _enableSourceIndex)
                {
                    // Count all links with specific source
                    return _sourceIndex.TryGetValue(source, out var sourceLinks) 
                        ? TLinkAddress.CreateTruncating(sourceLinks.Count) 
                        : TLinkAddress.Zero;
                }

                if (index == any && source == any && target != any && _enableTargetIndex)
                {
                    // Count all links with specific target
                    return _targetIndex.TryGetValue(target, out var targetLinks) 
                        ? TLinkAddress.CreateTruncating(targetLinks.Count) 
                        : TLinkAddress.Zero;
                }

                if (index == any && source != any && target != any && _enableLinkIndex)
                {
                    // Check if specific source-target pair exists
                    return _linkIndex.ContainsKey((source, target)) 
                        ? TLinkAddress.CreateTruncating(1) 
                        : TLinkAddress.Zero;
                }
            }

            // Fall back to base implementation
            return base.Count(restriction);
        }

        /// <summary>
        /// <para>
        /// Iterates through links with optional restriction using indexes when possible.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction criteria.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call for each matching link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links processed.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
        {
            if (restriction == null || restriction.Count == 0 || handler == null)
            {
                return base.Each(restriction, handler);
            }

            var constants = Constants;
            var any = constants.Any;

            // Try to use indexes for efficient iteration
            if (restriction.Count == 3)
            {
                var index = restriction[0];
                var source = restriction[1];
                var target = restriction[2];

                if (index == any && source != any && target == any && _enableSourceIndex)
                {
                    // Iterate all links with specific source
                    return IterateFromSourceIndex(source, handler);
                }

                if (index == any && source == any && target != any && _enableTargetIndex)
                {
                    // Iterate all links with specific target
                    return IterateFromTargetIndex(target, handler);
                }

                if (index == any && source != any && target != any && _enableLinkIndex)
                {
                    // Find specific source-target pair
                    if (_linkIndex.TryGetValue((source, target), out var linkIndex))
                    {
                        var linkData = _links.GetLinkStruct(linkIndex);
                        if (linkData != null)
                        {
                            var result = handler(linkData);
                            return result == constants.Break ? TLinkAddress.Zero : TLinkAddress.CreateTruncating(1);
                        }
                    }
                    return TLinkAddress.Zero;
                }
            }

            // Fall back to base implementation
            return base.Each(restriction, handler);
        }

        /// <summary>
        /// <para>
        /// Creates a new link and updates indexes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="substitution">
        /// <para>The substitution data for the new link.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The created link index.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            var result = base.Create(substitution, handler);
            
            if (substitution != null && substitution.Count >= 2)
            {
                var source = substitution[0];
                var target = substitution[1];
                AddToIndexes(result, source, target);
            }
            
            return result;
        }

        /// <summary>
        /// <para>
        /// Updates links and maintains indexes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction criteria.</para>
        /// <para></para>
        /// </param>
        /// <param name="substitution">
        /// <para>The substitution data.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links updated.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
        {
            // Remove old indexes before update
            if (restriction != null && restriction.Count >= 1)
            {
                var linkIndex = restriction[0];
                var linkData = _links.GetLinkStruct(linkIndex);
                if (linkData != null && linkData.Count >= 3)
                {
                    RemoveFromIndexes(linkIndex, linkData[1], linkData[2]);
                }
            }

            var result = base.Update(restriction, substitution, handler);
            
            // Add new indexes after update
            if (restriction != null && restriction.Count >= 1 && substitution != null && substitution.Count >= 2)
            {
                var linkIndex = restriction[0];
                var source = substitution[0];
                var target = substitution[1];
                AddToIndexes(linkIndex, source, target);
            }
            
            return result;
        }

        /// <summary>
        /// <para>
        /// Deletes links and removes from indexes.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="restriction">
        /// <para>The restriction criteria.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The write handler.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links deleted.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
        {
            // Remove from indexes before deletion
            if (restriction != null && restriction.Count >= 1)
            {
                var linkIndex = restriction[0];
                var linkData = _links.GetLinkStruct(linkIndex);
                if (linkData != null && linkData.Count >= 3)
                {
                    RemoveFromIndexes(linkIndex, linkData[1], linkData[2]);
                }
            }

            return base.Delete(restriction, handler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void BuildIndexes()
        {
            // Build indexes from existing links
            _links.Each(null, linkData =>
            {
                if (linkData != null && linkData.Count >= 3)
                {
                    var linkIndex = linkData[0];
                    var source = linkData[1];
                    var target = linkData[2];
                    AddToIndexes(linkIndex, source, target);
                }
                return Constants.Continue;
            });
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void AddToIndexes(TLinkAddress linkIndex, TLinkAddress source, TLinkAddress target)
        {
            if (_enableSourceIndex)
            {
                if (!_sourceIndex.TryGetValue(source, out var sourceLinks))
                {
                    sourceLinks = new HashSet<TLinkAddress>();
                    _sourceIndex[source] = sourceLinks;
                }
                sourceLinks.Add(linkIndex);
            }

            if (_enableTargetIndex)
            {
                if (!_targetIndex.TryGetValue(target, out var targetLinks))
                {
                    targetLinks = new HashSet<TLinkAddress>();
                    _targetIndex[target] = targetLinks;
                }
                targetLinks.Add(linkIndex);
            }

            if (_enableLinkIndex)
            {
                _linkIndex[(source, target)] = linkIndex;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RemoveFromIndexes(TLinkAddress linkIndex, TLinkAddress source, TLinkAddress target)
        {
            if (_enableSourceIndex && _sourceIndex.TryGetValue(source, out var sourceLinks))
            {
                sourceLinks.Remove(linkIndex);
                if (sourceLinks.Count == 0)
                {
                    _sourceIndex.Remove(source);
                }
            }

            if (_enableTargetIndex && _targetIndex.TryGetValue(target, out var targetLinks))
            {
                targetLinks.Remove(linkIndex);
                if (targetLinks.Count == 0)
                {
                    _targetIndex.Remove(target);
                }
            }

            if (_enableLinkIndex)
            {
                _linkIndex.Remove((source, target));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress IterateFromSourceIndex(TLinkAddress source, ReadHandler<TLinkAddress> handler)
        {
            if (!_sourceIndex.TryGetValue(source, out var sourceLinks))
            {
                return TLinkAddress.Zero;
            }

            var count = TLinkAddress.Zero;
            var constants = Constants;

            foreach (var linkIndex in sourceLinks)
            {
                var linkData = _links.GetLinkStruct(linkIndex);
                if (linkData != null)
                {
                    count++;
                    var result = handler(linkData);
                    if (result == constants.Break)
                    {
                        break;
                    }
                }
            }

            return count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress IterateFromTargetIndex(TLinkAddress target, ReadHandler<TLinkAddress> handler)
        {
            if (!_targetIndex.TryGetValue(target, out var targetLinks))
            {
                return TLinkAddress.Zero;
            }

            var count = TLinkAddress.Zero;
            var constants = Constants;

            foreach (var linkIndex in targetLinks)
            {
                var linkData = _links.GetLinkStruct(linkIndex);
                if (linkData != null)
                {
                    count++;
                    var result = handler(linkData);
                    if (result == constants.Break)
                    {
                        break;
                    }
                }
            }

            return count;
        }
    }
}