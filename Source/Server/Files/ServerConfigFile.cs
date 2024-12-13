namespace GameServer
{
    [Serializable]
    public class ServerConfigFile
    {
        public string IP = "0.0.0.0";

        public string Port = "25555";

        public string MaxPlayers = "100";

        public string MaxTimeoutInMS = "30000";

        public bool VerboseLogs = false;

        public bool ExtremeVerboseLogs = false;

        public bool DisplayChatInConsole = false;

        public bool UseUPnP = false;

        public bool SyncLocalSave = true;

        public bool TemporalActivityProtection = true;

        public bool TemporalEventProtection = true;

        public bool TemporalAidProtection = false;
    }
}
