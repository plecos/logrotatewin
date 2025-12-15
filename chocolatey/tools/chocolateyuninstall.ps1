$ErrorActionPreference = 'Stop'

$packageName = 'logrotate'
$toolsDir    = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"

# Remove the executable (Chocolatey will handle shim removal automatically)
$exePath = Join-Path $toolsDir "logrotate.exe"
if (Test-Path $exePath) {
  Remove-Item $exePath -Force
  Write-Host "LogRotate for Windows has been uninstalled."
}

# Clean up any remaining files
$filesToRemove = @(
  "logrotate.exe.config",
  "Content"
)

foreach ($file in $filesToRemove) {
  $filePath = Join-Path $toolsDir $file
  if (Test-Path $filePath) {
    Remove-Item $filePath -Recurse -Force
  }
}
