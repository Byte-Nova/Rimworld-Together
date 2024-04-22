using System;

namespace Shared
{
    [Serializable]
    public class MapFileData
    {
        public string mapOwner;

        public string mapTile;

        public byte[] mapData;
    }
}
