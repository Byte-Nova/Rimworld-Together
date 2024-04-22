using System;

namespace Shared
{
    [Serializable]
    public class MapFileJSON
    {
        public string mapOwner;

        public string mapTile;

        public byte[] mapData;
    }
}
