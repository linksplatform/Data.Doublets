using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links cascade uniqueness and usages resolver.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksUniquenessResolver{TLinkAddress}" />
public class LinksCascadeUniquenessAndUsagesResolver<TLinkAddress> : LinksUniquenessResolver<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksCascadeUniquenessAndUsagesResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksCascadeUniquenessAndUsagesResolver(ILinks<TLinkAddress> links) : base(links: links) { }

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
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    protected override TLinkAddress ResolveAddressChangeConflict(TLinkAddress oldLinkAddress, TLinkAddress newLinkAddress, WriteHandler<TLinkAddress>? handler)
    {
        var constants = _links.Constants;
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: constants.Continue, @break: constants.Break, handler: handler);
        // Use Facade (the last decorator) to ensure recursion working correctly
        handlerState.Apply(result: _facade.MergeUsages(oldLinkIndex: oldLinkAddress, newLinkIndex: newLinkAddress, handler: handlerState.Handler));
        handlerState.Apply(result: base.ResolveAddressChangeConflict(oldLinkAddress: oldLinkAddress, newLinkAddress: newLinkAddress, handler: handlerState.Handler));
        return handlerState.Result;
    }
}
