using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links uniqueness resolver.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class LinksUniquenessResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private static readonly EqualityComparer<TLinkAddress> _equalityComparer = EqualityComparer<TLinkAddress>.Default;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksUniquenessResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksUniquenessResolver(ILinks<TLinkAddress> links) : base(links: links) { }

    /// <summary>
    ///     <para>
    ///         Updates the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <param name="substitution">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Update(IList<TLinkAddress>? restriction, IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        var constants = _constants;
        var links = _links;
        var newLinkAddress = links.SearchOrDefault(source: links.GetSource(link: substitution), target: links.GetTarget(link: substitution));
        if (_equalityComparer.Equals(x: newLinkAddress, y: default))
        {
            return links.Update(restriction: restriction, substitution: substitution, handler: handler);
        }
        return ResolveAddressChangeConflict(oldLinkAddress: links.GetIndex(link: restriction), newLinkAddress: newLinkAddress, handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Resolves the address change conflict using the specified old link address.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="oldLinkAddress">
    ///     <para>The old link address.</para>
    ///     <para></para>
    /// </param>
    /// <param name="newLinkAddress">
    ///     <para>The new link address.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The new link address.</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress ResolveAddressChangeConflict(TLinkAddress oldLinkAddress, TLinkAddress newLinkAddress, WriteHandler<TLinkAddress>? handler)
    {
        if (!_equalityComparer.Equals(x: oldLinkAddress, y: newLinkAddress) && _links.Exists(link: oldLinkAddress))
        {
            return _facade.Delete(linkToDelete: oldLinkAddress, handler: handler);
        }
        return _links.Constants.Continue;
    }
}
