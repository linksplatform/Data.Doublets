using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.Patterns
{
    /// <summary>
    /// <para>
    /// Represents different types of pattern nodes in the pattern AST.
    /// </para>
    /// <para></para>
    /// </summary>
    public enum PatternNodeType
    {
        Any,
        Literal,
        Variable,
        Set,
        Sequence,
        Tree,
        Point,
        PartialPoint,
        Or,
        And,
        Not,
        GreaterThan,
        GreaterThanOrEqual,
        LessThan,
        LessThanOrEqual,
        Incoming,
        Outgoing,
        UserDefined
    }

    /// <summary>
    /// <para>
    /// Represents a node in the pattern abstract syntax tree.
    /// </para>
    /// <para></para>
    /// </summary>
    /// <typeparam name="TLinkAddress">
    /// <para>The link address type.</para>
    /// <para></para>
    /// </typeparam>
    public class PatternNode<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>, IComparisonOperators<TLinkAddress, TLinkAddress, bool>
    {
        /// <summary>
        /// <para>
        /// Gets or sets the type of the pattern node.
        /// </para>
        /// <para></para>
        /// </summary>
        public PatternNodeType Type { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the value associated with the pattern node (for literals, variables, etc.).
        /// </para>
        /// <para></para>
        /// </summary>
        public object? Value { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the child patterns for composite patterns.
        /// </para>
        /// <para></para>
        /// </summary>
        public List<PatternNode<TLinkAddress>> Children { get; set; } = new List<PatternNode<TLinkAddress>>();

        /// <summary>
        /// <para>
        /// Gets or sets the source pattern for doublet patterns.
        /// </para>
        /// <para></para>
        /// </summary>
        public PatternNode<TLinkAddress>? Source { get; set; }

        /// <summary>
        /// <para>
        /// Gets or sets the target pattern for doublet patterns.
        /// </para>
        /// <para></para>
        /// </summary>
        public PatternNode<TLinkAddress>? Target { get; set; }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="PatternNode{TLinkAddress}"/> instance.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="type">
        /// <para>The pattern node type.</para>
        /// <para></para>
        /// </param>
        public PatternNode(PatternNodeType type)
        {
            Type = type;
        }

        /// <summary>
        /// <para>
        /// Initializes a new <see cref="PatternNode{TLinkAddress}"/> instance with a value.
        /// </para>
        /// <para></para>
        /// </summary>
        /// <param name="type">
        /// <para>The pattern node type.</para>
        /// <para></para>
        /// </param>
        /// <param name="value">
        /// <para>The value associated with the node.</para>
        /// <para></para>
        /// </param>
        public PatternNode(PatternNodeType type, object? value) : this(type)
        {
            Value = value;
        }
    }
}