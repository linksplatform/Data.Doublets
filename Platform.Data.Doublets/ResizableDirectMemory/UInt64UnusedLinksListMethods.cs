using Platform.Collections.Methods.Lists;
using System.Runtime.CompilerServices;

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    public unsafe class UInt64UnusedLinksListMethods : CircularDoublyLinkedListMethods<ulong>, ILinksListMethods<ulong>
    {
        private readonly UInt64RawLink* _links;
        private readonly UInt64LinksHeader* _header;

        internal UInt64UnusedLinksListMethods(UInt64RawLink* links, UInt64LinksHeader* header)
        {
            _links = links;
            _header = header;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetFirst() => _header->FirstFreeLink;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetLast() => _header->LastFreeLink;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetPrevious(ulong element) => _links[element].Source;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetNext(ulong element) => _links[element].Target;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override ulong GetSize() => _header->FreeLinks;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetFirst(ulong element) => _header->FirstFreeLink = element;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetLast(ulong element) => _header->LastFreeLink = element;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetPrevious(ulong element, ulong previous) => _links[element].Source = previous;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetNext(ulong element, ulong next) => _links[element].Target = next;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected override void SetSize(ulong size) => _header->FreeLinks = size;
    }
}
