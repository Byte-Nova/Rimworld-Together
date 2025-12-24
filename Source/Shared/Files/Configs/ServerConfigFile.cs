using System;
using System.IO;

namespace Shared.Files.Configs
{
    public class ServerConfigFile : BaseFile
    {
        public static string SavePath { get; set; } = string.Empty;

        public string Name { get; set; } = "RimWorld-Together-Server";

        public string Description { get; set; } = "My new Rimworld Together Server!";

        public string IP { get; set; } = "0.0.0.0";

        public string Port { get; set; } = "25555";

        public string MaxPlayers { get; set; } = "100";

        public bool VerboseLogs { get; set; } = false;

        public bool ExtremeVerboseLogs { get; set; } = false;

        public bool DisplayChatInConsole { get; set; } = false;

        public bool UseUPnP { get; set; } = false;

        public bool SyncLocalSave { get; set; } = true;

        public bool EnableDiscordBridge { get; set; } = false;

        public string DiscordBotToken { get; set; } = string.Empty;

        public string DiscordChatChannelId { get; set; } = string.Empty;

        public string DiscordAdminChannelId { get; set; } = string.Empty;

        public string DiscordCommandPrefix { get; set; } = "!";

        public string DiscordAdminRoleIdsCsv { get; set; } = string.Empty;

        public override void Save()
        {
            try { Serializer.SerializeToFile(SavePath, this); }
            catch (Exception e) { throw new Exception(e.ToString()); }
        }

        public static object Load<T>()
        {
            if (File.Exists(SavePath)) return Serializer.SerializeFromFile<T>(SavePath);
            else
            {
                ServerConfigFile file = new ServerConfigFile();
                Serializer.SerializeToFile(SavePath, file);
                return file;
            }
        }
    }
}