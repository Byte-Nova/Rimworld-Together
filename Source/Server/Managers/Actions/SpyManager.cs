using GameServer.Managers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer
{
    public static class SpyManager
    {
        private enum SpyStepMode { Request, Deny }

        public static void ParseSpyPacket(Client client, Packet packet)
        {
            SpyDetailsJSON spyDetailsJSON = Serializer.SerializeFromString<SpyDetailsJSON>(packet.contents[0]);

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

        private static void SendRequestedMap(Client client, SpyDetailsJSON spyDetailsJSON)
        {
            if (!SaveManager.CheckIfMapExists(spyDetailsJSON.spyData))
            {
                spyDetailsJSON.spyStepMode = ((int)SpyStepMode.Deny).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(spyDetailsJSON) };
                Packet packet = new Packet("SpyPacket", contents);
                Network.SendData(client, packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(spyDetailsJSON.spyData);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    spyDetailsJSON.spyStepMode = ((int)SpyStepMode.Deny).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(spyDetailsJSON) };
                    Packet packet = new Packet("SpyPacket", contents);
                    Network.SendData(client, packet);
                }

                else
                {
                    MapFile mapFile = SaveManager.GetUserMapFromTile(spyDetailsJSON.spyData);
                    spyDetailsJSON.spyData = Serializer.SerializeToString(mapFile);

                    string[] contents = new string[] { Serializer.SerializeToString(spyDetailsJSON) };
                    Packet packet = new Packet("SpyPacket", contents);
                    Network.SendData(client, packet);
                }
            }
        }
    }
}
