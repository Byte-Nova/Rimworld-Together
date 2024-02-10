using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Network;
using Shared.Misc;

namespace RimworldTogether.GameServer.Managers
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(ServerClient client, bool broadcast = true)
        {
            Packet packet = Packet.CreatePacketFromJSON("IllegalActionPacket");
            client.clientListener.SendData(packet);
            client.disconnectFlag = true;

            if (broadcast) Logger.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", Logger.LogMode.Error);
        }

        public static void SendUnavailablePacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON("UserUnavailablePacket");
            client.clientListener.SendData(packet);
        }

        public static void SendBreakPacket(ServerClient client)
        {
            Packet packet = Packet.CreatePacketFromJSON("BreakPacket");
            client.clientListener.SendData(packet);
        }

        public static void SendNoPowerPacket(ServerClient client, FactionManifestJSON factionManifest)
        {
            factionManifest.manifestMode = ((int)CommonEnumerators.FactionManifestMode.NoPower).ToString();

            Packet packet = Packet.CreatePacketFromJSON("FactionPacket", factionManifest);
            client.clientListener.SendData(packet);
        }
    }
}
