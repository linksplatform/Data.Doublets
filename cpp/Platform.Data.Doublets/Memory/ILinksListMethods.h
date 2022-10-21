namespace Platform::Data::Doublets::Memory
{
    template<std::integral TLinkAddress>
    struct ILinksListMethods
    {
        void Detach(TLinkAddress freeLink)
            {
                return this->object().Detach(freeLink);
            };

        void AttachAsFirst(TLinkAddress link)
                {
                    return this->object().AttachAsFirst(link);
                };
    };
}
