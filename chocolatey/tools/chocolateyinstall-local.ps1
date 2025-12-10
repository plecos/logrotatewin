$ErrorActionPreference = 'Stop'

$packageName = 'logrotatewin'
$toolsDir    = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# For local installation, files should be included in the package
$exePath = Join-Path $toolsDir "logrotate.exe"

if (Test-Path $exePath) {
  Write-Host "LogRotate for Windows has been installed to: $toolsDir"
  Write-Host "You can now run 'logrotate' from the command line."

  # Display configuration file location
  $contentDir = Join-Path $toolsDir "Content"
  if (Test-Path $contentDir) {
    Write-Host ""
    Write-Host "Sample configuration file: $contentDir\logrotate.conf"
    Write-Host "Documentation: $contentDir\README.md"
  }
} else {
  Write-Error "Installation failed: logrotate.exe not found at $exePath"
}
