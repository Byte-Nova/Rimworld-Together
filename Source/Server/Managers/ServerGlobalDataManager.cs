using Shared;

namespace GameServer
{
    public static class ServerGlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData = GetServerConfigs(globalData);

            globalData = GetClientValues(client, globalData);

            globalData = GetServerValues(globalData);

            globalData = GetServerSettlements(client, globalData);

            globalData = GetServerSites(client, globalData);

            globalData = GetServerMarket(globalData);

            globalData = GetServerCaravans(globalData);

            globalData = GetServerRoads(globalData);

            globalData = GetServerPolution(globalData);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ServerValuesPacket), globalData);
            client.listener.EnqueuePacket(packet);
        }

        private static ServerGlobalData GetServerConfigs(ServerGlobalData globalData)
        {
            ServerConfigFile scf = Master.serverConfig;

            globalData.AllowCustomScenarios = scf.AllowCustomScenarios;

            return globalData;
        }

        private static ServerGlobalData GetClientValues(ServerClient client, ServerGlobalData globalData)
        {
            globalData.isClientAdmin = client.userFile.IsAdmin;

            globalData.isClientFactionMember = client.userFile.faction != null;

            return globalData;
        }

        private static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            globalData.eventValues = EventManagerHelper.loadedEvents;
            globalData.siteValues = Master.siteValues;
            globalData.difficultyValues = Master.difficultyValues;
            globalData.actionValues = Master.actionValues;
            globalData.roadValues = Master.roadValues;
            return globalData;
        }

        private static ServerGlobalData GetServerSettlements(ServerClient client, ServerGlobalData globalData)
        {
            List<OnlineSettlementFile> tempList = new List<OnlineSettlementFile>();
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                OnlineSettlementFile file = new OnlineSettlementFile();

                if (settlement.owner == client.userFile.Username) continue;
                else
                {
                    file.tile = settlement.tile;
                    file.owner = settlement.owner;
                    file.goodwill = GoodwillManager.GetSettlementGoodwill(client, settlement);

                    tempList.Add(file);
                }
            }

            globalData.playerSettlements = tempList.ToArray();
            if (Master.worldValues != null) globalData.npcSettlements = Master.worldValues.NPCSettlements;

            return globalData;
        }

        private static ServerGlobalData GetServerSites(ServerClient client, ServerGlobalData globalData)
        {
            List<OnlineSiteFile> tempList = new List<OnlineSiteFile>();
            SiteFile[] sites = SiteManager.GetAllSites();
            foreach (SiteFile site in sites)
            {
                OnlineSiteFile file = new OnlineSiteFile();

                file.tile = site.tile;
                file.owner = site.owner;
                file.goodwill = GoodwillManager.GetSiteGoodwill(client, site);
                file.type = site.type;
                file.fromFaction = site.factionFile != null;

                tempList.Add(file);
            }

            globalData.playerSites = tempList.ToArray();

            return globalData;
        }

        private static ServerGlobalData GetServerMarket(ServerGlobalData globalData)
        {
            globalData.marketValues = Master.marketValues;
            return globalData;
        }

        private static ServerGlobalData GetServerCaravans(ServerGlobalData globalData)
        {
            globalData.playerCaravans = CaravanManager.GetActiveCaravans();
            return globalData;
        }

        private static ServerGlobalData GetServerRoads(ServerGlobalData data)
        {
            if (Master.worldValues != null) data.roads = Master.worldValues.Roads;
            return data;
        }

        private static ServerGlobalData GetServerPolution(ServerGlobalData data)
        {
            if (Master.worldValues != null) data.pollutedTiles = Master.worldValues.PollutedTiles;
            return data;
        }
    }
}
