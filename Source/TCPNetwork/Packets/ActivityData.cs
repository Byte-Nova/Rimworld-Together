using Shared.Files.Maps;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class ActivityData
    {
        public ActivityStepMode _stepMode { get; set; } = ActivityStepMode.Request;

        public int _targetTile { get; set; } = -1;

        public byte[] _mapRawData { get; set; } = null;
    }
}