#!/bin/bash

# CLEF Viewer Release Automation Script
# This script automates the creation of packages for various package managers

set -e

VERSION=${1:-"1.0.0"}
DIST_DIR="dist"
PACKAGING_DIR="packaging"

echo "🚀 Starting release automation for version $VERSION"

# Colors for output
RED='\033[0;31m'
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

log_error() {
    echo -e "${RED}❌ $1${NC}"
}

# Function to calculate SHA256 hash
calculate_sha256() {
    if command -v sha256sum >/dev/null 2>&1; then
        sha256sum "$1" | cut -d' ' -f1
    elif command -v shasum >/dev/null 2>&1; then
        shasum -a 256 "$1" | cut -d' ' -f1
    else
        log_error "Neither sha256sum nor shasum found"
        exit 1
    fi
}

# Update version in package.json
update_version() {
    log_info "Updating version to $VERSION in package.json"
    if command -v jq >/dev/null 2>&1; then
        tmp=$(mktemp)
        jq ".version = \"$VERSION\"" package.json >"$tmp" && mv "$tmp" package.json
        log_success "Version updated in package.json"
    else
        log_warning "jq not found, please update version manually in package.json"
    fi
}

# Check if Docker is available
check_docker() {
    if ! command -v docker >/dev/null 2>&1; then
        log_error "Docker is required for building. Please install Docker and try again."
        exit 1
    fi

    if ! docker info >/dev/null 2>&1; then
        log_error "Docker is not running or not accessible. Please start Docker and try again."
        exit 1
    fi

    log_success "Docker is available and running"
}

# Build all distributions using the main build script
build_distributions() {
    log_info "Building all distributions using build.sh (Docker + Wine)"

    # Change to the root directory where build.sh is located
    cd ..

    if [[ ! -f "build.sh" ]]; then
        log_error "build.sh not found in parent directory. Please run this script from the frontend directory."
        exit 1
    fi

    # Make sure build.sh is executable
    chmod +x build.sh

    # Run the main build script
    ./build.sh

    # Return to frontend directory
    cd frontend

    log_success "All distributions built using Docker"
}

# Update Chocolatey package
update_chocolatey() {
    log_info "Updating Chocolatey package configuration"

    local setup_file="$DIST_DIR/clef-viewer-$VERSION-setup.exe"
    if [[ -f "$setup_file" ]]; then
        local checksum=$(calculate_sha256 "$setup_file")

        # Update nuspec version
        sed -i "s/<version>.*<\/version>/<version>$VERSION<\/version>/" "$PACKAGING_DIR/chocolatey/clef-viewer.nuspec"

        # Update install script
        sed -i "s/__VERSION__/$VERSION/g" "$PACKAGING_DIR/chocolatey/tools/chocolateyinstall.ps1"
        sed -i "s/__CHECKSUM__/$checksum/g" "$PACKAGING_DIR/chocolatey/tools/chocolateyinstall.ps1"

        log_success "Chocolatey package updated"
    else
        log_warning "Windows setup file not found: $setup_file"
    fi
}

# Update Winget manifests
update_winget() {
    log_info "Updating Winget manifests"

    local setup_file="$DIST_DIR/clef-viewer-$VERSION-setup.exe"
    if [[ -f "$setup_file" ]]; then
        local checksum=$(calculate_sha256 "$setup_file")

        # Update version manifest
        sed -i "s/PackageVersion: .*/PackageVersion: $VERSION/" "$PACKAGING_DIR/winget/azygis.CLEFViewer.yaml"

        # Update locale manifest
        sed -i "s/PackageVersion: .*/PackageVersion: $VERSION/" "$PACKAGING_DIR/winget/azygis.CLEFViewer.locale.en-US.yaml"

        # Update installer manifest
        sed -i "s/PackageVersion: .*/PackageVersion: $VERSION/" "$PACKAGING_DIR/winget/azygis.CLEFViewer.installer.yaml"
        sed -i "s|InstallerUrl: .*|InstallerUrl: https://github.com/azygis/clef-viewer/releases/download/v$VERSION/clef-viewer-$VERSION-setup.exe|" "$PACKAGING_DIR/winget/azygis.CLEFViewer.installer.yaml"
        sed -i "s/__INSTALLER_SHA256__/$checksum/" "$PACKAGING_DIR/winget/azygis.CLEFViewer.installer.yaml"

        log_success "Winget manifests updated"
    else
        log_warning "Windows setup file not found: $setup_file"
    fi
}

# Update Scoop manifest
update_scoop() {
    log_info "Updating Scoop manifest"

    local portable_file="$DIST_DIR/clef-viewer-$VERSION-portable.exe"
    if [[ -f "$portable_file" ]]; then
        local checksum=$(calculate_sha256 "$portable_file")

        if command -v jq >/dev/null 2>&1; then
            tmp=$(mktemp)
            jq ".version = \"$VERSION\" | .url = \"https://github.com/azygis/clef-viewer/releases/download/v$VERSION/clef-viewer-$VERSION-portable.exe\" | .hash = \"$checksum\"" "$PACKAGING_DIR/scoop/clef-viewer.json" >"$tmp" && mv "$tmp" "$PACKAGING_DIR/scoop/clef-viewer.json"
            log_success "Scoop manifest updated"
        else
            log_warning "jq not found, please update Scoop manifest manually"
        fi
    else
        log_warning "Windows portable file not found: $portable_file"
    fi
}

# Update Snap configuration
update_snap() {
    log_info "Updating Snap configuration"
    sed -i "s/version: .*/version: '$VERSION'/" "$PACKAGING_DIR/snap/snapcraft.yaml"
    sed -i "s|source: https://github.com/azygis/clef-viewer/releases/download/v.*/clef-viewer-.*.AppImage|source: https://github.com/azygis/clef-viewer/releases/download/v$VERSION/clef-viewer-$VERSION.AppImage|" "$PACKAGING_DIR/snap/snapcraft.yaml"
    log_success "Snap configuration updated"
}

# Update AUR PKGBUILD
update_aur() {
    log_info "Updating AUR PKGBUILD"

    local appimage_file="$DIST_DIR/clef-viewer-$VERSION.AppImage"
    if [[ -f "$appimage_file" ]]; then
        local checksum=$(calculate_sha256 "$appimage_file")

        sed -i "s/pkgver=.*/pkgver=$VERSION/" "$PACKAGING_DIR/aur/PKGBUILD"
        sed -i "s/sha256sums=.*/sha256sums=('$checksum')/" "$PACKAGING_DIR/aur/PKGBUILD"

        log_success "AUR PKGBUILD updated"
    else
        log_warning "AppImage file not found: $appimage_file"
    fi
}

# Update Flatpak manifest
update_flatpak() {
    log_info "Updating Flatpak manifest"

    local appimage_file="$DIST_DIR/clef-viewer-$VERSION.AppImage"
    if [[ -f "$appimage_file" ]]; then
        local checksum=$(calculate_sha256 "$appimage_file")

        sed -i "s|url: https://github.com/azygis/clef-viewer/releases/download/v.*/clef-viewer-.*.AppImage|url: https://github.com/azygis/clef-viewer/releases/download/v$VERSION/clef-viewer-$VERSION.AppImage|" "$PACKAGING_DIR/flatpak/com.azygis.clef-viewer.yml"
        sed -i "s/__APPIMAGE_SHA256__/$checksum/" "$PACKAGING_DIR/flatpak/com.azygis.clef-viewer.yml"

        log_success "Flatpak manifest updated"
    else
        log_warning "AppImage file not found: $appimage_file"
    fi
}

# Create release notes
create_release_notes() {
    log_info "Creating release notes"

    cat >"RELEASE_NOTES_$VERSION.md" <<EOF
# CLEF Viewer v$VERSION

## Installation

### Windows
- **Installer**: Download \`clef-viewer-$VERSION-setup.exe\`
- **Portable**: Download \`clef-viewer-$VERSION-portable.exe\`
- **Chocolatey**: \`choco install clef-viewer\`
- **Winget**: \`winget install azygis.CLEFViewer\`
- **Scoop**: \`scoop install clef-viewer\`

### Linux
- **AppImage**: Download \`clef-viewer-$VERSION.AppImage\`
- **Debian/Ubuntu**: Download \`clef-viewer_$VERSION_amd64.deb\`
- **Portable**: Extract \`clef-viewer-$VERSION.tar.gz\`
- **Snap**: \`snap install clef-viewer\`
- **Flatpak**: \`flatpak install com.azygis.clef-viewer\`
- **Arch Linux (AUR)**: \`yay -S clef-viewer\`

## Changes
- Initial release
- CLEF log file viewing and analysis
- Filtering and search capabilities
- Cross-platform support (Windows, Linux)

EOF

    log_success "Release notes created: RELEASE_NOTES_$VERSION.md"
}

# Main execution
main() {
    log_info "Starting CLEF Viewer release automation"

    # Check if we're in the right directory
    if [[ ! -f "package.json" ]]; then
        log_error "package.json not found. Please run this script from the frontend directory."
        exit 1
    fi

    # Check Docker availability
    check_docker

    # Update version
    update_version

    # Build distributions
    build_distributions

    # Update package manager configurations
    update_chocolatey
    update_winget
    update_scoop
    update_snap
    update_aur
    update_flatpak

    # Create release notes
    create_release_notes

    log_success "Release automation completed for version $VERSION"
    log_info "Next steps:"
    echo "  1. Review and commit the updated package configurations"
    echo "  2. Create a Git tag: git tag v$VERSION"
    echo "  3. Push changes and tag: git push origin main --tags"
    echo "  4. Create GitHub release with the built artifacts"
    echo "  5. Submit packages to respective repositories"
}

# Run main function
main "$@"
