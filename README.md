# CLEF Viewer

A modern desktop application for viewing and analyzing **CLEF (Compact Log Event Format)** log files with advanced filtering, search, and visualization capabilities.

![CLEF Viewer](https://img.shields.io/badge/platform-Windows%20%7C%20Linux-blue)
![License](https://img.shields.io/badge/license-MIT-green)
![Version](https://img.shields.io/github/v/release/azygis/clef-viewer)

## 🎯 What is CLEF?

CLEF (Compact Log Event Format) is a JSON-based log file format that's particularly popular with .NET applications using Serilog. CLEF files contain structured log events with timestamps, levels, messages, and properties, making them powerful for analysis but challenging to read in raw format.

## ✨ Features

### 📋 **Log File Management**

- Open and view CLEF log files instantly
- Real-time log file monitoring and updates
- Support for large log files with efficient loading
- Recent files history for quick access

### 🔍 **Advanced Filtering & Search**

- Filter by log level (Verbose, Debug, Information, Warning, Error, Fatal)
- Full-text search across log messages and properties
- Date/time range filtering
- Custom property filtering
- Regular expression support

### 📊 **Data Visualization**

- Clean, tabular view of log events
- Expandable property inspection
- Event count summaries by level
- Syntax highlighting for JSON properties
- Timestamps in customizable format

### 🎨 **User Experience**

- Modern, responsive UI built with Vue.js and Vuetify
- Resizable columns and panels
- Export filtered results

## 📦 Installation

### Windows

- **Installer** (recommended): Download the latest `.exe` installer
- **Portable**: Download the portable `.exe` version

### Linux

- **AppImage** (recommended): Download the latest `.AppImage` file
- **Debian/Ubuntu**: Download the `.deb` package
- **Fedora/RHEL/CentOS**: Download the `.rpm` package
- **Portable**: Download the `.tar.gz` archive

**📥 [Download from GitHub Releases](https://github.com/azygis/clef-viewer/releases/latest)**

**Note**: Package manager support (Chocolatey, Winget, Snap, Flatpak, etc.) is planned for future releases.

## 🚀 Quick Start

1. **Launch the application**
2. **Open a CLEF file** using File → Open or drag & drop
3. **Filter events** using the sidebar controls
4. **Search** for specific text or patterns
5. **Inspect properties** by expanding log events
6. **Export results** if needed

## 🛠️ Development

### Prerequisites

- Node.js 18+
- .NET 8.0 SDK
- Docker (for cross-platform builds)

### Setup

```bash
# Clone repository
git clone https://github.com/azygis/clef-viewer.git
cd clef-viewer

# Install dependencies
cd frontend && npm install

# Start development server
npm run dev
```

### Building

```bash
# Build for current platform
./build.sh

# Test builds
./scripts/test-release.sh
```

## 🏗️ Architecture

- **Frontend**: Electron + Vue.js + TypeScript + Vuetify
- **Backend**: .NET 8.0 Web API for log processing
- **Communication**: SignalR for real-time updates
- **Packaging**: Electron Builder with cross-platform support

## 🤝 Contributing

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

- **Issues**: [GitHub Issues](https://github.com/azygis/clef-viewer/issues)
- **Discussions**: [GitHub Discussions](https://github.com/azygis/clef-viewer/discussions)

## 🏆 Why CLEF Viewer?

- **🎯 Purpose-built** for CLEF log analysis
- **⚡ Fast** handling of large log files
- **🔍 Powerful** filtering and search capabilities
- **🎨 Modern** user interface
- **🆓 Free** and open source
- **🌐 Cross-platform** Windows and Linux support

## 🙏 Acknowledgments

This project is heavily inspired by [@warrenbuckley](https://github.com/warrenbuckley) and his excellent [Compact-Log-Format-Viewer](https://github.com/warrenbuckley/Compact-Log-Format-Viewer). Warren's work provided the foundation and inspiration for building this modern version with enhanced features and performance.

---
