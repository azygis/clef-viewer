#!/bin/bash

# Main build script that coordinates frontend and backend builds
# Accepts version as environment variable: BUILD_VERSION

set -e

echo "🚀 Starting build process..."

if [[ -n "$BUILD_VERSION" ]]; then
    echo "📦 Build version: $BUILD_VERSION"
    export BUILD_VERSION="$BUILD_VERSION"
else
    echo "ℹ️  No version specified, using default versions from project files"
fi

echo "🔨 Building backend..."
./build-backend.sh

echo "🔨 Building frontend..."
./build-electron.sh

echo "✅ Build process completed!"
