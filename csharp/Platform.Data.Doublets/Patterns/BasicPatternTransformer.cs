using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Basic implementation of pattern transformation and substitution functionality.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class BasicPatternTransformer<TLinkAddress> : IPatternTransformer<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TransformOnce(IPattern<TLinkAddress> sourcePattern, IPattern<TLinkAddress> targetPattern, ILinks<TLinkAddress> links)
        {
            var matchedLinks = FindMatches(sourcePattern, links);
            
            if (matchedLinks.Any())
            {
                var firstMatch = matchedLinks.First();
                return ApplyTransformation(firstMatch.Key, firstMatch.Value, targetPattern, links);
            }
            
            return false;
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int TransformAlways(IPattern<TLinkAddress> sourcePattern, IPattern<TLinkAddress> targetPattern, ILinks<TLinkAddress> links)
        {
            int transformationCount = 0;
            
            while (true)
            {
                var matchedLinks = FindMatches(sourcePattern, links);
                
                if (!matchedLinks.Any())
                    break;
                
                foreach (var match in matchedLinks)
                {
                    if (ApplyTransformation(match.Key, match.Value, targetPattern, links))
                    {
                        transformationCount++;
                    }
                }
            }
            
            return transformationCount;
        }

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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress ApplySubstitution(IPattern<TLinkAddress> pattern, IDictionary<string, TLinkAddress> variables, ILinks<TLinkAddress> links)
        {
            return CreateFromPattern(pattern, variables, links);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private Dictionary<TLinkAddress, IDictionary<string, TLinkAddress>> FindMatches(IPattern<TLinkAddress> pattern, ILinks<TLinkAddress> links)
        {
            var matches = new Dictionary<TLinkAddress, IDictionary<string, TLinkAddress>>();
            var allLinks = GetAllLinks(links);
            
            foreach (var link in allLinks)
            {
                var variables = new Dictionary<string, TLinkAddress>();
                if (pattern.Matches(link, links, variables))
                {
                    matches[link] = variables;
                }
            }
            
            return matches;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ApplyTransformation(TLinkAddress link, IDictionary<string, TLinkAddress> variables, IPattern<TLinkAddress> targetPattern, ILinks<TLinkAddress> links)
        {
            try
            {
                var newLink = CreateFromPattern(targetPattern, variables, links);
                
                if (newLink != default(TLinkAddress))
                {
                    if (newLink != link)
                    {
                        links.Update(link, links.GetSource(newLink), links.GetTarget(newLink));
                    }
                    return true;
                }
                else
                {
                    links.Delete(link);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress CreateFromPattern(IPattern<TLinkAddress> pattern, IDictionary<string, TLinkAddress> variables, ILinks<TLinkAddress> links)
        {
            if (pattern is BasicPattern<TLinkAddress> basicPattern)
            {
                return CreateFromPatternNode(basicPattern.GetRootNode(), variables, links);
            }
            
            return default(TLinkAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress CreateFromPatternNode(PatternNode<TLinkAddress> node, IDictionary<string, TLinkAddress> variables, ILinks<TLinkAddress> links)
        {
            return node.Type switch
            {
                PatternNodeType.Literal => (TLinkAddress)node.Value!,
                PatternNodeType.Variable => variables.TryGetValue((string)node.Value!, out var varValue) ? varValue : default(TLinkAddress),
                PatternNodeType.And when node.Source != null && node.Target != null => 
                    CreateDoublet(node.Source, node.Target, variables, links),
                _ => default(TLinkAddress)
            };
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private TLinkAddress CreateDoublet(PatternNode<TLinkAddress> sourceNode, PatternNode<TLinkAddress> targetNode, IDictionary<string, TLinkAddress> variables, ILinks<TLinkAddress> links)
        {
            var source = CreateFromPatternNode(sourceNode, variables, links);
            var target = CreateFromPatternNode(targetNode, variables, links);
            
            if (source != default(TLinkAddress) && target != default(TLinkAddress))
            {
                return links.GetOrCreate(source, target);
            }
            
            return default(TLinkAddress);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private List<TLinkAddress> GetAllLinks(ILinks<TLinkAddress> links)
        {
            var allLinks = new List<TLinkAddress>();
            var constants = links.Constants;
            var index = constants.InternalReferencesRange.Maximum;
            
            while (index > constants.InternalReferencesRange.Minimum)
            {
                if (links.Exists(index))
                {
                    allLinks.Add(index);
                }
                index--;
            }
            
            return allLinks;
        }
    }

    /// <summary>
    /// <para>
    /// Extension methods for BasicPattern to access internal structure.
    /// </para>
    /// <para></para>
    /// </summary>
    public static class BasicPatternExtensions
    {
        /// <summary>
        /// <para>
        /// Gets the root node of a BasicPattern (for internal use).
        /// </para>
        /// <para></para>
        /// </summary>
        /// <typeparam name="TLinkAddress">
        /// <para>The link address type.</para>
        /// <para></para>
        /// </typeparam>
        /// <param name="pattern">
        /// <para>The basic pattern.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The root pattern node.</para>
        /// <para></para>
        /// </returns>
        internal static PatternNode<TLinkAddress> GetRootNode<TLinkAddress>(this BasicPattern<TLinkAddress> pattern) 
            where TLinkAddress : IUnsignedNumber<TLinkAddress>
        {
            var field = typeof(BasicPattern<TLinkAddress>).GetField("_root", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (PatternNode<TLinkAddress>)field!.GetValue(pattern)!;
        }
    }
}