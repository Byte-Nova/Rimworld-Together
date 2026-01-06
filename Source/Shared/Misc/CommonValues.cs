namespace Shared
{
    public static class CommonValues
    {
        public const string MasterServer = "https://rimworldtogether.eragon.dev";

        public static string ExecutableVersion { get; set; } = "26.1.3.1";

        public static string DefaultSaveFormat { get; set; } = ".json";

        public static string TempSaveFormat { get; set; } = ".temp";

        public static string CompressedSaveFormat { get; set; } = ".zip";

        public static string ServerUsersPath { get; set; } = string.Empty;

        public static string ServerSitesPath { get; set; } = string.Empty;
    }
}