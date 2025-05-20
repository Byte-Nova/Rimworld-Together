
using System;

namespace GameServer.Core.Configs
{
    [Serializable]
    public class DiscordConfigFile
    {
        // Master switch – if false, Discord integration is skipped
        public bool Enabled = false;

        // Your Bot’s token from the Discord Developer Portal
        public string BotToken = "YOUR_DISCORD_BOT_TOKEN";

        public ulong ChatChannelId = 0UL;

        public ulong ConsoleChannelId = 0UL;

        public bool UseOnlineCount = true;
    }
}