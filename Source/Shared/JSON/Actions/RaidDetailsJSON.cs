using System;

namespace RimworldTogether.Shared.JSON.Actions
{
    [Serializable]
    public class RaidDetailsJSON
    {
        public string raidStepMode;

        public string targetTile;

        public MapDetailsJSON mapDetails;
    }
}
