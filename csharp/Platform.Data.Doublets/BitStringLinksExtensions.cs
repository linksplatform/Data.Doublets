using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents extension methods for ILinks that provide bitstring-based search functionality.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class BitStringLinksExtensions
    {
        /// <summary>
        /// <para>
        /// Builds a bitstring index for sequences/fragments to enable fast searching.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to build.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void BuildBitStringIndex<TLinkAddress>(this ILinks<TLinkAddress> links, IBitStringIndex<TLinkAddress> bitStringIndex) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var totalLinks = int.CreateTruncating(links.Count());
            
            // Iterate through all links to build the index
            links.Each(link =>
            {
                var linkArray = link?.ToArray();
                if (linkArray != null && linkArray.Length >= 3) // Index, Source, Target
                {
                    var linkAddress = linkArray[0];
                    var source = linkArray[1];
                    var target = linkArray[2];
                    var linkPosition = int.CreateTruncating(linkAddress);
                    
                    // Update bitstring for source
                    bitStringIndex.UpdateBit(source, linkPosition, true);
                    
                    // Update bitstring for target
                    bitStringIndex.UpdateBit(target, linkPosition, true);
                    
                    // For sequences, we also want to track the link itself in subsequences
                    // This allows finding links that contain this specific link as a fragment
                    bitStringIndex.UpdateBit(linkAddress, linkPosition, true);
                }
                
                return links.Constants.Continue;
            });
        }

        /// <summary>
        /// <para>
        /// Searches for sequences that contain all specified fragments using bitstring intersection.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to use for searching.</para>
        /// <para></para>
        /// </param>
        /// <param name="fragments">
        /// <para>The fragments that must be contained in the sequences.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The enumerable of link addresses that contain all fragments.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TLinkAddress> SearchSequencesContainingAllFragments<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            params TLinkAddress[] fragments) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            return bitStringIndex.FindLinksContainingAllFragments(fragments);
        }

        /// <summary>
        /// <para>
        /// Searches for sequences optimized by selecting least frequent fragments first.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to use for searching.</para>
        /// <para></para>
        /// </param>
        /// <param name="fragments">
        /// <para>The fragments that must be contained in the sequences.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The enumerable of link addresses that contain all fragments.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TLinkAddress> SearchSequencesContainingAllFragmentsOptimized<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            params TLinkAddress[] fragments) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (fragments.Length == 0) yield break;
            
            // Sort fragments by frequency (least frequent first) to optimize intersection
            var sortedFragments = fragments
                .Select(f => new { Fragment = f, Frequency = bitStringIndex.GetFrequency(f) })
                .Where(x => x.Frequency > 0) // Filter out fragments that don't exist
                .OrderBy(x => x.Frequency)
                .Select(x => x.Fragment)
                .ToArray();
            
            if (sortedFragments.Length == 0) yield break;
            
            foreach (var result in bitStringIndex.FindLinksContainingAllFragments(sortedFragments))
            {
                yield return result;
            }
        }

        /// <summary>
        /// <para>
        /// Searches for sequences containing pairs of fragments (as mentioned in the issue).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to use for searching.</para>
        /// <para></para>
        /// </param>
        /// <param name="pairs">
        /// <para>The pairs of fragments to search for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The enumerable of link addresses that contain the pairs.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IEnumerable<TLinkAddress> SearchSequencesContainingPairs<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            params (TLinkAddress, TLinkAddress)[] pairs) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            if (pairs.Length == 0) yield break;
            
            // Find the least frequent pair first
            var pairFrequencies = pairs
                .Select(p => new { 
                    Pair = p, 
                    Frequency = Math.Min(bitStringIndex.GetFrequency(p.Item1), bitStringIndex.GetFrequency(p.Item2)) 
                })
                .Where(x => x.Frequency > 0)
                .OrderBy(x => x.Frequency)
                .ToArray();
            
            if (pairFrequencies.Length == 0) yield break;
            
            // Start with the least frequent pair
            var startPair = pairFrequencies.First().Pair;
            var candidateLinks = bitStringIndex.FindLinksContainingAllFragments(new[] { startPair.Item1, startPair.Item2 }).ToList();
            
            // Intersect with other pairs
            foreach (var pairFreq in pairFrequencies.Skip(1))
            {
                var pair = pairFreq.Pair;
                var pairLinks = bitStringIndex.FindLinksContainingAllFragments(new[] { pair.Item1, pair.Item2 }).ToHashSet();
                candidateLinks = candidateLinks.Where(pairLinks.Contains).ToList();
                
                if (candidateLinks.Count == 0) yield break;
            }
            
            foreach (var link in candidateLinks)
            {
                yield return link;
            }
        }

        /// <summary>
        /// <para>
        /// Gets the frequency count of a fragment across all sequences.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to query.</para>
        /// <para></para>
        /// </param>
        /// <param name="fragment">
        /// <para>The fragment to get frequency for.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The frequency count of the fragment.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetFragmentFrequency<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            TLinkAddress fragment) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            return bitStringIndex.GetFrequency(fragment);
        }

        /// <summary>
        /// <para>
        /// Updates the bitstring index when a new link is created.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to update.</para>
        /// <para></para>
        /// </param>
        /// <param name="linkAddress">
        /// <para>The address of the newly created link.</para>
        /// <para></para>
        /// </param>
        /// <param name="source">
        /// <para>The source of the link.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target of the link.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateBitStringIndexOnCreate<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            TLinkAddress linkAddress,
            TLinkAddress source,
            TLinkAddress target) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var linkPosition = int.CreateTruncating(linkAddress);
            
            // Update bitstring for source
            bitStringIndex.UpdateBit(source, linkPosition, true);
            
            // Update bitstring for target  
            bitStringIndex.UpdateBit(target, linkPosition, true);
            
            // Update bitstring for the link itself
            bitStringIndex.UpdateBit(linkAddress, linkPosition, true);
        }

        /// <summary>
        /// <para>
        /// Updates the bitstring index when a link is deleted.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links.</para>
        /// <para></para>
        /// </param>
        /// <param name="bitStringIndex">
        /// <para>The bitstring index to update.</para>
        /// <para></para>
        /// </param>
        /// <param name="linkAddress">
        /// <para>The address of the deleted link.</para>
        /// <para></para>
        /// </param>
        /// <param name="source">
        /// <para>The source of the link.</para>
        /// <para></para>
        /// </param>
        /// <param name="target">
        /// <para>The target of the link.</para>
        /// <para></para>
        /// </param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void UpdateBitStringIndexOnDelete<TLinkAddress>(
            this ILinks<TLinkAddress> links, 
            IBitStringIndex<TLinkAddress> bitStringIndex,
            TLinkAddress linkAddress,
            TLinkAddress source,
            TLinkAddress target) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
        {
            var linkPosition = int.CreateTruncating(linkAddress);
            
            // Update bitstring for source
            bitStringIndex.UpdateBit(source, linkPosition, false);
            
            // Update bitstring for target
            bitStringIndex.UpdateBit(target, linkPosition, false);
            
            // Update bitstring for the link itself
            bitStringIndex.UpdateBit(linkAddress, linkPosition, false);
        }
    }
}