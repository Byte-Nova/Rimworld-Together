using Shared;

namespace GameServer
{
    public static class OfflineSpyManager
    {
        public static void ParseSpyPacket(ServerClient client, Packet packet)
        {
            SpyData spyData = (SpyData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(spyData.spyStepMode))
            {
                case (int)CommonEnumerators.SpyStepMode.Request:
                    SendRequestedMap(client, spyData);
                    break;

                case (int)CommonEnumerators.SpyStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, SpyData spyData)
        {
            if (!MapManager.CheckIfMapExists(spyData.targetTile))
            {
                spyData.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyData.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyData.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Deny).ToString();
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
