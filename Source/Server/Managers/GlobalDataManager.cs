using Shared;

namespace GameServer
{
    public static class GlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData = GlobalDataManagerHelper.GetClientValues(client, globalData);

            globalData = GlobalDataManagerHelper.GetServerValues(globalData);

            globalData = GlobalDataManagerHelper.GetServerSettlements(client, globalData);

            globalData = GlobalDataManagerHelper.GetServerSites(client, globalData);

            globalData = GlobalDataManagerHelper.GetServerMarket(globalData);

            globalData = GlobalDataManagerHelper.GetServerCaravans(globalData);

            globalData = GlobalDataManagerHelper.GetServerRoads(globalData);

            globalData = GlobalDataManagerHelper.GetServerPolution(globalData);

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.ServerValuesPacket), globalData);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class GlobalDataManagerHelper
    {
        public static ServerGlobalData GetClientValues(ServerClient client, ServerGlobalData globalData)
        {
            globalData.isClientAdmin = client.userFile.IsAdmin;

            globalData.isClientFactionMember = client.userFile.FactionFile != null;

            return globalData;
        }

        public static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            globalData.eventValues = EventManagerHelper.loadedEvents;
            globalData.siteValues = Master.siteValues;
            globalData.difficultyValues = Master.difficultyValues;
            globalData.actionValues = Master.actionValues;
            globalData.roadValues = Master.roadValues;
            return globalData;
        }

        public static ServerGlobalData GetServerSettlements(ServerClient client, ServerGlobalData globalData)
        {
            List<SettlementFile> tempList = new List<SettlementFile>();
            SettlementFile[] settlements = SettlementManager.GetAllSettlements();
            foreach (SettlementFile settlement in settlements)
            {
                SettlementFile file = new SettlementFile();

                if (settlement.Owner == client.userFile.Username) continue;
                else
                {
                    file.Tile = settlement.Tile;
                    file.Owner = settlement.Owner;
                    file.Goodwill = GoodwillManager.GetSettlementGoodwill(client, settlement);

                    tempList.Add(file);
                }
            }

            globalData.playerSettlements = tempList.ToArray();
            if (Master.worldValues != null) globalData.npcSettlements = Master.worldValues.NPCSettlements;

            return globalData;
        }

        public static ServerGlobalData GetServerSites(ServerClient client, ServerGlobalData globalData)
        {
            List<SiteFile> tempList = new List<SiteFile>();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteFile site in sites)
            {
                SiteFile file = new SiteFile();

                file.Tile = site.Tile;
                file.Owner = site.Owner;
                file.Goodwill = GoodwillManager.GetSiteGoodwill(client, site);
                file.Type = site.Type;
                file.FactionFile = site.FactionFile;

                tempList.Add(file);
            }

            globalData.playerSites = tempList.ToArray();

            return globalData;
        }

        public static ServerGlobalData GetServerMarket(ServerGlobalData globalData)
        {
            globalData.marketValues = Master.marketValues;
            return globalData;
        }

        public static ServerGlobalData GetServerCaravans(ServerGlobalData globalData)
        {
            globalData.playerCaravans = CaravanManager.GetActiveCaravans();
            return globalData;
        }

        public static ServerGlobalData GetServerRoads(ServerGlobalData data)
        {
            if (Master.worldValues != null) data.roads = Master.worldValues.Roads;
            return data;
        }

        public static ServerGlobalData GetServerPolution(ServerGlobalData data)
        {
            if (Master.worldValues != null) data.pollutedTiles = Master.worldValues.PollutedTiles;
            return data;
        }
    }
}
