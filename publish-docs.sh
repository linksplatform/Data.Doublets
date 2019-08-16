#!/bin/bash
set -e # Exit with nonzero exit code if anything fails

# Pull requests and commits to other branches shouldn't try to deploy, just build to verify
if [[ ( "$TRAVIS_PULL_REQUEST" != "false" ) || ( "$TRAVIS_BRANCH" != "$SOURCE_BRANCH" ) ]]; then
    echo "Skipping documentation deploy."
    exit 0
fi

# Settings
TARGET_BRANCH="gh-pages"
SHA=$(git rev-parse --verify HEAD)
COMMIT_USER_NAME="linksplatform-docs"
COMMIT_USER_EMAIL="konard@yandex.ru"
REPOSITORY="github.com/linksplatform/${TRAVIS_REPO_NAME}"

# Insert repository name into DocFX's configuration files
sed -i "s/\$TRAVIS_REPO_NAME/${TRAVIS_REPO_NAME}/g" toc.yml
sed -i "s/\$TRAVIS_REPO_NAME/${TRAVIS_REPO_NAME}/g" docfx.json

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
git config user.name "$COMMIT_USER_NAME"
git config user.email "$COMMIT_USER_EMAIL"
git remote rm origin
git remote add origin https://$COMMIT_USER_NAME:$TOKEN@$REPOSITORY.git

# Commit the "changes", i.e. the new version.
# The delta will show diffs between new and old versions.
git add --all
git commit -m "Deploy to GitHub Pages: ${SHA}"

# Now that we're all set up, we can push.
git push https://$COMMIT_USER_NAME:$TOKEN@$REPOSITORY.git $TARGET_BRANCH
