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
            if (Break == result)
            {
                Result = Break;
                Handler = nullptr;
            }
            return Result;
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
                auto result {Handler(std::forward<TArgs>(args)...)};
                return Apply(result);
            }
        }
    };
}
