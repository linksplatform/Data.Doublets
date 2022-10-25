namespace Platform::Data::Doublets::Memory::United::Generic
{

    template<typename TLinksOptions>
    class UnusedLinksListMethods
    : public Platform::Collections::Methods::Lists::AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>,
          public ILinksListMethods<typename TLinksOptions::LinkAddressType>
    {
        using base = Platform::Collections::Methods::Lists::AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>;
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;

        private: std::byte* _links;

        private: std::byte* _header;

        public: UnusedLinksListMethods(std::byte* storage, std::byte* header)
            : _links(storage), _header(header) {}

        public: auto& GetHeaderReference()
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header);
        }

        public: auto& GetLinkReference(LinkAddressType linkIndex)
        {
            return *(reinterpret_cast<RawLink<LinkAddressType>*>(_links) + linkIndex);
        }

        public: LinkAddressType GetFirst()
        {
            return GetHeaderReference().FirstFreeLink;
        }

        public: LinkAddressType GetLast()
        {
            return GetHeaderReference().LastFreeLink;
        }

        public: LinkAddressType GetPrevious(LinkAddressType element)
        {
            return GetLinkReference(element).Source;
        }

        public: LinkAddressType GetNext(LinkAddressType element)
        {
            return GetLinkReference(element).Target;
        }

        public: LinkAddressType GetSize()
        {
            return GetHeaderReference().FreeLinks;
        }

        public: void SetFirst(LinkAddressType element)
        {
            GetHeaderReference().FirstFreeLink = element;
        }

        public: void SetLast(LinkAddressType element)
        {
            GetHeaderReference().LastFreeLink = element;
        }

        public: void SetPrevious(LinkAddressType element, LinkAddressType previous)
        {
            GetLinkReference(element).Source = previous;
        }

        public: void SetNext(LinkAddressType element, LinkAddressType next)
        {
            GetLinkReference(element).Target = next;
        }

        public: void SetSize(LinkAddressType size)
        {
            GetHeaderReference().FreeLinks = size;
        }

        public: void Detach(LinkAddressType link) { base::Detach(link); }

        public: void AttachAsFirst(LinkAddressType link) { base::AttachAsFirst(link); }
    };
}
