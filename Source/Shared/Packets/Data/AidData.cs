using static Shared.CommonEnumerators;

namespace Shared
{

    public class AidData
    {
        public AidStepMode _stepMode { get; set; } = AidStepMode.Send;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public HumanFile _humanData { get; set; } = null;
    }
}