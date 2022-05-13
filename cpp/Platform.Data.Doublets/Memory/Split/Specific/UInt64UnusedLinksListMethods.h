

using TLinkAddress = std::uint64_t;

namespace Platform::Data::Doublets::Memory::Split::Specific
{
    public unsafe class UInt64UnusedLinksListMethods : public UnusedLinksListMethods<TLinkAddress>
    {
        private: RawLinkDataPart<std::uint64_t>* _links;
        private: LinksHeader<std::uint64_t>* _header;

        public: UInt64UnusedLinksListMethods(RawLinkDataPart<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header)
            : base((std::byte*)storage, (std::byte*)header)
        {
            _links = storage;
            _header = header;
        }

        public: ref RawLinkDataPart<TLinkAddress> GetLinkDataPartReference(TLinkAddress link) { return &_links[link]; }

        public: ref LinksHeader<TLinkAddress> GetHeaderReference() { return ref *_header; }
    };
}
