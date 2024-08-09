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

        public bool AllowCustomScenarios = true;

        public bool AllowNPCDestruction = false;

        public bool TemporalActivityProtection = true;

        public bool TemporalEventProtection = true;

        public bool TemporalAidProtection = false;

        public Discord DiscordIntegration = new();
    }

    public class Discord
    {
        public bool Enabled = false;
        public string Token = "";
        public ulong ChatChannelId = 0;
        public ulong ConsoleChannelId = 0;
        public string ChatWebhook = "";
        public bool ShowPlayerCount = true;
    }
}
