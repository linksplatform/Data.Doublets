using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Defines the base interface for all patterns in Links notation.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public interface IPattern<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        /// <para>
        /// Matches the pattern against the specified link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="link">
        /// <para>The link to match against.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <param name="variables">
        /// <para>The pattern variables context.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if the pattern matches, false otherwise.</para>
        /// <para></para>
        /// </returns>
        bool Matches(TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables);
    }
}