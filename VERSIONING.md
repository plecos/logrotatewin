# Automated Versioning Guide

This project uses [Nerdbank.GitVersioning](https://github.com/dotnet/Nerdbank.GitVersioning) for automated semantic versioning.

## How It Works

### Version Calculation

- **Version Format**: `0.0.X` where X auto-increments based on commit count
- **Base Version**: `0.0` (configured in `version.json`)
- **Version Offset**: `20` (so versions start from `0.0.20`)
- **Increment Trigger**: Every commit to `master` branch increments the version

### Example Version Progression

```
Commit 1 after version.json → 0.0.21
Commit 2 after version.json → 0.0.22
Commit 3 after version.json → 0.0.23
...
```

## Automated Updates

When you merge to `master`, GitHub Actions automatically:

1. ✅ **Builds** the project with the new version number
2. ✅ **Updates** `logrotate\Content\README.md` with release notes
3. ✅ **Commits** the updated README with `[skip ci]` to avoid version bump
4. ✅ **Creates** a GitHub release with the version tag
5. ✅ **Packages** the release as a ZIP file

When you run the Chocolatey publish workflow:

1. ✅ **Updates** `chocolatey\logrotatewin.nuspec` with version
2. ✅ **Updates** `chocolatey\VERIFICATION.txt` with version, commit, and checksum
3. ✅ **Generates** `chocolatey\tools\chocolateyinstall.ps1` dynamically
4. ✅ **Publishes** to Chocolatey Community Repository

## Writing Custom Release Notes

You have **two options** for release notes:

### Option A: Auto-Generated (Default)

If you don't pre-populate release notes, the workflow automatically generates:

```markdown
### 0.0.23 - 12 Dec 2025

- Automated release with Nerdbank.GitVersioning
- See [commit history](https://github.com/ken-salter/logrotatewin/commits/master) for detailed changes
```

### Option B: Custom Release Notes (Recommended)

Use the helper script to add custom release notes **before** merging:

```powershell
# Add template for next version
.\add-release-notes.ps1

# Or specify version manually
.\add-release-notes.ps1 -Version "0.0.25"

# Or add with custom message
.\add-release-notes.ps1 -Message "- Fixed critical rotation bug`n- Added new directive support"
```

**Workflow:**

1. Run `.\add-release-notes.ps1` to add a template
2. Edit `logrotate\Content\README.md` and fill in your release notes
3. Commit with `[skip ci]` to avoid version bump:
   ```bash
   git add logrotate\Content\README.md
   git commit -m "docs: Add release notes for v0.0.23 [skip ci]"
   ```
4. Merge to master - workflow will detect existing notes and skip auto-generation

## Version Control

### Files Updated Automatically

| File | When | How |
|------|------|-----|
| `logrotate.exe` | Every build | NBGv embeds version at compile time |
| `logrotate\Content\README.md` | On release | Workflow updates or uses existing |
| `chocolatey\logrotatewin.nuspec` | Chocolatey publish | Workflow updates |
| `chocolatey\VERIFICATION.txt` | Chocolatey publish | Workflow regenerates |

### Files NOT Updated

| File | Why |
|------|-----|
| `version.json` | Only change to adjust baseline or offset |
| `logrotate.csproj` | Version removed - managed by NBGv |
| Documentation examples | Static examples don't need version sync |

## Preventing Version Bumps

Use `[skip ci]` in commit messages to prevent workflow execution:

```bash
git commit -m "docs: Update README [skip ci]"
git commit -m "chore: Fix typo in comments [skip ci]"
```

**When to use `[skip ci]`:**
- Documentation-only changes
- Comment updates
- Formatting changes
- README updates

**When NOT to use `[skip ci]`:**
- Code changes (let version auto-increment)
- Bug fixes
- New features
- Dependency updates

## Manual Version Changes

### Changing the Base Version

Edit `version.json` to change the major/minor version:

```json
{
  "version": "0.1",  // Changed from "0.0"
  "versionHeightOffset": 0  // Reset offset
}
```

Next build will be `0.1.1`, `0.1.2`, etc.

### Adjusting Version Offset

To continue from a specific number without changing base:

```json
{
  "version": "0.0",
  "versionHeightOffset": 50  // Next build will be 0.0.51+
}
```

## Troubleshooting

### Version Not Incrementing

**Problem**: Version stays at `0.0.0.0` or doesn't increment

**Solutions**:
- Ensure `version.json` exists in repository root
- Check that commits are on the `master` branch
- Verify GitHub Actions has `fetch-depth: 0` (enables full git history)

### README Not Updating

**Problem**: README.md doesn't get updated with new version

**Solutions**:
- Check that `release.yml` workflow completed successfully
- Look for "Release Notes" section in `logrotate\Content\README.md`
- Verify commit was pushed back to repo (check git log)

### Infinite Build Loop

**Problem**: Every commit triggers multiple builds

**Solutions**:
- Ensure `[skip ci]` is in README commit message (line 114 in `release.yml`)
- Check `.github/workflows/release.yml` paths-ignore settings
- Verify the commit author is `github-actions[bot]`

### Wrong Version in README

**Problem**: README shows different version than executable

**Solutions**:
- This is normal if you manually updated README without `[skip ci]`
- Delete the README entry and let workflow regenerate it
- Or manually update to match current version

## Configuration Files

### version.json

Main version configuration file:

```json
{
  "version": "0.0",                    // Major.Minor (patch auto-increments)
  "versionHeightOffset": 20,           // Starting build number
  "assemblyVersion": {
    "precision": "revision"            // Include all 4 parts in AssemblyVersion
  },
  "publicReleaseRefSpec": [
    "^refs/heads/master$"              // Only master gets release versions
  ],
  "cloudBuild": {
    "buildNumber": {
      "enabled": true                  // Enable CI build number integration
    }
  }
}
```

### GitHub Actions Workflows

- `.github/workflows/build.yml` - Build and test on every push
- `.github/workflows/release.yml` - Create releases on master push
- `.github/workflows/publish-chocolatey.yml` - Publish to Chocolatey

## Best Practices

### For Regular Development

1. ✅ Make code changes
2. ✅ Commit with descriptive message
3. ✅ Merge to master
4. ✅ Let automation handle versioning

### For Releases with Custom Notes

1. ✅ Run `.\add-release-notes.ps1`
2. ✅ Edit README with detailed changes
3. ✅ Commit with `[skip ci]`
4. ✅ Merge code changes to master
5. ✅ Workflow uses your custom notes

### For Hotfixes

1. ✅ Fix the bug
2. ✅ Commit and merge to master
3. ✅ Version auto-increments
4. ✅ Release created automatically

## FAQ

**Q: Can I manually set a specific version?**
A: Yes, edit `version.json` and adjust both `version` and `versionHeightOffset`. But generally, let NBGv handle it.

**Q: What happens if I delete a commit?**
A: NBGv calculates based on commit count, so removing commits will lower the version. Use `versionHeightOffset` to compensate.

**Q: Can I have different versions on different branches?**
A: No, version is calculated from commit history. Feature branches will show prerelease versions like `0.0.23-alpha`.

**Q: How do I skip a version number?**
A: Increase `versionHeightOffset` in `version.json`. For example, to skip from 0.0.23 to 0.0.30, add 7 to the offset.

**Q: What if the workflow fails?**
A: The version still increments (it's based on commits, not workflow success). Fix the workflow and manually create the release if needed.

## References

- [Nerdbank.GitVersioning Documentation](https://github.com/dotnet/Nerdbank.GitVersioning/blob/main/doc/nbgv.md)
- [GitHub Actions - Skipping workflows](https://docs.github.com/en/actions/managing-workflow-runs/skipping-workflow-runs)
- [Semantic Versioning Spec](https://semver.org/)
