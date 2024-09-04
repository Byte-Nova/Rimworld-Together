using Shared;

namespace GameServer
{
    //Class with all the critical variables for the client to work

    public static class Master
    {
        //Paths

        public static string mainPath;

        public static string corePath;

        public static string mapsPath;

        public static string logsPath;

        public static string systemLogsPath;

        public static string chatLogsPath;

        public static string usersPath;

        public static string savesPath;

        public static string sitesPath;

        public static string factionsPath;

        public static string settlementsPath;

        public static string caravansPath;

        public static string eventsPath;

        public static string backupsPath;

        public static string backupServerPath;

        public static string backupUsersPath;

        public static string modsPath;

        public static string requiredModsPath;

        public static string optionalModsPath;

        public static string forbiddenModsPath;

        //Lists

        public static List<string> loadedRequiredMods = new List<string>();

        public static List<string> loadedOptionalMods = new List<string>();

        public static List<string> loadedForbiddenMods = new List<string>();

        //References

        public static MarketValuesFile marketValues;

        public static WhitelistFile whitelist;

        public static SiteValuesFile siteValues;

        public static WorldValuesFile worldValues;

        public static ServerConfigFile serverConfig;

        public static ActionValuesFile actionValues;

        public static DifficultyValuesFile difficultyValues;

        public static RoadValuesFile roadValues;

        public static DiscordConfigFile discordConfig;

        public static BackupConfigFile backupConfig;

        //Booleans

        public static bool isClosing;
    }
}
