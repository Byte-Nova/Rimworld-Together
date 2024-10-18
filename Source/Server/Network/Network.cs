using Shared;
using System.Net;
using System.Net.Sockets;
using static Shared.CommonEnumerators;

namespace GameServer
{
    //Main class that is used to handle the connection with the clients

    public static class Network
    {
        //IP and Port that the connection will be bound to

        private static IPAddress localAddress = IPAddress.Parse(Master.serverConfig.IP);

        public static int port = int.Parse(Master.serverConfig.Port);

        //TCP listener that will handle the connection with the clients, and list of currently connected clients

        private static TcpListener connection;

        public static List<ServerClient> connectedClients = new List<ServerClient>();

        //Entry point function of the network class

        public static void ReadyServer()
        {
            if (Master.serverConfig.UseUPnP) { _ = new UPnP(); }

            connection = new TcpListener(localAddress, port);
            connection.Start();

            Logger.Warning("Server launched");
            Logger.Warning($"Listening for users at {localAddress}:{port}");            
            Logger.Warning("Type 'help' to get a list of available commands");

            Threader.GenerateServerThread(Threader.ServerMode.Sites);
            Threader.GenerateServerThread(Threader.ServerMode.Caravans);

            Main_.ChangeTitle();

            while (true) ListenForIncomingUsers();
        }

        //Listens for any user that might connect and executes all required tasks  with it
        
        private static void ListenForIncomingUsers()
        {
            TcpClient newTCP = connection.AcceptTcpClient();
            ServerClient newServerClient = new ServerClient(newTCP);
            Listener newListener = new Listener(newServerClient, newTCP);
            newServerClient.listener = newListener;

            Threader.GenerateClientThread(newServerClient.listener, Threader.ClientMode.Listener);
            Threader.GenerateClientThread(newServerClient.listener, Threader.ClientMode.Sender);
            Threader.GenerateClientThread(newServerClient.listener, Threader.ClientMode.Health);
            Threader.GenerateClientThread(newServerClient.listener, Threader.ClientMode.KAFlag);

            if (Master.isClosing) newServerClient.listener.disconnectFlag = true;
            else if (Master.worldValues == null && connectedClients.Count() > 0) LoginManager.SendLoginResponse(newServerClient, LoginResponse.NoWorld);
            else
            {
                if (connectedClients.ToArray().Count() >= int.Parse(Master.serverConfig.MaxPlayers))
                {
                    LoginManager.SendLoginResponse(newServerClient, LoginResponse.ServerFull);
                    Logger.Warning($"Server Full");
                }

                else
                {
                    connectedClients.Add(newServerClient);

                    Main_.ChangeTitle();

                    Logger.Message($"[Connect] > {newServerClient.userFile.Username} | {newServerClient.userFile.SavedIP}");
                }
            }
        }

        //Kicks specified client from the server

        public static void KickClient(ServerClient client)
        {
            try
            {
                connectedClients.Remove(client);
                client.listener.DestroyConnection();

                Main_.ChangeTitle();
                UserManager.SendPlayerRecount();
                Logger.Message($"[Disconnect] > {client.userFile.Username} | {client.userFile.SavedIP}");
                if (Master.chatConfig.DisconnectNotifications) ChatManager.BroadcastServerNotification($"{client.userFile.Username} has left the server!");
            }
            catch { Logger.Warning($"Error disconnecting user {client.userFile.Username}, this will cause memory overhead"); }
        }
    }

    public static class NetworkHelper
    {
        public static ServerClient[] GetConnectedClientsSafe(ServerClient toExclude = null)
        {
            if (toExclude != null) return Network.connectedClients.Where(fetch => fetch.userFile.Username != toExclude.userFile.Username).ToArray();
            else return Network.connectedClients.ToArray();
        }

        public static ServerClient GetConnectedClientFromUsername(string username)
        {
            return GetConnectedClientsSafe().FirstOrDefault(fetch => fetch.userFile.Username == username);
        }

        public static void SendPacketToAllClients(Packet packet, ServerClient toExclude = null)
        {
            foreach (ServerClient client in GetConnectedClientsSafe(toExclude))
            {
                client.listener.EnqueuePacket(packet);
            }
        }
    }
}