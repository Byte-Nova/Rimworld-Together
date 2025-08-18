using Shared;
using Shared.Files;
using System;
using System.IO;

namespace Shared.Files
{
    public class ServerConfigFile : BaseFile
    {
        public static string Path { get; set; } = string.Empty;

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

        public bool TemporalActivityProtection { get; set; } = false;

        public int TemporalActivityProtectionTime { get; set; } = 3600;

        public bool TemporalEventProtection { get; set; } = true;

        public int TemporalEventProtectionTime { get; set; } = 3600;

        public bool TemporalAidProtection { get; set; } = false;

        public int TemporalAidProtectionTime { get; set; } = 3600;

        public bool TemporalSpyProtection { get; set; } = true;

        public int TemporalSpyProtectionTime { get; set; } = 3600;

        public override void Save()
        {
            try { Serializer.SerializeToFile(Path, this); }
            catch (Exception e) { throw new Exception(e.ToString()); }
        }

        public static object Load<T>()
        {
            if (File.Exists(Path)) return Serializer.SerializeFromFile<T>(Path);
            else
            {
                ServerConfigFile file = new ServerConfigFile();
                Serializer.SerializeToFile(Path, file);
                return file;
            }
        }
    }
}
