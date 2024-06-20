using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineSpyManager
    {
        public static void ParseSpyPacket(ServerClient client, Packet packet)
        {
            OfflineSpyData spyData = (OfflineSpyData)Serializer.ConvertBytesToObject(packet.contents);

            switch (spyData.spyStepMode)
            {
                case OfflineActivityStepMode.Request:
                    SendRequestedMap(client, spyData);
                    break;

                case OfflineActivityStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineSpyData spyData)
        {
            if (!MapManager.CheckIfMapExists(spyData.targetTile))
            {
                spyData.spyStepMode = OfflineActivityStepMode.Unavailable;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyData.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyData.spyStepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileData mapData = MapManager.GetUserMapFromTile(spyData.targetTile);
                    spyData.mapData = Serializer.ConvertObjectToBytes(mapData);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyData);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
