﻿using System.Management;

namespace Andromeda_Actions_Core.Infrastructure
{
    public interface IWmiServices
    {
        string RootNamespace { get; }
        ManagementScope ConnectToRemoteWmi(string hostname, string scope, ConnectionOptions options);
        string GetProcessReturnValueText(int retval);
        bool RepairRemoteWmi(string hostname);
        bool KillRemoteProcessByName(string device, string procName, ManagementScope remote);
        bool PerformRemoteUninstallByName(string device, string prodName, ManagementScope remote);
        bool PerformRemoteUninstallByProductId(string device, string prodId, ManagementScope remote);
        void ForceRebootRemoteDevice(string device, ManagementScope remote);

    }
}