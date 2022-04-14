namespace Platform::Data::Doublets::Memory
{
    template<typename TLinkAddress>
    struct LinksHeader
    {
        TLinkAddress AllocatedLinks;

        TLinkAddress ReservedLinks;

        TLinkAddress FreeLinks;

        TLinkAddress FirstFreeLink;

        TLinkAddress RootAsSource;

        TLinkAddress RootAsTarget;

        TLinkAddress LastFreeLink;

        TLinkAddress Reserved8;

        constexpr bool operator==(const LinksHeader&) const noexcept = default;
    };
}

template<typename TLinkAddress>
struct std::hash<Platform::Data::Doublets::Memory::LinksHeader<TLinkAddress>>
{
    using Self = Platform::Data::Doublets::Memory::LinksHeader<TLinkAddress>;

    auto operator()(const Self& self) const noexcept
    {
        using Platform::Hashing::Hash;
        return Hash(
            self.AllocatedLinks,
            self.ReservedLinks,
            self.FreeLinks,
            self.FirstFreeLink,
            self.RootAsSource,
            self.RootAsTarget,
            self.LastFreeLink,
            self.Reserved8
        );
    }
};
