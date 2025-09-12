using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Represents a basic pattern implementation using the pattern AST.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class BasicPattern<TLinkAddress> : IPattern<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly PatternNode<TLinkAddress> _root;
        private readonly Dictionary<string, PatternNode<TLinkAddress>> _userDefinedPatterns;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="BasicPattern{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="root">
        /// <para>The root pattern node.</para>
        /// <para></para>
        /// </param>
        /// <param name="userDefinedPatterns">
        /// <para>Dictionary of user-defined patterns.</para>
        /// <para></para>
        /// </param>
        public BasicPattern(PatternNode<TLinkAddress> root, Dictionary<string, PatternNode<TLinkAddress>>? userDefinedPatterns = null)
        {
            _root = root;
            _userDefinedPatterns = userDefinedPatterns ?? new Dictionary<string, PatternNode<TLinkAddress>>();
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Matches(TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return MatchesNode(_root, link, links, variables);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesNode(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return node.Type switch
            {
                PatternNodeType.Any => true,
                PatternNodeType.Literal => MatchesLiteral(node, link),
                PatternNodeType.Variable => MatchesVariable(node, link, variables),
                PatternNodeType.Point => MatchesPoint(link, links),
                PatternNodeType.PartialPoint => MatchesPartialPoint(link, links),
                PatternNodeType.Set => MatchesSet(node, link, links, variables),
                PatternNodeType.Sequence => MatchesSequence(node, link, links, variables),
                PatternNodeType.Tree => MatchesTree(node, link, links, variables),
                PatternNodeType.Or => MatchesOr(node, link, links, variables),
                PatternNodeType.And => MatchesAnd(node, link, links, variables),
                PatternNodeType.Not => MatchesNot(node, link, links, variables),
                PatternNodeType.GreaterThan => MatchesGreaterThan(node, link),
                PatternNodeType.GreaterThanOrEqual => MatchesGreaterThanOrEqual(node, link),
                PatternNodeType.LessThan => MatchesLessThan(node, link),
                PatternNodeType.LessThanOrEqual => MatchesLessThanOrEqual(node, link),
                PatternNodeType.Incoming => MatchesIncoming(node, link, links, variables),
                PatternNodeType.Outgoing => MatchesOutgoing(node, link, links, variables),
                PatternNodeType.UserDefined => MatchesUserDefined(node, link, links, variables),
                _ => false
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesLiteral(PatternNode<TLinkAddress> node, TLinkAddress link)
        {
            if (node.Value is TLinkAddress literalValue)
            {
                return link == literalValue;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesVariable(PatternNode<TLinkAddress> node, TLinkAddress link, IDictionary<string, TLinkAddress> variables)
        {
            if (node.Value is string variableName)
            {
                if (variables.TryGetValue(variableName, out var existingValue))
                {
                    return link == existingValue;
                }
                else
                {
                    variables[variableName] = link;
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesPoint(TLinkAddress link, ILinks<TLinkAddress> links)
        {
            var source = links.GetSource(link);
            var target = links.GetTarget(link);
            return source == target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesPartialPoint(TLinkAddress link, ILinks<TLinkAddress> links)
        {
            var source = links.GetSource(link);
            var target = links.GetTarget(link);
            return source == link || target == link;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesSet(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesSequence(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesTree(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            if (node.Children.Count == 0) return true;
            
            var elementPattern = node.Children[0];
            
            if (MatchesNode(elementPattern, link, links, variables))
                return true;
            
            var source = links.GetSource(link);
            var target = links.GetTarget(link);
            
            if (MatchesNode(elementPattern, source, links, variables) && 
                MatchesNode(elementPattern, target, links, variables))
                return true;
            
            var leftTree = new PatternNode<TLinkAddress>(PatternNodeType.Tree);
            leftTree.Children.Add(elementPattern);
            
            if (MatchesNode(leftTree, source, links, variables) && 
                MatchesNode(elementPattern, target, links, variables))
                return true;
            
            if (MatchesNode(elementPattern, source, links, variables) && 
                MatchesNode(leftTree, target, links, variables))
                return true;
            
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesOr(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            foreach (var child in node.Children)
            {
                var childVariables = new Dictionary<string, TLinkAddress>(variables);
                if (MatchesNode(child, link, links, childVariables))
                {
                    foreach (var kvp in childVariables)
                    {
                        variables[kvp.Key] = kvp.Value;
                    }
                    return true;
                }
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesAnd(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            foreach (var child in node.Children)
            {
                if (!MatchesNode(child, link, links, variables))
                    return false;
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesNot(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            if (node.Children.Count > 0)
            {
                var childVariables = new Dictionary<string, TLinkAddress>(variables);
                return !MatchesNode(node.Children[0], link, links, childVariables);
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesGreaterThan(PatternNode<TLinkAddress> node, TLinkAddress link)
        {
            if (node.Value is TLinkAddress value)
            {
                return link > value;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesGreaterThanOrEqual(PatternNode<TLinkAddress> node, TLinkAddress link)
        {
            if (node.Value is TLinkAddress value)
            {
                return link >= value;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesLessThan(PatternNode<TLinkAddress> node, TLinkAddress link)
        {
            if (node.Value is TLinkAddress value)
            {
                return link < value;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesLessThanOrEqual(PatternNode<TLinkAddress> node, TLinkAddress link)
        {
            if (node.Value is TLinkAddress value)
            {
                return link <= value;
            }
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesIncoming(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesOutgoing(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool MatchesUserDefined(PatternNode<TLinkAddress> node, TLinkAddress link, ILinks<TLinkAddress> links, IDictionary<string, TLinkAddress> variables)
        {
            if (node.Value is string patternName && _userDefinedPatterns.TryGetValue(patternName, out var userPattern))
            {
                return MatchesNode(userPattern, link, links, variables);
            }
            return false;
        }
    }
}