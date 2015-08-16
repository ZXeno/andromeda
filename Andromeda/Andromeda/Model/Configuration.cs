﻿namespace Andromeda.Model
{
    public class Configuration
    {
        public string DataFilePath { get; set; }
        public bool SaveOfflineComputers { get; set; }
        public bool SaveOnlineComputers { get; set; }
        public bool AlwaysDumpConsoleHistory { get; set; }
        public string ResultsDirectory { get; set; }
        public string ComponentDirectory { get; set; }
        public string FailedConnectListFile { get; set; }
        public string SuccessfulConnectionListFile { get; set; }
    }
}