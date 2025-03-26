#pragma once

#define CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES($Macro) \
    $Macro(UInt8);                                              \
    $Macro(UInt16);                                             \
    $Macro(UInt32);                                             \
    $Macro(UInt64);

#define CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES($Macro) \
    $Macro(uint8_t, UInt8);                                                   \
    $Macro(uint16_t, UInt16);                                                 \
    $Macro(uint32_t, UInt32);                                                 \
    $Macro(uint64_t, UInt64);

#define DECLARE_LINK($TLinkAddress, $Prefix) \
    typedef struct $Prefix##Link             \
    {                                        \
        $TLinkAddress Index;                 \
        $TLinkAddress Source;                \
        $TLinkAddress Target;                \
    } $Prefix##Link;

#define DECLARE_WRITE_HANDLER($TLinkAddress, $Prefix) \
    typedef $TLinkAddress (*$Prefix##CUDCallback)($Prefix##Link before, $Prefix##Link after);

#define DECLARE_READ_HANDLER($TLinkAddress, $Prefix) \
    typedef $TLinkAddress (*$Prefix##EachCallback)($Prefix##Link);

//typedef struct SharedLogger {
//    void (* formatter)(const Record*);
//} SharedLogger;

#define DECLARE_UNITED_MEMORY_LINKS_NEW($Prefix) \
    void* $Prefix##Links_New(const char* path);

#define DECLARE_UNITED_MEMORY_LINKS_NEW_WITH_CONSTANTS($Prefix) \
    void* $Prefix##Links_NewWithConstants(const char* path, $Prefix##LinksConstantsType* constants);

#define DECLARE_UNITED_MEMORY_LINKS_DROP($Prefix) \
    void $Prefix##Links_Drop(void* storage);

#define DECLARE_UNITED_MEMORY_LINKS_CREATE($TLinkAddress, $Prefix)  \
    $TLinkAddress $Prefix##Links_Create(void* storage,                \
                                        const $TLinkAddress* substitution, \
                                        uintptr_t len,              \
                                        $Prefix##CUDCallback callback);

#define DECLARE_UNITED_MEMORY_LINKS_EACH($TLinkAddress, $Prefix)  \
    $TLinkAddress $Prefix##Links_Each(void* storage,                \
                                      const $TLinkAddress* restriction, \
                                      uintptr_t len,              \
                                      $Prefix##EachCallback callback);

#define DECLARE_UNITED_MEMORY_LINKS_COUNT($TLinkAddress, $Prefix) \
    uintptr_t $Prefix##Links_Count(void* storage,                   \
                                   const $TLinkAddress* restriction,    \
                                   uintptr_t len);

#define DECLARE_UNITED_MEMORY_LINKS_UPDATE($TLinkAddress, $Prefix)         \
    $TLinkAddress $Prefix##Links_Update(void* storage,                       \
                                        const $TLinkAddress* restriction, \
                                        uintptr_t len_r,                   \
                                        const $TLinkAddress* substitution, \
                                        uintptr_t len_s,                   \
                                        $Prefix##CUDCallback callback);

#define DECLARE_UNITED_MEMORY_LINKS_DELETE($TLinkAddress, $Prefix) \
    void $Prefix##Links_Delete(void* storage,                        \
                               const $TLinkAddress* restriction,         \
                               uintptr_t len,                      \
                               $Prefix##CUDCallback callback);


//void setup_shared_logger(struct SharedLogger logger);

//void init_fmt_logger(void);

#if defined(__cplusplus)
extern "C" {
#endif
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_RANGE);
DECLARE_LINKS_CONSTANTS(uint8_t, UInt8, UINT8);
DECLARE_LINKS_CONSTANTS(uint16_t, UInt16, UINT16);
DECLARE_LINKS_CONSTANTS(uint32_t, UInt32, UINT32);
DECLARE_LINKS_CONSTANTS(uint64_t, UInt64, UINT64);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_LINK);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_WRITE_HANDLER);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_READ_HANDLER);
CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_NEW);
CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_DROP);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_CREATE);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_EACH);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_COUNT);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_UPDATE);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_DELETE);
#if defined(__cplusplus)
}
#endif
