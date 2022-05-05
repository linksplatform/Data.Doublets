namespace Platform::Data::Doublets
{

    template<typename TLinkAddress = std::uint64_t, LinksConstants<TLinkAddress> VConstants = LinksConstants<TLinkAddress>{true}, typename TLink = std::vector<TLinkAddress>, typename TReadHandler = std::function<TLinkAddress(std::vector<TLinkAddress>)>, typename TWriteHandler = std::function<TLinkAddress(std::vector<TLinkAddress>, std::vector<TLinkAddress>)>>
    struct LinksOptions : Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TReadHandler, TWriteHandler>
    {
        using base = Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TReadHandler, TWriteHandler>;
        using base::LinkAddressType;
        using base::Constants;
        using base::LinkType;
        using base::WriteHandlerType;
        using base::ReadHandlerType;
    };
}
