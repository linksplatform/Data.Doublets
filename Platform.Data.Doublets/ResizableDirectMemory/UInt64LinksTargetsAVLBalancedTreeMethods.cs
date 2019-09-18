using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe class UInt64LinksTargetsAVLBalancedTreeMethods : UInt64LinksAVLBalancedTreeMethodsBase, ILinksTreeMethods<ulong>
    {
        internal UInt64LinksTargetsAVLBalancedTreeMethods(UInt64ResizableDirectMemoryLinks memory, UInt64RawLink* links, UInt64LinksHeader* header) : base(memory, links, header) { }

        //protected override IntPtr GetLeft(ulong node) => new IntPtr(&Links[node].LeftAsTarget);

        //protected override IntPtr GetRight(ulong node) => new IntPtr(&Links[node].RightAsTarget);

        //protected override ulong GetSize(ulong node) => Links[node].SizeAsTarget;

        //protected override void SetLeft(ulong node, ulong left) => Links[node].LeftAsTarget = left;

        //protected override void SetRight(ulong node, ulong right) => Links[node].RightAsTarget = right;

        //protected override void SetSize(ulong node, ulong size) => Links[node].SizeAsTarget = size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref ulong GetLeftReference(ulong node) => ref _links[node].LeftAsTarget;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ref ulong GetRightReference(ulong node) => ref _links[node].RightAsTarget;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetLeft(ulong node) => _links[node].LeftAsTarget;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetRight(ulong node) => _links[node].RightAsTarget;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetSize(ulong node) => unchecked((_links[node].SizeAsTarget & 4294967264UL) >> 5);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeft(ulong node, ulong left) => _links[node].LeftAsTarget = left;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRight(ulong node, ulong right) => _links[node].RightAsTarget = right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(ulong node, ulong size)
        {
            unchecked
            {
                ref var storedValue = ref _links[node].SizeAsTarget;
                storedValue = (storedValue & 31UL) | ((size & 134217727UL) << 5);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetLeftIsChild(ulong node)
        {
            unchecked
            {
                return (_links[node].SizeAsTarget & 16UL) >> 4 == 1UL;
                // TODO: Check if this is possible to use
                //var nodeSize = GetSize(node);
                //var left = GetLeft(node);
                //var leftSize = GetSizeOrZero(left);
                //return leftSize > 0 && nodeSize > leftSize; 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeftIsChild(ulong node, bool value)
        {
            unchecked
            {
                ref var storedValue = ref _links[node].SizeAsTarget;
                storedValue = (storedValue & 4294967279UL) | ((As<bool, byte>(ref value) & 1UL) << 4);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetRightIsChild(ulong node)
        {
            unchecked
            {
                return (_links[node].SizeAsTarget & 8) >> 3 == 1UL;
                // TODO: Check if this is possible to use
                //var nodeSize = GetSize(node);
                //var right = GetRight(node);
                //var rightSize = GetSizeOrZero(right);
                //return rightSize > 0 && nodeSize > rightSize; 
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRightIsChild(ulong node, bool value)
        {
            unchecked
            {
                ref var storedValue = ref _links[node].SizeAsTarget;
                storedValue = (storedValue & 4294967287UL) | ((As<bool, byte>(ref value) & 1UL) << 3);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override sbyte GetBalance(ulong node)
        {
            unchecked
            {
                var value = _links[node].SizeAsTarget & 7UL;
                value |= 0xF8UL * ((value & 4UL) >> 2); // if negative, then continue ones to the end of sbyte
                return (sbyte)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBalance(ulong node, sbyte value)
        {
            unchecked
            {
                ref var storedValue = ref _links[node].SizeAsTarget;
                storedValue = (storedValue & 4294967288) | ((ulong)((((byte)value >> 5) & 4) | value & 3) & 7UL);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(ulong first, ulong second)
            => _links[first].Target < _links[second].Target ||
              (_links[first].Target == _links[second].Target && _links[first].Source < _links[second].Source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(ulong first, ulong second)
            => _links[first].Target > _links[second].Target ||
              (_links[first].Target == _links[second].Target && _links[first].Source > _links[second].Source);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetTreeRoot() => _header->FirstAsTarget;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetBasePartValue(ulong link) => _links[link].Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget)
            => firstTarget < secondTarget || (firstTarget == secondTarget && firstSource < secondSource);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(ulong firstSource, ulong firstTarget, ulong secondSource, ulong secondTarget)
            => firstTarget > secondTarget || (firstTarget == secondTarget && firstSource > secondSource);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearNode(ulong node)
        {
            ref UInt64RawLink link = ref _links[node];
            link.LeftAsTarget = 0UL;
            link.RightAsTarget = 0UL;
            link.SizeAsTarget = 0UL;
        }
    }
}