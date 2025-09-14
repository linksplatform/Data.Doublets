using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LinkIndex = System.UInt64;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// Matcher for partial sequence matching operations - simplified version.
    /// </summary>
    public class SimplePartialMatcher : SimpleSequenceMatcherBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimplePartialMatcher(Sequences sequences, IList<LinkIndex> patternSequence, HashSet<LinkIndex> results, Func<IList<LinkIndex>, LinkIndex> stopableHandler, HashSet<LinkIndex>? readAsElements = null)
            : base(sequences, patternSequence, results, stopableHandler, readAsElements)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Match(LinkIndex sequenceToMatch)
        {
            return PartialMatch(sequenceToMatch);
        }

        /// <remarks>
        /// TODO: Add support for LinksConstants.Any
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool PartialMatch(LinkIndex sequenceToMatch)
        {
            _filterPosition = -1;
            foreach (var part in Walk(sequenceToMatch))
            {
                if (!PartialMatchCore(part))
                {
                    break;
                }
            }
            return _filterPosition == _patternSequence.Count - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool PartialMatchCore(LinkIndex element)
        {
            if (_filterPosition == (_patternSequence.Count - 1))
            {
                return false; // Нашлось
            }
            if (_filterPosition >= 0)
            {
                if (element == _patternSequence[_filterPosition + 1])
                {
                    _filterPosition++;
                }
                else
                {
                    _filterPosition = -1;
                }
            }
            if (_filterPosition < 0)
            {
                if (element == _patternSequence[0])
                {
                    _filterPosition = 0;
                }
            }
            return true; // Ищем дальше
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AddMatchedToResults(IList<LinkIndex> restrictions)
        {
            var sequenceToMatch = restrictions[_sequences.Links.Constants.IndexPart];
            AddPartialMatchedToResults(sequenceToMatch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddPartialMatchedToResults(LinkIndex sequenceToMatch)
        {
            if (PartialMatch(sequenceToMatch))
            {
                _results.Add(sequenceToMatch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override LinkIndex HandleMatched(IList<LinkIndex> restrictions)
        {
            return HandlePartialMatched(restrictions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkIndex HandlePartialMatched(IList<LinkIndex> restrictions)
        {
            var sequenceToMatch = restrictions[_sequences.Links.Constants.IndexPart];
            if (PartialMatch(sequenceToMatch))
            {
                return _stopableHandler(new LinkAddress<LinkIndex>(sequenceToMatch));
            }
            return _sequences.Links.Constants.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAllPartialMatchedToResults(IEnumerable<LinkIndex> sequencesToMatch)
        {
            foreach (var sequenceToMatch in sequencesToMatch)
            {
                if (PartialMatch(sequenceToMatch))
                {
                    _results.Add(sequenceToMatch);
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddAllPartialMatchedToResultsAndReadAsElements(IEnumerable<LinkIndex> sequencesToMatch)
        {
            foreach (var sequenceToMatch in sequencesToMatch)
            {
                if (PartialMatch(sequenceToMatch))
                {
                    _readAsElements?.Add(sequenceToMatch);
                    _results.Add(sequenceToMatch);
                }
            }
        }
    }
}