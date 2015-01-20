using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.IO;
using System.Diagnostics;
using System.Net.Mail;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Collections;

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
    class Program
    {
        // this object will parse the command line args
        private static CmdLineArgs cla = null;
        
        // this contains any global config settings
        private static logrotateconf GlobalConfig = new logrotateconf();

        // this is a list of file paths and the associated Config section
        private static Dictionary<string, logrotateconf> FilePathConfigSection = new Dictionary<string, logrotateconf>();

        // this object provides management of the Status file
        private static logrotatestatus Status = null;

        static void Main(string[] args)
        {
            // parse the command line
            try
            {
                if (args.Length == 0)
                {
                    PrintVersion();
                    PrintUsage();
                    Environment.Exit(0);
                }

                cla = new CmdLineArgs(args);
                if (cla.Usage)
                {
                    PrintUsage();
                    Environment.Exit(0);
                }

                Status = new logrotatestatus(cla.AlternateStateFile);
                
                // now process the config files
                foreach (string s in cla.ConfigFilePaths)
                {
                    ProcessConfigPath(s);
                }

                // if there was an include directive in the global settings, then we need to process that 
                if (GlobalConfig.Include != "")
                {
                    ProcessIncludeDirective();
                }

                if (GlobalConfig.FirstAction != null)
                {
                    Logging.Log(Strings.ExecutingFirstAction, Logging.LogType.Verbose);
                    ProcessFirstAction(GlobalConfig);
                }


                // now that config files have been read, let's run the main process
                // iterate through the sections
                foreach (KeyValuePair<string,logrotateconf>kvp in FilePathConfigSection)
                {
                    // if sharedscripts enabled, then run prerotate
                    if ((kvp.Value.PreRotate != null) && (kvp.Value.SharedScripts == true))
                    {
                        Logging.Log(Strings.ExecutingPreRotateSharedScripts, Logging.LogType.Verbose);
                        PreRotate(kvp.Value, kvp.Key);
                        // clear the prerotate so it won't be called again for any other files in this section since sharedscripts is set to true
                        kvp.Value.Clear_PreRotate();
                    }

                    // decrement the number of times we have seen this config section.  when it reaches zero, we know we don't have anymore files to process
                    // and we can then run the PostRotate
                    kvp.Value.Decrement_ProcessCount();

                    Logging.Log(Strings.Processing + " " + kvp.Key, Logging.LogType.Verbose);
                    List<FileInfo> m_rotatefis = new List<FileInfo>();

                    try
                    {

                        // if kvp.Key is a single file, then process
                        if ((File.Exists(kvp.Key) == true) || ((File.Exists(kvp.Key) == false) && (kvp.Key.Contains("?") == false) && (kvp.Key.Contains("*") == false)))
                        {
                            if (CheckForRotate(kvp.Key, kvp.Value))
                                m_rotatefis.Add(new FileInfo(kvp.Key));
                        }
                        else
                        {
                            // if it is a folder or wildcard, then we need to get a list of files to process
                            if (Directory.Exists(kvp.Key))
                            {
                                // this is pointed to a folder, so process all files in the folder
                                DirectoryInfo di = new DirectoryInfo(kvp.Key);
                                FileInfo[] fis = di.GetFiles();
                                foreach (FileInfo m_fi in fis)
                                {
                                    Logging.Log(Strings.Processing + " " + m_fi.FullName, Logging.LogType.Verbose);
                                    if (CheckForRotate(m_fi.FullName, kvp.Value))
                                        m_rotatefis.Add(m_fi);
                                }
                            }
                            else
                            {
                                // assume there is a wildcard character in the path, so now we need to parse the directory string and go through each folder
                                // if there is a wildcard in the subfolder name, then get all the files as specified (note there may be a wildcard for the filename too)
                                string filename = Path.GetFileName(kvp.Key);
                                List<string> dirs = new List<string>();
                                string[] parsed = Path.GetDirectoryName(kvp.Key).Split(Path.DirectorySeparatorChar);
                                string tmp = "";
                                bool bFoundWildcard = false;
                                foreach (string p in parsed)
                                {
                                    if ((p.Contains("*")) || (p.Contains("?")))
                                    {
                                        bFoundWildcard = true;
                                        Logging.Log(Strings.LookingForFolders+" '" + tmp + "' "+Strings.WithWildcardPattern+" '" + p + "'", Logging.LogType.Verbose);
                                        string[] new_dirs = Directory.GetDirectories(tmp, p, SearchOption.AllDirectories);
                                        if (new_dirs.Length == 0)
                                        {
                                            Logging.Log(Strings.NoFoldersFound+" '" + tmp + "' "+Strings.WithWildcardPattern+" '" + p + "'",Logging.LogType.Verbose);
                                        }
                                        foreach (string new_dir in new_dirs)
                                        {
                                            Logging.Log(Strings.MatchedFolder+" '"+new_dir+"'", Logging.LogType.Verbose);
                                            dirs.Add(new_dir);
                                        }
                                            
                                    }
                                    else
                                    {
                                        if (p != Path.DirectorySeparatorChar.ToString())
                                        {
                                            tmp += p;
                                            tmp += Path.DirectorySeparatorChar;
                                        }
                                    }
                                }
                                if (bFoundWildcard == false)
                                    dirs.Add(tmp);

                                foreach (string search_dir in dirs)
                                {
                                    // assume this is a wildcard, so attempt to use it
                                    DirectoryInfo di = new DirectoryInfo(search_dir);
                                    FileInfo[] fis = di.GetFiles(filename);
                                    if (fis.Length == 0)
                                    {
                                        Logging.Log(Strings.NoFilesFound+" '" + search_dir + "' "+Strings.Matching+" '" + filename + "'", Logging.LogType.Verbose);

                                    }
                                    foreach (FileInfo m_fi in fis)
                                    {
                                        Logging.Log(Strings.Processing + " " + m_fi.FullName, Logging.LogType.Verbose);
                                        if (CheckForRotate(m_fi.FullName, kvp.Value))
                                            m_rotatefis.Add(m_fi);
                                    }
                                }
                            }
                        }

                        // now rotate
                        foreach (FileInfo r_fi in m_rotatefis)
                        {
                            RotateFile(kvp.Value, r_fi);
                        }

                        // if sharedscripts enabled, then run postrotate if this is the last file
                        if ((kvp.Value.PostRotate != null) && (kvp.Value.SharedScripts == true))
                        {
                            if (kvp.Value.ProcessCount == 0)
                            {
                                Logging.Log(Strings.ExecutingPostRotateSharedScripts, Logging.LogType.Verbose);
                                PostRotate(kvp.Value, kvp.Key);
                                // clear the postrotate so it won't be called again for any other files in this section since sharedscripts is set to true
                                kvp.Value.Clear_PostRotate();
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logging.LogException(e);
                    }
                }
                // now run lastaction if needed
                if (GlobalConfig.LastAction != null) 
                {
                    Logging.Log(Strings.ExecutingLastAction, Logging.LogType.Verbose);
                    ProcessLastAction(GlobalConfig);
                }



            }
            catch (Exception e)
            {
                Logging.LogException(e);
                Environment.Exit(1);
            }
        }

        private void RecursiveParseFolders(string sfolder, string pattern, ref List<string> dirs)
        {
            foreach (string dir in Directory.GetDirectories(sfolder,pattern))
            {

            }
        }


        /// <summary>
        /// Process the include directive if specfied
        /// </summary>
        private static void ProcessIncludeDirective()
        {
            if (Directory.Exists(GlobalConfig.Include))
            {
                // this is a folder, so get all files in the folder and process them
                DirectoryInfo di = new DirectoryInfo(GlobalConfig.Include);
                FileInfo[] m_fis = di.GetFiles();
                // sort alphabetically
                Array.Sort(m_fis, delegate(FileSystemInfo a, FileSystemInfo b)
                {
                    return ((new CaseInsensitiveComparer()).Compare(b.Name, a.Name));
                });
                // make sure ext of file is not in the tabooext list
                foreach (FileInfo m_fi in m_fis)
                {
                    bool bFound = false;
                    for (int i = 0; i < GlobalConfig.TabooList.Length; i++)
                    {
                        if (m_fi.Extension == GlobalConfig.TabooList[i])
                        {
                            Logging.Log(Strings.Skipping+" " + m_fi.FullName + " - " + Strings.ExtInTaboo, Logging.LogType.Verbose);
                            bFound = true;
                            break;
                        }
                    }
                    if (!bFound)
                    {
                        Logging.Log(Strings.ProcessInclude+" " + m_fi.FullName, Logging.LogType.Verbose);
                        ProcessConfigFile(m_fi.FullName);
                    }
                }
            }
            else
            {
                // this (might be) a file, so attempt to process it
                if (File.Exists(GlobalConfig.Include))
                {
                    Logging.Log(Strings.ProcessInclude+" "+ GlobalConfig.Include, Logging.LogType.Verbose);
                    ProcessConfigPath(GlobalConfig.Include);
                }
                else
                {
                    // this could be a directory, so let's check for that also
                    if ((File.GetAttributes(GlobalConfig.Include) & FileAttributes.Directory) == FileAttributes.Directory)
                    {
                        DirectoryInfo di = new DirectoryInfo(GlobalConfig.Include);
                        FileInfo[] fis = di.GetFiles("*");
                        foreach (FileInfo fi in fis)
                        {
                            Logging.Log(Strings.ProcessInclude + " " + GlobalConfig.Include, Logging.LogType.Verbose);
                            ProcessConfigPath(fi.FullName);
                        }
                    }
                    else
                    {
                        Logging.Log(GlobalConfig.Include + " "+Strings.CouldNotBeFound, Logging.LogType.Error);
                        Environment.Exit(1);
                    }
                }
            }
        }



        private static string GetRotateLogDirectory(string logfilepath, logrotateconf lrc)
        {
            if (lrc.OldDir != "")
                return lrc.OldDir;
            else
                return Path.GetDirectoryName(logfilepath);
        }

        /// <summary>
        /// Check to see if the logfile specified is eligible for rotation
        /// </summary>
        /// <param name="logfilepath">Full path to the log file to check</param>
        /// <param name="lrc">logrotationconf object</param>
        /// <returns>True if need to rotate, False if not</returns>
        private static bool CheckForRotate(string logfilepath, logrotateconf lrc)
        {

            if (cla.Force)
            {
                Logging.Log(Strings.ForceOptionRotate, Logging.LogType.Verbose);
                return true;
            }

            bool bDoRotate = false;
            // first check if file exists.  if it doesn't error out unless we are set not to
            if (File.Exists(logfilepath) == false)
            {
                if (lrc.MissingOK==false)
                {
                    Logging.Log(logfilepath + " "+Strings.CouldNotBeFound,Logging.LogType.Error);
                    return false;
                }
            }

            FileInfo fi = new FileInfo(logfilepath);

            //if (logfilepath.Length == 0)
            if (fi.Length == 0)
            {
                if (lrc.IfEmpty == false)
                {
                    Logging.Log(Strings.LogFileEmpty+" - " + Strings.Skipping,Logging.LogType.Verbose);
                    return false;
                }
            }
            
            // determine if we need to rotate the file.  this can be based on a number of criteria, including size, date, etc.
            if (lrc.MinSize != 0)
            {
                if (fi.Length < lrc.MinSize)
                {
                    Logging.Log(Strings.NoRotateNotGTEMinimumFileSize,Logging.LogType.Verbose);

                    return false;
                }
            }

            if (lrc.Size != 0)
            {
                if (fi.Length >= lrc.Size)
                {
                    Logging.Log(Strings.RotateBasedonFileSize,Logging.LogType.Verbose);

                    bDoRotate = true;
                    
                }
            }
            else
            {
                //if ((lrc.Daily == false) && (lrc.Monthly == false) && (lrc.Yearly == false))
                // fix for rotate not working as submitted by Matt Richardson 1/19/2015
                if ((lrc.Daily == false) && (lrc.Weekly == false) && (lrc.Monthly == false) && (lrc.Yearly == false))
                {
                    // this is a misconfiguration is we get here
                    Logging.Log(Strings.NoTimestampDirectives, Logging.LogType.Verbose);
                }
                else
                {
                    // check last date of rotation
                    DateTime lastRotate = Status.GetRotationDate(logfilepath);
                    TimeSpan ts = DateTime.Now - lastRotate;
                    if (lrc.Daily)
                    {
                        // check to see if lastRotate is more than a day old
                        if (ts.TotalDays > 1)
                        {
                            bDoRotate = true;
                        }
                    }
                    if (lrc.Weekly)
                    {
                        // check if total # of days is greater than a week or if the current weekday is less than the weekday of the last rotation
                        if (ts.TotalDays > 7)
                        {
                            bDoRotate = true;
                        }
                        else if (DateTime.Now.DayOfWeek < lastRotate.DayOfWeek)
                        {
                            bDoRotate = true;
                        }
                    }
                    if (lrc.Monthly)
                    {
                        // check if the month is different
                        if ((lastRotate.Year != DateTime.Now.Year) || ((lastRotate.Year == DateTime.Now.Year) && (lastRotate.Month != DateTime.Now.Month)))
                        {
                            bDoRotate = true;
                        }
                    }
                    if (lrc.Yearly)
                    {
                        // check if the year is different
                        if (lastRotate.Year != DateTime.Now.Year)
                        {
                            bDoRotate = true;
                        }
                    }

                }
            }

            return bDoRotate;

        }

        private static void RemoveOldRotateFile(string logfilepath,logrotateconf lrc,FileInfo m_fi)
        {
            if ((lrc.MailAddress != "") && (lrc.MailLast))
            {
                // attempt to mail this file
                SendEmail(logfilepath, lrc, m_fi.FullName);
            }

            DeleteRotateFile(m_fi.FullName, lrc);

        }

        /// <summary>
        /// Delete a file using the configured method (shred or delete)
        /// </summary>
        /// <param name="m_filepath">Path to the file to delete</param>
        /// <param name="lrc">Currently loaded configuration file</param>
        private static void DeleteRotateFile(string m_filepath, logrotateconf lrc)
        {
            if (File.Exists(m_filepath) == false)
                return;

            if (lrc.Shred)
            {
                Logging.Log(Strings.ShreddingFile+" " + m_filepath, Logging.LogType.Verbose);

                if (cla.Debug == false)
                {
                    ShredFile sf = new ShredFile(m_filepath);
                    sf.ShredIt(lrc.ShredCycles, cla.Debug);
                }
            }
            else
            {
                Logging.Log(Strings.DeletingFile+" " + m_filepath, Logging.LogType.Verbose);

                if (cla.Debug == false)
                {
                    File.Delete(m_filepath);
                }
            }
        }

        #region FirstAction,LastAction,PreRotate,PostRotate

        /// <summary>
        /// Creates a script using the List of strings and executes it
        /// </summary>
        /// <param name="m_script_lines">a List of strings containing the lines for the script</param>
        /// <param name="path_to_logfile">The path to the log file we are processing; it is passed as a command line parameter to the script</param>
        /// <returns>True if script executes with no errors, False if there was an error</returns>
        private static bool CreateScriptandExecute(List<string> m_script_lines, string path_to_logfile)
        {
            // create our temporary script file
            string temp_path_orig = Path.GetTempFileName();
            string temp_path = Path.ChangeExtension(temp_path_orig, "cmd");
            // get rid of the original file
            File.Delete(temp_path_orig);
            Logging.Log("Script file path: " + temp_path, Logging.LogType.Debug);
            try
            {
                StreamWriter sw = new StreamWriter(temp_path, false);

                foreach (string s in m_script_lines)
                {
                    sw.WriteLine(s);
                }

                sw.Close();
            }
            catch (Exception e)
            {
                Logging.LogException(e);
                return false;
            }

            try
            {
                if (cla.Debug == false)
                {
                    ProcessStartInfo psi = new ProcessStartInfo(Environment.SystemDirectory + "\\cmd.exe", "/S /C \"\"" + temp_path + "\" \"" + path_to_logfile + "\"\"");
                    psi.RedirectStandardOutput = true;
                    psi.RedirectStandardError = true;
                    psi.UseShellExecute = false;
                    Logging.Log(Strings.Executing+" " + psi.FileName + " " + psi.Arguments, Logging.LogType.Verbose);
                    Process p = Process.Start(psi);
                    string output = p.StandardOutput.ReadToEnd();
                    string error = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    if (output != "")
                    {
                        Logging.Log(output, Logging.LogType.Verbose);
                    }
                    if (error != "")
                    {
                        Logging.Log(error, Logging.LogType.Error);
                        return false;
                    }
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e);
                return false;
            }
            finally
            {
                Logging.Log(Strings.DeletingFile+" " + temp_path, Logging.LogType.Debug);
                File.Delete(temp_path);
            }

            return true;

        }

        /// <summary>
        /// Execute any firstaction commands.  The commands are written to a temporary script file, and then this script file is executed with c:\windows\cmd.
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="path_to_logfile">The path to the logfile, which is passed as a paramenter to the script</param>
        /// <returns>True if script is executed with no errors, False is there was an error</returns>
        private static bool ProcessFirstAction(logrotateconf lrc)
        {
            if (lrc.FirstAction == null)
                throw new ArgumentNullException("lrc.FirstAction");

            return CreateScriptandExecute(lrc.FirstAction, "");
            
        }

        /// <summary>
        /// Execute any lastaction commands.  The commands are written to a temporary script file, and then this script file is executed with c:\windows\cmd.
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="path_to_logfile">The path to the logfile, which is passed as a paramenter to the script</param>
        /// <returns>True if script is executed with no errors, False is there was an error</returns>
        private static bool ProcessLastAction(logrotateconf lrc)
        {
            if (lrc.LastAction == null)
                throw new ArgumentNullException("lrc.LastAction");

            return CreateScriptandExecute(lrc.LastAction, "");
        }

        /// <summary>
        /// Execute any prerotate commands.  The commands are written to a temporary script file, and then this script file is executed with c:\windows\cmd.
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="path_to_logfile">The path to the logfile, which is passed as a paramenter to the script</param>
        /// <returns>True if script is executed with no errors, False is there was an error</returns>
        private static bool PreRotate(logrotateconf lrc, string path_to_logfile)
        {
            if (lrc.PreRotate == null)
                throw new ArgumentNullException("lrc.PreRotate");

            return CreateScriptandExecute(lrc.PreRotate, path_to_logfile);
        }

        /// <summary>
        /// Execute any postrotate commands.  The commands are written to a temporary script file, and then this script file is executed with c:\windows\cmd.
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="path_to_logfile">The path to the logfile, which is passed as a paramenter to the script</param>
        /// <returns>True if script is executed with no errors, False is there was an error</returns>
        private static bool PostRotate(logrotateconf lrc, string path_to_logfile)
        {
            if (lrc.PostRotate == null)
                throw new ArgumentNullException("lrc.PostRotate");

            return CreateScriptandExecute(lrc.PostRotate, path_to_logfile);

        }
        #endregion

        /// <summary>
        /// Returns the path that the rotated log should go, depends on the olddir directive
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="fi">the rotated log fileinfo object</param>
        /// <returns>String containing path for the rotated log file</returns>
        private static string GetRotatePath(logrotateconf lrc, FileInfo fi)
        {
            // determine path to put the rotated log file
            string rotate_path = "";
            if (lrc.OldDir != "")
            {
                if (!Directory.Exists(lrc.OldDir))
                {
                    Directory.CreateDirectory(lrc.OldDir);
                }

                rotate_path = lrc.OldDir + "\\";
            }
            else
                rotate_path = Path.GetDirectoryName(fi.FullName) + "\\";

            return rotate_path;
        }

        /// <summary>
        /// Determines the name of the rotated log file
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="fi">FileInfo object of the rotated log file</param>
        /// <returns>String containing rotated log file name</returns>
        private static string GetRotateName(logrotateconf lrc, FileInfo fi)
        {
            string rotate_name = "";
            if (lrc.DateExt)
            {
                string time_str = lrc.DateFormat;
                time_str = time_str.Replace("%Y", DateTime.Now.Year.ToString());
                time_str = time_str.Replace("%m", DateTime.Now.Month.ToString());
                time_str = time_str.Replace("%d", DateTime.Now.Day.ToString());
                time_str = time_str.Replace("%s", ((double)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds).ToString());
                rotate_name = fi.Name + time_str;
            }
            else
            {
                
                rotate_name = fi.Name + "." + lrc.Start;
            }
            return rotate_name;
        }

        /// <summary>
        /// Age out old rotated files, and rename rotated files as needed.  Also support delaycompress option
        /// </summary>
        /// <param name="lrc">logrotateconf object</param>
        /// <param name="fi">FileInfo object for the log file</param>
        /// <param name="rotate_path">the folder rotated logs are located in</param>
        private static void AgeOutRotatedFiles(logrotateconf lrc, FileInfo fi,string rotate_path)
        {
            DirectoryInfo di = new DirectoryInfo(rotate_path);
            FileInfo[] fis = di.GetFiles(fi.Name + "*");
            if (fis.Length == 0)
                // nothing to do
                return;

            // look for any rotated log files, and rename them with the count if not using dateext
            Regex pattern = new Regex("[0-9]");

            // sort alphabetically reversed
            Array.Sort<FileSystemInfo>(fis, delegate(FileSystemInfo a, FileSystemInfo b)
            {
                return ((new CaseInsensitiveComparer()).Compare(b.Name, a.Name));
            });
            // if original file is in this list, remove it
            if (fis[fis.Length - 1].Name == fi.Name)
            {
                // this is the original file, remove from this list
                Array.Resize<FileInfo>(ref fis, fis.Length - 1);
            }
            // go ahead and remove files that are too old by age
            foreach (FileInfo m_fi in fis)
            {
                if (lrc.MaxAge != 0)
                {
                    // any log files that are "old" need to be handled - either deleted or emailed
                    if (m_fi.LastWriteTime < DateTime.Now.Subtract(new TimeSpan(lrc.MaxAge, 0, 0, 0)))
                    {
                        Logging.Log(m_fi.FullName + " is too old - "+Strings.DeletingFile, Logging.LogType.Verbose);
                        RemoveOldRotateFile(fi.FullName, lrc, m_fi);
                        continue;
                    }
                }
            }
            // iterate through array, determine if file needs to be removed or emailed
            if (lrc.DateExt == true)
            {
                for (int rotation_counter = lrc.Rotate-1; rotation_counter < fis.Length; rotation_counter++)
                {
                    // remove any entries that are past the rotation limit
                    RemoveOldRotateFile(fi.FullName, lrc, fis[rotation_counter]);
                }
            }
            else
            {
                
                foreach (FileInfo m_fi in fis)
                {
                    // if not aged out and we are not using dateext, then rename the file
                    // determine the rotation number of this file.  Account if it is compressed
                    string[] exts = m_fi.Name.Split(new char[] { '.' });
                    // determine which (if any) of the extensions match the regex.  w hen we find one we will use that as our rename reference
                    int i;
                    for (i = exts.Length - 1; i > 0; i--)
                    {
                        if (pattern.IsMatch(exts[i]))
                        {
                            break;
                        }
                    }
                    if (Convert.ToInt32(exts[i]) >= lrc.Rotate)
                    {
                        // too old!
                        RemoveOldRotateFile(fi.FullName, lrc, m_fi);
                    }
                    else
                    {
                        int newnum = Convert.ToInt32(exts[i]) + 1;
                        // build new filename
                        string newFile = "";
                        for (int j = 0; j < i; j++)
                        {
                            newFile = newFile + exts[j];
                            newFile += ".";
                        }
                        newFile += newnum.ToString();
                        for (int j = i + 1; j < exts.Length; j++)
                        {
                            newFile += ".";
                            newFile += exts[j];
                        }
                        Logging.Log(Strings.Renaming+" " + m_fi.FullName + Strings.To + rotate_path + newFile, Logging.LogType.Verbose);
                        if (cla.Debug == false)
                        {
                            // the there is already a file with the new name, then delete that file so we can rename this one
                            if (File.Exists(rotate_path + newFile))
                            {
                                DeleteRotateFile(rotate_path + newFile, lrc);
                            }

                            File.Move(m_fi.FullName, rotate_path + newFile);

                            // if we are set to compress, then check if the new file is compressed.  this is done by looking at the first two bytes
                            // if they are set to 0x1f8b then it is already compressed.  There is a possibility of a false positive, but this should
                            // be very unlikely since log files are text files and will not start with these bytes

                            if (lrc.Compress)
                            {
                                FileStream fs = File.Open(rotate_path + newFile, FileMode.Open);
                                byte[] magicnumber = new byte[2];
                                fs.Read(magicnumber, 0, 2);
                                fs.Close();
                                if ((magicnumber[0] != 0x1f) && (magicnumber[1] != 0x8b))
                                {
                                    CompressRotatedFile(rotate_path + newFile, lrc);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static void RotateFile(logrotateconf lrc, FileInfo fi)
        {
            Logging.Log(Strings.RotatingFile+" " + fi.FullName,Logging.LogType.Verbose);

            // we don't actually rotate if debug is enabled
            if (cla.Debug)
                Logging.Log(Strings.RotateSimulated,Logging.LogType.Debug);

            if ((lrc.PreRotate!=null)&&(lrc.SharedScripts==false))
                PreRotate(lrc,fi.FullName);

            // determine path to put the rotated log file
            string rotate_path = GetRotatePath(lrc,fi);

            // age out old logs
            AgeOutRotatedFiles(lrc, fi,rotate_path);

            // determine name of rotated log file
            string rotate_name = GetRotateName(lrc,fi);

            bool bLogFileExists = fi.Exists;

            // now either rename or copy (depends on copy setting) 
            if ((lrc.Copy)||(lrc.CopyTruncate))
            {
                Logging.Log(Strings.Copying+" " + fi.FullName + Strings.To + rotate_path + rotate_name, Logging.LogType.Verbose);

                if (cla.Debug == false)
                {
                    try
                    {
                        if (bLogFileExists)
                            File.Copy(fi.FullName, rotate_path + rotate_name, false);
                    }
                    catch (Exception e)
                    {
                        Logging.Log("Error copying file " + fi.FullName + " to " + rotate_path + rotate_name, Logging.LogType.Error);
                        Logging.LogException(e); 
                        return;
                    }

                    if (bLogFileExists)
                    {
                        File.SetCreationTime(rotate_path + rotate_name, DateTime.Now);
                        File.SetLastAccessTime(rotate_path + rotate_name, DateTime.Now);
                        File.SetLastWriteTime(rotate_path + rotate_name, DateTime.Now);
                    }

                }

                if (lrc.CopyTruncate)
                {
                    Logging.Log(Strings.TruncateLogFile,Logging.LogType.Verbose);

                    if (cla.Debug == false)
                    {
                        try
                        {
                            if (bLogFileExists)
                            {
                                FileStream fs = new FileStream(fi.FullName, FileMode.Open);
                                fs.SetLength(0);
                                fs.Close();
                            }
                        }
                        catch (Exception e)
                        {
                            Logging.Log("Error truncating file " + fi.FullName, Logging.LogType.Error);
                            Logging.LogException(e); 
                            return;
                        }
                    }
                }

            }
            else
            {
                Logging.Log(Strings.Renaming+" " + fi.FullName + Strings.To + rotate_path + rotate_name,Logging.LogType.Verbose);

                if (cla.Debug == false)
                {
                    try
                    {
                        if (bLogFileExists)
                            File.Move(fi.FullName, rotate_path + rotate_name);
                    }
                    catch (Exception e)
                    {
                        Logging.Log("Error renaming file " + fi.FullName + " to " + rotate_path + rotate_name, Logging.LogType.Error);
                        Logging.LogException(e); 
                        return;
                    }
                }

                if (lrc.Create)
                {
                    Logging.Log(Strings.CreateNewEmptyLogFile,Logging.LogType.Verbose);

                    if (cla.Debug == false)
                    {
                        try
                        {
                            FileStream fs = new FileStream(fi.FullName, FileMode.CreateNew);
                            fs.SetLength(0);
                            fs.Close();
                        }
                        catch (Exception e)
                        {
                            Logging.Log("Error creating new file " + fi.FullName, Logging.LogType.Error);
                            Logging.LogException(e); 
                            return;
                        }
                    }
                }
            }

            // now, compress the rotated log file if we are set to
            if (lrc.Compress) 
            {
                if (lrc.DelayCompress == false)
                {
                    CompressRotatedFile(rotate_path + rotate_name, lrc);
                    rotate_name += lrc.CompressExt;
                }
            }

            if (lrc.MailLast==false)
                SendEmail(fi.FullName,lrc, rotate_path + rotate_name);
            

            // rotation done, update status file
            if (cla.Debug == false)
            {
                Logging.Log(Strings.UpdateStatus, Logging.LogType.Verbose);
                Status.SetRotationDate(fi.FullName);
            }

            if ((lrc.PostRotate != null) && (lrc.SharedScripts == false))
                PostRotate(lrc,fi.FullName);
        }

        /// <summary>
        /// Compress a file using .Net 
        /// </summary>
        /// <param name="m_filepath">the file to compress</param>
        /// <param name="lrc">logrotateconf object</param>
        private static void CompressRotatedFile(string m_filepath, logrotateconf lrc)
        {
            int chunkSize = 65536;
            FileInfo fi = new FileInfo(m_filepath);

            if (fi.Extension == "."+lrc.CompressExt)
            {
                return;
            }

            string compressed_file_path = m_filepath + "." + lrc.CompressExt;
            Logging.Log(Strings.Compressing+" " + m_filepath, Logging.LogType.Verbose);

            if (cla.Debug == false)
            {
                try
                {
                    using (FileStream fs = new FileStream(m_filepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    {
                        using (System.IO.Compression.GZipStream zs = new System.IO.Compression.GZipStream(new FileStream(compressed_file_path, FileMode.Create), System.IO.Compression.CompressionMode.Compress))
                        {
                            byte[] buffer = new byte[chunkSize];
                            while (true)
                            {
                                int bytesRead = fs.Read(buffer, 0, chunkSize);
                                if (bytesRead == 0)
                                    break;
                                zs.Write(buffer, 0, bytesRead);
                            }
                        }
                    }

                    DeleteRotateFile(m_filepath, lrc);
                }
                catch (Exception e)
                {
                    Logging.Log("Error in CompressRotatedFile with file " + m_filepath, Logging.LogType.Error);
                    Logging.LogException(e);
                    return;
                }
            }
        }

        /// <summary>
        /// Sends a log file as an email attachment if email setttings are configured
        /// </summary>
        /// <param name="m_filepath">Path to the original log file</param>
        /// <param name="lrc">LogRotationConf object</param>
        /// <param name="m_file_attachment_path">Path to the log file to attach to the email</param>
        private static void SendEmail(string m_filepath, logrotateconf lrc, string m_file_attachment_path)
        {
            if ((lrc.SMTPServer != "") && (lrc.SMTPPort != 0) && (lrc.MailLast) && (lrc.MailAddress != ""))
            {
                Logging.Log(Strings.SendingEmailTo+" " + lrc.MailAddress,Logging.LogType.Verbose);
                Attachment a = new Attachment(m_file_attachment_path);
                try
                {
                    MailMessage mm = new MailMessage(lrc.MailFrom, lrc.MailAddress);
                    mm.Subject = "Log file rotated";
                    mm.Body = Strings.ProgramName + " has rotated the following log file: " + m_filepath + "\r\n\nThe rotated log file is attached.";

                    mm.Attachments.Add(a);
                    SmtpClient smtp = new SmtpClient(lrc.SMTPServer, lrc.SMTPPort);
                    if (lrc.SMTPUserName != "")
                    {
                        smtp.Credentials = new System.Net.NetworkCredential(lrc.SMTPUserName, lrc.SMTPUserPassword);
                    }
                    smtp.EnableSsl = lrc.SMTPUseSSL;

                    smtp.Send(mm);
                }
                catch (Exception e)
                {
                    Logging.LogException(e);
                    return;
                }
                finally
                {
                    a.Dispose();
                }
            }
        }

        private static void PrintUsage()
        {
            Console.WriteLine(Strings.Usage1);
            Console.WriteLine(Strings.Usage2);
            Console.WriteLine(Strings.Usage3);
        }

        private static void PrintVersion()
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            
            Console.WriteLine(Strings.ProgramName+" "+asm.GetName().Version.ToString()+" - " + Strings.CopyRight);
            Console.WriteLine(Strings.GNURights);
            Console.WriteLine();
        }

        private static void ProcessConfigPath(string m_path)
        {
            // if this is pointed to a folder (most lilely), then process each file in it
            if ((File.GetAttributes(m_path) & FileAttributes.Directory) == FileAttributes.Directory)
            {
                DirectoryInfo di = new DirectoryInfo(m_path);
                FileInfo[] fis = di.GetFiles("*.*", SearchOption.TopDirectoryOnly);
                foreach (FileInfo fi in fis)
                {
                    ProcessConfigFile(fi.FullName);
                }
            }
            else
            {
                ProcessConfigFile(m_path);
            }

        }

        private static void ProcessConfigFile(string m_path_to_file)
        {
            Logging.Log(Strings.ParseConfigFile+" " + m_path_to_file,Logging.LogType.Verbose);

            StreamReader sr = new StreamReader(m_path_to_file);
            bool bSawASection = false;
            // read in lines until done
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                    break;

                line = line.Trim();
                Logging.Log(Strings.ReadLine+" " + line,Logging.LogType.Debug);

                // skip blank lines
                if (line == "")
                    continue;

                // if line begins with #, then it is a comment and can be ignored
                if (line[0] == '#')
                {
                    Logging.Log(Strings.Skipping+" "+Strings.Comment,Logging.LogType.Debug);
                    continue;
                }

                // see if there is a { in the line.  If so, this is the beginning of a section 
                // otherwise it may be a global setting

                if (line.Contains("{"))
                {
                    bSawASection = true;
                    Logging.Log(Strings.Processing+" "+Strings.NewSection,Logging.LogType.Verbose);

                    // create a new config object taking defaults from Global Config
                    logrotateconf lrc = new logrotateconf(GlobalConfig);

                    ProcessConfileFileSection(line, sr, lrc);
                }
                else
                {
                    if (bSawASection == false)
                        GlobalConfig.Parse(line, cla.Debug);
                    else
                    {
                        Logging.Log(Strings.GlobalOptionsAboveSections, Logging.LogType.Error);
                    }
                }

            }
            



        }

        private static void ProcessConfileFileSection(string starting_line, StreamReader sr,logrotateconf lrc)
        {
            // the first part of the line contains the file(s) or folder(s) that will be associated with this section
            // we need to break this line apart by spaces
            string split = "";
            bool bQuotedPath = false;
            for (int i = 0; i < starting_line.Length; i++)
            {
                switch (starting_line[i])
                {
                    // if we see the open brace, we are done
                    case '{':
                        i = starting_line.Length;
                        break;
                    // if we see a ", then this is either starting or ending a file path with spaces
                    case '\"':
                        if (bQuotedPath == false)
                           bQuotedPath = true;
                        else
                           bQuotedPath = false;
                        split += starting_line[i];
                        break;
                    case ' ':
                        // we see a space and we are not processing a quote path, so this is a delimeter and treat it as such
                        if (bQuotedPath == false)
                        {
                            string newsplit = "";
                            // remove any invalid characters before adding
                            char[] invalidPathChars = Path.GetInvalidPathChars();
                            foreach (char ipc in invalidPathChars)
                            {
                                for (int ii = 0; ii < split.Length; ii++)
                                {
                                    if (split[ii] != ipc)
                                    {
                                        newsplit += split[ii];
                                    }
                                }
                                split = newsplit;
                                newsplit = "";
                            }

                            lrc.Increment_ProcessCount();
                            FilePathConfigSection.Add(split, lrc);
                            split = "";
                        }
                        else
                            split += starting_line[i];
                        break;
                    default:
                        split += starting_line[i];
                        break;
                }


            }

            /*
            string[] split = starting_line.Split(new char[] { ' ' });
            for (int i = 0; i < split.Length-1; i++)
            {
                FilePathConfigSection.Add(split[i], lrc);
            }
             */


            // read until we hit a } and process
            while (true)
            {
                string line = sr.ReadLine();
                if (line == null)
                    break;

                line = line.Trim();
                Logging.Log(Strings.ReadLine+" " + line,Logging.LogType.Debug);

                // skip blank lines
                if (line == "")
                    continue;

                // if line begins with #, then it is a comment and can be ignored
                if (line[0] == '#')
                {
                    Logging.Log(Strings.Skipping+" "+Strings.Comment,Logging.LogType.Debug);
                    continue;
                }

                if (line.Contains("}"))
                {
                    break;
                }

                lrc.Parse(line, cla.Debug);
            }

        }

    }

    class CmdLineArgs
    {
        private bool bForce = false;

        public bool Force
        {
            get { return bForce; }
        }

        private bool bVerbose = false;

        public bool Verbose
        {
            get { return bVerbose; }
        }

        private bool bUsage = false;

        public bool Usage
        {
            get { return bUsage; }
        }

        private bool bDebug = false;

        public bool Debug
        {
            get { return bDebug; }
        }

        private string sAlternateStateFile = "";
        public string AlternateStateFile
        {
            get { return sAlternateStateFile; }
        }

        private List<string> sConfigFilePaths = new List<string>();

        public List<string> ConfigFilePaths
        {
            get { return sConfigFilePaths; }
        }


        public CmdLineArgs(string[] args)
        {
            bool bWatchForState = false;
            // iterate through the args array
            foreach (string a in args)
            {
                // if the string starts with a '-' then it is a switch
                if (a[0] == '-')
                {
                    switch (a)
                    {
                        case "-d":
                            bDebug = true;
                            bVerbose = true;
                            Logging.SetDebug(true);
                            Logging.SetVerbose(true);
                            Logging.Log(Strings.DebugOptionSet);
                            Logging.Log(Strings.VerboseOptionSet);
                            break;
                        case "-f":
                        case "--force":
                            bForce = true;
                            Logging.Log(Strings.ForceOptionSet,Logging.LogType.Required);
                            break;
                        case "-?":
                        case "--usage":
                            bUsage = true;
                            break;
                        case "-v":
                        case "--verbose":
                            bVerbose = true;
                            Logging.SetVerbose(true);
                            Logging.Log(Strings.VerboseOptionSet);
                            break;
                        case "-m":
                        case "--mail":
                            Logging.Log(Strings.MailOptionSet,Logging.LogType.Error);
                            break;
                        case "-s":
                        case "--state":
                            bWatchForState = true;
                            break;
                        default:
                            // no match, so print an error
                            Logging.Log(Strings.UnknownCmdLineArg + ": "+ a , Logging.LogType.Error);
                            Environment.Exit(1);
                            break;
                    }
                }
                else
                {
                    if (bWatchForState)
                    {
                        sAlternateStateFile = a;
                        Logging.Log(Strings.AlternateStateFile+" " + a,Logging.LogType.Verbose);
                        bWatchForState = false;
                    }
                    else
                    {
                        // otherwise, it is the path to a config file or folder containing config files
                        Logging.Log(a + " " + Strings.AddingConfigFile, Logging.LogType.Verbose);
                        sConfigFilePaths.Add(a);
                    }
                }
            }
        }

    }
}
