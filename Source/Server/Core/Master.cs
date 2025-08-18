using Shared;
using Shared.Files;

namespace GameServer.Core
{
    public static class Master
    {
        public static string MainPath { get; set; } = Directory.GetCurrentDirectory();

        public static string AssetsPath { get; set; } = Path.Combine(Master.MainPath, "Assets");

        public static string BackupsPath { get; set; } = Path.Combine(Master.MainPath, "Backups");

        public static string BackupServerPath { get; set; } = Path.Combine(Master.BackupsPath, "Servers");

        public static string BackupUsersPath { get; set; } = Path.Combine(Master.BackupsPath, "Users");

        public static string ConfigsPath { get; set; } = Path.Combine(Master.MainPath, "Configs");

        public static string LogsPath { get; set; } = Path.Combine(Master.MainPath, "Logs");

        public static string SystemLogsPath { get; set; } = Path.Combine(Master.LogsPath, "System");

        public static string ChatLogsPath { get; set; } = Path.Combine(Master.LogsPath, "Chat");

        public static string TempPath { get; set; } = Path.Combine(Master.MainPath, "Temp");

        public static string MapsPath { get; set; } = Path.Combine(Master.AssetsPath, "Maps");

        public static string UsersPath { get; set; } = Path.Combine(Master.AssetsPath, "Users");

        public static string SavesPath { get; set; } = Path.Combine(Master.AssetsPath, "Saves");

        public static string SitesPath { get; set; } = Path.Combine(Master.AssetsPath, "Sites");

        public static string FactionsPath { get; set; } = Path.Combine(Master.AssetsPath, "Factions");

        public static string SettlementsPath { get; set; } = Path.Combine(Master.AssetsPath, "Settlements");

        public static string EventsPath { get; set; } = Path.Combine(Master.AssetsPath, "Events");

        public static string WorldPath { get; set; } = Path.Combine(Master.AssetsPath, "World");

        public static string CompatibilityPatchesPath { get; set; } = Path.Combine(Master.AssetsPath, "Patches");

        //References

        public static WhitelistConfigFile Whitelist { get; set; } = null;

        public static SiteValuesFile SiteValues { get; set; } = null;

        public static WorldValuesFile WorldValues { get; set; } = null;

        public static ServerConfigFile ServerConfig { get; set; } = null;

        public static ActionValuesFile ActionConfigs { get; set; } = null;

        public static DifficultyValuesFile DifficultyValues { get; set; } = null;

        public static StorytellerValuesFile StorytellerValues { get; set; } = null;

        public static ScenarioValuesFile ScenarioValues { get; set; } = null;

        public static RoadValuesFile RoadValues { get; set; } = null;

        public static BackupConfigFile BackupConfig { get; set; } = null;

        public static ModConfigFile ModConfig { get; set; } = null;

        public static ChatConfigFile ChatConfig { get; set; } = null;

        public static ServerBrowserConfig ServerBrowserConfig { get; set; } = null;

        //Booleans

        public static bool IsClosing { get; set; } = false;
    }
}
