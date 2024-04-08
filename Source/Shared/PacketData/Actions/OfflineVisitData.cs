using System;

namespace Shared
{
    [Serializable]
    public class OfflineVisitData
    {
        public string offlineVisitStepMode;

        public string targetTile;

        public byte[] mapDetails;
    }
}
