using GameServer.Core;
using GameServer.Integrations.Discord;
using GameServer.Managers;
using GameServer.Misc;
using Shared;
using Shared.Misc;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using TCPNetwork;
using TCPNetwork.Files.Client;
using TCPNetwork.Misc;
using TCPNetwork.Packets;
using static Shared.CommonEnumerators;

namespace GameServer.Hooks.TCPNetwork
{
    public class ServerNetwork : Network
    {
        public static ServerNetwork Instance { get; private set; } = null;

        public override Action<PacketHeader, byte[], ServerClient> OnReadPacket { get; set; } = delegate (PacketHeader header, byte[] buffer, ServerClient client)
        {
            PacketCache.ServerMethodDictionary[header](client, buffer, header);
        };

        public override Action<bool> OnWritePacket { get; set; } = delegate (bool mode) { };

        public override Action<ServerClient> OnConnect { get; set; } = delegate (ServerClient client) { };

        public override Action<ServerClient> OnDisconnect { get; set; } = delegate (ServerClient client)
        {
            try
            {
                Instance.ServerClients.Remove(client);

                Main_.ChangeTitle();
                UserManager.SendPlayerRecount();
                InformationDisplayer.DisplayDisconnect(client);

                if (Master.ChatConfig.DisconnectNotifications && client?.UserFile != null)
                    ChatManager.BroadcastServerNotification($"{client.UserFile.Username} has left the server!");

                try
                {
                    string user = client?.UserFile?.Username;
                    if (!string.IsNullOrWhiteSpace(user))
                        DiscordPlayerAnnouncer.AnnounceLeft(user);
                }
                catch { }
            }
            catch
            {
                try
                {
                    string u = client?.UserFile?.Username;
                    if (string.IsNullOrWhiteSpace(u)) u = "(unknown)";
                    Printer.Warning($"Error disconnecting user {u}, this will cause memory overhead");
                }
                catch { }
            }
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

            try { DiscordBridge.TryStart(); } catch { }

            Main_.ChangeTitle();

            while (true) ListenForNewClients();
        }

        private void ListenForNewClients()
        {
            TcpClient newTCP = ServerListener.AcceptTcpClient();

            ServerClient client = new ServerClient(newTCP);
            client.Listener = new Listener(client, newTCP, OnReadPacket, OnWritePacket, OnConnect, OnDisconnect, Listener.ListenerMode.Server);

            if (ServerNetwork.Instance.GetConnectedClientsSafe().Length >= int.Parse(Master.ServerConfig.MaxPlayers))
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.Full);
            }
            else if (Master.WorldValues == null && ServerNetwork.Instance.GetConnectedClientsSafe().Length > 0)
            {
                LoginManagerH.DenyConnectionWithReason(client, LoginResponse.NoWorld);
            }
            else
            {
                ServerNetwork.Instance.ServerClients.Add(client);

                Main_.ChangeTitle();

                InformationDisplayer.DisplayConnect(client);

                VersionManager.AskForClientVersion(client);
            }
        }

        public ServerClient[] GetConnectedClientsSafe(ServerClient toExclude = null)
        {
            if (toExclude != null)
                return ServerNetwork.Instance.ServerClients.Where(fetch => fetch.UserFile.Username != toExclude.UserFile.Username).ToArray();
            else
                return ServerNetwork.Instance.ServerClients.ToArray();
        }

        public ServerClient GetConnectedClientFromUsername(string username)
        {
            return GetConnectedClientsSafe().FirstOrDefault(fetch => fetch.UserFile.Username == username);
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