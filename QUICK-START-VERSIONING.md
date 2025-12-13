# Quick Start: Automated Versioning

## TL;DR

✅ **Just commit and merge** - versions increment automatically
✅ **Use `[skip ci]`** for docs-only commits
✅ **Run `.\add-release-notes.ps1`** for custom release notes

---

## Common Workflows

### 🚀 Regular Development (Most Common)

```bash
# Make your changes
git add .
git commit -m "feat: Add new rotation directive"
git push

# Merge PR to master
# ✅ Version auto-increments (e.g., 0.0.23 → 0.0.24)
# ✅ GitHub release created automatically
# ✅ README.md updated with generic notes
```

### 📝 Release with Custom Notes

```bash
# Step 1: Add release notes template
.\add-release-notes.ps1

# Step 2: Edit logrotate\Content\README.md with your notes
# Example:
#   ### 0.0.24 - 12 Dec 2025
#   - Fixed critical bug in rotation logic
#   - Added support for new directive
#   - Updated documentation

# Step 3: Commit with [skip ci]
git add logrotate\Content\README.md
git commit -m "docs: Add release notes for v0.0.24 [skip ci]"
git push

# Step 4: Merge your code changes
git add .
git commit -m "feat: Implement new features"
git push

# Merge PR to master
# ✅ Version increments to 0.0.24
# ✅ Uses YOUR custom release notes
# ✅ GitHub release created
```

### 📚 Documentation-Only Changes

```bash
git add README.md
git commit -m "docs: Fix typo in installation guide [skip ci]"
git push

# ✅ No version increment
# ✅ No build triggered
```

### 🐛 Hotfix

```bash
# Fix the bug
git add .
git commit -m "fix: Resolve rotation edge case"
git push

# Merge to master
# ✅ Version auto-increments
# ✅ Automatic release created
```

---

## Version Format

Current: `0.0.X` where X = commit count + 20

Examples:
- First commit: `0.0.21`
- Second commit: `0.0.22`
- Third commit: `0.0.23`

---

## Key Files

| File | Purpose | Edit? |
|------|---------|-------|
| `version.json` | Version config | Rarely |
| `add-release-notes.ps1` | Helper script | No |
| `logrotate\Content\README.md` | Release notes | Yes (for custom notes) |
| `VERSIONING.md` | Full documentation | No |

---

## Troubleshooting

### Version stuck at 0.0.0.0?
- Wait for next commit after `version.json` was added
- Current build shows height=0, next will be height=1 → version 0.0.21

### README not updating?
- Check GitHub Actions logs
- Ensure "Release Notes" section exists in README.md

### Infinite build loop?
- Always use `[skip ci]` when committing documentation
- Workflow already includes it for auto-commits

---

## Need Help?

📖 **Full Documentation**: See `VERSIONING.md`
🔧 **Workflow Files**: `.github/workflows/`
⚙️ **Configuration**: `version.json`

---

**Remember**: The system is designed to "just work" - commit your code and let automation handle the rest! 🎉
