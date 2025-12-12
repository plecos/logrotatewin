using System;
using System.Collections.Generic;

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

namespace logrotate
{
    class logrotateconf
    {
        #region Private consts
        private const string sgzipdefaultcompressext = "gz";
        #endregion

        #region Private variables
        private bool bgzipcompress = true;
        private string scompresscmd = ""; // Empty = use built-in GZipStream
        private string suncompresscmd = ""; // Empty = use built-in GZipStream
        private string scompressext;
        private string scompressoptions = ""; // Default options for external compression
        private bool bcopy = false;
        private bool bcopytruncate = false;
        private bool brenamecopy = false;
        private bool bcreate = false;
        private int iminutes = 0;
        private bool bhourly = false;
        private bool bdaily = false;
        private bool bdateext = false;
        private string sdateformat = "-%Y%m%d";
        private bool bdateyesterday = false;
        private bool bdatehourago = false;
        private bool bdelaycompress = false;
        private bool bifempty = true;
        private string smail = "";
        private string ssmtpserver = "";
        private int ismtpport = 25;
        private bool bsmtpssl = false;
        private string ssmtpuser = "";
        private string ssmtpuserpwd = "";
        private string ssmtpfrom = Strings.ProgramName + "@" + Environment.MachineName;
        private string sinclude = "";
        private long iminsize = 0;
        private long imaxsize = 0;
        private int imaxage = 0;
        private int iminage = 0;
        private bool bmissingok = false;
        private bool bignoreduplicates = false;
        private bool bmonthly = false;
        private int imonthday = 0; // 0 = not specified, 1-31 = specific day
        private bool bmaillast = true;
        private string solddir = "";
        private bool bcreateolddir = true; // default is to create olddir if it doesn't exist
        private List<string> spostrotate = null;
        private List<string> sprerotate = null;
        private List<string> sfirstaction = null;
        private List<string> slastaction = null;
        private List<string> spreremove = null;
        private int irotate = 0;
        private long lsize = 0;
        private bool bsharedscripts = false;
        private int istart = 1;
        private string[] stabooext = { ".swp" };
        private string[] staboopat = null;
        private bool bweekly = false;
        private int iweekday = -1; // -1 = not specified, 0-6 = Sunday-Saturday
        private bool byearly = false;
        private bool bshred = false;
        private int ishredcycles = 3;
        private string sextension = "";
        private string saddextension = "";
        private bool bretry_logfileopen = false;
        private int inumretry_logfileopen = 0;
        private int inumms_retry_logfileopen = 1000;
        

        private bool bpostrotate = false;
        private bool bprerotate = false;
        private bool bfirstaction = false;
        private bool blastaction = false;
        private bool bpreremove = false;

        private int process_count = 0;

        #endregion

        #region Public properties

        public int ProcessCount
        {
            get { return process_count; }
        }

        public bool Compress
        {
            get { return bgzipcompress; }
        }

        public string CompressExt
        {
            get { return scompressext; }
        }
        public string CompressCmd
        {
            get { return scompresscmd; }
        }
        public string UncompressCmd
        {
            get { return suncompresscmd; }
        }
        public string CompressOptions
        {
            get { return scompressoptions; }
        }
        public bool DelayCompress
        {
            get { return bdelaycompress; }
        }

        public bool MissingOK
        {
            get { return bmissingok; }
        }

        public bool IfEmpty
        {
            get { return bifempty; }
        }

        public long Size
        {
            get { return lsize; }
        }

        public bool Copy
        {
            get { return bcopy; }
        }

        public bool DateExt
        {
            get { return bdateext; }
        }

        public bool DateYesterday
        {
            get { return bdateyesterday; }
        }

        public bool DateHourAgo
        {
            get { return bdatehourago; }
        }

        public List<string> PreRotate
        {
            get { return sprerotate; }
        }

        public List<string> PostRotate
        {
            get { return spostrotate; }
        }
        public List<string> FirstAction
        {
            get { return sfirstaction; }
        }
        public List<string> LastAction
        {
            get { return slastaction; }
        }
        public List<string> PreRemove
        {
            get { return spreremove; }
        }

        public string DateFormat
        {
            get { return sdateformat; }
        }

        private void PrintDebug(string line, string value, bool bDebug)
        {
            if (bDebug)
            {
                Logging.Log(Strings.Setting + " " + line + Strings.To + value, Logging.LogType.Debug);
            }
        }

        public int Start
        {
            get { return istart; }
        }

        public string OldDir
        {
            get { return solddir; }
        }

        public bool CreateOldDir
        {
            get { return bcreateolddir; }
        }

        public bool CopyTruncate
        {
            get { return bcopytruncate; }
        }

        public bool RenameCopy
        {
            get { return brenamecopy; }
        }

        public bool Create
        {
            get { return bcreate; }
        }

        public long MinSize
        {
            get { return iminsize; }
        }

        public long MaxSize
        {
            get { return imaxsize; }
        }

        public int Minutes
        {
            get { return iminutes; }
        }
        public bool Hourly
        {
            get { return bhourly; }
        }
        public bool Daily
        {
            get { return bdaily; }
        }
        public bool Weekly
        {
            get { return bweekly; }
        }
        public int Weekday
        {
            get { return iweekday; }
        }
        public bool Monthly
        {
            get { return bmonthly; }
        }
        public int MonthDay
        {
            get { return imonthday; }
        }
        public bool Yearly
        {
            get { return byearly; }
        }
        public bool Shred
        {
            get { return bshred; }
        }
        public int ShredCycles
        {
            get { return ishredcycles; }
        }
        public string Extension
        {
            get { return sextension; }
        }
        public string AddExtension
        {
            get { return saddextension; }
        }
        public bool MailLast
        {
            get { return bmaillast; }
        }
        public int SMTPPort
        {
            get { return ismtpport; }
        }
        public string SMTPServer
        {
            get { return ssmtpserver; }
        }
        public string SMTPUserName
        {
            get { return ssmtpuser; }
        }
        public string SMTPUserPassword
        {
            get { return ssmtpuserpwd; }
        }
        
        public string MailAddress
        {
            get { return smail; }
        }
        public string MailFrom
        {
            get { return ssmtpfrom; }
        }
        public bool SMTPUseSSL
        {
            get { return bsmtpssl; }
        }
        public int MaxAge
        {
            get { return imaxage; }
        }
        public int MinAge
        {
            get { return iminage; }
        }
        public bool IgnoreDuplicates
        {
            get { return bignoreduplicates; }
        }
        public bool SharedScripts
        {
            get { return bsharedscripts; }
        }
        public int Rotate
        {
            get { return irotate; }
        }
        public string Include
        {
            get { return sinclude; }
        }
        public string[] TabooList
        {
            get { return stabooext; }
        }
        public string[] TabooPatList
        {
            get { return staboopat; }
        }
        public bool LogFileOpen_Retry
        {
            get { return bretry_logfileopen; }
        }
        public int LogFileOpen_NumberRetryAttempts
        {
            get { return inumretry_logfileopen; }
        }
        public int LogFileOpen_MSBetweenRetryAttempts
        {
            get { return inumms_retry_logfileopen; }
        }
        #endregion

        /// <summary>
        /// Default constructor
        /// </summary>
        public logrotateconf()
        {
        }

        /// <summary>
        /// This constructor will create a logrotateconf object as a copy of another one
        /// </summary>
        /// <param name="m_source">The source logrotateconf object to copy</param>
        public logrotateconf(logrotateconf m_source)
        {
            bgzipcompress = m_source.bgzipcompress;
            scompresscmd = m_source.scompresscmd;
            suncompresscmd = m_source.suncompresscmd;
            scompressoptions = m_source.scompressoptions;
            scompressext = m_source.scompressext;
            bcopy = m_source.bcopy;
            bcopytruncate = m_source.bcopytruncate;
            brenamecopy = m_source.brenamecopy;
            bcreate = m_source.bcreate;
            iminutes = m_source.iminutes;
            bhourly = m_source.bhourly;
            bdaily = m_source.bdaily;
            bdateext = m_source.bdateext;
            sdateformat = m_source.sdateformat;
            bdateyesterday = m_source.bdateyesterday;
            bdatehourago = m_source.bdatehourago;
            bdelaycompress = m_source.bdelaycompress;
            bifempty = m_source.bifempty;
            smail = m_source.smail;
            iminsize = m_source.iminsize;
            imaxsize = m_source.imaxsize;
            imaxage = m_source.imaxage;
            iminage = m_source.iminage;
            bmissingok = m_source.bmissingok;
            bignoreduplicates = m_source.bignoreduplicates;
            bmonthly = m_source.bmonthly;
            imonthday = m_source.imonthday;
            solddir = m_source.solddir;
            bcreateolddir = m_source.bcreateolddir;
            spostrotate = m_source.spostrotate;
            sprerotate = m_source.sprerotate;
            sfirstaction = m_source.sfirstaction;
            slastaction = m_source.slastaction;
            spreremove = m_source.spreremove;
            irotate = m_source.irotate;
            lsize = m_source.lsize;
            bsharedscripts = m_source.bsharedscripts;
            istart = m_source.istart;
            stabooext = m_source.stabooext;
            staboopat = m_source.staboopat;
            bweekly = m_source.bweekly;
            iweekday = m_source.iweekday;
            byearly = m_source.byearly;
            bshred = m_source.bshred;
            ishredcycles = m_source.ishredcycles;
            sextension = m_source.sextension;
            saddextension = m_source.saddextension;
            bmaillast = m_source.bmaillast;
            ssmtpserver = m_source.ssmtpserver;
            ismtpport = m_source.ismtpport;
            bsmtpssl = m_source.bsmtpssl;
            ssmtpuser = m_source.ssmtpuser;
            ssmtpuserpwd = m_source.ssmtpuserpwd;
            sinclude = m_source.sinclude;
            stabooext = m_source.stabooext;
            ssmtpfrom = m_source.ssmtpfrom;
            bretry_logfileopen = m_source.bretry_logfileopen;
            inumretry_logfileopen = m_source.inumretry_logfileopen;
            inumms_retry_logfileopen = m_source.inumms_retry_logfileopen;
        }

        /// <summary>
        /// Parses the supplied line and extracts directives and options
        /// </summary>
        /// <param name="line">A string containing the line to parse</param>
        /// <param name="bDebug">Boolean indicating if logrotate is running in debug mode</param>
        /// <returns>True if a directive was found, false if no directive found</returns>
        public bool Parse(string line, bool bDebug)
        {
            string[] split = line.Split(new char[] {' '});


            // if we are currently inside of a postrotate,prerotate,lastaction, or firstaction block
            // look for the endscript directive, otherwise add the line to the array for the appropriate block type
            if ((bpostrotate == true) || (bprerotate == true) || (blastaction == true) || (bfirstaction == true) || (bpreremove == true))
            {
                if (split[0] == "endscript")
                {
                    bpostrotate = bprerotate = blastaction = bfirstaction = bpreremove = false;
                }
                else
                {
                    if (bpostrotate)
                        ParsePostRotate(line);
                    if (bprerotate)
                        ParsePreRotate(line);
                    if (blastaction)
                        ParseLastAction(line);
                    if (bfirstaction)
                        ParseFirstAction(line);
                    if (bpreremove)
                        ParsePreRemove(line);
                }
                return true;
            }

            switch (split[0])
            {
                case "compress":
                    bgzipcompress = true;
                    if (string.IsNullOrEmpty(scompressext))
                    {
                        scompressext = sgzipdefaultcompressext;
                    }
                    PrintDebug(split[0], bgzipcompress.ToString(), bDebug);
                    break;
                case "nocompress":
                    bgzipcompress = false;
                    PrintDebug(split[0], bgzipcompress.ToString(), bDebug);
                    break;
                case "copy":
                    bcopy = true;
                    PrintDebug(split[0], bcopy.ToString(), bDebug);
                    break;
                case "nocopy":
                    bcopy = false;
                    PrintDebug(split[0], bcopy.ToString(), bDebug);
                    break;
                case "copytruncate":
                    bcopytruncate = true;
                    PrintDebug(split[0], bcopytruncate.ToString(), bDebug);
                    break;
                case "nocopytruncate":
                    bcopytruncate = false;
                    PrintDebug(split[0], bcopytruncate.ToString(), bDebug);
                    break;
                case "renamecopy":
                    brenamecopy = true;
                    // renamecopy implies nocopytruncate
                    bcopytruncate = false;
                    PrintDebug(split[0], brenamecopy.ToString(), bDebug);
                    break;
                case "norenamecopy":
                    brenamecopy = false;
                    PrintDebug(split[0], brenamecopy.ToString(), bDebug);
                    break;
                case "create":
                    bcreate = true;
                    PrintDebug(split[0], bcreate.ToString(), bDebug);
                    break;
                case "nocreate":
                    bcreate = false;
                    PrintDebug(split[0], bcreate.ToString(), bDebug);
                    break;
                case "minutes":
                    iminutes = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "hourly":
                    bhourly = true;
                    PrintDebug(split[0], bhourly.ToString(), bDebug);
                    break;
                case "daily":
                    bdaily = true;
                    PrintDebug(split[0], bdaily.ToString(), bDebug);
                    break;
                case "delaycompress":
                    bdelaycompress = true;
                    PrintDebug(split[0], bdelaycompress.ToString(), bDebug);
                    break;
                case "nodelaycompress":
                    bdelaycompress = false;
                    PrintDebug(split[0], bdelaycompress.ToString(), bDebug);
                    break;
                case "ifempty":
                    bifempty = true;
                    PrintDebug(split[0], bifempty.ToString(), bDebug);
                    break;
                case "notifempty":
                    bifempty = false;
                    PrintDebug(split[0], bifempty.ToString(), bDebug);
                    break;
                case "missingok":
                    bmissingok = true;
                    PrintDebug(split[0], bmissingok.ToString(), bDebug);
                    break;
                case "nomissingok":
                    bmissingok = false;
                    PrintDebug(split[0], bmissingok.ToString(), bDebug);
                    break;
                case "ignoreduplicates":
                    bignoreduplicates = true;
                    PrintDebug(split[0], bignoreduplicates.ToString(), bDebug);
                    break;
                case "monthly":
                    bmonthly = true;
                    // Optional parameter: specific day of month (1-31)
                    if (split.Length > 1)
                    {
                        imonthday = Convert.ToInt32(split[1]);
                        PrintDebug(split[0], split[1], bDebug);
                    }
                    else
                    {
                        PrintDebug(split[0], bmonthly.ToString(), bDebug);
                    }
                    break;
                case "sharedscripts":
                    bsharedscripts = true;
                    bcreate = true;
                    PrintDebug(split[0], bsharedscripts.ToString(), bDebug);
                    break;
                case "nosharedscripts":
                    bsharedscripts = false;
                    PrintDebug(split[0], bsharedscripts.ToString(), bDebug);
                    break;
                case "weekly":
                    bweekly = true;
                    // Optional parameter: specific day of week (0-6, Sunday=0)
                    if (split.Length > 1)
                    {
                        iweekday = Convert.ToInt32(split[1]);
                        PrintDebug(split[0], split[1], bDebug);
                    }
                    else
                    {
                        PrintDebug(split[0], bweekly.ToString(), bDebug);
                    }
                    break;
                case "yearly":
                    byearly = true;
                    PrintDebug(split[0], byearly.ToString(), bDebug);
                    break;
                case "compresscmd":
                    scompresscmd = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "uncompresscmd":
                    suncompresscmd = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "compressext":
                    scompressext = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "compressoptions":
                    // Join all remaining parts in case options contain spaces
                    scompressoptions = string.Join(" ", split, 1, split.Length - 1);
                    PrintDebug(split[0], scompressoptions, bDebug);
                    break;
                case "dateformat":
                    sdateformat = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "mail":
                    smail = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "nomail":
                    smail = "";
                    PrintDebug(split[0], "", bDebug);
                    break;
                case "maxage":
                    imaxage = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "minage":
                    iminage = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "olddir":
                    solddir = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "noolddir":
                    solddir = "";
                    PrintDebug(split[0], "", bDebug);
                    break;
                case "createolddir":
                    bcreateolddir = true;
                    // Note: Linux accepts optional mode/owner/group parameters, but we ignore them on Windows
                    PrintDebug(split[0], bcreateolddir.ToString(), bDebug);
                    break;
                case "nocreateolddir":
                    bcreateolddir = false;
                    PrintDebug(split[0], bcreateolddir.ToString(), bDebug);
                    break;
                case "rotate":
                    irotate = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "minsize":
                    // the size can be for following:  100, 100k, 100m, 100g
                    string minsize_type = split[1].Substring(split[1].Length - 1, 1).ToUpper();
                    if (Char.IsNumber(minsize_type, 0))
                        iminsize = Convert.ToInt64(split[1]);
                    else
                    {
                        if (minsize_type == "K")
                            iminsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1024;
                        else if (minsize_type == "M")
                            iminsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1048576;
                        else if (minsize_type == "G")
                            iminsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1073741824;
                        else
                        {
                            Logging.Log(Strings.UnknownSizeType+" " + line, Logging.LogType.Error);
                            return false;
                        }
                    }

                    PrintDebug(split[0], iminsize.ToString(), bDebug);
                    break;
                case "maxsize":
                    // the size can be for following:  100, 100k, 100m, 100g
                    string maxsize_type = split[1].Substring(split[1].Length - 1, 1).ToUpper();
                    if (Char.IsNumber(maxsize_type, 0))
                        imaxsize = Convert.ToInt64(split[1]);
                    else
                    {
                        if (maxsize_type == "K")
                            imaxsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1024;
                        else if (maxsize_type == "M")
                            imaxsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1048576;
                        else if (maxsize_type == "G")
                            imaxsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1073741824;
                        else
                        {
                            Logging.Log(Strings.UnknownSizeType+" " + line, Logging.LogType.Error);
                            return false;
                        }
                    }

                    PrintDebug(split[0], imaxsize.ToString(), bDebug);
                    break;
                case "shred":
                    bshred = true;
                    PrintDebug(split[0], bshred.ToString(), bDebug);
                    break;
                case "noshred":
                    bshred = false;
                    PrintDebug(split[0], bshred.ToString(), bDebug);
                    break;
                case "shredcycles":
                    ishredcycles = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "extension":
                    sextension = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "addextension":
                    saddextension = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "start":
                    istart = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "postrotate":
                    bpostrotate = true;
                    PrintDebug(split[0], bpostrotate.ToString(), bDebug);
                    break;
                case "prerotate":
                    bprerotate = true;
                    PrintDebug(split[0], bprerotate.ToString(), bDebug);
                    break;
                case "firstaction":
                    bfirstaction = true;
                    PrintDebug(split[0], bfirstaction.ToString(), bDebug);
                    break;
                case "lastaction":
                    blastaction = true;
                    PrintDebug(split[0], blastaction.ToString(), bDebug);
                    break;
                case "preremove":
                    bpreremove = true;
                    PrintDebug(split[0], bpreremove.ToString(), bDebug);
                    break;
                
                case "size":
                    // the size can be for following:  100, 100k, 100m, 100g
                    string size_type = split[1].Substring(split[1].Length - 1, 1).ToUpper();
                    if (Char.IsNumber(size_type, 0))
                        lsize = Convert.ToInt64(split[1]);
                    else
                    {
                        if (size_type == "K")
                            lsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1024;
                        else if (size_type == "M")
                            lsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1048576;
                        else if (size_type == "G")
                            lsize = Convert.ToInt64(split[1].Substring(0, split[1].Length - 1)) * 1073741824;
                        else
                        {
                            Logging.Log(Strings.UnknownSizeType + " " + line, Logging.LogType.Error);
                            return false;
                        }
                    }
                    PrintDebug(split[0], lsize.ToString(), bDebug);
                    break;
                case "mailfirst":
                    bmaillast = false;
                    PrintDebug(split[0], bmaillast.ToString(), bDebug);
                    break;
                case "maillast":
                    bmaillast = true;
                    PrintDebug(split[0], bmaillast.ToString(), bDebug);
                    break;
                case "smtpserver":
                    ssmtpserver = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpport":
                    ismtpport = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpssl":
                    bsmtpssl = true;
                    PrintDebug(split[0], bsmtpssl.ToString(), bDebug);
                    break;
                case "nosmtpssl":
                    bsmtpssl = false;
                    PrintDebug(split[0], bsmtpssl.ToString(), bDebug);
                    break;
                case "smtpuser":
                    ssmtpuser = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpfrom":
                    ssmtpfrom = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpuserpwd":
                    ssmtpuserpwd = StripQuotes(split[1]);
                    PrintDebug(split[0], "****", bDebug);
                    break;
                case "dateext":
                    bdateext = true;
                    PrintDebug(split[0], bdateext.ToString(), bDebug);
                    break;
                case "nodateext":
                    bdateext = false;
                    PrintDebug(split[0], bdateext.ToString(), bDebug);
                    break;
                case "dateyesterday":
                    bdateyesterday = true;
                    PrintDebug(split[0], bdateyesterday.ToString(), bDebug);
                    break;
                case "nodateyesterday":
                    bdateyesterday = false;
                    PrintDebug(split[0], bdateyesterday.ToString(), bDebug);
                    break;
                case "datehourago":
                    bdatehourago = true;
                    PrintDebug(split[0], bdatehourago.ToString(), bDebug);
                    break;
                case "nodatehourago":
                    bdatehourago = false;
                    PrintDebug(split[0], bdatehourago.ToString(), bDebug);
                    break;
                case "tabooext":
                    int taboo_start_idx = 2;
                    if (split[1] != "+")
                    {
                        taboo_start_idx = 1;
                        Array.Resize<string>(ref stabooext,0);
                    }
                    for (int j = taboo_start_idx; j < split.Length; j++ )
                    {
                        Array.Resize<string>(ref stabooext, stabooext.Length + 1);
                        stabooext[stabooext.Length - 1] = split[j];
                    }
                    break;
                case "taboopat":
                    int taboopat_start_idx = 2;
                    if (split[1] != "+")
                    {
                        taboopat_start_idx = 1;
                        staboopat = new string[0];
                    }
                    else if (staboopat == null)
                    {
                        staboopat = new string[0];
                    }
                    for (int j = taboopat_start_idx; j < split.Length; j++)
                    {
                        Array.Resize<string>(ref staboopat, staboopat.Length + 1);
                        staboopat[staboopat.Length - 1] = split[j];
                    }
                    break;
                case "include":
                    sinclude = StripQuotes(split[1]);
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "logfileopen_retry":
                    bretry_logfileopen = true;
                    PrintDebug(split[0], bretry_logfileopen.ToString(), bDebug);
                    break;
                case "logfileopen_msbetweenretryattempts":
                    inumms_retry_logfileopen = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], inumms_retry_logfileopen.ToString(), bDebug);
                    break;
                case "logfileopen_numretryattempts":
                    inumretry_logfileopen = Convert.ToInt32(split[1]);
                    PrintDebug(split[0], inumretry_logfileopen.ToString(), bDebug);
                    break;
                default:
                    Logging.Log(Strings.UnknownDirective + " " + line, Logging.LogType.Warning);
                    return false;
            }
            return true;
        }

        private long ParseSize(string value)
        {
            // the size can be for following:  100, 100k, 100m, 100g
            string size_type = value.Substring(value.Length - 1, 1).ToUpper();
            if (Char.IsNumber(size_type, 0))
                return Convert.ToInt64(value);

            long size_base = Convert.ToInt64(value.Substring(0, value.Length - 1));
            
            switch (size_type)
            {
                case "K":
                    return size_base * 1024;
                
                case "M":
                    return size_base * 1048576;
                
                case "G":
                    return size_base * 1073741824;
                
                default:
                    Logging.Log(Strings.UnknownSizeType+" " + value, Logging.LogType.Error);
                    return 0;
            }
        }

        private void ParseFirstAction(string line)
        {
            if (sfirstaction == null)
                sfirstaction = new List<string>();

            sfirstaction.Add(line);
        }

        private void ParseLastAction(string line)
        {
            if (slastaction == null)
                slastaction = new List<string>();

            slastaction.Add(line);
        }

        private void ParsePreRotate(string line)
        {
            if (sprerotate == null)
                sprerotate = new List<string>();

            sprerotate.Add(line);
        }

        private void ParsePostRotate(string line)
        {
            if (spostrotate == null)
                spostrotate = new List<string>();

            spostrotate.Add(line);
        }

        private void ParsePreRemove(string line)
        {
            if (spreremove == null)
                spreremove = new List<string>();

            spreremove.Add(line);
        }

        public void Clear_PreRotate()
        {
            sprerotate.Clear();
            sprerotate = null;
        }

        public void Clear_PostRotate()
        {
            spostrotate.Clear();
            spostrotate = null;
        }

        /// <summary>
        /// Strips surrounding quotes from a directive value to match Linux logrotate shell quoting rules.
        /// Supports both single quotes (') and double quotes (").
        /// </summary>
        /// <param name="value">The directive value that may contain quotes</param>
        /// <returns>The value with surrounding quotes removed</returns>
        private string StripQuotes(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            // Check if value is surrounded by double quotes
            if (value.Length >= 2 && value[0] == '"' && value[value.Length - 1] == '"')
            {
                return value.Substring(1, value.Length - 2);
            }

            // Check if value is surrounded by single quotes
            if (value.Length >= 2 && value[0] == '\'' && value[value.Length - 1] == '\'')
            {
                return value.Substring(1, value.Length - 2);
            }

            return value;
        }

        public void Increment_ProcessCount()
        {
            process_count++;
        }

        public void Decrement_ProcessCount()
        {
            process_count--;
        }
    }
}
