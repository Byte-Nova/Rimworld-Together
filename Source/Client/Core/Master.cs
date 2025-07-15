using GameClient.Core.Configs;

namespace GameClient.Core
{
    public static class Master
    {
        // Instances

        public static ModConfigGetter ModConfigs { get; private set; } = new ModConfigGetter();

        // Paths

        public static string AppdataPath { get; set; }

        public static string AppdataRTPath { get; set; }

        public static string AppdataTempPath { get; set; }

        public static string AppdataTempVersionPath { get; set; }

        public static string AppdataTempModsPath { get; set; }

        public static string ModMainPath { get; set; }

        public static string ModAssemblyPath { get; set; }

        public static string ModAddonsPath { get; set; }

        public static string ConnectionDataPath { get; set; }

        public static string LoginDataPath { get; set; }

        public static string ClientPreferencesPath { get; set; }

        public static string RecentServersPath { get; set; }

        public static string SavesFolderPath { get; set; }
        
        // Steam
        
        public static bool HasSteam { get; set; }

        public static ulong SteamID { get; set; }

        public static string SteamName { get; set; }

        // Values

        public static string ModPackageID { get; private set; } = "nova.rimworldtogether";

        public static string ModID { get; private set; } = "RimWorld Together";
    }
}
