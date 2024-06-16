﻿using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ServerGlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData = GetServerValues(globalData);

            globalData = GetClientValues(client, globalData);

            globalData = GetEventCosts(globalData);

            globalData = GetSiteData(globalData);

            globalData = GetServerDifficulty(globalData);

            globalData = GetServerSettlements(client, globalData);

            globalData = GetServerSites(client, globalData);

            globalData = GetActionsCost(globalData);

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.ServerValuesPacket), globalData);
            client.listener.EnqueuePacket(packet);
        }

        private static ServerGlobalData GetServerValues(ServerGlobalData globalData)
        {
            ServerValuesFile svf = Master.serverValues;
            globalData.AllowCustomScenarios = svf.AllowCustomScenarios;

            return globalData;
        }

        private static ServerGlobalData GetClientValues(ServerClient client, ServerGlobalData globalData)
        {
            globalData.isClientAdmin = client.isAdmin;

            globalData.isClientFactionMember = client.hasFaction;

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

                if (settlement.owner == client.username) continue;
                else
                {
                    file.tile = settlement.tile;
                    file.owner = settlement.owner;
                    file.goodwill = GoodwillManager.GetSettlementGoodwill(client, settlement);

                    tempList.Add(file);
                }
            }

            globalData.settlements = tempList.ToArray();

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

            globalData.sites = tempList.ToArray();

            return globalData;
        }

        private static ServerGlobalData GetActionsCost(ServerGlobalData globalData)
        {
            globalData.actionValues = Master.actionValues;
            return globalData;
        }
    }
}