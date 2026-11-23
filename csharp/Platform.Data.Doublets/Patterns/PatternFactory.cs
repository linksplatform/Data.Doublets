using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Factory for creating common pattern instances.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public static class PatternFactory<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        /// <para>
        /// Creates an "any" pattern that matches any link.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The any pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Any()
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Any);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a literal pattern that matches a specific link address.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="address">
        /// <para>The link address to match.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The literal pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Literal(TLinkAddress address)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Literal, address);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a variable pattern with the specified variable name.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="variableName">
        /// <para>The variable name.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The variable pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Variable(string variableName)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Variable, variableName);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a point pattern that matches links where source equals target.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The point pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Point()
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Point);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a partial point pattern that matches links that reference themselves.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <returns>
        /// <para>The partial point pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> PartialPoint()
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.PartialPoint);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a tree pattern with the specified element pattern.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="elementPattern">
        /// <para>The element pattern for tree nodes.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The tree pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Tree(IPattern<TLinkAddress> elementPattern)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Tree);
            if (elementPattern is BasicPattern<TLinkAddress> basicPattern)
            {
                node.Children.Add(basicPattern.GetRootNode());
            }
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates an OR pattern that matches if any of the child patterns match.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patterns">
        /// <para>The child patterns.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The OR pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Or(params IPattern<TLinkAddress>[] patterns)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Or);
            foreach (var pattern in patterns)
            {
                if (pattern is BasicPattern<TLinkAddress> basicPattern)
                {
                    node.Children.Add(basicPattern.GetRootNode());
                }
            }
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates an AND pattern that matches only if all child patterns match.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patterns">
        /// <para>The child patterns.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The AND pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> And(params IPattern<TLinkAddress>[] patterns)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.And);
            foreach (var pattern in patterns)
            {
                if (pattern is BasicPattern<TLinkAddress> basicPattern)
                {
                    node.Children.Add(basicPattern.GetRootNode());
                }
            }
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a NOT pattern that matches if the child pattern doesn't match.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="pattern">
        /// <para>The child pattern to negate.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The NOT pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Not(IPattern<TLinkAddress> pattern)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.Not);
            if (pattern is BasicPattern<TLinkAddress> basicPattern)
            {
                node.Children.Add(basicPattern.GetRootNode());
            }
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a doublet pattern that matches links with specific source and target patterns.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sourcePattern">
        /// <para>The source pattern.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPattern">
        /// <para>The target pattern.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The doublet pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> Doublet(IPattern<TLinkAddress> sourcePattern, IPattern<TLinkAddress> targetPattern)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.And);
            if (sourcePattern is BasicPattern<TLinkAddress> basicSourcePattern)
            {
                node.Source = basicSourcePattern.GetRootNode();
            }
            if (targetPattern is BasicPattern<TLinkAddress> basicTargetPattern)
            {
                node.Target = basicTargetPattern.GetRootNode();
            }
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a greater than pattern that matches links with address greater than specified value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="value">
        /// <para>The comparison value.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The greater than pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> GreaterThan(TLinkAddress value)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.GreaterThan, value);
            return new BasicPattern<TLinkAddress>(node);
        }

        /// <summary>
        /// <para>
        /// Creates a less than pattern that matches links with address less than specified value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="value">
        /// <para>The comparison value.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The less than pattern.</para>
        /// <para></para>
        /// </returns>
        public static IPattern<TLinkAddress> LessThan(TLinkAddress value)
        {
            var node = new PatternNode<TLinkAddress>(PatternNodeType.LessThan, value);
            return new BasicPattern<TLinkAddress>(node);
        }
    }
}