namespace Platform::Data::Doublets
{
    template<std::integral TLinkAddress>
    struct Link
    {
        using value_type = TLinkAddress;
        public: static constexpr Link Null{};

        private: static constexpr LinksConstants<TLinkAddress> _constants{};

        private: static constexpr std::size_t Length = 3;

        public: TLinkAddress Index = _constants.Null;
        public: TLinkAddress Source = _constants.Null;
        public: TLinkAddress Target = _constants.Null;

        public: Link() noexcept = default;
        public: Link(const Link&) noexcept = default;
        public: Link(Link&&) noexcept = default;

        public: auto& operator=(const Link& other) {
            Index = other.Index;
            Source = other.Source;
            Target = other.Target;
            return *this;
        }

        public: bool operator==(const Link&) const noexcept = default;

        public: Link(TLinkAddress index, TLinkAddress source, TLinkAddress target) : Index(index), Source(source), Target(target) {}

        public: Link(std::convertible_to<TLinkAddress> auto ...valuesPack)
            {
                constexpr auto length = sizeof...(valuesPack);
                std::array<TLinkAddress, length> values {static_cast<TLinkAddress>(valuesPack)... };
                if constexpr(0 == length)
                {
                    Index = _constants.Null;
                    Source = _constants.Null;
                    Target = _constants.Null;
                }
                else if constexpr(1 == length)
                {
                    Index = values[0];
                    Source = _constants.Null;
                    Target = _constants.Null;
                }
                else if constexpr(2 == length)
                {
                    Index = values[0];
                    Source = values[1];
                    Target = _constants.Null;
                }
                else if constexpr(3 == length)
                {
                    Index = values[0];
                    Source = values[1];
                    Target = values[2];
                }
                else
                {
                    throw std::invalid_argument("Link: too many values");
                }
            }


        public: Link(std::ranges::range auto&& range)
        {
            for (std::size_t i = 0; auto&& item : range | std::views::take(3))
            {
                (*this)[i++] = item; // TODO: later later use std::forward
            }
        }

        public: bool IsNull() const noexcept
        {
            constexpr auto null = _constants.Null;
            return *this == Link{null, null, null};
        }

        public: auto&& operator[](std::size_t index) const
        {
            using namespace Platform::Ranges; // TODO: EnsureExtensions
            switch (index)
            {
                case _constants.IndexPart:
                    return Index;
                case _constants.SourcePart:
                    return Source;
                case _constants.TargetPart:
                    return Target;
                default:
                    throw std::out_of_range("Link index out of range");
            }
        }

    public: auto&& operator[](std::size_t index)
        {
            using namespace Platform::Ranges; // TODO: EnsureExtensions
            switch (index)
            {
                case _constants.IndexPart:
                    return Index;
                case _constants.SourcePart:
                    return Source;
                case _constants.TargetPart:
                    return Target;
                default:
                    throw std::out_of_range("Link index out of range");
            }
        }


        public: std::size_t size() const noexcept
        {
            return Length;
        }

    public: bool empty() const noexcept { return 0 == size(); }

        // TODO: a little unsafe
        public: auto begin() const noexcept { return &Index; }

        public: auto end() const noexcept { return &Target + 1; }

        public: explicit operator std::string() const { return Index == _constants.Null ? ToString(Source, Target) : ToString(Index, Source, Target); }

        public: friend auto& operator<<(std::ostream& stream, const Link<TLinkAddress>& self) { return stream << static_cast<std::string>(self); }

        public: static std::string ToString(TLinkAddress index, TLinkAddress source, TLinkAddress target) { return std::string("(").append(Platform::Converters::To<std::string>(index)).append(": ").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }

        public: static std::string ToString(TLinkAddress source, TLinkAddress target) { return std::string("(").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }
    };

    template<typename... Args>
    Link(Args...) -> Link<std::common_type_t<Args...>>;

    template<std::ranges::range Range>
    Link(Range) -> Link<std::ranges::range_value_t<Range>>;
}

template<typename TLinkAddress>
struct std::hash<Platform::Data::Doublets::Link<TLinkAddress>>
{
    using Self = Platform::Data::Doublets::Link<TLinkAddress>;
    std::size_t operator()(const Self& self) const noexcept
    {
        return Platform::Hashing::Hash(self.Index, self.Source, self.Target);
    }
};
