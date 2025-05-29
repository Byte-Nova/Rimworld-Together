using static Shared.CommonEnumerators;

namespace Shared
{

    public class RoadData
    {
        public RoadStepMode _stepMode { get; set; } = RoadStepMode.Add;

        public RoadDetails _details { get; set; } = null;
    }
}