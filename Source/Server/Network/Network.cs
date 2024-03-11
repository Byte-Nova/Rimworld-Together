using Shared;
using System.Net;
using System.Net.Sockets;

namespace GameServer
{
    //Main class that is used to handle the connection with the clients

    public static class Network
    {
        //IP and Port that the connection will be bound to
        private static IPAddress localAddress = IPAddress.Parse(Master.serverConfig.IP);
        private static int port = int.Parse(Master.serverConfig.Port);

        //TCP listener that will handle the connection with the clients, and list of currently connected clients
        private static TcpListener connection;
        public static List<ServerClient> connectedClients = new List<ServerClient>();

        //Entry point function of the network class

        public static void ReadyServer()
        {
            connection = new TcpListener(localAddress, port);
            connection.Start();

            Threader.GenerateServerThread(Threader.ServerMode.Sites);

            Logger.WriteToConsole("Type 'help' to get a list of available commands", Logger.LogMode.Warning);
            Logger.WriteToConsole($"Listening for users at {localAddress}:{port}", Logger.LogMode.Warning);
            Logger.WriteToConsole("Server launched", Logger.LogMode.Warning);
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
            else if (Master.worldValues == null && connectedClients.Count() > 0) newServerClient.listener.disconnectFlag = true;
            else
            {
                if (connectedClients.ToArray().Count() >= int.Parse(Master.serverConfig.MaxPlayers))
                {
                    UserManager.SendLoginResponse(newServerClient, CommonEnumerators.LoginResponse.ServerFull);
                    Logger.WriteToConsole($"[Warning] > Server Full", Logger.LogMode.Warning);
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

                UserManager.SendPlayerRecount();

                Master.ChangeTitle();

                Logger.WriteToConsole($"[Disconnect] > {client.username} | {client.SavedIP}");
            }

            catch
            {
                Logger.WriteToConsole($"Error disconnecting user {client.username}, this will cause memory overhead", Logger.LogMode.Warning);
            }
        }
    }
}