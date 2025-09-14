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
        if (constants.Any !=  itselfConstant && restriction != null && restriction.Contains(item: itselfConstant))
        {
            // Handle 'Itself' constant by filtering for self-referential links
            return HandleEachWithItselfConstant(restriction, handler, itselfConstant);
        }
        return _links.Each(restriction: restriction, handler: handler);
    }

    /// <summary>
    ///     <para>
    ///         Handles Each operations when 'Itself' constant is present in the restriction.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The original restriction containing 'Itself' constants.</para>
    ///     <para></para>
    /// </param>
    /// <param name="handler">
    ///     <para>The handler to execute for matching links.</para>
    ///     <para></para>
    /// </param>
    /// <param name="itselfConstant">
    ///     <para>The 'Itself' constant value.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The result of the Each operation.</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private TLinkAddress HandleEachWithItselfConstant(IList<TLinkAddress> restriction, ReadHandler<TLinkAddress>? handler, TLinkAddress itselfConstant)
    {
        var constants = _constants;
        
        // Create a wrapper handler that filters for self-referential links
        ReadHandler<TLinkAddress>? filteringHandler = null;
        if (handler != null)
        {
            filteringHandler = (link) =>
            {
                var linkIndex = _links.GetIndex(link);
                var linkSource = _links.GetSource(link);
                var linkTarget = _links.GetTarget(link);
                
                // Check if this link matches the 'Itself' pattern in the restriction
                if (MatchesItselfPattern(restriction, itselfConstant, linkIndex, linkSource, linkTarget))
                {
                    return handler(link);
                }
                
                return constants.Continue;
            };
        }
        
        // Get all links and let our filtering handler process them
        return _links.Each(restriction: null, handler: filteringHandler);
    }

    /// <summary>
    ///     <para>
    ///         Checks if a link matches the pattern specified by the restriction with 'Itself' constants.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="restriction">
    ///     <para>The restriction pattern.</para>
    ///     <para></para>
    /// </param>
    /// <param name="itselfConstant">
    ///     <para>The 'Itself' constant value.</para>
    ///     <para></para>
    /// </param>
    /// <param name="linkIndex">
    ///     <para>The link's index.</para>
    ///     <para></para>
    /// </param>
    /// <param name="linkSource">
    ///     <para>The link's source.</para>
    ///     <para></para>
    /// </param>
    /// <param name="linkTarget">
    ///     <para>The link's target.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>True if the link matches the pattern, false otherwise.</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    private bool MatchesItselfPattern(IList<TLinkAddress> restriction, TLinkAddress itselfConstant, TLinkAddress linkIndex, TLinkAddress linkSource, TLinkAddress linkTarget)
    {
        var constants = _constants;
        
        // Check index (position 0)
        if (restriction.Count > 0 && restriction[0] != constants.Any)
        {
            var expectedIndex = restriction[0] == itselfConstant ? linkIndex : restriction[0];
            if (linkIndex != expectedIndex)
            {
                return false;
            }
        }
        
        // Check source (position 1)
        if (restriction.Count > 1 && restriction[1] != constants.Any)
        {
            var expectedSource = restriction[1] == itselfConstant ? linkIndex : restriction[1];
            if (linkSource != expectedSource)
            {
                return false;
            }
        }
        
        // Check target (position 2)
        if (restriction.Count > 2 && restriction[2] != constants.Any)
        {
            var expectedTarget = restriction[2] == itselfConstant ? linkIndex : restriction[2];
            if (linkTarget != expectedTarget)
            {
                return false;
            }
        }
        
        return true;
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
