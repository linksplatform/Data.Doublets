

using TLink = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64UnusedLinksListMethods : public UnusedLinksListMethods<TLink>
    {
        private: readonly RawLinkDataPart<std::uint64_t>* _links;
        private: readonly LinksHeader<std::uint64_t>* _header;

        public: UInt64UnusedLinksListMethods(RawLinkDataPart<std::uint64_t>* links, LinksHeader<std::uint64_t>* header)
            : base((std::uint8_t*)links, (std::uint8_t*)header)
        {
            _links = links;
            _header = header;
        }

        protected: override ref RawLinkDataPart<TLink> GetLinkDataPartReference(TLink link) { return &_links[link]; }

        protected: override ref LinksHeader<TLink> GetHeaderReference() { return ref *_header; }
    };
}
