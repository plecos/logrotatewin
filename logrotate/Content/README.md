# LogRotate for Windows

Written by Ken Salter (C) 2012-2025

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

Project homepage: <https://sourceforge.net/projects/logrotatewin/>

## Requirements

- .NET Framework 4.8 or better

## Installation

Run the setup.exe to install.

This installation will copy the executable, README.md, gnu_license.txt, and a sample .conf file to the folder you specify.

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

### 0.0.0.20 - 10 Dec 2025
- Upgraded project to SDK-style format
- Updated to .NET Framework 4.8
- Changed platform target from x86 to AnyCPU
- Modernized build system
- Standardized exit codes for better script integration
- Fixed potential deadlock issues in script execution
- Improved resource disposal with using statements
- Added comprehensive exit code documentation
- Updated GitHub Actions workflows for automated releases
- Added Chocolatey package support

### 0.0.0.19 - (Skipped)

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
