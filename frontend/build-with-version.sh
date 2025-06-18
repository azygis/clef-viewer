#!/bin/bash

# Frontend build script with version support
# This script is run inside the Docker container

set -e

echo "🔧 Starting frontend build process..."

# Fix app-builder permissions (common Docker issue)
if [ -f "./node_modules/app-builder-bin/linux/x64/app-builder" ]; then
    chmod +x ./node_modules/app-builder-bin/linux/x64/app-builder
fi

# Ensure no publishing happens by unsetting any publish-related environment variables
unset GITHUB_TOKEN GH_TOKEN SNAP_TOKEN SNAPCRAFT_STORE_CREDENTIALS CSC_LINK CSC_KEY_PASSWORD

# Check if BUILD_VERSION is set
if [[ -n "$BUILD_VERSION" ]]; then
    echo "📦 Using version from environment: $BUILD_VERSION"

    # Temporarily update package.json version for this build
    echo "📝 Temporarily updating package.json version..."
    yarn version --new-version "$BUILD_VERSION" --no-git-tag-version

    # Build the application
    echo "🔨 Building application..."
    yarn build

    # Run electron-builder with explicit no publishing
    echo "📦 Running electron-builder..."
    yarn electron-builder --linux --win --publish=never

    echo "✅ Frontend build completed with version $BUILD_VERSION"
else
    echo "ℹ️  No BUILD_VERSION set, using package.json version"

    # Build the application normally
    echo "🔨 Building application..."
    yarn build

    # Run electron-builder with explicit no publishing
    echo "📦 Running electron-builder..."
    yarn electron-builder --linux --win --publish=never

    echo "✅ Frontend build completed"
fi
