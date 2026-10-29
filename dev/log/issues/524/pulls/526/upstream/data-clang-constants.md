C++20 consumers of Platform.Data 0.1.1 fail under Clang 18 when an extension reads a static constant through a function parameter reference:

```cpp
struct Storage { static constexpr int Constants = 1; };
template<class TStorage> void operation(TStorage& storage) {
    constexpr auto constants = storage.Constants;
    (void)constants;
}
int main() { Storage storage; operation(storage); }
```

Reproduce with `clang++-18 -std=c++20 -Werror -fsyntax-only example.cpp`.
The diagnostic is `constexpr variable 'constants' must be initialized by a constant expression`, with a note that the function parameter has an unknown value. GCC accepts the same expression, so GCC-only CI misses it.

In Data.Doublets' real Clang build this originates from Platform.Data's `ILinksExtensions.h` functions, in addition to analogous expressions in Data.Doublets itself. It is recorded in [PR 526](https://github.com/linksplatform/Data.Doublets/pull/526) under `dev/log/issues/524/pulls/526/local/cpp-static-iteration2.log`.

Workaround / proposed code change: reference the static member through the template type, `constexpr auto constants = TStorage::Constants;`. For deduced cv/ref object types use `std::remove_cvref_t<decltype(storage)>::Constants`. This preserves constant-expression semantics and does not read the referenced object.

Please apply this to every matching C++ helper and test both GCC and Clang in C++20 mode. The downstream pinned Conan recipe applies the bounded replacement during `source()` until an upstream release includes it.
