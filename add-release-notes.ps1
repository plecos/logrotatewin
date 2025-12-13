#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Adds a release notes template to README.md for the next version.

.DESCRIPTION
    This script helps you pre-populate release notes in README.md before merging to master.
    It detects the next version number from Nerdbank.GitVersioning and adds a template
    entry to the Release Notes section.

.PARAMETER Version
    Optional. Specify the version number manually. If not provided, will auto-detect from NBGv.

.PARAMETER Message
    Optional. Custom release note message. If not provided, uses a template.

.EXAMPLE
    .\add-release-notes.ps1
    # Auto-detects version and adds template

.EXAMPLE
    .\add-release-notes.ps1 -Version "0.0.25"
    # Adds template for version 0.0.25

.EXAMPLE
    .\add-release-notes.ps1 -Message "- Fixed critical bug in rotation logic"
    # Auto-detects version and uses custom message
#>

param(
    [string]$Version,
    [string]$Message
)

$ErrorActionPreference = 'Stop'

# Detect version if not provided
if (-not $Version) {
    Write-Host "Detecting version from Nerdbank.GitVersioning..."

    # Check if nbgv is installed
    if (-not (Get-Command nbgv -ErrorAction SilentlyContinue)) {
        Write-Host "Installing nbgv tool..."
        dotnet tool install --global nbgv
    }

    $versionJson = nbgv get-version -f json | ConvertFrom-Json
    $Version = $versionJson.SimpleVersion
    Write-Host "Detected version: $Version" -ForegroundColor Green
}

# Set default message if not provided
if (-not $Message) {
    $Message = @"
- TODO: Add release notes here
- TODO: Document new features, bug fixes, and changes
- Remove this TODO section before committing
"@
}

# Path to README
$readmePath = "logrotate\Content\README.md"

if (-not (Test-Path $readmePath)) {
    Write-Error "README.md not found at: $readmePath"
    exit 1
}

# Read README
$content = Get-Content $readmePath -Raw

# Check if version already exists
$versionPattern = "### $Version -"
if ($content -match [regex]::Escape($versionPattern)) {
    Write-Warning "Release notes for version $Version already exist in README.md"
    Write-Host "Edit the existing entry or specify a different version with -Version parameter"
    exit 0
}

# Get current date
$date = Get-Date -Format "dd MMM yyyy"

# Create new entry
$newEntryLines = @(
    "## Release Notes",
    "",
    "### $Version - $date",
    "",
    $Message,
    ""
)
$newEntry = $newEntryLines -join "`n"

# Replace the "## Release Notes" header
if ($content -match '(?s)(## Release Notes\r?\n)') {
    $content = $content -replace '## Release Notes\r?\n', $newEntry

    # Write back to file
    $content | Out-File -FilePath $readmePath -Encoding utf8 -NoNewline

    Write-Host "`nSuccessfully added release notes template for version $Version" -ForegroundColor Green
    Write-Host "`nNext steps:" -ForegroundColor Yellow
    Write-Host "1. Edit $readmePath and add your release notes"
    Write-Host "2. Commit with: git add $readmePath && git commit -m `"docs: Add release notes for v$Version [skip ci]`""
    Write-Host "3. Merge to master - the workflow will detect existing notes and skip auto-generation"
} else {
    Write-Error "Could not find '## Release Notes' section in README.md"
    exit 1
}
