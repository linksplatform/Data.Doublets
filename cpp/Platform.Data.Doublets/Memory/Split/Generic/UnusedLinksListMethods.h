

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::Split::Generic
{
    class UnusedLinksListMethods : public AbsoluteCircularDoublyLinkedListMethods<TLinkAddress>, ILinksListMethods<TLinkAddress>
    {
        private: static UncheckedConverter<TLinkAddress, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLinkAddress, std::int64_t>.Default;

        private: std::uint8_t* _links;
        private: std::uint8_t* _header;

        public: UnusedLinksListMethods(std::uint8_t* storage, std::uint8_t* header)
        {
            _links = storage;
            _header = header;
        }

        protected: LinksHeader<TLinkAddress>& GetHeaderReference() { *reinterpret_cast<LinksHeader<TLinkAddress>*>(_header); }

        protected: RawLinkDataPart<TLinkAddress>& GetLinkDataPartReference(TLinkAddress link) { *reinterpret_cast<RawLinkDataPart<TLinkAddress>*>(_links + (RawLinkDataPart<TLinkAddress>::SizeInBytes * (link))); }

        protected: override TLinkAddress GetFirst() { return GetHeaderReference().FirstFreeLink; }

        protected: override TLinkAddress GetLast() { return GetHeaderReference().LastFreeLink; }

        protected: TLinkAddress GetPrevious(TLinkAddress element) override { return this->GetLinkDataPartReference(element)->Source; }

        protected: TLinkAddress GetNext(TLinkAddress element) override { return this->GetLinkDataPartReference(element)->Target; }

        protected: override TLinkAddress GetSize() { return GetHeaderReference().FreeLinks; }

        protected: void SetFirst(TLinkAddress element) override { this->GetHeaderReference().FirstFreeLink = element; }

        protected: void SetLast(TLinkAddress element) override { this->GetHeaderReference().LastFreeLink = element; }

        protected: void SetPrevious(TLinkAddress element, TLinkAddress previous) override { this->GetLinkDataPartReference(element)->Source = previous; }

        protected: void SetNext(TLinkAddress element, TLinkAddress next) override { this->GetLinkDataPartReference(element)->Target = next; }

        protected: void SetSize(TLinkAddress size) override { this->GetHeaderReference().FreeLinks = size; }
    };
}
