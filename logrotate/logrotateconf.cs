// LogrotateConf.cs
// Created by MUHAMMAD ABUBAKAR
// Created: 2015-09-19 12:26 PM
// Modified: 2015-10-02 10:40 AM

#region Imports

using System;
using System.Collections.Generic;

#endregion

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

namespace Logrotate
{
    internal class LogrotateConf
    {
        #region Constructors

        /// <summary>
        ///     Default constructor
        /// </summary>
        public LogrotateConf()
        {
        }

        /// <summary>
        ///     This constructor will create a logrotateconf object as a copy of another one
        /// </summary>
        /// <param name="m_source">The source logrotateconf object to copy</param>
        public LogrotateConf( LogrotateConf m_source )
        {
            this.bcompress = m_source.bcompress;
            //scompresscmd = m_source.scompresscmd;
            //suncompressedcmd = m_source.suncompressedcmd;
            this.scompressext = m_source.scompressext;
            //scompressoptions = m_source.scompressoptions;
            this.bcopy = m_source.bcopy;
            this.bcopytruncate = m_source.bcopytruncate;
            this.bcreate = m_source.bcreate;
            this.bdaily = m_source.bdaily;
            this.bdateext = m_source.bdateext;
            this.sdateformat = m_source.sdateformat;
            this.bdelaycompress = m_source.bdelaycompress;
            this.bifempty = m_source.bifempty;
            this.smail = m_source.smail;
            this.iminsize = m_source.iminsize;
            this.imaxage = m_source.imaxage;
            this.bmissingok = m_source.bmissingok;
            this.bmonthly = m_source.bmonthly;
            this.solddir = m_source.solddir;
            this.spostrotate = m_source.spostrotate;
            this.sprerotate = m_source.sprerotate;
            this.sfirstaction = m_source.sfirstaction;
            this.slastaction = m_source.slastaction;
            this.irotate = m_source.irotate;
            this.lsize = m_source.lsize;
            this.bsharedscripts = m_source.bsharedscripts;
            this.istart = m_source.istart;
            this.stabooext = m_source.stabooext;
            this.bweekly = m_source.bweekly;
            this.byearly = m_source.byearly;
            this.bshred = m_source.bshred;
            this.ishredcycles = m_source.ishredcycles;
            this.bmaillast = m_source.bmaillast;
            this.ssmtpserver = m_source.ssmtpserver;
            this.ismtpport = m_source.ismtpport;
            this.bsmtpssl = m_source.bsmtpssl;
            this.ssmtpuser = m_source.ssmtpuser;
            this.ssmtpuserpwd = m_source.ssmtpuserpwd;
            this.sinclude = m_source.sinclude;
            this.stabooext = m_source.stabooext;
            this.ssmtpfrom = m_source.ssmtpfrom;
        }

        #endregion // Constructors

        #region Public Methods

        /// <summary>
        ///     Parses the supplied line and extracts directives and options
        /// </summary>
        /// <param name="line">A string containing the line to parse</param>
        /// <param name="bDebug">Boolean indicating if logrotate is running in debug mode</param>
        /// <returns>True if a directive was found, false if no directive found</returns>
        public bool Parse( string line, bool bDebug )
        {
            string[] split = line.Split( ' ' );


            // if we are currently inside of a postrotate,prerotate,lastaction, or firstaction block
            // look for the endscript directive, otherwise add the line to the array for the appropriate block type
            if ( this.bpostrotate || this.bprerotate || this.blastaction || this.bfirstaction )
            {
                if ( split[0] == "endscript" )
                {
                    this.bpostrotate = this.bprerotate = this.blastaction = this.bfirstaction = false;
                }
                else
                {
                    if ( this.bpostrotate )
                    {
                        this.ParsePostRotate( line );
                    }
                    if ( this.bprerotate )
                    {
                        this.ParsePreRotate( line );
                    }
                    if ( this.blastaction )
                    {
                        this.ParseLastAction( line );
                    }
                    if ( this.bfirstaction )
                    {
                        this.ParseFirstAction( line );
                    }
                }
                return true;
            }

            switch ( split[0] )
            {
                case "compress":
                    this.bcompress = true;
                    this.PrintDebug( split[0], this.bcompress.ToString(), bDebug );
                    break;
                case "nocompress":
                    this.bcompress = false;
                    this.PrintDebug( split[0], this.bcompress.ToString(), bDebug );
                    break;
                case "copy":
                    this.bcopy = true;
                    this.PrintDebug( split[0], this.bcopy.ToString(), bDebug );
                    break;
                case "nocopy":
                    this.bcopy = false;
                    this.PrintDebug( split[0], this.bcopy.ToString(), bDebug );
                    break;
                case "copytruncate":
                    this.bcopytruncate = true;
                    this.PrintDebug( split[0], this.bcopytruncate.ToString(), bDebug );
                    break;
                case "nocopytruncate":
                    this.bcopytruncate = false;
                    this.PrintDebug( split[0], this.bcopytruncate.ToString(), bDebug );
                    break;
                case "create":
                    this.bcreate = true;
                    this.PrintDebug( split[0], this.bcreate.ToString(), bDebug );
                    break;
                case "nocreate":
                    this.bcreate = false;
                    this.PrintDebug( split[0], this.bcreate.ToString(), bDebug );
                    break;
                case "daily":
                    this.bdaily = true;
                    this.PrintDebug( split[0], this.bdaily.ToString(), bDebug );
                    break;
                case "delaycompress":
                    this.bdelaycompress = true;
                    this.PrintDebug( split[0], this.bdelaycompress.ToString(), bDebug );
                    break;
                case "ifempty":
                    this.bifempty = true;
                    this.PrintDebug( split[0], this.bifempty.ToString(), bDebug );
                    break;
                 case "notifempty":
                    this.bnotifempty = true;
                    this.PrintDebug( split[0], this.bifempty.ToString(), bDebug );
                    break;
                case "missingok":
                    this.bmissingok = true;
                    this.PrintDebug( split[0], this.bmissingok.ToString(), bDebug );
                    break;
                case "monthly":
                    this.bmonthly = true;
                    this.PrintDebug( split[0], this.bmonthly.ToString(), bDebug );
                    break;
                case "sharedscripts":
                    this.bsharedscripts = true;
                    this.bcreate = true;
                    this.PrintDebug( split[0], this.bsharedscripts.ToString(), bDebug );
                    break;
                case "nosharedscripts":
                    this.bsharedscripts = false;
                    this.PrintDebug( split[0], this.bsharedscripts.ToString(), bDebug );
                    break;
                case "weekly":
                    this.bweekly = true;
                    this.PrintDebug( split[0], this.bweekly.ToString(), bDebug );
                    break;
                case "yearly":
                    this.byearly = true;
                    this.PrintDebug( split[0], this.byearly.ToString(), bDebug );
                    break;
                case "compresscmd":
                    //scompresscmd = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    this.PrintDebug( split[0], Strings.UnknownDirective, bDebug );
                    break;
                case "uncompresscmd":
                    //suncompressedcmd = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    this.PrintDebug( split[0], Strings.UnknownDirective, bDebug );
                    break;
                case "compressext":
                    this.scompressext = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "scompressoptions":
                    //scompressoptions = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    this.PrintDebug( split[0], Strings.UnknownDirective, bDebug );
                    break;
                case "dateformat":
                    this.sdateformat = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "mail":
                    this.smail = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "nomail":
                    this.smail = "";
                    this.PrintDebug( split[0], "", bDebug );
                    break;
                case "maxage":
                    this.imaxage = Convert.ToInt32( split[1] );
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "olddir":
                    this.solddir = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "noolddir":
                    this.solddir = "";
                    this.PrintDebug( split[0], "", bDebug );
                    break;
                case "rotate":
                    this.irotate = Convert.ToInt32( split[1] );
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "minsize":
                    // the size can be for following:  100, 100k, 100m, 100g
                    string minsize_type = split[1].Substring( split[1].Length - 1, 1 ).ToUpper();
                    if ( Char.IsNumber( minsize_type, 0 ) )
                    {
                        this.iminsize = Convert.ToInt64( split[1] );
                    }
                    else
                    {
                        if ( minsize_type == "K" )
                        {
                            this.iminsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1024;
                        }
                        else if ( minsize_type == "M" )
                        {
                            this.iminsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1048576;
                        }
                        else if ( minsize_type == "G" )
                        {
                            this.iminsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1073741824;
                        }
                        else
                        {
                            Logging.Log( Strings.UnknownSizeType + " " + line, Logging.LogType.Error );
                            return false;
                        }
                    }

                    this.PrintDebug( split[0], this.lsize.ToString(), bDebug );

                    break;
                case "shred":
                    this.bshred = true;
                    this.PrintDebug( split[0], this.bshred.ToString(), bDebug );
                    break;
                case "noshred":
                    this.bshred = false;
                    this.PrintDebug( split[0], this.bshred.ToString(), bDebug );
                    break;
                case "shredcycles":
                    this.ishredcycles = Convert.ToInt32( split[1] );
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "start":
                    this.istart = Convert.ToInt32( split[1] );
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "postrotate":
                    this.bpostrotate = true;
                    this.PrintDebug( split[0], this.bpostrotate.ToString(), bDebug );
                    break;
                case "prerotate":
                    this.bprerotate = true;
                    this.PrintDebug( split[0], this.bprerotate.ToString(), bDebug );
                    break;
                case "firstaction":
                    this.bfirstaction = true;
                    this.PrintDebug( split[0], this.bfirstaction.ToString(), bDebug );
                    break;
                case "lastaction":
                    this.blastaction = true;
                    this.PrintDebug( split[0], this.blastaction.ToString(), bDebug );
                    break;

                case "size":
                    // the size can be for following:  100, 100k, 100m, 100g
                    string size_type = split[1].Substring( split[1].Length - 1, 1 ).ToUpper();
                    if ( Char.IsNumber( size_type, 0 ) )
                    {
                        this.lsize = Convert.ToInt64( split[1] );
                    }
                    else
                    {
                        if ( size_type == "K" )
                        {
                            this.lsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1024;
                        }
                        else if ( size_type == "M" )
                        {
                            this.lsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1048576;
                        }
                        else if ( size_type == "G" )
                        {
                            this.lsize = Convert.ToInt64( split[1].Substring( 0, split[1].Length - 1 ) ) * 1073741824;
                        }
                        else
                        {
                            Logging.Log( Strings.UnknownSizeType + " " + line, Logging.LogType.Error );
                            return false;
                        }
                    }

                    this.PrintDebug( split[0], this.lsize.ToString(), bDebug );
                    break;
                case "mailfirst":
                    this.bmaillast = false;
                    this.PrintDebug( split[0], this.bmaillast.ToString(), bDebug );
                    break;
                case "maillast":
                    this.bmaillast = true;
                    this.PrintDebug( split[0], this.bmaillast.ToString(), bDebug );
                    break;
                case "smtpserver":
                    this.ssmtpserver = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "smtpport":
                    this.ismtpport = Convert.ToInt32( split[1] );
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "smtpssl":
                    this.bsmtpssl = true;
                    this.PrintDebug( split[0], this.bsmtpssl.ToString(), bDebug );
                    break;
                case "nosmtpssl":
                    this.bsmtpssl = false;
                    this.PrintDebug( split[0], this.bsmtpssl.ToString(), bDebug );
                    break;
                case "smtpuser":
                    this.ssmtpuser = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "smtpfrom":
                    this.ssmtpfrom = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                case "smtpuserpwd":
                    this.ssmtpuserpwd = split[1];
                    this.PrintDebug( split[0], "****", bDebug );
                    break;
                case "dateext":
                    this.bdateext = true;
                    this.PrintDebug( split[0], this.bdateext.ToString(), bDebug );
                    break;
                case "tabooext":
                    int taboo_start_idx = 2;
                    if ( split[1] != "+" )
                    {
                        taboo_start_idx = 1;
                        Array.Resize( ref this.stabooext, 0 );
                    }
                    for ( int j = taboo_start_idx; j < split.Length; j++ )
                    {
                        Array.Resize( ref this.stabooext, this.stabooext.Length + 1 );
                        this.stabooext[this.stabooext.Length - 1] = split[j];
                    }
                    break;
                case "include":
                    this.sinclude = split[1];
                    this.PrintDebug( split[0], split[1], bDebug );
                    break;
                default:
                    Logging.Log( Strings.UnknownDirective + " " + line, Logging.LogType.Error );
                    return false;
            }
            return true;
        }

        public void Clear_PreRotate()
        {
            this.sprerotate.Clear();
            this.sprerotate = null;
        }

        public void Clear_PostRotate()
        {
            this.spostrotate.Clear();
            this.spostrotate = null;
        }

        public void Increment_ProcessCount()
        {
            this.process_count++;
        }

        public void Decrement_ProcessCount()
        {
            this.process_count--;
        }

        #endregion // Public Methods

        #region Private Methods

        void ParseFirstAction( string line )
        {
            if ( this.slastaction == null )
            {
                this.slastaction = new List<string>();
            }

            this.slastaction.Add( line );
        }

        void ParseLastAction( string line )
        {
            if ( this.sfirstaction == null )
            {
                this.sfirstaction = new List<string>();
            }

            this.sfirstaction.Add( line );
        }

        void ParsePreRotate( string line )
        {
            if ( this.sprerotate == null )
            {
                this.sprerotate = new List<string>();
            }

            this.sprerotate.Add( line );
        }

        void ParsePostRotate( string line )
        {
            if ( this.spostrotate == null )
            {
                this.spostrotate = new List<string>();
            }

            this.spostrotate.Add( line );
        }

        #endregion // Private Methods

        #region Properties

        public int ProcessCount
        {
            get { return this.process_count; }
        }

        public bool Compress
        {
            get { return this.bcompress; }
        }

        public string CompressExt
        {
            get { return this.scompressext; }
        }

        public bool DelayCompress
        {
            get { return this.bdelaycompress; }
        }

        public bool MissingOK
        {
            get { return this.bmissingok; }
        }

        public bool IfEmpty
        {
            get { return this.bifempty; }
        }
        public bool NotIfEmpty
        {
            get { return this.bnotifempty; }
        }

        public long Size
        {
            get { return this.lsize; }
        }

        public bool Copy
        {
            get { return this.bcopy; }
        }

        public bool DateExt
        {
            get { return this.bdateext; }
        }

        public List<string> PreRotate
        {
            get { return this.sprerotate; }
        }

        public List<string> PostRotate
        {
            get { return this.spostrotate; }
        }

        public List<string> FirstAction
        {
            get { return this.sfirstaction; }
        }

        public List<string> LastAction
        {
            get { return this.slastaction; }
        }

        public string DateFormat
        {
            get { return this.sdateformat; }
        }

        void PrintDebug( string line, string value, bool bDebug )
        {
            Logging.Log( Strings.Setting + " " + line + Strings.To + value, Logging.LogType.Debug );
        }

        public int Start
        {
            get { return this.istart; }
        }

        public string OldDir
        {
            get { return this.solddir; }
        }

        public bool CopyTruncate
        {
            get { return this.bcopytruncate; }
        }

        public bool Create
        {
            get { return this.bcreate; }
        }

        public long MinSize
        {
            get { return this.iminsize; }
        }

        public bool Daily
        {
            get { return this.bdaily; }
        }

        public bool Monthly
        {
            get { return this.bmonthly; }
        }

        public bool Yearly
        {
            get { return this.byearly; }
        }

        public bool Shred
        {
            get { return this.bshred; }
        }

        public int ShredCycles
        {
            get { return this.ishredcycles; }
        }

        public bool MailLast
        {
            get { return this.bmaillast; }
        }

        public int SMTPPort
        {
            get { return this.ismtpport; }
        }

        public string SMTPServer
        {
            get { return this.ssmtpserver; }
        }

        public string SMTPUserName
        {
            get { return this.ssmtpuser; }
        }

        public string SMTPUserPassword
        {
            get { return this.ssmtpuserpwd; }
        }

        public string MailAddress
        {
            get { return this.smail; }
        }

        public string MailFrom
        {
            get { return this.ssmtpfrom; }
        }

        public bool SMTPUseSSL
        {
            get { return this.bsmtpssl; }
        }

        public int MaxAge
        {
            get { return this.imaxage; }
        }

        public bool SharedScripts
        {
            get { return this.bsharedscripts; }
        }

        public bool Weekly
        {
            get { return this.bweekly; }
        }

        public int Rotate
        {
            get { return this.irotate; }
        }

        public string Include
        {
            get { return this.sinclude; }
        }

        public string[] TabooList
        {
            get { return this.stabooext; }
        }

        #endregion // Properties

        #region Fields

        bool bcompress = true;
        //private string scompresscmd = "gzip";
        //private string suncompressedcmd = "gunzip";
        string scompressext = "gz";
        //private string scompressoptions = "-9";
        bool bcopy;
        bool bcopytruncate;
        bool bcreate;
        bool bdaily;
        bool bdateext;
        string sdateformat = "-%Y%m%d%H%M%S";
        bool bdelaycompress;
        bool bifempty = false;
        bool bnotifempty = true;
        string smail = "";
        string ssmtpserver = "";
        int ismtpport = 25;
        bool bsmtpssl;
        string ssmtpuser = "";
        string ssmtpuserpwd = "";
        string ssmtpfrom = Strings.ProgramName + "@" + Environment.MachineName;
        string sinclude = "";
        long iminsize;
        int imaxage;
        bool bmissingok;
        bool bmonthly;
        bool bmaillast = true;
        string solddir = "";
        List<string> spostrotate;
        List<string> sprerotate;
        List<string> sfirstaction;
        List<string> slastaction;
        int irotate;
        long lsize;
        bool bsharedscripts;
        int istart = 1;
        string[] stabooext = {".swp"};
        bool bweekly;
        bool byearly;
        bool bshred;
        int ishredcycles = 3;


        bool bpostrotate;
        bool bprerotate;
        bool bfirstaction;
        bool blastaction;

        int process_count;

        #endregion // Fields
    }
}