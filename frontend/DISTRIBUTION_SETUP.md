# CLEF Viewer - Complete Package Manager Setup

## ℹ️ This is an upcoming change, not supported in initial version.

## 🎯 Summary

CLEF Viewer supports distribution through **all major package managers** with **portable versions** for both Windows and Linux platforms.

## 📦 Supported Distribution Methods

### Windows

- **NSIS Installer** (`clef-viewer-1.0.0-setup.exe`)
- **Portable Executable** (`clef-viewer-1.0.0-portable.exe`)
- **ZIP Archive** (`clef-viewer-1.0.0-win.zip`)
- **Chocolatey** (`choco install clef-viewer`)
- **Winget** (`winget install azygis.CLEFViewer`)
- **Scoop** (`scoop install clef-viewer`)

### Linux

- **AppImage** (`clef-viewer-1.0.0.AppImage`)
- **Debian Package** (`clef-viewer_1.0.0_amd64.deb`)
- **Portable Archive** (`clef-viewer-1.0.0.tar.gz`)
- **Snap Store** (`snap install clef-viewer`)
- **Flatpak** (`flatpak install com.azygis.clef-viewer`)
- **AUR (Arch Linux)** (`yay -S clef-viewer`)

## 🚀 Quick Start

### Prerequisites

- **Docker**: Required for cross-platform builds
- **Node.js**: For frontend dependencies
- **.NET SDK**: For backend builds

### Build All Distributions

```bash
# From the root directory
./build.sh
```

### Create Release

```bash
cd frontend
./scripts/release.sh 1.0.0
```

### Test Builds

```bash
cd frontend
./scripts/test-builds.sh
```

## 📁 File Structure

```
frontend/
├── packaging/                     # Package manager configurations
│   ├── chocolatey/               # Windows Chocolatey
│   ├── winget/                   # Windows Package Manager
│   ├── scoop/                    # Windows Scoop
│   ├── snap/                     # Linux Snap
│   ├── flatpak/                  # Linux Flatpak
│   ├── aur/                      # Arch Linux AUR
│   └── README.md                 # Distribution guide
├── scripts/
│   ├── release.sh                # Automated release script
│   └── test-builds.sh           # Build testing script
├── .github/workflows/
│   └── release.yml               # GitHub Actions automation
└── electron-builder.yml         # Updated build configuration
```

## 🔧 Configuration Files Created

### Package Managers

- ✅ **Chocolatey**: `clef-viewer.nuspec` + PowerShell scripts
- ✅ **Winget**: Three YAML manifests (version, locale, installer)
- ✅ **Scoop**: JSON manifest with auto-update
- ✅ **Snap**: `snapcraft.yaml` + desktop file
- ✅ **Flatpak**: Application manifest
- ✅ **AUR**: `PKGBUILD` for Arch Linux

### Automation

- ✅ **Release Script**: Updates all configurations automatically
- ✅ **GitHub Actions**: CI/CD for automated releases
- ✅ **Test Script**: Validates all build targets

### Build Targets

- ✅ **Windows**: Installer, Portable, ZIP
- ✅ **Linux**: AppImage, DEB, TAR.GZ, Snap
- ✅ **Cross-platform**: Docker-based builds

## 🎯 Next Steps

1. **Test the setup**:

    ```bash
    cd frontend
    ./scripts/test-builds.sh
    ```

2. **Create first release**:

    ```bash
    cd frontend
    ./scripts/release.sh 1.0.0
    git add .
    git commit -m "Add package manager support"
    git tag v1.0.0
    git push origin main --tags
    ```

3. **Submit to package managers**:

    - Follow the guides in `frontend/packaging/README.md`
    - Submit PRs to respective repositories
    - Wait for approval and publication

4. **Automation**:
    - GitHub Actions will handle future releases automatically
    - Just create tags and the workflow will build and distribute

## 🔍 Key Benefits

- **Portable Support**: Users can run without installation
- **Automated Process**: One script updates all package managers
- **Professional Distribution**: Support for all major package managers
- **CI/CD Ready**: GitHub Actions for automated releases
- **User Choice**: Multiple installation methods per platform

## 📝 Notes

- All package manager configurations use placeholders for hashes/versions
- The release script automatically calculates checksums and updates configs
- GitHub Actions workflow handles the complete release pipeline
- Portable versions require no installation and avoid SmartScreen issues
- Documentation provides step-by-step distribution guide

Your CLEF Viewer is now ready for professional distribution across all major platforms! 🎉
