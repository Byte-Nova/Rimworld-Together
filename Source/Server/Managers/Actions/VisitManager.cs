using RimworldTogether.GameServer.Files;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON.Actions;
using RimworldTogether.Shared.Network;
using RimworldTogether.Shared.Serializers;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers.Actions
{
    public static class VisitManager
    {
        public static void ParseVisitPacket(ServerClient client, Packet packet)
        {
            VisitDetailsJSON visitDetailsJSON = (VisitDetailsJSON)ObjectConverter.ConvertBytesToObject(packet.contents);

            switch (int.Parse(visitDetailsJSON.visitStepMode))
            {
                case (int)CommonEnumerators.VisitStepMode.Request:
                    SendVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Accept:
                    AcceptVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Reject:
                    RejectVisitRequest(client, visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Action:
                    SendVisitActions(client, visitDetailsJSON);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Stop:
                    SendVisitStop(client, visitDetailsJSON);
                    break;
            }
        }

        private static void SendVisitRequest(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.targetTile);
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client);
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null)
                {
                    visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                    Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                    client.clientListener.SendData(packet);
                }

                else
                {
                    if (toGet.inVisitWith != null)
                    {
                        visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                        Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                        client.clientListener.SendData(packet);
                    }

                    else
                    {
                        visitDetailsJSON.visitorName = client.username;
                        Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                        toGet.clientListener.SendData(packet);
                    }
                }
            }
        }

        private static void AcceptVisitRequest(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    client.inVisitWith = toGet;
                    toGet.inVisitWith = client;

                    Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                    toGet.clientListener.SendData(packet);
                }
            }
        }

        private static void RejectVisitRequest(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitDetailsJSON.fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                    toGet.clientListener.SendData(packet);
                }
            }
        }

        private static void SendVisitActions(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            if (client.inVisitWith == null)
            {
                visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Stop).ToString();
                Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                client.clientListener.SendData(packet);
            }

            else
            {
                Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);
                client.inVisitWith.clientListener.SendData(packet);
            }
        }

        public static void SendVisitStop(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            Packet packet = Packet.CreatePacketFromJSON("VisitPacket", visitDetailsJSON);

            if (client.inVisitWith == null) client.clientListener.SendData(packet);
            else
            {
                client.clientListener.SendData(packet);
                client.inVisitWith.clientListener.SendData(packet);

                client.inVisitWith.inVisitWith = null;
                client.inVisitWith = null;
            }
        }
    }
}
