using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Defines the interface for pattern transformations and substitutions.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public interface IPatternTransformer<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        /// <para>
        /// Applies a transformation once to the links storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sourcePattern">
        /// <para>The source pattern to match.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPattern">
        /// <para>The target pattern for substitution.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>True if transformation was applied, false otherwise.</para>
        /// <para></para>
        /// </returns>
        bool TransformOnce(IPattern<TLinkAddress> sourcePattern, IPattern<TLinkAddress> targetPattern, ILinks<TLinkAddress> links);

        /// <summary>
        /// <para>
        /// Applies a transformation continuously to the links storage.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sourcePattern">
        /// <para>The source pattern to match.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPattern">
        /// <para>The target pattern for substitution.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The number of transformations applied.</para>
        /// <para></para>
        /// </returns>
        int TransformAlways(IPattern<TLinkAddress> sourcePattern, IPattern<TLinkAddress> targetPattern, ILinks<TLinkAddress> links);

        /// <summary>
        /// <para>
        /// Applies variable substitution to create new links based on pattern and variables.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="pattern">
        /// <para>The target pattern for creation.</para>
        /// <para></para>
        /// </param>
        /// <param name="variables">
        /// <para>The variables context for substitution.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The created link address, or default if creation failed.</para>
        /// <para></para>
        /// </returns>
        TLinkAddress ApplySubstitution(IPattern<TLinkAddress> pattern, IDictionary<string, TLinkAddress> variables, ILinks<TLinkAddress> links);
    }
}