using GameServer.Core;
using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{

    public static class AidManager
    {
        [HandlesPacket(PacketHeader.AidManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {
            if (!Master.ActionConfigs.EnableAids)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            AidData data = Serializer.ConvertBytesToObject<AidData>(bytes);

            Printer.Warning(data, LogImportanceMode.Extreme);

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
            if (!SettlementManager.CheckIfTileIsInUse(data._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Uid} attempted to send an aid packet to settlement at tile {data._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._toTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);

                    if (!ValueChecker.CheckIfCanAid(target.UserFile))
                    {
                        data._stepMode = AidStepMode.Reject;
                        client.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                    }

                    else
                    {
                        data._stepMode = AidStepMode.Receive;
                        target.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                    }
                }

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    client.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                }
            }
        }

        private static void SendAidAccept(ServerClient client, AidData data)
        {
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Uid} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    client.UserFile.UpdateAidTime();

                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);
                    target.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                }

                //Back to client sending the request

                else
                {
                    data._stepMode = AidStepMode.Reject;
                    client.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                }
            }
        }

        private static void SendAidReject(ServerClient client, AidData data)
        {
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Uid} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    ServerClient target = NetworkHelper.GetConnectedClientFromUid(settlementFile.UID);
                    target.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                }

                //Back to client sending the request

                else client.Listener.EnqueuePacket(PacketHeader.AidManager, data);
            }
        }
    }
}
