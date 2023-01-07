using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Delegates;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Decorators;

/// <summary>
///     <para>
///         Represents the links itself constant to self reference resolver.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksDecoratorBase{TLinkAddress}" />
public class LinksItselfConstantToSelfReferenceResolver<TLinkAddress> : LinksDecoratorBase<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private static readonly EqualityComparer<TLinkAddress> _equalityComparer = EqualityComparer<TLinkAddress>.Default;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="LinksItselfConstantToSelfReferenceResolver" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public LinksItselfConstantToSelfReferenceResolver(ILinks<TLinkAddress> links) : base(links: links) { }

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
        var constants = _constants;
        var itselfConstant = constants.Itself;
        if (!_equalityComparer.Equals(x: constants.Any, y: itselfConstant) && restriction.Contains(item: itselfConstant))
        {
            // Itself constant is not supported for Each method right now, skipping execution
            return constants.Continue;
        }
        return _links.Each(restriction: restriction, handler: handler);
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
        return _links.Update(restriction: restriction, substitution: _links.ResolveConstantAsSelfReference(constant: _constants.Itself, restriction: restriction, substitution: substitution), handler: handler);
    }
}
