namespace Platform::Data::Doublets
{
    using namespace Platform::Interfaces;
    template<typename TStorage>
    struct WriteHandlerState
    {
    public:
        typename TStorage::LinkAddressType Break;
        typename TStorage::LinkAddressType Result;
        std::function<typename TStorage::LinkAddressType(typename TStorage::HandlerParameterType, typename TStorage::HandlerParameterType)> Handler;

        WriteHandlerState(typename TStorage::LinkAddressType $continue, typename TStorage::LinkAddressType $break, auto&& handler) :
            Result{$continue}, Break{$break}, Handler{handler} {}

        typename TStorage::LinkAddressType Apply(typename TStorage::LinkAddressType result)
        {
            if(Break == Result || Break != result)
            {
                return Result;
            }
            Result = result;
            return result;
        }

        template<typename ...TArgs>
        typename TStorage::LinkAddressType Handle(TArgs ...args)
        {
            if(nullptr == Handler)
            {
                return Result;
            }
            else
            {
                return Apply({Handler(std::forward<TArgs>(args)...)});
            }
        }
    };
}
