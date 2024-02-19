using Shared;

namespace GameServer
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, bool broadcast = true)
        {
            Packet packet = Packet.CreatePacketFromJSON("IllegalActionPacket");
            client.listener.dataQueue.Enqueue(packet);
            client.listener.disconnectFlag = true;

            if (broadcast) Logger.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", Logger.LogMode.Error);
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON("UserUnavailablePacket");
            client.listener.dataQueue.Enqueue(packet);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON("BreakPacket");
            client.listener.dataQueue.Enqueue(packet);
        }

        public static void SendNoPowerPacket(ServerClient client, FactionManifestJSON factionManifest)
        {
            factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NoPower).ToString();

            Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
            client.listener.dataQueue.Enqueue(packet);
        }
    }
}
