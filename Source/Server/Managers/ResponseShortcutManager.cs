using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;
using TCPNetwork.Server;
using TCPNetwork.Packets;

namespace GameServer.Managers
{

    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, string message, bool shouldBroadcast = true)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data._stepMode = ResponseStepMode.IllegalAction;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcutManager, data);
            client.Listener.DisconnectFlag = true;

            if (shouldBroadcast)
            {
                Printer.Warning($"[Illegal action] > {client.UserFile.Uid} > {client.UserFile.SavedIP}");
                Printer.Warning($"[Illegal reason] > {message}");
            }
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data._stepMode = ResponseStepMode.UserUnavailable;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcutManager, data);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            ResponseShortcutData data = new ResponseShortcutData();
            data._stepMode = ResponseStepMode.Pop;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcutManager, data);
        }

        public static void SendNoPowerPacket(ServerClient client, PlayerGuildData data)
        {
            data._stepMode = GuildStepMode.NoPower;

            client.Listener.EnqueuePacket(PacketHeader.ResponseShortcutManager, data);
        }
    }
}
