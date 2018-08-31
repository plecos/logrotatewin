using System;
using System.Collections.Generic;
using System.Text;

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
        #region Private variables
        private bool bcompress = true;
        //private string scompresscmd = "gzip";
        //private string suncompressedcmd = "gunzip";
        private string scompressext = "gz";
        //private string scompressoptions = "-9";
        private bool bcopy = false;
        private bool bcopytruncate = false;
        private bool bcreate = false;
        private bool bdaily = false;
        private bool bdateext = false;
        private string sdateformat = "-%Y%m%d";
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
        private int imaxage = 0;
        private bool bmissingok = false;
        private bool bmonthly = false;
        private bool bmaillast = true;
        private string solddir = "";
        private List<string> spostrotate = null;
        private List<string> sprerotate = null;
        private List<string> sfirstaction = null;
        private List<string> slastaction = null;
        private int irotate = 0;
        private long lsize = 0;
        private bool bsharedscripts = false;
        private int istart = 1;
        private string[] stabooext = { ".swp" };
        private bool bweekly = false;
        private bool byearly = false;
        private bool bshred = false;
        private int ishredcycles = 3;
        private bool bretry_logfileopen = false;
        private int inumretry_logfileopen = 0;
        private int inumms_retry_logfileopen = 1000;
        

        private bool bpostrotate = false;
        private bool bprerotate = false;
        private bool bfirstaction = false;
        private bool blastaction = false;

        private int process_count = 0;

        #endregion

        #region Public properties

        public int ProcessCount
        {
            get { return process_count; }
        }

        public bool Compress
        {
            get { return bcompress; }
        }

        public string CompressExt
        {
            get { return scompressext; }
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

        public string DateFormat
        {
            get { return sdateformat; }
        }

        private void PrintDebug(string line, string value,bool bDebug)
        {
            Logging.Log(Strings.Setting+" " + line + Strings.To + value,Logging.LogType.Debug);
        }

        public int Start
        {
            get { return istart; }
        }

        public string OldDir
        {
            get { return solddir; }
        }

        public bool CopyTruncate
        {
            get { return bcopytruncate; }
        }

        public bool Create
        {
            get { return bcreate; }
        }

        public long MinSize
        {
            get { return iminsize; }
        }

        public bool Daily
        {
            get { return bdaily; }
        }
        public bool Monthly
        {
            get { return bmonthly; }
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
        public bool SharedScripts
        {
            get { return bsharedscripts; }
        }
        public bool Weekly
        {
            get { return bweekly; }
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
            bcompress = m_source.bcompress;
            //scompresscmd = m_source.scompresscmd;
            //suncompressedcmd = m_source.suncompressedcmd;
            scompressext = m_source.scompressext;
            //scompressoptions = m_source.scompressoptions;
            bcopy = m_source.bcopy;
            bcopytruncate = m_source.bcopytruncate;
            bcreate = m_source.bcreate;
            bdaily = m_source.bdaily;
            bdateext = m_source.bdateext;
            sdateformat = m_source.sdateformat;
            bdelaycompress = m_source.bdelaycompress;
            bifempty = m_source.bifempty;
            smail = m_source.smail;
            iminsize = m_source.iminsize;
            imaxage = m_source.imaxage;
            bmissingok = m_source.bmissingok;
            bmonthly = m_source.bmonthly;
            solddir = m_source.solddir;
            spostrotate = m_source.spostrotate;
            sprerotate = m_source.sprerotate;
            sfirstaction = m_source.sfirstaction;
            slastaction = m_source.slastaction;
            irotate = m_source.irotate;
            lsize = m_source.lsize;
            bsharedscripts = m_source.bsharedscripts;
            istart = m_source.istart;
            stabooext = m_source.stabooext;
            bweekly = m_source.bweekly;
            byearly = m_source.byearly;
            bshred = m_source.bshred;
            ishredcycles = m_source.ishredcycles;
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
            if ((bpostrotate == true) || (bprerotate == true) || (blastaction == true) || (bfirstaction == true))
            {
                if (split[0] == "endscript")
                {
                    bpostrotate = bprerotate = blastaction = bfirstaction = false;
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
                }
                return true;
            }

            switch (split[0])
            {
                case "compress":
                    bcompress = true;
                    PrintDebug(split[0], bcompress.ToString(), bDebug);
                    break;
                case "nocompress":
                    bcompress = false;
                    PrintDebug(split[0], bcompress.ToString(), bDebug);
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
                case "create":
                    bcreate = true;
                    PrintDebug(split[0], bcreate.ToString(), bDebug);
                    break;
                case "nocreate":
                    bcreate = false;
                    PrintDebug(split[0], bcreate.ToString(), bDebug);
                    break;
                case "daily":
                    bdaily = true;
                    PrintDebug(split[0], bdaily.ToString(), bDebug);
                    break;
                case "delaycompress":
                    bdelaycompress = true;
                    PrintDebug(split[0], bdelaycompress.ToString(), bDebug);
                    break;
                case "ifempty":
                    bifempty = true;
                    PrintDebug(split[0], bifempty.ToString(), bDebug);
                    break;
                case "missingok":
                    bmissingok = true;
                    PrintDebug(split[0], bmissingok.ToString(), bDebug);
                    break;
                case "monthly":
                    bmonthly = true;
                    PrintDebug(split[0], bmonthly.ToString(), bDebug);
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
                    PrintDebug(split[0], bweekly.ToString(), bDebug);
                    break;
                case "yearly":
                    byearly = true;
                    PrintDebug(split[0], byearly.ToString(), bDebug);
                    break;
                case "compresscmd":
                    //scompresscmd = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    PrintDebug(split[0], Strings.UnknownDirective, bDebug);
                    break;
                case "uncompresscmd":
                    //suncompressedcmd = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    PrintDebug(split[0], Strings.UnknownDirective, bDebug);
                    break;
                case "compressext":
                    scompressext = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "scompressoptions":
                    //scompressoptions = split[1];
                    //PrintDebug(split[0], split[1], bDebug);
                    PrintDebug(split[0], Strings.UnknownDirective, bDebug);
                    break;
                case "dateformat":
                    sdateformat = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "mail":
                    smail = split[1];
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
                case "olddir":
                    solddir = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "noolddir":
                    solddir = "";
                    PrintDebug(split[0], "", bDebug);
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
                            Logging.Log(Strings.UnknownSizeType+" " + line,Logging.LogType.Error);
                            return false;
                        }
                    }

                    PrintDebug(split[0], lsize.ToString(), bDebug);

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
                            Logging.Log(Strings.UnknownSizeType+" " + line,Logging.LogType.Error);
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
                    ssmtpserver = split[1];
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
                    ssmtpuser = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpfrom":
                    ssmtpfrom = split[1];
                    PrintDebug(split[0], split[1], bDebug);
                    break;
                case "smtpuserpwd":
                    ssmtpuserpwd = split[1];
                    PrintDebug(split[0], "****", bDebug);
                    break;
                case "dateext":
                    bdateext = true;
                    PrintDebug(split[0], bdateext.ToString(), bDebug);
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
                case "include":
                    sinclude = split[1];
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
                    Logging.Log(Strings.UnknownDirective+" " + line,Logging.LogType.Error);
                    return false;
            }
            return true;
        }

        private void ParseFirstAction(string line)
        {
            if (slastaction == null)
                slastaction = new List<string>();

            slastaction.Add(line);
        }

        private void ParseLastAction(string line)
        {
            if (sfirstaction == null)
                sfirstaction = new List<string>();

            sfirstaction.Add(line);
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
