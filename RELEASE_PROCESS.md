# CLEF Viewer - Release Process

## 🚀 Automated GitHub Release

This repository is configured with a simple, automated release process that builds and attaches all distribution files to GitHub releases.

## 📋 How It Works

### 1. **Trigger**: Create a GitHub Release

- Go to GitHub → Releases → "Create a new release"
- Choose or create a tag (e.g., `v1.0.0`)
- Add release title and description
- Click "Publish release"

### 2. **Automatic Build**: GitHub Actions Workflow

- **Workflow file**: `.github/workflows/build-release.yml`
- **Runner**: `ubuntu-latest` with Docker support
- **Build process**: Uses `build.sh` script (Docker + Wine)
- **Duration**: ~5-10 minutes

### 3. **Automatic Upload**: Built Files Attached

The workflow automatically builds and uploads:

- **Windows**: `clef-viewer-X.X.X-setup.exe`, `clef-viewer-X.X.X-portable.exe`, `*.zip`
- **Linux**: `clef-viewer-X.X.X.AppImage`, `clef-viewer_X.X.X_amd64.deb`, `clef-viewer-X.X.X.x86_64.rpm`, `*.tar.gz`
- **Checksums**: `checksums.txt` with SHA256 hashes

## 🧪 Test Release Build Locally

Before creating a release, you can test the build process:

```bash
# From project root
./scripts/test-release.sh
```

This script:

- ✅ Checks Docker availability
- ✅ Installs dependencies
- ✅ Runs the full build process
- ✅ Generates checksums
- ✅ Lists all files that would be uploaded

## 📁 Build Outputs

After a successful release, users can download:

### Windows Users

```bash
# Installer (recommended)
clef-viewer-1.0.0-setup.exe

# Portable (no installation)
clef-viewer-1.0.0-portable.exe

# Archive
Compact Log Event Format Viewer-1.0.0-win.zip
```

### Linux Users

```bash
# AppImage (recommended)
clef-viewer-1.0.0.AppImage

# Debian/Ubuntu package
clef-viewer_1.0.0_amd64.deb

# RPM package (Fedora/RHEL/CentOS)
clef-viewer-1.0.0.x86_64.rpm

# Portable archive
clef-viewer-1.0.0.tar.gz
```

## 🔧 Workflow Details

### GitHub Actions Workflow (`.github/workflows/build-release.yml`)

```yaml
Trigger: release.created
Runner: ubuntu-latest
Steps: 1. Checkout code
  2. Setup Node.js 18
  3. Setup .NET 8.0
  4. Install frontend dependencies
  5. Run build.sh (Docker + Wine)
  6. Generate checksums
  7. Upload all files to release
```

### Build Process (`build.sh`)

1. **Backend**: .NET builds for Linux and Windows
2. **Frontend**: Electron builds using Docker + Wine
3. **Packaging**: Creates all distribution formats
4. **Output**: Files in `frontend/dist/`

## ⚡ Quick Release Checklist

1. **Test locally**: `./scripts/test-release.sh`
2. **Update version**: Update `package.json` if needed
3. **Create release**: GitHub → Releases → New release
4. **Wait**: ~5-10 minutes for automated build
5. **Verify**: Check release has all attached files
6. **Done**: Users can download from GitHub releases

## 🐛 Troubleshooting

### Build Fails

- Check GitHub Actions logs in the "Actions" tab
- Common issues: Docker not available, dependency problems
- Test locally first with `./scripts/test-release.sh`

### Missing Files

- Check if `build.sh` completed successfully
- Verify file names match expected patterns in workflow
- Check `frontend/dist/` directory exists

### Upload Fails

- Verify `GITHUB_TOKEN` permissions (should be automatic)
- Check release was created properly
- Ensure workflow has write permissions

## 📝 Notes

- **No signing**: Files are unsigned (no certificate costs)
- **Cross-platform**: Windows builds on Linux using Wine
- **Automatic**: Zero manual steps after creating release
- **Simple**: Just build and attach - no package manager complexity

This setup provides professional distribution without ongoing costs or complex package manager submissions! 🎯
