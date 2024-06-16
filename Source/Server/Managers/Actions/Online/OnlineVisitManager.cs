using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OnlineVisitManager
    {
        public static void ParseVisitPacket(ServerClient client, Packet packet)
        {
            OnlineVisitData visitData = (OnlineVisitData)Serializer.ConvertBytesToObject(packet.contents);

            switch (visitData.visitStepMode)
            {
                case OnlineVisitStepMode.Request:
                    SendVisitRequest(client, visitData);
                    break;

                case OnlineVisitStepMode.Accept:
                    AcceptVisitRequest(client, visitData);
                    break;

                case OnlineVisitStepMode.Reject:
                    RejectVisitRequest(client, visitData);
                    break;

                case OnlineVisitStepMode.Action:
                    SendVisitActions(client, visitData);
                    break;

                case OnlineVisitStepMode.Create:
                    SendVisitActions(client, visitData);
                    break;

                case OnlineVisitStepMode.Destroy:
                    SendVisitActions(client, visitData);
                    break;

                case OnlineVisitStepMode.Stop:
                    SendVisitStop(client);
                    break;
            }
        }

        private static void SendVisitRequest(ServerClient client, OnlineVisitData visitData)
        {
            SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(visitData.targetTile);
            if (settlementFile == null) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.username} tried to visit a settlement at tile {visitData.targetTile}, but no settlement could be found");
            else
            {
                ServerClient toGet = UserManager.GetConnectedClientFromUsername(settlementFile.owner);
                if (toGet == null)
                {
                    visitData.visitStepMode = OnlineVisitStepMode.Unavailable;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    if (toGet.inVisitWith != null)
                    {
                        visitData.visitStepMode = OnlineVisitStepMode.Unavailable;
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

        private static void AcceptVisitRequest(ServerClient client, OnlineVisitData visitData)
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

        private static void RejectVisitRequest(ServerClient client, OnlineVisitData visitData)
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

        private static void SendVisitActions(ServerClient client, OnlineVisitData visitData)
        {
            if (client.inVisitWith == null)
            {
                visitData.visitStepMode = OnlineVisitStepMode.Stop;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.VisitPacket), visitData);
                client.inVisitWith.listener.EnqueuePacket(packet);
            }
        }

        public static void SendVisitStop(ServerClient client)
        {
            OnlineVisitData visitData = new OnlineVisitData();
            visitData.visitStepMode = OnlineVisitStepMode.Stop;
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
