namespace Shared
{
    public static class CommonValues
    {
        public static readonly string MasterServer = "https://rimworldtogether.eragon.dev";

        public static readonly string ExecutableVersion = "26.1.18.1";

        public static readonly string DefaultSaveFormat = ".json";

        public static readonly string TempSaveFormat = ".temp";

        public static readonly string CompressedSaveFormat  = ".zip";

        public static string ServerUsersPath { get; set; } = string.Empty;

        public static string ServerSitesPath { get; set; } = string.Empty;
    }
}