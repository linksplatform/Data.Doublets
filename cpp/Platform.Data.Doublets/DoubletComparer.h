namespace Platform::Data::Doublets
{
    template <typename ...> class DoubletComparer;
    template <typename T> class DoubletComparer<T> : public IEqualityComparer<Doublet<T>>
    {
        public: inline static DoubletComparer<T> Default;

        public: bool operator ==(const Doublet<T> x, Doublet<T> &y) const { return x.Equals(y); }

        public: std::int32_t GetHashCode(Doublet<T> obj) { return obj.GetHashCode(); }
    };
}
