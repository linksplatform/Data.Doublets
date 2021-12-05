

using static System::Runtime::CompilerServices::Unsafe;

namespace Platform::Data::Doublets::Memory::United::Generic
{
    public unsafe class UnusedLinksListMethods<TLink> : public AbsoluteCircularDoublyLinkedListMethods<TLink>, ILinksListMethods<TLink>
    {
        private: static readonly UncheckedConverter<TLink, std::int64_t> _addressToInt64Converter = UncheckedConverter<TLink, std::int64_t>.Default;

        private: readonly std::uint8_t* _links;
        private: readonly std::uint8_t* _header;

        public: UnusedLinksListMethods(std::uint8_t* links, std::uint8_t* header)
        {
            _links = links;
            _header = header;
        }

        protected: virtual ref LinksHeader<TLink> GetHeaderReference() { return ref AsRef<LinksHeader<TLink>>(_header); }

        protected: virtual ref RawLink<TLink> GetLinkReference(TLink link) { return ref AsRef<RawLink<TLink>>(_links + (RawLink<TLink>.SizeInBytes * _addressToInt64Converter.Convert(link))); }

        protected: override TLink GetFirst() { return GetHeaderReference().FirstFreeLink; }

        protected: override TLink GetLast() { return GetHeaderReference().LastFreeLink; }

        protected: TLink GetPrevious(TLink element) override { return this->GetLinkReference(element)->Source; }

        protected: TLink GetNext(TLink element) override { return this->GetLinkReference(element)->Target; }

        protected: override TLink GetSize() { return GetHeaderReference().FreeLinks; }

        protected: void SetFirst(TLink element) override { this->GetHeaderReference().FirstFreeLink = element; }

        protected: void SetLast(TLink element) override { this->GetHeaderReference().LastFreeLink = element; }

        protected: void SetPrevious(TLink element, TLink previous) override { this->GetLinkReference(element)->Source = previous; }

        protected: void SetNext(TLink element, TLink next) override { this->GetLinkReference(element)->Target = next; }

        protected: void SetSize(TLink size) override { this->GetHeaderReference().FreeLinks = size; }
    };
}
