using Shared.Files;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class ActivityData
    {
        public ActivityStepMode _stepMode { get; set; } = ActivityStepMode.Request;

        public int _targetTile { get; set; } = -1;

        public MapFile _mapFile { get; set; } = null;

        public override string ToString()
        {
            return $"ActivityData:|{_stepMode}|{_targetTile}|{_mapFile}";
        }
    }
}