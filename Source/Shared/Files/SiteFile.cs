using System;
using System.Threading;
using static Shared.CommonEnumerators;

namespace Shared
{
    public class SiteFile
    {
        public int Tile;

        public string UID;

        public string GuildName;

        public Goodwill Goodwill;

        public SiteInfoFile Type = new SiteInfoFile();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
