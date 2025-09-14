using System;
using System.Collections.Generic;
using System.Numerics;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Defines a tree-like interface for representing links in hierarchical structure
    /// instead of flat IList&lt;TLinkAddress&gt; representation.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    public interface ILinkTree<TLinkAddress> where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        /// <summary>
        /// Gets the index (identifier) of this link.
        /// </summary>
        TLinkAddress Index { get; }

        /// <summary>
        /// Gets the source of this link.
        /// </summary>
        TLinkAddress Source { get; }

        /// <summary>
        /// Gets the target of this link.
        /// </summary>
        TLinkAddress Target { get; }

        /// <summary>
        /// Gets the parent link in the tree structure, if any.
        /// </summary>
        ILinkTree<TLinkAddress>? Parent { get; }

        /// <summary>
        /// Gets the children of this link in the tree structure.
        /// </summary>
        IEnumerable<ILinkTree<TLinkAddress>> Children { get; }

        /// <summary>
        /// Gets the number of children in this tree node.
        /// </summary>
        int ChildrenCount { get; }

        /// <summary>
        /// Gets the child at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the child.</param>
        /// <returns>The child at the specified index, or null if index is out of range.</returns>
        ILinkTree<TLinkAddress>? GetChild(int index);

        /// <summary>
        /// Gets the source as a tree node, if it represents a link.
        /// </summary>
        /// <returns>The source as a tree node, or null if source is not a link.</returns>
        ILinkTree<TLinkAddress>? GetSourceAsTree();

        /// <summary>
        /// Gets the target as a tree node, if it represents a link.
        /// </summary>
        /// <returns>The target as a tree node, or null if target is not a link.</returns>
        ILinkTree<TLinkAddress>? GetTargetAsTree();

        /// <summary>
        /// Determines whether this tree represents a null/empty link.
        /// </summary>
        /// <returns>True if this is a null link, false otherwise.</returns>
        bool IsNull();

        /// <summary>
        /// Converts this tree structure to a flat array representation for compatibility.
        /// </summary>
        /// <returns>An array containing [Index, Source, Target].</returns>
        TLinkAddress[] ToArray();
    }
}