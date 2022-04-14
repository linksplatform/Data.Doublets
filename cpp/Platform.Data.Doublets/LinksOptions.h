namespace Platform::Data::Doublets
{
    template<typename TLinkAddress = std::uint64_t, LinksConstants<TLinkAddress> VConstants = LinksConstants<TLinkAddress>{true}, typename TLink = std::vector<TLinkAddress>, typename TWriteHandler = std::function<TLinkAddress(TLink, TLink)>, typename TReadHandler = std::function<TLinkAddress(TLink)>>
    struct LinksOptions : Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TWriteHandler, TReadHandler>
    {
        using base = Platform::Data::LinksOptions<TLinkAddress, VConstants, TLink, TWriteHandler, TReadHandler>;
        using base::LinkAddressType;
        using base::LinkType;
        using base::WriteHandlerType;
        using base::ReadHandlerType;
        using base::Constants;
    };
}
