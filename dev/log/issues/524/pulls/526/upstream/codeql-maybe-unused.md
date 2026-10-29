Observed in a real C++20 Clang build with CodeQL CLI **2.27.3**, `security-and-quality`, on Ubuntu, rather than a no-build database. Public run: https://github.com/linksplatform/Data.Doublets/actions/runs/40836388311/job/123322406479 (the fatal SARIF gate lists the findings near the end). Commit: `b80b5b3253feeef304e44da04d84e9730489658b`.

The reduced examples below compile and execute successfully with both GCC and Clang 18 using `-std=c++20 -Wall -Wextra -Werror -pedantic`. I have not independently analyzed the reduced examples with a local CodeQL database; the recorded CodeQL observations come from the real project and its complete downloadable `codeql-c-cpp` artifact.

### Observed and expected behavior

`cpp/unused-local-variable` reports six Google Benchmark range-for iteration variables explicitly annotated `[[maybe_unused]]`. `cpp/unused-static-variable` also reports six `BENCHMARK` macro registration pointers: their initializers register benchmarks as a required side effect, and the macro marks them `[[maybe_unused]]`. These diagnostics persist with the full dependency-aware build.

Original example: https://github.com/linksplatform/Data.Doublets/blob/b80b5b3253feeef304e44da04d84e9730489658b/cpp/Platform.Data.Doublets.Benchmarks/CreateMillionPointsBenchmarks.h

Expected: standard C++17 `[[maybe_unused]]` should express intentional non-use just as GNU `unused` does. Separately, a scalar registration pointer initialized by a side-effecting call need not be read for its declaration to matter.

### Reduced example

```cpp
// Reproduction for CodeQL cpp/unused-{local,static}-variable.
// Compile separately; the annotations and registration side effect are intentional.
#include <array>

int registrations = 0;

int* Register()
{
    ++registrations;
    return &registrations;
}

[[maybe_unused]] static int* registration = Register();

void Iterate()
{
    for ([[maybe_unused]] auto value : std::array{1, 2})
    {
    }
}

int main()
{
    Iterate();
    return registrations == 1 ? 0 : 1;
}
```

To reproduce, save as `repro.cpp`, create a C/C++ CodeQL database tracing `c++ -std=c++20 -c repro.cpp`, and analyze it with `cpp-security-and-quality.qls`. Compare the unused queries' output with the real run above. Running the compiled program also verifies that registration occurred once.

### Query-source investigation / suggested correction

`cpp/ql/src/Best Practices/Unused Entities/UnusedLocals.ql` excludes attributes whose name is `unused`; `UnusedStaticVariables.ql` excludes `used` and `unused`. Neither explicitly checks `maybe_unused`. Please check how the extractor represents the standard annotation and add the appropriate exemption and C++17 regression cases to both queries. The static-variable side-effect predicate currently checks class constructors/destructors only; consider initializer expressions with observable side effects as well, or report their unnecessary binding separately rather than suggesting the declaration is useless.

### Workaround

Explicitly consume the loop token with `(void)iteration;`. Register benchmarks through the supported `benchmark::RegisterBenchmark` API, with the same names, arguments, and registration order, retaining registration handles in a static tuple. Our six-name before/after comparison and benchmark smoke run verify preservation. This avoids disabling either query or weakening the findings gate.

### Verified workaround in the complete build

[Run 40842727244](https://github.com/linksplatform/Data.Doublets/actions/runs/40842727244) at `5cf40c5a338a38978221aab65a8e017005c95303` finished successfully on 2026-10-29 at 08:39:37 UTC. Its full traced native `security-and-quality` SARIF has **zero results** and a successful analysis invocation. GCC, Clang and ASan/UBSan each pass all **46 native tests**, the benchmark smoke and the real installed-package consumer. The workaround therefore passes both runtime and full-repository CodeQL checks without a query suppression. This verifies the production workaround; it does not claim a separate CodeQL run on the reduced standalone example.
