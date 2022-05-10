namespace Platform::Data::Doublets::Memory
{
    template<typename TLinkAddress>
    struct ILinksListMethods
    {
         void Detach(TLinkAddress freeLink)
{
return this->object()->Detach(TLinkAddress freeLink);
};

         void AttachAsFirst(TLinkAddress link)
{
return this->object()->AttachAsFirst(TLinkAddress link);
};
    };
}
