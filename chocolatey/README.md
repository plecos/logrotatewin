# Chocolatey Package for LogRotate for Windows

This directory contains the files needed to create a Chocolatey package for LogRotate for Windows.

## Building the Package

### Option 1: Local Build (for testing)

1. Build the Release version of the project:
   ```powershell
   dotnet build -c Release
   ```

2. Copy the Release binaries to the chocolatey/tools directory:
   ```powershell
   .\build-chocolatey-local.ps1
   ```

3. Build the Chocolatey package:
   ```powershell
   cd chocolatey
   choco pack
   ```

4. Install locally for testing:
   ```powershell
   choco install logrotatewin -s . -f
   ```

### Option 2: GitHub Release Build (for distribution)

1. Create a release on GitHub with the binaries
2. Update the URL and checksum in `tools\chocolateyinstall.ps1`
3. Build the package:
   ```powershell
   cd chocolatey
   choco pack
   ```

4. Submit to Chocolatey Community Repository:
   ```powershell
   choco push logrotatewin.0.0.0.19.nupkg --source https://push.chocolatey.org/
   ```

## Testing the Package Locally

```powershell
# Install
choco install logrotatewin -s chocolatey -f

# Test
logrotate --help

# Uninstall
choco uninstall logrotatewin
```

## Package Structure

- `logrotatewin.nuspec` - Package metadata and dependencies
- `tools/chocolateyinstall.ps1` - Installation script (for GitHub releases)
- `tools/chocolateyinstall-local.ps1` - Installation script (for local builds)
- `tools/chocolateyuninstall.ps1` - Uninstallation script
- `tools/*.exe` - Application binaries (copied during build)
- `tools/Content/` - Configuration files and documentation (copied during build)

## Notes

- The package requires .NET Framework 4.8 or better
- The executable is automatically shimmed by Chocolatey, making it available in PATH
- Configuration files are included in the `Content` subdirectory
