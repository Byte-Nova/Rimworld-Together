using RTServer.Files;
using RTShared.Files;
using RTShared.Files.Configs;
using RTShared.Files.Marketplace;

namespace RTServer.Core
{
    public static class Master
    {
        public static string MainPath { get; set; } = Directory.GetCurrentDirectory();

        public static string AssetsPath { get; set; } = Path.Combine(Master.MainPath, "Assets");

        public static string BackupsPath { get; set; } = Path.Combine(Master.MainPath, "Backups");

        public static string BackupServerPath { get; set; } = Path.Combine(Master.BackupsPath, "Servers");

        public static string BackupUsersPath { get; set; } = Path.Combine(Master.BackupsPath, "Users");

        public static string ConfigsPath { get; set; } = Path.Combine(Master.MainPath, "Configs");
        
        public static string ActionsPath { get; set; } = Path.Combine(Master.ConfigsPath, "Actions");

        public static string LogsPath { get; set; } = Path.Combine(Master.MainPath, "Logs");

        public static string SystemLogsPath { get; set; } = Path.Combine(Master.LogsPath, "System");

        public static string ChatLogsPath { get; set; } = Path.Combine(Master.LogsPath, "Chat");

        public static string MapsPath { get; set; } = Path.Combine(Master.AssetsPath, "Maps");

        public static string WorldObjectsPath { get; set; } = Path.Combine(Master.AssetsPath, "WorldObjects");

        public static string UsersPath { get; set; } = Path.Combine(Master.AssetsPath, "Users");

        public static string SavesPath { get; set; } = Path.Combine(Master.AssetsPath, "Saves");

        public static string SitesPath { get; set; } = Path.Combine(Master.AssetsPath, "Sites");

        public static string GuildsPath { get; set; } = Path.Combine(Master.AssetsPath, "Guilds");

        public static string SettlementsPath { get; set; } = Path.Combine(Master.AssetsPath, "Settlements");

        public static string EventsPath { get; set; } = Path.Combine(Master.AssetsPath, "Events");

        //References

        public static FL_PasswordConfig PasswordConfig { get; set; } = null;

        public static FL_PlanetConfig WorldValues { get; set; } = null;

        public static FL_ServerConfig ServerConfig { get; set; } = null;

        public static FL_ActionsConfig ActionConfigs { get; set; } = null;

        public static FL_DifficultyConfig DifficultyValues { get; set; } = null;

        public static FL_StorytellerConfig StorytellerValues { get; set; } = null;

        public static FL_ScenarioConfig ScenarioValues { get; set; } = null;

        public static FL_BackupsConfig BackupConfig { get; set; } = null;

        public static FL_ModConfig ModConfig { get; set; } = null;

        public static FL_ChatConfig ChatConfig { get; set; } = null;

        public static FL_Leaderboard LeaderboardFile { get; set; } = null;

        public static FL_Market MarketFile { get; set; } = null;
        
        public static FL_Road RoadFile { get; set; } = null;
        
        public static FL_River RiverFile { get; set; } = null;
    }
}
