namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt64UnusedLinksListMethods : public UnusedLinksListMethods<std::uint64_t>
    {
        private: RawLink<std::uint64_t>* _links;
        private: LinksHeader<std::uint64_t>* _header;

        public: UInt64UnusedLinksListMethods(RawLink<std::uint64_t>* storage, LinksHeader<std::uint64_t>* header)
            : base((std::uint8_t*)storage, (std::uint8_t*)header)
        {
            _links = storage;
            _header = header;
        }

        protected: override ref RawLink<std::uint64_t> GetLinkReference(std::uint64_t link) { return &_links[link]; }

        protected: override ref LinksHeader<std::uint64_t> GetHeaderReference() { return ref *_header; }
    };
}
