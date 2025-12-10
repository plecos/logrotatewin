# Chocolatey Package Setup for LogRotate for Windows

This document describes the Chocolatey package setup for LogRotate for Windows.

## Directory Structure

```
chocolatey/
├── logrotatewin.nuspec           # Package metadata
├── README.md                      # Build and packaging instructions
├── VERIFICATION.txt               # Package verification information
├── test-package.ps1              # Testing script (run as admin)
├── logrotatewin.0.0.0.19.nupkg   # Built package (generated)
└── tools/
    ├── chocolateyinstall.ps1          # Install script (for local builds)
    ├── chocolateyinstall-local.ps1    # Template for local builds
    ├── chocolateyuninstall.ps1        # Uninstall script
    ├── logrotate.exe                  # Application binary (copied during build)
    ├── logrotate.exe.config          # App configuration (copied during build)
    └── Content/                       # Documentation and config files (copied during build)
        ├── logrotate.conf
        ├── gnu_license.rtf
        └── README.md
```

## Quick Start

### 1. Build the Chocolatey Package

Run the build script from the repository root:

```powershell
.\build-chocolatey-local.ps1
```

This will:
- Build the Release version of the application
- Copy binaries to `chocolatey/tools/`
- Prepare the package for building

### 2. Create the Package

```powershell
cd chocolatey
choco pack
```

This creates `logrotatewin.0.0.0.19.nupkg`

### 3. Test the Package Locally

**IMPORTANT: Must run PowerShell as Administrator**

```powershell
cd chocolatey
.\test-package.ps1
```

Or manually:

```powershell
# Install
choco install logrotatewin -s . -f -y

# Test
logrotate

# Uninstall
choco uninstall logrotatewin -y
```

## Publishing to Chocolatey Community Repository

### Preparation

1. Create a GitHub release with the binaries
2. Get the SHA256 checksum of the release ZIP file
3. Update `tools/chocolateyinstall.ps1`:
   - Set the correct `$url` pointing to the GitHub release
   - Set the `checksum` value
4. Update `VERIFICATION.txt` with commit hash and checksum

### Publishing

1. Get your Chocolatey API key from https://community.chocolatey.org/account

2. Set the API key (one time):
   ```powershell
   choco apikey --key YOUR_API_KEY --source https://push.chocolatey.org/
   ```

3. Push the package:
   ```powershell
   choco push logrotatewin.0.0.0.19.nupkg --source https://push.chocolatey.org/
   ```

4. The package will be reviewed by Chocolatey moderators before being published

## For GitHub Release Builds

When creating a package for distribution via GitHub releases:

1. Replace `tools/chocolateyinstall.ps1` with the GitHub release version:
   ```powershell
   # Update the URL to point to your GitHub release
   $url = 'https://github.com/ken-salter/logrotatewin/releases/download/v0.0.0.19/logrotatewin-0.0.0.19.zip'

   # Add the checksum
   $checksum = 'YOUR_SHA256_CHECKSUM_HERE'
   ```

2. Remove the binaries from `tools/` (they'll be downloaded from the URL)

3. Build the package: `choco pack`

## Installation for End Users

Once published to Chocolatey, users can install with:

```powershell
choco install logrotatewin
```

Or for the latest version:

```powershell
choco upgrade logrotatewin
```

## Files Ignored by Git

The following files are automatically excluded from git (see `.gitignore`):
- `chocolatey/tools/*.exe`
- `chocolatey/tools/*.config`
- `chocolatey/tools/Content/`
- `chocolatey/*.nupkg`

These files are generated during the build process and should not be committed to the repository.

## Links

- [Chocolatey Package Documentation](https://docs.chocolatey.org/en-us/create/create-packages)
- [Chocolatey Package Guidelines](https://docs.chocolatey.org/en-us/community-repository/moderation/package-validator/rules/)
- [Project Homepage](https://sourceforge.net/projects/logrotatewin/)
- [GitHub Repository](https://github.com/ken-salter/logrotatewin)
