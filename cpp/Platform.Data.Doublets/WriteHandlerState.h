namespace Platform::Data::Doublets
{
    using namespace Platform::Interfaces;
    template<typename TLinkAddress, typename THandler>
    class WriteHandlerState
    {
    private:
        TLinkAddress _result;
        TLinkAddress _break;
        THandler& _handler;
    public:

        WriteHandlerState(TLinkAddress $continue, TLinkAddress $break, THandler& handler) : _result{$continue}, _break{$break}, _handler{handler} {}

        TLinkAddress operator () (auto&& ...args)
        {
            if(_break == _result)
            {
                return _break;
            }
            else
            {
                _result = _handler(std::forward<decltype(args)>(args)...);
                return _result;
            }
        }
    };
}
