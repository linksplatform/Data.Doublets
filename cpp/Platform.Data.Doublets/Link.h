namespace Platform::Data::Doublets
{
    template<typename TLink>
    struct Link
    {
        public: static constexpr Link Null{};

        private: static constexpr LinksConstants<TLink> _constants{};

        private: static constexpr std::size_t Length = 3;

        public: TLink Index = _constants.Null;
        public: TLink Source = _constants.Null;
        public: TLink Target = _constants.Null;

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

        public: Link(TLink index, TLink source, TLink target) : Index(index), Source(source), Target(target) {}

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
                    Always::ArgumentInRange(index, Range{0, Length - 1}, "index");
            }
        }


        public: std::size_t size() const noexcept
        {
            return Length;
        }

        // TODO: a little unsafe
        public: auto begin() const noexcept { return &Index; }

        public: auto end() const noexcept { return &Target + 1; }

        public: explicit operator std::string() const { return Index == _constants.Null ? ToString(Source, Target) : ToString(Index, Source, Target); }

        public: friend auto& operator<<(std::ostream& stream, const Link<TLink>& self) { return stream << static_cast<std::string>(self); }

        public: static std::string ToString(TLink index, TLink source, TLink target) { return std::string("(").append(Platform::Converters::To<std::string>(index)).append(": ").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }

        public: static std::string ToString(TLink source, TLink target) { return std::string("(").append(Platform::Converters::To<std::string>(source)).append("->").append(Platform::Converters::To<std::string>(target)).append(1, ')'); }
    };

    template<typename... Args>
    Link(Args...) -> Link<std::common_type_t<Args...>>;

    template<std::ranges::range Range>
    Link(Range) -> Link<std::ranges::range_value_t<Range>>;
}

template<typename TLink>
struct std::hash<Platform::Data::Doublets::Link<TLink>>
{
    using Self = Platform::Data::Doublets::Link<TLink>;
    std::size_t operator()(const Self& self) const noexcept
    {
        return Platform::Hashing::Hash(self.Index, self.Source, self.Target);
    }
};
