using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Memory.UnitedRanged
{
    public static class UnitedRangedLinksExtensions
    {
        public static bool IsRawLinkSequence<TLinkAddress>(this ILinks<TLinkAddress> links, IList<TLinkAddress>? link)
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            if (link == null || link.Count <= links.Constants.SourcePart)
            {
                return false;
            }
            return links.Constants is UnitedRangedLinksConstants<TLinkAddress> constants
                && link[links.Constants.SourcePart] == constants.RawLinkSequenceMarker;
        }
    }
}
