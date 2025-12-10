# logrotatewin
This is a Windows implementation of the logrotate utility found in Linux platforms. The goal is to use the same command line parameters and files as the Linux version.

LogRotate for Windows
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

Program description:

This is a port of the logrotate utility available for Linux.  See the Wiki for more notes.

https://sourceforge.net/projects/logrotatewin/

Requirements:

.NET Framework 4.8 or better

## Installation

### Chocolatey (Recommended)

The easiest way to install LogRotate for Windows is using Chocolatey:

```powershell
choco install logrotatewin
```

After installation, the `logrotate` command will be available in your PATH.

### Manual Installation

Download the latest release from the [Releases page](https://github.com/ken-salter/logrotatewin/releases) and extract to your desired location.

## Building

The project uses SDK-style project format and can be built using:
- Visual Studio 2019 or later
- Visual Studio Code with C# extension
- .NET SDK 6.0 or later (for tooling)

Build from command line:
```
dotnet build
```

Build Release configuration:
```
dotnet build -c Release
```

## Usage

```
logrotate [options] <configfile>
```

### Options

- `-d, --debug` - Debug mode (verbose output, no actual rotation)
- `-f, --force` - Force rotation even if not needed
- `-v, --verbose` - Verbose output
- `-s, --state <file>` - Use alternate state file
- `-?, --usage, --help` - Show usage information

### Exit Codes

LogRotate for Windows uses standard exit codes to indicate success or failure:

| Exit Code | Name | Description |
|-----------|------|-------------|
| 0 | SUCCESS | Successful execution |
| 1 | GENERAL_ERROR | General runtime error or exception |
| 2 | INVALID_ARGUMENTS | Invalid command line arguments |
| 3 | CONFIG_ERROR | Configuration file not found or invalid |
| 4 | NO_FILES_TO_ROTATE | No log files found to process |

These exit codes can be used in scripts to determine the result of the operation:

```powershell
logrotate myconfig.conf
if ($LASTEXITCODE -eq 0) {
    Write-Host "Rotation completed successfully"
} elseif ($LASTEXITCODE -eq 4) {
    Write-Host "No files to rotate"
} else {
    Write-Host "Error occurred: exit code $LASTEXITCODE"
}
```
