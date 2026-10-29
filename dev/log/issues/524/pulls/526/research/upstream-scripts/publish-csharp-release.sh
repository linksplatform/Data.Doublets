#!/bin/bash
set -e # Exit with nonzero exit code if anything failss

SEPARATOR="

"

if [ ! -f "CSHARP_PACKAGE_VERSION.txt" ]
then
    echo "CSHARP_PACKAGE_VERSION.txt not found."
    exit 1
fi

PACKAGE_VERSION=$(<CSHARP_PACKAGE_VERSION.txt)
PACKAGE_RELEASE_NOTES=$(<CSHARP_PACKAGE_RELEASE_NOTES.txt)
PACKAGE_URL="https://www.nuget.org/packages/Platform.$REPOSITORY_NAME/$CSHARP_PACKAGE_VERSION"

TAG="csharp_$PACKAGE_VERSION"
RELEASE_NAME="[C#] $PACKAGE_VERSION"
RELEASE_BODY="$PACKAGE_URL$SEPARATOR$PACKAGE_RELEASE_NOTES"

./publish-release.sh "$TAG" "$RELEASE_NAME" "$RELEASE_BODY"
