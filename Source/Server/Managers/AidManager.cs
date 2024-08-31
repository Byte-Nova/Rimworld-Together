using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class AidManager
    {
        private static readonly double baseAidTimer = 3600000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableAids)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            AidData data = Serializer.ConvertBytesToObject<AidData>(packet.contents);

            switch (data._stepMode)
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
            if (!SettlementManager.CheckIfTileIsInUse(data._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._toTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.Owner))
                {
                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.Owner);

                    if (Master.serverConfig.TemporalAidProtection && !TimeConverter.CheckForEpochTimer(target.userFile.AidProtectionTime, baseAidTimer))
                    {
                        data._stepMode = AidStepMode.Reject;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        data._stepMode = AidStepMode.Receive;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                        target.listener.EnqueuePacket(packet);
                    }
                }

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, AidData data)
        {
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.Owner))
                {
                    client.userFile.UpdateAidTime();

                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.Owner);
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.AidPacket), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidReject(ServerClient client, AidData data) 
        {
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Username} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.Owner))
                {
                    ServerClient target = UserManagerHelper.GetConnectedClientFromUsername(settlementFile.Owner);
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
