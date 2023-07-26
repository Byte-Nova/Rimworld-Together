namespace RimworldTogether.Shared.Network
{
    public class MainNetworkingUnit
    {
        public static NetworkingUnitClient client;
        public static NetworkingUnitServer server;

        // we make sure getting the client results in an exception if it's not initialized
        public static bool IsClient;
        public const int startPort = 15555;

        public static void Send<T>(int type, T data, int targetId = 0)
        {
            if (IsClient) client.Send(type, data);
            else server.Send(type, data, targetId);
        }
    }
}