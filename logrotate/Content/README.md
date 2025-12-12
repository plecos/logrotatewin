# LogRotate for Windows

Written by Ken Salter (C) 2012-2025

You can help support my efforts by buying me a coffee!
https://buymeacoffee.com/kenasalter

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

## Program Description

This is a port of the logrotate utility available for Linux. See the Wiki for more notes.

**Feature Coverage**: Implements **91%** (63/69) of Linux logrotate directives, providing comprehensive log rotation functionality for Windows environments.

Project homepage: <https://sourceforge.net/projects/logrotatewin/>
GitHub repository: <https://github.com/ken-salter/logrotatewin>

## Requirements

- .NET Framework 4.8 or better

## Installation

### Chocolatey (Recommended)

The easiest way to install LogRotate for Windows is using Chocolatey:

```powershell
choco install logrotatewin
```

After installation, the `logrotate` command will be available in your PATH.

### Manual Installation

Download the latest release from the [GitHub Releases page](https://github.com/ken-salter/logrotatewin/releases) and extract to your desired location. The package includes:
- `logrotate.exe` - Main executable
- `README.md` - This documentation
- `gnu_license.txt` - License information
- Sample configuration files

## Building from Source

The project uses SDK-style project format and can be built using:
- Visual Studio 2019 or later
- Visual Studio Code with C# extension
- .NET SDK 6.0 or later (for tooling)

Build from command line:
```bash
dotnet build
```

Build Release configuration:
```bash
dotnet build -c Release
```

## Usage

```
logrotate [options] <configfile>
```

### Command Line Options

- `-d, --debug` - Debug mode (verbose output, no actual rotation)
- `-f, --force` - Force rotation even if not needed
- `-v, --verbose` - Verbose output
- `-s, --state <file>` - Use alternate state file
- `-?, --usage, --help` - Show usage information

### Configuration File Directives

LogRotate for Windows supports 63 configuration directives (91% of Linux logrotate directives) plus 7 Windows-specific directives. Configuration files use the same syntax as Linux logrotate.

**Example configuration:**
```
C:\logs\myapp\*.log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
    create
    postrotate
        echo "Logs rotated" >> C:\logs\rotation.log
    endscript
}
```

**Key directive categories:**
- **Rotation scheduling**: `hourly`, `daily`, `weekly`, `monthly`, `yearly`, `size`, `minsize`, `maxsize`
- **Compression**: `compress`, `nocompress`, `delaycompress`, `compresscmd`, `compressoptions`
- **File handling**: `create`, `copy`, `copytruncate`, `renamecopy`, `olddir`, `missingok`
- **File naming**: `dateext`, `dateformat`, `dateyesterday`, `datehourago`, `extension`, `addextension`
- **Cleanup**: `rotate`, `maxage`, `minage`, `shred`, `shredcycles`
- **Scripts**: `prerotate`, `postrotate`, `firstaction`, `lastaction`, `preremove`, `sharedscripts`
- **Mail**: `mail`, `mailfirst`, `maillast` (requires Windows-specific SMTP configuration)
- **Configuration**: `include`, `tabooext`, `taboopat`
- **Windows-specific**: `smtpserver`, `smtpport`, `smtpssl`, `smtpuser`, `smtpuserpwd`, `smtpfrom`

For complete documentation, visit the GitHub repository: <https://github.com/ken-salter/logrotatewin>

### Exit Codes

LogRotate for Windows uses standard exit codes to indicate success or failure. These codes can be used in scripts and scheduled tasks to determine the result of the operation.

| Exit Code | Name | Description |
|-----------|------|-------------|
| 0 | SUCCESS | Successful execution |
| 1 | GENERAL_ERROR | General runtime error or exception |
| 2 | INVALID_ARGUMENTS | Invalid command line arguments |
| 3 | CONFIG_ERROR | Configuration file not found or invalid |
| 4 | NO_FILES_TO_ROTATE | No log files found to process |

#### Example Usage in Scripts

**PowerShell:**
```powershell
logrotate C:\logs\logrotate.conf
if ($LASTEXITCODE -eq 0) {
    Write-Host "Rotation completed successfully"
} elseif ($LASTEXITCODE -eq 4) {
    Write-Host "No files to rotate"
} else {
    Write-Host "Error occurred: exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}
```

**Batch File:**
```batch
logrotate C:\logs\logrotate.conf
if %ERRORLEVEL% EQU 0 (
    echo Rotation completed successfully
) else if %ERRORLEVEL% EQU 4 (
    echo No files to rotate
) else (
    echo Error occurred: exit code %ERRORLEVEL%
    exit /b %ERRORLEVEL%
)
```

## Release Notes

### 0.0.0.20 - 12 Dec 2025
- **Major Feature Update**: Expanded directive coverage from 74% to 91% (63/69 Linux directives)
- **New Directives**: `dateyesterday`, `nodateyesterday`, `datehourago`, `nodatehourago`, `nodateext`
- **New Directives**: `createolddir`, `nocreateolddir` - Control automatic olddir creation
- **New Directives**: `renamecopy`, `norenamecopy` - Cross-device rotation support
- **New Directives**: `nodelaycompress`, `nomissingok`, `minage`, `taboopat`, `ignoreduplicates`
- **Bug Fix**: Extension directive file search pattern corrected for proper age-out
- **Bug Fix**: Original file removal from search results improved for broader patterns
- **Bug Fix**: Postrotate script execution timing fixed for renamecopy
- **Testing**: Added 30+ comprehensive integration test files with 150+ test cases
- **Documentation**: Added DIRECTIVE_COMPARISON.md, TESTING.md, EXIT-CODES.md
- Standardized exit codes for better script integration
- Fixed potential deadlock issues in script execution
- Improved resource disposal with using statements
- Added comprehensive exit code documentation

### 0.0.0.19 - 10 Dec 2025
- Upgraded project to SDK-style format
- Updated to .NET Framework 4.8
- Changed platform target from x86 to AnyCPU
- Modernized build system
- Added Chocolatey package support
- Updated GitHub Actions workflows for automated releases

### 0.0.0.18 - 31 Aug 2018 (beta)
- Getting source code up to date

### 0.0.0.17 - 13 Jan 2017 (beta)
- Merge changes from Github contributors

### 0.0.0.16 - 21 Jul 2015 (beta)
- Additional fix for target filename found by Dom Edwards

### 0.0.0.15 - 10 Apr 2015 (beta)
- Additional fix for target filename containing a number causing exception found by Chris Thorp
- Slight change to handling IOException when trying to truncate log file that is locked by another process
- Add new conf option to allow program to retry opening log file for truncation if it is locked

### 0.0.0.14 - 08 Apr 2015 (beta)
- Fix for date extension not have leading zeroes for month and day found by Alex Faraino
- Fix for target filename containing a number causing exception found by Chris Thorp
- Fix for rotate directive missing causes exception
- Added version number display to logging when verbose mode is set

### 0.0.0.13 - 25 Mar 2015 (beta)
- Fix for truncating files (submitted by Geert De Peuter)

### 0.0.0.12 - 19 Jan 2015 (beta)
- Fix for basic config not rotating (submitted by Matt Richardson)

### 0.0.0.11 - 17 Dec 2013 (beta)
- Fix for sharescripts not executing just pre/post script only once if multiple files specified in conf section as discovered by Marcel Maas

### 0.0.0.10 - 17 Dec 2013 (beta)
- Fixes for prerotate/postrotate scripting as discovered by Marcel Maas

### 0.0.0.9 - 11 Oct 2013 (beta)
- Fix issue with spaces in file paths in the config file

### 0.0.0.8 - 17 Sep 2013 (beta)
- Include changes from "treewitz" to fix the notifempty option

### 0.0.0.7 - 24 May 2013 (beta)
- Changed copyright year to include 2013

### 0.0.0.6 - 24 May 2013 (beta)
- Added ability to wildcard match on folders (i.e. c:\folder\*\*.log matches all subfolders of c:\folder)

### 0.0.0.5 - 18 Dec 2012 (beta)
- Added more error logging

### 0.0.0.4 - 13 Nov 2012 (beta)
- Moved all strings to localization file, spanish translation added, email tested, bug fixes

### 0.0.0.3 - 06 Nov 2012 (beta)
- Code optimizations, bug fixes

### 0.0.0.2 - 31 Oct 2012 (alpha)
- Bug fixes, testing

### 0.0.0.1 - 30 Oct 2012 (pre-alpha)
- Initial code release. Some testing has been done, but coding is still in progress.
