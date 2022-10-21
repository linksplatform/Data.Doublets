namespace Platform::Data::Doublets::Memory::United
{
    template<std::integral TLinkAddress>
    struct RawLink
    {
        TLinkAddress Source;

        TLinkAddress Target;

        TLinkAddress LeftAsSource;

        TLinkAddress RightAsSource;

        TLinkAddress SizeAsSource;

        TLinkAddress LeftAsTarget;

        TLinkAddress RightAsTarget;

        TLinkAddress SizeAsTarget;

        constexpr bool operator==(const RawLink&) const noexcept = default;
    };
}


template<std::integral TLinkAddress>
struct std::hash<Platform::Data::Doublets::Memory::United::RawLink<TLinkAddress>>
{
    using Self = Platform::Data::Doublets::Memory::United::RawLink<TLinkAddress>;

    auto operator()(const Self& self) const noexcept
    {
        using Platform::Hashing::Hash;
        return Hash(
            self.Source,
            self.Target,
            self.LeftAsSource,
            self.RightAsSource,
            self.SizeAsSource,
            self.LeftAsTarget,
            self.RightAsTarget,
            self.SizeAsTarget
        );
    }
};
