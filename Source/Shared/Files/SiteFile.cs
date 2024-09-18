using System;
using System.Threading;
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

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
