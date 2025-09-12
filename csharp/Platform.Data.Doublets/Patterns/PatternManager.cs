using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Manages pattern parsing, matching, and transformation operations.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class PatternManager<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly IPatternParser<TLinkAddress> _parser;
        private readonly IPatternTransformer<TLinkAddress> _transformer;
        private readonly Dictionary<string, PatternNode<TLinkAddress>> _userDefinedPatterns;

        /// <summary>
        /// <para>
        /// Gets the pattern parser.
        /// </para>
        /// <para></para>
        /// </summary>
        public IPatternParser<TLinkAddress> Parser => _parser;

        /// <summary>
        /// <para>
        /// Gets the pattern transformer.
        /// </para>
        /// <para></para>
        /// </summary>
        public IPatternTransformer<TLinkAddress> Transformer => _transformer;

        /// <summary>
        /// <para>
        /// Gets the user-defined patterns.
        /// </para>
        /// <para></para>
        /// </summary>
        public IReadOnlyDictionary<string, PatternNode<TLinkAddress>> UserDefinedPatterns => _userDefinedPatterns;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="PatternManager{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        public PatternManager()
        {
            _parser = new LinksNotationPatternParser<TLinkAddress>();
            _transformer = new BasicPatternTransformer<TLinkAddress>();
            _userDefinedPatterns = new Dictionary<string, PatternNode<TLinkAddress>>();
            
            InitializeBuiltInPatterns();
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="PatternManager{TLinkAddress}"/> instance with custom implementations.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="parser">
        /// <para>The pattern parser implementation.</para>
        /// <para></para>
        /// </param>
        /// <param name="transformer">
        /// <para>The pattern transformer implementation.</para>
        /// <para></para>
        /// </param>
        public PatternManager(IPatternParser<TLinkAddress> parser, IPatternTransformer<TLinkAddress> transformer)
        {
            _parser = parser;
            _transformer = transformer;
            _userDefinedPatterns = new Dictionary<string, PatternNode<TLinkAddress>>();
            
            InitializeBuiltInPatterns();
        }

        /// <summary>
        /// <para>
        /// Parses a pattern string into a pattern object.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patternString">
        /// <para>The pattern string to parse.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>The parsed pattern.</para>
        /// <para></para>
        /// </returns>
        public IPattern<TLinkAddress> ParsePattern(string patternString)
        {
            var node = _parser.Parse(patternString);
            return new BasicPattern<TLinkAddress>(node, _userDefinedPatterns);
        }

        /// <summary>
        /// <para>
        /// Defines new user patterns from pattern definition strings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="patternDefinitions">
        /// <para>The pattern definitions to parse and register.</para>
        /// <para></para>
        /// </param>
        public void DefinePatterns(string patternDefinitions)
        {
            var definitions = _parser.ParseDefinitions(patternDefinitions);
            foreach (var kvp in definitions)
            {
                _userDefinedPatterns[kvp.Key] = kvp.Value;
            }
        }

        /// <summary>
        /// <para>
        /// Matches a pattern against links and returns all matching links.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="pattern">
        /// <para>The pattern to match.</para>
        /// <para></para>
        /// </param>
        /// <param name="links">
        /// <para>The links storage.</para>
        /// <para></para>
        /// </param>
        /// <returns>
        /// <para>Dictionary of matching links and their variable assignments.</para>
        /// <para></para>
        /// </returns>
        public Dictionary<TLinkAddress, IDictionary<string, TLinkAddress>> FindMatches(IPattern<TLinkAddress> pattern, ILinks<TLinkAddress> links)
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

        /// <summary>
        /// <para>
        /// Applies a transformation once using pattern strings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sourcePatternString">
        /// <para>The source pattern string.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPatternString">
        /// <para>The target pattern string.</para>
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
        public bool TransformOnce(string sourcePatternString, string targetPatternString, ILinks<TLinkAddress> links)
        {
            var sourcePattern = ParsePattern(sourcePatternString);
            var targetPattern = ParsePattern(targetPatternString);
            return _transformer.TransformOnce(sourcePattern, targetPattern, links);
        }

        /// <summary>
        /// <para>
        /// Applies a transformation continuously using pattern strings.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="sourcePatternString">
        /// <para>The source pattern string.</para>
        /// <para></para>
        /// </param>
        /// <param name="targetPatternString">
        /// <para>The target pattern string.</para>
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
        public int TransformAlways(string sourcePatternString, string targetPatternString, ILinks<TLinkAddress> links)
        {
            var sourcePattern = ParsePattern(sourcePatternString);
            var targetPattern = ParsePattern(targetPatternString);
            return _transformer.TransformAlways(sourcePattern, targetPattern, links);
        }

        private void InitializeBuiltInPatterns()
        {
            var builtInPatterns = GetBuiltInPatternDefinitions();
            var definitions = _parser.ParseDefinitions(builtInPatterns);
            foreach (var kvp in definitions)
            {
                _userDefinedPatterns[kvp.Key] = kvp.Value;
            }
        }

        private string GetBuiltInPatternDefinitions()
        {
            return @"
                (pattern (name ""point"") ($x: $x $x))
                (pattern (name ""partial-point"") (or ($x: $x *) ($x: * $x)))
                (pattern (name ""tree"") (or #element (#element #element) (tree #element) (#element tree)))
            ";
        }

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
}