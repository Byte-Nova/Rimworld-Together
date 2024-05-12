using System;

namespace Shared
{
    [Serializable]
    public class OfflineVisitData
    {
        public string offlineVisitStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
