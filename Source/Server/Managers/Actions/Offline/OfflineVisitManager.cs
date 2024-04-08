using Shared;

namespace GameServer
{
    public static class OfflineVisitManager
    {
        public static void ParseOfflineVisitPacket(ServerClient client, Packet packet)
        {
            OfflineVisitData offlineVisitData = (OfflineVisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitData.offlineVisitStepMode))
            {
                case (int)CommonEnumerators.OfflineVisitStepMode.Request:
                    SendRequestedMap(client, offlineVisitData);
                    break;

                case (int)CommonEnumerators.OfflineVisitStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineVisitData offlineVisitData)
        {
            if (!MapManager.CheckIfMapExists(offlineVisitData.targetTile))
            {
                offlineVisitData.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitData.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitData.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileData mapDetails = MapManager.GetUserMapFromTile(offlineVisitData.targetTile);
                    offlineVisitData.mapData = Serializer.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
