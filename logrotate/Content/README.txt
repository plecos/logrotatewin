LogRotate for Windows
Written by Ken Salter (C) 2012-2016

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

.NET Framework v4.5 or better

Installation:

Run the setup.exe to install.  

This installation will copy the executable, README.txt, gnu_license.txt, and a sample .conf file to the folder you specify.

Release Notes:

0.0.0.17- 13 Jan 2017 (beta) - merge changes from Github contributors
                               changed default .NET Framework to 4.5

0.0.0.16- 21 Jul 2015 (beta) - additional fix for target filename found by Dom Edwards

0.0.0.15- 10 Apr 2015 (beta) - additional fix for target filename containing a number causing exception found by Chris Thorp
                             - slight change to handling IOException when trying to truncate log file that is locked by another process
						     - add new conf option to allow program to retry opening log file for truncation if it is locked

0.0.0.14- 08 Apr 2015 (beta) - fix for date extenstion not have leading zeroes for month and day found by Alex Faraino
                             - fix for target filename containing a number causing exception found by Chris Thorp
							 - fix for rotate directive missing causes exception
							 - added version number display to logging when verbose mode is set

0.0.0.13- 25 Mar 2015 (beta) - fix for truncating files (submitted by Geert De Peuter)

0.0.0.12- 19 Jan 2015 (beta) - fix for basic config not rotating (submitted by Matt Richardson)

0.0.0.11- 17 Dec 2013 (beta) - fix for sharescripts not executing just pre/post script only once if multiple files specified in conf section as discovered by Marcel Maas

0.0.0.10- 17 Dec 2013 (beta) - fixes for prerotate/postrotate scripting as discovered by Marcel Maas

0.0.0.9 - 11 Oct 2013 (beta) - fix issue with spaces in file paths in the config file

0.0.0.8 - 17 Sep 2013 (beta) - include changes from "treewitz" to fix the notifempty option

0.0.0.7 - 24 May 2013 (beta) - changed copyright year to include 2013

0.0.0.6 - 24 May 2013 (beta) - addeed ability to wildcard match on folders (i.e. c:\folder\*\*.log matches all subfolders of c:\folder)

0.0.0.5 - 18 Dec 2012 (beta) - added more error logging

0.0.0.4 - 13 Nov 2012 (beta) - moved all strings to localization file, spanish translation added, email tested, bug fixes

0.0.0.3 - 06 Nov 2012 (beta) - code optimizations, bug fixes

0.0.0.2 - 31 Oct 2012 (alpha) - bug fixes, testing

0.0.0.1 - 30 Oct 2012 (pre-alpha) - initial code release.  Some testing has been done, but coding is still in progress.  




