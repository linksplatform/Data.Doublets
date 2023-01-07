using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Interfaces;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.CriterionMatchers;

/// <summary>
///     <para>
///         Represents the target matcher.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksOperatorBase{TLinkAddress}" />
/// <seealso cref="ICriterionMatcher{TLinkAddress}" />
public class TargetMatcher<TLinkAddress> : LinksOperatorBase<TLinkAddress>, ICriterionMatcher<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly TLinkAddress _targetToMatch;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="TargetMatcher" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    /// <param name="targetToMatch">
    ///     <para>A target to match.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public TargetMatcher(ILinks<TLinkAddress> links, TLinkAddress targetToMatch) : base(links: links)
    {
        _targetToMatch = targetToMatch;
    }

    /// <summary>
    ///     <para>
    ///         Determines whether this instance is matched.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="link">
    ///     <para>The link.</para>
    ///     <para></para>
    /// </param>
    /// <returns>
    ///     <para>The bool</para>
    ///     <para></para>
    /// </returns>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public bool IsMatched(TLinkAddress link)
    {
        return _links.GetTarget(link: link) ==  _targetToMatch;
    }
}
