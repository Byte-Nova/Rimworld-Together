using System;

namespace GameServer.Core.Configs
{
    [Serializable]
    public class DiscordConfigFile
    {
        public bool Enabled = false;
        public string BotToken = "";
        public ulong ChatChannelId = 0;
        public ulong ConsoleChannelId = 0;
        public bool UseOnlineCount = true;
    }
}