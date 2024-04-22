﻿using Shared;

namespace GameServer
{
    public static class OnlineVisitManager
    {
        public static void ParseVisitPacket(ServerClient client, Packet packet)
        {
            VisitData visitData = (VisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (int.Parse(visitData.visitStepMode))
            {
                case (int)CommonEnumerators.VisitStepMode.Request:
                    SendVisitRequest(client, visitData);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Accept:
                    AcceptVisitRequest(client, visitData);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Reject:
                    RejectVisitRequest(client, visitData);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Action:
                    SendVisitActions(client, visitData);
                    break;

                case (int)CommonEnumerators.VisitStepMode.Stop:
                    SendVisitStop(client, visitData);
                    break;
            }
        }

        private static void SendVisitRequest(ServerClient client, VisitData visitData)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitData.targetTile);
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to visit a settlement at tile {visitData.targetTile}, but no settlement could be found");
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null)
                {
                    visitData.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    if (toGet.inVisitWith != null)
                    {
                        visitData.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Unavailable).ToString();
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        visitData.visitorName = client.username;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                        toGet.listener.EnqueuePacket(packet);
                    }
                }
            }
        }

        private static void AcceptVisitRequest(ServerClient client, VisitData visitData)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitData.fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    client.inVisitWith = toGet;
                    toGet.inVisitWith = client;

                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    toGet.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void RejectVisitRequest(ServerClient client, VisitData visitData)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitData.fromTile);
            if (settlementFile == null) return;
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null) return;
                else
                {
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    toGet.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendVisitActions(ServerClient client, VisitData visitData)
        {
            if (client.inVisitWith == null)
            {
                visitData.visitStepMode = ((int)CommonEnumerators.VisitStepMode.Stop).ToString();
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                client.inVisitWith.listener.EnqueuePacket(packet);
            }
        }

        public static void SendVisitStop(ServerClient client, VisitData visitData)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);

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
