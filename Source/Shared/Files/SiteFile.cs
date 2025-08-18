using System;
using System.Threading;
using static Shared.CommonEnumerators;

namespace Shared.Files
{
    public class SiteFile
    {
        public int Tile { get; set; } = -1;

        public string UID { get; set; } = string.Empty;

        public string GuildName { get; set; } = string.Empty;

        public Goodwill Goodwill { get; set; } = new Goodwill();

        public SiteInfoFile Type { get; set; } = new SiteInfoFile();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);
    }
}
