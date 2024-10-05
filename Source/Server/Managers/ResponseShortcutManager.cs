using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, string message, bool shouldBroadcast = true)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data.stepMode = ResponseStepMode.IllegalAction;

            Packet packet = Packet.CreatePacketFromObject(nameof(ResponseShortcutManager), data);
            client.listener.EnqueuePacket(packet);
            client.listener.disconnectFlag = true;

            if (shouldBroadcast) 
            { 
                Logger.Warning($"[Illegal action] > {client.userFile.Username} > {client.userFile.SavedIP}");
                Logger.Warning($"[Illegal reason] > {message}");
            }
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data.stepMode = ResponseStepMode.UserUnavailable;
            
            Packet packet = Packet.CreatePacketFromObject(nameof(ResponseShortcutManager), data);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data.stepMode = ResponseStepMode.Pop;

            Packet packet = Packet.CreatePacketFromObject(nameof(ResponseShortcutManager), data);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendNoPowerPacket(ServerClient client, PlayerFactionData data)
        {
            data._stepMode = FactionStepMode.NoPower;

            Packet packet = Packet.CreatePacketFromObject(nameof(FactionManager), data);
            client.listener.EnqueuePacket(packet);
        }
    }
}
