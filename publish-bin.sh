#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

# Pull requests and commits to other branches shouldn't try to deploy, just build to verify


# Settings
TARGET_BRANCH="binaries"

SHA=$(git rev-parse --verify HEAD)
COMMIT_USER_NAME="linksplatform"
COMMIT_USER_EMAIL="linksplatformtechnologies@gmail.com"
REPOSITORY="github.com/linksplatform/${TRAVIS_REPO_NAME}"

# Clone the existing gh-pages for this repo into out/
# Create a new empty branch if gh-pages doesn't exist yet (should only happen on first deply)
git clone https://$REPOSITORY out
cd out || exit
git checkout $TARGET_BRANCH || git checkout --orphan $TARGET_BRANCH
cd ..

# Clean out existing contents
rm -rf out/${TARGET_PLATFORM}/**/* || exit 0

mkdir -p out/${TARGET_PLATFORM}/native

# Copy compiled binaries
cp Platform.${TRAVIS_REPO_NAME}/Platform.${TRAVIS_REPO_NAME}/${LIB_PATH}/${LIB_NAME} out/${TARGET_PLATFORM}/native/${TARGET_LIB_NAME}

cd out

# Now let's go have some fun with the cloned repo
git config user.name "$COMMIT_USER_NAME"
git config user.email "$COMMIT_USER_EMAIL"
git remote rm origin
git remote add origin "https://linksplatform:$GITHUB_TOKEN@$REPOSITORY.git"

# Commit the "changes", i.e. the new version.
# The delta will show diffs between new and old versions.
git add --all
git commit -m "Deploy to ${TARGET_BRANCH} branch for: ${SHA}" || exit 0

# Now that we're all set up, we can push.
git push "https://linksplatform:$GITHUB_TOKEN@$REPOSITORY.git" $TARGET_BRANCH || true
cd ..
