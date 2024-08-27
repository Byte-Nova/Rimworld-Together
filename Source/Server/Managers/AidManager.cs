using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class AidManager
    {
        private static readonly double baseAidTimer = 3600000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            AidData data = Serializer.ConvertBytesToObject<AidData>(packet.contents);

            switch (data.stepMode)
            {
                case AidStepMode.Send:
                    SendAidRequest(client, data);
                    break;

                case AidStepMode.Receive:
                    //Empty
                    break;

                case AidStepMode.Accept:
                    SendAidAccept(client, data);
                    break;

                case AidStepMode.Reject:
                    SendAidReject(client, data);
                    break;
            }
        }

        private static void SendAidRequest(ServerClient client, AidData data) 
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data.toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.toTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.owner))
                {
                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);

                    if (Master.serverConfig.TemporalAidProtection && !TimeConverter.CheckForEpochTimer(target.userFile.AidProtectionTime, baseAidTimer))
                    {
                        data.stepMode = AidStepMode.Reject;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        data.stepMode = AidStepMode.Receive;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                        target.listener.EnqueuePacket(packet);
                    }
                }

                else
                {
                    data.stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, AidData data)
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data.fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.fromTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.owner))
                {
                    client.userFile.UpdateAidTime();

                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    data.stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidReject(ServerClient client, AidData data) 
        {
            if (!SettlementManager.CheckIfTileIsInUse(data.fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data.fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.fromTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.owner))
                {
                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.owner);
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
