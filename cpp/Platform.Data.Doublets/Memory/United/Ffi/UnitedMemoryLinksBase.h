namespace Platform::Data::Doublets::Memory::United::Ffi
{
    using namespace Platform::Interfaces;
    using namespace Platform::Ranges;

    template<typename Signature>
    thread_local std::function<Signature> GLOBAL_FUNCTION = nullptr;

    template<typename TReturn, typename Signature, typename... TArgs>
    TReturn call_last_global(TArgs... args)
    {
        decltype(auto) result = GLOBAL_FUNCTION<Signature>(args...);
        return result;
    }

    template<typename Signature>
    void set_global(std::function<Signature> function)
    {
        GLOBAL_FUNCTION<Signature> = std::move(function);
    }

    template<typename TLinkAddress>
    using CUDCallback = TLinkAddress (*)(Link<TLinkAddress> before, Link<TLinkAddress> after);

    template<typename TLinkAddress>
    using EachCallback = TLinkAddress (*)(Link<TLinkAddress>);

    template<typename TLinkAddress>
    struct FfiConstants
    {
        TLinkAddress index_part;
        TLinkAddress source_part;
        TLinkAddress target_part;
        TLinkAddress null;
        TLinkAddress $continue;
        TLinkAddress $break;
        TLinkAddress skip;
        TLinkAddress any;
        TLinkAddress itself;
        TLinkAddress error;
        Ranges::Range<TLinkAddress> internal_range;
        Ranges::Range<TLinkAddress> external_range;
        bool _opt_marker;
    };

    extern "C"
    {
        void* ByteLinks_New(const char* path);

        void* UInt16Links_New(const char* path);

        void* UInt32Links_New(const char* path);

        void* UInt64Links_New(const char* path);

        void ByteLinks_Drop(void* this_);

        void UInt16Links_Drop(void* this_);

        void UInt32Links_Drop(void* this_);

        void UInt64Links_Drop(void* this_);

        FfiConstants<uint8_t> ByteLinks_GetConstants(void* this_);

        FfiConstants<uint16_t> UInt16Links_GetConstants(void* this_);

        FfiConstants<uint32_t> UInt32Links_GetConstants(void* this_);

        FfiConstants<uint64_t> UInt64Links_GetConstants(void* this_);

        uint8_t ByteLinks_Create(void* this_,
                                 const uint8_t* query,
                                 uintptr_t len,
                                 CUDCallback<uint8_t> callback);

        uint16_t UInt16Links_Create(void* this_,
                                    const uint16_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint16_t> callback);

        uint32_t UInt32Links_Create(void* this_,
                                    const uint32_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint32_t> callback);

        uint64_t UInt64Links_Create(void* this_,
                                    const uint64_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint64_t> callback);

        uint8_t ByteLinks_Each(void* this_,
                               const uint8_t* query,
                               uintptr_t len,
                               EachCallback<uint8_t> callback);

        uint16_t UInt16Links_Each(void* this_,
                                  const uint16_t* query,
                                  uintptr_t len,
                                  EachCallback<uint16_t> callback);

        uint32_t UInt32Links_Each(void* this_,
                                  const uint32_t* query,
                                  uintptr_t len,
                                  EachCallback<uint32_t> callback);

        uint64_t UInt64Links_Each(void* this_,
                                  const uint64_t* query,
                                  uintptr_t len,
                                  EachCallback<uint64_t> callback);

        uint8_t ByteLinks_Count(void* this_, const uint8_t* query, uintptr_t len);

        uint16_t UInt16Links_Count(void* this_, const uint16_t* query, uintptr_t len);

        uint32_t UInt32Links_Count(void* this_, const uint32_t* query, uintptr_t len);

        uint64_t UInt64Links_Count(void* this_, const uint64_t* query, uintptr_t len);

        uint8_t ByteLinks_Update(void* this_,
                                 const uint8_t* restriction,
                                 uintptr_t len_r,
                                 const uint8_t* substitution,
                                 uintptr_t len_s,
                                 CUDCallback<uint8_t> callback);

        uint16_t UInt16Links_Update(void* this_,
                                    const uint16_t* restriction,
                                    uintptr_t len_r,
                                    const uint16_t* substitution,
                                    uintptr_t len_s,
                                    CUDCallback<uint16_t> callback);

        uint32_t UInt32Links_Update(void* this_,
                                    const uint32_t* restriction,
                                    uintptr_t len_r,
                                    const uint32_t* substitution,
                                    uintptr_t len_s,
                                    CUDCallback<uint32_t> callback);

        uint64_t UInt64Links_Update(void* this_,
                                    const uint64_t* restriction,
                                    uintptr_t len_r,
                                    const uint64_t* substitution,
                                    uintptr_t len_s,
                                    CUDCallback<uint64_t> callback);

        uint8_t ByteLinks_Delete(void* this_,
                                 const uint8_t* query,
                                 uintptr_t len,
                                 CUDCallback<uint8_t> callback);

        uint16_t UInt16Links_Delete(void* this_,
                                    const uint16_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint16_t> callback);

        uint32_t UInt32Links_Delete(void* this_,
                                    const uint32_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint32_t> callback);

        uint64_t UInt64Links_Delete(void* this_,
                                    const uint64_t* query,
                                    uintptr_t len,
                                    CUDCallback<uint64_t> callback);

        void init_fmt_logger();
    }

    template<typename Stop>
    struct stopper
    {
        static constexpr bool value = false;
    };

    template<typename TSelf, typename TLinkAddress, LinksConstants<TLinkAddress> VConstants, CArray<TLinkAddress> THandlerParameter, typename... TBase>
    class UnitedMemoryLinksBase : public Interfaces::Polymorph<TSelf, TBase...>
    {
    private:
        void* _ptr;

    public:
        static constexpr auto Constants = VConstants;
        using LinkAddressType = TLinkAddress;
        using HandlerParameterType = THandlerParameter;

            UnitedMemoryLinksBase(std::string_view path)
        {
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                _ptr = ByteLinks_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                _ptr = UInt16Links_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                _ptr = UInt32Links_New(path.data());
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                _ptr = UInt64Links_New(path.data());
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        ~UnitedMemoryLinksBase()
        {
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                if (_ptr != nullptr)
                {
                    ByteLinks_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                if (_ptr != nullptr)
                {
                    UInt16Links_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                if (_ptr != nullptr)
                {
                    UInt32Links_Drop(_ptr);
                }
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                if (_ptr != nullptr)
                {
                    UInt64Links_Drop(_ptr);
                }
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Create(const LinkType& substitution, auto&& handler)
        {
            auto substitutionLength = std::ranges::size(substitution);
            auto substitutionPtr = std::ranges::data(substitution);
            auto callback = [&](Link<TLinkAddress> before, Link<TLinkAddress> after) -> TLinkAddress {
                Link beforeLink{before.Index, before.Source, before.Target};
                Link afterLink{after.Index, after.Source, after.Target};
                return handler(beforeLink, afterLink);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteLinks_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16Links_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32Links_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64Links_Create(_ptr, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        };

        TLinkAddress Update(const  LinkType& restriction, const LinkType& substitution, auto&& handler)
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr{std::ranges::data(restriction)};
            auto substitutionLength = std::ranges::size(substitution);
            auto substitutionPtr{std::ranges::data(substitution)};
            auto callback = [&](Link<TLinkAddress> before, Link<TLinkAddress> after) {
                Link beforeLink{before.Index, before.Source, before.Target};
                Link afterLink{after.Index, after.Source, after.Target};
                return handler(beforeLink, afterLink);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteLinks_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16Links_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32Links_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64Links_Update(_ptr, restrictionPtr, restrictionLength, substitutionPtr, substitutionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Delete(const  LinkType& restriction, auto&& handler)
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            auto callback = [&](Link<TLinkAddress> before, Link<TLinkAddress> after) {
                Link beforeLink{before.Index, before.Source, before.Target};
                Link afterLink{after.Index, after.Source, after.Target};
                return handler(beforeLink, afterLink);
            };
            using Signature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteLinks_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16Links_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32Links_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64Links_Delete(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Each(const  LinkType& restriction, auto&& handler) const
        {
            using Signature = TLinkAddress(Link<TLinkAddress>);
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            auto callback = [&](Link<TLinkAddress> link) {
                return handler(Link{link.Index, link.Source, link.Target});
            };
            set_global<Signature>(callback);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteLinks_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16Links_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32Links_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64Links_Each(_ptr, restrictionPtr, restrictionLength, call_last_global<TLinkAddress, Signature, Link<TLinkAddress>>);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        TLinkAddress Count(const  LinkType& restriction) const
        {
            auto restrictionLength = std::ranges::size(restriction);
            auto restrictionPtr = std::ranges::data(restriction);
            if constexpr (std::same_as<TLinkAddress, std::uint8_t>)
            {
                return ByteLinks_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint16_t>)
            {
                return UInt16Links_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint32_t>)
            {
                return UInt32Links_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else if constexpr (std::same_as<TLinkAddress, std::uint64_t>)
            {
                return UInt64Links_Count(_ptr, restrictionPtr, restrictionLength);
            }
            else
            {
                static_assert(stopper<TLinkAddress>::value, "TLinkAddress must be one of std::uint8_t, std::uint16_t, std::uint32_t, std::uint64_t");
            }
        }

        // Extensions
    };
}// namespace Platform::Data::Doublets::Memory::United::Ffi
