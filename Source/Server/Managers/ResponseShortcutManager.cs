using RimworldTogether.GameServer.Managers.Actions;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network;
using RimworldTogether.Shared.JSON;
using RimworldTogether.Shared.Misc;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Managers
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(Client client, bool broadcast = true)
        {
            Packet Packet = new Packet("IllegalActionPacket");
            Network.Network.SendData(client, Packet);
            client.disconnectFlag = true;

            if (broadcast) Logger.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", Logger.LogMode.Error);
        }

        public static void SendUnavailablePacket(Client client)
        {
            Packet packet = new Packet("UserUnavailablePacket");
            Network.Network.SendData(client, packet);
        }

        public static void SendBreakPacket(Client client)
        {
            Packet packet = new Packet("BreakPacket");
            Network.Network.SendData(client, packet);
        }

        public static void SendNoPowerPacket(Client client, FactionManifestJSON factionManifest)
        {
            factionManifest.manifestMode = ((int)FactionManager.FactionManifestMode.NoPower).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
            Packet packet = new Packet("FactionPacket", contents);
            Network.Network.SendData(client, packet);
        }
    }
}
