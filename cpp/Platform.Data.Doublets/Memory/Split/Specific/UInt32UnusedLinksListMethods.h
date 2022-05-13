

using TLinkAddress = std::uint32_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt32UnusedLinksListMethods : public UnusedLinksListMethods<TLinkAddress>
    {
        private: RawLinkDataPart<TLinkAddress>* _links;
        private: LinksHeader<TLinkAddress>* _header;

        public: UInt32UnusedLinksListMethods(RawLinkDataPart<TLinkAddress>* storage, LinksHeader<TLinkAddress>* header)
            : base((std::byte*)storage, (std::byte*)header)
        {
            _links = storage;
            _header = header;
        }

        public: ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return &_links[link]; }

        public: ref LinksHeader<TLinkAddress> GetHeaderReference() { return ref *_header; }
    };
}
