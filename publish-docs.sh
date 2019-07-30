#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

SOURCE_BRANCH="master"
TARGET_BRANCH="gh-pages"

# Pull requests and commits to other branches shouldn't try to deploy, just build to verify
if [ "$TRAVIS_PULL_REQUEST" != "false" -o "$TRAVIS_BRANCH" != "$SOURCE_BRANCH" ]; then
    echo "Skipping deploy."
    exit 0
fi

# Save some useful information
SHA=`git rev-parse --verify HEAD`
COMMIT_AUTHOR_EMAIL="konard@me.com"
REPOSITORY="github.com/linksplatform/Data.Doublets"

# DocFX installation
nuget install docfx.console
mono $(ls | grep "docfx.console.")/tools/docfx.exe docfx.json

# Clone the existing gh-pages for this repo into out/
# Create a new empty branch if gh-pages doesn't exist yet (should only happen on first deply)
git clone https://$REPOSITORY out
cd out
git checkout $TARGET_BRANCH || git checkout --orphan $TARGET_BRANCH
cd ..

# Clean out existing contents
rm -rf out/**/* || exit 0

# Copy genereted docs site
cp -r _site/* out

cd out

# Do not use index.md
cp README.html index.html

# Now let's go have some fun with the cloned repo
git config user.name "Travis CI"
git config user.email "$COMMIT_AUTHOR_EMAIL"
git remote rm origin
git remote add origin https://linksplatform-docs:$TOKEN@$REPOSITORY.git

# Commit the "changes", i.e. the new version.
# The delta will show diffs between new and old versions.
git add --all
git commit -m "Deploy to GitHub Pages: ${SHA}"

# Now that we're all set up, we can push.
git push https://linksplatform-docs:$TOKEN@$REPOSITORY.git $TARGET_BRANCH
