namespace Shared
{
    public static class CommonValues
    {
        public readonly static string executableVersion = "dev";

        public readonly static string clientAssemblyName = "GameClient";

        public readonly static string serverAssemblyName = "GameServer";

        public static readonly string defaultParserMethodName = "ParsePacket";
        
        public static readonly string defaultKeepAlivePacketName = "KeepAliveManager";

        public static readonly string[] ignoredLogPackets =
        {
            "KeepAlivePacket",
            "OnlineActivityPacket"
        };
    }
}