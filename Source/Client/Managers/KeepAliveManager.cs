using System;
using GameClient.Hooks.TCPNetwork;
using GameClient.Misc;
using Shared;

namespace GameClient.Managers
{
    public static class KeepAliveManager
    {
        [HandlesPacket(PacketHeader.KeepAliveManager)]
        private static void ParsePacket(byte[] bytes)
        {
            ClientNetwork.Instance.ClientListener.LastKAPacket = DateTime.Now;
        }
    }
}