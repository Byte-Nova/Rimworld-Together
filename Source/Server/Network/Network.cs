using System.Net;
using System.Net.Sockets;
using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.GameServer.Network.Listener;
using Shared.Misc;

namespace RimworldTogether.GameServer.Network
{
    public static class Network
    {
        private static TcpListener server;
        private static IPAddress localAddress = IPAddress.Parse(Program.serverConfig.IP);
        private static int port = int.Parse(Program.serverConfig.Port);
        public static List<ServerClient> connectedClients = new List<ServerClient>();

        public static bool isServerOpen;

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, port);
            server.Start();
            isServerOpen = true;

            Threader.GenerateServerThread(Threader.ServerMode.Sites, Program.serverCancelationToken);

            Logger.WriteToConsole("Type 'help' to get a list of available commands");
            Logger.WriteToConsole($"Listening for users at {localAddress}:{port}");
            Logger.WriteToConsole("Server launched");
            Titler.ChangeTitle();

            while (true) ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            ServerClient newServerClient = new ServerClient(server.AcceptTcpClient());
            newServerClient.clientListener = new ClientListener(newServerClient);

            Threader.GenerateClientThread(newServerClient.clientListener, Threader.ClientMode.Listener, Program.serverCancelationToken);
            Threader.GenerateClientThread(newServerClient.clientListener, Threader.ClientMode.Health, Program.serverCancelationToken);
            Threader.GenerateClientThread(newServerClient.clientListener, Threader.ClientMode.KAFlag, Program.serverCancelationToken);

            if (Program.isClosing) newServerClient.disconnectFlag = true;
            else
            {
                if (connectedClients.ToArray().Count() >= int.Parse(Program.serverConfig.MaxPlayers))
                {
                    UserManager_Joinings.SendLoginResponse(newServerClient, CommonEnumerators.LoginResponse.ServerFull);
                    Logger.WriteToConsole($"[Warning] > Server Full", Logger.LogMode.Warning);
                }

                else
                {
                    connectedClients.Add(newServerClient);

                    Titler.ChangeTitle();

                    Logger.WriteToConsole($"[Connect] > {newServerClient.username} | {newServerClient.SavedIP}");
                }
            }
        }

        public static void KickClient(ServerClient client)
        {
            try
            {
                connectedClients.Remove(client);
                client.tcp.Dispose();

                UserManager.SendPlayerRecount();

                Titler.ChangeTitle();

                Logger.WriteToConsole($"[Disconnect] > {client.username} | {client.SavedIP}");
            }

            catch
            {
                Logger.WriteToConsole($"Error disconnecting user {client.username}, this will cause memory overhead", Logger.LogMode.Warning);
            }
        }
    }
}