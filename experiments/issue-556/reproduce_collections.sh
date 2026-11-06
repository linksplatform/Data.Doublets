#!/usr/bin/env bash
# Reproduce Collections.Methods#114 without modifying the working tree.
set -euo pipefail
scratch=$(mktemp -d)
trap 'rm -rf "$scratch"' EXIT
dotnet new classlib --name Repro --framework net10.0 --output "$scratch" >/dev/null
dotnet add "$scratch/Repro.csproj" package Platform.Collections.Methods --version 1.0.0 >/dev/null
printf 'using Platform.Collections.Methods.Trees;\npublic class Repro {}\n' > "$scratch/Class1.cs"
dotnet build "$scratch/Repro.csproj" --no-restore -m:1
