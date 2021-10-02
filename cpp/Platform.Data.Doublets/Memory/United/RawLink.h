namespace Platform::Data::Doublets::Memory::United
{
    template<typename TLink>
    struct RawLink
    {
        TLink Source;

        TLink Target;

        TLink LeftAsSource;

        TLink RightAsSource;

        TLink SizeAsSource;

        TLink LeftAsTarget;

        TLink RightAsTarget;

        TLink SizeAsTarget;

        constexpr bool operator==(const RawLink&) const noexcept = default;
    };
}


template<typename TLink>
struct std::hash<Platform::Data::Doublets::Memory::United::RawLink<TLink>>
{
    using Self = Platform::Data::Doublets::Memory::United::RawLink<TLink>;

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