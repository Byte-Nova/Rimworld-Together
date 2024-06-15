using System;

namespace Shared
{
    [Serializable]
    public class MapFileData
    {
        public string mapOwner;

        public int mapTile;

        public byte[] mapData;
    }
}
