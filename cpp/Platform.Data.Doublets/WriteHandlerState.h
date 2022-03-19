namespace Platform::Data::Doublets
{
    using namespace Platform::Interfaces;
    template<typename TLinkAddress, typename THandlerParameter>
    class WriteHandlerState
    {
    private:
        TLinkAddress _result;
        TLinkAddress _break;
        std::function<TLinkAddress(THandlerParameter, THandlerParameter)> _handler;
    public:

        WriteHandlerState(TLinkAddress $continue, TLinkAddress $break, auto&& handler) : _result{$continue}, _break{$break}, _handler{handler} {}

        TLinkAddress Apply(TLinkAddress result)
        {
            _result = result;
            return result;
        }

        TLinkAddress operator () (auto&& ...args)
        {
            if(nullptr == _handler)
            {
                return _result;
            }
            else
            {
                return _handler(std::forward<decltype(args)>(args)...);
            }
        }

        TLinkAddress Result() const
        {
            return _result;
        }
    };
}
