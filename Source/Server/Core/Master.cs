﻿using Shared;

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

        public static ModConfigFile modConfig;
        
        public static ChatConfigFile chatConfig;

        //Booleans

        public static bool isClosing;
    }
}
