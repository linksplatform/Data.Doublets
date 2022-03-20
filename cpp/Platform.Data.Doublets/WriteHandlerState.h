namespace Platform::Data::Doublets
{
    using namespace Platform::Interfaces;
    template<typename TStorage>
    class WriteHandlerState
    {
    private:
        typename TStorage::LinkAddressType _break;
    public:
        typename TStorage::LinkAddressType Result;
        std::function<typename TStorage::LinkAddressType(typename TStorage::HandlerParameterType, typename TStorage::HandlerParameterType)> Handler;

        WriteHandlerState(typename TStorage::LinkAddressType $continue, typename TStorage::LinkAddressType $break, auto&& handler) :
            Result{$continue}, _break{$break}, Handler{handler} {}

        typename TStorage::LinkAddressType Apply(typename TStorage::LinkAddressType result)
        {
            Result = result;
            return result;
        }

        typename TStorage::LinkAddressType Handle(auto ...args)
        {
            if(nullptr == Handler)
            {
                return Result;
            }
            else
            {
                return Handler(std::forward<decltype(args)>(args)...);
            }
        }
    };
}
