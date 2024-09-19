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

        public SiteConfigFile Type = new SiteConfigFile();

        public int chosenReward = 0;

        public FactionFile FactionFile;

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
