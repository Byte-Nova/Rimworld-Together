using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class EventData
    {
        public EventStepMode eventStepMode;
        public int fromTile;
        public int toTile;

        public int eventID;
    }
}
