using GameServer.Core;
using GameServer.Managers;
using GameServer.Misc;
using TCPNetwork;
using TCPNetwork.Server;
using Shared;
using System.Net;
using System.Net.Sockets;
using static Shared.CommonEnumerators;

namespace GameServer
{
    public class ServerNetwork : Network
    {
        public static ServerNetwork Instance { get; private set; } = null;

        public override Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = delegate (PacketHeader header, byte[] buffer, ServerClient client)
        {
            MethodGatherer.ServerMethodDictionary[header].Invoke(null, new object[] { client, buffer });
        };

        public override Action<ServerClient> OnDisconnect { get; set; } = delegate (ServerClient client) { Instance.Disconnect(client); };

        public override Action<object, LogImportanceMode> OnMessage { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Message(obj, mode);
        };

        public override Action<object, LogImportanceMode> OnWarning { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Warning(obj, mode);
        };

        public override Action<object, LogImportanceMode> OnError { get; set; } = delegate (object obj, LogImportanceMode mode)
        {
            Printer.Error(obj, mode);
        };

        public ServerNetwork()
        {
            Instance = this;
            Ip = Master.ServerConfig.IP;
            Port = Master.ServerConfig.Port;

            Task.Run(Setup);
        }

        public void Setup()
        {
            if (Master.ServerConfig.UseUPnP) { _ = new UPnP(); }

            try
            {
                ServerListener = new TcpListener(IPAddress.Parse(Ip), int.Parse(Port));
                ServerListener.Start();
            }

            catch (SocketException e)
            {
                Printer.Error(
                    $"Failed to start server on {Ip}:{Port}, try setting the address to your local ip address or '0.0.0.0' on port 25555, {e}");
            }

            catch (Exception e)
            {
                Printer.Error(e);
            }

            Printer.Warning("Server launched");
            Printer.Warning($"Listening for users at {Ip}:{Port}");
            Printer.Warning("Type 'help' to get a list of available commands");

            Threader.GenerateServerThread(Threader.ServerMode.Sites);

            Main_.ChangeTitle();

            while (true) TryConnect();
        }

        private void TryConnect()
        {
            TcpClient newTCP = ServerListener.AcceptTcpClient();
            ServerClient newServerClient = new ServerClient(newTCP, Master.UsersPath);

            Listener newListener = new Listener(newServerClient, newTCP, OnReadPacket, OnDisconnect,
                OnMessage, OnWarning, OnError, Listener.ListenerMode.Server);

            newServerClient.Listener = newListener;

            if (Master.IsClosing)
            {
                newServerClient.Listener.DisconnectFlag = true;
            }

            else if (ServerNetwork.Instance.GetConnectedClientsSafe().Length >= int.Parse(Master.ServerConfig.MaxPlayers))
            {
                LoginManagerH.DenyConnectionWithReason(newServerClient, LoginResponse.ServerFull);
            }

            else if (Master.WorldValues == null && ServerNetwork.Instance.GetConnectedClientsSafe().Length > 0)
            {
                LoginManagerH.DenyConnectionWithReason(newServerClient, LoginResponse.NoWorld);
            }

            else
            {
                ServerClients.Add(newServerClient);

                Main_.ChangeTitle();

                InformationDisplayer.DisplayConnect(newServerClient);

                VersionManager.AskForClientVersion(newServerClient);
            }
        }

        public void Disconnect(ServerClient client)
        {
            try
            {
                ServerClients.Remove(client);
                client.Listener.DestroyConnection();

                Main_.ChangeTitle();
                UserManager.SendPlayerRecount();
                InformationDisplayer.DisplayDisconnect(client);
                if (Master.ChatConfig.DisconnectNotifications) ChatManager.BroadcastServerNotification($"{client.UserFile.Uid} has left the server!");
            }
            catch { Printer.Warning($"Error disconnecting user {client.UserFile.Uid}, this will cause memory overhead"); }
        }

        public ServerClient[] GetConnectedClientsSafe(ServerClient toExclude = null)
        {
            if (toExclude != null) return ServerNetwork.Instance.ServerClients.Where(fetch => fetch.UserFile.Uid != toExclude.UserFile.Uid).ToArray();
            else return ServerNetwork.Instance.ServerClients.ToArray();
        }

        public ServerClient GetConnectedClientFromUid(string uid)
        {
            return GetConnectedClientsSafe().FirstOrDefault(fetch => fetch.UserFile.Uid == uid);
        }

        public void SendPacketToAllClients(PacketHeader header, object obj, ServerClient toExclude = null)
        {
            foreach (ServerClient client in GetConnectedClientsSafe(toExclude))
            {
                client.Listener.EnqueuePacket(header, obj);
            }
        }
    }
}