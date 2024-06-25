using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ServerGlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData = GetServerConfigs(globalData);

            globalData = GetClientValues(client, globalData);

            globalData = GetEventCosts(globalData);

            globalData = GetSiteData(globalData);

            globalData = GetServerDifficulty(globalData);

            globalData = GetServerSettlements(client, globalData);

            globalData = GetServerSites(client, globalData);

            globalData = GetServerRoads(globalData);

            globalData = GetActionsCost(globalData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ServerValuesPacket), globalData);
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

            globalData.isClientFactionMember = client.userFile.HasFaction;

            return globalData;
        }

        private static ServerGlobalData GetEventCosts(ServerGlobalData globalData)
        {
            globalData.eventValues = Master.eventValues;
            return globalData;
        }

        private static ServerGlobalData GetSiteData(ServerGlobalData globalData)
        {
            globalData.siteValues = Master.siteValues;
            return globalData;
        }

        private static ServerGlobalData GetServerDifficulty(ServerGlobalData globalData)
        {
            globalData.difficultyValues = Master.difficultyValues;
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
                file.fromFaction = site.isFromFaction;

                tempList.Add(file);
            }

            globalData.playerSites = tempList.ToArray();

            return globalData;
        }

        private static ServerGlobalData GetServerRoads(ServerGlobalData data)
        {
            data.roads = RoadManager.GetAllRoads();

            Logger.Warning(data.roads.Count().ToString());

            return data;
        }

        private static ServerGlobalData GetActionsCost(ServerGlobalData globalData)
        {
            globalData.actionValues = Master.actionValues;
            return globalData;
        }
    }
}
