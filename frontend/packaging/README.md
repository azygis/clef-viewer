# Package Manager Distribution Guide

This document explains how to distribute CLEF Viewer through various package managers.

## Prerequisites

- **Docker**: Required for cross-platform builds using Wine
- **Node.js**: For frontend dependencies and scripts
- **.NET SDK**: For backend compilation

## Automated Release Process

Use the automated release script:

```bash
cd frontend
./scripts/release.sh 1.0.0
```

This script will:

- Check Docker availability
- Update version in package.json
- Build all distributions using `build.sh` (Docker + Wine)
- Update package manager configurations
- Generate release notes

## Manual Distribution Steps

### 1. Chocolatey (Windows)

**Prerequisites:**

- Chocolatey account at https://community.chocolatey.org/
- API key from your account

**Steps:**

1. Update configurations using the release script
2. Build the package:
    ```bash
    cd frontend/packaging/chocolatey
    choco pack
    ```
3. Test locally:
    ```bash
    choco install clef-viewer -s .
    ```
4. Push to Chocolatey:
    ```bash
    choco push clef-viewer.1.0.0.nupkg --source https://push.chocolatey.org/ --api-key YOUR_API_KEY
    ```

### 2. Winget (Windows Package Manager)

**Prerequisites:**

- Fork of https://github.com/microsoft/winget-pkgs

**Steps:**

1. Update manifest files using the release script
2. Copy updated manifests to your fork:
    ```bash
    mkdir -p winget-pkgs/manifests/a/azygis/CLEFViewer/1.0.0/
    cp frontend/packaging/winget/* winget-pkgs/manifests/a/azygis/CLEFViewer/1.0.0/
    ```
3. Create pull request to microsoft/winget-pkgs

### 3. Scoop (Windows)

**Prerequisites:**

- Fork of a Scoop bucket (e.g., https://github.com/ScoopInstaller/Extras)

**Steps:**

1. Update manifest using the release script
2. Copy to your bucket fork:
    ```bash
    cp frontend/packaging/scoop/clef-viewer.json scoop-bucket/bucket/
    ```
3. Create pull request to the bucket repository

### 4. Snap Store (Linux)

**Prerequisites:**

- Ubuntu One account
- Snapcraft CLI installed

**Steps:**

1. Build and publish:
    ```bash
    cd frontend/packaging/snap
    snapcraft
    snapcraft upload --release=stable clef-viewer_1.0.0_amd64.snap
    ```

### 5. Flathub (Linux)

**Prerequisites:**

- Fork of https://github.com/flathub/flathub

**Steps:**

1. Update manifest using the release script
2. Copy to Flathub fork:
    ```bash
    cp frontend/packaging/flatpak/* flathub-fork/
    ```
3. Create pull request to flathub/flathub

### 6. AUR (Arch Linux)

**Prerequisites:**

- AUR account
- SSH key configured

**Steps:**

1. Clone AUR repository:
    ```bash
    git clone ssh://aur@aur.archlinux.org/clef-viewer.git
    ```
2. Update PKGBUILD:
    ```bash
    cp frontend/packaging/aur/PKGBUILD clef-viewer/
    cd clef-viewer
    makepkg --printsrcinfo > .SRCINFO
    ```
3. Commit and push:
    ```bash
    git add PKGBUILD .SRCINFO
    git commit -m "Update to 1.0.0"
    git push
    ```

## Distribution Checklist

Before each release:

- [ ] Update version in package.json
- [ ] Build all distributions
- [ ] Test installations on clean systems
- [ ] Update all package manager configurations
- [ ] Calculate and update checksums
- [ ] Create GitHub release with artifacts
- [ ] Submit to package managers
- [ ] Update documentation

## Portable Versions

### Windows Portable

- Built as `clef-viewer-1.0.0-portable.exe`
- No installation required
- Can be distributed via:
    - Direct download
    - Scoop
    - Portable app collections

### Linux Portable

- Built as `clef-viewer-1.0.0.tar.gz`
- Extract and run
- Can be distributed via:
    - Direct download
    - Custom repositories

## Testing Package Installations

### Windows

```bash
# Chocolatey
choco install clef-viewer

# Winget
winget install azygis.CLEFViewer

# Scoop
scoop install clef-viewer
```

### Linux

```bash
# Snap
snap install clef-viewer

# Flatpak
flatpak install com.azygis.clef-viewer

# AUR (Arch)
yay -S clef-viewer
```

## Troubleshooting

### Common Issues

1. **Hash Mismatches**: Ensure checksums are calculated correctly
2. **Missing Dependencies**: Update dependency lists in package configs
3. **Permission Issues**: Ensure proper permissions for package uploads
4. **Build Failures**: Check build logs and update build scripts

### Support Channels

- GitHub Issues: https://github.com/azygis/clef-viewer/issues
