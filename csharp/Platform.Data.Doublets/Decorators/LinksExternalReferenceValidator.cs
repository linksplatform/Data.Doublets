using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links external reference validator.
///         A layer that checks each link to exist or to be external (hybrid link's raw number).
///         This validator allows references to be treated as binary data when they are external references.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class LinksExternalReferenceValidator<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksExternalReferenceValidator" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksExternalReferenceValidator(ILinks<TLinkAddress> links) : base(links: links) { }

    /// <summary>
    ///     <para>
    ///         Eaches the handler.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="handler">
    ///     <para>The handler.</para>
    ///     <para></para>
    /// </param>
    /// <param name="restriction">
    ///     <para>The restriction.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Each(IList<TLinkAddress>? restriction, ReadHandler<TLinkAddress>? handler)
    {
        var links = _links;
        links.EnsureReferenceExistsOrExternal(restriction: restriction, argumentName: nameof(restriction));
        return links.Each(restriction: restriction, handler: handler);
    }

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
        links.EnsureReferenceExistsOrExternal(restriction: restriction, argumentName: nameof(restriction));
        links.EnsureReferenceExistsOrExternal(restriction: substitution, argumentName: nameof(substitution));
        return links.Update(restriction: restriction, substitution: substitution, handler: handler);
    }

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
        var links = _links;
        var link = links.GetIndex(link: restriction);
        links.EnsureLinkExists(link: link, argumentName: nameof(link));
        return links.Delete(restriction: restriction, handler: handler);
    }
}
