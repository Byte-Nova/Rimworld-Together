using GameServer.Misc;
using GameServer.TCP;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.Managers
{
    [RTManager]
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
                Printer.Warning($"[Illegal action] > {client.userFile.Uid} > {client.userFile.SavedIP}");
                Printer.Warning($"[Illegal reason] > {message}");
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

        public static void SendNoPowerPacket(ServerClient client, PlayerGuildData data)
        {
            data._stepMode = GuildStepMode.NoPower;

            Packet packet = Packet.CreatePacketFromObject(nameof(GuildManager), data);
            client.listener.EnqueuePacket(packet);
        }
    }
}
