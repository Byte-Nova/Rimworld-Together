using GameServer.Core.Configs;
using Shared;
using System.Reflection;

namespace GameServer.Core
{
    public static class Master
    {
        public static string mainPath = string.Empty;
        public static string assetsPath = string.Empty;
        public static string backupsPath = string.Empty;
        public static string configsPath = string.Empty;
        public static string logsPath = string.Empty;
        public static string tempPath = string.Empty;
        public static string mapsPath = string.Empty;
        public static string systemLogsPath = string.Empty;
        public static string chatLogsPath = string.Empty;
        public static string usersPath = string.Empty;
        public static string savesPath = string.Empty;
        public static string sitesPath = string.Empty;
        public static string factionsPath = string.Empty;
        public static string settlementsPath = string.Empty;
        public static string eventsPath = string.Empty;
        public static string backupServerPath = string.Empty;
        public static string backupUsersPath = string.Empty;
        public static string compatibilityPatchesPath = string.Empty;

        public static WhitelistConfigFile whitelist = new WhitelistConfigFile();
        public static SiteValuesFile siteValues = new SiteValuesFile();
        public static WorldValuesFile worldValues = new WorldValuesFile();
        public static ServerConfigFile serverConfig = new ServerConfigFile();
        public static ActionValuesFile actionConfigs = new ActionValuesFile();
        public static DifficultyValuesFile difficultyValues = new DifficultyValuesFile();
        public static StorytellerValuesFile storytellerValues = new StorytellerValuesFile();
        public static ScenarioValuesFile scenarioValues = new ScenarioValuesFile();
        public static RoadValuesFile roadValues = new RoadValuesFile();
        public static BackupConfigFile backupConfig = new BackupConfigFile();
        public static ModConfigFile modConfig = new ModConfigFile();
        public static ChatConfigFile chatConfig = new ChatConfigFile();
        public static DiscordConfigFile discordConfig = new DiscordConfigFile();

        public static Dictionary<string, MethodInfo> managerDictionary = new Dictionary<string, MethodInfo>();
        public static bool isClosing;
    }
}