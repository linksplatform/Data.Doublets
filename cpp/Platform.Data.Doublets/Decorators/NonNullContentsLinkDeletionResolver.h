namespace Platform::Data::Doublets::Decorators
{
    template <typename ...> class NonNullContentsLinkDeletionResolver;
    template <typename TFacade, typename TDecorated>
    class NonNullContentsLinkDeletionResolver : public LinksDecoratorBase<TFacade, TDecorated>
    {
        using base = LinksDecoratorBase<TFacade, TDecorated>;
    public:
        USE_ALL_BASE_CONSTRUCTORS(NonNullContentsLinkDeletionResolver, base);

        public: void Delete(CList auto&&restrictions) override
        {
            auto linkIndex = restrictions[_constants.IndexPart];
            auto storage = this->decorated();
            storage.EnforceResetValues(linkIndex);
            storage.Delete(linkIndex);
        }
    };
}
