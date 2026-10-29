// Reduced example for CodeQL's unread-static-variable diagnostic on an empty pack.
#include <concepts>
#include <vector>

struct Store
{
    using Value = unsigned;
};

template <typename TStore>
static auto Select(const TStore&, std::convertible_to<typename TStore::Value> auto... values)
{
    return std::vector<typename TStore::Value>{static_cast<typename TStore::Value>(values)...};
}

int main()
{
    Store store;
    return Select(store).empty() && Select(store, 2U) == std::vector{2U} ? 0 : 1;
}
