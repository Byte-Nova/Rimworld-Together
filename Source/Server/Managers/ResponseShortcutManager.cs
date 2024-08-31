using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, string message, bool shouldBroadcast = true)
        {
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.IllegalActionPacket));
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
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.UserUnavailablePacket));
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.BreakPacket));
            client.listener.EnqueuePacket(packet);
        }

        public static void SendNoPowerPacket(ServerClient client, PlayerFactionData data)
        {
            data._stepMode = FactionStepMode.NoPower;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.FactionPacket), data);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorkerInsidePacket(ServerClient client)
        {
            SiteData siteData = new SiteData();
            siteData._stepMode = SiteStepMode.WorkerError;

            Packet packet = Packet.CreatePacketFromObject(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
