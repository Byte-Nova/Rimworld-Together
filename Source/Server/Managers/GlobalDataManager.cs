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

            globalData = GlobalDataManagerHelper.GetServerCaravans(globalData);

            globalData = GlobalDataManagerHelper.GetServerRoads(globalData);

            globalData = GlobalDataManagerHelper.GetServerPolution(globalData);

            Packet packet = Packet.CreatePacketFromObject(nameof(GlobalDataManager), globalData);
            client.listener.EnqueuePacket(packet);
        }
    }

    public static class GlobalDataManagerHelper
    {
        public static ServerGlobalData GetClientValues(ServerClient client, ServerGlobalData globalData)
        {
            globalData._isClientAdmin = client.userFile.IsAdmin;

            globalData._isClientFactionMember = client.userFile.FactionFile != null;

            return globalData;
        }

        public static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            globalData._eventValues = EventManagerHelper.loadedEvents;
            globalData._siteValues = Master.siteValues;
            globalData._difficultyValues = Master.difficultyValues;
            globalData._actionValues = Master.actionValues;
            globalData._roadValues = Master.roadValues;
            return globalData;
        }

        public static ServerGlobalData GetServerSettlements(ServerClient client, ServerGlobalData globalData)
        {
            List<SettlementFile> tempList = new List<SettlementFile>();
            SettlementFile[] settlements = PlayerSettlementManager.GetAllSettlements();
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

            globalData._playerSettlements = tempList.ToArray();
            if (Master.worldValues != null) globalData._npcSettlements = Master.worldValues.NPCSettlements;

            return globalData;
        }

        public static ServerGlobalData GetServerSites(ServerClient client, ServerGlobalData globalData)
        {
            List<SiteIdendityFile> tempList = new List<SiteIdendityFile>();
            SiteIdendityFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteIdendityFile site in sites)
            {
                SiteIdendityFile file = new SiteIdendityFile();

                file.Tile = site.Tile;
                file.Owner = site.Owner;
                file.Goodwill = GoodwillManager.GetSiteGoodwill(client, site);
                file.Type = site.Type;
                file.FactionFile = site.FactionFile;

                tempList.Add(file);
            }

            globalData._playerSites = tempList.ToArray();

            return globalData;
        }

        public static ServerGlobalData GetServerCaravans(ServerGlobalData globalData)
        {
            globalData._playerCaravans = CaravanManagerHelper.GetActiveCaravans();
            return globalData;
        }

        public static ServerGlobalData GetServerRoads(ServerGlobalData data)
        {
            if (Master.worldValues != null) data._roads = Master.worldValues.Roads;
            return data;
        }

        public static ServerGlobalData GetServerPolution(ServerGlobalData data)
        {
            if (Master.worldValues != null) data._pollutedTiles = Master.worldValues.PollutedTiles;
            return data;
        }
    }
}
