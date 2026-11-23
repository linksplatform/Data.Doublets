using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using LinkIndex = System.UInt64;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// Base class for sequence matchers providing common functionality without Walker dependency.
    /// </summary>
    public abstract class SimpleSequenceMatcherBase : ISequenceMatcher
    {
        protected readonly Sequences _sequences;
        protected readonly IList<LinkIndex> _patternSequence;
        protected readonly HashSet<LinkIndex> _linksInSequence;
        protected readonly HashSet<LinkIndex> _results;
        protected readonly Func<IList<LinkIndex>, LinkIndex> _stopableHandler;
        protected readonly HashSet<LinkIndex>? _readAsElements;
        protected int _filterPosition;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected SimpleSequenceMatcherBase(Sequences sequences, IList<LinkIndex> patternSequence, HashSet<LinkIndex> results, Func<IList<LinkIndex>, LinkIndex> stopableHandler, HashSet<LinkIndex>? readAsElements = null)
        {
            _sequences = sequences;
            _patternSequence = patternSequence;
            _linksInSequence = new HashSet<LinkIndex>(patternSequence.Where(x => x != sequences.Links.Constants.Any));
            _results = results;
            _stopableHandler = stopableHandler;
            _readAsElements = readAsElements;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual bool IsElement(LinkIndex link) => (_readAsElements != null && _readAsElements.Contains(link)) || _linksInSequence.Contains(link);

        public abstract bool Match(LinkIndex sequenceToMatch);
        
        public abstract void AddMatchedToResults(IList<LinkIndex> restrictions);
        
        public abstract LinkIndex HandleMatched(IList<LinkIndex> restrictions);

        /// <summary>
        /// Simple walk method without complex Walker dependency.
        /// </summary>
        protected virtual IEnumerable<LinkIndex> Walk(LinkIndex sequence)
        {
            // Simple implementation for walking through a sequence
            // This is a basic version for the refactoring goal
            
            if (!_sequences.Links.Exists(sequence))
                yield break;
                
            var link = _sequences.Links.GetLink(sequence);
            var source = _sequences.Links.GetSource(link);
            var target = _sequences.Links.GetTarget(link);
            
            if (IsElement(source))
                yield return source;
            else if (source != sequence) // Avoid infinite recursion
            {
                foreach (var part in Walk(source))
                    yield return part;
            }
            
            if (IsElement(target))
                yield return target;
            else if (target != sequence) // Avoid infinite recursion
            {
                foreach (var part in Walk(target))
                    yield return part;
            }
        }
    }
}