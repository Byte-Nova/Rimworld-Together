using GameServer.Misc;
using Shared;
using TCPNetwork.Files.Client;

namespace GameServer.Managers
{
    public static class KeepAliveManager
    {
        [HandlesPacket(PacketHeader.KeepAliveManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes, PacketHeader header)
        {
            client.Listener.LastKAPacket = DateTime.Now;
        }
    }
}