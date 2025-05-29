using static Shared.CommonEnumerators;

namespace Shared
{

    public class ActivityData
    {
        public ActivityStepMode _stepMode { get; set; } = ActivityStepMode.Request;

        public int _targetTile { get; set; } = -1;

        public MapFile _mapFile { get; set; } = null;
    }
}