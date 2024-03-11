using Shared;

namespace GameServer
{
    public static class OfflineVisitManager
    {
        public static void ParseOfflineVisitPacket(ServerClient client, Packet packet)
        {
            OfflineVisitDetailsJSON offlineVisitDetails = (OfflineVisitDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitDetails.offlineVisitStepMode))
            {
                case (int)CommonEnumerators.OfflineVisitStepMode.Request:
                    SendRequestedMap(client, offlineVisitDetails);
                    break;

                case (int)CommonEnumerators.OfflineVisitStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineVisitDetailsJSON offlineVisitDetails)
        {
            if (!MapManager.CheckIfMapExists(offlineVisitDetails.targetTile))
            {
                offlineVisitDetails.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitDetails);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitDetails.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitDetails.offlineVisitStepMode = ((int)CommonEnumerators.OfflineVisitStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitDetails);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(offlineVisitDetails.targetTile);
                    offlineVisitDetails.mapDetails = Serializer.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitDetails);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
