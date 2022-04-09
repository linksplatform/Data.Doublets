namespace Platform::Data::Doublets::Memory
{
    template<typename TLinksOptions>
    struct ILinksTreeMethods
    {
        using OptionsType = TLinksOptions;
        using LinkAddressType = OptionsType::LinkAddressType;
        using LinkType = OptionsType::LinkType;
        using ReadHandlerType = OptionsType::ReadHandlerType;
        virtual LinkAddressType CountUsages(LinkAddressType root) = 0;

        virtual LinkAddressType Search(LinkAddressType source, LinkAddressType target) = 0;

        virtual LinkAddressType EachUsage(LinkAddressType root, const ReadHandlerType& handler) = 0;

        virtual void Detach(LinkAddressType& root, LinkAddressType linkIndex) = 0;

        virtual void Attach(LinkAddressType& root, LinkAddressType linkIndex) = 0;
    };
}
