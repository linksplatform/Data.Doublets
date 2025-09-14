using Platform.Collections.Lists;
using Platform.Exceptions;
using Platform.Ranges;
using Platform.Singletons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Linq;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets
{
    /// <summary>
    /// <para>
    /// Represents a tree-like structure for links instead of flat IList representation.
    /// </para>
    /// </summary>
    /// <typeparam name="TLinkAddress">The type of the link address.</typeparam>
    public struct LinkTree<TLinkAddress> : ILinkTree<TLinkAddress>, IEquatable<LinkTree<TLinkAddress>>
        where TLinkAddress : IUnsignedNumber<TLinkAddress>
    {
        public static readonly LinkTree<TLinkAddress> Null = new LinkTree<TLinkAddress>();
        private static readonly LinksConstants<TLinkAddress> _constants = Default<LinksConstants<TLinkAddress>>.Instance;

        public readonly TLinkAddress Index;
        public readonly TLinkAddress Source;
        public readonly TLinkAddress Target;
        private readonly ILinkTree<TLinkAddress>? _parent;
        private readonly ILinkTree<TLinkAddress>[]? _children;

        TLinkAddress ILinkTree<TLinkAddress>.Index => Index;
        TLinkAddress ILinkTree<TLinkAddress>.Source => Source;
        TLinkAddress ILinkTree<TLinkAddress>.Target => Target;

        public ILinkTree<TLinkAddress>? Parent => _parent;

        public IEnumerable<ILinkTree<TLinkAddress>> Children => 
            _children ?? Enumerable.Empty<ILinkTree<TLinkAddress>>();

        public int ChildrenCount => _children?.Length ?? 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkTree(TLinkAddress index, TLinkAddress source, TLinkAddress target)
        {
            Index = index;
            Source = source;
            Target = target;
            _parent = null;
            _children = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkTree(TLinkAddress index, TLinkAddress source, TLinkAddress target, 
                       ILinkTree<TLinkAddress>? parent, ILinkTree<TLinkAddress>[]? children = null)
        {
            Index = index;
            Source = source;
            Target = target;
            _parent = parent;
            _children = children;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkTree(params TLinkAddress[] values)
        {
            SetValues(values, out Index, out Source, out Target);
            _parent = null;
            _children = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkTree(IList<TLinkAddress>? values)
        {
            SetValues(values, out Index, out Source, out Target);
            _parent = null;
            _children = null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public LinkTree(object other)
        {
            if (other is LinkTree<TLinkAddress> otherTree)
            {
                SetValues(ref otherTree, out Index, out Source, out Target);
                _parent = otherTree._parent;
                _children = otherTree._children;
            }
            else if (other is Link<TLinkAddress> otherLink)
            {
                Index = otherLink.Index;
                Source = otherLink.Source;
                Target = otherLink.Target;
                _parent = null;
                _children = null;
            }
            else if (other is IList<TLinkAddress> otherList)
            {
                SetValues(otherList, out Index, out Source, out Target);
                _parent = null;
                _children = null;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetValues(ref LinkTree<TLinkAddress> other, out TLinkAddress index, out TLinkAddress source, out TLinkAddress target)
        {
            index = other.Index;
            source = other.Source;
            target = other.Target;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetValues(IList<TLinkAddress>? values, out TLinkAddress index, out TLinkAddress source, out TLinkAddress target)
        {
            if (values == null)
            {
                index = default;
                source = default;
                target = default;
                return;
            }
            switch (values.Count)
            {
                case 3:
                    index = values[0];
                    source = values[1];
                    target = values[2];
                    break;
                case 2:
                    index = values[0];
                    source = values[1];
                    target = default;
                    break;
                case 1:
                    index = values[0];
                    source = default;
                    target = default;
                    break;
                default:
                    index = default;
                    source = default;
                    target = default;
                    break;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILinkTree<TLinkAddress>? GetChild(int index)
        {
            if (_children == null || index < 0 || index >= _children.Length)
            {
                return null;
            }
            return _children[index];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILinkTree<TLinkAddress>? GetSourceAsTree()
        {
            // If Source is a valid link address, we could potentially create a tree for it
            // For now, returning null to indicate source is a primitive value
            // This could be enhanced to look up the actual link if needed
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ILinkTree<TLinkAddress>? GetTargetAsTree()
        {
            // If Target is a valid link address, we could potentially create a tree for it
            // For now, returning null to indicate target is a primitive value
            // This could be enhanced to look up the actual link if needed
            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNull() => Index == _constants.Null
                             && Source == _constants.Null
                             && Target == _constants.Null;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TLinkAddress[] ToArray() => new[] { Index, Source, Target };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => (Index, Source, Target).GetHashCode();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is LinkTree<TLinkAddress> && Equals((LinkTree<TLinkAddress>)other);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(LinkTree<TLinkAddress> other) => Index == other.Index
                                                          && Source == other.Source
                                                          && Target == other.Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(LinkTree<TLinkAddress> left, LinkTree<TLinkAddress> right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(LinkTree<TLinkAddress> left, LinkTree<TLinkAddress> right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator TLinkAddress[](LinkTree<TLinkAddress> tree) => tree.ToArray();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator LinkTree<TLinkAddress>(TLinkAddress[] linkArray) => new LinkTree<TLinkAddress>(linkArray);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator LinkTree<TLinkAddress>(Link<TLinkAddress> link) => 
            new LinkTree<TLinkAddress>(link.Index, link.Source, link.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Link<TLinkAddress>(LinkTree<TLinkAddress> tree) => 
            new Link<TLinkAddress>(tree.Index, tree.Source, tree.Target);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => Index == _constants.Null ? 
            Link<TLinkAddress>.ToString(Source, Target) : 
            Link<TLinkAddress>.ToString(Index, Source, Target);
    }
}