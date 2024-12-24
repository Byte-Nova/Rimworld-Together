using Shared;

namespace GameClient
{
    public static class GlobalDataManager
    {
        public static void ParsePacket(Packet packet)
        {
            ServerGlobalData serverGlobalData = Serializer.ConvertBytesToObject<ServerGlobalData>(packet.contents);
            
            ServerValues.SetValues(serverGlobalData);
            SessionValues.SetValues(serverGlobalData);
            EventManagerHelper.SetValues(serverGlobalData);
            DifficultyManager.SetValues(serverGlobalData);
            PlayerSettlementManagerHelper.SetValues(serverGlobalData);
            NPCSettlementManagerHelper.SetValues(serverGlobalData);
            SiteManagerHelper.SetValues(serverGlobalData);
            CaravanManagerHelper.SetValues(serverGlobalData);
            RoadManagerHelper.SetValues(serverGlobalData);
            PollutionManagerHelper.SetValues(serverGlobalData);
        }
    }
}