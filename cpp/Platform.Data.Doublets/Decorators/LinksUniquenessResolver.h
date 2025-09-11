namespace Platform::Data::Doublets::Decorators
{
template <typename TFacade, typename TDecorated>
struct LinksUniquenessResolver : DecoratorBase<TFacade, TDecorated>
{
public:
    using base = DecoratorBase<TFacade, TDecorated>;
    using base::Constants;
    using typename base::LinkAddressType;
    using typename base::LinkType;
    using typename base::ReadHandlerType;
    using typename base::WriteHandlerType;

public:
    USE_ALL_BASE_CONSTRUCTORS(LinksUniquenessResolver, base);

public:
    LinkAddressType Update(const LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler)
    {
        auto newLinkAddress =
            SearchOrDefault(this->decorated(), substitution[Constants.SourcePart], substitution[Constants.TargetPart]);
        if (newLinkAddress == LinkAddressType{})
        {
            return this->decorated().TDecorated::Update(restriction, substitution, handler);
        }
        return this->ResolveAddressChangeConflict(restriction[Constants.IndexPart], newLinkAddress, handler);
    }

public:
    LinkAddressType ResolveAddressChangeConflict(LinkAddressType oldLinkAddress, LinkAddressType newLinkAddress,
                                                 const WriteHandlerType& handler)
    {
        if (oldLinkAddress != newLinkAddress && Exists(this->decorated(), oldLinkAddress))
        {
            this->facade().Delete(LinkType{oldLinkAddress}, handler);
        }
        return Constants.Continue;
    }
};
} // namespace Platform::Data::Doublets::Decorators
