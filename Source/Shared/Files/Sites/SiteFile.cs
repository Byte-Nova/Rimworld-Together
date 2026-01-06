using Shared.Files.Guilds;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using static Shared.CommonEnumerators;

namespace Shared.Files.Sites
{
    public class SiteFile
    {
        public int Tile { get; set; } = -1;

        public string Username { get; set; } = string.Empty;

        public string GuildName { get; set; } = string.Empty;

        public Goodwill Goodwill { get; set; } = Goodwill.Neutral;

        public string WorkerString { get; set; } = null;

        public SiteType Type { get; set; } = new SiteType();

        [NonSerialized] public Semaphore SavingSemaphore = new Semaphore(1, 1);

        public void SaveSite()
        {
            SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(CommonValues.ServerSitesPath, Tile + CommonValues.DefaultSaveFormat), this); }
            catch (Exception e) { throw new Exception(e.ToString()); }

            SavingSemaphore.Release();
        }

        public void UpdateFaction(GuildFile toUpdateWith)
        {
            if (toUpdateWith == null) GuildName = null;
            else GuildName = toUpdateWith.Name;
            SaveSite();
        }
    }
}
