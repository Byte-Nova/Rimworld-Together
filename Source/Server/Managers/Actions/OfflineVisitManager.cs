using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.JSON;

namespace RimworldTogether.GameServer.Managers.Actions
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
                client.clientListener.SendData(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitDetails.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitDetails.offlineVisitStepMode = ((int)OfflineVisitStepMode.Deny).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetails);
                    client.clientListener.SendData(packet);
                }

                else
                {
                    MapFileJSON mapDetails = MapManager.GetUserMapFromTile(offlineVisitDetails.targetTile);
                    offlineVisitDetails.mapDetails = ObjectConverter.ConvertObjectToBytes(mapDetails);

                    Packet packet = Packet.CreatePacketFromJSON("OfflineVisitPacket", offlineVisitDetails);
                    client.clientListener.SendData(packet);
                }
            }
        }
    }
}
