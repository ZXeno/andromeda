﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Andromeda.Model;

namespace Andromeda.Logic.Command
{
    public class CleanComputerCaches : Action
    {
        private const string CcmCachePath = "\\Windows\\ccmcache";
        private const string UsersDirectory = "\\users";
        private const string UserTemp = "\\AppData\\Local\\Temp";
        private const string UserTempInternetFiles = "\\AppData\\Local\\Microsoft\\Windows\\Temporary Internet Files";
        private const string WindowsTemp = "\\Windows\\Temp";

        public CleanComputerCaches()
        {
            ActionName = "Clean Computer Temp/Cache Files";
            Description = "Cleans the temp and caches files on a remote device.";
            Category = ActionGroup.Other;
        }

        public override void RunCommand(string rawDeviceList)
        {
            List<string> devlist = ParseDeviceList(rawDeviceList);
            List<string> confirmedConnectionList = GetPingableDevices.GetDevices(devlist);
            List<string> failedlist = new List<string>();

            UpdateProgressBarForFailedConnections(devlist, confirmedConnectionList);

            foreach (var device in confirmedConnectionList)
            {
                if (ValidateDirectoryExists(device, CcmCachePath))
                {
                    CleanDirectory(device, CcmCachePath);
                }

                if (ValidateDirectoryExists(device, WindowsTemp))
                {
                    CleanDirectory(device, CcmCachePath);
                }

                List<string> userDirPaths = Directory.EnumerateDirectories("\\\\" + device + "\\C$" + UsersDirectory).ToList();
                List<string> userFolders = new List<string>();

                // Create useable paths
                foreach (var userDir in userDirPaths)
                {
                    var cleanedPath = UsersDirectory + "\\" + userDir.Substring(userDir.LastIndexOf("\\") + 1);
                    userFolders.Add(cleanedPath);
                }

                foreach (var userFolder in userFolders)
                {
                    if (ValidateDirectoryExists(device, userFolder))
                    {
                        // Validate and Clean User Temp Folder at "c:\users\[user]\appdata\local\temp"
                        if (ValidateDirectoryExists(device, userFolder + UserTemp))
                        {
                            CleanDirectory(device, userFolder + UserTemp);
                        }

                        // Validate and Clean User Temporary Internet Files at "C:\Users\[user]\AppData\Local\Microsoft\Windows\Temporary Internet Files"
                        if (ValidateDirectoryExists(device, userFolder + UserTempInternetFiles))
                        {
                            CleanDirectory(device, userFolder + UserTemp);
                        }
                    }
                }

                ProgressData.OnUpdateProgressBar(1);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}