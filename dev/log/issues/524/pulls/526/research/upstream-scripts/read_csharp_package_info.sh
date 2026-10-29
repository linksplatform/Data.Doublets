#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

sudo apt-get install xmlstarlet

CSHARP_PACKAGE_PROJECT_PATH="Platform.$REPOSITORY_NAME/Platform.$REPOSITORY_NAME.csproj"
echo "$CSHARP_PACKAGE_PROJECT_PATH"
echo "$CSHARP_PACKAGE_PROJECT_PATH" > CSHARP_PACKAGE_PROJECT_PATH.txt
CSHARP_PACKAGE_VERSION=$(xmlstarlet sel -t -m '//VersionPrefix[1]' -v . -n <"$CSHARP_PACKAGE_PROJECT_PATH")
echo "$CSHARP_PACKAGE_VERSION"
echo "$CSHARP_PACKAGE_VERSION" > CSHARP_PACKAGE_VERSION.txt
CSHARP_PACKAGE_RELEASE_NOTES=$(xmlstarlet sel -t -m '//PackageReleaseNotes[1]' -v . -n <"$CSHARP_PACKAGE_PROJECT_PATH")
echo "$CSHARP_PACKAGE_RELEASE_NOTES"
echo "$CSHARP_PACKAGE_RELEASE_NOTES" > CSHARP_PACKAGE_RELEASE_NOTES.txt
