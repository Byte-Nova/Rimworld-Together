using GameServer.Core;
using Shared;
using Shared.Files;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Managers
{
    public static class GlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            ServerGlobalData globalData = new ServerGlobalData();

            globalData._isClientAdmin = client.UserFile.IsAdmin;
            globalData._isClientFactionMember = !string.IsNullOrEmpty(client.UserFile.GuildName);

            globalData._serverValues = new ServerValuesFile();
            globalData._serverValues.ServerName = Master.ServerConfig.Name;

            globalData._eventValues = EventManagerH.LoadedEvents;
            globalData._siteValues = Master.SiteValues;
            globalData._difficultyValues = Master.DifficultyValues;
            globalData._scenarioValues = Master.ScenarioValues;
            globalData._storytellerValues = Master.StorytellerValues;
            globalData._actionValues = Master.ActionConfigs;
            globalData._roadValues = Master.RoadValues;
            globalData._modConfigs = Master.ModConfig;

            if (Master.WorldValues != null)
            {
                globalData._roads = Master.WorldValues.Roads;
                globalData._pollutedTiles = Master.WorldValues.PollutedTiles;
                globalData._playerSettlements = GlobalDataManagerHelper.GetServerSettlements(client);
                globalData._npcSettlements = Master.WorldValues.NPCSettlements;
                globalData._playerSites = GlobalDataManagerHelper.GetServerSites(client);
            }

            client.Listener.EnqueuePacket(PacketHeader.GlobalDataManager, globalData);
        }
    }

    public static class GlobalDataManagerHelper
    {
        public static SettlementFile[] GetServerSettlements(ServerClient client)
        {
            List<SettlementFile> tempList = new List<SettlementFile>();
            foreach (SettlementFile settlement in SettlementManager.GetAllSettlements())
            {
                SettlementFile file = new SettlementFile();

                if (settlement.UID == client.UserFile.Uid) continue;
                else
                {
                    file.Tile = settlement.Tile;
                    file.UID = settlement.UID;
                    file.Label = settlement.Label;
                    file.Goodwill = GoodwillManager.GetSettlementGoodwill(client, settlement);

                    tempList.Add(file);
                }
            }

            return tempList.ToArray();
        }

        public static SiteFile[] GetServerSites(ServerClient client)
        {
            List<SiteFile> tempList = new List<SiteFile>();
            foreach (SiteFile site in SiteManagerHelper.GetAllSites())
            {
                SiteFile file = new SiteFile();

                file.Tile = site.Tile;
                file.UID = site.UID;
                file.Goodwill = GoodwillManager.GetSiteGoodwill(client, site);
                file.Type = site.Type;
                file.GuildName = site.GuildName;

                tempList.Add(file);
            }

            return tempList.ToArray();
        }
    }
}
