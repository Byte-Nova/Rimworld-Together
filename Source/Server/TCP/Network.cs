using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;
using System.Linq;
using GameServer.Core;
using GameServer.Managers;
using GameServer.Misc;
using Shared;
using static Shared.CommonEnumerators;

namespace GameServer.TCP
{
    public static class Network
    {
        private static IPAddress    LocalAddress      = IPAddress.Parse(Master.ServerConfig.IP);
        public  static int          Port              = int.Parse(Master.ServerConfig.Port);
        private static TcpListener  Connection;
        public  static List<ServerClient> ConnectedClients = new List<ServerClient>();

        public static void ReadyServer()
        {
            if (Master.ServerConfig.UseUPnP)
                _ = new UPnP();

            Connection = new TcpListener(LocalAddress, Port);
            Connection.Start();

            Printer.Warning("Server launched");
            Printer.Warning($"Listening for users at {LocalAddress}:{Port}");
            Printer.Warning("Type 'help' to get a list of available commands");

            Threader.GenerateServerThread(Threader.ServerMode.Sites);
            Main_.ChangeTitle();

            while (true)
                ListenForIncomingUsers();
        }

        private static void ListenForIncomingUsers()
        {
            var tcp       = Connection.AcceptTcpClient();
            var newClient = new ServerClient(tcp);
            newClient.Listener = new Listener(newClient, tcp);

            if (Master.IsClosing)
            {
                newClient.Listener.DisconnectFlag = true;
                return;
            }

            // server full?
            if (NetworkHelper.GetConnectedClientsSafe().Length >= int.Parse(Master.ServerConfig.MaxPlayers))
            {
                LoginManagerH.DenyConnectionWithReason(newClient, LoginResponse.ServerFull);
                return;
            }

            // world not loaded?
            if (Master.WorldValues == null && NetworkHelper.GetConnectedClientsSafe().Length > 0)
            {
                LoginManagerH.DenyConnectionWithReason(newClient, LoginResponse.NoWorld);
                return;
            }

            // ACCEPT
            ConnectedClients.Add(newClient);
            Main_.ChangeTitle();

            // kick off version check
            VersionManager.AskForClientVersion(newClient);
        }

        public static void KickClient(ServerClient client)
        {
            try
            {
                ConnectedClients.Remove(client);
                client.Listener.DestroyConnection();
                Main_.ChangeTitle();

                // 1) In-game chat notification
                if (Master.ChatConfig.DisconnectNotifications)
                    ChatManager.BroadcastServerNotification($"{client.UserFile.Label} has left the server!");

                // 2) Console / Discord notification
                var leaveMsg = $"[Disconnect] {client.UserFile.Label} (UID: {client.UserFile.Uid}) has left the server!. " +
                               $"Players: {ConnectedClients.Count}";
                Printer.Message(leaveMsg);
                if (Master.DiscordConfig.Enabled)
                    _ = DiscordManager.SendConsoleMessageAsync(leaveMsg);
            }
            catch
            {
                Printer.Warning($"Error disconnecting user {client.UserFile.Uid}, this will cause memory overhead");
            }
        }
    }

    public static class NetworkHelper
    {
        public static ServerClient[] GetConnectedClientsSafe(ServerClient? toExclude = null) =>
            toExclude != null
                ? Network.ConnectedClients.Where(c => c.UserFile.Uid != toExclude.UserFile.Uid).ToArray()
                : Network.ConnectedClients.ToArray();

        public static ServerClient? GetConnectedClientFromUid(string uid) =>
            GetConnectedClientsSafe().FirstOrDefault(c => c.UserFile.Uid == uid);

        public static void SendPacketToAllClients(PacketHeader header, object obj, ServerClient? toExclude = null)
        {
            foreach (var client in GetConnectedClientsSafe(toExclude))
                client.Listener.EnqueuePacket(header, obj);
        }
    }
}