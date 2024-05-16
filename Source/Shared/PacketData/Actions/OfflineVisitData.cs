using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineVisitData
    {
        public OfflineVisitStepMode offlineVisitStepMode;

        public string targetTile;

        public byte[] mapData;
    }
}
