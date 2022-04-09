namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkDataPart;
    template <typename TLinkAddress> struct RawLinkDataPart<TLinkAddress>
    {
        public: inline static const std::int64_t SizeInBytes = Structure<RawLinkDataPart<TLinkAddress>>.Size;

        public: TLinkAddress Source = 0;
        public: TLinkAddress Target = 0;

        public: bool Equals(RawLinkDataPart<TLinkAddress> other)
            => Source == other.Source
            && Target == other.Target;

        public: override std::int32_t GetHashCode() { return Platform::Hashing::Hash(Source, Target); }
    };
}
