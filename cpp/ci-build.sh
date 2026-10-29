#!/usr/bin/env bash
# Shared by CI, local tests, benchmarks and Gitpod. Verbose tracing is opt-in.
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
cpp_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
build_dir=${CPP_BUILD_DIR:-$cpp_root/build}
build_type=${CPP_BUILD_TYPE:-Release}
mode=${1:-test}
# The hashing dependency uses CRC32 intrinsics; use an explicit portable x86
# baseline rather than inheriting the build host's -march=native setting.
if [[ $(uname -m) == x86_64 ]]; then
    export CXXFLAGS="${CXXFLAGS:--march=x86-64-v2}"
    export CPPFLAGS="${CPPFLAGS:--march=x86-64-v2}"
fi
test_flag=ON
benchmark_flag=ON
case "$mode" in
    test) ;;
    benchmarks) ;;
    *) echo "Usage: $0 {test|benchmarks}" >&2; exit 2 ;;
esac
conan install "$cpp_root" --output-folder "$build_dir" --build=missing \
    -s "build_type=$build_type" -s compiler.cppstd=20 -c "tools.build:jobs=${CMAKE_BUILD_PARALLEL_LEVEL:-1}"
cmake -S "$cpp_root" -B "$build_dir" \
    -DCMAKE_TOOLCHAIN_FILE="$build_dir/conan_toolchain.cmake" \
    -DCMAKE_BUILD_TYPE="$build_type" -DLINKS_PLATFORM_TESTS="$test_flag" \
    -DLINKS_PLATFORM_BENCHMARKS="$benchmark_flag" \
    -DCMAKE_EXPORT_COMPILE_COMMANDS=ON -DLINKS_PLATFORM_COVERAGE="${CPP_COVERAGE:-OFF}"
cmake --build "$build_dir" --parallel "${CMAKE_BUILD_PARALLEL_LEVEL:-1}"
if [[ $mode == test ]]; then
    ctest --test-dir "$build_dir" --output-on-failure --no-tests=error --timeout 180
else
    "$build_dir/bin/Platform.Data.Doublets.Benchmarks" "${@:2}"
fi
