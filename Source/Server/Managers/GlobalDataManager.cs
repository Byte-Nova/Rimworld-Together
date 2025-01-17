using GameServer.Core;
using GameServer.TCP;
using Shared;

namespace GameServer.Managers
{
    [RTManager]
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

            globalData._isClientFactionMember = !string.IsNullOrEmpty(client.userFile.GuildName);

            return globalData;
        }

        public static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            globalData._serverValues = new ServerValuesFile(Master.serverConfig.Name);
            globalData._eventValues = EventManagerHelper.loadedEvents;
            globalData._siteValues = Master.siteValues;
            globalData._difficultyValues = Master.difficultyValues;
            globalData._scenarioValues = Master.scenarioValues;
            globalData._storytellerValues = Master.storytellerValues;
            globalData._actionValues = Master.actionConfigs;
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

                if (settlement.UID == client.userFile.Uid) continue;
                else
                {
                    file.Tile = settlement.Tile;
                    file.UID = settlement.UID;
                    file.Label = settlement.Label;
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
            List<SiteFile> tempList = new List<SiteFile>();
            SiteFile[] sites = SiteManagerHelper.GetAllSites();
            foreach (SiteFile site in sites)
            {
                SiteFile file = new SiteFile();

                file.Tile = site.Tile;
                file.UID = site.UID;
                file.Goodwill = GoodwillManager.GetSiteGoodwill(client, site);
                file.Type = site.Type;
                file.GuildName = site.GuildName;

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
