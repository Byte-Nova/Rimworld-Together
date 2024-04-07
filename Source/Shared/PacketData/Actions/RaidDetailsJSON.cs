using System;

namespace Shared
{
    [Serializable]
    public class RaidDetailsJSON
    {
        public string raidStepMode;

        public string targetTile;

        public byte[] mapDetails;
    }
}
