using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineActivityManager
    {
        private static readonly double baseActivityTimer = 3600000;

        public static void ParseOfflineActivityPacket(ServerClient client, Packet packet)
        {
            OfflineActivityData data = Serializer.ConvertBytesToObject<OfflineActivityData>(packet.contents);

            switch (data.stepMode)
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
            if (!MapManager.CheckIfMapExists(data.targetTile))
            {
                data.stepMode = OfflineActivityStepMode.Unavailable;
                Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OfflineActivityPacket), data);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.targetTile);

                if (UserManagerHelper.CheckIfUserIsConnected(settlementFile.owner))
                {
                    data.stepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OfflineActivityPacket), data);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    UserFile userFile = UserManagerHelper.GetUserFileFromName(settlementFile.owner);

                    if (Master.serverConfig.TemporalActivityProtection && !TimeConverter.CheckForEpochTimer(userFile.ActivityProtectionTime, baseActivityTimer))
                    {
                        data.stepMode = OfflineActivityStepMode.Deny;
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OfflineActivityPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        userFile.UpdateActivityTime();

                        data.mapData = MapManager.GetUserMapFromTile(data.targetTile);
                        Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.OfflineActivityPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
