#include <Platform.Data.Doublets.h>

//#define DECLARE_TEST($Prefix) \

UInt64Link createdLink;
UInt64Link updatedLink;
UInt64Link foundLink;
UInt64Link deletedLink;

uint64_t CreateHandler(UInt64Link before, UInt64Link after)
{
    createdLink = after;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t UpdateHandler(UInt64Link before, UInt64Link after)
{
    updatedLink = after;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t EachHandler(UInt64Link link)
{
    foundLink = link;
    return DefaultUInt64LinksConstants.Continue;
}

uint64_t DeleteHandler(UInt64Link before, UInt64Link after)
{
    deletedLink = before;
    return DefaultUInt64LinksConstants.Continue;
}

int main()
{
    memset(&createdLink, 0, sizeof(createdLink));
    memset(&updatedLink, 0, sizeof(updatedLink));
    memset(&foundLink, 0, sizeof(foundLink));
    memset(&deletedLink, 0, sizeof(deletedLink));
    const char* tempFileName = tmpnam(NULL);
    void* uint64FfiStorage = UInt64Links_New(tempFileName);
    UInt64Links_Create(uint64FfiStorage, NULL, 0, CreateHandler);
    printf("Created link: %lu : %lu -> %lu \n", createdLink.Index, createdLink.Source, createdLink.Target);
    const uint64_t restriction[1] = {1};
    const uint64_t substitution[3] = {1, 1, 1};
    UInt64Links_Update(uint64FfiStorage, (const uint64_t*) &restriction, 1, (const uint64_t*) &substitution, 3, UpdateHandler);
    printf("Updated link: %lu : %lu -> %lu \n", updatedLink.Index, updatedLink.Source, updatedLink.Target);
    UInt64Links_Each(uint64FfiStorage, (const uint64_t*) &restriction, 1, EachHandler);
    printf("Found link after update: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    UInt64Links_Delete(uint64FfiStorage, (const uint64_t*) &restriction, 1, DeleteHandler);
    printf("Deleted link: %lu : %lu -> %lu \n", deletedLink.Index, deletedLink.Source, deletedLink.Target);
    memset(&foundLink, 0, sizeof(foundLink));
    UInt64Links_Each(uint64FfiStorage, (const uint64_t*) &restriction, 1, EachHandler);
    printf("Found link after delete: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    UInt64Links_Drop(uint64FfiStorage);
    remove(tempFileName);
    return 0;
}
