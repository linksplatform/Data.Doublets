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
        protected override TLink GetSize(TLink node)
        {
            unchecked
            {
                var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset);
                return Bit<TLink>.PartialRead(previousValue, 5, -5);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeft(TLink node, TLink left) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.LeftAsTargetOffset, left);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRight(TLink node, TLink right) => Write(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.RightAsTargetOffset, right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(TLink node, TLink size)
        {
            unchecked
            {
                var linkSizeAsTargetOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset;
                var previousValue = Read<TLink>(linkSizeAsTargetOffset);
                Write(linkSizeAsTargetOffset, Bit<TLink>.PartialWrite(previousValue, size, 5, -5));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetLeftIsChild(TLink node)
        {
            var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset);
            //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 4, 1);
            return !EqualityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 4, 1), default);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLeftIsChild(TLink node, bool value)
        {
            unchecked
            {
                var linkSizeAsTargetOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset;
                var previousValue = Read<TLink>(linkSizeAsTargetOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 4, 1);
                Write(linkSizeAsTargetOffset, modified);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool GetRightIsChild(TLink node)
        {
            unchecked
            {
                var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset);
                //return (Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 3, 1);
                return !EqualityComparer.Equals(Bit<TLink>.PartialRead(previousValue, 3, 1), default);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetRightIsChild(TLink node, bool value)
        {
            unchecked
            {
                var linkSizeAsTargetOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset;
                var previousValue = Read<TLink>(linkSizeAsTargetOffset);
                var modified = Bit<TLink>.PartialWrite(previousValue, (Integer<TLink>)value, 3, 1);
                Write(linkSizeAsTargetOffset, modified);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override sbyte GetBalance(TLink node)
        {
            unchecked
            {
                var previousValue = Read<TLink>(Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset);
                var value = (int)(Integer<TLink>)Bit<TLink>.PartialRead(previousValue, 0, 3);
                value |= 0xF8 * ((value & 4) >> 2); // if negative, then continue ones to the end of sbyte
                return (sbyte)value;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetBalance(TLink node, sbyte value)
        {
            unchecked
            {
                var linkSizeAsTargetOffset = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)node + RawLink<TLink>.SizeAsTargetOffset;
                var previousValue = Read<TLink>(linkSizeAsTargetOffset);
                var packagedValue = (TLink)(Integer<TLink>)((((byte)value >> 5) & 4) | value & 3);
                var modified = Bit<TLink>.PartialWrite(previousValue, packagedValue, 0, 3);
                Write(linkSizeAsTargetOffset, modified);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheLeftOfSecond(TLink first, TLink second)
        {
            var firstLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)first;
            var secondLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)second;
            var firstTarget = Read<TLink>(firstLink + RawLink<TLink>.TargetOffset);
            var secondTarget = Read<TLink>(secondLink + RawLink<TLink>.TargetOffset);
            return LessThan(firstTarget, secondTarget) ||
                   (IsEquals(firstTarget, secondTarget) && LessThan(Read<TLink>(firstLink + RawLink<TLink>.SourceOffset), Read<TLink>(secondLink + RawLink<TLink>.SourceOffset)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override bool FirstIsToTheRightOfSecond(TLink first, TLink second)
        {
            var firstLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)first;
            var secondLink = Links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)second;
            var firstTarget = Read<TLink>(firstLink + RawLink<TLink>.TargetOffset);
            var secondTarget = Read<TLink>(secondLink + RawLink<TLink>.TargetOffset);
            return GreaterThan(firstTarget, secondTarget) ||
                   (IsEquals(firstTarget, secondTarget) && GreaterThan(Read<TLink>(firstLink + RawLink<TLink>.SourceOffset), Read<TLink>(secondLink + RawLink<TLink>.SourceOffset)));
        }

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