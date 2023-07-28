using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class VisitManager
    {
        public enum VisitStepMode { Request, Accept, Reject, Unavailable, Action, Stop }

        public static void ParseVisitPacket(Client client, Packet packet)
        {
            VisitDetailsJSON visitDetailsJSON = Serializer.SerializeFromString<VisitDetailsJSON>(packet.contents[0]);

            switch (int.Parse(visitDetailsJSON.visitStepMode))
            {
                case (int)VisitStepMode.Request:
                    SendVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Accept:
                    AcceptVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Reject:
                    RejectVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Action:
                    SendVisitActions(client, visitDetailsJSON);
                    break;

                case (int)VisitStepMode.Stop:
                    SendVisitStop(client, visitDetailsJSON);
                    break;
            }
        }

        private static void SendVisitRequest(Client client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.targetTile);
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                Client toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null)
                {
                    visitDetailsJSON.visitStepMode = ((int)VisitStepMode.Unavailable).ToString();
                    string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                    Packet packet = new Packet("VisitPacket", contents);
                    Network.Network.SendData(client, packet);
                }

                else
                {
                    if (toGet.inVisitWith != null)
                    {
                        visitDetailsJSON.visitStepMode = ((int)VisitStepMode.Unavailable).ToString();
                        string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                        Packet packet = new Packet("VisitPacket", contents);
                        Network.Network.SendData(client, packet);
                    }

                    else
                    {
                        visitDetailsJSON.visitorName = client.username;
                        string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                        Packet packet = new Packet("VisitPacket", contents);
                        Network.Network.SendData(toGet, packet);
                    }
                }
            }
        }

        private static void AcceptVisitRequest(Client client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.fromTile);
            if (settlementFile == null) return;
            else
            {
                Client toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    client.inVisitWith = toGet;
                    toGet.inVisitWith = client;

                    string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                    Packet packet = new Packet("VisitPacket", contents);
                    Network.Network.SendData(toGet, packet);
                }
            }
        }

        private static void RejectVisitRequest(Client client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.fromTile);
            if (settlementFile == null) return;
            else
            {
                Client toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                    Packet packet = new Packet("VisitPacket", contents);
                    Network.Network.SendData(toGet, packet);
                }
            }
        }

        private static void SendVisitActions(Client client, VisitDetailsJSON visitDetailsJSON)
        {
            if (client.inVisitWith == null)
            {
                visitDetailsJSON.visitStepMode = ((int)VisitStepMode.Stop).ToString();
                string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                Packet packet = new Packet("VisitPacket", contents);
                Network.Network.SendData(client, packet);
            }

            else
            {
                string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
                Packet packet = new Packet("VisitPacket", contents);
                Network.Network.SendData(client.inVisitWith, packet);
            }
        }

        public static void SendVisitStop(Client client, VisitDetailsJSON visitDetailsJSON)
        {
            string[] contents = new string[] { Serializer.SerializeToString(visitDetailsJSON) };
            Packet packet = new Packet("VisitPacket", contents);

            if (client.inVisitWith == null) Network.Network.SendData(client, packet);
            else
            {
                Network.Network.SendData(client, packet);
                Network.Network.SendData(client.inVisitWith, packet);

                client.inVisitWith.inVisitWith = null;
                client.inVisitWith = null;
            }
        }
    }
}
