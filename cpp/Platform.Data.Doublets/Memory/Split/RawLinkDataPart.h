namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...> struct RawLinkDataPart;
    template <typename TLinkAddress> struct RawLinkDataPart<TLinkAddress>
    {
    public:
        static const std::int64_t SizeInBytes = sizeof(RawLinkDataPart<TLinkAddress>);

        TLinkAddress Source;
        TLinkAddress Target;

        bool operator =(RawLinkDataPart<TLinkAddress> other)
        {
            return (Source == other.Source) && (Target == other.Target);
        }

    public:
        std::int32_t GetHashCode() override
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
