namespace Platform::Data::Doublets::Memory::United::Generic
{
    using namespace Platform::Collections::Methods::Lists;

    template<typename TLink>
    class UnusedLinksListMethods
        : public AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLink>, TLink>,
          public ILinksListMethods<TLink>
    {
        using base = AbsoluteCircularDoublyLinkedListMethods<UnusedLinksListMethods<TLink>, TLink>;

        private: std::byte* _links;

        private: std::byte* _header;

        public: UnusedLinksListMethods(std::byte* links, std::byte* header)
            : _links(links), _header(header) {}

    public: auto& GetHeaderReference()
        {
            return *reinterpret_cast<LinksHeader<TLink>*>(_header);
        }

    public: auto& GetLinkReference(TLink linkIndex)
        {
            return *(reinterpret_cast<RawLink<TLink>*>(_links) + linkIndex);
        }

    public: TLink GetFirst()
        {
            return GetHeaderReference().FirstFreeLink;
        }

    public: TLink GetLast()
        {
            return GetHeaderReference().LastFreeLink;
        }

    public: TLink GetPrevious(TLink element)
        {
            return GetLinkReference(element).Source;
        }

    public: TLink GetNext(TLink element)
        {
            return GetLinkReference(element).Target;
        }

    public: TLink GetSize()
        {
            return GetHeaderReference().FreeLinks;
        }

    public: void SetFirst(TLink element)
        {
            GetHeaderReference().FirstFreeLink = element;
        }

    public: void SetLast(TLink element)
        {
            GetHeaderReference().LastFreeLink = element;
        }

    public: void SetPrevious(TLink element, TLink previous)
        {
            GetLinkReference(element).Source = previous;
        }

    public: void SetNext(TLink element, TLink next)
        {
            GetLinkReference(element).Target = next;
        }

    public: void SetSize(TLink size)
        {
            GetHeaderReference().FreeLinks = size;
        }

    public:
        void Detach(TLink link) override { base::Detach(link); }

        void AttachAsFirst(TLink link) override { base::AttachAsFirst(link); }
    };
}
