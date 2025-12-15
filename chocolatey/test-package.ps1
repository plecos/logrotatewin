# Test script for Chocolatey package
# Run this as Administrator

$ErrorActionPreference = 'Stop'

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Testing LogRotate Chocolatey Package" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if running as administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script must be run as Administrator!" -ForegroundColor Red
    Write-Host "Please right-click PowerShell and select 'Run as Administrator'" -ForegroundColor Yellow
    exit 1
}

# Uninstall if already installed
Write-Host "Checking for existing installation..." -ForegroundColor Green
$existing = choco list --local-only logrotate
if ($existing -match "logrotate") {
    Write-Host "Uninstalling existing version..." -ForegroundColor Yellow
    choco uninstall logrotate -y
}

# Install from local package
Write-Host "`nInstalling logrotate from local package..." -ForegroundColor Green
choco install logrotate -s . -f -y

if ($LASTEXITCODE -ne 0) {
    Write-Host "`nERROR: Installation failed!" -ForegroundColor Red
    exit 1
}

# Test if command is available
Write-Host "`nTesting logrotate command..." -ForegroundColor Green
$logrotate = Get-Command logrotate -ErrorAction SilentlyContinue
if ($logrotate) {
    Write-Host "SUCCESS: logrotate command is available" -ForegroundColor Green
    Write-Host "Location: $($logrotate.Source)" -ForegroundColor Gray

    # Test help command
    Write-Host "`nRunning logrotate with no args to check it works..." -ForegroundColor Green
    & logrotate 2>&1 | Select-Object -First 5

    Write-Host "`n=====================================" -ForegroundColor Cyan
    Write-Host "Package test completed successfully!" -ForegroundColor Green
    Write-Host "=====================================" -ForegroundColor Cyan
} else {
    Write-Host "ERROR: logrotate command not found in PATH" -ForegroundColor Red
    exit 1
}

Write-Host "`nTo uninstall: choco uninstall logrotate -y" -ForegroundColor Yellow
