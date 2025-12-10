# GitHub Actions Workflows

This repository uses GitHub Actions for automated building, testing, releasing, and publishing to Chocolatey.

## Workflows

### 1. Build and Test (`build.yml`)

**Triggers:**
- Push to `master` or `develop` branches
- Pull requests to `master`

**What it does:**
- Sets up .NET SDK 8.0
- Restores dependencies
- Builds both Debug and Release configurations
- Tests the executable
- Uploads build artifacts

**Artifacts:** Build outputs are kept for 7 days

### 2. Create Release (`release.yml`)

**Triggers:**
- Push to `master` branch (except for markdown and workflow changes)

**What it does:**
- Extracts version from `logrotate.csproj`
- Checks if release tag already exists (skips if it does)
- Builds Release configuration
- Creates ZIP package with:
  - `logrotate.exe`
  - `logrotate.exe.config`
  - `Content/` directory (docs and sample config)
  - Package README
- Calculates SHA256 checksum
- Generates release notes
- Creates GitHub Release with tag `v{version}`
- Uploads ZIP file as release asset

**Version Detection:**
The version is automatically read from the `<Version>` property in `logrotate/logrotate.csproj`:
```xml
<Version>0.0.0.19</Version>
```

**Release Naming:**
- Tag: `v0.0.0.19`
- Release Title: `LogRotate for Windows 0.0.0.19`
- Asset: `logrotatewin-0.0.0.19.zip`

### 3. Publish to Chocolatey (`publish-chocolatey.yml`)

**Triggers:**
- Manual workflow dispatch (Actions tab → "Publish to Chocolatey" → "Run workflow")
- Can be automated by uncommenting the `release` trigger

**What it does:**
- Downloads the release ZIP from GitHub
- Calculates SHA256 checksum
- Updates `chocolatey/logrotatewin.nuspec` with version
- Creates `chocolateyinstall.ps1` with download URL and checksum
- Builds Chocolatey package (`.nupkg`)
- Tests package creation
- Publishes to Chocolatey Community Repository (if `CHOCOLATEY_API_KEY` secret is configured)
- Uploads package as artifact

## Setup Instructions

### Required Permissions

The workflows require the following repository permissions:
- **Contents**: Write (for creating releases)
- **Actions**: Write (for uploading artifacts)

These are automatically granted for workflows in the same repository.

### Setting up Chocolatey Publishing

To enable automatic publishing to Chocolatey:

1. **Get your Chocolatey API Key:**
   - Go to https://community.chocolatey.org/account
   - Sign in or create an account
   - Navigate to "API Keys" section
   - Copy your API key

2. **Add the API key to GitHub:**
   - Go to your repository → Settings → Secrets and variables → Actions
   - Click "New repository secret"
   - Name: `CHOCOLATEY_API_KEY`
   - Value: Your API key from step 1
   - Click "Add secret"

3. **Enable automatic publishing (optional):**
   - Edit `.github/workflows/publish-chocolatey.yml`
   - Uncomment these lines:
     ```yaml
     # release:
     #   types: [published]
     ```
   - This will automatically publish to Chocolatey when a GitHub release is created

## Workflow Progression

### Normal Release Flow

1. **Merge PR to master** → `build.yml` runs

2. **Master push** → `release.yml` runs:
   - Reads version from `.csproj`
   - Creates GitHub Release if tag doesn't exist
   - Uploads ZIP package

3. **Manual Chocolatey publish**:
   - Go to Actions → "Publish to Chocolatey"
   - Click "Run workflow"
   - Enter version (e.g., `0.0.0.19`)
   - Click "Run workflow"

4. **Chocolatey moderation**:
   - Package is submitted for review
   - Moderators will review and approve
   - Package becomes available on Chocolatey

### Automated Release Flow (Future)

Once `CHOCOLATEY_API_KEY` is configured and automatic triggers are enabled:

1. Merge PR to master
2. `build.yml` runs (validation)
3. `release.yml` runs (creates GitHub Release)
4. `publish-chocolatey.yml` runs automatically (publishes to Chocolatey)

## Version Management

To release a new version:

1. **Update version in `logrotate/logrotate.csproj`:**
   ```xml
   <Version>0.0.0.20</Version>
   <FileVersion>0.0.0.20</FileVersion>
   <AssemblyVersion>0.0.0.20</AssemblyVersion>
   ```

2. **Update release notes in `logrotate/Content/README.md`:**
   ```markdown
   ### 0.0.0.20 - DD MMM YYYY
   - Feature 1
   - Feature 2
   ```

3. **Update copyright year if needed:**
   ```xml
   <Copyright>Copyright © 2013-2025 Ken Salter</Copyright>
   ```

4. **Commit and push to master:**
   ```bash
   git add logrotate/logrotate.csproj logrotate/Content/README.md
   git commit -m "Bump version to 0.0.0.20"
   git push origin master
   ```

5. **Wait for workflows to complete:**
   - Check the Actions tab to monitor progress
   - Release will be created automatically

## Troubleshooting

### Release not created

**Check:**
- Does the tag already exist? (`git tag -l`)
- Was there an error in the build?
- Check Actions tab for error logs

### Chocolatey publish fails

**Common issues:**
- API key not configured or expired
- Package version already exists on Chocolatey
- Package doesn't meet Chocolatey guidelines
- Network connectivity issues

**Solutions:**
- Verify `CHOCOLATEY_API_KEY` secret is set
- Check Chocolatey moderation queue
- Review package validation errors in workflow logs

### Build artifacts not found

**Check:**
- Was the build step successful?
- Are the file paths correct?
- Check artifact upload/download logs in Actions

## Testing Workflows Locally

You can test the workflow logic locally using PowerShell:

```powershell
# Test build
dotnet build -c Release

# Test package creation (simulating release workflow)
$version = "0.0.0.19"
$outputDir = "logrotate\bin\Release\net48"
$packageName = "logrotatewin-$version"

New-Item -ItemType Directory -Path $packageName -Force
Copy-Item "$outputDir\logrotate.exe" -Destination $packageName
Copy-Item "$outputDir\logrotate.exe.config" -Destination $packageName
Copy-Item "$outputDir\Content" -Destination $packageName -Recurse

Compress-Archive -Path $packageName\* -DestinationPath "$packageName.zip" -Force
$hash = (Get-FileHash "$packageName.zip" -Algorithm SHA256).Hash
Write-Host "SHA256: $hash"
```

## Links

- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Chocolatey Package Guidelines](https://docs.chocolatey.org/en-us/community-repository/moderation/)
- [softprops/action-gh-release](https://github.com/softprops/action-gh-release)
