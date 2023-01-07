using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links null constant to self reference resolver.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class LinksNullConstantToSelfReferenceResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksNullConstantToSelfReferenceResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksNullConstantToSelfReferenceResolver(ILinks<TLinkAddress> links) : base(links: links) { }

    /// <summary>
    ///     <para>
    ///         Creates the substitution.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="substitution">
    ///     <para>The substitution.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The link</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public override TLinkAddress Create(IList<TLinkAddress>? substitution, WriteHandler<TLinkAddress>? handler)
    {
        return _links.CreatePoint(handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Updates the substitution.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The substitution.</para>
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
        return _links.Update(restriction: restriction, substitution: _links.ResolveConstantAsSelfReference(constant: _constants.Null, restriction: restriction, substitution: substitution), handler: handler);
    }
}
