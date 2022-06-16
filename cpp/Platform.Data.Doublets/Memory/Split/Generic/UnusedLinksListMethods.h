namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class UnusedLinksListMethods : public  Platform::Collections::Methods::Lists::AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType> /*, ILinksListMethods<typename TLinksOptions::LinkAddressType> */
    {
    public:
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using WriteHandlerType = typename LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;
        using base = Platform::Collections::Methods::Lists::AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>;
    public: static constexpr auto Constants = LinksOptionsType::Constants;
        private: std::byte* _storage;
        private: std::byte* _header;

        public: UnusedLinksListMethods(std::byte* storage, std::byte* header)
        {
            _storage = storage;
            _header = header;
        }

    public: const LinksHeader<LinkAddressType>& GetHeaderReference() const
        {
            return *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header);
        }

        public: LinksHeader<LinkAddressType>& GetHeaderReference()
            {
                return *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header);
            }

        public: const RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) const
            {
                return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_storage + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link)));
            }

        public: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link)
            {
                return *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_storage + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link)));
            }

        public: LinkAddressType GetFirst() { return this->GetHeaderReference().FirstFreeLink; }

        public: LinkAddressType GetLast() { return this->GetHeaderReference().LastFreeLink; }

        public: LinkAddressType GetPrevious(LinkAddressType element) { return this->GetLinkDataPartReference(element).Source; }

        public: LinkAddressType GetNext(LinkAddressType element) { return this->GetLinkDataPartReference(element).Target; }

        public: LinkAddressType GetSize() { return this->GetHeaderReference().FreeLinks; }

        public: void SetFirst(LinkAddressType element) { this->GetHeaderReference().FirstFreeLink = element; }

        public: void SetLast(LinkAddressType element) { this->GetHeaderReference().LastFreeLink = element; }

        public: void SetPrevious(LinkAddressType element, LinkAddressType previous) { this->GetLinkDataPartReference(element).Source = previous; }

        public: void SetNext(LinkAddressType element, LinkAddressType next) { this->GetLinkDataPartReference(element).Target = next; }

        public: void SetSize(LinkAddressType size) { this->GetHeaderReference().FreeLinks = size; }
    };
}
