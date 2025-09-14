using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using Platform.Interfaces;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.CriterionMatchers;

/// <summary>
///     <para>
///         Represents the source matcher.
///     </para>
///     <para></para>
/// </summary>
/// <seealso cref="LinksOperatorBase{TLinkAddress}" />
/// <seealso cref="ICriterionMatcher{TLinkAddress}" />
public class SourceMatcher<TLinkAddress> : LinksOperatorBase<TLinkAddress>, ICriterionMatcher<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
{
    private readonly TLinkAddress _sourceToMatch;

    /// <summary>
    ///     <para>
    ///         Initializes a new <see cref="SourceMatcher" /> instance.
    ///     </para>
    ///     <para></para>
    /// </summary>
    /// <param name="links">
    ///     <para>A links.</para>
    ///     <para></para>
    /// </param>
    /// <param name="sourceToMatch">
    ///     <para>A source to match.</para>
    ///     <para></para>
    /// </param>
    [MethodImpl(methodImplOptions: MethodImplOptions.AggressiveInlining)]
    public SourceMatcher(ILinks<TLinkAddress> links, TLinkAddress sourceToMatch) : base(links: links)
    {
        _sourceToMatch = sourceToMatch;
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
        return _links.GetSource(link: link) ==  _sourceToMatch;
    }
}