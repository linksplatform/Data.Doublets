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
        protected override TLink GetSize(TLink node)
        {
            var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset);
            return Bit<TLink>.PartialRead(previousValue, 5, -5);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeft(TLink node, TLink left) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsSourceOffset, left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRight(TLink node, TLink right) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsSourceOffset, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(TLink node, TLink size)
        {
            var linkSizeAsSourceOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset;
            var previousValue = Read<TLink>(linkSizeAsSourceOffset);
            Write(linkSizeAsSourceOffset, Bit<TLink>.PartialWrite(previousValue, size, 5, -5));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetLeftIsChild(TLink node)
        {
            var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset);
            //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 4, 1);
            return !EqualityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 4, 1), default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeftIsChild(TLink node, bool value)
        {
            var linkSizeAsSourceOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset;
            var previousValue = Read<TLink>(linkSizeAsSourceOffset);
            var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 4, 1);
            Write(linkSizeAsSourceOffset, modified);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetRightIsChild(TLink node)
        {
            var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset);
            //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 3, 1);
            return !EqualityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 3, 1), default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRightIsChild(TLink node, bool value)
        {
            var linkSizeAsSourceOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset;
            var previousValue = Read<TLink>(linkSizeAsSourceOffset);
            var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 3, 1);
            Write(linkSizeAsSourceOffset, modified);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override sbyte GetBalance(TLink node)
        {
            unchecked
            {
                var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset);
                var value = (int)(Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 0, 3);
                value |= 0xF8 * ((value & 4) >> 2); // if negative, then continue ones to the end of sbyte
                return (sbyte)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBalance(TLink node, sbyte value)
        {
            var linkSizeAsSourceOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsSourceOffset;
            var previousValue = Read<TLink>(linkSizeAsSourceOffset);
            var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
            var modified = Bit<TLink>.PartialWrite(previousValue, packagedValue, 0, 3);
            Write(linkSizeAsSourceOffset, modified);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
        {
            var firstLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)first;
            var secondLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)second;
            var firstSource = Read<TLink>(firstLink + RawLink<TLink>.SourceOffset);
            var secondSource = Read<TLink>(secondLink + RawLink<TLink>.SourceOffset);
            return LessThan(firstSource, secondSource) ||
                   (IsEquals(firstSource, secondSource) && LessThan(Read<TLink>(firstLink + RawLink<TLink>.TargetOffset), Read<TLink>(secondLink + RawLink<TLink>.TargetOffset)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
        {
            var firstLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)first;
            var secondLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)second;
            var firstSource = Read<TLink>(firstLink + RawLink<TLink>.SourceOffset);
            var secondSource = Read<TLink>(secondLink + RawLink<TLink>.SourceOffset);
            return GreaterThan(firstSource, secondSource) ||
                   (IsEquals(firstSource, secondSource) && GreaterThan(Read<TLink>(firstLink + RawLink<TLink>.TargetOffset), Read<TLink>(secondLink + RawLink<TLink>.TargetOffset)));
        }

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