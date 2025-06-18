#!/bin/bash

# Frontend build script using Docker
# Accepts version as environment variable: BUILD_VERSION

set -e

echo "🔧 Building frontend with Docker..."

if [[ -n "$BUILD_VERSION" ]]; then
    echo "📦 Building frontend with version: $BUILD_VERSION"
    export BUILD_VERSION="$BUILD_VERSION"
else
    echo "ℹ️  Building frontend with package.json version"
fi

# Filter out problematic environment variables that could trigger publishing
# Exclude GitHub-related variables and publishing tokens
docker run --rm \
    --env-file <(env | grep -iE 'DEBUG|NODE_|ELECTRON_|YARN_|NPM_|CI|CIRCLE|TRAVIS_TAG|TRAVIS|TRAVIS_REPO_|TRAVIS_BUILD_|TRAVIS_BRANCH|TRAVIS_PULL_REQUEST_|APPVEYOR_|CSC_|BT_|AWS_|STRIP|BUILD_' | grep -v -E 'GITHUB_|GH_|SNAP_|SNAPCRAFT_') \
    --env BUILD_VERSION="${BUILD_VERSION}" \
    --env ELECTRON_CACHE="/root/.cache/electron" \
    --env ELECTRON_BUILDER_CACHE="/root/.cache/electron-builder" \
    --env DISABLE_PUBLISH="true" \
    -v ${PWD}/publish:/publish \
    -v ${PWD}/frontend:/project \
    -v ~/.cache/electron:/root/.cache/electron \
    -v ~/.cache/electron-builder:/root/.cache/electron-builder \
    electronuserland/builder:wine \
    /bin/bash -c './build-with-version.sh'

echo "✅ Frontend build completed!"
