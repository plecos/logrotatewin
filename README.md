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
