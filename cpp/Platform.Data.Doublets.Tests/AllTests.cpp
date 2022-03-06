#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>
#include "ILinksTestExtensions.h"

//#include "GenericLinksTests.cpp"
//#include "UnitedMemoryLinksFfiTests.cpp"

namespace Platform::Data::Doublets::Tests
{
    template<typename Signature>
    thread_local std::function<Signature> GLOBAL_FUNCTION = nullptr;

    template<typename TReturn, typename Signature, typename ...TArgs>
    TReturn call_last_global(TArgs ...args) {
        decltype(auto) result = GLOBAL_FUNCTION<Signature>(args...);
        return result;
    }

    template<typename Signature>
    void set_global(std::function<Signature> function)
    {
        GLOBAL_FUNCTION<Signature> = std::move(function);
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

    TEST(TEST, TEST1)
    {
        using TLinkAddress = uint8_t;
        using namespace Platform::Data;
        using namespace Platform::Data::Doublets;
        LinksConstants<TLinkAddress> constants {true};
		TLinkAddress _continue = constants.Continue;
        auto tempName {tmpnam(nullptr)};
        try
        {
            auto ptr {ByteUnitedMemoryLinks_New(tempName)};
			std::array<TLinkAddress, 0> countRestriction {};
			auto linksCount {ByteUnitedMemoryLinks_Count(ptr, countRestriction.data(), countRestriction.size())};
			Expects(0 == linksCount);
            std::array<TLinkAddress, 0> createRestriction {};
            TLinkAddress createdLinkAddress;
			auto handler{[&createdLinkAddress, _continue](Interfaces::CArray<TLinkAddress> auto&& link){
			  createdLinkAddress = link[0];
			  return _continue;
			}};
            auto createCallback {[_continue, &handler](Link<TLinkAddress> before, Link<TLinkAddress> after)
                                 {
                                     return handler(std::array{after.Index, after.Source, after.Target});
                                 }};
            using CreateSignature = TLinkAddress(Link<TLinkAddress>, Link<TLinkAddress>);
            set_global<CreateSignature>(createCallback);
            auto createRestrictionPtr {std::ranges::data(createRestriction)};
            auto createRestrictionLength = std::ranges::size(createRestriction);
            ByteUnitedMemoryLinks_Create(ptr, createRestrictionPtr, createRestrictionLength, call_last_global<TLinkAddress, CreateSignature, Link<TLinkAddress>, Link<TLinkAddress>>);
            std::cout << "Created link address: " << static_cast<uint64_t>(createdLinkAddress) << std::endl;
            TLinkAddress foundLink;
			auto eachHandler{[&foundLink, _continue](Interfaces::CArray<TLinkAddress> auto&& link) {
				foundLink = link[0];
				return _continue;
			}};
            auto eachCallback {[_continue, &eachHandler](Link<TLinkAddress> link)
							   {
								 return eachHandler(std::array{link.Index, link.Source, link.Target});
							   }};
            using EachSignature = TLinkAddress(Link<TLinkAddress>);
            set_global<EachSignature>(eachCallback);
            std::array eachRestriction {createdLinkAddress, constants.Any, constants.Any};
			auto eachRestrictionPtr {std::ranges::data(eachRestriction)};
			auto eachRestrictionLength = std::ranges::size(eachRestriction);
            ByteUnitedMemoryLinks_Each(ptr, eachRestrictionPtr, eachRestrictionLength, call_last_global<TLinkAddress, EachSignature, Link<TLinkAddress>>);
            std::cout << "Found link: " << static_cast<uint64_t>(foundLink) << std::endl;
			set_global<EachSignature>(eachCallback);
			ByteUnitedMemoryLinks_Each(ptr, eachRestriction.data(), eachRestriction.size(), call_last_global<TLinkAddress, EachSignature, Link<TLinkAddress>>);
			std::cout << "Found link: " << static_cast<uint64_t>(foundLink) << std::endl;
			set_global<EachSignature>(eachCallback);
			ByteUnitedMemoryLinks_Each(ptr, eachRestriction.data(), eachRestriction.size(), call_last_global<TLinkAddress, EachSignature, Link<TLinkAddress>>);
			std::cout << "Found link: " << static_cast<uint64_t>(foundLink) << std::endl;
			set_global<EachSignature>(eachCallback);
			ByteUnitedMemoryLinks_Each(ptr, eachRestriction.data(), eachRestriction.size(), call_last_global<TLinkAddress, EachSignature, Link<TLinkAddress>>);
			std::cout << "Found link: " << static_cast<uint64_t>(foundLink) << std::endl;
		}
        catch (const std::exception& e)
        {
            std::remove(tempName);
            std::cerr << e.what() << std::endl;
            throw;
        }
        std::remove(tempName);
    }
}

