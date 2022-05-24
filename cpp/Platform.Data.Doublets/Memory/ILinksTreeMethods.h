namespace Platform::Data::Doublets::Memory
{
    template<typename TLinksOptions>
    struct ILinksTreeMethods
    {
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;

        LinkAddressType CountUsages(LinkAddressType root)
            {
                return this->object().CountUsages(root);
            };

        LinkAddressType Search(LinkAddressType source, LinkAddressType target)
                {
                    return this->object().Search(source, target);
                };

        LinkAddressType EachUsage(LinkAddressType root, const ReadHandlerType& handler)
                    {
                        return this->object().EachUsage(root, handler);
                    };

        void Detach(LinkAddressType& root, LinkAddressType linkIndex)
                        {
                            this->object().Detach(root, linkIndex);
                        };

        void Attach(LinkAddressType& root, LinkAddressType linkIndex)
                            {
                                this->object().Attach(root, linkIndex);
                            };
    };
}
