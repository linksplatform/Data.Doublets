#include <Platform.Data.Doublets.h>

int main() {
    void* ffiStoragePtr = UInt64Links_New("db.links");
    UInt64Links_Drop(ffiStoragePtr);
    free(ffiStoragePtr);
    return 0;
}
