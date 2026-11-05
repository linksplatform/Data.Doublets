#include <benchmark/benchmark.h>
#include <tuple>
#include <Platform.Data.Doublets.h>
#include "CreateMillionPointsBenchmarks.h"
#include "CheckDefaultHandlerOrJustCallBenchmarks.h"

namespace
{
    // Retain registration handles; the benchmark library owns the benchmark objects.
    const auto benchmarks = std::tuple{Platform::Data::Doublets::Benchmarks::RegisterCreateMillionPointsBenchmarks(),
                                       Platform::Data::Doublets::Benchmarks::RegisterHandlerBenchmarks()};
} // namespace

BENCHMARK_MAIN();
