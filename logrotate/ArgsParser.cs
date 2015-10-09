// ArgsParser.cs
// Created by MUHAMMAD ABUBAKAR
// Created: 2015-09-19 12:28 PM
// Modified: 2015-10-02 5:09 PM

/*
    LogRotate - rotates, compresses, and mails system logs
    Copyright (C) 2012  Ken Salter

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
*/

#region Imports

using System;
using System.Collections.Generic;

#endregion

namespace Logrotate
{
    internal class ArgsParser
    {
        #region Constructors

        public ArgsParser( string[] args )
        {
            this._bForce = false;
            this._bVerbose = false;
            this._bUsage = false;
            this._bDebug = false;
            this._sAlternateStateFile = "";
            this._sConfigFilePaths = new List<string>();

            this.Parse( args );
        }

        #endregion // Constructors

        #region Private Methods

        void Parse( string[] args )
        {
            bool bWatchForState = false;
            // iterate through the args array
            foreach ( string a in args )
            {
                // if the string starts with a '-' then it is a switch
                if ( a[0] == '-' )
                {
                    switch ( a )
                    {
                        case "-d":
                            this._bDebug = true;
                            this._bVerbose = true;
                            Logging.SetDebug( true );
                            Logging.SetVerbose( true );
                            Logging.Log( Strings.DebugOptionSet );
                            Logging.Log( Strings.VerboseOptionSet );
                            break;
                        case "-f":
                        case "--force":
                            this._bForce = true;
                            Logging.Log( Strings.ForceOptionSet, Logging.LogType.Required );
                            break;
                        case "-?":
                        case "--usage":
                            this._bUsage = true;
                            break;
                        case "-v":
                        case "--verbose":
                            this._bVerbose = true;
                            Logging.SetVerbose( true );
                            Logging.Log( Strings.VerboseOptionSet );
                            break;
                        case "-m":
                        case "--mail":
                            Logging.Log( Strings.MailOptionSet, Logging.LogType.Error );
                            break;
                        case "-s":
                        case "--state":
                            bWatchForState = true;
                            break;
                        default:
                            // no match, so print an error
                            Logging.Log( Strings.UnknownCmdLineArg + ": " + a, Logging.LogType.Error );
                            Environment.Exit( 1 );
                            break;
                    }
                }
                else
                {
                    if ( bWatchForState )
                    {
                        this._sAlternateStateFile = a;
                        Logging.Log( Strings.AlternateStateFile + " " + a, Logging.LogType.Verbose );
                        bWatchForState = false;
                    }
                    else
                    {
                        // otherwise, it is the path to a config file or folder containing config files
                        Logging.Log( a + " " + Strings.AddingConfigFile, Logging.LogType.Verbose );
                        this._sConfigFilePaths.Add( a );
                    }
                }
            }
        }

        #endregion // Private Methods

        #region Properties

        public bool Force
        {
            get { return this._bForce; }
        }

        public bool Verbose
        {
            get { return this._bVerbose; }
        }

        public bool Usage
        {
            get { return this._bUsage; }
        }

        public bool Debug
        {
            get { return this._bDebug; }
        }

        public string AlternateStateFile
        {
            get { return this._sAlternateStateFile; }
        }


        public List<string> ConfigFilePaths
        {
            get { return this._sConfigFilePaths; }
        }

        #endregion // Properties

        #region Fields

        bool _bForce;
        bool _bVerbose;
        bool _bUsage;
        bool _bDebug;
        string _sAlternateStateFile;
        List<string> _sConfigFilePaths;

        #endregion // Fields
    }
}