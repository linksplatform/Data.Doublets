namespace Platform::Data::Doublets::Memory
{
    template <std::integral TLinkAddress>
    struct ILinksListMethods
    {
        virtual ~ILinksListMethods() = default;

        virtual void Detach(TLinkAddress freeLink) = 0;

        virtual void AttachAsFirst(TLinkAddress link) = 0;
    };
} // namespace Platform::Data::Doublets::Memory
