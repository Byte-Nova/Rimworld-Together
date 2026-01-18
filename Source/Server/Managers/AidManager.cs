using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using Shared.Files;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;
using GameServer.Hooks.TCPNetwork;

namespace GameServer.Managers
{

    public static class AidManager
    {
        [HandlesPacket(PacketHeader.AidManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.AidAction.IsEnabled)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            AidData data = Serializer.ConvertBytesToObject<AidData>(bytes);

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
            if (!SettlementManager.CheckIfTileIsInUse(data._toTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Username} attempted to send an aid packet to settlement at tile {data._toTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._toTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    ServerClient target = ServerNetwork.Instance.GetConnectedClientFromUsername(settlementFile.Username);
                    
                    if (!PlayerCooldown.CheckIfCanAid(target.UserFile, Master.ActionConfigs.AidAction.IsEnabled, Master.ActionConfigs.AidAction.Cooldown))
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
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Username} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    client.UserFile.Cooldowns.SetAidTimer(TimeConverter.GetCurrentTimeToEpoch(), client.UserFile);

                    ServerClient target = ServerNetwork.Instance.GetConnectedClientFromUsername(settlementFile.Username);
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
            if (!SettlementManager.CheckIfTileIsInUse(data._fromTile)) ResponseShortcutManager.SendIllegalPacket(client, $"Player {client.UserFile.Username} attempted to send an aid packet to settlement at tile {data._fromTile}, but it has no settlement");
            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data._fromTile);
                if (UserManagerH.CheckIfUserIsConnected(settlementFile.Username))
                {
                    ServerClient target = ServerNetwork.Instance.GetConnectedClientFromUsername(settlementFile.Username);
                    target.Listener.EnqueuePacket(PacketHeader.AidManager, data);
                }

                //Back to client sending the request

                else client.Listener.EnqueuePacket(PacketHeader.AidManager, data);
            }
        }
    }
}
