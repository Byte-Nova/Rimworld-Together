using Shared;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, string message, bool shouldBroadcast = true)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.IllegalActionPacket));
            client.listener.EnqueuePacket(packet);
            client.listener.disconnectFlag = true;

            if (shouldBroadcast) 
            { 
                ConsoleManager.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", LogMode.Warning);
                ConsoleManager.WriteToConsole($"[Illegal reason] > {message}", LogMode.Warning);
            }
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.UserUnavailablePacket));
            client.listener.EnqueuePacket(packet);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.BreakPacket));
            client.listener.EnqueuePacket(packet);
        }

        public static void SendNoPowerPacket(ServerClient client, PlayerFactionData factionManifest)
        {
            factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NoPower).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorkerInsidePacket(ServerClient client)
        {
            SiteData siteData = new SiteData();
            siteData.siteStep = ((int)CommonEnumerators.SiteStepMode.WorkerError).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteData);
            client.listener.EnqueuePacket(packet);
        }
    }
}
