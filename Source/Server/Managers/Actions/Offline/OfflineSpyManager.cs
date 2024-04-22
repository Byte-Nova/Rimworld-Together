using Shared;

namespace GameServer
{
    public static class OfflineSpyManager
    {
        public static void ParseSpyPacket(ServerClient client, Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = (SpyDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(spyDetailsJSON.spyStepMode))
            {
                case (int)CommonEnumerators.SpyStepMode.Request:
                    SendRequestedMap(client, spyDetailsJSON);
                    break;

                case (int)CommonEnumerators.SpyStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, SpyDetailsJSON spyDetailsJSON)
        {
            if (!MapManager.CheckIfMapExists(spyDetailsJSON.targetTile))
            {
                spyDetailsJSON.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyDetailsJSON);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyDetailsJSON.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyDetailsJSON.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(spyDetailsJSON.targetTile);
                    spyDetailsJSON.mapDetails = Serializer.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SpyPacket), spyDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
