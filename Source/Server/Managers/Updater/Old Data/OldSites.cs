using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Updater
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
