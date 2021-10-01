namespace Platform::Data::Doublets::Memory::United
{
    template <typename ...> struct RawLink;
    template <typename TLink> struct RawLink<TLink>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<RawLink<TLink>>.Size;

        public: TLink Source = 0;
        public: TLink Target = 0;
        public: TLink LeftAsSource = 0;
        public: TLink RightAsSource = 0;
        public: TLink SizeAsSource = 0;
        public: TLink LeftAsTarget = 0;
        public: TLink RightAsTarget = 0;
        public: TLink SizeAsTarget = 0;

        public: bool Equals(RawLink<TLink> other)
            => Source == other.Source
            && Target == other.Target
            && LeftAsSource == other.LeftAsSource
            && RightAsSource == other.RightAsSource
            && SizeAsSource == other.SizeAsSource
            && LeftAsTarget == other.LeftAsTarget
            && RightAsTarget == other.RightAsTarget
            && SizeAsTarget == other.SizeAsTarget;

        public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(Source, Target, LeftAsSource, RightAsSource, SizeAsSource, LeftAsTarget, RightAsTarget, SizeAsTarget); }
    };
}