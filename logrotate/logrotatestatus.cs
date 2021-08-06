using System;
using System.IO;

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
    /// <summary>
    /// This class encapsulates the status file, handling updates, etc.
    /// </summary>
    class logrotatestatus
    {
        private readonly string sfile_path;

        private DateTime lastmod;

        public logrotatestatus()
        {
            GetStatus_LastModDate();
        }

        public logrotatestatus(string m_path)
        {
            if (m_path != "")
                sfile_path = m_path;
            else
            {
                string[] args = Environment.GetCommandLineArgs();
                sfile_path = Path.Combine(Path.GetDirectoryName(args[0]), "logrotate.status");
            }
            // see if the file exists.  if not, create a blank one
            if (File.Exists(sfile_path) == false)
            {
                StreamWriter sw = File.CreateText(sfile_path);
                sw.WriteLine("# logrotate state file created " + DateTime.Now);
                sw.WriteLine("logrotate state -- version 2");
                sw.Close();
            }

            GetStatus_LastModDate();
            Logging.Log(Strings.StateFileLocation + " " + Path.GetFullPath(sfile_path), Logging.LogType.Verbose);
        }

        public DateTime GetStatus_LastModDate()
        {
            lastmod = File.GetLastWriteTime(sfile_path);
            return lastmod;
        }

        public void SetRotationDate(string m_log_path)
        {
            // first need to see if the m_log_path is in the file.  If so, update it.  Otherwise append to the end
            string[] lines = File.ReadAllLines(sfile_path);
            string[] stringSeparator = new string[] { "\" " };
            bool bFound = false;
            for (int i = 0; i < lines.Length; i++)
            {
                string[] splitline = lines[i].Split(stringSeparator,StringSplitOptions.None);
                if (splitline[0] == "\"" + m_log_path)
                {
                    // found the line, replace the data
                    lines[i] = "\"" + m_log_path + "\" " + DateTime.Now.ToString("yyyy-M-d");
                    bFound = true;
                    break;
                }

            }
            if (bFound == false)
            {
                // didn't find it, so add
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = "\"" + m_log_path + "\" " + DateTime.Now.ToString("yyyy-M-d");
            }

            // write changes back out to file
            File.WriteAllLines(sfile_path, lines);
        }

        public DateTime GetRotationDate(string m_log_path)
        {
            // read in file, see if the log file name is in it.  if so, return the date.
            // if not, return today's date
            string[] lines = File.ReadAllLines(sfile_path);
            string[] stringSeparator = new string[] { "\" " };
            for (int i = 0; i < lines.Length; i++)
            {
                string[] splitline = lines[i].Split(stringSeparator, StringSplitOptions.None);
                //some runtime versions mess arround with backslash names so better replace them all
                if (splitline[0].Replace("\\", "/") == "\"" + m_log_path.Replace("\\", "/"))
                {
                    string[] splitdate = splitline[1].Split(new char[] { '-' });
                    return new DateTime(Convert.ToInt32(splitdate[0]), Convert.ToInt32(splitdate[1]), Convert.ToInt32(splitdate[2]));
                }
            }

            // if we get here, we didn't find it, so we need to force a rotate.  returns back a very old date
            Logging.Log(Strings.NoStatusDate + " " + m_log_path, Logging.LogType.Verbose);
            return new DateTime(1970, 1, 1);
        }
    }
}
