using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineActivityManager
    {
        private static readonly double baseActivityTimer = 3600000;

        public static void ParsePacket(ServerClient client, Packet packet)
        {
            if (!Master.actionValues.EnableOfflineActivities)
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

                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.Owner))
                {
                    data._stepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.Owner);

                    if (Master.serverConfig.TemporalActivityProtection && !TimeConverter.CheckForEpochTimer(userFile.ActivityProtectionTime, baseActivityTimer))
                    {
                        data._stepMode = OfflineActivityStepMode.Deny;
                        Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        userFile.UpdateActivityTime();

                        data._mapFile = MapManager.GetUserMapFromTile(userFile.Username, data._targetTile);
                        Packet packet = Packet.CreatePacketFromObject(nameof(OfflineActivityManager), data);
                        client.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
