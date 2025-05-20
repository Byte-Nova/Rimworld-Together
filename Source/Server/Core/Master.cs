using GameServer.Core.Configs;
using Shared;

namespace GameServer.Core
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        public static string? MainPath { get; set; }

        public static string? AssetsPath { get; set; }

        public static string? BackupsPath { get; set; }

        public static string? ConfigsPath { get; set; }

        public static string? LogsPath { get; set; }

        public static string? TempPath { get; set; }

        public static string? MapsPath { get; set; }

        public static string? SystemLogsPath { get; set; }

        public static string? ChatLogsPath { get; set; }

        public static string? UsersPath { get; set; }

        public static string? SavesPath { get; set; }

        public static string? SitesPath { get; set; }

        public static string? FactionsPath { get; set; }

        public static string? SettlementsPath { get; set; }

        public static string? EventsPath { get; set; }

        public static string? BackupServerPath { get; set; }

        public static string? BackupUsersPath { get; set; }

        public static string? CompatibilityPatchesPath { get; set; }

        //References

        public static WhitelistConfigFile? Whitelist { get; set; }

        public static SiteValuesFile? SiteValues { get; set; }

        public static WorldValuesFile? WorldValues { get; set; }

        public static ServerConfigFile? ServerConfig { get; set; }

        public static ActionValuesFile? ActionConfigs { get; set; }

        public static DifficultyValuesFile? DifficultyValues { get; set; }

        public static StorytellerValuesFile? StorytellerValues { get; set; }

        public static ScenarioValuesFile? ScenarioValues { get; set; }

        public static RoadValuesFile? RoadValues { get; set; }

        public static BackupConfigFile? BackupConfig { get; set; }

        public static ModConfigFile? ModConfig { get; set; }

        public static ChatConfigFile? ChatConfig { get; set; }

        //Booleans

        public static bool IsClosing { get; set; }
    }
}
