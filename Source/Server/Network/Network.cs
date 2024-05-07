using Shared;
using System.Net;
using System.Net.Sockets;
using static Shared.CommonEnumerators;
using Mono.Nat;

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

            Threader.GenerateServerThread(Threader.ServerMode.Sites);
            connection = new TcpListener(localAddress, port);
            connection.Start();

            Threader.GenerateServerThread(Threader.ServerMode.Sites);

            Logger.WriteToConsole("Type 'help' to get a list of available commands", LogMode.Warning);
            Logger.WriteToConsole($"Listening for users at {localAddress}:{port}", LogMode.Warning);
            Logger.WriteToConsole("Server launched", LogMode.Warning);
            Master.ChangeTitle();

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
            else if (Master.worldValues == null && connectedClients.Count() > 0) UserManager.SendLoginResponse(newServerClient, CommonEnumerators.LoginResponse.NoWorld);
            else
            {
                if (connectedClients.ToArray().Count() >= int.Parse(Master.serverConfig.MaxPlayers))
                {
                    UserManager.SendLoginResponse(newServerClient, CommonEnumerators.LoginResponse.ServerFull);
                    Logger.WriteToConsole($"[Warning] > Server Full", LogMode.Warning);
                }

                else
                {
                    connectedClients.Add(newServerClient);

                    Master.ChangeTitle();

                    Logger.WriteToConsole($"[Connect] > {newServerClient.username} | {newServerClient.SavedIP}");
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

                Master.ChangeTitle();
                UserManager.SendPlayerRecount();
                Logger.WriteToConsole($"[Disconnect] > {client.username} | {client.SavedIP}");
            }

            catch
            {
                Logger.WriteToConsole($"Error disconnecting user {client.username}, this will cause memory overhead", LogMode.Warning);
            }
        }
    }
}