namespace Shared
{
    public static class CommonValues
    {
        public readonly static string executableVersion = "24.10.6.1";

        public readonly static string clientAssemblyName = "GameClient";

        public readonly static string serverAssemblyName = "GameServer";

        public static readonly string defaultParserMethodName = "ParsePacket";

        public static readonly string[] ignoredLogPackets =
        {
            "KeepAliveManager"
        };
    }
}