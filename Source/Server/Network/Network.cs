using Shared;
using System.Net;
using System.Net.Sockets;
using Mono.Nat;

namespace GameServer
{
    //Main class that is used to handle the connection with the clients

    public static class Network
    {
        //AutoPortForwardBool
        public static bool autoPortForwardSuccessful;

        //IP and Port that the connection will be bound to
        private static IPAddress localAddress = IPAddress.Parse(Master.serverConfig.IP);
        private static int port = int.Parse(Master.serverConfig.Port);

        //TCP listener that will handle the connection with the clients, and list of currently connected clients
        private static TcpListener connection;
        public static List<ServerClient> connectedClients = new List<ServerClient>();

        private static Punchthrough punchthrough;

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
            else if (Master.worldValues == null && connectedClients.Count() > 0) UserManager.SendLoginResponse(newServerClient, CommonEnumerators.LoginResponse.NoWorld);
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

        public static void TryToForwardPort()
        {
            autoPortForwardSuccessful = false;
            Logger.WriteToConsole($"Attempting to forward port {port}");
            Task.Run(delegate { punchthrough = new Punchthrough(); });

            //Wait for the server to try to portforward for 5 seconds or until it is successful
            Thread.Sleep(5000);
            if (!autoPortForwardSuccessful)
            {
                Logger.WriteToConsole("Could not Auto PortForward - \n " +
                    "              Possible causes:\n" +
                    "              - the port is being used\n" +
                    "              - the router has uPnP disabled\n" +
                    "              - or the router/modem does not have ports", Logger.LogMode.Warning);
                NatUtility.StopDiscovery();
            }
        }

        //class for auto portforwarding with UPnP
        public class Punchthrough
        {

            public Punchthrough()
            {
                Logger.WriteToConsole("Attempting to auto portforward");
                NatUtility.DeviceFound += DeviceFound;
                NatUtility.DeviceLost += DeviceLost;
                NatUtility.StartDiscovery();
            }

            private void DeviceFound(object sender, DeviceEventArgs args)
            {
                Logger.WriteToConsole("Device found");
                autoPortForwardSuccessful = true;
                INatDevice device = args.Device;
                device.CreatePortMap(new Mapping(Protocol.Tcp, port, port));
                Logger.WriteToConsole("portforward successful");
                // on device found code
            }

            private void DeviceLost(object sender, DeviceEventArgs args)
            {
                Logger.WriteToConsole("Device Lost");
                INatDevice device = args.Device;
                device.DeletePortMap(new Mapping(Protocol.Tcp, port, port));
                // on device disconnect code
            }
        }
    }

    
}