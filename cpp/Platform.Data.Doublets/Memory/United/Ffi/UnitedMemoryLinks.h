#include <cstdarg>
#include <cstdint>
#include <cstdlib>
#include <ostream>
#include <new>


namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template <typename TLinkAddress>
    thread_local std::function<TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>)> GLOBAL_FUNCTION = nullptr;

//    template <typename TReturn, class... Args>
//    TReturn ffiCall(TReturn(*f)(Args&&... args)) {
//        return f(args);
//    }
//
//    template <typename TFunction, class... Args>
//    auto callLastGlobal(Args&&... args) {
//        auto result { GLOBAL_FUNCTION<TFunction>(args) };
//        GLOBAL_FUNCTION<TFunction> = nullptr;
//        return result;
//    }
//
//    template <typename TLinkAddress, class... Args>
//    auto&& call(std::function<TLinkAddress(Args&&...)> f) {
//        GLOBAL_FUNCTION<TLinkAddress(Args&&...)> = std::move(f);
//        return ffiCall<TLinkAddress, Args>(callLastGlobal<TLinkAddress(Args&&...), Args>);
//    }

    template<typename T>
    using CUDCallback = T(*)(Link<T> before, Link<T> after);

    template<typename T>
    using EachCallback = T(*)(Link<T>);

//struct SharedLogger {
//    void (* formatter)(const Record*);
//};

    extern "C" {

    void* ByteUnitedMemoryLinks_New(const char* path);

    void* UInt16UnitedMemoryLinks_New(const char* path);

    void* UInt32UnitedMemoryLinks_New(const char* path);

    void* UInt64UnitedMemoryLinks_New(const char* path);

    void ByteUnitedMemoryLinks_Drop(void* this_);

    void UInt16UnitedMemoryLinks_Drop(void* this_);

    void UInt32UnitedMemoryLinks_Drop(void* this_);

    void UInt64UnitedMemoryLinks_Drop(void* this_);

    uint8_t ByteUnitedMemoryLinks_Create(void* this_,
                                         const uint8_t* query,
                                         uintptr_t len,
                                         CUDCallback<uint8_t> callback);

    uint16_t UInt16UnitedMemoryLinks_Create(void* this_,
                                            const uint16_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint16_t> callback);

    uint32_t UInt32UnitedMemoryLinks_Create(void* this_,
                                            const uint32_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint32_t> callback);

    uint64_t UInt64UnitedMemoryLinks_Create(void* this_,
                                            const uint64_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint64_t> callback);

    uint8_t ByteUnitedMemoryLinks_Each(void* this_,
                                       const uint8_t* query,
                                       uintptr_t len,
                                       EachCallback<uint8_t> callback);

    uint16_t UInt16UnitedMemoryLinks_Each(void* this_,
                                          const uint16_t* query,
                                          uintptr_t len,
                                          EachCallback<uint16_t> callback);

    uint32_t UInt32UnitedMemoryLinks_Each(void* this_,
                                          const uint32_t* query,
                                          uintptr_t len,
                                          EachCallback<uint32_t> callback);

    uint64_t UInt64UnitedMemoryLinks_Each(void* this_,
                                          const uint64_t* query,
                                          uintptr_t len,
                                          EachCallback<uint64_t> callback);

    uint8_t ByteUnitedMemoryLinks_Count(void* this_, const uint8_t* query, uintptr_t len);

    uint16_t UInt16UnitedMemoryLinks_Count(void* this_, const uint16_t* query, uintptr_t len);

    uint32_t UInt32UnitedMemoryLinks_Count(void* this_, const uint32_t* query, uintptr_t len);

    uint64_t UInt64UnitedMemoryLinks_Count(void* this_, const uint64_t* query, uintptr_t len);

    uint8_t ByteUnitedMemoryLinks_Update(void* this_,
                                         const uint8_t* restrictions,
                                         uintptr_t len_r,
                                         const uint8_t* substitutuion,
                                         uintptr_t len_s,
                                         CUDCallback<uint8_t> callback);

    uint16_t UInt16UnitedMemoryLinks_Update(void* this_,
                                            const uint16_t* restrictions,
                                            uintptr_t len_r,
                                            const uint16_t* substitutuion,
                                            uintptr_t len_s,
                                            CUDCallback<uint16_t> callback);

    uint32_t UInt32UnitedMemoryLinks_Update(void* this_,
                                            const uint32_t* restrictions,
                                            uintptr_t len_r,
                                            const uint32_t* substitutuion,
                                            uintptr_t len_s,
                                            CUDCallback<uint32_t> callback);

    uint64_t UInt64UnitedMemoryLinks_Update(void* this_,
                                            const uint64_t* restrictions,
                                            uintptr_t len_r,
                                            const uint64_t* substitutuion,
                                            uintptr_t len_s,
                                            CUDCallback<uint64_t> callback);

    uint8_t ByteUnitedMemoryLinks_Delete(void* this_,
                                         const uint8_t* query,
                                         uintptr_t len,
                                         CUDCallback<uint8_t> callback);

    uint16_t UInt16UnitedMemoryLinks_Delete(void* this_,
                                            const uint16_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint16_t> callback);

    uint32_t UInt32UnitedMemoryLinks_Delete(void* this_,
                                            const uint32_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint32_t> callback);

    uint64_t UInt64UnitedMemoryLinks_Delete(void* this_,
                                            const uint64_t* query,
                                            uintptr_t len,
                                            CUDCallback<uint64_t> callback);

//void setup_shared_logger(SharedLogger logger);

    void init_fmt_logger();

    } // extern "C"

    template <typename TSelf, typename TLinkAddress, typename ...TBase>
    class UnitedMemoryLinks : public Interfaces::Polymorph<TSelf, TBase...>, public ILinks<TSelf, TLinkAddress>
    {
    private:
        void* _ptr;
    public:
        LinksConstants<TLinkAddress> Constants;

        UnitedMemoryLinks(std::string path) : _ptr { ByteUnitedMemoryLinks_New(path.c_str()) }, Constants { true }
        {
        }

        ~UnitedMemoryLinks()
        {
            ByteUnitedMemoryLinks_Drop(_ptr);
        }

        TLinkAddress Create(auto&& restriction, std::function<TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>)> handler)
        {
            GLOBAL_FUNCTION<TLinkAddress> = handler;
            if(typeid(TLinkAddress) == typeid(uint8_t))
            {
                return ByteUnitedMemoryLinks_Create<uint8_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint16_t))
            {
                return ByteUnitedMemoryLinks_Create<uint16_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint32_t))
            {
                return ByteUnitedMemoryLinks_Create<uint32_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint64_t))
            {
                return ByteUnitedMemoryLinks_Create<uint64_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else
            {
                throw std::runtime_error("The type of TLinkAddress is not supported. Use any type of uint8_t, uint16_t, uint32_t, uint64_t.");
            }
        }

        TLinkAddress Update(auto&& restriction, auto&& substitution, std::function<TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>)> handler)
        {
            GLOBAL_FUNCTION<TLinkAddress> = handler;
            if(typeid(TLinkAddress) == typeid(uint8_t))
            {
                return ByteUnitedMemoryLinks_Update<uint8_t>(_ptr, &restriction, std::ranges::size(restriction), &substitution, std::ranges::size(substitution), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint16_t))
            {
                return ByteUnitedMemoryLinks_Update<uint16_t>(_ptr, &restriction, std::ranges::size(restriction), &substitution, std::ranges::size(substitution), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint32_t))
            {
                return ByteUnitedMemoryLinks_Update<uint32_t>(_ptr, &restriction, std::ranges::size(restriction), &substitution, std::ranges::size(substitution), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint64_t))
            {
                return ByteUnitedMemoryLinks_Update<uint64_t>(_ptr, &restriction, std::ranges::size(restriction), &substitution, std::ranges::size(substitution), handler);
            }
            else
            {
                throw std::runtime_error("The type of TLinkAddress is not supported. Use any type of uint8_t, uint16_t, uint32_t, uint64_t.");
            }
        }

        TLinkAddress Delete(auto&& restriction, std::function<TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>)> handler)
        {
            GLOBAL_FUNCTION<TLinkAddress> = handler;
            if(typeid(TLinkAddress) == typeid(uint8_t))
            {
                return ByteUnitedMemoryLinks_Delete<uint8_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint16_t))
            {
                return ByteUnitedMemoryLinks_Delete<uint16_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint32_t))
            {
                return ByteUnitedMemoryLinks_Delete<uint32_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint64_t))
            {
                return ByteUnitedMemoryLinks_Delete<uint64_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else
            {
                throw std::runtime_error("The type of TLinkAddress is not supported. Use any type of uint8_t, uint16_t, uint32_t, uint64_t.");
            }
        }

        TLinkAddress Each(auto&& restriction, std::function<TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>)> handler)
        {
            GLOBAL_FUNCTION<TLinkAddress> = handler;
            if(typeid(TLinkAddress) == typeid(uint8_t))
            {
                return ByteUnitedMemoryLinks_Each<uint8_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint16_t))
            {
                return ByteUnitedMemoryLinks_Each<uint16_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint32_t))
            {
                return ByteUnitedMemoryLinks_Each<uint32_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else if(typeid(TLinkAddress) == typeid(uint64_t))
            {
                return ByteUnitedMemoryLinks_Each<uint64_t>(_ptr, &restriction, std::ranges::size(restriction), handler);
            }
            else
            {
                throw std::runtime_error("The type of TLinkAddress is not supported. Use any type of uint8_t, uint16_t, uint32_t, uint64_t.");
            }
        }

        TLinkAddress Count(auto&& restriction)
        {
            if(typeid(TLinkAddress) == typeid(uint8_t))
            {
                return ByteUnitedMemoryLinks_Count<uint8_t>(_ptr, &restriction, std::ranges::size(restriction));
            }
            else if(typeid(TLinkAddress) == typeid(uint16_t))
            {
                return ByteUnitedMemoryLinks_Count<uint16_t>(_ptr, &restriction, std::ranges::size(restriction));
            }
            else if(typeid(TLinkAddress) == typeid(uint32_t))
            {
                return ByteUnitedMemoryLinks_Count<uint32_t>(_ptr, &restriction, std::ranges::size(restriction));
            }
            else if(typeid(TLinkAddress) == typeid(uint64_t))
            {
                return ByteUnitedMemoryLinks_Count<uint64_t>(_ptr, &restriction, std::ranges::size(restriction));
            }
            else
            {
                throw std::runtime_error("The type of TLinkAddress is not supported. Use any type of uint8_t, uint16_t, uint32_t, uint64_t.");
            }
        }
    };
}


