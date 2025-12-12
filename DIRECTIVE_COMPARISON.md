# LogrotateWin vs Linux Logrotate - Directive Comparison

This document compares the directives implemented in logrotatewin against the official Linux logrotate directives.

## Summary Statistics

- **Linux logrotate total directives**: 69 configuration file directives
- **Logrotatewin implemented**: 63 functional directives (includes enhanced weekly/monthly)
- **Logrotatewin Windows-specific**: 7 additional directives
- **Not implemented**: 3 directives (2 Unix-specific hardlink directives + 1 Unix-only su directive)
- **Coverage**: 91% of Linux directives (63/69)

## Fully Implemented Directives ✅

### Rotation Scheduling
- ✅ `minutes <minutes>` - Rotate after specified minutes
- ✅ `hourly` - Rotate logs hourly
- ✅ `daily` - Rotate logs daily
- ✅ `weekly [weekday]` - Rotate logs weekly (optional: specific day 0-6, Sunday=0)
- ✅ `monthly [monthday]` - Rotate logs monthly (optional: specific day 1-31)
- ✅ `yearly` - Rotate logs yearly
- ✅ `rotate <count>` - Number of rotations to keep
- ✅ `size <size>` - Rotate when file reaches size
- ✅ `minsize <size>` - Rotate when size AND time conditions met
- ✅ `maxsize <size>` - Rotate when size exceeded, even before time interval

### Compression
- ✅ `compress` - Compress rotated logs
- ✅ `nocompress` - Don't compress
- ✅ `compresscmd` - Custom compression command
- ✅ `uncompresscmd` - Custom decompression command (parsing only)
- ✅ `compressext` - Custom compression extension
- ✅ `compressoptions` - Options for compression command
- ✅ `delaycompress` - Delay compression until next rotation
- ✅ `nodelaycompress` - Don't delay compression

### File Handling
- ✅ `create` - Create new log file after rotation
- ✅ `nocreate` - Don't create new log file
- ✅ `copy` - Copy instead of rename
- ✅ `nocopy` - Don't copy
- ✅ `copytruncate` - Copy then truncate original
- ✅ `nocopytruncate` - Don't use copytruncate
- ✅ `renamecopy` - Rename to temp, run postrotate script, copy to final location
- ✅ `norenamecopy` - Don't use renamecopy
- ✅ `olddir <directory>` - Move rotated logs to directory
- ✅ `noolddir` - Keep rotated logs in same directory
- ✅ `createolddir` - Automatically create olddir if missing (default behavior)
- ✅ `nocreateolddir` - Don't create olddir, error if missing
- ✅ `missingok` - Don't error if log is missing
- ✅ `ifempty` - Rotate even if empty
- ✅ `notifempty` - Don't rotate if empty

### File Naming
- ✅ `dateext` - Use date extension instead of numbers
- ✅ `nodateext` - Don't use date extensions
- ✅ `dateformat <format>` - Custom date format
- ✅ `dateyesterday` - Use yesterday's date in rotation
- ✅ `nodateyesterday` - Use today's date (default)
- ✅ `datehourago` - Use hour-ago timestamp
- ✅ `nodatehourago` - Use current hour (default)
- ✅ `start <number>` - Starting number for rotation sequence
- ✅ `extension <ext>` - Preserve file extension after rotation
- ✅ `addextension <ext>` - Add extension after rotation suffix

### Deletion/Cleanup
- ✅ `maxage <days>` - Remove logs older than specified days
- ✅ `shred` - Use shred to delete files
- ✅ `noshred` - Use normal deletion
- ✅ `shredcycles <count>` - Number of shred overwrite cycles

### Scripts
- ✅ `prerotate` / `endscript` - Script before rotation
- ✅ `postrotate` / `endscript` - Script after rotation
- ✅ `firstaction` / `endscript` - Script before all rotations
- ✅ `lastaction` / `endscript` - Script after all rotations
- ✅ `preremove` / `endscript` - Script before file deletion
- ✅ `sharedscripts` - Run scripts once for all logs
- ✅ `nosharedscripts` - Run scripts per log file

### Mail
- ✅ `mail <address>` - Mail logs before deletion
- ✅ `nomail` - Don't mail logs
- ✅ `mailfirst` - Mail just-rotated file
- ✅ `maillast` - Mail about-to-expire file

### Configuration
- ✅ `include <file>` - Include external config
- ✅ `tabooext [+] <list>` - Extensions to ignore in include
- ✅ `taboopat [+] <list>` - Glob patterns to ignore in include

## Windows-Specific Extensions ✅

These directives are unique to logrotatewin and don't exist in Linux logrotate:

- ✅ `smtpserver` - SMTP server for email
- ✅ `smtpport` - SMTP port
- ✅ `smtpssl` / `nosmtpssl` - Enable/disable SSL
- ✅ `smtpuser` - SMTP username
- ✅ `smtpuserpwd` - SMTP password
- ✅ `smtpfrom` - From address
- ✅ `logfileopen_retry` - Retry opening locked files
- ✅ `logfileopen_msbetweenretryattempts` - Retry delay
- ✅ `logfileopen_numretryattempts` - Number of retries

## Missing/Not Implemented Directives ⚠️

### Frequency Directives
*All frequency directives are now fully implemented*

### File Selection
- ✅ `nomissingok` - Error if file missing (opposite behavior only)
- ✅ `ignoreduplicates` - Ignore duplicate log file matches
- ✅ `minage <count>` - Don't rotate logs younger than N days
- ✅ `taboopat [+] <list>` - Glob patterns to ignore

### Files and Folders
- ❌ `allowhardlink` - Allow rotation of hardlinked files
- ❌ `noallowhardlink` - Prevent rotation of hardlinked files

### Compression
*All compression directives are now fully implemented*

### Filenames
*All filename/date directives are now fully implemented*

### User/Group Management
- ❌ `su <user> <group>` - Run rotation as specific user (not applicable on Windows)

## Implementation Recommendations

### Medium Priority (Less Common)
1. **`allowhardlink`** - Unix-specific edge case

### Not Applicable for Windows
1. **`su <user> <group>`** - Unix permission model not applicable

## Notes

- Most "no-" prefix directives (like `nocompress`, `nocreate`) are implemented where their positive counterpart exists
- The implementation focuses on the most common use cases for Windows environments
- Windows-specific SMTP directives provide better integration than Linux's mail command
- Some directives like `uncompresscmd` are parsed but not fully implemented (decompression not needed for rotation)

## References

- [Linux logrotate man page](https://man7.org/linux/man-pages/man8/logrotate.8.html)
- [logrotate on linux.die.net](https://linux.die.net/man/8/logrotate)
- [Ubuntu manpage](https://manpages.ubuntu.com/manpages/focal/man8/logrotate.8.html)
