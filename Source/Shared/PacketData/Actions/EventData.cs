using System;

namespace Shared
{
    [Serializable]
    public class EventData
    {
        public string eventStepMode;

        public int fromTile;

        public int toTile;

        public string eventID;
    }
}
