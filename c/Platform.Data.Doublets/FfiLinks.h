#pragma once

#define CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES($Macro) \
    $Macro(uint8_t);                                             \
    $Macro(uint16_t);                                            \
    $Macro(uint32_t);                                            \
    $Macro(uint64_t);

#define CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES($Macro) \
    $Macro(Byte);                                             \
    $Macro(UInt16);                                            \
    $Macro(UInt32);                                            \
    $Macro(UInt64);

#define CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES($Macro) \
    $Macro(uint8_t, Byte);                                             \
    $Macro(uint16_t, UInt16);                                            \
    $Macro(uint32_t, UInt32);                                            \
    $Macro(uint64_t, UInt64);

#define DECLARE_RANGE($TLinkAddress)                                                               \
    typedef struct Range_##$TLinkAddress                                                                     \
    {                                                                                              \
        $TLinkAddress Minimum;                                                               \
        $TLinkAddress Maximum;                                                               \
    } Range_##$TLinkAddress;                                                                                             \
//    void InitializeRange_##$TLinkAddress( Range_##$TLinkAddress* range, $TLinkAddress minimum, $TLinkAddress maximum) \
//    {                                                                                              \
//        range->Minimum = minimum;                                                                  \
//        range->Maximum = maximum;                                                                  \
//    }

#define DECLARE_LINKS_CONSTANTS($TLinkAddress) \
    typedef struct LinksConstants_##$TLinkAddress        \
    {                                          \
        $TLinkAddress index_part;              \
        $TLinkAddress source_part;             \
        $TLinkAddress target_part;             \
        $TLinkAddress null;                    \
        $TLinkAddress $continue;               \
        $TLinkAddress $break;                  \
        $TLinkAddress skip;                    \
        $TLinkAddress any;                     \
        $TLinkAddress itself;                  \
        $TLinkAddress error;                   \
        Range_##$TLinkAddress internal_range;   \
        Range_##$TLinkAddress external_range;   \
        bool _opt_marker;                      \
    } LinksConstants_##$TLinkAddress;


#define DECLARE_LINK($TLinkAddress) \
    typedef struct Link_##$TLinkAddress       \
    {                               \
        $TLinkAddress index;        \
        $TLinkAddress source;       \
        $TLinkAddress target;       \
    } Link_##$TLinkAddress;

#define DECLARE_WRITE_HANDLER($TLinkAddress) \
    typedef $TLinkAddress (*CUDCallback_##$TLinkAddress)(Link_##$TLinkAddress before, Link_##$TLinkAddress after);

#define DECLARE_READ_HANDLER($TLinkAddress) \
    typedef $TLinkAddress (*EachCallback_##$TLinkAddress)(Link_##$TLinkAddress);

//typedef struct SharedLogger {
//    void (* formatter)(const Record*);
//} SharedLogger;

#define DECLARE_UNITED_MEMORY_LINKS_NEW($FunctionPrefixType) \
    void* $FunctionPrefixType##Links_New(const char* path);

#define DECLARE_UNITED_MEMORY_LINKS_DROP($FunctionPrefixType) \
    void $FunctionPrefixType##Links_Drop(void* this_); 

#define DECLARE_UNITED_MEMORY_LINKS_CREATE($TLinkAddress, $FunctionPrefixType) \
    $TLinkAddress $FunctionPrefixType##Links_Create(void* this_, \
                                         const $TLinkAddress* query, \
                                         uintptr_t len, \
                                         CUDCallback_##$TLinkAddress callback); 


#define DECLARE_UNITED_MEMORY_LINKS_EACH($TLinkAddress, $FunctionPrefixType) \
    $TLinkAddress $FunctionPrefixType##Links_Each(void* this_, \
                                         const $TLinkAddress* query, \
                                         uintptr_t len, \
                                         EachCallback_##$TLinkAddress callback); 

#define DECLARE_UNITED_MEMORY_LINKS_COUNT($TLinkAddress, $FunctionPrefixType) \
    uintptr_t $FunctionPrefixType##Links_Count(void* this_, \
                                         const $TLinkAddress* query, \
                                         uintptr_t len); 

#define DECLARE_UNITED_MEMORY_LINKS_UPDATE($TLinkAddress, $FunctionPrefixType) \
    $TLinkAddress $FunctionPrefixType##Links_Update(void* this_, \
                                         const $TLinkAddress* query, \
                                         uintptr_t len, \
                                         CUDCallback_##$TLinkAddress callback); 

#define DECLARE_UNITED_MEMORY_LINKS_DELETE($TLinkAddress, $FunctionPrefixType) \
    void $FunctionPrefixType##Links_Delete(void* this_, \
                                         const $TLinkAddress* query, \
                                         uintptr_t len); 

//void setup_shared_logger(struct SharedLogger logger);

//void init_fmt_logger(void);


CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES(DECLARE_RANGE);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES(DECLARE_LINKS_CONSTANTS);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES(DECLARE_LINK);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES(DECLARE_WRITE_HANDLER);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES(DECLARE_READ_HANDLER);
CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_NEW);
CALL_MACRO_WITH_SUPPORTED_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_DROP);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_CREATE);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_EACH);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_COUNT);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_UPDATE);
CALL_MACRO_WITH_SUPPORTED_TLINKADDRESS_TYPES_AND_FUNCTION_PREFIX_TYPES(DECLARE_UNITED_MEMORY_LINKS_DELETE);

