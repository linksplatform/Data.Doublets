namespace Platform::Data::Doublets::Memory::Split::Generic
{
    using namespace Platform::Collections::Methods::Lists;
    template<typename TLinksOptions>
    class UnusedLinksListMethods : public  AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinksOptions>, typename TLinksOptions::LinkAddressType>, ILinksListMethods<typename TLinksOptions::LinkAddressType>
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

        protected: LinksHeader<LinkAddressType>& GetHeaderReference() { *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header); }

        protected: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) { *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_storage + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link))); }

        protected: LinkAddressType GetFirst() { return this->GetHeaderReference().FirstFreeLink; }

        protected: LinkAddressType GetLast() { return this->GetHeaderReference().LastFreeLink; }

        protected: LinkAddressType GetPrevious(LinkAddressType element) { return this->GetLinkDataPartReference(element)->Source; }

        protected: LinkAddressType GetNext(LinkAddressType element) { return this->GetLinkDataPartReference(element)->Target; }

        protected: LinkAddressType GetSize() { return this->GetHeaderReference().FreeLinks; }

        protected: void SetFirst(LinkAddressType element) { this->GetHeaderReference().FirstFreeLink = element; }

        protected: void SetLast(LinkAddressType element) { this->GetHeaderReference().LastFreeLink = element; }

        protected: void SetPrevious(LinkAddressType element, LinkAddressType previous) { this->GetLinkDataPartReference(element)->Source = previous; }

        protected: void SetNext(LinkAddressType element, LinkAddressType next) { this->GetLinkDataPartReference(element)->Target = next; }

        protected: void SetSize(LinkAddressType size) { this->GetHeaderReference().FreeLinks = size; }

        public: void Detach(LinkAddressType freeLink)
        {
            return base::Detach(freeLink);
        };
    };
}
