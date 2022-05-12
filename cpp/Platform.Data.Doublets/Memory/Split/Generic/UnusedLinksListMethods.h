namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Collections::Methods::Lists;
    template<typename TLinksOptions>
    class UnusedLinksListMethods : public  AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType> /*, ILinksListMethods<typename TLinksOptions::LinkAddressType> */
    {
    public:
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using WriteHandlerType = LinksOptionsType::WriteHandlerType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
        using base = AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>;
    public: static constexpr auto Constants = LinksOptionsType::Constants;
        private: std::byte* _storage;
        private: std::byte* _header;

        public: UnusedLinksListMethods(std::byte* storage, std::byte* header)
        {
            _storage = storage;
            _header = header;
        }

        public: LinksHeader<LinkAddressType>& GetHeaderReference() { *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header); }

        public: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) { *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_storage + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link))); }

        public: LinkAddressType GetFirst() { return this->GetHeaderReference().FirstFreeLink; }

        public: LinkAddressType GetLast() { return this->GetHeaderReference().LastFreeLink; }

        public: LinkAddressType GetPrevious(LinkAddressType element) { return this->GetLinkDataPartReference(element)->Source; }

        public: LinkAddressType GetNext(LinkAddressType element) { return this->GetLinkDataPartReference(element)->Target; }

        public: LinkAddressType GetSize() { return this->GetHeaderReference().FreeLinks; }

        public: void SetFirst(LinkAddressType element) { this->GetHeaderReference().FirstFreeLink = element; }

        public: void SetLast(LinkAddressType element) { this->GetHeaderReference().LastFreeLink = element; }

        public: void SetPrevious(LinkAddressType element, LinkAddressType previous) { this->GetLinkDataPartReference(element)->Source = previous; }

        public: void SetNext(LinkAddressType element, LinkAddressType next) { this->GetLinkDataPartReference(element)->Target = next; }

        public: void SetSize(LinkAddressType size) { this->GetHeaderReference().FreeLinks = size; }
    };
}
