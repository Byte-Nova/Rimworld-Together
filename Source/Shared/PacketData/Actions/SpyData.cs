using System;

namespace Shared
{
    [Serializable]
    public class SpyData
    {
        public string spyStepMode;

        public int targetTile;

        public byte[] mapData;
    }
}
