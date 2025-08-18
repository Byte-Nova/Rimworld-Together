using Shared;
using TCPNetwork.Server;

namespace GameServer.Managers
{

    public static class RecountManager
    {
        [HandlesPacket(PacketHeader.RecountManager)]
        private static void ParsePacket(ServerClient client, byte[] bytes)
        {

        }
    }
}