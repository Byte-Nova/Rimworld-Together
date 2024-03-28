using Shared;

namespace GameServer
{
    public static class OnlineVisitManager
    {
        public static void ParseVisitPacket(ServerClient client, Packet packet)
        {
            VisitDetailsJSON visitDetailsJSON = (VisitDetailsJSON)Serializer.ConvertBytesToObject(packet.contents);

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
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to visit a settlement at tile {visitDetailsJSON.targetTile}, but no settlement could be found");
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null)
                {
                    visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    if (toGet.inVisitWith != null)
                    {
                        visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        visitDetailsJSON.visitorName = client.username;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                        toGet.listener.EnqueuePacket(packet);
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

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                    toGet.listener.EnqueuePacket(packet);
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
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                    toGet.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendVisitActions(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            if (client.inVisitWith == null)
            {
                visitDetailsJSON.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Stop).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);
                client.inVisitWith.listener.EnqueuePacket(packet);
            }
        }

        public static void SendVisitStop(ServerClient client, VisitDetailsJSON visitDetailsJSON)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitDetailsJSON);

            if (client.inVisitWith == null) client.listener.EnqueuePacket(packet);
            else
            {
                client.listener.EnqueuePacket(packet);
                client.inVisitWith.listener.EnqueuePacket(packet);

                client.inVisitWith.inVisitWith = null;
                client.inVisitWith = null;
            }
        }
    }
}
