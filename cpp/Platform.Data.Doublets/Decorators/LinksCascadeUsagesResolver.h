namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class LinksCascadeUsagesResolver;

    template <typename TFacade, typename TDecorated>
    class LinksCascadeUsagesResolver : LinksDecoratorBase<TFacade, TDecorated>
    {
        using base = LinksDecoratorBase<TFacade, TDecorated>;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: void Delete(CList auto&& restrictions)
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            this->facade().DeleteAllUsages(linkIndex);
            this->decorated().Delete(linkIndex);
        }
    };
}
