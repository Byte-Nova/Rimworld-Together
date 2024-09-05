using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SiteManager
    {
        //Variables

        private static readonly double taskDelayMS = 1800000;

        public static void ParseSitePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableSites)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);

            switch(siteData._stepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, siteData);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, siteData);
                    break;

                case SiteStepMode.Info:
                    SiteManagerHelper.GetSiteInfo(client, siteData);
                    break;

                case SiteStepMode.Deposit:
                    DepositWorkerIntoSite(client, siteData);
                    break;

                case SiteStepMode.Retrieve:
                    RetrieveWorkerFromSite(client, siteData);
                    break;
            }
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SiteManagerHelper.SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Build;
            siteData._siteFile = siteFile;

            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData._goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData._stepMode = SiteStepMode.Accept;
            Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(rPacket);

            Logger.Warning($"[Created site] > {client.userFile.Username}");
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a settlement");
            else if (SiteManagerHelper.CheckIfTileIsInUse(siteData._siteFile.Tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData._siteFile.Tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = null;

                if (siteData._siteFile.FactionFile != null)
                {
                    FactionFile factionFile = client.userFile.FactionFile;

                    if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                        return;
                    }

                    else
                    {
                        siteFile = new SiteFile();
                        siteFile.Tile = siteData._siteFile.Tile;
                        siteFile.Owner = client.userFile.Username;
                        siteFile.Type = siteData._siteFile.Type;
                        siteFile.FactionFile = factionFile;
                    }
                }

                else
                {
                    siteFile = new SiteFile();
                    siteFile.Tile = siteData._siteFile.Tile;
                    siteFile.Owner = client.userFile.Username;
                    siteFile.Type = siteData._siteFile.Type;
                }

                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._siteFile.Tile);

            if (siteFile.FactionFile != null)
            {
                if (siteFile.FactionFile.Name != client.userFile.FactionFile.Name)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData._siteFile.Tile} was attempted to be destroyed by {client.userFile.Username}, but player wasn't a part of faction {siteFile.FactionFile.Name}");
                }

                else
                {
                    FactionFile factionFile = client.userFile.FactionFile;
                    if (FactionManagerHelper.GetMemberRank(factionFile, client.userFile.Username) != FactionRanks.Member) DestroySiteFromFile(siteFile);
                    else ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                }
            }

            else
            {
                if (siteFile.Owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData._siteFile.Tile} was attempted to be destroyed by {client.userFile.Username}, but the player {siteFile.Owner} owns it");
                else if (siteFile.WorkerData != null) ResponseShortcutManager.SendWorkerInsidePacket(client);
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Destroy;
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            NetworkHelper.SendPacketToAllClients(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.Tile + SiteManagerHelper.fileExtension));
            Logger.Warning($"[Remove site] > {siteFile.Tile}");
        }

        private static void DepositWorkerIntoSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._siteFile.Tile);

            if (siteFile.FactionFile != null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to deposit worker into faction site");
            }

            else
            {
                if (siteFile.Owner != client.userFile.Username)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to deposit a worker in the site at tile {siteData._siteFile.Tile}, but the player {siteFile.Owner} owns it");
                }

                else if (siteFile.WorkerData != null)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to deposit a worker in the site at tile {siteData._siteFile.Tile}, but the site already has a worker");
                }

                else
                {
                    siteFile.WorkerData = siteData._siteFile.WorkerData;
                    SiteManagerHelper.SaveSite(siteFile);
                }
            }
        }

        private static void RetrieveWorkerFromSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = SiteManagerHelper.GetSiteFileFromTile(siteData._siteFile.Tile);

            if (siteFile.FactionFile != null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to extract worker from faction site");
            }

            else
            {
                if (siteFile.Owner != client.userFile.Username)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to retrieve a worker from the site at tile {siteData._siteFile.Tile}, but the player {siteFile.Owner} of faction {siteFile.FactionFile.Name} owns it");
                }

                else if (siteFile.WorkerData == null)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to retrieve a worker from the site at tile {siteData._siteFile.Tile}, but it has no workers");
                }

                else
                {
                    siteData._siteFile.WorkerData = siteFile.WorkerData;
                    siteFile.WorkerData = null;
                    SiteManagerHelper.SaveSite(siteFile);

                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        public static async Task StartSiteTicker()
        {
            while (true)
            {
                try { SiteRewardTick(); }
                catch (Exception e) { Logger.Error($"Site tick failed, this should never happen. Exception > {e}"); }

                await Task.Delay(TimeSpan.FromMilliseconds(taskDelayMS));
            }
        }

        public static void SiteRewardTick()
        {
            SiteFile[] sites = SiteManagerHelper.GetAllSites();

            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.Reward;

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData._sitesWithRewards.Clear();

                //Get player specific sites

                List<SiteFile> playerSites = sites.ToList().FindAll(fetch => fetch.FactionFile == null && fetch.Owner == client.userFile.Username);
                foreach (SiteFile site in playerSites)
                {
                    if (site.WorkerData != null)
                    {
                        siteData._sitesWithRewards.Add(site.Tile);
                    }
                }

                //Get faction specific sites

                if (client.userFile.FactionFile != null)
                {
                    List<SiteFile> factionSites = sites.ToList().FindAll(fetch => fetch.FactionFile != null && fetch.FactionFile.Name == client.userFile.FactionFile.Name);
                    foreach (SiteFile site in factionSites)
                    {
                        if (site.FactionFile != null) siteData._sitesWithRewards.Add(site.Tile);
                    }
                }

                if (siteData._sitesWithRewards.Count() > 0)
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                    client.listener.EnqueuePacket(packet);
                }
            }

            Logger.Warning($"[Site tick]");
        }
    }

    public static class SiteManagerHelper
    {
        public readonly static string fileExtension = ".mpsite";

        public static void SaveSite(SiteFile siteFile)
        {
            siteFile.SavingSemaphore.WaitOne();

            try { Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.Tile + fileExtension), siteFile); }
            catch (Exception e) { Logger.Error(e.ToString()); }
            
            siteFile.SavingSemaphore.Release();
        }

        public static void UpdateFaction(SiteFile siteFile, FactionFile toUpdateWith)
        {
            siteFile.FactionFile = toUpdateWith;
            SaveSite(siteFile);
        }

        public static SiteFile[] GetAllSitesFromUsername(string username)
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.FactionFile == null && siteFile.Owner == username) sitesList.Add(siteFile);
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(int tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToGet) return siteFile;
            }

            return null;
        }

        public static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData._siteFile.Tile);
            siteData._siteFile = siteFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;
                sitesList.Add(Serializer.SerializeFromFile<SiteFile>(site));
            }

            return sitesList.ToArray();
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.Tile == tileToCheck) return true;
            }

            return false;
        }
    }
}
