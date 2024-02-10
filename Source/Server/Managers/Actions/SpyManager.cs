using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class SpyManager
    {
        public static void ParseSpyPacket(ServerClient client, Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = (SpyDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

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
                Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                client.clientListener.SendData(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyDetailsJSON.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyDetailsJSON.spyStepMode = ((int)CommonEnumerators.SpyStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    client.clientListener.SendData(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(spyDetailsJSON.targetTile);
                    spyDetailsJSON.mapDetails = ObjectConverter.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    client.clientListener.SendData(packet);
                }
            }
        }
    }
}
