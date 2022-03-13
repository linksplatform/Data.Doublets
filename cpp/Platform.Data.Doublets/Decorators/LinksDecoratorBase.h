template<typename TFacade, typename TDecorated>
struct DecoratorBase : public TDecorated
{
    USE_ALL_BASE_CONSTRUCTORS(DecoratorBase, TDecorated)

    THIS_REFERENCE_WRAPPER_METHODS(decorated, TDecorated)
    THIS_REFERENCE_WRAPPER_METHODS(facade, TDecorated)
};
