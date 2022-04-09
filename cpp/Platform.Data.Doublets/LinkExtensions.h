namespace Platform::Data::Doublets
{
    class LinkExtensions
    {
        public: template <typename TLinkAddress> static bool IsFullPoint(Link<TLinkAddress> link) { return Point<TLinkAddress>.IsFullPoint(link); }

        public: template <typename TLinkAddress> static bool IsPartialPoint(Link<TLinkAddress> link) { return Point<TLinkAddress>.IsPartialPoint(link); }
    };
}
