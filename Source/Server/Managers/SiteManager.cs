using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class SiteManager
    {
        public static void ParseSitePacket(ServerClient client, Packet packet)
        {
            SiteData siteData = (SiteData)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(siteData.siteStep))
            {
                case (int)CommonEnumerators.SiteStepMode.Build:
                    AddNewSite(client, siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Destroy:
                    DestroySite(client, siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Info:
                    GetSiteInfo(client, siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Deposit:
                    DepositWorkerToSite(client, siteData);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Retrieve:
                    RetrieveWorkerFromSite(client, siteData);
                    break;
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(".json")) continue;
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToCheck) return true;
            }

            return false;
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SaveSite(siteFile);

            SiteData siteData = new SiteData();
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
            siteData.tile = siteFile.tile;
            siteData.owner = client.username;
            siteData.type = siteFile.type;
            siteData.isFromFaction = siteFile.isFromFaction;

            foreach (ServerClient cClient in Network.connectedClients.ToArray())
            {
                siteData.goodwill = GoodwillManager.GetSiteGoodwill(cClient, siteFile).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);

                cClient.listener.EnqueuePacket(packet);
            }

            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Accept).ToString();
            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(rPacket);

            ConsoleManager.WriteToConsole($"[Created site] > {client.username}", LogMode.Warning);
        }

        public static void SaveSite(SiteFile siteFile)
        {
            Serializer.SerializeToFile(Path.Combine(Master.sitesPath, siteFile.tile + ".json"), siteFile);
        }

        public static SiteFile[] GetAllSites()
        {
            List<SiteFile> sitesList = new List<SiteFile>();

            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(".json")) continue;
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
                if (!site.EndsWith(".json")) continue;
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (!siteFile.isFromFaction && siteFile.owner == username)
                {
                    sitesList.Add(siteFile);
                }
            }

            return sitesList.ToArray();
        }

        public static SiteFile GetSiteFileFromTile(string tileToGet)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                if (!site.EndsWith(".json")) continue;
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToGet) return siteFile;
            }

            return null;
        }

        private static void AddNewSite(ServerClient client, SiteData siteData)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.tile}, but that tile already has a settlement");
            else if (CheckIfTileIsInUse(siteData.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteData.tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = null;

                if (siteData.isFromFaction)
                {
                    FactionFile factionFile = OnlineFactionManager.GetFactionFromClient(client);

                    if (OnlineFactionManager.GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                        return;
                    }

                    else
                    {
                        siteFile = new SiteFile();
                        siteFile.tile = siteData.tile;
                        siteFile.owner = client.username;
                        siteFile.type = siteData.type;
                        siteFile.isFromFaction = true;
                        siteFile.factionName = client.factionName;
                    }
                }

                else
                {
                    siteFile = new SiteFile();
                    siteFile.tile = siteData.tile;
                    siteFile.owner = client.username;
                    siteFile.type = siteData.type;
                    siteFile.isFromFaction = false;
                }

                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.tile);

            if (siteFile.isFromFaction)
            {
                if (siteFile.factionName != client.factionName) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData.tile} was attempted to be destroyed by {client.username}, but player wasn't a part of faction {siteFile.factionName}");
                else
                {
                    FactionFile factionFile = OnlineFactionManager.GetFactionFromClient(client);

                    if (OnlineFactionManager.GetMemberRank(factionFile, client.username) !=
                        CommonEnumerators.FactionRanks.Member) DestroySiteFromFile(siteFile);

                    else ResponseShortcutManager.SendNoPowerPacket(client, new PlayerFactionData());
                }
            }

            else
            {
                if (siteFile.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteData.tile} was attempted to be destroyed by {client.username}, but the player {siteFile.owner} owns it");
                else if (siteFile.workerData != null) ResponseShortcutManager.SendWorkerInsidePacket(client);
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteData siteData = new SiteData();
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Destroy).ToString();
            siteData.tile = siteFile.tile;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            foreach (ServerClient client in Network.connectedClients.ToArray()) client.listener.EnqueuePacket(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.tile + ".json"));
            ConsoleManager.WriteToConsole($"[Destroyed site] > {siteFile.tile}", LogMode.Warning);
        }

        private static void GetSiteInfo(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.tile);

            siteData.type = siteFile.type;
            siteData.workerData = siteFile.workerData;
            siteData.isFromFaction = siteFile.isFromFaction;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }

        private static void DepositWorkerToSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.tile);

            if (siteFile.owner != client.username && OnlineFactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to deposit a worker in the site at tile {siteData.tile}, but the player {siteFile.owner} owns it");
            }

            else if (siteFile.workerData != null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to deposit a worker in the site at tile {siteData.tile}, but the site already has a worker");
            }

            else
            {
                siteFile.workerData = siteData.workerData;
                SaveSite(siteFile);
            }
        }

        private static void RetrieveWorkerFromSite(ServerClient client, SiteData siteData)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteData.tile);

            if (siteFile.owner != client.username && OnlineFactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to retrieve a worker from the site at tile {siteData.tile}, but the player {siteFile.owner} of faction {siteFile.factionName} owns it");
            }

            else if (siteFile.workerData == null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to retrieve a worker from the site at tile {siteData.tile}, but it has no workers");
            }

            else
            {
                siteData.workerData = siteFile.workerData;
                siteFile.workerData = null;
                SaveSite(siteFile);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
                client.listener.EnqueuePacket(packet);
            }
        }

        public static void StartSiteTicker()
        {
            while (true)
            {
                Thread.Sleep(1800000);

                SiteRewardTick();
            }
        }

        public static void SiteRewardTick()
        {
            SiteFile[] sites = GetAllSites();

            SiteData siteData = new SiteData();
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.Reward).ToString();

            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                siteData.sitesWithRewards.Clear();

                List<SiteFile> playerSites = sites.ToList().FindAll(x => x.owner == client.username);
                foreach (SiteFile site in playerSites)
                {
                    if (site.workerData != null && !site.isFromFaction)
                    {
                        siteData.sitesWithRewards.Add(site.tile);
                    }
                }

                if (client.hasFaction)
                {
                    List<SiteFile> factionSites = sites.ToList().FindAll(x => x.factionName == client.factionName);
                    foreach (SiteFile site in factionSites)
                    {
                        if (site.isFromFaction) siteData.sitesWithRewards.Add(site.tile);
                    }
                }

                if (siteData.sitesWithRewards.Count() > 0)
                {
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
                    client.listener.EnqueuePacket(packet);
                }
            }

            ConsoleManager.WriteToConsole($"[Site tick]");
        }
    }
}
