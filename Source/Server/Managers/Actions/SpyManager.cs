using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class SpyManager
    {
        private enum SpyStepMode { Request, Deny }

        public static void ParseSpyPacket(ServerClient client, Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = (SpyDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(spyDetailsJSON.spyStepMode))
            {
                case (int)SpyStepMode.Request:
                    SendRequestedMap(client, spyDetailsJSON);
                    break;

                case (int)SpyStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, SpyDetailsJSON spyDetailsJSON)
        {
            if (!SaveManager.CheckIfMapExists(spyDetailsJSON.targetTile))
            {
                spyDetailsJSON.spyStepMode = ((int)SpyStepMode.Deny).ToString();
                Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                client.clientListener.SendData(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyDetailsJSON.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyDetailsJSON.spyStepMode = ((int)SpyStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    client.clientListener.SendData(packet);
                }

                else
                {
                    MapDetailsJSON mapDetails = SaveManager.GetUserMapFromTile(spyDetailsJSON.targetTile);
                    spyDetailsJSON.mapDetails = mapDetails;

                    Packet packet = Packet.CreatePacketFromJSON("SpyPacket", spyDetailsJSON);
                    client.clientListener.SendData(packet);
                }
            }
        }
    }
}
