namespace Platform::Data::Doublets::Memory
{
    template <typename ...> struct LinksHeader;
    template <typename TLink> struct LinksHeader<TLink>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<LinksHeader<TLink>>.Size;

        public: TLink AllocatedLinks = 0;
        public: TLink ReservedLinks = 0;
        public: TLink FreeLinks = 0;
        public: TLink FirstFreeLink = 0;
        public: TLink RootAsSource = 0;
        public: TLink RootAsTarget = 0;
        public: TLink LastFreeLink = 0;
        public: TLink Reserved8 = 0;

        public: bool Equals(LinksHeader<TLink> other)
            => AllocatedLinks == other.AllocatedLinks
            && ReservedLinks == other.ReservedLinks
            && FreeLinks == other.FreeLinks
            && FirstFreeLink == other.FirstFreeLink
            && RootAsSource == other.RootAsSource
            && RootAsTarget == other.RootAsTarget
            && LastFreeLink == other.LastFreeLink
            && Reserved8 == other.Reserved8;

        public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(AllocatedLinks, ReservedLinks, FreeLinks, FirstFreeLink, RootAsSource, RootAsTarget, LastFreeLink, Reserved8); }
    };
}