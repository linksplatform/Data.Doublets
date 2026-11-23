using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Algorithms
{
    /// <summary>
    /// <para>
    /// Provides extension methods for computing differences between sequences in the context of doublets.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class DiffExtensions
    {
        /// <summary>
        /// <para>
        /// Computes the diff between two sequences of links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link addresses.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourceSequence">
        /// <para>The source sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetSequence">
        /// <para>The target sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of diff operations representing the difference between the sequences.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<DiffOperation<TLinkAddress>> ComputeSequenceDiff<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            IList<TLinkAddress> sourceSequence,
            IList<TLinkAddress> targetSequence) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (sourceSequence == null) throw new ArgumentNullException(nameof(sourceSequence));
            if (targetSequence == null) throw new ArgumentNullException(nameof(targetSequence));

            var algorithm = new SequenceDiffAlgorithm<TLinkAddress>();
            return algorithm.ComputeDiff(sourceSequence, targetSequence);
        }

        /// <summary>
        /// <para>
        /// Computes a simplified diff between two sequences of links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link addresses.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourceSequence">
        /// <para>The source sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetSequence">
        /// <para>The target sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list of simplified diff operations.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<DiffOperation<TLinkAddress>> ComputeSimplifiedSequenceDiff<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            IList<TLinkAddress> sourceSequence,
            IList<TLinkAddress> targetSequence) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (sourceSequence == null) throw new ArgumentNullException(nameof(sourceSequence));
            if (targetSequence == null) throw new ArgumentNullException(nameof(targetSequence));

            var algorithm = new SequenceDiffAlgorithm<TLinkAddress>();
            return algorithm.ComputeSimplifiedDiff(sourceSequence, targetSequence);
        }

        /// <summary>
        /// <para>
        /// Computes the edit distance between two sequences of links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link addresses.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourceSequence">
        /// <para>The source sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetSequence">
        /// <para>The target sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The edit distance between the sequences.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ComputeSequenceEditDistance<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            IList<TLinkAddress> sourceSequence,
            IList<TLinkAddress> targetSequence) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (sourceSequence == null) throw new ArgumentNullException(nameof(sourceSequence));
            if (targetSequence == null) throw new ArgumentNullException(nameof(targetSequence));

            var algorithm = new SequenceDiffAlgorithm<TLinkAddress>();
            return algorithm.ComputeEditDistance(sourceSequence, targetSequence);
        }

        /// <summary>
        /// <para>
        /// Computes the longest common subsequence between two sequences of links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link addresses.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="sourceSequence">
        /// <para>The source sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetSequence">
        /// <para>The target sequence of links.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The longest common subsequence.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress> ComputeLongestCommonSubsequence<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            IList<TLinkAddress> sourceSequence,
            IList<TLinkAddress> targetSequence) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (sourceSequence == null) throw new ArgumentNullException(nameof(sourceSequence));
            if (targetSequence == null) throw new ArgumentNullException(nameof(targetSequence));

            var algorithm = new SequenceDiffAlgorithm<TLinkAddress>();
            return algorithm.ComputeLongestCommonSubsequence(sourceSequence, targetSequence);
        }

        /// <summary>
        /// <para>
        /// Extracts a sequence from a link structure by following the chain of links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The type of link addresses.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="startLink">
        /// <para>The starting link address.</para>
        /// <para></para>
        /// </param>
        /// <param name="maxLength">
        /// <para>Maximum length of sequence to extract (default: 1000).</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>A list representing the sequence of links.</para>
        /// <para></para>
        /// </returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IList<TLinkAddress> ExtractSequence<TLinkAddress>(
            this ILinks<TLinkAddress> links,
            TLinkAddress startLink,
            int maxLength = 1000) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (links == null) throw new ArgumentNullException(nameof(links));
            if (!links.Exists(startLink)) throw new ArgumentException("Start link does not exist", nameof(startLink));

            var sequence = new List<TLinkAddress>();
            var currentLink = startLink;
            var visited = new HashSet<TLinkAddress>();

            while (sequence.Count < maxLength && links.Exists(currentLink) && visited.Add(currentLink))
            {
                sequence.Add(currentLink);
                var linkData = links.GetLink(currentLink);
                var target = links.GetTarget(linkData);
                
                if (EqualityComparer<TLinkAddress>.Default.Equals(target, currentLink))
                {
                    break;
                }
                
                currentLink = target;
            }

            return sequence;
        }
    }
}