using Shared.Files;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{

    public class PlayerGuildData
    {
        public GuildStepMode _stepMode { get; set; } = GuildStepMode.Create;

        public GuildFile _file { get; set; } = new GuildFile();

        public int _dataInt { get; set; } = -1;

        public override string ToString()
        {
            return $"PlayerGuildData:|{_stepMode}|{_file}|{_dataInt}";
        }
    }
}
