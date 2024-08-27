using System;

namespace Shared
{
    [Serializable]
    public class SiteFile
    {
        public int tile;

        public string owner;

        public int type;

        public byte[] workerData;

        public FactionFile factionFile;
    }
}
