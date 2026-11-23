using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LinkIndex = System.UInt64;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// Matcher for full sequence matching operations - simplified version.
    /// </summary>
    public class SimpleFullMatcher : SimpleSequenceMatcherBase
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public SimpleFullMatcher(Sequences sequences, IList<LinkIndex> patternSequence, HashSet<LinkIndex> results, Func<IList<LinkIndex>, LinkIndex> stopableHandler, HashSet<LinkIndex>? readAsElements = null)
            : base(sequences, patternSequence, results, stopableHandler, readAsElements)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Match(LinkIndex sequenceToMatch)
        {
            return FullMatch(sequenceToMatch);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool FullMatch(LinkIndex sequenceToMatch)
        {
            _filterPosition = 0;
            foreach (var part in Walk(sequenceToMatch))
            {
                if (!FullMatchCore(part))
                {
                    break;
                }
            }
            return _filterPosition == _patternSequence.Count;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FullMatchCore(LinkIndex element)
        {
            if (_filterPosition == _patternSequence.Count)
            {
                _filterPosition = -2; // Длиннее чем нужно
                return false;
            }
            if (_patternSequence[_filterPosition] != _sequences.Links.Constants.Any
             && element != _patternSequence[_filterPosition])
            {
                _filterPosition = -1;
                return false; // Начинается/Продолжается иначе
            }
            _filterPosition++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override void AddMatchedToResults(IList<LinkIndex> restrictions)
        {
            AddFullMatchedToResults(restrictions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddFullMatchedToResults(IList<LinkIndex> restrictions)
        {
            var sequenceToMatch = restrictions[_sequences.Links.Constants.IndexPart];
            if (FullMatch(sequenceToMatch))
            {
                _results.Add(sequenceToMatch);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override LinkIndex HandleMatched(IList<LinkIndex> restrictions)
        {
            return HandleFullMatched(restrictions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkIndex HandleFullMatched(IList<LinkIndex> restrictions)
        {
            var sequenceToMatch = restrictions[_sequences.Links.Constants.IndexPart];
            if (FullMatch(sequenceToMatch) && _results.Add(sequenceToMatch))
            {
                return _stopableHandler(new LinkAddress<LinkIndex>(sequenceToMatch));
            }
            return _sequences.Links.Constants.Continue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkIndex HandleFullMatchedSequence(IList<LinkIndex> restrictions)
        {
            var sequenceToMatch = restrictions[_sequences.Links.Constants.IndexPart];
            var sequence = _sequences.GetSequenceByElements(sequenceToMatch);
            if (sequence != _sequences.Links.Constants.Null && FullMatch(sequenceToMatch) && _results.Add(sequenceToMatch))
            {
                return _stopableHandler(new LinkAddress<LinkIndex>(sequence));
            }
            return _sequences.Links.Constants.Continue;
        }
    }
}