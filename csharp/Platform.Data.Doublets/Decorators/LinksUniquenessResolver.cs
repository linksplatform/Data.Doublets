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

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksUniquenessResolver{TLinkAddress}" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksUniquenessResolver(ILinks<TLinkAddress> links) : base(links: links) { }

    /// <param name="handler">Optional callback; null continues without invoking a callback.</param>
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
        var links = _links;
        var newLinkAddress = links.SearchOrDefault(source: links.GetSource(link: substitution), target: links.GetTarget(link: substitution));
        if (newLinkAddress == default)
        {
            return links.Update(restriction: restriction, substitution: substitution, handler: handler);
        }
        return ResolveAddressChangeConflict(oldLinkAddress: links.GetIndex(link: restriction), newLinkAddress: newLinkAddress, handler: handler);
    }

    /// <param name="handler">Optional callback; null continues without invoking a callback.</param>
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
    ///     <para>Callback control (Continue or Break).</para>
    ///     <para></para>
    /// </returns>
    /// <remarks>
    /// Cleanup is followed by a survivor-to-survivor notification that identifies
    /// the resolved link, including unchanged pairs. Break suppresses later
    /// notifications while cleanup completes.
    /// </remarks>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected virtual TLinkAddress ResolveAddressChangeConflict(TLinkAddress oldLinkAddress, TLinkAddress newLinkAddress, WriteHandler<TLinkAddress>? handler)
    {
        var constants = _links.Constants;
        WriteHandlerState<TLinkAddress> handlerState = new(constants.Continue, constants.Break, handler);
        if (oldLinkAddress != newLinkAddress && _links.Exists(link: oldLinkAddress))
        {
            handlerState.Apply(_facade.Delete(linkToDelete: oldLinkAddress, handler: handlerState.Handler));
        }
        // Report the resolved result last: reset/deletion callbacks describe the
        // redundant link and must not become the result of address-returning APIs.
        // This also supplies a result when the requested pair is already at old.
        if (handlerState.Handler is { } resolvedHandler)
        {
            var survivor = _links.GetLink(newLinkAddress);
            handlerState.Apply(resolvedHandler(survivor, survivor));
        }
        return handlerState.Result;
    }
}
