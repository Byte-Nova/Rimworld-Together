using GameServer.Core;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using TCPNetwork.Packets;
using TCPNetwork.Files.Client;

namespace GameServer.Managers
{
    public static class ActivityManager
    {
        [HandlesPacket(PacketHeader.ActivityManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            if (!Master.ActionConfigs.ActivityAction.IsEnabled)
            {
                ResponseShortcutManager.SendIllegalPacket(client, "Tried to use disabled feature!");
                return;
            }

            ActivityData data = Serializer.ConvertBytesToObject<ActivityData>(bytes);

            switch (data._stepMode)
            {
                case ActivityStepMode.Request:
                    SendRequestedMap(client, data);
                    break;
            }
        }

        private static void SendRequestedMap(ServerClient client, ActivityData data)
        {
            if (!MapManager.CheckIfMapExists(data._targetTile))
            {
                data._stepMode = ActivityStepMode.Deny;
                client.Listener.EnqueuePacket(PacketHeader.ActivityManager, data);
            }

            else
            {
                data._stepMode = ActivityStepMode.Request;
                data._mapRawData = MapManager.GetMapFromTile(data._targetTile);

                client.Listener.EnqueuePacket(PacketHeader.ActivityManager, data);
            }
        }
    }
}
