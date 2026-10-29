// A self-typed constexpr value needs its definition after the type is complete.
// Referencing a variable template delays initialization until that point.
#include <concepts>
template<std::integral T> struct Link;
template<std::integral T> inline constexpr Link<T> NullLink{};
template<std::integral T> struct Link {
    T Index{};
    static constexpr const Link& Null = NullLink<T>;
};
constexpr auto value = Link<int>::Null;
static_assert(value.Index == 0);
int main() {}
