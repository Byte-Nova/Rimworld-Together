using Shared;

namespace GameServer
{
    public static class OfflineRaidManager
    {
        public static void ParseRaidPacket(ServerClient client, Packet packet)
        {
            RaidDetailsJSON raidDetailsJSON = (RaidDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(raidDetailsJSON.raidStepMode))
            {
                case (int)CommonEnumerators.RaidStepMode.Request:
                    SendRequestedMap(client, raidDetailsJSON);
                    break;

                case (int)CommonEnumerators.RaidStepMode.Deny:
                    //Do nothing
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, RaidDetailsJSON raidDetailsJSON)
        {
            if (!MapManager.CheckIfMapExists(raidDetailsJSON.targetTile))
            {
                raidDetailsJSON.raidStepMode = ((int)CommonEnumerators.RaidStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RaidPacket), raidDetailsJSON);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(raidDetailsJSON.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    raidDetailsJSON.raidStepMode = ((int)CommonEnumerators.RaidStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RaidPacket), raidDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(raidDetailsJSON.targetTile);
                    raidDetailsJSON.mapDetails = Serializer.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.RaidPacket), raidDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
