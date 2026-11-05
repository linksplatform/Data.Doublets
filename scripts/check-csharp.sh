#!/usr/bin/env bash
set -euo pipefail
if [[ ${CI_VERBOSE:-0} == 1 ]]; then set -x; fi
repo_root=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)
cd "$repo_root"
dotnet restore csharp/Platform.Data.Doublets.sln --locked-mode
dotnet build csharp/Platform.Data.Doublets.sln -c Release --no-restore -m:1 -warnaserror
dotnet format whitespace csharp/Platform.Data.Doublets.sln --no-restore --verify-no-changes
dotnet test csharp/Platform.Data.Doublets.sln -c Release --no-build --no-restore \
    --blame-hang-timeout 3m --collect:"XPlat Code Coverage" --logger trx --results-directory artifacts/test-results
dotnet run --project examples/ExternalReferences -c Release
dotnet run --project examples/Deletion -c Release
dotnet run --project examples/Update -c Release
# --no-build ensures publication uses precisely the assembly that passed tests.
dotnet pack csharp/Platform.Data.Doublets -c Release --no-build --no-restore -o artifacts/packages -warnaserror
python3 scripts/smoke_packages.py csharp
