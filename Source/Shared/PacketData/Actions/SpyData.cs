using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SpyData
    {
        public OfflineSpyStepMode spyStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
