
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

void* ByteLinks_New(const char* path);

void* UInt16Links_New(const char* path);

void* UInt32Links_New(const char* path);

void* UInt64Links_New(const char* path);

void ByteLinks_Drop(void* this_);

void UInt16Links_Drop(void* this_);

void UInt32Links_Drop(void* this_);

void UInt64Links_Drop(void* this_);

uint8_t ByteLinks_Create(void* this_,
        const uint8_t* query,
        uintptr_t len,
        CUDCallback_u8 callback);

uint16_t UInt16Links_Create(void* this_,
        const uint16_t* query,
        uintptr_t len,
        CUDCallback_u16 callback);

uint32_t UInt32Links_Create(void* this_,
        const uint32_t* query,
        uintptr_t len,
        CUDCallback_u32 callback);

uint64_t UInt64Links_Create(void* this_,
        const uint64_t* query,
        uintptr_t len,
        CUDCallback_u64 callback);

uint8_t ByteLinks_Each(void* this_,
        const uint8_t* query,
        uintptr_t len,
        EachCallback_u8 callback);

uint16_t UInt16Links_Each(void* this_,
        const uint16_t* query,
        uintptr_t len,
        EachCallback_u16 callback);

uint32_t UInt32Links_Each(void* this_,
        const uint32_t* query,
        uintptr_t len,
        EachCallback_u32 callback);

uint64_t UInt64Links_Each(void* this_,
        const uint64_t* query,
        uintptr_t len,
        EachCallback_u64 callback);

uint8_t ByteLinks_Count(void* this_, const uint8_t* query, uintptr_t len);

uint16_t UInt16Links_Count(void* this_, const uint16_t* query, uintptr_t len);

uint32_t UInt32Links_Count(void* this_, const uint32_t* query, uintptr_t len);

uint64_t UInt64Links_Count(void* this_, const uint64_t* query, uintptr_t len);

uint8_t ByteLinks_Update(void* this_,
        const uint8_t* query,
        uintptr_t len_q,
        const uint8_t* replacement,
        uintptr_t len_r,
        CUDCallback_u8 callback);

uint16_t UInt16Links_Update(void* this_,
        const uint16_t* query,
        uintptr_t len_q,
        const uint16_t* replacement,
        uintptr_t len_r,
        CUDCallback_u16 callback);

uint32_t UInt32Links_Update(void* this_,
        const uint32_t* query,
        uintptr_t len_q,
        const uint32_t* replacement,
        uintptr_t len_r,
        CUDCallback_u32 callback);

uint64_t UInt64Links_Update(void* this_,
        const uint64_t* query,
        uintptr_t len_q,
        const uint64_t* replacement,
        uintptr_t len_r,
        CUDCallback_u64 callback);

uint8_t ByteLinks_Delete(void* this_,
        const uint8_t* query,
        uintptr_t len,
        CUDCallback_u8 callback);

uint16_t UInt16Links_Delete(void* this_,
        const uint16_t* query,
        uintptr_t len,
        CUDCallback_u16 callback);

uint32_t UInt32Links_Delete(void* this_,
        const uint32_t* query,
        uintptr_t len,
        CUDCallback_u32 callback);

uint64_t UInt64Links_Delete(void* this_,
        const uint64_t* query,
        uintptr_t len,
        CUDCallback_u64 callback);

uint64_t UInt64Links_SmartCreate(void* this_);
uint64_t UInt64Links_SmartUpdate(void* this_, uint64_t index, uint64_t source, uint64_t target);


void foo(int);