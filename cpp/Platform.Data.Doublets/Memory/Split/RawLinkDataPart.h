namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkDataPart;
    template <std::integral TLinkAddress> struct RawLinkDataPart<TLinkAddress>
    {
    public:
        constexpr static std::size_t SizeInBytes = sizeof(RawLinkDataPart<TLinkAddress>);

        TLinkAddress Source;
        TLinkAddress Target;

        bool operator =(RawLinkDataPart<TLinkAddress> other)
        {
            return (Source == other.Source) && (Target == other.Target);
        }

    public:
        std::int32_t GetHashCode()
        {
            return Platform::Hashing::Hash(Source, Target);
        }

        bool operator ==(RawLinkDataPart<TLinkAddress> other)
        {
            return (Source == other.Source) && (Target == other.Target);
        }

        bool operator !=(RawLinkDataPart<TLinkAddress> other)
        {
            return !(this == other);
        }
    };
}

namespace std
{
    template <std::integral TLinkAddress>
    struct hash<Platform::Data::Doublets::Memory::Split::RawLinkDataPart<TLinkAddress>>
    {
        std::size_t operator()(const Platform::Data::Doublets::Memory::Split::RawLinkDataPart<TLinkAddress> &obj) const
        {
            return obj.GetHashCode();
        }
    };
}
