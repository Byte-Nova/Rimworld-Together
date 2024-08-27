using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class EventData
    {
        public EventStepMode stepMode;
        
        public int fromTile;

        public int toTile;

        public EventFile eventFile;
    }
}
