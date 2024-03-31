using Shared;

namespace GameServer
{
    public static class SiteManager
    {
        public static void ParseSitePacket(ServerClient client, Packet packet)
        {
            SiteDetailsJSON siteDetailsJSON = (SiteDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch(int.Parse(siteDetailsJSON.siteStep))
            {
                case (int)CommonEnumerators.SiteStepMode.Build:
                    AddNewSite(client, siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Destroy:
                    DestroySite(client, siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Info:
                    GetSiteInfo(client, siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Deposit:
                    DepositWorkerToSite(client, siteDetailsJSON);
                    break;

                case (int)CommonEnumerators.SiteStepMode.Retrieve:
                    RetrieveWorkerFromSite(client, siteDetailsJSON);
                    break;
            }
        }

        public static bool CheckIfTileIsInUse(string tileToCheck)
        {
            string[] sites = Directory.GetFiles(Master.sitesPath);
            foreach (string site in sites)
            {
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToCheck) return true;
            }

            return false;
        }

        public static void ConfirmNewSite(ServerClient client, SiteFile siteFile)
        {
            SaveSite(siteFile);

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Build).ToString();
            siteDetailsJSON.tile = siteFile.tile;
            siteDetailsJSON.owner = client.username;
            siteDetailsJSON.type = siteFile.type;
            siteDetailsJSON.isFromFaction = siteFile.isFromFaction;

            foreach (ServerClient cClient in Network.connectedClients.ToArray())
            {
                siteDetailsJSON.likelihood = LikelihoodManager.GetSiteLikelihood(cClient, siteFile).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);

                cClient.listener.EnqueuePacket(packet);
            }

            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Accept).ToString();
            Packet rPacket = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            client.listener.EnqueuePacket(rPacket);

            Logger.WriteToConsole($"[Created site] > {client.username}", Logger.LogMode.Warning);
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
                SiteFile siteFile = Serializer.SerializeFromFile<SiteFile>(site);
                if (siteFile.tile == tileToGet) return siteFile;
            }

            return null;
        }

        private static void AddNewSite(ServerClient client, SiteDetailsJSON siteDetailsJSON)
        {
            if (SettlementManager.CheckIfTileIsInUse(siteDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteDetailsJSON.tile}, but that tile already has a settlement");
            else if (CheckIfTileIsInUse(siteDetailsJSON.tile)) ResponseShortcutManager.SendIllegalPacket(client, $"A site tried to be added to tile {siteDetailsJSON.tile}, but that tile already has a site");
            else
            {
                SiteFile siteFile = null;

                if (siteDetailsJSON.isFromFaction)
                {
                    FactionFile factionFile = OnlineFactionManager.GetFactionFromClient(client);

                    if (OnlineFactionManager.GetMemberRank(factionFile, client.username) == CommonEnumerators.FactionRanks.Member)
                    {
                        ResponseShortcutManager.SendNoPowerPacket(client, new FactionManifestJSON());
                        return;
                    }

                    else
                    {
                        siteFile = new SiteFile();
                        siteFile.tile = siteDetailsJSON.tile;
                        siteFile.owner = client.username;
                        siteFile.type = siteDetailsJSON.type;
                        siteFile.isFromFaction = true;
                        siteFile.factionName = client.factionName;
                    }
                }

                else
                {
                    siteFile = new SiteFile();
                    siteFile.tile = siteDetailsJSON.tile;
                    siteFile.owner = client.username;
                    siteFile.type = siteDetailsJSON.type;
                    siteFile.isFromFaction = false;
                }

                ConfirmNewSite(client, siteFile);
            }
        }

        private static void DestroySite(ServerClient client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.isFromFaction)
            {
                if (siteFile.factionName != client.factionName) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteDetailsJSON.tile} was attempted to be destroyed by {client.username}, but player wasn't a part of faction {siteFile.factionName}");
                else
                {
                    FactionFile factionFile = OnlineFactionManager.GetFactionFromClient(client);

                    if (OnlineFactionManager.GetMemberRank(factionFile, client.username) !=
                        CommonEnumerators.FactionRanks.Member) DestroySiteFromFile(siteFile);

                    else ResponseShortcutManager.SendNoPowerPacket(client, new FactionManifestJSON());
                }
            }

            else
            {
                if (siteFile.owner != client.username) ResponseShortcutManager.SendIllegalPacket(client, $"The site at tile {siteDetailsJSON.tile} was attempted to be destroyed by {client.username}, but the player {siteFile.owner} owns it");
                else if (siteFile.workerData != null) ResponseShortcutManager.SendWorkerInsidePacket(client);
                else DestroySiteFromFile(siteFile);
            }
        }

        public static void DestroySiteFromFile(SiteFile siteFile)
        {
            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Destroy).ToString();
            siteDetailsJSON.tile = siteFile.tile;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            foreach (ServerClient client in Network.connectedClients.ToArray()) client.listener.EnqueuePacket(packet);

            File.Delete(Path.Combine(Master.sitesPath, siteFile.tile + ".json"));
            Logger.WriteToConsole($"[Destroyed site] > {siteFile.tile}", Logger.LogMode.Warning);
        }

        private static void GetSiteInfo(ServerClient client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            siteDetailsJSON.type = siteFile.type;
            siteDetailsJSON.workerData = siteFile.workerData;
            siteDetailsJSON.isFromFaction = siteFile.isFromFaction;

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
            client.listener.EnqueuePacket(packet);
        }

        private static void DepositWorkerToSite(ServerClient client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.owner != client.username && OnlineFactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to deposit a worker in the site at tile {siteDetailsJSON.tile}, but the player {siteFile.owner} owns it");
            }

            else if (siteFile.workerData != null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to deposit a worker in the site at tile {siteDetailsJSON.tile}, but the site already has a worker");
            }

            else
            {
                siteFile.workerData = siteDetailsJSON.workerData;
                SaveSite(siteFile);
            }
        }

        private static void RetrieveWorkerFromSite(ServerClient client, SiteDetailsJSON siteDetailsJSON)
        {
            SiteFile siteFile = GetSiteFileFromTile(siteDetailsJSON.tile);

            if (siteFile.owner != client.username && OnlineFactionManager.GetFactionFromClient(client).factionMembers.Contains(siteFile.owner))
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to retrieve a worker from the site at tile {siteDetailsJSON.tile}, but the player {siteFile.owner} of faction {siteFile.factionName} owns it");
            }

            else if (siteFile.workerData == null)
            {
                ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} attempted to retrieve a worker from the site at tile {siteDetailsJSON.tile}, but it has no workers");
            }

            else
            {
                siteDetailsJSON.workerData = siteFile.workerData;
                siteFile.workerData = null;
                SaveSite(siteFile);

                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
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

            SiteDetailsJSON siteDetailsJSON = new SiteDetailsJSON();
            siteDetailsJSON.siteStep = ((int)CommonEnumerators.SiteStepMode.Reward).ToString();

            foreach (ServerClient client in Network.connectedClients.ToArray())
            {
                siteDetailsJSON.sitesWithRewards.Clear();

                List<SiteFile> playerSites = sites.ToList().FindAll(x => x.owner == client.username);
                foreach (SiteFile site in playerSites)
                {
                    if (site.workerData != null && !site.isFromFaction)
                    {
                        siteDetailsJSON.sitesWithRewards.Add(site.tile);
                    }
                }

                if (client.hasFaction)
                {
                    List<SiteFile> factionSites = sites.ToList().FindAll(x => x.factionName == client.factionName);
                    foreach (SiteFile site in factionSites)
                    {
                        if (site.isFromFaction) siteDetailsJSON.sitesWithRewards.Add(site.tile);
                    }
                }

                if (siteDetailsJSON.sitesWithRewards.Count() > 0)
                {
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }
            }

            Logger.WriteToConsole($"[Site tick]");
        }
    }
}
