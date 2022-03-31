namespace Platform::Data::Doublets::Memory
{
    template<typename TLink>
    struct LinksHeader
    {
        TLink AllocatedLinks;

        TLink ReservedLinks;

        TLink FreeLinks;

        TLink FirstFreeLink;

        TLink RootAsSource;

        TLink RootAsTarget;

        TLink LastFreeLink;

        TLink Reserved8;

        constexpr bool operator==(const LinksHeader&) const noexcept = default;
    };
}

template<typename TLink>
struct std::hash<Platform::Data::Doublets::Memory::LinksHeader<TLink>>
{
    using Self = Platform::Data::Doublets::Memory::LinksHeader<TLink>;

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
