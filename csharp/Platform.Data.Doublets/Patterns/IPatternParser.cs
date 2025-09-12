using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Defines the interface for parsing pattern strings into pattern AST.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public interface IPatternParser<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        /// <para>
        /// Parses the specified pattern string into a pattern AST.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patternString">
        /// <para>The pattern string to parse.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The parsed pattern node.</para>
        /// <para></para>
        /// </returns>
        PatternNode<TLinkAddress> Parse(string patternString);

        /// <summary>
        /// <para>
        /// Parses multiple pattern definitions into a dictionary.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patternDefinitions">
        /// <para>The pattern definitions to parse.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>Dictionary of pattern names to pattern nodes.</para>
        /// <para></para>
        /// </returns>
        Dictionary<string, PatternNode<TLinkAddress>> ParseDefinitions(string patternDefinitions);
    }
}