#include <cassert>
#include <concepts>
#include <cstdint>
#include <functional>
#include "../../cpp/Platform.Data.Doublets/Memory/ILinksListMethods.h"
#include "../../cpp/Platform.Data.Doublets/Memory/ILinksTreeMethods.h"

using Address = std::uint64_t;
struct Options
{
    using LinkAddressType = Address;
    using LinkType = Address;
    using ReadHandlerType = std::function<Address(Address)>;
};
struct List : Platform::Data::Doublets::Memory::ILinksListMethods<Address>
{
    Address value = 0;
    void Detach(Address link)
    {
        value = link;
    }
    void AttachAsFirst(Address link)
    {
        value = link;
    }
};
struct Tree : Platform::Data::Doublets::Memory::ILinksTreeMethods<Options>
{
    Address CountUsages(Address root)
    {
        return root;
    }
    Address Search(Address source, Address target)
    {
        return source + target;
    }
    Address EachUsage(Address root, const ReadHandlerType& handler)
    {
        return handler(root);
    }
    void Detach(Address& root, Address link)
    {
        root -= link;
    }
    void Attach(Address& root, Address link)
    {
        root += link;
    }
};
int main()
{
    List list;
    Platform::Data::Doublets::Memory::ILinksListMethods<Address>& listInterface = list;
    listInterface.AttachAsFirst(4);
    assert(list.value == 4);
    listInterface.Detach(2);
    assert(list.value == 2);
    Tree tree;
    Platform::Data::Doublets::Memory::ILinksTreeMethods<Options>& treeInterface = tree;
    Address root = 5;
    assert(treeInterface.CountUsages(root) == 5);
    assert(treeInterface.Search(2, 3) == 5);
    assert(treeInterface.EachUsage(root, [](Address value) { return value + 1; }) == 6);
    treeInterface.Attach(root, 3);
    assert(root == 8);
    treeInterface.Detach(root, 2);
    assert(root == 6);
}
