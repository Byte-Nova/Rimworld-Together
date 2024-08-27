using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineActivityData
    {
        public OfflineActivityStepMode stepMode;

        public int targetTile;
        
        public MapData mapData;
    }
}