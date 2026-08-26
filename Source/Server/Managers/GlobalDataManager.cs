using RTServer.Core;
using RTServer.PacketManagers;
using RTNetwork.Packets;
using RTNetwork.Components;
using RTShared.Misc;
using RTShared.Files.Player;

namespace RTServer.Managers
{
    public class GlobalDataManager
    {
        public static void SendServerGlobalData(ServerClient client)
        {
            PKT_ServerGlobalData globalData = new PKT_ServerGlobalData();
            globalData.IsAdmin = client.GetData<FL_Player>().IsAdmin;
            globalData.IsGuildMember = GuildManagerH.GetGuildFromName(client.GetData<FL_Player>().GuildName) != null;
            globalData.ActionValues = Master.ActionConfigs;
            globalData.RoadValues = Master.ActionConfigs.RoadAction.RoadValues;
            globalData.WorldObjects = PM_WorldObject.GetAllWorldObjects();
            globalData.PlayerSettlements = PM_Settlements.GetAllSettlements().Where(fetch => fetch.Username != client.GetData<FL_Player>().Username).ToList();
            globalData.PlayerSites = PM_Sites.GetAllSites();
            globalData.ScenarioValues = Master.ScenarioValues;
            globalData.DifficultyValues = Master.DifficultyValues;
            globalData.StorytellerValues = Master.StorytellerValues;
            globalData.ModConfigs = Master.ModConfig;
            globalData.EventValues = PM_Events.LoadedEvents;
            globalData.Roads = PM_Roads.GetAllRoads();
            globalData.Rivers = PM_Rivers.GetAllRivers();
            
            if (Master.WorldValues != null) globalData.PollutedTiles = Master.WorldValues.PollutedTiles;

            client.Listener.EnqueuePacket(PacketHeader.GlobalData, globalData);
        }
    }
}
