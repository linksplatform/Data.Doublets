namespace Platform::Data::Doublets
{
    class LinkExtensions
    {
        public: template <typename TLink> static bool IsFullPoint(Link<TLink> link) { return Point<TLink>.IsFullPoint(link); }

        public: template <typename TLink> static bool IsPartialPoint(Link<TLink> link) { return Point<TLink>.IsPartialPoint(link); }
    };
}
