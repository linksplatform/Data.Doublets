namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkDataPart;
    template <typename TLink> struct RawLinkDataPart<TLink>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<RawLinkDataPart<TLink>>.Size;

        public: TLink Source = 0;
        public: TLink Target = 0;

        public: bool Equals(RawLinkDataPart<TLink> other)
            => Source == other.Source
            && Target == other.Target;

        public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(Source, Target); }
    };
}