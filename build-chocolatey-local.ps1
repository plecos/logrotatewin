# Build script for creating a local Chocolatey package
# This script builds the project and copies files to the chocolatey/tools directory

$ErrorActionPreference = 'Stop'

Write-Host "Building LogRotate for Windows (Release)..." -ForegroundColor Green
dotnet build -c Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed!"
    exit 1
}

$releaseDir = "logrotate\bin\Release\net48"
$toolsDir = "chocolatey\tools"

Write-Host "`nCopying files to Chocolatey tools directory..." -ForegroundColor Green

# Clean the tools directory (except .ps1 files)
if (Test-Path $toolsDir) {
    Get-ChildItem $toolsDir -Exclude "*.ps1" | Remove-Item -Recurse -Force
}

# Copy executable and config
Copy-Item "$releaseDir\logrotate.exe" -Destination $toolsDir
Copy-Item "$releaseDir\logrotate.exe.config" -Destination $toolsDir -ErrorAction SilentlyContinue

# Copy Content directory
if (Test-Path "$releaseDir\Content") {
    Copy-Item "$releaseDir\Content" -Destination $toolsDir -Recurse
}

# Rename the install script for local builds
if (Test-Path "$toolsDir\chocolateyinstall-local.ps1") {
    Copy-Item "$toolsDir\chocolateyinstall-local.ps1" -Destination "$toolsDir\chocolateyinstall.ps1" -Force
}

Write-Host "`nFiles copied successfully!" -ForegroundColor Green
Write-Host "`nTo build the Chocolatey package, run:" -ForegroundColor Yellow
Write-Host "  cd chocolatey" -ForegroundColor Yellow
Write-Host "  choco pack" -ForegroundColor Yellow
Write-Host "`nTo install locally for testing:" -ForegroundColor Yellow
Write-Host "  choco install logrotatewin -s . -f" -ForegroundColor Yellow
