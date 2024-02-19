using Shared;

namespace GameServer
{
    public static class RaidManager
    {
        private enum RaidStepMode { Request, Deny }

        public static void ParseRaidPacket(ServerClient client, Packet packet)
        {
            RaidDetailsJSON raidDetailsJSON = (RaidDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(raidDetailsJSON.raidStepMode))
            {
                case (int)RaidStepMode.Request:
                    SendRequestedMap(client, raidDetailsJSON);
                    break;

                case (int)RaidStepMode.Deny:
                    //Do nothing
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, RaidDetailsJSON raidDetailsJSON)
        {
            if (!MapManager.CheckIfMapExists(raidDetailsJSON.targetTile))
            {
                raidDetailsJSON.raidStepMode = ((int)RaidStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON("RaidPacket", raidDetailsJSON);
                client.listener.dataQueue.Enqueue(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(raidDetailsJSON.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    raidDetailsJSON.raidStepMode = ((int)RaidStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("RaidPacket", raidDetailsJSON);
                    client.listener.dataQueue.Enqueue(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(raidDetailsJSON.targetTile);
                    raidDetailsJSON.mapDetails = ObjectConverter.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON("RaidPacket", raidDetailsJSON);
                    client.listener.dataQueue.Enqueue(packet);
                }
            }
        }
    }
}
