using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SiteManager
    {
        //Variables

        public readonly static string fileExtension = ".mpsite";

        public static void ParseSitePacket(ServerClient client, Packet packet)
        {
            SiteData siteData = Serializer.ConvertBytesToObject<SiteData>(packet.contents);

            switch(siteData.siteStepMode)
            {
                case SiteStepMode.Build:
                    AddNewSite(client, siteData);
                    break;

                case SiteStepMode.Destroy:
                    DestroySite(client, siteData);
                    break;

                case SiteStepMode.Info:
                    GetSiteInfo(client, siteData);
                    break;

                case SiteStepMode.Deposit:
                    DepositWorkerToSite(client, siteData);
                    break;

                case SiteStepMode.Retrieve:
                    RetrieveWorkerFromSite(client, siteData);
                    break;
            }
        }

        public static bool CheckIfTileIsInUse(int tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToCheck) return true;
            }

            return false;
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData.siteStepMode = SiteStepMode.Build;
            siteData.siteFile = siteFile;

            foreach (ServerClient cClient in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData.goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile);
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData.siteStepMode = SiteStepMode.Accept;
            Packet rPacket = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(rPacket);

            Logger.Warning($"[Created site] > {client.userFile.Username}");
        }

        public static void SaveSite(SiteFile siteFile)
        {
            Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.tile + fileExtension), siteFile);
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

        public static SiteFile[] GetAllSitesFromUsername(string username)
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(fileExtension)) continue;

                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.factionFile == null && siteFile.owner == username) sitesList.Add(siteFile);
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
                if (siteFile.tile == tileToGet) return siteFile;
            }

            return null;
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData.siteFile.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.siteFile.tile}, but that tile already has a settlement");
            else if (CheckIfTileIsInUse(siteData.siteFile.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.siteFile.tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = null;

                if (siteData.siteFile.factionFile != null)
                {
                    FactionFile factionFile = FactionManager.GetFactionFromClient(client);

                    if (FactionManager.GetMemberRank(factionFile, client.userFile.Username) == FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                        return;
                    }

                    else
                    {
                        siteFile = new SiteFile();
                        siteFile.tile = siteData.siteFile.tile;
                        siteFile.owner = client.userFile.Username;
                        siteFile.type = siteData.siteFile.type;
                        siteFile.factionFile = factionFile;
                    }
                }

                else
                {
                    siteFile = new SiteFile();
                    siteFile.tile = siteData.siteFile.tile;
                    siteFile.owner = client.userFile.Username;
                    siteFile.type = siteData.siteFile.type;
                }

                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.siteFile.tile);

            if (siteFile.factionFile != null)
            {
                if (siteFile.factionFile.name != client.userFile.faction.name)
                {
                    ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData.siteFile.tile} was attempted to be destroyed by {client.userFile.Username}, but player wasn't a part of faction {siteFile.factionFile.name}");
                }

                else
                {
                    FactionFile factionFile = FactionManager.GetFactionFromClient(client);

                    if (FactionManager.GetMemberRank(factionFile, client.userFile.Username) !=
                        FactionRanks.Member) DestroySiteFromFile(siteFile);

                    else ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                }
            }

            else
            {
                if (siteFile.owner != client.userFile.Username) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData.siteFile.tile} was attempted to be destroyed by {client.userFile.Username}, but the player {siteFile.owner} owns it");
                else if (siteFile.workerData != null) ResponseShortcutManager.SendWorkerInsidePacket(client);
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData.siteStepMode = SiteStepMode.Destroy;
            siteData.siteFile.tile = siteFile.tile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            NetworkHelper.SendPacketToAllClients(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.tile + fileExtension));
            Logger.Warning($"[Remove site] > {siteFile.tile}");
        }

        private static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.siteFile.tile);

            siteData.siteFile.type = siteFile.type;
            siteData.siteFile.workerData = siteFile.workerData;
            siteData.siteFile.factionFile = siteFile.factionFile;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }

        private static void DepositWorkerToSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.siteFile.tile);

            if (siteFile.owner != client.userFile.Username && FactionManager.GetFactionFromClient(client).currentMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to deposit a worker in the site at tile {siteData.siteFile.tile}, but the player {siteFile.owner} owns it");
            }

            else if (siteFile.workerData != null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} tried to deposit a worker in the site at tile {siteData.siteFile.tile}, but the site already has a worker");
            }

            else
            {
                siteFile.workerData = siteData.siteFile.workerData;
                SaveSite(siteFile);
            }
        }

        private static void RetrieveWorkerFromSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.siteFile.tile);

            if (siteFile.owner != client.userFile.Username && FactionManager.GetFactionFromClient(client).currentMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to retrieve a worker from the site at tile {siteData.siteFile.tile}, but the player {siteFile.owner} of faction {siteFile.factionFile.name} owns it");
            }

            else if (siteFile.workerData == null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to retrieve a worker from the site at tile {siteData.siteFile.tile}, but it has no workers");
            }

            else
            {
                siteData.siteFile.workerData = siteFile.workerData;
                siteFile.workerData = null;
                SaveSite(siteFile);

                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                client.listener.EnqueuePacket(packet);
            }
        }

        public static void StartSiteTicker()
        {
            while (true)
            {
                Thread.Sleep(1800000);

                try { SiteRewardTick(); }
                catch (Exception e) { Logger.Error($"Site tick failed, this should never happen. Exception > {e}"); }
            }
        }

        public static void SiteRewardTick()
        {
            SiteFile[] sites = GetAllSites();

            SiteData siteData = new SiteData();
            siteData.siteStepMode = SiteStepMode.Reward;

            foreach (ServerClient client in NetworkHelper.GetConnectedClientsSafe())
            {
                siteData.sitesWithRewards.Clear();

                //Get player specific sites

                List<SiteFile> playerSites = sites.ToList().FindAll(fetch => fetch.factionFile == null && fetch.owner == client.userFile.Username);
                foreach (SiteFile site in playerSites)
                {
                    if (site.workerData != null)
                    {
                        siteData.sitesWithRewards.Add(site.tile);
                    }
                }

                //Get faction specific sites

                if (client.userFile.faction != null)
                {
                    List<SiteFile> factionSites = sites.ToList().FindAll(fetch => fetch.factionFile != null && fetch.factionFile.name == client.userFile.faction.name);
                    foreach (SiteFile site in factionSites)
                    {
                        if (site.factionFile != null) siteData.sitesWithRewards.Add(site.tile);
                    }
                }

                if (siteData.sitesWithRewards.Count() > 0)
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
                    client.listener.EnqueuePacket(packet);
                }
            }

            Logger.Message($"[Site tick]");
        }
    }
}
