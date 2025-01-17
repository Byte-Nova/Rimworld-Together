namespace GameServer.Core.Configs
{
    public class WhitelistConfigFile
    {
        public bool UseWhitelist = false;

        public List<string> WhitelistedUsers = new List<string>() { };
    }
}
