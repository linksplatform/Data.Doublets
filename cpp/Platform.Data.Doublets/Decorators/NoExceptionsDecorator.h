namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct NoExceptionsDecorator : LinksDecoratorBase<TFacade, TDecorated>
    {
    public:
        using base = LinksDecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
        using base::Constants;

    public:
        USE_ALL_BASE_CONSTRUCTORS(NoExceptionsDecorator, base);

        LinkAddressType Count(const LinkType& restriction) override
        {
            try
            {
                return base::Count(restriction);
            }
            catch (...)
            {
                return Constants.Error;
            }
        }

        LinkAddressType Each(const LinkType& restriction, const ReadHandlerType& handler) override
        {
            try
            {
                return base::Each(restriction, handler);
            }
            catch (...)
            {
                return Constants.Error;
            }
        }

        LinkAddressType Create(const LinkType& substitution, const WriteHandlerType& handler) override
        {
            try
            {
                return base::Create(substitution, handler);
            }
            catch (...)
            {
                return Constants.Error;
            }
        }

        LinkAddressType Update(const LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler) override
        {
            try
            {
                return base::Update(restriction, substitution, handler);
            }
            catch (...)
            {
                return Constants.Error;
            }
        }

        LinkAddressType Delete(const LinkType& restriction, const WriteHandlerType& handler) override
        {
            try
            {
                return base::Delete(restriction, handler);
            }
            catch (...)
            {
                return Constants.Error;
            }
        }
    };
}