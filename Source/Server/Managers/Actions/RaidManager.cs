using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class RaidManager
    {
        private enum RaidStepMode { Request, Deny }

        public static void ParseRaidPacket(Client client, Packet packet)
        {
            RaidDetailsJSON raidDetailsJSON = Serializer.SerializeFromString<RaidDetailsJSON>(packet.contents[0]);

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

        private static void SendRequestedMap(Client client, RaidDetailsJSON raidDetailsJSON)
        {
            if (!SaveManager.CheckIfMapExists(raidDetailsJSON.raidData))
            {
                raidDetailsJSON.raidStepMode = ((int)RaidStepMode.Deny).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(raidDetailsJSON) };
                Packet packet = new Packet("RaidPacket", contents);
                Network.Network.SendData(client, packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(raidDetailsJSON.raidData);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    raidDetailsJSON.raidStepMode = ((int)RaidStepMode.Deny).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(raidDetailsJSON) };
                    Packet packet = new Packet("RaidPacket", contents);
                    Network.Network.SendData(client, packet);
                }

                else
                {
                    MapFile mapFile = SaveManager.GetUserMapFromTile(raidDetailsJSON.raidData);
                    raidDetailsJSON.raidData = Serializer.SerializeToString(mapFile);

                    string[] contents = new string[] { Serializer.SerializeToString(raidDetailsJSON) };
                    Packet packet = new Packet("RaidPacket", contents);
                    Network.Network.SendData(client, packet);
                }
            }
        }
    }
}
