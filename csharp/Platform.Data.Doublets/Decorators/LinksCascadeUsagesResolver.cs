using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <remarks>
///     <para>Must be used in conjunction with NonNullContentsLinkDeletionResolver.</para>
///     <para>Должен использоваться вместе с NonNullContentsLinkDeletionResolver.</para>
/// </remarks>
public class LinksCascadeUsagesResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksCascadeUsagesResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksCascadeUsagesResolver(ILinks<TLinkAddress> links) : base(links: links) { }

    /// <summary>
    ///     <para>
    ///         Deletes the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        var constants = _links.Constants;
        WriteHandlerState<TLinkAddress> handlerState = new(@continue: constants.Continue, @break: constants.Break, handler: handler);
        var linkIndex = _links.GetIndex(link: restriction);
        // Use Facade (the last decorator) to ensure recursion working correctly
        handlerState.Apply(result: _facade.DeleteAllUsages(linkIndex: linkIndex, handler: handlerState.Handler));
        handlerState.Apply(result: _links.Delete(restriction: restriction, handler: handlerState.Handler));
        return handlerState.Result;
    }
}
