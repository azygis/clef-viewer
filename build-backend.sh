#!/bin/bash

# Backend build script
# Accepts version as environment variable: BUILD_VERSION

set -e

# Validate and set version properties for dotnet build
validate_dotnet_version() {
    local version="$1"
    # .NET version format: Major.Minor.Patch[.Build] where each part is 0-65534
    if [[ ! $version =~ ^[0-9]+\.[0-9]+\.[0-9]+(\.[0-9]+)?$ ]]; then
        echo "❌ Invalid version format for .NET: $version"
        echo "   .NET requires format: Major.Minor.Patch[.Build] (e.g., 1.0.0 or 1.2.3.4)"
        return 1
    fi
    return 0
}

VERSION_PROPS=""
if [[ -n "$BUILD_VERSION" ]]; then
    if validate_dotnet_version "$BUILD_VERSION"; then
        echo "📦 Building backend with version: $BUILD_VERSION"
        VERSION_PROPS="-p:Version=$BUILD_VERSION -p:AssemblyVersion=$BUILD_VERSION -p:FileVersion=$BUILD_VERSION"
    else
        echo "⚠️  Using fallback version: 0.0.1"
        VERSION_PROPS="-p:Version=0.0.1 -p:AssemblyVersion=0.0.1 -p:FileVersion=0.0.1"
    fi
else
    echo "ℹ️  Building backend with project file version"
fi

echo "🔧 Restoring backend dependencies..."
dotnet restore ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj

echo "📦 Publishing backend for Linux x64..."
dotnet publish ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj \
    --no-restore \
    --runtime linux-x64 \
    --output ./publish/linux \
    --self-contained \
    -p:PublishTrimmed=true \
    $VERSION_PROPS

echo "📦 Publishing backend for Windows x64..."
dotnet publish ./backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj \
    --no-restore \
    --runtime win-x64 \
    --output ./publish/win \
    --self-contained \
    -p:PublishTrimmed=true \
    $VERSION_PROPS

echo "✅ Backend build completed!"
