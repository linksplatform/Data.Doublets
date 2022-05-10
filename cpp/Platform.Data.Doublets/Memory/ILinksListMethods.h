namespace Platform::Data::Doublets::Memory
{
    template<typename TLinkAddress>
    struct ILinksListMethods
    {
        virtual void Detach(TLinkAddress freeLink) = 0;

        virtual void AttachAsFirst(TLinkAddress link) = 0;
    };
}
