// Program.cs
// Created by MUHAMMAD ABUBAKAR
// Created: 2015-09-19 12:26 PM
// Modified: 2015-09-19 4:22 PM

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
using System.Diagnostics;
using System.ServiceProcess;

#endregion

namespace Logrotate
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // parse the command line
            try
            {
                if (Environment.UserName.Equals("System", StringComparison.OrdinalIgnoreCase))
                {
                    ServiceBase[] ServicesToRun;
                    ServicesToRun = new ServiceBase[]
                    {
                        new LogrotateService()
                    };
                    ServiceBase.Run(ServicesToRun);
                }
                else
                {
                    Rotater rotater = new Rotater(args);
                    rotater.Process();
                }
            }
            catch (Exception e)
            {
                Logging.LogException(e);
                Environment.Exit(1);
            }
        }
    }
}