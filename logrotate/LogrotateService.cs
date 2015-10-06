// LogrotateService.cs
// Created by MUHAMMAD ABUBAKAR
// Created: 2015-09-21 12:32 PM
// Modified: 2015-10-09 3:27 PM

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
using System.IO;
using System.ServiceProcess;
using System.Text.RegularExpressions;
using System.Threading;

#endregion

namespace Logrotate
{
    partial class LogrotateService : ServiceBase
    {
        #region Constructor

        public LogrotateService()
        {
            InitializeComponent();
        }

        #endregion // Constructor

        #region Overridden Methods

        protected override void OnStart( string[] args )
        {
            this._settingsFile = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "service.ini" );
            ThreadPool.QueueUserWorkItem( this.Init, null );
        }

        protected override void OnStop()
        {
            if ( this._timer != null )
            {
                this._timer.Change( Timeout.Infinite, Timeout.Infinite );
            }
        }

        #endregion // Overridden Methods

        #region Private Methods

        void Init( object state )
        {
            try
            {
                this._genericConfigFile = Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "logrotate.conf" );
                this.ParseSettings();
                this._rotater = new Rotater( this._args );
                this._timer = new Timer( this.Rotate, null, TimeSpan.FromSeconds( this._interval ),
                    TimeSpan.FromSeconds( this._interval ) );
            }
            catch ( Exception ex )
            {
                EventLog.WriteEntry( this.GetBaseCause( ex ) );
            }
        }

        void ParseSettings()
        {
            List<string> args = new List<string>();

            using ( StreamReader sr = new StreamReader( this._settingsFile ) )
            {
                string line;
                while ( !sr.EndOfStream )
                {
                    line = sr.ReadLine();
                    if ( string.IsNullOrEmpty( line ) || line.StartsWith( ";" ) )
                    {
                        continue;
                    }

                    line = line.Trim();

                    if ( line.StartsWith( "args" ) )
                    {
                        line = line.Substring( line.IndexOf( '=' ) + 1 );
                        Regex rex = new Regex( "\"(?:[^\\\"]|\\.)*\"", RegexOptions.Compiled );
                        Match m = rex.Match( line );

                        while ( m.Success )
                        {
                            if ( !string.IsNullOrEmpty( m.Value ) )
                            {
                                args.Add( m.Value.Trim( '"', ' ' ) );
                                line = line.Replace( m.Value, "" );
                            }
                            m = m.NextMatch();
                        }

                        string[] otherArgs = line.Trim().Split( (char[]) null, StringSplitOptions.RemoveEmptyEntries );
                        if ( otherArgs.Length > 0 )
                        {
                            args.AddRange( otherArgs );
                        }
                    }
                    else if ( line.StartsWith( "interval" ) )
                    {
                        line = line.Substring( line.IndexOf( '=' ) + 1 ).Trim();
                        if ( !string.IsNullOrEmpty( line ) )
                        {
                            this._interval = Convert.ToInt32( line );
                        }
                    }
                }
            }

            if ( !args.Contains( this._genericConfigFile ) )
            {
                args.Add( this._genericConfigFile );
            }

            this._args = args.ToArray();

            if ( this._interval < 1 )
            {
                this._interval = 5;
            }
        }

        void Rotate( object state )
        {
            try
            {
                this._rotater.Process();
            }
            catch ( Exception ex )
            {
                EventLog.WriteEntry( this.GetBaseCause( ex ) );
            }
        }

        string GetBaseCause( Exception ex )
        {
            var inEx = ex.GetBaseException();
            string error = null;
            error = string.Format( "{1}{0}{2}", Environment.NewLine, inEx.Message, inEx.StackTrace );
            return error;
        }

        #endregion // Private Methods

        #region Fields

        Rotater _rotater;
        Timer _timer;
        string _settingsFile;
        string[] _args;
        int _interval;
        string _genericConfigFile;

        #endregion // Fields
    }
}