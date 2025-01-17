using GameServer.TCP;
using Shared;

namespace GameServer.Managers
{
    [RTManager]
    public static class KeepAliveManager
    {
        public static void ParsePacket(ServerClient client, Packet packet)
        {
            client.listener.KAFlag = true;
        }
    }
}