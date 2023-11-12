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
        public static bool usingNewNetworking;

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, port);
            server.Start();
            isServerOpen = true;

            Threader.GenerateServerThread(Threader.ServerMode.Heartbeat, Program.serverCancelationToken);
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

                    newServerClient.clientListener = new ClientListener(newServerClient);

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

        public static void HearbeatClients()
        {
            while (true)
            {
                Thread.Sleep(100);

                ServerClient[] actualClients = connectedClients.ToArray();

                foreach (ServerClient client in actualClients)
                {
                    try
                    {
                        if (client.disconnectFlag || !CheckIfConnected(client))
                        {
                            KickClient(client);
                        }
                    }
                    catch { KickClient(client); }
                }
            }
        }

        private static bool CheckIfConnected(ServerClient client)
        {
            if (!client.tcp.Connected) return false;
            else
            {
                if (client.tcp.Client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buff = new byte[1];
                    if (client.tcp.Client.Receive(buff, SocketFlags.Peek) == 0) return false;
                    else return true;
                }

                else return true;
            }
        }
    }
}