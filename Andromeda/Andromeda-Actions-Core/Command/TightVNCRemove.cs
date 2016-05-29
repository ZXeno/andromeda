﻿using System;
using System.Collections.Generic;
using System.Management;
using Andromeda_Actions_Core.Infrastructure;

namespace Andromeda_Actions_Core.Command
{
    public class TightVNCRemove : Action
    {
        private readonly string _processName = "tvnserver.exe";
        private readonly string _productName = "TightVNC";

        private readonly IWmiServices _wmiServices;

        public TightVNCRemove(INetworkServices networkServices, IFileAndFolderServices fileAndFolderServices, IWmiServices wmiServices) : base(networkServices, fileAndFolderServices)
        {
            ActionName = "TightVNC Remove";
            Description = "Removes TightVNC from the specified computers. [Requires Credentials]";
            Category = ActionGroup.Other;

            _wmiServices = wmiServices;
        }

        public override void RunCommand(string a)
        {
            var devlist = ParseDeviceList(a);
            var failedlist = new List<string>();

            if (!CredentialManager.Instance.CredentialsAreValid)
            {
                ResultConsole.AddConsoleLine("You must enter your username and password for this command to work.");
                ResultConsole.AddConsoleLine($"{ActionName} was canceled due to invalid credentials.");
                Logger.Log($"Tried to run {ActionName} but there were no credentials added.");
                return;
            }

            var connOps = new ConnectionOptions
            {
                Username = CredentialManager.Instance.UserCredentials.User,
                SecurePassword = CredentialManager.Instance.UserCredentials.SecurePassword,
                Impersonation = ImpersonationLevel.Impersonate
            };

            try
            {
                foreach (var device in devlist)
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        continue;
                    }

                    var remote = _wmiServices.ConnectToRemoteWmi(device, _wmiServices.RootNamespace, connOps);
                    if (remote != null)
                    {
                        var procquery =
                            new SelectQuery($"select * from Win32_process where name = '{_processName}'");
                        var productquery = new SelectQuery($"select * from Win32_product where name='{_productName}'");

                        using (var searcher = new ManagementObjectSearcher(remote, procquery))
                        {
                            foreach (ManagementObject process in searcher.Get()) // this is the fixed line
                            {
                                process.InvokeMethod("Terminate", null);
                                ResultConsole.AddConsoleLine($"Called process terminate ({process["Name"]}) on device {device}.");
                                Logger.Log($"Called process terminate ({process["Name"]}) on device {device}.");
                            }
                        }

                        using (var searcher = new ManagementObjectSearcher(remote, productquery))
                        {
                            foreach (ManagementObject product in searcher.Get()) // this is the fixed line
                            {
                                product.InvokeMethod("uninstall", null);
                                ResultConsole.AddConsoleLine($"Called uninstall on device {device}.");
                                Logger.Log($"Called uninstall on device {device}.");
                            }
                        }
                    }
                    else
                    {
                        ResultConsole.AddConsoleLine($"Error connecting to WMI scope {device}. Process aborted for this device.");
                        Logger.Log($"Error connecting to WMI scope {device}. Process aborted for this device.");
                        failedlist.Add(device);
                    }

                }
            }
            catch (OperationCanceledException e)
            {
                ResultConsole.AddConsoleLine($"Operation {ActionName} canceled.");
                Logger.Log($"Operation {ActionName} canceled by user request. {e.Message}");
                ResetCancelToken();
            }
            
            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }
    }
}