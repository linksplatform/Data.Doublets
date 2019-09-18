namespace Platform.Data.Doublets.ResizableDirectMemory
{
    internal struct UInt64LinksHeader
    {
        public ulong AllocatedLinks;
        public ulong ReservedLinks;
        public ulong FreeLinks;
        public ulong FirstFreeLink;
        public ulong FirstAsSource;
        public ulong FirstAsTarget;
        public ulong LastFreeLink;
        public ulong Reserved8;
    }
}