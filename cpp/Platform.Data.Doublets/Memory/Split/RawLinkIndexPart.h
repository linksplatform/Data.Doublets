namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkIndexPart;
    template <typename TLinkAddress> struct RawLinkIndexPart<TLinkAddress>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<RawLinkIndexPart<TLinkAddress>>.Size;

        public: TLinkAddress RootAsSource = 0;
        public: TLinkAddress LeftAsSource = 0;
        public: TLinkAddress RightAsSource = 0;
        public: TLinkAddress SizeAsSource = 0;
        public: TLinkAddress RootAsTarget = 0;
        public: TLinkAddress LeftAsTarget = 0;
        public: TLinkAddress RightAsTarget = 0;
        public: TLinkAddress SizeAsTarget = 0;

        public: bool Equals(RawLinkIndexPart<TLinkAddress> other)
            => RootAsSource == other.RootAsSource
            && LeftAsSource == other.LeftAsSource
            && RightAsSource == other.RightAsSource
            && SizeAsSource == other.SizeAsSource
            && RootAsTarget == other.RootAsTarget
            && LeftAsTarget == other.LeftAsTarget
            && RightAsTarget == other.RightAsTarget
            && SizeAsTarget == other.SizeAsTarget;

        public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(RootAsSource, LeftAsSource, RightAsSource, SizeAsSource, RootAsTarget, LeftAsTarget, RightAsTarget, SizeAsTarget); }
    };
}
