#!/usr/bin/env bash
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
cpp_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)
recipes_dir=${CONAN_RECIPES_DIR:-$cpp_root/build/recipes}
# Reviewed Conan 2 recipes, not mutable main or deprecated Conan 1 profiles.
recipes_revision=435ad91f44d2fd40c4e4be366163f97333013d65
if [[ ! -d $recipes_dir/.git ]]; then
    git init "$recipes_dir"
    git -C "$recipes_dir" remote add origin https://github.com/linksplatform/conan-center-index.git
fi
git -C "$recipes_dir" fetch --depth 1 origin "$recipes_revision"
git -C "$recipes_dir" checkout --detach FETCH_HEAD
python3 "$cpp_root/../scripts/patch_conan_recipes.py" "$recipes_dir"
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.interfaces/all" --version 0.3.41
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.delegates/all" --version 0.3.7
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.exceptions/all" --version 0.3.2
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.converters/all" --version 0.2.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.hashing/all" --version 0.5.6
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.ranges/all" --version 0.2.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.random/all" --version 0.2.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.equality/all" --version 0.0.1
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.collections/all" --version 0.2.1
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.setters/all" --version 0.1.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.data/all" --version 0.1.1
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.numbers/all" --version 0.1.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.memory/all" --version 0.1.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/platform.collections.methods/all" --version 0.3.0

# Pinned upstream build/metadata repairs use the same reviewed recipe snapshot.
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/benchmark/all" --version 1.6.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/cpuinfo/all" --version cci.20220228
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/gtest/all" --version 1.13.0
conan export '-cc=core:warnings_as_errors=["*"]' "$recipes_dir/recipes/ms-gsl/all" --version 4.0.0
