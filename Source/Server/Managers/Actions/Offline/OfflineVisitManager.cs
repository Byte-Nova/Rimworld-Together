using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineVisitManager
    {
        public static void ParseOfflineVisitPacket(ServerClient client, Packet packet)
        {
            OfflineVisitData offlineVisitData = (OfflineVisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (offlineVisitData.offlineVisitStepMode)
            {
                case OfflineActivityStepMode.Request:
                    SendRequestedMap(client, offlineVisitData);
                    break;

                case OfflineActivityStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineVisitData offlineVisitData)
        {
            if (!MapManager.CheckIfMapExists(offlineVisitData.targetTile))
            {
                offlineVisitData.offlineVisitStepMode = OfflineActivityStepMode.Unavailable;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitData.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitData.offlineVisitStepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileData mapData = MapManager.GetUserMapFromTile(offlineVisitData.targetTile);
                    offlineVisitData.mapData = Serializer.ConvertObjectToBytes(mapData);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineVisitPacket), offlineVisitData);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
