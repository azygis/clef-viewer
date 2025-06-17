#!/bin/bash

# CLEF Viewer Build Test Script
# Tests all build targets to ensure they work correctly

set -e

echo "🧪 Testing CLEF Viewer builds"

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
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

# Check if Docker is available
check_docker() {
    if ! command -v docker >/dev/null 2>&1; then
        echo "❌ Docker is required for building. Please install Docker and try again."
        exit 1
    fi

    if ! docker info >/dev/null 2>&1; then
        echo "❌ Docker is not running or not accessible. Please start Docker and try again."
        exit 1
    fi

    log_success "Docker is available and running"
}

# Check if we're in the right directory
if [[ ! -f "package.json" ]]; then
    echo "❌ package.json not found. Please run this script from the frontend directory."
    exit 1
fi

# Check Docker availability
check_docker

# Clean previous builds
log_info "Cleaning previous builds"
rm -rf dist out
log_success "Previous builds cleaned"

# Test basic build
log_info "Testing basic build"
yarn build
log_success "Basic build completed"

# Test full build using Docker
log_info "Testing full build using build.sh (Docker + Wine)"
cd ..
if [[ ! -f "build.sh" ]]; then
    log_warning "build.sh not found in parent directory"
else
    chmod +x build.sh
    ./build.sh
    cd frontend
    log_success "Full build completed using Docker"
fi

# List generated files
log_info "Generated files:"
if [[ -d "dist" ]]; then
    find dist -type f -name "*.exe" -o -name "*.AppImage" -o -name "*.deb" -o -name "*.tar.gz" -o -name "*.zip" | sort
else
    log_warning "No dist directory found"
fi

log_success "Build testing completed!"

echo ""
echo "📦 To create a full release:"
echo "   ./scripts/release.sh 1.0.0"
echo ""
echo "🚀 To test package managers:"
echo "   See frontend/packaging/README.md"
