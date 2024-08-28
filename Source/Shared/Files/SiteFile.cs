using System;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteFile
    {
        public int Tile;

        public string Owner;

        public Goodwill Goodwill;

        public int Type;

        public byte[] WorkerData;

        public FactionFile FactionFile;

        public void UpdateFaction(FactionFile toUpdateWith)
        {
            FactionFile = toUpdateWith;
        }
    }
}
