#include <Platform.Data.Doublets.h>

int main() {
    void* ffiStoragePtr = UInt64UnitedMemoryLinks_New("db.links");
    UInt64UnitedMemoryLinks_Drop(ffiStoragePtr);
    free(ffiStoragePtr);
    return 0;
}
