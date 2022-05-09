namespace Platform::Data::Doublets::Memory::Split::Generic
{
    template<typename TLinksOptions>
    class UnusedLinksListMethods : public AbsoluteCircularDoublyLinkedListMethods<TLinksOptions>, ILinksListMethods<typename TLinksOptions::LinkAddressType>
    {
        private: std::uint8_t* _storage;
        private: std::uint8_t* _header;

        public: UnusedLinksListMethods(std::uint8_t* storage, std::uint8_t* header)
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
    };
}
