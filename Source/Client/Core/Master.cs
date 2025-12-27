using GameClient.Core.Configs;

namespace GameClient.Core
{
    public static class Master
    {
        public static string AppdataPath { get; set; }

        public static string AppdataRTPath { get; set; }

        public static string AppdataTempPath { get; set; }

        public static string AppdataVersionPath { get; set; }

        public static string ModMainPath { get; set; }

        public static string ModAssemblyPath { get; set; }

        public static string ModScriptsPath { get; set; }

        public static string SavesFolderPath { get; set; }

        // Values

        public static string ModPackageID { get; private set; } = "nova.rimworldtogether";

        public static string ModID { get; private set; } = "RimWorld Together";
    }
}
