using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SpyData
    {
        public SpyStepMode spyStepMode;

        public string targetTile;

        public byte[] mapData;
    }
}
