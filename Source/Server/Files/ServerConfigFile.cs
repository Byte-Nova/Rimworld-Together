namespace RimworldTogether.GameServer.Files
{
    [Serializable]
    public class ServerConfigFile
    {
        public string IP = "0.0.0.0";

        public string Port = "25555";

        public string MaxPlayers = "100";

        public bool verboseLogs = false;
    }
}
