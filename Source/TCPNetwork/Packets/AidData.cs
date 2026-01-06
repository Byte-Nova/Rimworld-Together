using Shared.Files;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class AidData
    {
        public AidStepMode _stepMode { get; set; } = AidStepMode.Send;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public string _humanData { get; set; } = string.Empty;
    }
}