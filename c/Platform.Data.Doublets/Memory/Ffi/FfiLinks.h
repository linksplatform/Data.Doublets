#ifndef DATA_DOUBLETS_FFI_H
#define DATA_DOUBLETS_FFI_H
#include <stdarg.h>
#include <stdbool.h>
#include <stdint.h>
#include <stdlib.h>

typedef struct Link_u8 {
    uint8_t index;
    uint8_t source;
    uint8_t target;
} Link_u8;

typedef uint8_t (* CUDCallback_u8)(struct Link_u8 before, struct Link_u8 after);

typedef struct Link_u16 {
    uint16_t index;
    uint16_t source;
    uint16_t target;
} Link_u16;

typedef uint16_t (* CUDCallback_u16)(struct Link_u16 before, struct Link_u16 after);

typedef struct Link_u32 {
    uint32_t index;
    uint32_t source;
    uint32_t target;
} Link_u32;

typedef uint32_t (* CUDCallback_u32)(struct Link_u32 before, struct Link_u32 after);

typedef struct Link_u64 {
    uint64_t index;
    uint64_t source;
    uint64_t target;
} Link_u64;

typedef uint64_t (* CUDCallback_u64)(struct Link_u64 before, struct Link_u64 after);

typedef uint8_t (* EachCallback_u8)(struct Link_u8);

typedef uint16_t (* EachCallback_u16)(struct Link_u16);

typedef uint32_t (* EachCallback_u32)(struct Link_u32);

typedef uint64_t (* EachCallback_u64)(struct Link_u64);

//typedef struct SharedLogger {
//    void (* formatter)(const Record*);
//} SharedLogger;

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
        CUDCallback_u8 callback);

uint16_t UInt16UnitedMemoryLinks_Create(void* this_,
        const uint16_t* query,
        uintptr_t len,
        CUDCallback_u16 callback);

uint32_t UInt32UnitedMemoryLinks_Create(void* this_,
        const uint32_t* query,
        uintptr_t len,
        CUDCallback_u32 callback);

uint64_t UInt64UnitedMemoryLinks_Create(void* this_,
        const uint64_t* query,
        uintptr_t len,
        CUDCallback_u64 callback);

uint8_t ByteUnitedMemoryLinks_Each(void* this_,
        const uint8_t* query,
        uintptr_t len,
        EachCallback_u8 callback);

uint16_t UInt16UnitedMemoryLinks_Each(void* this_,
        const uint16_t* query,
        uintptr_t len,
        EachCallback_u16 callback);

uint32_t UInt32UnitedMemoryLinks_Each(void* this_,
        const uint32_t* query,
        uintptr_t len,
        EachCallback_u32 callback);

uint64_t UInt64UnitedMemoryLinks_Each(void* this_,
        const uint64_t* query,
        uintptr_t len,
        EachCallback_u64 callback);

uint8_t ByteUnitedMemoryLinks_Count(void* this_, const uint8_t* query, uintptr_t len);

uint16_t UInt16UnitedMemoryLinks_Count(void* this_, const uint16_t* query, uintptr_t len);

uint32_t UInt32UnitedMemoryLinks_Count(void* this_, const uint32_t* query, uintptr_t len);

uint64_t UInt64UnitedMemoryLinks_Count(void* this_, const uint64_t* query, uintptr_t len);

uint8_t ByteUnitedMemoryLinks_Update(void* this_,
        const uint8_t* query,
        uintptr_t len_q,
        const uint8_t* replacement,
        uintptr_t len_r,
        CUDCallback_u8 callback);

uint16_t UInt16UnitedMemoryLinks_Update(void* this_,
        const uint16_t* query,
        uintptr_t len_q,
        const uint16_t* replacement,
        uintptr_t len_r,
        CUDCallback_u16 callback);

uint32_t UInt32UnitedMemoryLinks_Update(void* this_,
        const uint32_t* query,
        uintptr_t len_q,
        const uint32_t* replacement,
        uintptr_t len_r,
        CUDCallback_u32 callback);

uint64_t UInt64UnitedMemoryLinks_Update(void* this_,
        const uint64_t* query,
        uintptr_t len_q,
        const uint64_t* replacement,
        uintptr_t len_r,
        CUDCallback_u64 callback);

uint8_t ByteUnitedMemoryLinks_Delete(void* this_,
        const uint8_t* query,
        uintptr_t len,
        CUDCallback_u8 callback);

uint16_t UInt16UnitedMemoryLinks_Delete(void* this_,
        const uint16_t* query,
        uintptr_t len,
        CUDCallback_u16 callback);

uint32_t UInt32UnitedMemoryLinks_Delete(void* this_,
        const uint32_t* query,
        uintptr_t len,
        CUDCallback_u32 callback);

uint64_t UInt64UnitedMemoryLinks_Delete(void* this_,
        const uint64_t* query,
        uintptr_t len,
        CUDCallback_u64 callback);

void setup_shared_logger(struct SharedLogger logger);

void init_fmt_logger(void);

#endif
