#include <Platform.Data.Doublets.h>

UInt64Link createdLink;
UInt64Link updatedLink;
UInt64Link foundLink;
UInt64Link deletedLink;

uint64_t createHandler(UInt64Link before, UInt64Link after)
{
    createdLink = after;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t updateHandler(UInt64Link before, UInt64Link after)
{
    updatedLink = after;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t eachHandler(UInt64Link link)
{
    foundLink = link;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t deleteHandler(UInt64Link before, UInt64Link after)
{
    deletedLink = before;
    return DefaultUInt64LinksConstants.Continue;
}

int main() {
    const char* tempFileName = tmpnam(NULL);
    void* uint64FfiStorage = UInt64Links_New(tempFileName);
    UInt64Links_Create(uint64FfiStorage, NULL, 0, createHandler);
    printf("Created link: %lu : %lu -> %lu \n", createdLink.Index, createdLink.Source, createdLink.Target);
    const uint64_t restriction[1] = {1};
    const uint64_t substitution[3] = {1, 1, 1};
    UInt64Links_Update(uint64FfiStorage, (const uint64_t*) &restriction, 1, (const uint64_t*) &substitution, 3, updateHandler);
    printf("Updated link: %lu : %lu -> %lu \n", updatedLink.Index, updatedLink.Source, updatedLink.Target);
    UInt64Links_Each(uint64FfiStorage, (const uint64_t*) &restriction, 1, eachHandler);
    printf("Found link after update: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    UInt64Links_Delete(uint64FfiStorage, (const uint64_t*) &restriction, 1, deleteHandler);
    printf("Deleted link: %lu : %lu -> %lu \n", deletedLink.Index, deletedLink.Source, deletedLink.Target);
    memset(&foundLink, 0, sizeof(foundLink));
    UInt64Links_Each(uint64FfiStorage, (const uint64_t*) &restriction, 1, eachHandler);
    printf("Found link after delete: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    UInt64Links_Drop(uint64FfiStorage);
    remove(tempFileName);
    return 0;
}
