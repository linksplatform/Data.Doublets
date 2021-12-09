namespace Platform::Data::Doublets
{
    template <typename ...> struct Doublet;
    template <typename T> struct Doublet<T>
    {
        public: T Source = 0;

        public: T Target = 0;

        public: Doublet(T source, T target)
        {
            Source = source;
            Target = target;
        }

        public: operator std::string() const { return std::string("").append(Platform::Converters::To<std::string>(Source)).append("->").append(Platform::Converters::To<std::string>(Target)).append(""); }

        public: friend std::ostream & operator <<(std::ostream &out, const Doublet<T> &obj) { return out << (std::string)obj; }

        public: bool operator ==(const Doublet<T> &other) const { return Source == other.Source && Target == other.Target; }

        public: bool Equals(void *obj) override { return obj is Doublet<T> doublet ? base.Equals(doublet) : false; }
    };
}

namespace std
{
    template <typename T>
    struct hash<Platform::Data::Doublets::Doublet<T>>
    {
        std::size_t operator()(const Platform::Data::Doublets::Doublet<T> &obj) const
        {
            return Platform::Hashing::Hash(obj.Source, obj.Target);
        }
    };
}
