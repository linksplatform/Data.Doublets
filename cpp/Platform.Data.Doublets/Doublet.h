namespace Platform::Data::Doublets
{
    // todo! use link concept
    template<typename T>
    struct Doublet
    {
        public: T Source = 0;

        public: T Target = 0;

        public: Doublet(T source = 0, T target = 0)
            : Source(source), Target(target) {}

        public: explicit operator std::string() const { return std::string("").append(Platform::Converters::To<std::string>(Source)).append("->").append(Platform::Converters::To<std::string>(Target)).append(""); }

        public: friend std::ostream& operator<<(std::ostream& stream, const Doublet<T>& self) { return stream << static_cast<std::string>(self); }

        public: bool operator==(const Doublet<T> &other) const = default;
    };

    template<typename... Args>
    Doublet(Args...) -> Doublet<std::common_type_t<Args...>>;
}

template<typename T>
struct std::hash<Platform::Data::Doublets::Doublet<T>>
{
    std::size_t operator()(const Platform::Data::Doublets::Doublet<T>& self) const
    {
        return Platform::Hashing::Hash(self.Source, self.Target);
    }
};
