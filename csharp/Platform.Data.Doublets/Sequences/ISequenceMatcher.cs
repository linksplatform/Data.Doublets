using System;
using System.Collections.Generic;
using LinkIndex = System.UInt64;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences
{
    /// <summary>
    /// Interface for sequence matching operations.
    /// </summary>
    public interface ISequenceMatcher
    {
        bool Match(LinkIndex sequenceToMatch);
        void AddMatchedToResults(IList<LinkIndex> restrictions);
        LinkIndex HandleMatched(IList<LinkIndex> restrictions);
    }
}