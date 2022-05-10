namespace Platform::Data::Doublets::Memory
{
    template<typename TLinksOptions>
    struct ILinksTreeMethods
    {
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = LinksOptionsType::LinkAddressType;
        using LinkType = LinksOptionsType::LinkType;
        using ReadHandlerType = LinksOptionsType::ReadHandlerType;
         LinkAddressType CountUsages(LinkAddressType root)
{
return this->object()->CountUsages(LinkAddressType root);
};

         LinkAddressType Search(LinkAddressType source, LinkAddressType target)
{
return this->object()->Search(LinkAddressType source, LinkAddressType target);
};

         LinkAddressType EachUsage(LinkAddressType root, const ReadHandlerType& handler)
{
return this->object()->EachUsage(LinkAddressType root, const ReadHandlerType& handler);
};

         void Detach(LinkAddressType& root, LinkAddressType linkIndex)
{
return this->object()->Detach(LinkAddressType& root, LinkAddressType linkIndex);
};

         void Attach(LinkAddressType& root, LinkAddressType linkIndex)
{
return this->object()->Attach(LinkAddressType& root, LinkAddressType linkIndex);
};
    };
}
