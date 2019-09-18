using Platform.Collections.Methods.Lists;
using Platform.Numbers;
using System.Runtime.CompilerServices;
using static System.Runtime.CompilerServices.Unsafe;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe class UnusedLinksListMethods<TLink> : CircularDoublyLinkedListMethods<TLink>, ILinksListMethods<TLink>
    {
        private readonly byte* _links;
        private readonly byte* _header;

        public UnusedLinksListMethods(byte* links, byte* header)
        {
            _links = links;
            _header = header;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetFirst() => Read<TLink>(_header + LinksHeader<TLink>.FirstFreeLinkOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetLast() => Read<TLink>(_header + LinksHeader<TLink>.LastFreeLinkOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetPrevious(TLink element) => Read<TLink>(_links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)element + RawLink<TLink>.SourceOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetNext(TLink element) => Read<TLink>(_links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)element + RawLink<TLink>.TargetOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override TLink GetSize() => Read<TLink>(_header + LinksHeader<TLink>.FreeLinksOffset);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetFirst(TLink element) => Write(_header + LinksHeader<TLink>.FirstFreeLinkOffset, element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLast(TLink element) => Write(_header + LinksHeader<TLink>.LastFreeLinkOffset, element);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPrevious(TLink element, TLink previous) => Write(_links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)element + RawLink<TLink>.SourceOffset, previous);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetNext(TLink element, TLink next) => Write(_links + RawLink<TLink>.SizeInBytes * (Integer<TLink>)element + RawLink<TLink>.TargetOffset, next);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(TLink size) => Write(_header + LinksHeader<TLink>.FreeLinksOffset, size);
    }
}
