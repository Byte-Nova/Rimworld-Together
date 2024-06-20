using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class OfflineSpyData
    {
        public OfflineActivityStepMode spyStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
