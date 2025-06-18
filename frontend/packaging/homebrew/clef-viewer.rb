class ClefViewer < Formula
  desc "Compact Log Event Format (CLEF) Viewer - Desktop application for viewing and analyzing CLEF log files"
  homepage "https://github.com/azygis/clef-viewer"
  version "#{VERSION}"

  # Support both Intel and Apple Silicon Macs
  if Hardware::CPU.arm?
    url "https://github.com/azygis/clef-viewer/releases/download/v#{VERSION}/clef-viewer-#{VERSION}-macos-arm64.zip"
    sha256 "#{SHA256_ARM64}"
  else
    url "https://github.com/azygis/clef-viewer/releases/download/v#{VERSION}/clef-viewer-#{VERSION}-macos-x64.zip"
    sha256 "#{SHA256_X64}"
  end

  depends_on macos: ">= :mojave"

  def install
    libexec.install Dir["*"]
    bin.write_exec_script libexec/"CLEF Viewer.app/Contents/MacOS/CLEF Viewer"
  end

  def caveats
    <<~EOS
      CLEF Viewer has been installed as a command-line application.
      To run the GUI application, you can:
      1. Run 'clef-viewer' from the terminal, or
      2. Copy the app to Applications folder:
         cp -r "#{libexec}/CLEF Viewer.app" /Applications/
    EOS
  end

  test do
    # Test if the binary exists and is executable
    assert_predicate libexec/"CLEF Viewer.app/Contents/MacOS/CLEF Viewer", :executable?
  end
end
