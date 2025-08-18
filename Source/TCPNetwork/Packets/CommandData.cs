using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class CommandData
    {
        public CommandMode _commandMode { get; set; } = CommandMode.Op;

        public string _details { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"CommandData:|{_commandMode}|{_details}";
        }
    }
}
