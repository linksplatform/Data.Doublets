namespace Platform::Data::Doublets::Memory::Split
{
    template <typename ...>
    struct RawLinkIndexPart;

    template <typename TLinkAddress>
    struct RawLinkIndexPart<TLinkAddress>
    {
        public: inline static const std::int64_t SizeInBytes = sizeof(RawLinkIndexPart<TLinkAddress>);

        TLinkAddress RootAsSource;
        TLinkAddress LeftAsSource;
        TLinkAddress RightAsSource;
        TLinkAddress SizeAsSource;
        TLinkAddress RootAsTarget;
        TLinkAddress LeftAsTarget;
        TLinkAddress RightAsTarget;
        TLinkAddress SizeAsTarget;

        bool operator ==(RawLinkIndexPart<TLinkAddress> other)
        {
            return RootAsSource == other.RootAsSource
            && LeftAsSource == other.LeftAsSource
            && RightAsSource == other.RightAsSource
            && SizeAsSource == other.SizeAsSource
            && RootAsTarget == other.RootAsTarget
            && LeftAsTarget == other.LeftAsTarget
            && RightAsTarget == other.RightAsTarget
            && SizeAsTarget == other.SizeAsTarget;
        }

        bool operator !=(RawLinkIndexPart<TLinkAddress> other)
        {
            return !(this == other);
        }
    };
}

namespace std
{
    template<typename TLinkAddress>
    struct hash<Platform::Data::Doublets::Memory::Split::RawLinkIndexPart<TLinkAddress>>
    {
        std::size_t operator()(const Platform::Data::Doublets::Memory::Split::RawLinkIndexPart<TLinkAddress>& obj) const
        {
            return Platform::Hashing::Hash(obj.RootAsSource, obj.LeftAsSource, obj.RightAsSource, obj.SizeAsSource, obj.RootAsTarget, obj.LeftAsTarget, obj.RightAsTarget, obj.SizeAsTarget);
        }
    };
}
