using System;

namespace Shared
{
    [Serializable]
    public class MapFileJSON
    {
        public string mapOwner;

        public string mapTile;

        public string mapMode;

        public byte[] mapData;
    }
}
