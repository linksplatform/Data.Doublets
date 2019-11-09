using System.Collections.Generic;
using Platform.Converters;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Sequences.Converters
{
    public abstract class LinksListToSequenceConverterBase<TLink> : IConverter<IList<TLink>, TLink>
    {
        protected readonly ILinks<TLink> Links;

        protected LinksListToSequenceConverterBase(ILinks<TLink> links) => Links = links;

        public abstract TLink Convert(IList<TLink> source);
    }
}
