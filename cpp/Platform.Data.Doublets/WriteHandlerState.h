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

        void Apply(TLinkAddress result)
        {
            _result = result;
        }

        TLinkAddress operator () (auto&& ...args)
        {
            if(_break == _result)
            {
                return _break;
            }
            else
            {
                return _handler(std::forward<decltype(args)>(args)...);
            }
        }
    };
}
