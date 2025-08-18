using Shared.Files;
using static Shared.CommonEnumerators;

namespace TCPNetwork.Packets
{
    public class EventData
    {
        public EventStepMode _stepMode { get; set; } = EventStepMode.Send;

        public int _fromTile { get; set; } = -1;

        public int _toTile { get; set; } = -1;

        public EventFile _eventFile { get; set; } = null;

        public EventFile[] _eventFiles { get; set; } = null;

        public override string ToString()
        {
            return $"EventData:|{_stepMode}|{_fromTile}|{_toTile}|{_eventFile}";
        }
    }
}
