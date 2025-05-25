namespace Shared
{
    // These classes should not be touched without notifying me first. The fields need to exactly match in both name and type to the master server.
    // NOTIFY ERAGON UPON ANY CHANGES

    public class ServerInfo
    {
        public string _ip { get; set; } = string.Empty;

        public string _name { get; set; } = string.Empty;

        public string _description { get; set; } = string.Empty;

        public ModConfigFile _config { get; set; } = null;

        public int _maximumPlayerCount { get; set; } = -1;

        public int _currentPlayerCount { get; set; } = -1;

        public int _port { get; set; } = -1;

        public override string ToString()
        {
            return $"ServerInfo:|{_ip}|{_name}|{_description}|{_config}|{_maximumPlayerCount}|{_currentPlayerCount}|{_currentPlayerCount}|{_port}";
        }
    }

    public class AllServersPacket
    {
        public ServerInfo[] _serverInfos { get; set; } = null;

        public override string ToString()
        {
            return $"AllServersPacket:|{_serverInfos?.Length ?? 0}";
        }
    }
}