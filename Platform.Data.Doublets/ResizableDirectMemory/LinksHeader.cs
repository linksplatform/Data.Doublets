using Platform.Unsafe;
using System.Runtime.InteropServices;

namespace Platform.Data.Doublets.ResizableDirectMemory
{
    internal struct LinksHeader<TLink>
    {
        public static readonly long SizeInBytes = Structure<LinksHeader<TLink>>.Size;
        public static readonly long AllocatedLinksOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(AllocatedLinks)).ToInt64();
        public static readonly long ReservedLinksOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(ReservedLinks)).ToInt64();
        public static readonly long FreeLinksOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(FreeLinks)).ToInt64();
        public static readonly long FirstFreeLinkOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(FirstFreeLink)).ToInt64();
        public static readonly long FirstAsSourceOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(FirstAsSource)).ToInt64();
        public static readonly long FirstAsTargetOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(FirstAsTarget)).ToInt64();
        public static readonly long LastFreeLinkOffset = Marshal.OffsetOf(typeof(LinksHeader<TLink>), nameof(LastFreeLink)).ToInt64();

        public TLink AllocatedLinks;
        public TLink ReservedLinks;
        public TLink FreeLinks;
        public TLink FirstFreeLink;
        public TLink FirstAsSource;
        public TLink FirstAsTarget;
        public TLink LastFreeLink;
        public TLink Reserved8;
    }
}