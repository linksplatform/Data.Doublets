namespace Platform::Data::Doublets
{
    template<typename TLinkAddress = std::uint64_t, LinksConstants<TLinkAddress> VConstants = LinksConstants<TLinkAddress>{true}, typename TLink = std::vector<TLinkAddress>, typename TReadHandler = std::function<TLinkAddress(TLink)>, typename TWriteHandler = std::function<TLinkAddress(TLink, TLink)>>
    struct LinksOptions : Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TReadHandler, TWriteHandler>
    {
        using base = Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TReadHandler, TWriteHandler>;
        using base::LinkAddressType;
        using base::LinkType;
        using base::ReadHandlerType;
        using base::WriteHandlerType;
        using base::Constants;
    };
}
