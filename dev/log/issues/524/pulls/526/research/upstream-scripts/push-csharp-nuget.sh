#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

CSHARP_PACKAGE_VERSION=$(<CSHARP_PACKAGE_VERSION.txt)

# Ensure NuGet package does not exist
NuGetPackageUrl="https://globalcdn.nuget.org/packages/Platform.$REPOSITORY_NAME.$CSHARP_PACKAGE_VERSION.nupkg"
NuGetPackageUrl=$(echo "$NuGetPackageUrl" | tr '[:upper:]' '[:lower:]')

NuGetPageStatus="$(curl -Is "$NuGetPackageUrl" | head -1)"
StatusContents=( $NuGetPageStatus )
if [ "${StatusContents[1]}" == "200" ]; then
    echo "NuGet with current version is already pushed."
    exit 0
fi

echo "The NuGet package does not exist at $NuGetPackageUrl"

# Pack NuGet package
dotnet pack -c Release

# Push NuGet package
dotnet nuget push ./**/*.nupkg -s https://api.nuget.org/v3/index.json --skip-duplicate -k "${NUGETTOKEN}"

# Clean up
find . -type f -name '*.nupkg' -delete
find . -type f -name '*.snupkg' -delete
