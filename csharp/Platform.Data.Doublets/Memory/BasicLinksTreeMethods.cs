using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;
using Platform.Data.Numbers;
using Platform.Data.Doublets.Memory.United;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory
{
    /// <summary>
    /// <para>
    /// Represents basic links tree methods that perform linear search without indexing.
    /// </para>
    /// <para>
    /// This implementation provides a simple, non-indexed approach for link operations.
    /// It's slower than tree-indexed methods but doesn't require maintaining indexes.
    /// </para>
    /// </summary>
    public unsafe class BasicLinksTreeMethods<TLinkAddress> : ILinksTreeMethods<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private static readonly TLinkAddress _zero = TLinkAddress.Zero;
        private static readonly TLinkAddress _one = ++_zero;
        
        private readonly LinksConstants<TLinkAddress> _constants;
        private readonly byte* _links;
        private readonly byte* _header;
        private readonly bool _indexBySource;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicLinksTreeMethods"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="constants">
        /// <para>The constants.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links memory pointer.</para>
        /// <para></para>
        /// </param>
        /// <param name="header">
        /// <para>The header memory pointer.</para>
        /// <para></para>
        /// </param>
        /// <param name="indexBySource">
        /// <para>True if this instance indexes by source, false if by target.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public BasicLinksTreeMethods(LinksConstants<TLinkAddress> constants, byte* links, byte* header, bool indexBySource)
        {
            _constants = constants;
            _links = links;
            _header = header;
            _indexBySource = indexBySource;
        }

        /// <summary>
        /// <para>
        /// Counts the usages of a link by performing linear search through all links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="root">
        /// <para>The link to count usages for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of links that reference the root link.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress CountUsages(TLinkAddress root)
        {
            var count = _zero;
            var total = GetTotal();
            
            for (var i = _one; i <= total; i++)
            {
                if (LinkExists(i))
                {
                    ref var link = ref GetLinkReference(i);
                    if (_indexBySource ? link.Source == root : link.Target == root)
                    {
                        count++;
                    }
                }
            }
            
            return count;
        }

        /// <summary>
        /// <para>
        /// Searches for a link with specified source and target by linear search.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="source">
        /// <para>The source link.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target link.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The link address if found, null constant otherwise.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress Search(TLinkAddress source, TLinkAddress target)
        {
            var total = GetTotal();
            
            for (var i = _one; i <= total; i++)
            {
                if (LinkExists(i))
                {
                    ref var link = ref GetLinkReference(i);
                    if (link.Source == source && link.Target == target)
                    {
                        return i;
                    }
                }
            }
            
            return _constants.Null;
        }

        /// <summary>
        /// <para>
        /// Iterates through all usages of a link by performing linear search.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="root">
        /// <para>The link to find usages for.</para>
        /// <para></para>
        /// </param>
        /// <param name="handler">
        /// <para>The handler to call for each usage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of usages processed.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress EachUsage(TLinkAddress root, ReadHandler<TLinkAddress>? handler)
        {
            var count = _zero;
            var total = GetTotal();
            
            for (var i = _one; i <= total; i++)
            {
                if (LinkExists(i))
                {
                    ref var link = ref GetLinkReference(i);
                    if (_indexBySource ? link.Source == root : link.Target == root)
                    {
                        count++;
                        var linkArray = new TLinkAddress[] { i, link.Source, link.Target };
                        if (handler != null)
                        {
                            var @continue = handler(linkArray);
                            if (@continue == _constants.Break)
                            {
                                break;
                            }
                        }
                    }
                }
            }
            
            return count;
        }

        /// <summary>
        /// <para>
        /// Does nothing in basic implementation as there's no index to detach from.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="root">
        /// <para>The root (unused).</para>
        /// <para></para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>The link index (unused).</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Detach(ref TLinkAddress root, TLinkAddress linkIndex)
        {
            // No-op: basic implementation doesn't maintain indexes
        }

        /// <summary>
        /// <para>
        /// Does nothing in basic implementation as there's no index to attach to.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="root">
        /// <para>The root (unused).</para>
        /// <para></para>
        /// </param>
        /// <param name="linkIndex">
        /// <para>The link index (unused).</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Attach(ref TLinkAddress root, TLinkAddress linkIndex)
        {
            // No-op: basic implementation doesn't maintain indexes
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress GetTotal()
        {
            ref var header = ref System.Runtime.CompilerServices.Unsafe.AsRef<LinksHeader<TLinkAddress>>(_header);
            return header.AllocatedLinks;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool LinkExists(TLinkAddress linkIndex)
        {
            ref var link = ref GetLinkReference(linkIndex);
            return link.Source != _zero || link.Target != _zero;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ref RawLink<TLinkAddress> GetLinkReference(TLinkAddress linkIndex)
        {
            return ref System.Runtime.CompilerServices.Unsafe.AsRef<RawLink<TLinkAddress>>(_links + (RawLink<TLinkAddress>.SizeInBytes * System.Runtime.CompilerServices.Unsafe.As<TLinkAddress, long>(ref linkIndex)));
        }
    }
}