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
        ConnectionOptions connOps;
        WMIFuncs wmi;

        public RunRemoteCommand()
        {
            ActionName = "Run Command Remotely";
            Desctiption = "Run any console command remotely, as specified credentials.";
            Category = ActionGroup.Other;
            netconn = new NetworkConnections();
            wmi = new WMIFuncs();
            connOps = new ConnectionOptions();
        }


        public override void RunCommand(string deviceList) 
        {
            string wmiscope = "\\root\\cimv2";
            string processToRun = "";
            List<string> devices = ParseDeviceList(deviceList);
            if (!CredentialManager.IsImpersonationEnabled)
            {
                connOps.Username = CredentialManager.UserName;
                connOps.Password = CredentialManager.ExtractSecureString(CredentialManager.Password);
            }
            else
            {
                connOps = CredentialManager.GetImpersonatedConnOptions();
                
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
                    //processToRun = processToRun.Replace("\\", "\\\\");
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
            //var processToRun = new[] { process };

            try
            {
                if (wmi.CheckWMIAccessible(d, scope, options))
                {
                    deviceWMI = wmi.ConnectToRemoteWMI(d, scope, options);
                    ManagementPath p = new ManagementPath("Win32_Process");
                    ManagementClass wmiProcess = new ManagementClass(deviceWMI, p, null);
                    ManagementClass startupSettings = new ManagementClass("Win32_ProcessStartup");
                    startupSettings.Scope = deviceWMI;
                    //startupSettings["CreateFlags"] = 16777216;
                    ManagementBaseObject inParams = wmiProcess.GetMethodParameters("Create");
                    inParams["CommandLine"] = process;
                    inParams["ProcessStartupInformation"] = startupSettings;
                    ManagementBaseObject outValue = wmiProcess.InvokeMethod("Create", inParams, null);
                    
                    //string retval = outValue.Properties.ToString();
                    foreach (var v in outValue.Properties)
                    {
                        ResultConsole.AddConsoleLine(v.Name.ToString() + "  " + v.Value.ToString());
                    }

                    result = d + " returned exit code: " + outValue["ReturnValue"].ToString();
                }
                else
                {
                    result = "Unable to connect to device " + d + ".";
                }

                return result;
            }
            finally
            {
                result = string.Empty;
                deviceWMI = null;
            }
        }
    }
}
