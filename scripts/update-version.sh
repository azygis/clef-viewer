#!/bin/bash

# Version Update Script
# Updates version in both package.json and backend project file (for development)
#
# NOTE: For production builds, use BUILD_VERSION environment variable instead:
#   BUILD_VERSION=1.2.3 ./build.sh
#
# This script is primarily for development version updates and testing.

set -e

VERSION=${1}

if [[ -z "$VERSION" ]]; then
    echo "❌ Version is required"
    echo "Usage: $0 <version>"
    echo "Example: $0 1.2.3"
    echo ""
    echo "For production builds, use BUILD_VERSION environment variable:"
    echo "  BUILD_VERSION=1.2.3 ./build.sh"
    exit 1
fi

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

log_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

log_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

log_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

echo "🔄 Updating version to $VERSION"

# Check if we're in the right directory (root of project)
if [[ ! -f "build.sh" ]]; then
    echo "❌ build.sh not found. Please run this script from the project root directory."
    exit 1
fi

# Update package.json version
log_info "Updating package.json version to $VERSION"
cd frontend
if command -v yarn >/dev/null 2>&1; then
    yarn version --new-version $VERSION --no-git-tag-version
    log_success "Updated package.json to version $VERSION"
else
    log_warning "yarn not found, skipping package.json version update"
fi
cd ..

# Update backend version
log_info "Updating backend version to $VERSION"
if [[ -f "backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj" ]]; then
    # Update version in backend project file
    sed -i "s/<Version>.*<\/Version>/<Version>$VERSION<\/Version>/" backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj

    # If no Version tag exists, add it
    if ! grep -q "<Version>" backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj; then
        sed -i "s/<PropertyGroup>/<PropertyGroup>\n    <Version>$VERSION<\/Version>/" backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj
    fi
    log_success "Updated backend version to $VERSION"
else
    log_warning "Backend project file not found"
fi

# Show current versions
echo ""
echo "📋 Current versions:"
if [[ -f "frontend/package.json" ]]; then
    PACKAGE_VERSION=$(grep '"version"' frontend/package.json | sed 's/.*"version": "\([^"]*\)".*/\1/')
    echo "   Frontend: $PACKAGE_VERSION"
fi

if [[ -f "backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj" ]]; then
    BACKEND_VERSION=$(grep '<Version>' backend/ClefViewer/ClefViewer.API/ClefViewer.API.csproj | sed 's/.*<Version>\([^<]*\)<\/Version>.*/\1/' 2>/dev/null || echo "Not set")
    echo "   Backend:  $BACKEND_VERSION"
fi

echo ""
log_success "Version update completed!"
echo ""
echo "🚀 Next steps:"
echo "   1. Review changes: git diff"
echo "   2. Commit changes: git add . && git commit -m \"Bump version to $VERSION\""
echo "   3. Create tag: git tag v$VERSION"
echo "   4. Push: git push origin main --tags"
echo "   5. Create GitHub release with tag v$VERSION"
