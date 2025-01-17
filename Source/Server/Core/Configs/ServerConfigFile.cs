namespace GameServer.Core.Configs
{
    [Serializable]
    public class ServerConfigFile
    {
        public string Name = "RimWorld Together Server";

        public string IP = "0.0.0.0";

        public string Port = "25555";

        public string MaxPlayers = "100";

        public string MaxTimeoutInMS = "30000";

        public bool VerboseLogs = false;

        public bool ExtremeVerboseLogs = false;

        public bool DisplayChatInConsole = false;

        public bool UseUPnP = false;

        public bool SyncLocalSave = true;

        public bool TemporalActivityProtection = false;

        public int TemporalActivityProtectionTime = 3600;

        public bool TemporalEventProtection = true;

        public int TemporalEventProtectionTime = 3600;

        public bool TemporalAidProtection = false;

        public int TemporalAidProtectionTime = 3600;
    }
}
