

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    class UnusedLinksListMethods : public AbsoluteCircularDoublyLinkedListMethods<LinkAddressType>, ILinksListMethods<LinkAddressType>
    {
        private: static UncheckedConverter<LinkAddressType, std::int64_t> _addressToInt64Converter = UncheckedConverter<LinkAddressType, std::int64_t>.Default;

        private: std::uint8_t* _links;
        private: std::uint8_t* _header;

        public: UnusedLinksListMethods(std::uint8_t* storage, std::uint8_t* header)
        {
            _links = storage;
            _header = header;
        }

        protected: LinksHeader<LinkAddressType>& GetHeaderReference() { *reinterpret_cast<LinksHeader<LinkAddressType>*>(_header); }

        protected: RawLinkDataPart<LinkAddressType>& GetLinkDataPartReference(LinkAddressType link) { *reinterpret_cast<RawLinkDataPart<LinkAddressType>*>(_links + (RawLinkDataPart<LinkAddressType>::SizeInBytes * (link))); }

        protected: override LinkAddressType GetFirst() { return this->GetHeaderReference().FirstFreeLink; }

        protected: override LinkAddressType GetLast() { return this->GetHeaderReference().LastFreeLink; }

        protected: LinkAddressType GetPrevious(LinkAddressType element) override { return this->GetLinkDataPartReference(element)->Source; }

        protected: LinkAddressType GetNext(LinkAddressType element) override { return this->GetLinkDataPartReference(element)->Target; }

        protected: override LinkAddressType GetSize() { return this->GetHeaderReference().FreeLinks; }

        protected: void SetFirst(LinkAddressType element) override { this->GetHeaderReference().FirstFreeLink = element; }

        protected: void SetLast(LinkAddressType element) override { this->GetHeaderReference().LastFreeLink = element; }

        protected: void SetPrevious(LinkAddressType element, LinkAddressType previous) override { this->GetLinkDataPartReference(element)->Source = previous; }

        protected: void SetNext(LinkAddressType element, LinkAddressType next) override { this->GetLinkDataPartReference(element)->Target = next; }

        protected: void SetSize(LinkAddressType size) override { this->GetHeaderReference().FreeLinks = size; }
    };
}
