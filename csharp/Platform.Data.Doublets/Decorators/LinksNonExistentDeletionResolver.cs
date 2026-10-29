using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

namespace Platform.Data.Doublets.Decorators;

/// <summary>Ignores deletion of a missing link without calling the inner store or handler.</summary>
/// <remarks>
/// Place this outside reset and cascade resolvers so it runs before they read the link.
/// Existing links are forwarded unchanged; endpoint reset and usages remain separate policies.
/// </remarks>
public class LinksNonExistentDeletionResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress>
    where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>Wraps a store with the ignore-missing-deletion policy.</summary>
    public LinksNonExistentDeletionResolver(ILinks<TLinkAddress> links) : base(links) { }

    /// <summary>Returns Continue for a missing link, or forwards an existing deletion.</summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        var index = _links.GetIndex(restriction);
        return _links.Exists(index) ? _links.Delete(restriction, handler) : Constants.Continue;
    }
}
