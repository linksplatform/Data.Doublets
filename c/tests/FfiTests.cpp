#include <gtest/gtest.h>
#include <Platform.Data.Doublets.h>

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

TEST(FfiTests, Test)
{
    memset(&createdLink, 0, sizeof(createdLink));
    memset(&updatedLink, 0, sizeof(updatedLink));
    memset(&foundLink, 0, sizeof(foundLink));
    memset(&deletedLink, 0, sizeof(deletedLink));
    const char* tempFileName = tmpnam(NULL);
    void* storage = UInt64Links_New(tempFileName);
    UInt64Links_Create(storage, NULL, 0, CreateHandler);
    ASSERT_EQ(createdLink.Index, 1);
    ASSERT_EQ(createdLink.Source, 0);
    ASSERT_EQ(createdLink.Target, 0);
    printf("Created link: %lu : %lu -> %lu \n", createdLink.Index, createdLink.Source, createdLink.Target);
    const uint64_t restriction[1] = {1};
    const uint64_t substitution[3] = {1, 1, 1};
    UInt64Links_Update(storage, (const uint64_t*) &restriction, 1, (const uint64_t*) &substitution, 3, UpdateHandler);
    printf("Updated link: %lu : %lu -> %lu \n", updatedLink.Index, updatedLink.Source, updatedLink.Target);
    ASSERT_EQ(updatedLink.Index, 1);
    ASSERT_EQ(updatedLink.Source, 1);
    ASSERT_EQ(updatedLink.Target, 1);
    UInt64Links_Each(storage, (const uint64_t*) &restriction, 1, EachHandler);
    printf("Found link after update: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    ASSERT_EQ(foundLink.Index, 1);
    ASSERT_EQ(foundLink.Source, 1);
    ASSERT_EQ(foundLink.Target, 1);
    UInt64Links_Delete(storage, (const uint64_t*) &restriction, 1, DeleteHandler);
    printf("Deleted link: %lu : %lu -> %lu \n", deletedLink.Index, deletedLink.Source, deletedLink.Target);
    ASSERT_EQ(deletedLink.Index, 1);
    ASSERT_EQ(deletedLink.Source, 1);
    ASSERT_EQ(deletedLink.Target, 1);
    memset(&foundLink, 0, sizeof(foundLink));
    UInt64Links_Each(storage, (const uint64_t*) &restriction, 1, EachHandler);
    printf("Found link after delete: %lu : %lu -> %lu \n", foundLink.Index, foundLink.Source, foundLink.Target);
    ASSERT_EQ(foundLink.Index, 0);
    ASSERT_EQ(foundLink.Source, 0);
    ASSERT_EQ(foundLink.Target, 0);
    UInt64Links_Drop(storage);
    remove(tempFileName);
}
