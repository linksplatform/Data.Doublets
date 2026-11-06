namespace Platform::Data::Doublets::Memory
{
    template <typename TLinksOptions>
    struct ILinksTreeMethods
    {
        using LinksOptionsType = TLinksOptions;
        using LinkAddressType = typename LinksOptionsType::LinkAddressType;
        using LinkType = typename LinksOptionsType::LinkType;
        using ReadHandlerType = typename LinksOptionsType::ReadHandlerType;

        virtual ~ILinksTreeMethods() = default;

        virtual LinkAddressType CountUsages(LinkAddressType root) = 0;

        virtual LinkAddressType Search(LinkAddressType source, LinkAddressType target) = 0;

        virtual LinkAddressType EachUsage(LinkAddressType root, const ReadHandlerType& handler) = 0;

        virtual void Detach(LinkAddressType& root, LinkAddressType linkIndex) = 0;

        virtual void Attach(LinkAddressType& root, LinkAddressType linkIndex) = 0;
    };
} // namespace Platform::Data::Doublets::Memory
