using System;

namespace Shared
{
    [Serializable]
    public class RaidData
    {
        public string raidStepMode;

        public string targetTile;

        public byte[] mapData;
    }
}
