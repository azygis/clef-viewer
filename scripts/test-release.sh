#!/bin/bash

# Test Release Build Script
# This script simulates the GitHub Actions release workflow locally

set -e

VERSION=${1:-"1.0.0"}

echo "🚀 Testing Release Build Workflow for version $VERSION"

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

# Check if we're in the right directory (root of project)
if [[ ! -f "build.sh" ]]; then
    echo "❌ build.sh not found. Please run this script from the project root directory."
    exit 1
fi

# Check Docker
if ! command -v docker >/dev/null 2>&1; then
    echo "❌ Docker is required. Please install Docker and try again."
    exit 1
fi

if ! docker info >/dev/null 2>&1; then
    echo "❌ Docker is not running. Please start Docker and try again."
    exit 1
fi

log_success "Docker is available and running"

log_info "Testing build with version $VERSION"

# Install frontend dependencies first
log_info "Installing frontend dependencies"
cd frontend
yarn install --frozen-lockfile
cd ..
log_success "Dependencies installed"

# Build using environment variable (no file modifications)
log_info "Building application using build.sh with BUILD_VERSION=$VERSION"
export BUILD_VERSION="$VERSION"
chmod +x build.sh
./build.sh
chmod +x build.sh
./build.sh
log_success "Application built successfully"

# Generate checksums
log_info "Generating checksums"
cd frontend/dist
sha256sum *.exe *.AppImage *.deb *.tar.gz *.zip >checksums.txt 2>/dev/null || true
log_success "Checksums generated"

# List built files
echo ""
echo "📦 Built files ready for release:"
ls -la *.exe *.AppImage *.deb *.rpm *.tar.gz *.zip *.snap checksums.txt 2>/dev/null || echo "No files found"

echo ""
log_success "Release build test completed for version $VERSION!"
echo ""
echo "🚀 To create an actual release:"
echo "   1. Create a new release on GitHub with tag v$VERSION"
echo "   2. The workflow will automatically build and attach these files"
echo ""
echo "📁 Files are located in: frontend/dist/"
echo ""
echo "💡 Usage: $0 [version]"
echo "   Example: $0 1.2.3"
