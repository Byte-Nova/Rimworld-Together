using Shared;

namespace GameServer
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, bool broadcast = true)
        {
            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.IllegalActionPacket));
            client.listener.EnqueuePacket(packet);
            client.listener.disconnectFlag = true;

            if (broadcast) Logger.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", Logger.LogMode.Error);
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

        public static void SendNoPowerPacket(ServerClient client, FactionManifestJSON factionManifest)
        {
            factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NoPower).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.FactionPacket), factionManifest);
            client.listener.EnqueuePacket(packet);
        }

        public static void SendWorkerInsidePacket(ServerClient client)
        {
            SiteDetailsJSON siteDetails = new SiteDetailsJSON();
            siteDetails.siteStep = ((int)CommonEnumerators.SiteStepMode.WorkerError).ToString();

            Packet packet = Packet.CreatePacketFromJSON(nameof(PacketHandler.SitePacket), siteDetails);
            client.listener.EnqueuePacket(packet);
        }
    }
}
