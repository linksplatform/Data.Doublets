namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LinksDecoratorBase : DecoratorBase<TFacade, TDecorated>
    {
    public:
        using base = DecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
        using base::Constants;

    public:
        USE_ALL_BASE_CONSTRUCTORS(LinksDecoratorBase, base);

        virtual LinkAddressType Count(const LinkType& restriction)
        {
            return this->decorated().Count(restriction);
        }

        virtual LinkAddressType Each(const LinkType& restriction, const ReadHandlerType& handler)
        {
            return this->decorated().Each(restriction, handler);
        }

        virtual LinkAddressType Create(const LinkType& substitution, const WriteHandlerType& handler)
        {
            return this->decorated().Create(substitution, handler);
        }

        virtual LinkAddressType Update(const LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler)
        {
            return this->decorated().Update(restriction, substitution, handler);
        }

        virtual LinkAddressType Delete(const LinkType& restriction, const WriteHandlerType& handler)
        {
            return this->decorated().Delete(restriction, handler);
        }
    };
}