namespace RimworldTogether.Shared.Network
{
    public class MainNetworkingUnit
    {
        public static NetworkingUnitClient client = new NetworkingUnitClient();
        public static NetworkingUnitServer server = new NetworkingUnitServer();
        public static bool isClient;
        public const int startPort = 15555;
        public static void Send<T>(int type, T data, int targetId = 0)
        {
            if (isClient) client.Send(type, data);
            else server.Send(type, data, targetId);
        }
    }
}