$ErrorActionPreference = 'Stop'
$packageName = 'clef-viewer'
$toolsDir = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url64 = 'https://github.com/azygis/clef-viewer/releases/download/v__VERSION__/clef-viewer-__VERSION__-setup.exe'
$checksum64 = '__CHECKSUM__'

$packageArgs = @{
    packageName    = $packageName
    unzipLocation  = $toolsDir
    fileType       = 'exe'
    url64bit       = $url64
    softwareName   = 'Compact Log Event Format Viewer*'
    checksum64     = $checksum64
    checksumType64 = 'sha256'
    silentArgs     = '/S'
    validExitCodes = @(0)
}

Install-ChocolateyPackage @packageArgs
