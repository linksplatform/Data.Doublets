using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text.RegularExpressions;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Parses Links notation pattern strings into pattern AST.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class LinksNotationPatternParser<TLinkAddress> : IPatternParser<TLinkAddress> 
        where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        private readonly Dictionary<string, PatternNode<TLinkAddress>> _userDefinedPatterns;

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="LinksNotationPatternParser{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        public LinksNotationPatternParser()
        {
            _userDefinedPatterns = new Dictionary<string, PatternNode<TLinkAddress>>();
        }

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
        public PatternNode<TLinkAddress> Parse(string patternString)
        {
            var tokens = Tokenize(patternString);
            int index = 0;
            return ParseExpression(tokens, ref index);
        }

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
        public Dictionary<string, PatternNode<TLinkAddress>> ParseDefinitions(string patternDefinitions)
        {
            var definitions = new Dictionary<string, PatternNode<TLinkAddress>>();
            var patterns = SplitPatternDefinitions(patternDefinitions);

            foreach (var pattern in patterns)
            {
                if (TryParseDefinition(pattern, out var name, out var node))
                {
                    definitions[name] = node;
                    _userDefinedPatterns[name] = node;
                }
            }

            return definitions;
        }

        private List<string> Tokenize(string input)
        {
            var tokens = new List<string>();
            var regex = new Regex(@"\(|\)|[*]|[^()\s*]+|\s+");
            var matches = regex.Matches(input);
            
            foreach (Match match in matches)
            {
                if (!string.IsNullOrWhiteSpace(match.Value))
                {
                    tokens.Add(match.Value.Trim());
                }
            }
            
            return tokens;
        }

        private PatternNode<TLinkAddress> ParseExpression(List<string> tokens, ref int index)
        {
            if (index >= tokens.Count) 
                throw new ArgumentException("Unexpected end of input");

            var token = tokens[index];
            index++;

            // Parse basic patterns
            switch (token)
            {
                case "*":
                    return new PatternNode<TLinkAddress>(PatternNodeType.Any);
                
                case "set":
                    return new PatternNode<TLinkAddress>(PatternNodeType.Set);
                
                case "sequence":
                    return new PatternNode<TLinkAddress>(PatternNodeType.Sequence);
                
                case "tree":
                    return new PatternNode<TLinkAddress>(PatternNodeType.Tree);
                
                case "point":
                    return new PatternNode<TLinkAddress>(PatternNodeType.Point);
                
                case "partial-point":
                    return new PatternNode<TLinkAddress>(PatternNodeType.PartialPoint);
                
                case "(":
                    return ParseParenthesizedExpression(tokens, ref index);
                
                default:
                    if (token.StartsWith("$"))
                    {
                        return new PatternNode<TLinkAddress>(PatternNodeType.Variable, token.Substring(1));
                    }
                    else if (TryParseLiteral(token, out var literalValue))
                    {
                        return new PatternNode<TLinkAddress>(PatternNodeType.Literal, literalValue);
                    }
                    else if (_userDefinedPatterns.ContainsKey(token))
                    {
                        return new PatternNode<TLinkAddress>(PatternNodeType.UserDefined, token);
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown token: {token}");
                    }
            }
        }

        private PatternNode<TLinkAddress> ParseParenthesizedExpression(List<string> tokens, ref int index)
        {
            if (index >= tokens.Count)
                throw new ArgumentException("Unexpected end of input in parenthesized expression");

            var firstToken = tokens[index];

            // Handle logical operators
            switch (firstToken)
            {
                case "or":
                case "|":
                    return ParseLogicalOperator(tokens, ref index, PatternNodeType.Or);
                
                case "and":
                case "&":
                    return ParseLogicalOperator(tokens, ref index, PatternNodeType.And);
                
                case "not":
                case "!":
                    return ParseLogicalOperator(tokens, ref index, PatternNodeType.Not);
                
                case "gt":
                case ">":
                    return ParseComparisonOperator(tokens, ref index, PatternNodeType.GreaterThan);
                
                case "gte":
                case ">=":
                    return ParseComparisonOperator(tokens, ref index, PatternNodeType.GreaterThanOrEqual);
                
                case "lt":
                case "<":
                    return ParseComparisonOperator(tokens, ref index, PatternNodeType.LessThan);
                
                case "lte":
                case "<=":
                    return ParseComparisonOperator(tokens, ref index, PatternNodeType.LessThanOrEqual);
                
                case "in":
                    return ParseSingleArgumentOperator(tokens, ref index, PatternNodeType.Incoming);
                
                case "out":
                    return ParseSingleArgumentOperator(tokens, ref index, PatternNodeType.Outgoing);
                
                case "pattern":
                    return ParsePatternDefinition(tokens, ref index);
                
                default:
                    return ParseDoublet(tokens, ref index);
            }
        }

        private PatternNode<TLinkAddress> ParseLogicalOperator(List<string> tokens, ref int index, PatternNodeType type)
        {
            index++; // Skip operator token
            var node = new PatternNode<TLinkAddress>(type);
            
            while (index < tokens.Count && tokens[index] != ")")
            {
                node.Children.Add(ParseExpression(tokens, ref index));
            }
            
            if (index < tokens.Count && tokens[index] == ")")
                index++; // Skip closing parenthesis
            
            return node;
        }

        private PatternNode<TLinkAddress> ParseComparisonOperator(List<string> tokens, ref int index, PatternNodeType type)
        {
            index++; // Skip operator token
            
            if (index >= tokens.Count)
                throw new ArgumentException("Expected value after comparison operator");
            
            var valueToken = tokens[index++];
            if (!TryParseLiteral(valueToken, out var value))
                throw new ArgumentException($"Invalid literal value: {valueToken}");
            
            if (index < tokens.Count && tokens[index] == ")")
                index++; // Skip closing parenthesis
            
            return new PatternNode<TLinkAddress>(type, value);
        }

        private PatternNode<TLinkAddress> ParseSingleArgumentOperator(List<string> tokens, ref int index, PatternNodeType type)
        {
            index++; // Skip operator token
            var argument = ParseExpression(tokens, ref index);
            
            if (index < tokens.Count && tokens[index] == ")")
                index++; // Skip closing parenthesis
            
            var node = new PatternNode<TLinkAddress>(type);
            node.Children.Add(argument);
            return node;
        }

        private PatternNode<TLinkAddress> ParseDoublet(List<string> tokens, ref int index)
        {
            var sourcePattern = ParseExpression(tokens, ref index);
            var targetPattern = ParseExpression(tokens, ref index);
            
            if (index < tokens.Count && tokens[index] == ")")
                index++; // Skip closing parenthesis
            
            var doubletNode = new PatternNode<TLinkAddress>(PatternNodeType.And);
            doubletNode.Source = sourcePattern;
            doubletNode.Target = targetPattern;
            
            return doubletNode;
        }

        private PatternNode<TLinkAddress> ParsePatternDefinition(List<string> tokens, ref int index)
        {
            index++; // Skip "pattern" token
            
            if (index >= tokens.Count || tokens[index] != "(")
                throw new ArgumentException("Expected opening parenthesis after 'pattern'");
            
            index++; // Skip opening parenthesis
            
            if (index >= tokens.Count || tokens[index] != "name")
                throw new ArgumentException("Expected 'name' in pattern definition");
            
            index++; // Skip "name" token
            
            if (index >= tokens.Count)
                throw new ArgumentException("Expected pattern name");
            
            var patternName = tokens[index++].Trim('"');
            
            if (index >= tokens.Count || tokens[index] != ")")
                throw new ArgumentException("Expected closing parenthesis after pattern name");
            
            index++; // Skip closing parenthesis
            
            var patternBody = ParseExpression(tokens, ref index);
            
            if (index < tokens.Count && tokens[index] == ")")
                index++; // Skip closing parenthesis
            
            _userDefinedPatterns[patternName] = patternBody;
            return new PatternNode<TLinkAddress>(PatternNodeType.UserDefined, patternName);
        }

        private bool TryParseLiteral(string token, out TLinkAddress value)
        {
            value = default(TLinkAddress);
            
            if (token.EndsWith(":"))
                token = token.Substring(0, token.Length - 1);
            
            if (ulong.TryParse(token, out var ulongValue))
            {
                value = TLinkAddress.CreateChecked(ulongValue);
                return true;
            }
            
            return false;
        }

        private bool TryParseDefinition(string pattern, out string name, out PatternNode<TLinkAddress> node)
        {
            name = string.Empty;
            node = null!;
            
            try
            {
                var tokens = Tokenize(pattern);
                var index = 0;
                node = ParseExpression(tokens, ref index);
                
                if (node.Type == PatternNodeType.UserDefined && node.Value is string patternName)
                {
                    name = patternName;
                    return true;
                }
            }
            catch
            {
            }
            
            return false;
        }

        private List<string> SplitPatternDefinitions(string input)
        {
            var definitions = new List<string>();
            var depth = 0;
            var current = string.Empty;
            
            for (int i = 0; i < input.Length; i++)
            {
                var ch = input[i];
                current += ch;
                
                if (ch == '(') depth++;
                else if (ch == ')') depth--;
                
                if (depth == 0 && (ch == '\n' || ch == '\r' || i == input.Length - 1))
                {
                    var trimmed = current.Trim();
                    if (!string.IsNullOrEmpty(trimmed))
                    {
                        definitions.Add(trimmed);
                    }
                    current = string.Empty;
                }
            }
            
            return definitions;
        }
    }
}