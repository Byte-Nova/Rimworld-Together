using Shared.Files;

namespace GameServer.Files
{
    public class ServerConfigFile : FL_Base
    {
        public static string SavePath { get; set; } = string.Empty;

        public string Name { get; set; } = "RimWorld Together Server";

        public string Description { get; set; } = string.Empty;

        public string DiscordURL { get; set; } = string.Empty;

        public string SteamWorkshopURL { get; set; } = string.Empty;

        public string IP { get; set; } = string.Empty;

        public int Port { get; set; } = 25555;

        public int MaxPlayers { get; set; } = 100;

        public int Verbosity { get; set; } = 0;

        public bool DisplayChatInConsole { get; set; } = false;

        public bool UseUPnP { get; set; } = false;

        public bool SyncLocalSave { get; set; } = true;

        public bool EnableServerBrowser { get; set; } = true;

        public bool EnableServerTelemetry { get; set; } = true;
    }
}
