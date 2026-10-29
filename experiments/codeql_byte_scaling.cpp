// Reproduction for CodeQL cpp/suspicious-pointer-scaling.
// The offset is a record index, not a byte offset.
#include <cstddef>

struct Record
{
    int value;
};

Record& At(std::byte* storage, std::size_t recordIndex)
{
    return *(reinterpret_cast<Record*>(storage) + recordIndex);
}

Record& AtUsingSubscript(std::byte* storage, std::size_t recordIndex)
{
    return reinterpret_cast<Record*>(storage)[recordIndex];
}

int main()
{
    Record records[]{{11}, {22}};
    auto* storage = reinterpret_cast<std::byte*>(records);
    return At(storage, 1).value == 22 && &At(storage, 1) == &AtUsingSubscript(storage, 1) ? 0 : 1;
}
