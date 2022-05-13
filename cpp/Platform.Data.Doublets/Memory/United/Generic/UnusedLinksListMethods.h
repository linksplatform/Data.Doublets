namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Collections::Methods::Lists;

    template<typename TLinkAddress>
    class UnusedLinksListMethods
        : public AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinkAddress>, TLinkAddress>,
          public ILinksListMethods<TLinkAddress>
    {
        using base = AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLinkAddress>, TLinkAddress>;

        private: std::byte* _links;

        private: std::byte* _header;

        public: UnusedLinksListMethods(std::byte* storage, std::byte* header)
            : _links(storage), _header(header) {}

        public: auto& GetHeaderReference()
        {
            return *reinterpret_cast<LinksHeader<TLinkAddress>*>(_header);
        }

        public: auto& GetLinkReference(TLinkAddress linkIndex)
        {
            return *(reinterpret_cast<RawLink<TLinkAddress>*>(_links) + linkIndex);
        }

        public: TLinkAddress GetFirst()
        {
            return GetHeaderReference().FirstFreeLink;
        }

        public: TLinkAddress GetLast()
        {
            return GetHeaderReference().LastFreeLink;
        }

        public: TLinkAddress GetPrevious(TLinkAddress element)
        {
            return GetLinkReference(element).Source;
        }

        public: TLinkAddress GetNext(TLinkAddress element)
        {
            return GetLinkReference(element).Target;
        }

        public: TLinkAddress GetSize()
        {
            return GetHeaderReference().FreeLinks;
        }

        public: void SetFirst(TLinkAddress element)
        {
            GetHeaderReference().FirstFreeLink = element;
        }

        public: void SetLast(TLinkAddress element)
        {
            GetHeaderReference().LastFreeLink = element;
        }

        public: void SetPrevious(TLinkAddress element, TLinkAddress previous)
        {
            GetLinkReference(element).Source = previous;
        }

        public: void SetNext(TLinkAddress element, TLinkAddress next)
        {
            GetLinkReference(element).Target = next;
        }

        public: void SetSize(TLinkAddress size)
        {
            GetHeaderReference().FreeLinks = size;
        }

        public: void Detach(TLinkAddress link) { base::Detach(link); }

        public: void AttachAsFirst(TLinkAddress link) { base::AttachAsFirst(link); }
    };
}
