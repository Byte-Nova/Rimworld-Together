using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameServer.Managers
{
    public static class ResponseShortcutManager
    {
        public static void SendIllegalPacket(Client client, bool broadcast = true)
        {
            Packet Packet = new Packet("IllegalActionPacket");
            Network.SendData(client, Packet);
            client.disconnectFlag = true;

            if (broadcast) Logger.WriteToConsole($"[Illegal action] > {client.username} > {client.SavedIP}", Logger.LogMode.Error);
        }

        public static void SendUnavailablePacket(Client client)
        {
            Packet packet = new Packet("UserUnavailablePacket");
            Network.SendData(client, packet);
        }

        public static void SendBreakPacket(Client client)
        {
            Packet packet = new Packet("BreakPacket");
            Network.SendData(client, packet);
        }

        public static void SendNoPowerPacket(Client client, FactionManifestJSON factionManifest)
        {
            factionManifest.manifestMode = ((int)FactionManager.FactionManifestMode.NoPower).ToString();

            string[] contents = new string[] { Serializer.SerializeToString(factionManifest) };
            Packet packet = new Packet("FactionPacket", contents);
            Network.SendData(client, packet);
        }
    }
}
