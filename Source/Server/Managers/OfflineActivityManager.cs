using GameServer.Core;
using GameServer.Files;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
    public static class OfflineActivityManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionConfigs.EnableOfflineActivities)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            OfflineActivityData data = Serializer.ConvertBytesToObject<OfflineActivityData>(packet.contents);

            switch (data._stepMode)
            {
                case OfflineActivityStepMode.Request:
                    SendRequestedMap(client, data);
                    break;

                case OfflineActivityStepMode.Deny:
                    //Nothing goes here
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, OfflineActivityData data)
        {
            if (!MapManager.CheckIfMapExists(data._targetTile))
            {
                data._stepMode = OfflineActivityStepMode.Unavailable;
                Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = PlayerSettlementManager.GetSettlementFileFromTile(data._targetTile);

                if (UserManagerH.CheckIfUserIsConnected(settlementFile.UID))
                {
                    data._stepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    UserFile userFile = UserManagerH.GetUserFileFromName(settlementFile.UID);

                    if (Master.serverConfig.TemporalActivityProtection && !TimeConverter.CheckForEpochTimer(userFile.ActivityProtectionTime, Master.serverConfig.TemporalActivityProtectionTime * 1000))
                    {
                        data._stepMode = OfflineActivityStepMode.Deny;
                        Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        userFile.UpdateActivityTime();

                        data._mapFile = MapManager.GetUserMapFromTile(data._targetTile);
                        Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                        client.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
