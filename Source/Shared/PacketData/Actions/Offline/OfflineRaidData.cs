using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineRaidData
    {
        public OfflineRaidStepMode raidStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
