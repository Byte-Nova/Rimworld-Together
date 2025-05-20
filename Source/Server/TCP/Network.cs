using GameServer.Core;
using GameServer.Managers;
using GameServer.Misc;
using Shared;
using System.Net;
using System.Net.Sockets;
using static Shared.CommonEnumerators;

namespace GameServer.TCP
{
    public static class Network
    {
        private static IPAddress LocalAddress { get; set; } = IPAddress.Parse(Master.ServerConfig.IP);

        public static int Port { get; private set; } = int.Parse(Master.ServerConfig.Port);

        private static TcpListener? Connection { get; set; }

        public static List<ServerClient> ConnectedClients { get; private set; } = new List<ServerClient>();

        public static void ReadyServer()
        {
            if (Master.ServerConfig.UseUPnP) { _ = new UPnP(); }

            Connection = new TcpListener(LocalAddress, Port);
            Connection.Start();

            Printer.Warning("Server launched");
            Printer.Warning($"Listening for users at {LocalAddress}:{Port}");
            Printer.Warning("Type 'help' to get a list of available commands");

            Threader.GenerateServerThread(Threader.ServerMode.Sites);

            Main_.ChangeTitle();

            while (true) ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            TcpClient newTCP = Connection.AcceptTcpClient();
            ServerClient newServerClient = new ServerClient(newTCP);
            Listener newListener = new Listener(newServerClient, newTCP);
            newServerClient.Listener = newListener;

            if (Master.IsClosing)
            {
                newServerClient.Listener.DisconnectFlag = true;
            }

            else if (NetworkHelper.GetConnectedClientsSafe().Length >= int.Parse(Master.ServerConfig.MaxPlayers))
            {
                LoginManagerH.DenyConnectionWithReason(newServerClient, LoginResponse.ServerFull);
            }

            else if (Master.WorldValues == null && NetworkHelper.GetConnectedClientsSafe().Length > 0)
            {
                LoginManagerH.DenyConnectionWithReason(newServerClient, LoginResponse.NoWorld);
            }

            else
            {
                ConnectedClients.Add(newServerClient);

                Main_.ChangeTitle();

                InformationDisplayer.DisplayConnect(newServerClient);

                VersionManager.AskForClientVersion(newServerClient);
            }
        }

        public static void KickClient(ServerClient client)
        {
            try
            {
                ConnectedClients.Remove(client);
                client.Listener.DestroyConnection();

                Main_.ChangeTitle();
                UserManager.SendPlayerRecount();
                InformationDisplayer.DisplayDisconnect(client);
                if (Master.ChatConfig.DisconnectNotifications) ChatManager.BroadcastServerNotification($"{client.UserFile.Uid} has left the server!");
            }
            catch { Printer.Warning($"Error disconnecting user {client.UserFile.Uid}, this will cause memory overhead"); }
        }
    }

    public static class NetworkHelper
    {
        public static ServerClient[] GetConnectedClientsSafe(ServerClient toExclude = null)
        {
            if (toExclude != null) return Network.ConnectedClients.Where(fetch => fetch.UserFile.Uid != toExclude.UserFile.Uid).ToArray();
            else return Network.ConnectedClients.ToArray();
        }

        public static ServerClient GetConnectedClientFromUid(string uid)
        {
            return GetConnectedClientsSafe().FirstOrDefault(fetch => fetch.UserFile.Uid == uid);
        }

        public static void SendPacketToAllClients(PacketHeader header, object obj, ServerClient toExclude = null)
        {
            foreach (ServerClient client in GetConnectedClientsSafe(toExclude))
            {
                client.Listener.EnqueuePacket(header, obj);
            }
        }
    }
}