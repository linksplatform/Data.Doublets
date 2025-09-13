namespace Platform::Data::Doublets::Decorators
{
    template <typename TFacade, typename TDecorated>
    struct LoggingDecorator : LinksDecoratorBase<TFacade, TDecorated>
    {
    public:
        using base = LinksDecoratorBase<TFacade, TDecorated>;
        using typename base::LinkAddressType;
        using typename base::LinkType;
        using typename base::WriteHandlerType;
        using typename base::ReadHandlerType;
        using base::Constants;

    private:
        std::ostream& _logStream;

    public:
        USE_ALL_BASE_CONSTRUCTORS(LoggingDecorator, base);
        
        LoggingDecorator(const TDecorated& decorated, std::ostream& logStream) 
            : base(decorated), _logStream(logStream)
        {
        }

        LinkAddressType Create(const LinkType& substitution, const WriteHandlerType& handler) override
        {
            WriteHandlerState<TDecorated> handlerState{Constants.Continue, Constants.Break, handler};
            
            auto wrappedHandler = [this, &handlerState](const LinkType& before, const LinkType& after) -> LinkAddressType
            {
                handlerState.Handle(before, after);
                _logStream << "Create. Before: " << Link<LinkAddressType>(before) 
                          << ". After: " << Link<LinkAddressType>(after) << std::endl;
                return Constants.Continue;
            };
            
            return base::Create(substitution, wrappedHandler);
        }

        LinkAddressType Update(const LinkType& restriction, const LinkType& substitution, const WriteHandlerType& handler) override
        {
            WriteHandlerState<TDecorated> handlerState{Constants.Continue, Constants.Break, handler};
            
            auto wrappedHandler = [this, &handlerState](const LinkType& before, const LinkType& after) -> LinkAddressType
            {
                handlerState.Handle(before, after);
                _logStream << "Update. Before: " << Link<LinkAddressType>(before) 
                          << ". After: " << Link<LinkAddressType>(after) << std::endl;
                return Constants.Continue;
            };
            
            return base::Update(restriction, substitution, wrappedHandler);
        }

        LinkAddressType Delete(const LinkType& restriction, const WriteHandlerType& handler) override
        {
            WriteHandlerState<TDecorated> handlerState{Constants.Continue, Constants.Break, handler};
            
            auto wrappedHandler = [this, &handlerState](const LinkType& before, const LinkType& after) -> LinkAddressType
            {
                handlerState.Handle(before, after);
                _logStream << "Delete. Before: " << Link<LinkAddressType>(before) 
                          << ". After: " << Link<LinkAddressType>(after) << std::endl;
                return Constants.Continue;
            };
            
            return base::Delete(restriction, wrappedHandler);
        }
    };
}