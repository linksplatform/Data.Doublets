using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Platform.Collections.Methods.Trees;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe abstract class UInt64LinksAVLBalancedTreeMethodsBase : SizedAndThreadedAVLBalancedTreeMethods<ulong>
    {
        private readonly UInt64ResizableDirectMemoryLinks _memory;
        private readonly LinksConstants<ulong> _constants;
        internal readonly UInt64RawLink* _links;
        internal readonly UInt64LinksHeader* _header;

        internal UInt64LinksAVLBalancedTreeMethodsBase(UInt64ResizableDirectMemoryLinks memory, UInt64RawLink* links, UInt64LinksHeader* header)
        {
            _links = links;
            _header = header;
            _memory = memory;
            _constants = memory.Constants;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetZero() => 0UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool EqualToZero(ulong value) => value == 0UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool IsEquals(ulong first, ulong second) => first == second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GreaterThanZero(ulong value) => value > 0UL;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GreaterThan(ulong first, ulong second) => first > second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GreaterOrEqualThan(ulong first, ulong second) => first >= second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GreaterOrEqualThanZero(ulong value) => true; // value >= 0 is always true for ulong

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool LessOrEqualThanZero(ulong value) => value == 0; // value is always >= 0 for ulong

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool LessOrEqualThan(ulong first, ulong second) => first <= second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool LessThanZero(ulong value) => false; // value < 0 is always false for ulong

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool LessThan(ulong first, ulong second) => first < second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong Increment(ulong value) => ++value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong Decrement(ulong value) => --value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong Add(ulong first, ulong second) => first + second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong Subtract(ulong first, ulong second) => first - second;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong GetTreeRoot();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract ulong GetBasePartValue(ulong link);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheLeftOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected abstract bool FirstIsToTheRightOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
        {
            ref var firstLink = ref _links[first];
            ref var secondLink = ref _links[second];
            return FirstIsToTheLeftOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
        {
            ref var firstLink = ref _links[first];
            ref var secondLink = ref _links[second];
            return FirstIsToTheRightOfSecond(firstLink.Source, firstLink.Target, secondLink.Source, secondLink.Target);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static ulong GetSizeValue(ulong value) => unchecked((value & 4294967264UL) >> 5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SetSizeValue(ref ulong storedValue, ulong size) => storedValue = unchecked((storedValue & 31UL) | ((size & 134217727UL) << 5));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool GetLeftIsChildValue(ulong value) => unchecked((value & 16UL) >> 4 == 1UL);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SetLeftIsChildValue(ref ulong storedValue, bool value) => storedValue = unchecked((storedValue & 4294967279UL) | ((As<bool, byte>(ref value) & 1UL) << 4));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static bool GetRightIsChildValue(ulong value) => unchecked((value & 8UL) >> 3 == 1UL);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SetRightIsChildValue(ref ulong storedValue, bool value) => storedValue = unchecked((storedValue & 4294967287UL) | ((As<bool, byte>(ref value) & 1UL) << 3));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static sbyte GetBalanceValue(ulong value) => unchecked((sbyte)((value & 7UL) | 0xF8UL * ((value & 4UL) >> 2))); // if negative, then continue ones to the end of sbyte

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected static void SetBalanceValue(ref ulong storedValue, sbyte value) => storedValue = unchecked((storedValue & 4294967288UL) | ((ulong)((((byte)value >> 5) & 4) | value & 3) & 7UL));

        public ulong this[ulong index]
        {
            get
            {
                var root = GetTreeRoot();
                if (index >= GetSize(root))
                {
                    return 0;
                }
                while (root != 0)
                {
                    var left = GetLeftOrDefault(root);
                    var leftSize = GetSizeOrZero(left);
                    if (index < leftSize)
                    {
                        root = left;
                        continue;
                    }
                    if (index == leftSize)
                    {
                        return root;
                    }
                    root = GetRightOrDefault(root);
                    index -= leftSize + 1;
                }
                return 0; // TODO: Impossible situation exception (only if tree structure broken)
            }
        }

        /// <summary>
        /// Выполняет поиск и возвращает индекс связи с указанными Source (началом) и Target (концом).
        /// </summary>
        /// <param name="source">Индекс связи, которая является началом на искомой связи.</param>
        /// <param name="target">Индекс связи, которая является концом на искомой связи.</param>
        /// <returns>Индекс искомой связи.</returns>
        public ulong Search(ulong source, ulong target)
        {
            var root = GetTreeRoot();
            while (root != 0)
            {
                var rootLink = _links[root];
                var rootSource = rootLink.Source;
                var rootTarget = rootLink.Target;
                if (FirstIsToTheLeftOfSecond(source, target, rootSource, rootTarget)) // node.Key < root.Key
                {
                    root = GetLeftOrDefault(root);
                }
                else if (FirstIsToTheRightOfSecond(source, target, rootSource, rootTarget)) // node.Key > root.Key
                {
                    root = GetRightOrDefault(root);
                }
                else // node.Key == root.Key
                {
                    return root;
                }
            }
            return 0;
        }

        // TODO: Return indices range instead of references count
        public ulong CountUsages(ulong link)
        {
            var root = GetTreeRoot();
            var total = GetSize(root);
            var totalRightIgnore = 0UL;
            while (root != 0)
            {
                var @base = GetBasePartValue(root);
                if (@base <= link)
                {
                    root = GetRightOrDefault(root);
                }
                else
                {
                    totalRightIgnore += GetRightSize(root) + 1;
                    root = GetLeftOrDefault(root);
                }
            }
            root = GetTreeRoot();
            var totalLeftIgnore = 0UL;
            while (root != 0)
            {
                var @base = GetBasePartValue(root);
                if (@base >= link)
                {
                    root = GetLeftOrDefault(root);
                }
                else
                {
                    totalLeftIgnore += GetLeftSize(root) + 1;
                    root = GetRightOrDefault(root);
                }
            }
            return total - totalRightIgnore - totalLeftIgnore;
        }

        public ulong EachUsage(ulong link, Func<IList<ulong>, ulong> handler)
        {
            var root = GetTreeRoot();
            if (root == 0)
            {
                return _constants.Continue;
            }
            ulong first = 0, current = root;
            while (current != 0)
            {
                var @base = GetBasePartValue(current);
                if (@base >= link)
                {
                    if (@base == link)
                    {
                        first = current;
                    }
                    current = GetLeftOrDefault(current);
                }
                else
                {
                    current = GetRightOrDefault(current);
                }
            }
            if (first != 0)
            {
                current = first;
                while (true)
                {
                    if (handler(_memory.GetLinkStruct(current)) == _constants.Break)
                    {
                        return _constants.Break;
                    }
                    current = GetNext(current);
                    if (current == 0 || GetBasePartValue(current) != link)
                    {
                        break;
                    }
                }
            }
            return _constants.Continue;
        }

        protected override void PrintNodeValue(ulong node, StringBuilder sb)
        {
            ref var link = ref _links[node];
            sb.Append(' ');
            sb.Append(link.Source);
            sb.Append('-');
            sb.Append('>');
            sb.Append(link.Target);
        }
    }
}