#include <cstdint>
#include <string>

namespace Platform::Data::Doublets::Memory::United::Generic::Ffi
{
    template <typename T>
    using CUDCallback = T (*)(Link<T> before, Link<T> after);

    template <typename T>
    using EachCallback = T (*)(Link<T>);

    struct SharedLogger
    {
        //void (*formatter)(const Record*);
    };

    namespace externC
    {
        extern "C"
        {
            void *ByteUnitedMemoryLinksFfi_New(const char *path);
            void *UInt16UnitedMemoryLinksFfi_New(const char *path);
            void *UInt32UnitedMemoryLinksFfi_New(const char *path);
            void *UInt64UnitedMemoryLinksFfi_New(const char *path);

            void ByteUnitedMemoryLinksFfi_Drop(void *this_);
            void UInt16UnitedMemoryLinksFfi_Drop(void *this_);
            void UInt32UnitedMemoryLinksFfi_Drop(void *this_);
            void UInt64UnitedMemoryLinksFfi_Drop(void *this_);

            uint8_t ByteUnitedMemoryLinksFfi_Create(void *this_,
                                                 const uint8_t *query,
                                                 uintptr_t len,
                                                 CUDCallback<uint8_t> callback);

            uint16_t UInt16UnitedMemoryLinksFfi_Create(void *this_,
                                                    const uint16_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint16_t> callback);

            uint32_t UInt32UnitedMemoryLinksFfi_Create(void *this_,
                                                    const uint32_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint32_t> callback);

            uint64_t UInt64UnitedMemoryLinksFfi_Create(void *this_,
                                                    const uint64_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint64_t> callback);

            uint8_t ByteUnitedMemoryLinksFfi_Each(void *this_,
                                               const uint8_t *query,
                                               uintptr_t len,
                                               EachCallback<uint8_t> callback);

            uint16_t UInt16UnitedMemoryLinksFfi_Each(void *this_,
                                                  const uint16_t *query,
                                                  uintptr_t len,
                                                  EachCallback<uint16_t> callback);

            uint32_t UInt32UnitedMemoryLinksFfi_Each(void *this_,
                                                  const uint32_t *query,
                                                  uintptr_t len,
                                                  EachCallback<uint32_t> callback);

            uint64_t UInt64UnitedMemoryLinksFfi_Each(void *this_,
                                                  const uint64_t *query,
                                                  uintptr_t len,
                                                  EachCallback<uint64_t> callback);

            uint8_t ByteUnitedMemoryLinksFfi_Count(void *this_, const uint8_t *query, uintptr_t len);
            uint16_t UInt16UnitedMemoryLinksFfi_Count(void *this_, const uint16_t *query, uintptr_t len);
            uint32_t UInt32UnitedMemoryLinksFfi_Count(void *this_, const uint32_t *query, uintptr_t len);
            uint64_t UInt64UnitedMemoryLinksFfi_Count(void *this_, const uint64_t *query, uintptr_t len);

            uint8_t ByteUnitedMemoryLinksFfi_Update(void *this_,
                                                 const uint8_t *restrictions,
                                                 uintptr_t len_r,
                                                 const uint8_t *substitutuion,
                                                 uintptr_t len_s,
                                                 CUDCallback<uint8_t> callback);

            uint16_t UInt16UnitedMemoryLinksFfi_Update(void *this_,
                                                    const uint16_t *restrictions,
                                                    uintptr_t len_r,
                                                    const uint16_t *substitutuion,
                                                    uintptr_t len_s,
                                                    CUDCallback<uint16_t> callback);

            uint32_t UInt32UnitedMemoryLinksFfi_Update(void *this_,
                                                    const uint32_t *restrictions,
                                                    uintptr_t len_r,
                                                    const uint32_t *substitutuion,
                                                    uintptr_t len_s,
                                                    CUDCallback<uint32_t> callback);

            uint64_t UInt64UnitedMemoryLinksFfi_Update(void *this_,
                                                    const uint64_t *restrictions,
                                                    uintptr_t len_r,
                                                    const uint64_t *substitutuion,
                                                    uintptr_t len_s,
                                                    CUDCallback<uint64_t> callback);

            uint8_t ByteUnitedMemoryLinksFfi_Delete(void *this_,
                                                 const uint8_t *query,
                                                 uintptr_t len,
                                                 CUDCallback<uint8_t> callback);

            uint16_t UInt16UnitedMemoryLinksFfi_Delete(void *this_,
                                                    const uint16_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint16_t> callback);

            uint32_t UInt32UnitedMemoryLinksFfi_Delete(void *this_,
                                                    const uint32_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint32_t> callback);

            uint64_t UInt64UnitedMemoryLinksFfi_Delete(void *this_,
                                                    const uint64_t *query,
                                                    uintptr_t len,
                                                    CUDCallback<uint64_t> callback);

            void setup_shared_logger(SharedLogger logger);

            void init_fmt_logger();

        } // extern "C"
    }

    template <typename TLink>
    class UnitedMemoryLinksFfi;

    template <>
    class UnitedMemoryLinksFfi<uint8_t>
    {
    private:
        void *this_{nullptr};

    public:
        UnitedMemoryLinksFfi(const std::string_view &path)
        {
            this_ = externC::ByteUnitedMemoryLinksFfi_New(path.data());
        }

        ~UnitedMemoryLinksFfi()
        {
            if (this_)
                externC::ByteUnitedMemoryLinksFfi_Drop(this_);
        }
    };


    template <>
    class UnitedMemoryLinksFfi<uint16_t>
    {
    private:
        void *this_{nullptr};

    public:
        UnitedMemoryLinksFfi(const std::string_view &path)
        {
            this_ = externC::UInt16UnitedMemoryLinksFfi_New(path.data());
        }

        ~UnitedMemoryLinksFfi()
        {
            if (this_)
                externC::UInt16UnitedMemoryLinksFfi_Drop(this_);
        }
    };


    template <>
    class UnitedMemoryLinksFfi<uint32_t>
    {
    private:
        void *this_{nullptr};

    public:
        UnitedMemoryLinksFfi(const std::string_view &path)
        {
            this_ = externC::UInt32UnitedMemoryLinksFfi_New(path.data());
        }

        ~UnitedMemoryLinksFfi()
        {
            if (this_)
                externC::UInt32UnitedMemoryLinksFfi_Drop(this_);
        }
    };


    template <>
    class UnitedMemoryLinksFfi<uint64_t>
    {
    private:
        void *this_{nullptr};

    public:
        UnitedMemoryLinksFfi(const std::string_view &path)
        {
            this_ = externC::UInt64UnitedMemoryLinksFfi_New(path.data());
        }

        ~UnitedMemoryLinksFfi()
        {
            if (this_)
                externC::UInt64UnitedMemoryLinksFfi_Drop(this_);
        }
    };
}
