using System;

namespace RimworldTogether.Shared.Network
{
    public class MainNetworkingUnit
    {
        public static int clientId;

        public static void Send<T>(int type, T data, int targetId = 0)
        {
            throw new NotImplementedException();
        }
    }
}