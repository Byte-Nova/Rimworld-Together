using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class EventData
    {
        public EventStepMode _stepMode;
        
        public int _fromTile;

        public int _toTile;

        public EventFile _eventFile;
    }
}
