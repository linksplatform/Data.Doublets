namespace Platform::Data::Doublets
{

    template<typename TLink = Link<std::uint64_t>, typename TWriteHandler = std::function<typename TLink::value_type(TLink, TLink)>, typename TReadHandler = std::function<typename TLink::value_type(TLink)>, LinksConstants<typename TLink::value_type> VConstants = LinksConstants<typename TLink::value_type>{true}>
    struct LinksOptions : Platform::Data::LinksOptions<TLink, TWriteHandler, TReadHandler, VConstants>
    {
        using base = Platform::Data::LinksOptions<TLink, TWriteHandler, TReadHandler, VConstants>;
        using base::LinkType;
        using base::LinkAddressType;
        using base::WriteHandlerType;
        using base::ReadHandlerType;
        using base::Constants;
    };
}
