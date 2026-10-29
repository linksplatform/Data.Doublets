Observed in a real C++20 Clang build with CodeQL CLI **2.27.3**, `security-and-quality`, on Ubuntu, rather than a no-build database. Public run: https://github.com/linksplatform/Data.Doublets/actions/runs/40836388311/job/123322406479 (the fatal SARIF gate lists the findings near the end). Commit: `b80b5b3253feeef304e44da04d84e9730489658b`.

The reduced examples below compile and execute successfully with both GCC and Clang 18 using `-std=c++20 -Wall -Wextra -Werror -pedantic`. I have not independently analyzed the reduced examples with a local CodeQL database; the recorded CodeQL observations come from the real project and its complete downloadable `codeql-c-cpp` artifact.

### Observed and expected behavior

`cpp/suspicious-pointer-scaling` reports six expressions of the form `*(reinterpret_cast<RawLink<T>*>(bytes) + linkIndex)`, where `bytes` is `std::byte*` and `linkIndex` is a record number. Scaling by `sizeof(RawLink<T>)` is required here. There is no extra multiplication by `sizeof` and no byte-offset parameter.

Original locations: https://github.com/linksplatform/Data.Doublets/blob/b80b5b3253feeef304e44da04d84e9730489658b/cpp/Platform.Data.Doublets/Memory/United/Generic/UnitedMemoryLinksBase.h#L50

For example, the diagnostic says the source type is `byte` (size 1), while arithmetic uses `RawLink<unsigned long>*` (size 64). The storage is a growable, aligned record buffer exposed through a byte pointer; intentionally casting to the correct record type before indexing agrees with the query-help recommendation.

### Reduced example

```cpp
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
```

Save as `repro.cpp`, create a C/C++ database tracing `c++ -std=c++20 -c repro.cpp`, and analyze `cpp-security-and-quality.qls`. Compare `At` with `AtUsingSubscript`; executing the program verifies they address the same second record. The real-project SARIF observation is linked above; the reduced standalone CodeQL run remains for maintainers to confirm.

### Source investigation / suggested correction

`IncorrectPointerScaling.ql` explicitly exempts source/destination `CharType` and `VoidType` because byte-buffer pointer casts are likely false positives. C++17 `std::byte` is an enum, so it misses those exemptions. Consider recognizing the actual standard-library `std::byte` type, preserving diagnostics for arbitrary small enums, and adding paired `char*`/`std::byte*` tests. Alternatively, improve classification of an explicit typed cast followed by record indexing.

Query help itself recommends casting before pointer arithmetic: https://codeql.github.com/codeql-query-help/cpp/cpp-suspicious-pointer-scaling/

### Workaround

Use `reinterpret_cast<Record*>(storage)[recordIndex]`. This follows the query's array-subscript recommendation, preserves address calculations and const-correctness, and avoids suppressing the security query. We applied this to all six matching expressions and run the full native suite and ASan/UBSan in CI.

### Verified workaround in the complete build

[Run 40842727244](https://github.com/linksplatform/Data.Doublets/actions/runs/40842727244) at `5cf40c5a338a38978221aab65a8e017005c95303` finished successfully on 2026-10-29 at 08:39:37 UTC. Its full traced native `security-and-quality` SARIF has **zero results** and a successful analysis invocation. GCC, Clang and ASan/UBSan each pass all **46 native tests**, the benchmark smoke and the real installed-package consumer. The workaround therefore passes both runtime and full-repository CodeQL checks without a query suppression. This verifies the production workaround; it does not claim a separate CodeQL run on the reduced standalone example.
