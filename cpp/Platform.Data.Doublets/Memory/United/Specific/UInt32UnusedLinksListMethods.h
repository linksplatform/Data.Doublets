namespace Platform::Data::Doublets::Memory::United::Specific
{
    public unsafe class UInt32UnusedLinksListMethods : public UnusedLinksListMethods<std::uint32_t>
    {
        private: RawLink<std::uint32_t>* _links;
        private: LinksHeader<std::uint32_t>* _header;

        public: UInt32UnusedLinksListMethods(RawLink<std::uint32_t>* storage, LinksHeader<std::uint32_t>* header)
            : base((std::byte*)storage, (std::byte*)header)
        {
            _links = storage;
            _header = header;
        }

        protected: override ref RawLink<std::uint32_t> GetLinkReference(std::uint32_t link) { return &_links[link]; }

        protected: override ref LinksHeader<std::uint32_t> GetHeaderReference() { return ref *_header; }
    };
}
