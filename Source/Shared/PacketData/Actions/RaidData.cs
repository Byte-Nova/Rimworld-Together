using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class RaidData
    {
        public RaidStepMode raidStepMode;

        public string targetTile;

        public byte[] mapData;
    }
}
