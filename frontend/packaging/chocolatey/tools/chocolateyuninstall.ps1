$ErrorActionPreference = 'Stop'
$packageArgs = @{
    packageName    = 'clef-viewer'
    softwareName   = 'Compact Log Event Format Viewer*'
    fileType       = 'exe'
    silentArgs     = '/S'
    validExitCodes = @(0)
}

[array]$key = Get-UninstallRegistryKey -SoftwareName $packageArgs['softwareName']

if ($key.Count -eq 1) {
    $key | % { 
        $packageArgs['file'] = "$($_.UninstallString)"
        if ($packageArgs['fileType'] -eq 'MSI') {
            $packageArgs['silentArgs'] = "$($_.PSChildName) $($packageArgs['silentArgs'])"
            $packageArgs['file'] = ''
        }
        Uninstall-ChocolateyPackage @packageArgs
    }
}
elseif ($key.Count -eq 0) {
    Write-Warning "$packageName has already been uninstalled by other means."
}
elseif ($key.Count -gt 1) {
    Write-Warning "$($key.Count) matches found!"
    Write-Warning "To prevent accidental data loss, no programs will be uninstalled."
}
