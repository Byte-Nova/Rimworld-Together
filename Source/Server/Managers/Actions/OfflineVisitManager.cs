using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class OfflineVisitManager
    {
        private enum OfflineVisitStepMode { Request, Deny }

        public static void ParseOfflineVisitPacket(Client client, Packet packet)
        {
            OfflineVisitDetailsJSON offlineVisitDetails = Serializer.SerializeFromString<OfflineVisitDetailsJSON>(packet.contents[0]);

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

        private static void SendRequestedMap(Client client, OfflineVisitDetailsJSON offlineVisitDetails)
        {
            if (!SaveManager.CheckIfMapExists(offlineVisitDetails.offlineVisitData))
            {
                offlineVisitDetails.offlineVisitStepMode = ((int)OfflineVisitStepMode.Deny).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(offlineVisitDetails) };
                Packet packet = new Packet("OfflineVisitPacket", contents);
                Network.SendData(client, packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(offlineVisitDetails.offlineVisitData);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    offlineVisitDetails.offlineVisitStepMode = ((int)OfflineVisitStepMode.Deny).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(offlineVisitDetails) };
                    Packet packet = new Packet("OfflineVisitPacket", contents);
                    Network.SendData(client, packet);
                }

                else
                {
                    MapFile mapFile = SaveManager.GetUserMapFromTile(offlineVisitDetails.offlineVisitData);
                    offlineVisitDetails.offlineVisitData = Serializer.SerializeToString(mapFile);

                    string[] contents = new string[] { Serializer.SerializeToString(offlineVisitDetails) };
                    Packet packet = new Packet("OfflineVisitPacket", contents);
                    Network.SendData(client, packet);
                }
            }
        }
    }
}
