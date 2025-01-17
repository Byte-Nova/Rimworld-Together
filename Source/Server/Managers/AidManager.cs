using GameServer.Core;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class AidManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionConfigs.EnableAids)
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
            if (!PlayerSettlementManager.CheckIfTileIsInUse(data._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Uid} attempted to send an aid packet to settlement at tile {data._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._toTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);

                    if (Master.serverConfig.TemporalAidProtection && !TimeConverter.CheckForEpochTimer(target.userFile.AidProtectionTime, Master.serverConfig.TemporalAidProtectionTime * 1000))
                    {
                        data._stepMode = AidStepMode.Reject;
                        Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        data._stepMode = AidStepMode.Receive;
                        Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                        target.listener.EnqueuePacket(packet);
                    }
                }

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, AidData data)
        {
            if (!PlayerSettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Uid} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    client.userFile.UpdateAidTime();

                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);
                    Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }

        private static void SendAidReject(ServerClient client, AidData data)
        {
            if (!PlayerSettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.userFile.Uid} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);
                    Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                    target.listener.EnqueuePacket(packet);
                }

                //Back to client sending the request

                else
                {
                    Packet packet = Packet.CreatePacketFromObject(nameof(AidManager), data);
                    client.listener.EnqueuePacket(packet);
                }
            }
        }
    }
}
