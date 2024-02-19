using Shared;

namespace GameServer
{
    public static class OfflineVisitManager
    {
        private enum OfflineVisitStepMode { Request, Deny }

        public static void ParseOfflineVisitPacket(ServerClient client, Packet packet)
        {
            OfflineVisitDetailsJSON offlineVisitDetails = (OfflineVisitDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(offlineVisitDetails.offlineVisitStepMode))
            {
                case (int)OfflineVisitStepMode.Request:
                    SendRequestedMap(client, offlineVisitDetails);
                    break;

                case (int)OfflineVisitStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineVisitDetailsJSON offlineVisitDetails)
        {
            if (!MapManager.CheckIfMapExists(offlineVisitDetails.targetTile))
            {
                offlineVisitDetails.offlineVisitStepMode = ((int)OfflineVisitStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetails);
                client.listener.dataQueue.Enqueue(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitDetails.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitDetails.offlineVisitStepMode = ((int)OfflineVisitStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetails);
                    client.listener.dataQueue.Enqueue(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(offlineVisitDetails.targetTile);
                    offlineVisitDetails.mapDetails = ObjectConverter.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetails);
                    client.listener.dataQueue.Enqueue(packet);
                }
            }
        }
    }
}
