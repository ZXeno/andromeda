﻿using Microsoft.Win32;

namespace AndromedaCore.Infrastructure
{
    public interface IRegistryServices
    {
        bool ValidateKeyExists(string targetDevice, RegistryHive baseKey, string targetSubKey);
        bool ValidateKeyValueExists(RegistryKey subKey, string targetKeyValue, string expectedName);
        RegistryKey GetRegistryKey(string targetDevice, RegistryHive baseKey, string targetSubKey);
        void DeleteSubkeyTree(string targetDevice, RegistryHive baseKey, string targetSubKey);
        void WriteToSubkey(string targetDevice, RegistryHive baseKey, string targetSubKey, string name, string value, RegistryValueKind keyKind);
    }
}