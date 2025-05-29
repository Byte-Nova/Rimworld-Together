using static Shared.CommonEnumerators;

namespace Shared
{

    public class EventData
    {
        public EventStepMode _stepMode { get; set; } = EventStepMode.Send;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public EventFile _eventFile { get; set; } = null;
    }
}
