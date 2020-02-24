using System.Collections.Generic;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators
{
    public class LinksUniquenessResolver<TLink> : LinksDecoratorBase<TLink>
    {
        private static readonly EqualityComparer<TLink> _equalityComparer = EqualityComparer<TLink>.Default;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinksUniquenessResolver(ILinks<TLink> links) : base(links) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override TLink Update(IList<TLink> restrictions, IList<TLink> substitution)
        {
            var constants = _constants;
            var links = _links;
            var newLinkAddress = links.SearchOrDefault(substitution[constants.SourcePart], substitution[constants.TargetPart]);
            if (_equalityComparer.Equals(newLinkAddress, default))
            {
                return links.Update(restrictions, substitution);
            }
            return ResolveAddressChangeConflict(restrictions[constants.IndexPart], newLinkAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual TLink ResolveAddressChangeConflict(TLink oldLinkAddress, TLink newLinkAddress)
        {
            if (!_equalityComparer.Equals(oldLinkAddress, newLinkAddress) && _links.Exists(oldLinkAddress))
            {
                _facade.Delete(oldLinkAddress);
            }
            return newLinkAddress;
        }
    }
}
