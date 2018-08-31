using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

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
    class ShredFile
    {
        #region p/invoke
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool GetDiskFreeSpace(string lpRootPathName,
           out uint lpSectorsPerCluster,
           out uint lpBytesPerSector,
           out uint lpNumberOfFreeClusters,
           out uint lpTotalNumberOfClusters);
        #endregion

        /// <summary>
        /// path to the file we want to shred
        /// </summary>
        private string sfile_path;

        /// <summary>
        /// Shred a file by writing random data to it
        /// </summary>
        /// <param name="m_path">the full path to the file.  Can be a UNC path.  Will throw ArgumentException if file does not exist</param>
        public ShredFile(string m_path)
        {
            if (File.Exists(m_path) == false)
                throw new ArgumentException(m_path + " " + Strings.CouldNotBeFound);
            sfile_path = m_path;
        }

        /// <summary>
        /// Shreds the file using random garbage data and multiple overwrites
        /// </summary>
        /// <param name="iShredCycles">number of shred overwrites to perform</param>
        /// <param name="bDebug">flag indicating if this is in debug mode (file will not be shredded if in debug mode)</param>
        /// <remarks>Will throw InvalidOperationException if it errors invoking Win32 API GetDiskFreeSpace</remarks>
        /// <returns>True if file was shredded, otherwise false</returns>
        public bool ShredIt(int iShredCycles, bool bDebug)
        {
            Logging.Log(Strings.ShreddingFile+" " + sfile_path + " ShredCycles = " + iShredCycles,Logging.LogType.Debug);

            try
            {

                // set attributes in case the file is readonly for some reason
                File.SetAttributes(sfile_path, FileAttributes.Normal);

                // get the size of a sector on the disk where the file is located
                uint SectorsPerCluster;
                uint BytesPerSector;
                uint NumberofFreeClusters;
                uint TotalNumberOfClusters;
                if (!GetDiskFreeSpace(Path.GetPathRoot(sfile_path), out SectorsPerCluster, out BytesPerSector, out NumberofFreeClusters, out TotalNumberOfClusters))
                    throw new InvalidOperationException("Error calling Win32 API GetDiskFreeSpace Root Path = " + Path.GetPathRoot(sfile_path));

                //Logging.Log("Number of bytes per sector for " + Path.GetPathRoot(sfile_path) + " is " + BytesPerSector, Logging.LogType.Debug);

                if (bDebug == false)
                {

                    FileInfo fi = new FileInfo(sfile_path);
                    // calculate number of sectors in the file based on sector size
                    double SectorsInFile = Math.Ceiling((double)(fi.Length / BytesPerSector));

                    // create a buffer equal to the size of a sector
                    byte[] dummyBuffer = new byte[BytesPerSector];

                    // Create a crypto random number generator to make random garbage data
                    RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

                    // open file and write data iShredCycles to it
                    FileStream fs = new FileStream(sfile_path, FileMode.Open);
                    for (int ipass = 0; ipass < iShredCycles; ipass++)
                    {
                        // go to beginning of file
                        fs.Position = 0;
                        for (int isectors = 0; isectors < SectorsInFile; isectors++)
                        {
                            // fill our buffer with random data
                            rng.GetBytes(dummyBuffer);
                            // write it to the file
                            fs.Write(dummyBuffer, 0, dummyBuffer.Length);
                        }
                    }

                    // truncate the file
                    fs.SetLength(0);

                    fs.Close();

                    // change the dates of the file to help prevent recovery
                    DateTime dt = new DateTime(2037, 1, 1, 0, 0, 0);
                    File.SetCreationTime(sfile_path, dt);
                    File.SetLastAccessTime(sfile_path, dt);
                    File.SetLastWriteTime(sfile_path, dt);

                    // delete the file
                    File.Delete(sfile_path);
                    //Logging.Log(sfile_path + " has been shredded", Logging.LogType.Debug);
                }

            }
            catch (Exception e)
            {
                Logging.LogException(e);
                return false;
            }

            return true;

        }
    }
}
