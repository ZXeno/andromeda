﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AndromedaActions.View;
using AndromedaActions.ViewModel;
using AndromedaCore.Infrastructure;
using AndromedaCore.Managers;
using AndromedaCore.ViewModel;
using Microsoft.Win32;
using Action = AndromedaCore.Action;

namespace AndromedaActions.Command
{
    public class SccmRemoteAccessRegModify : Action
    {
        private readonly IWindowService _windowService;
        private readonly IRegistryServices _registry;

        private const string SccmRemoteControlRegistryPath = "SOFTWARE\\Microsoft\\SMS\\Client\\Client Components\\Remote Control";
        private const string RemoteAccessEnabledKeyName = "Enabled";
        private const string RequiresUserApprovalKeyName = "Permission Required";
        private const string ShowConnectionBannerKeyName = "RemCtrl Connection Bar";
        private const string ShowTaskbarIconKeyName = "RemCtrl Taskbar Icon";
        private const string AllowAccessOnUnattendedComputersKeyName = "Allow Remote Control of an unattended computer";
        private const string AllowLocalAdministratorsToRemoteControlKeyName = "Allow Local Administrators to do Remote Control";
        private const string AudibleSignalKeyName = "Audible Signal";

        private bool _remoteAccessEnabled;
        private bool _requiresUserApproval;
        private bool _showConnectionBanner;
        private bool _showTaskBarIcon;
        private bool _allowAccessOnUnattended;
        private bool _allowLocalAdministratorsToRemoteControl;
        private bool _audibleSignal = false;

        private List<string> _parsedListCache;

        public SccmRemoteAccessRegModify(
            ILoggerService logger, 
            INetworkServices networkServices, 
            IFileAndFolderServices fileAndFolderServices, 
            IRegistryServices registryServices, 
            IWindowService windowService) : base(logger, networkServices, fileAndFolderServices)
        {
            _windowService = windowService;
            _registry = registryServices;

            ActionName = "SCCM Remote Access Registry Modify";
            Description = "Changes the remote access options for SCCM remote control.";
            Category = "SCCM";

            HasUserInterfaceElement = true;
            UiCallback = CallbackMethod;
        }

        public override void OpenUserInterfaceElement(string rawDeviceList)
        {
            _parsedListCache = ParseDeviceList(rawDeviceList);

            var sccmRegHackContext = new SccmRegHackOptionViewModel();
            _windowService.ShowDialog<SccmRegHackOptionsPrompt>(sccmRegHackContext);

            if (!sccmRegHackContext.Result)
            {
                var msg = $"Action {ActionName} canceled by user.";
                Logger.LogMessage(msg);
                ResultConsole.AddConsoleLine(msg);
                CancellationToken.Cancel();
            }

            _remoteAccessEnabled = sccmRegHackContext.RemoteAccessEnabled;
            _requiresUserApproval = sccmRegHackContext.RequiresUserApproval;
            _showConnectionBanner = sccmRegHackContext.ShowConnectionBanner;
            _showTaskBarIcon = sccmRegHackContext.ShowTaskbarIcon;
            _allowAccessOnUnattended = sccmRegHackContext.AllowAccessOnUnattended;
            _allowLocalAdministratorsToRemoteControl = sccmRegHackContext.AllowLocalAdministratorsToRemoteControl;
        }

        private void CallbackMethod()
        {
            var failedlist = new List<string>();

            try
            {
                Parallel.ForEach(_parsedListCache, (device) =>
                {
                    CancellationToken.Token.ThrowIfCancellationRequested();

                    if (!NetworkServices.VerifyDeviceConnectivity(device))
                    {
                        failedlist.Add(device);
                        ResultConsole.Instance.AddConsoleLine($"Device {device} failed connection verification. Added to failed list.");
                        return;
                    }

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        RemoteAccessEnabledKeyName,
                        BoolToIntString(_remoteAccessEnabled).ToString(),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        RequiresUserApprovalKeyName,
                        BoolToIntString(_requiresUserApproval),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        ShowConnectionBannerKeyName,
                        BoolToIntString(_showConnectionBanner),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        ShowTaskbarIconKeyName,
                        BoolToIntString(_showTaskBarIcon),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AllowAccessOnUnattendedComputersKeyName,
                        BoolToIntString(_allowAccessOnUnattended),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AllowLocalAdministratorsToRemoteControlKeyName,
                        BoolToIntString(_allowLocalAdministratorsToRemoteControl),
                        RegistryValueKind.DWord);

                    _registry.WriteToSubkey(
                        device,
                        RegistryHive.LocalMachine,
                        SccmRemoteControlRegistryPath,
                        AudibleSignalKeyName,
                        BoolToIntString(_audibleSignal),
                        RegistryValueKind.DWord);
                });
            }
            catch (OperationCanceledException e)
            {
                ResetCancelToken(ActionName, e);
            }

            if (failedlist.Count > 0)
            {
                WriteToFailedLog(ActionName, failedlist);
            }
        }

        public override void RunCommand(string rawDeviceList)
        {
            throw new NotImplementedException($"{ActionName} has a user interface element and does utilize the RunCommand method interface.");
        }

        private string BoolToIntString(bool value)
        {
            return value ? "1" : "0";
        }
    }
}