using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class EventData
    {
        public EventStepMode eventStepMode;

        public string fromTile;

        public string toTile;

        public string eventID;
    }
}
