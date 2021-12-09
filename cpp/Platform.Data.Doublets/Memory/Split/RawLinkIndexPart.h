namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkIndexPart;
    template <typename TLink> struct RawLinkIndexPart<TLink>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<RawLinkIndexPart<TLink>>.Size;

        public: TLink RootAsSource = 0;
        public: TLink LeftAsSource = 0;
        public: TLink RightAsSource = 0;
        public: TLink SizeAsSource = 0;
        public: TLink RootAsTarget = 0;
        public: TLink LeftAsTarget = 0;
        public: TLink RightAsTarget = 0;
        public: TLink SizeAsTarget = 0;

        public: bool Equals(RawLinkIndexPart<TLink> other)
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