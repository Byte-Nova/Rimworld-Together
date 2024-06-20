using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineVisitData
    {
        public OfflineActivityStepMode offlineVisitStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
