namespace Platform::Data::Doublets::Decorators
{
template <typename TFacade, typename TDecorated>
struct NonNullContentsLinkDeletionResolver : public DecoratorBase<TFacade, TDecorated>
{
public:
    using base = DecoratorBase<TFacade, TDecorated>;
    using base::Constants;
    using typename base::LinkAddressType;
    using typename base::LinkType;
    using typename base::ReadHandlerType;
    using typename base::WriteHandlerType;

public:
    USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

public:
    LinkAddressType Delete(const LinkType& restriction, const WriteHandlerType& handler)
    {
        auto $break = Constants.Break;
        auto linkIndex = restriction[Constants.IndexPart];
        WriteHandlerState<TDecorated> handlerState{Constants.Continue, Constants.Break, handler};
        handlerState.Apply(EnforceResetValues(this->decorated(), linkIndex, handlerState.Handler));
        return handlerState.Apply(this->decorated().TDecorated::Delete(LinkType{linkIndex}, handlerState.Handler));
    }
};
} // namespace Platform::Data::Doublets::Decorators
