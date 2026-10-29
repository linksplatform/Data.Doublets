using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Data.Exceptions;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the non null contents link deletion resolver.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class NonNullContentsLinkDeletionResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="NonNullContentsLinkDeletionResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public NonNullContentsLinkDeletionResolver(ILinks<TLinkAddress> links) : base(links: links) { }

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
    /// <exception cref="ArgumentLinkDoesNotExistsException{TLinkAddress}">
    /// The link does not exist. The store and handler are left untouched.
    /// </exception>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Delete(IList<TLinkAddress>? restriction, WriteHandler<TLinkAddress>? handler)
    {
        var linkIndex = _links.GetIndex(link: restriction);
        _links.EnsureLinkExists(link: linkIndex, argumentName: nameof(linkIndex));
        var constants = _links.Constants;
        WriteHandlerState<TLinkAddress> handlerResult = new(@continue: constants.Continue, @break: constants.Break, handler: handler);
        handlerResult.Apply(result: _links.EnforceResetValues(linkIndex: linkIndex, handler: handlerResult.Handler));
        handlerResult.Apply(result: _links.Delete(restriction: restriction, handler: handlerResult.Handler));
        return handlerResult.Result;
    }
}
