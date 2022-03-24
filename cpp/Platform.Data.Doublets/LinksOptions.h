namespace Platform::Data::Doublets
{

    template<typename TLinkAddress = std::uint64_t, typename TWriteHandler = std::function<typename TLinkAddress(std::vector<TLinkAddress>, std::vector<TLinkAddress>)>, typename TReadHandler = std::function<TLinkAddress(std::vector<TLinkAddress>)>, LinksConstants<TLinkAddress> VConstants = LinksConstants<TLinkAddress>{true}>
    struct LinksOptions : Platform::Data::LinksOptions<TLink, TWriteHandler, TReadHandler, VConstants>
    {
        using base = Platform::Data::LinksOptions<TLink, TWriteHandler, TReadHandler, VConstants>;

        using base::LinkAddressType;
        using base::WriteHandlerType;
        using base::ReadHandlerType;
        using base::Constants;
    };
}
