using Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class OfflineActivityManager
    {
        private static readonly double baseActivityTimer = 3600000;

        public static void ParseOfflineActivityPacket(ServerClient client, Packet packet)
        {
            OfflineActivityData data = (OfflineActivityData)Serializer.ConvertBytesToObject(packet.contents);

            switch (data.activityStepMode)
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
                data.activityStepMode = OfflineActivityStepMode.Unavailable;
                Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineActivityPacket), data);
                client.listener.EnqueuePacket(packet);
            }

            else
            {
                SettlementFile settlementFile = SettlementManager.GetSettlementFileFromTile(data.targetTile);

                if (UserManager.CheckIfUserIsConnected(settlementFile.owner))
                {
                    data.activityStepMode = OfflineActivityStepMode.Deny;
                    Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineActivityPacket), data);
                    client.listener.EnqueuePacket(packet);
                }

                else
                {
                    UserFile userFile = UserManager.GetUserFileFromName(settlementFile.owner);

                    if (Master.serverConfig.TemporalActivityProtection && !TimeConverter.CheckForEpochTimer(userFile.ActivityProtectionTime, baseActivityTimer))
                    {
                        data.activityStepMode = OfflineActivityStepMode.Deny;
                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineActivityPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }

                    else
                    {
                        userFile.UpdateActivityTime();

                        MapFileData mapData = MapManager.GetUserMapFromTile(data.targetTile);
                        data.mapData = Serializer.ConvertObjectToBytes(mapData);

                        Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.OfflineActivityPacket), data);
                        client.listener.EnqueuePacket(packet);
                    }
                }
            }
        }
    }
}
