using System;
using System.Threading;
using static Shared.CommonEnumerators;

namespace Shared
{
    [Serializable]
    public class SiteIdendity
    {
        public int Tile;

        public string Owner;

        public Goodwill Goodwill;

        public SiteInfoFile Type = new SiteInfoFile();

        public FactionFile FactionFile;

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
