using System.Runtime.CompilerServices;
using Platform.Numbers;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe class LinksSourcesAVLBalancedTreeMethods<TLink> : LinksAVLBalancedTreeMethodsBase<TLink>, ILinksTreeMethods<TLink>
    {
        public LinksSourcesAVLBalancedTreeMethods(ResizableDirectMemoryLinks<TLink> memory, byte* links, byte* header) : base(memory, links, header) { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe override ref TLink GetLeftReference(TLink node) => ref AsRef<TLink>((void*)(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected unsafe override ref TLink GetRightReference(TLink node) => ref AsRef<TLink>((void*)(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetLeft(TLink node) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsSourceOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetRight(TLink node) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsSourceOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeft(TLink node, TLink left) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsSourceOffset, left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRight(TLink node, TLink right) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsSourceOffset, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetSize(TLink node) => GetSizeValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(TLink node, TLink size) => SetSizeValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset), size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetLeftIsChild(TLink node) => GetLeftIsChildValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeftIsChild(TLink node, bool value) => SetLeftIsChildValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetRightIsChild(TLink node) => GetRightIsChildValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRightIsChild(TLink node, bool value) => SetRightIsChildValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override sbyte GetBalance(TLink node) => GetBalanceValue(Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBalance(TLink node, sbyte value) => SetBalanceValue(ref AsRef<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset), value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetTreeRoot() => Read<TLink>(Header + LinksHeader<TLink>.FirstAsSourceOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetBasePartValue(TLink link) => Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)link + RawLink<TLink>.SourceOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => LessThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && LessThan(firstTarget, secondTarget));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(TLink firstSource, TLink firstTarget, TLink secondSource, TLink secondTarget) => GreaterThan(firstSource, secondSource) || (IsEquals(firstSource, secondSource) && GreaterThan(firstTarget, secondTarget));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void ClearNode(TLink node)
        {
            byte* link = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node;
            Write(link + RawLink<TLink>.LeftAsSourceOffset, Zero);
            Write(link + RawLink<TLink>.RightAsSourceOffset, Zero);
            Write(link + RawLink<TLink>.SizeAsSourceOffset, Zero);
        }
    }
}