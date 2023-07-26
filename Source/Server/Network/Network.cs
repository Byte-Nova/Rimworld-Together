using System.Net;
using System.Net.Sockets;
using RimworldTogether.GameServer.Core;
using RimworldTogether.GameServer.Managers;
using RimworldTogether.GameServer.Misc;
using RimworldTogether.Shared.Network;

namespace RimworldTogether.GameServer.Network
{
    public static class Network
    {
        public static List<Client> connectedClients = new List<Client>();
        private static TcpListener server;
        private static IPAddress localAddress = IPAddress.Parse(Program.serverConfig.IP);
        private static int port = int.Parse(Program.serverConfig.Port);

        public static bool isServerOpen;

        public static void ReadyServer()
        {
            server = new TcpListener(localAddress, port);
            server.Start();
            isServerOpen = true;
            MainNetworkingUnit.server = new();
            MainNetworkingUnit.server.Listen(localAddress.ToString(), port + 1);
            Threader.GenerateServerThread(Threader.ServerMode.Heartbeat);
            Threader.GenerateServerThread(Threader.ServerMode.Sites);

            Logger.WriteToConsole("Type 'help' to get a list of available commands");
            Logger.WriteToConsole($"Listening for users at {localAddress}:{port}");
            Logger.WriteToConsole("Server launched");
            Titler.ChangeTitle();

            while (true) ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            Client newServerClient = new Client(server.AcceptTcpClient());

            if (Program.isClosing) newServerClient.disconnectFlag = true;
            else
            {
                if (connectedClients.ToArray().Count() >= int.Parse(Program.serverConfig.MaxPlayers))
                {
                    UserManager_Joinings.SendLoginResponse(newServerClient, UserManager_Joinings.LoginResponse.ServerFull);
                    Logger.WriteToConsole($"[Warning] > Server Full", Logger.LogMode.Warning);
                }

                else
                {
                    connectedClients.Add(newServerClient);

                    Titler.ChangeTitle();

                    Threader.GenerateClientThread(Threader.ClientMode.Start, newServerClient);

                    Logger.WriteToConsole($"[Connect] > {newServerClient.username} | {newServerClient.SavedIP}");
                }
            }
        }

        public static void ListenToClient(Client client)
        {
            try
            {
                while (!client.disconnectFlag)
                {
                    string data = client.streamReader.ReadLine();
                    if (data == null) break;

                    Packet receivedPacket = Serializer.SerializeToPacket(data);
                    if (receivedPacket == null) break;

                    try
                    {
                        PacketHandler.HandlePacket(client, receivedPacket);
                    }
                    catch
                    {
                        ResponseShortcutManager.SendIllegalPacket(client, true);
                    }
                }
            }

            catch
            {
                client.disconnectFlag = true;

                return;
            }
        }

        public static void SendData(Client client, Packet packet)
        {
            while (client.isBusy) Thread.Sleep(100);

            try
            {
                client.isBusy = true;

                client.streamWriter.WriteLine(Serializer.SerializeToString(packet));
                client.streamWriter.Flush();

                client.isBusy = false;
            }
            catch
            {
            }
        }

        public static void KickClient(Client client)
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

                Client[] actualClients = connectedClients.ToArray();

                foreach (Client client in actualClients)
                {
                    try
                    {
                        if (client.disconnectFlag || !CheckIfConnected(client))
                        {
                            KickClient(client);
                        }
                    }

                    catch
                    {
                        KickClient(client);
                    }
                }
            }
        }

        private static bool CheckIfConnected(Client client)
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