using System.Runtime.CompilerServices;
using Platform.Numbers;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe class LinksTargetsAVLBalancedTreeMethods<TLink> : LinksAVLBalancedTreeMethodsBase<TLink>, ILinksTreeMethods<TLink>
    {
        public LinksTargetsAVLBalancedTreeMethods(ResizableDirectMemoryLinks<TLink> memory, byte* links, byte* header) : base(memory, links, header) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe override ref TLink GetLeftReference(TLink node) => ref AsRef<TLink>((void*)(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe override ref TLink GetRightReference(TLink node) => ref AsRef<TLink>((void*)(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetLeft(TLink node) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsTargetOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetRight(TLink node) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsTargetOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeft(TLink node, TLink left) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsTargetOffset, left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRight(TLink node, TLink right) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsTargetOffset, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetSize(TLink node) => GetSizeValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(TLink node, TLink size) => SetSizeValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset), size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetLeftIsChild(TLink node) => GetLeftIsChildValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeftIsChild(TLink node, bool value) => SetLeftIsChildValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetRightIsChild(TLink node) => GetRightIsChildValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRightIsChild(TLink node, bool value) => SetRightIsChildValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override sbyte GetBalance(TLink node) => GetBalanceValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBalance(TLink node, sbyte value) => SetBalanceValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetTreeRoot() => Read<TLink>(Header + LinksHeader<TLink>.FirstAsTargetOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetBasePartValue(TLink link) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)link + RawLink<TLink>.TargetOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => LessThan(firstTarget, secondTarget) || (IsEquals(firstTarget, secondTarget) && LessThan(firstSource, secondSource));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => GreaterThan(firstTarget, secondTarget) || (IsEquals(firstTarget, secondTarget) && GreaterThan(firstSource, secondSource));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearNode(TLink node)
        {
            byte* link = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node;
            Write(link + RawLink<TLink>.LeftAsTargetOffset, Zero);
            Write(link + RawLink<TLink>.RightAsTargetOffset, Zero);
            Write(link + RawLink<TLink>.SizeAsTargetOffset, Zero);
        }
    }
}