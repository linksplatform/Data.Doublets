#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <dlfcn.h>

// Function pointer types matching the exported functions
typedef void* (*CreateLinksFunc)(const char* path);
typedef void (*DropLinksFunc)(void* handle);
typedef uint64_t (*CreateLinkFunc)(void* handle, uint64_t* query, size_t queryLen);
typedef uint64_t (*CountLinksFunc)(void* handle, uint64_t* query, size_t queryLen);
typedef uint64_t (*UpdateLinkFunc)(void* handle, uint64_t* query, size_t queryLen, uint64_t* replacement, size_t replacementLen);
typedef uint64_t (*DeleteLinkFunc)(void* handle, uint64_t* query, size_t queryLen);
typedef const char* (*GetVersionFunc)(void);

int main() {
    // Load the native library
    void* lib = dlopen("./Platform.Data.Doublets.NativeLibrary.so", RTLD_LAZY);
    if (!lib) {
        fprintf(stderr, "Failed to load library: %s\n", dlerror());
        return 1;
    }

    // Load function pointers
    CreateLinksFunc createLinks = (CreateLinksFunc)dlsym(lib, "UInt64UnitedMemoryLinks_New");
    DropLinksFunc dropLinks = (DropLinksFunc)dlsym(lib, "UInt64UnitedMemoryLinks_Drop");
    CreateLinkFunc createLink = (CreateLinkFunc)dlsym(lib, "UInt64UnitedMemoryLinks_Create");
    CountLinksFunc countLinks = (CountLinksFunc)dlsym(lib, "UInt64UnitedMemoryLinks_Count");
    UpdateLinkFunc updateLink = (UpdateLinkFunc)dlsym(lib, "UInt64UnitedMemoryLinks_Update");
    DeleteLinkFunc deleteLink = (DeleteLinkFunc)dlsym(lib, "UInt64UnitedMemoryLinks_Delete");
    GetVersionFunc getVersion = (GetVersionFunc)dlsym(lib, "GetLibraryVersion");

    if (!createLinks || !dropLinks || !createLink || !countLinks || !getVersion) {
        fprintf(stderr, "Failed to load required functions\n");
        dlclose(lib);
        return 1;
    }

    // Get and display library version
    const char* version = getVersion();
    printf("Platform.Data.Doublets Native Library Version: %s\n", version);

    // Create a new links database in memory
    void* links = createLinks("test-native.links");
    if (!links) {
        printf("Failed to create links database\n");
        dlclose(lib);
        return 1;
    }

    printf("Successfully created links database\n");

    // Create some links
    uint64_t query[2] = {0, 0}; // [any, any]
    
    printf("Creating links...\n");
    uint64_t link1 = createLink(links, query, 2);
    printf("Created link 1: %llu\n", link1);
    
    uint64_t link2 = createLink(links, query, 2);
    printf("Created link 2: %llu\n", link2);
    
    uint64_t link3 = createLink(links, query, 2);
    printf("Created link 3: %llu\n", link3);

    // Count all links
    uint64_t count = countLinks(links, query, 2);
    printf("Total links in database: %llu\n", count);

    // Create a specific link [link1, link2]
    uint64_t specific_query[2] = {link1, link2};
    uint64_t specific_link = createLink(links, specific_query, 2);
    printf("Created specific link [%llu, %llu]: %llu\n", link1, link2, specific_link);

    // Count again
    count = countLinks(links, query, 2);
    printf("Total links after creating specific link: %llu\n", count);

    // Test updating a link (change link3 to point from link1 to link2)
    uint64_t update_query[3] = {link3, 0, 0}; // link to update
    uint64_t update_replacement[3] = {link3, link1, link2}; // new values
    uint64_t update_result = updateLink(links, update_query, 3, update_replacement, 3);
    printf("Updated link %llu to [%llu, %llu]: result=%llu\n", link3, link1, link2, update_result);

    // Clean up
    dropLinks(links);
    dlclose(lib);

    printf("Test completed successfully!\n");
    return 0;
}