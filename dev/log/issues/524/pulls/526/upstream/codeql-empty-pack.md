CodeQL CLI **2.27.3** with a full traced C++20 dependency-aware build reports:

> Static variable restrictionPack is never read.

The location is a **function parameter pack**, which is expanded in a braced initializer; it is not a static-storage variable. This is the sole remaining native finding in https://github.com/linksplatform/Data.Doublets/actions/runs/40840157071/job/123334722755 (line 3847 of the downloadable job log). The complete `codeql-c-cpp` SARIF artifact contains one result, `cpp/unused-static-variable`.

Source at the exact analyzed commit: https://github.com/linksplatform/Data.Doublets/blob/3e61f330dcade7dc2871e31d4cc91b03aa3416aa/cpp/Platform.Data.Doublets/ILinksExtensions.h#L250

```cpp
template <typename TStorage>
static auto All(const TStorage& storage,
    std::convertible_to<typename TStorage::LinkAddressType> auto... restrictionPack)
{
    typename TStorage::LinkType restriction{
        static_cast<typename TStorage::LinkAddressType>(restrictionPack)...};
    return All(storage, restriction);
}
```

The test build instantiates this with both zero and one restriction arguments. Runtime tests confirm that the nonempty pack is used to restrict the result. I suspect the zero-length instantiation leaves an extracted variable with no expansion accesses, which is misclassified or not excluded by the unused-static query. This is an inference from the source and SARIF; I have not inspected the database's raw variable facts.

### Reduced example

```cpp
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
```

Both GCC and Clang 18 compile and execute this with `-std=c++20 -Wall -Wextra -Werror -pedantic`. I have not independently analyzed this reduced example locally with CodeQL; the observed CodeQL result is from the real build linked above. To test it, create a C/C++ CodeQL database tracing `c++ -std=c++20 -c repro.cpp` and analyze the `security-and-quality` suite, or the UnusedStaticVariables query directly.

### Workaround

Provide a separate `All(storage)` overload that forwards an empty collection, and declare the variadic overload with explicit template parameters and a nonempty constraint:

```cpp
template <typename TStorage, typename... Values>
requires (sizeof...(Values) > 0 &&
          (std::convertible_to<Values, typename TStorage::LinkAddressType> && ...))
static auto All(const TStorage& storage, Values... values);
```

This retains zero-, one-, and collection-argument calls and their behavior without suppressing the query. Full native regressions cover the original calls; fresh CodeQL verification of this workaround is pending.

### Suggested correction

Please inspect the extractor's variable/storage classification and access facts for empty abbreviated parameter-pack instantiations. Function parameters/pack placeholders should not be reported as unused static-storage objects. Add zero/nonzero pack regression cases to `cpp/ql/src/Best Practices/Unused Entities/UnusedStaticVariables.ql`; if extraction retains a placeholder variable, explicitly exclude it or associate expansion accesses correctly.
