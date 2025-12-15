# logrotatewin
This is a Windows implementation of the logrotate utility found in Linux platforms. The goal is to use the same command line parameters and files as the Linux version.

LogRotate for Windows
Written by Ken Salter (C) 2012-2025

[![Download LogRotateWin](https://img.shields.io/sourceforge/dt/logrotatewin.svg)](https://sourceforge.net/projects/logrotatewin/files/latest/download)

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

Program description:

This is a port of the logrotate utility available for Linux. See the Wiki for more notes.

**Feature Coverage**: Implements **91%** (63/69) of Linux logrotate directives. See [DIRECTIVE_COMPARISON.md](DIRECTIVE_COMPARISON.md) for complete directive coverage details.

https://sourceforge.net/projects/logrotatewin/

## Requirements

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

## Testing

The project includes comprehensive integration tests covering all major directives and functionality:

```bash
dotnet test
```

For more information about testing, see [TESTING.md](TESTING.md).

## Documentation

- **[DIRECTIVE_COMPARISON.md](DIRECTIVE_COMPARISON.md)** - Complete comparison of implemented directives vs Linux logrotate
- **[TESTING.md](TESTING.md)** - Comprehensive testing documentation
- **[EXIT-CODES.md](EXIT-CODES.md)** - Exit code reference

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
/var/log/myapp/*.log {
    daily
    rotate 7
    compress
    delaycompress
    missingok
    notifempty
    create 0644
    postrotate
        echo "Logs rotated" >> /var/log/rotation.log
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
- **Mail**: `mail`, `mailfirst`, `maillast`
- **Configuration**: `include`, `tabooext`, `taboopat`

For a complete list of all supported directives, see [DIRECTIVE_COMPARISON.md](DIRECTIVE_COMPARISON.md).

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
