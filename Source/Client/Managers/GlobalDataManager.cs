using GameClient.Core.Preferences;
using GameClient.TCP;
using GameClient.Values;
using Shared;

namespace GameClient.Managers
{
    [RTManager]
    public static class GlobalDataManager
    {
        public static void ParsePacket(Packet packet)
        {
            ServerGlobalData serverGlobalData = Serializer.ConvertBytesToObject<ServerGlobalData>(packet.contents);

            ServerValues.SetValues(serverGlobalData);
            SessionValues.SetValues(serverGlobalData);
            EventManagerHelper.SetValues(serverGlobalData);
            GameParameterManager.SetValues(serverGlobalData);
            PlayerSettlementManagerHelper.SetValues(serverGlobalData);
            NPCSettlementManagerHelper.SetValues(serverGlobalData);
            SiteManagerHelper.SetValues(serverGlobalData);
            CaravanManagerHelper.SetValues(serverGlobalData);
            RoadManagerHelper.SetValues(serverGlobalData);
            PollutionManagerHelper.SetValues(serverGlobalData);
            RecentServersManager.AddServerToList(serverGlobalData._serverValues.ServerName, $"{Network.ip}:{Network.port}");
        }
    }
}