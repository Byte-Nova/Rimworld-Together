namespace GameServer
{
    [Serializable]
    public class DiscordConfigFile
    {
        public bool Enabled = false;

        public string Token = "";

        public string ChatWebhook = "";

        public ulong ChatChannelId = 0;

        public ulong ConsoleChannelId = 0;
    }
}