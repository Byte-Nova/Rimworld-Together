using System;

namespace Shared
{
    [Serializable]
    public class RaidData
    {
        public string raidStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
