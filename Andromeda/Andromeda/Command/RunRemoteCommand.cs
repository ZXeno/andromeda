﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Management;
using System.Windows;

namespace Andromeda.Command
{
    public class RunRemoteCommand : Action
    {

        NetworkConnections netconn;

        public RunRemoteCommand()
        {
            ActionName = "Run Command Remotely";
            Desctiption = "Run any console command remotely, as specified credentials.";
            Category = ActionGroup.Other;
            netconn = new NetworkConnections();
        }


        public override void RunCommand(string deviceList) 
        {
            string wmiscope = "\\root\\cimv2";
            string processToRun = "";
            List<string> devices = ParseDeviceList(deviceList);
            ConnectionOptions connOps = new ConnectionOptions();
            if (!CredentialManager.IsImpersonationEnabled)
            {
                connOps.Username = CredentialManager.UserName;
                connOps.Password = CredentialManager.ExtractSecureString(CredentialManager.Password);
            }
            else
            {
                connOps.Impersonation = ImpersonationLevel.Impersonate;
            }

            CLI_Prompt newPrompt = new CLI_Prompt();
            newPrompt.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            newPrompt.Owner = App.Current.MainWindow;
            newPrompt.ShowDialog();

            try
            {
                if (!newPrompt.WasCanceled)
                {
                    processToRun = newPrompt.TextBoxContents;
                    newPrompt = null;
                }
                else if (newPrompt.WasCanceled)
                {
                    newPrompt = null;
                    ResultConsole.AddConsoleLine("Run Command Remotely action was canceled.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error running this command. \n " + ex.Message);
                ResultConsole.AddConsoleLine("Command failed with exception error caught: \n" + ex.Message);
            }

            foreach (string d in devices)
            {
                ResultConsole.AddConsoleLine(RunOnDevice(d, wmiscope, processToRun, connOps));
            }

            wmiscope = "";
            processToRun = "";
        }

        private string RunOnDevice(string d, string scope, string process, ConnectionOptions options)
        {
            ManagementScope deviceWMI;
            string result = "";
            var processToRun = new[] { process };

            if (netconn.CheckWMIAccessible(d, scope, options))
            {
                deviceWMI = netconn.ConnectToRemoteWMI(d, scope, options);
                ManagementPath p = new ManagementPath("Win32_Process");
                ManagementClass wmiProcess = new ManagementClass(deviceWMI, p, new ObjectGetOptions());
                ManagementClass startupSettings = new ManagementClass("Win32_ProcessStartup");
                startupSettings.Scope = deviceWMI;
                ManagementBaseObject inParams = wmiProcess.GetMethodParameters("Create");
                inParams["CommandLine"] = process;
                inParams["ProcessStartupInformation"] = startupSettings;
                ManagementBaseObject outValue = wmiProcess.InvokeMethod("Create", inParams, null);
                string retval = outValue["ReturnValue"].ToString();
                ResultConsole.AddConsoleLine(d + "returned exit code: " + retval);
            }
            else
            {
                result = "Unable to connect to device " + d + ".";
            }

            return result;
        }
    }
}
