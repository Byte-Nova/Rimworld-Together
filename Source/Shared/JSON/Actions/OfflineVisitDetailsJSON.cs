using System;

namespace Shared
{
    [Serializable]
    public class OfflineVisitDetailsJSON
    {
        public string offlineVisitStepMode;

        public string targetTile;

        public byte[] mapDetails;
    }
}
