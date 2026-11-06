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

# Current Conan Center recipes: GSL still needs its legacy metadata removed
# (conan-center-index#31362); cpuinfo's newest source recipe is not on the remote.
center_revision=dbfaf1c016d9ed37b2d536d479cf5f0499bb361a
for dependency in ms-gsl/5.0.1 cpuinfo/cci.20260612; do
    IFS=/ read -r name version <<< "$dependency"
    recipe_dir="$recipes_dir/modern/$name"
    mkdir -p "$recipe_dir"
    for file in conanfile.py conandata.yml; do
        curl --fail --silent --show-error --location \
            "https://raw.githubusercontent.com/conan-io/conan-center-index/$center_revision/recipes/$name/all/$file" -o "$recipe_dir/$file"
    done
    python3 - "$cpp_root/../scripts" "$recipe_dir/conanfile.py" <<'PYTHON'
from pathlib import Path
import sys
sys.path.insert(0, sys.argv[1])
from patch_conan_recipes import modern_metadata, package_id_metadata
recipe = Path(sys.argv[2])
recipe.write_text(modern_metadata(package_id_metadata(recipe.read_text(), recipe.parent.name)))
PYTHON
    conan export '-cc=core:warnings_as_errors=["*"]' "$recipe_dir" --version "$version"
done
