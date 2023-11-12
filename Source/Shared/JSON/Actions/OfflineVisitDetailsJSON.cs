using System;

namespace RimworldTogether.Shared.JSON.Actions
{
    [Serializable]
    public class OfflineVisitDetailsJSON
    {
        public string offlineVisitStepMode;

        public string targetTile;

        public MapDetailsJSON mapDetails;
    }
}
