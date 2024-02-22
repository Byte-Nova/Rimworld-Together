using System;

namespace Shared.JSON
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
