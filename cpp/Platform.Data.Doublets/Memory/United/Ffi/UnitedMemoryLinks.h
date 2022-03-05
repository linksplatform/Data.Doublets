#include <cstdarg>
#include <cstdint>
#include <cstdlib>
#include <ostream>
#include <new>

namespace Platform::Data::Doublets::Memory::United::Ffi
{
    template<typename Signature>
    thread_local std::function<Signature> GLOBAL_FUNCTION = nullptr;

    template<typename TReturn, typename Signature, typename ...TArgs>
    TReturn call_last_global(TArgs ...args) {
        decltype(auto) result = GLOBAL_FUNCTION<Signature>(args...);
        GLOBAL_FUNCTION<Signature> = nullptr;
        return result;
    }

    template<typename Signature>
    void set_global(std::function<Signature> function)
    {
        GLOBAL_FUNCTION<Signature> = std::move(function);
    }

    template<typename Signature>
    auto get_global()
    {
        return GLOBAL_FUNCTION<Signature>;
    }

    template<typename TLinkAddress>
    using CUDCallback = TLinkAddress(*)(Link<TLinkAddress> before, Link<TLinkAddress> after);

    template<typename TLinkAddress>
    using EachCallback = TLinkAddress(*)(Link<TLinkAddress>);

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
            const uint8_t* substitution,
            uintptr_t len_s,
            CUDCallback<uint8_t> callback);

    uint16_t UInt16UnitedMemoryLinks_Update(void* this_,
            const uint16_t* restrictions,
            uintptr_t len_r,
            const uint16_t* substitution,
            uintptr_t len_s,
            CUDCallback<uint16_t> callback);

    uint32_t UInt32UnitedMemoryLinks_Update(void* this_,
            const uint32_t* restrictions,
            uintptr_t len_r,
            const uint32_t* substitution,
            uintptr_t len_s,
            CUDCallback<uint32_t> callback);

    uint64_t UInt64UnitedMemoryLinks_Update(void* this_,
            const uint64_t* restrictions,
            uintptr_t len_r,
            const uint64_t* substitution,
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

    void init_fmt_logger();
}

    template<typename Stop>
    struct stopper {
        static constexpr bool value = false;
    };

    template<typename TLinkAddress>
    class UnitedMemoryLinks /*: public Interfaces::Polymorph<UnitedMemoryLinks<TLinkAddress, TBase...>, TBase...>*/
    {
    private:
        void* _ptr;
    public:

        LinksConstants<TLinkAddress> Constants;

        UnitedMemoryLinks(std::string_view path) : Constants{LinksConstants<TLinkAddress>(true)}
      {
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                _ptr = ByteUnitedMemoryLinks_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                _ptr = UInt16UnitedMemoryLinks_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                _ptr = UInt32UnitedMemoryLinks_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                _ptr = UInt64UnitedMemoryLinks_New(path.data());
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        ~UnitedMemoryLinks()
        {
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                if (_ptr != nullptr) {
                    ByteUnitedMemoryLinks_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                if (_ptr != nullptr) {
                    UInt16UnitedMemoryLinks_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                if (_ptr != nullptr) {
                    UInt32UnitedMemoryLinks_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                if (_ptr != nullptr) {
                    UInt64UnitedMemoryLinks_Drop(_ptr);
                }
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Create(Interfaces::CArray auto&& substitution, auto&& handler)
        {
            auto substitutionLength = std::ranges::size(substitution);
            auto substitutionPtr = std::ranges::data(substitution);
            auto callback = [&] (Link<TLinkAddress> before, Link<TLinkAddress> after) {
                std::array beforeArray {before.Index, before.Source, before.Target};
                std::array afterArray {after.Index, after.Source, after.Target};
                return handler(beforeArray, afterArray);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteUnitedMemoryLinks_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16UnitedMemoryLinks_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32UnitedMemoryLinks_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64UnitedMemoryLinks_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        };

        TLinkAddress Update(Interfaces::CArray auto&& restriction, Interfaces::CArray auto&& substitution, auto&& handler)
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr { std::ranges::data(restriction) };
            auto substitutionLength = std::ranges::size(substitution);
            auto substitutionPtr { std::ranges::data(substitution) };
            auto callback = [&] (Link<TLinkAddress> before, Link<TLinkAddress> after) {
                std::array beforeArray {before.Index, before.Source, before.Target};
                std::array afterArray {after.Index, after.Source, after.Target};
                return handler(beforeArray, afterArray);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteUnitedMemoryLinks_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16UnitedMemoryLinks_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32UnitedMemoryLinks_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64UnitedMemoryLinks_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Delete(Interfaces::CArray auto&& restriction, auto&& handler)
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            auto callback = [&] (Link<TLinkAddress> before, Link<TLinkAddress> after) {
                std::array beforeArray {before.Index, before.Source, before.Target};
                std::array afterArray {after.Index, after.Source, after.Target};
                return handler(beforeArray, afterArray);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteUnitedMemoryLinks_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16UnitedMemoryLinks_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32UnitedMemoryLinks_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64UnitedMemoryLinks_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Each(Interfaces::CArray auto&& restriction, auto&& handler) const
        {
            using Signature = TLinkAddress(Link<TLinkAddress>);
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            auto callback = [&] (Link<TLinkAddress> link) {
                return handler(std::array{link.Index, link.Source, link.Target});
            };
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteUnitedMemoryLinks_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16UnitedMemoryLinks_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32UnitedMemoryLinks_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64UnitedMemoryLinks_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Count(Interfaces::CArray auto&& restriction) const
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteUnitedMemoryLinks_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16UnitedMemoryLinks_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32UnitedMemoryLinks_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64UnitedMemoryLinks_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        // Extensions
    };
}


