namespace GameServer
{
    [Serializable]
    public class ServerConfigFile
    {
        public string IP = "0.0.0.0";

        public string Port = "25555";

        public string MaxPlayers = "100";

        public string MaxTimeoutInMS = "5000";

        public bool VerboseLogs = false;

        public bool DisplayChatInConsole = false;
    }
}
